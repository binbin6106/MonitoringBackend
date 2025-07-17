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
    public class DeviceCollectorTask
    {
        public long DeviceId { get; }
        private CancellationTokenSource _cts;
        private Task _task;
        private bool online { get; set; }
        private readonly ILogger _logger;
        private Device device1 { get; }
        private readonly IHubContext<DeviceDataHub> _hubContext;
        public bool IsRunning => !_task.IsCompleted;
        private ModbusTcpHelper modbustcp;
        private readonly AlarmService _alarmService;

        public DeviceCollectorTask(Device device, ILogger logger, IHubContext<DeviceDataHub> hubContext, AlarmService alarmService)
        {
            device1 = device;
            online = false;
            _cts = new CancellationTokenSource();
            _alarmService = alarmService;
            _logger = logger;
            _hubContext = hubContext;
            _task = Task.Run(() => Run(_cts.Token));
        }

        private async Task Run(CancellationToken token)
        {
            _logger.LogInformation($"[{device1.id}] 开始采集...");
            ModbusTcpHelper modbustcp = new ModbusTcpHelper(device1);
            
            while (!token.IsCancellationRequested)
            {
                _logger.LogInformation($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{device1.id}] 正在采集...");
                (List<SensorData> data, online) = modbustcp.getData();

                var result = new
                {
                    device1.id,
                    device1.ip,
                    device1.location,
                    device1.name,
                    device1.image,
                    online,
                    data = data
                };

                var json = JsonSerializer.Serialize(result);
                Debug.WriteLine(json);

                await _hubContext.Clients.All.SendAsync("ReceiveDeviceData", json);

                foreach (var item in data)
                {
                    await _alarmService.ProcessDataAsync(item.SensorId, "XVibration", (float)item.XVibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "YVibration", item.YVibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "ZVibration", item.ZVibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "Vibration", item.Vibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "Temperature", item.Temperature);
                }





                await Task.Delay(2000, token); // 每秒采集
            }
            _logger.LogInformation($"[{device1.id}] 已停止采集。");
        }

        public void Stop()
        {
                _cts.Cancel();    
        }
    }

}
