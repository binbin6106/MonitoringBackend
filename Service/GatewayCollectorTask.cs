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
        // 当前传感器的报警状态（高、低、正常）
        private enum AlarmState
        {
            Normal,
            HighAlarm,
            LowAlarm
        }

        // 记录每个传感器的状态
        private readonly Dictionary<int, AlarmState> _sensorAlarmStates = new();

        public enum AlarmType
        {
            None,
            High,
            Low
        }

        public GatewayCollectorTask(Gateway gateway, ILogger logger, IHubContext<GatewayHub> hubContext)
        {
            gateway1 = gateway;
            string[] low_threshold_str = gateway.sensors[i].low_threshold.Split(',');
            string[] up_threshold_str = gateway.sensors[i].up_threshold.Split(',');
            online = false;
            _cts = new CancellationTokenSource();
            _logger = logger;
            _hubContext = hubContext;
            _task = Task.Run(() => Run(_cts.Token));
        }

        private async Task Run(CancellationToken token)
        {
            _logger.LogInformation($"[{gateway1.id}] 开始采集...");
            ModbusTcpHelper modbustcp = new ModbusTcpHelper(gateway1);
            
            while (!token.IsCancellationRequested)
            {
                (List<SensorData> data, online) = modbustcp.getData();


                var result = new
                {
                    gateway1.id,
                    gateway1.name,
                    gateway1.ip,
                    online,
                    data = data
                };

                var json = JsonSerializer.Serialize(result);

                await _hubContext.Clients.All.SendAsync("ReceiveGatewayData", json);
                await _hubContext.Clients.All.SendAsync("Alarms", json);
                foreach (var item in data)
                {
                    //await _alarmService.ProcessDataAsync(item.SensorId, "XVibration", (float)item.XVibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "YVibration", item.YVibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "ZVibration", item.ZVibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "Vibration", item.Vibration);
                    //await _alarmService.ProcessDataAsync(item.SensorId, "Temperature", item.Temperature);
                }
                
                await Task.Delay(2000, token); // 每秒采集
            }
            _logger.LogInformation($"[{gateway1.id}] 已停止采集。");
        }

        public void Stop()
        {
                _cts.Cancel();    
        }
        // 检查是否需要报警（仅在状态发生变化时返回 true）
        private (bool needAlarm, AlarmType type) CheckAndUpdateAlarm(int sensorId, float value, float lowThreshold, float highThreshold)
        {
            var previousState = _sensorAlarmStates.TryGetValue(sensorId, out var prev) ? prev : AlarmState.Normal;
            AlarmState currentState;

            if (value > highThreshold)
            {
                currentState = AlarmState.HighAlarm;
            }
            else if (value < lowThreshold)
            {
                currentState = AlarmState.LowAlarm;
            }
            else
            {
                currentState = AlarmState.Normal;
            }

            // 状态未变 → 不报警
            if (previousState == currentState)
            {
                return (false, AlarmType.None);
            }

            // 状态变了 → 更新状态并报警
            _sensorAlarmStates[sensorId] = currentState;

            return currentState switch
            {
                AlarmState.HighAlarm => (true, AlarmType.High),
                AlarmState.LowAlarm => (true, AlarmType.Low),
                _ => (false, AlarmType.None) // 状态恢复正常可以触发“恢复”事件
            };
        }

    }

}
