using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Migrations;
using MonitoringBackend.Helpers;
using MonitoringBackend.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Reactive;
using System.Text.Json;

namespace MonitoringBackend.Service
{
    public class GatewayCollectorTask
    {
        public long GatewayId { get; }
        private CancellationTokenSource _cts;
        private Task _task;
        private bool online { get; set; }
        private readonly ILogger _logger;
        private Gateway gateway1 { get; }
        private readonly IHubContext<GatewayHub> _hubContext;
        public bool IsRunning => !_task.IsCompleted;
        private ModbusTcpHelper modbustcp;
        private readonly InfluxDbHelper _influxDbHelper;

        public GatewayCollectorTask(Gateway gateway, ILogger logger, IHubContext<GatewayHub> hubContext, InfluxDbHelper influxDbHelper)
        {
            gateway1 = gateway;
            gateway1.sensors = gateway.sensors.OrderBy(s => s.id).ToList();
            online = false;
            _cts = new CancellationTokenSource();
            _logger = logger;
            _hubContext = hubContext;
            _influxDbHelper = influxDbHelper;
            _task = Task.Run(() => Run(_cts.Token));
        }

        private async Task Run(CancellationToken token)
        {
            _logger.LogInformation($"[{gateway1.id}] 开始采集...");
            ModbusTcpHelper modbustcp = new ModbusTcpHelper(gateway1);
            
            while (!token.IsCancellationRequested)
            {
                try
                {
                    (List<SensorData> data, online, Dictionary<string, AlarmRecord> alarmRecord) = modbustcp.getData();

                    var result = new
                    {
                        gateway1.id,
                        gateway1.name,
                        gateway1.ip,
                        online,
                        data = data
                    };

                    var json = JsonSerializer.Serialize(result);
                    var alarm_json = JsonSerializer.Serialize(alarmRecord.Values.ToList());
                    
                    // 并行发送数据和写入InfluxDB以提高性能
                    var tasks = new[]
                    {
                        _hubContext.Clients.All.SendAsync("ReceiveGatewayData", json, token),
                        _hubContext.Clients.All.SendAsync("Alarms", alarm_json, token),
                        _influxDbHelper.WriteSensorBatchAsync(data)
                    };
                    
                    await Task.WhenAll(tasks);
                }
                catch (Exception ex) when (!token.IsCancellationRequested)
                {
                    _logger.LogError(ex, $"[{gateway1.id}] 数据采集出错: {ex.Message}");
                    online = false;
                }

                await Task.Delay(1000, token); // 每秒采集
            }
            _logger.LogInformation($"[{gateway1.id}] 已停止采集。");
        }

        public void Stop()
        {
            _cts.Cancel();    
        }
    }
}
