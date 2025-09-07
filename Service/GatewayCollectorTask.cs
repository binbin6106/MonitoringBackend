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
        //private List<AlarmRecord> alarmRecords = new List<AlarmRecord>();
        public bool IsRunning => !_task.IsCompleted;
        private ModbusTcpHelper modbustcp;
        private readonly InfluxDbHelper _influxDbHelper;
        // 当前传感器的报警状态（高、低、正常）
        private enum AlarmState
        {
            Normal,
            HighAlarm,
            LowAlarm
        }

        public enum AlarmType
        {
            None,
            High,
            Low
        }

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
                await _hubContext.Clients.All.SendAsync("ReceiveGatewayData", json);
                await _hubContext.Clients.All.SendAsync("Alarms", alarm_json);
                await _influxDbHelper.WriteSensorBatchAsync(data);
                //foreach (var item in data)
                //{
                //    //await _alarmService.ProcessDataAsync(item.SensorId, "XVibration", (float)item.XVibration);
                //    //await _alarmService.ProcessDataAsync(item.SensorId, "YVibration", item.YVibration);
                //    //await _alarmService.ProcessDataAsync(item.SensorId, "ZVibration", item.ZVibration);
                //    //await _alarmService.ProcessDataAsync(item.SensorId, "Vibration", item.Vibration);
                //    //await _alarmService.ProcessDataAsync(item.SensorId, "Temperature", item.Temperature);
                //}
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
