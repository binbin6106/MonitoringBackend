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

        public DeviceCollectorTask(Device device, ILogger logger, IHubContext<DeviceDataHub> hubContext)
        {
            device1 = device;
            online = false;
            _cts = new CancellationTokenSource();
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
                _logger.LogInformation($"[{device1.id}] 正在采集...");
                (List<SensorData> data, online) = modbustcp.getData();
                
                //List<SensorData> data = new List<SensorData>
                //{
                //    new SensorData
                //    {
                //        SensorId = 1,
                //        Temperature = Math.Round(Random.Shared.Next(20, 30) + Random.Shared.NextDouble(),2),
                //        XVibration = Math.Round(Random.Shared.NextDouble(),2),
                //        YVibration = Math.Round(Random.Shared.NextDouble(),2),
                //        ZVibration = Math.Round(Random.Shared.NextDouble(),2),
                //        Vibration = Math.Round(Random.Shared.NextDouble(),2),
                //        Timestamp = DateTime.Now
                //    },
                //    new SensorData
                //    {
                //        SensorId = 2,
                //        Temperature = Math.Round(Random.Shared.Next(20, 30) + Random.Shared.NextDouble(),2),
                //        XVibration = Math.Round(Random.Shared.NextDouble(),2),
                //        YVibration = Math.Round(Random.Shared.NextDouble(),2),
                //        ZVibration = Math.Round(Random.Shared.NextDouble(),2),
                //        Vibration = Math.Round(Random.Shared.NextDouble(),2),
                //        Timestamp = DateTime.Now
                //    }
                //};
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
                //var result = new
                //{
                //    device1.id,
                //    device1.ip,
                //    device1.location,
                //    data = data
                //};
                var json = JsonSerializer.Serialize(result);
                Debug.WriteLine(json);

                await _hubContext.Clients.All.SendAsync("ReceiveDeviceData", json);
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
