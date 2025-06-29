using Microsoft.AspNetCore.SignalR;
using MonitoringBackend.Helpers;
using MonitoringBackend.Models;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MonitoringBackend;

public class TcpListenerService : BackgroundService
{
    private readonly InfluxService _influxService;
    private readonly IHubContext<DeviceDataHub> _hubContext;
    //private ModbusTcpHelper modbustcp = new ModbusTcpHelper();

    public TcpListenerService(
        InfluxService influxService,
        IHubContext<DeviceDataHub> hubContext)
    {
        _influxService = influxService;
        _hubContext = hubContext;
    }



    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Random rand = new();

        while (!stoppingToken.IsCancellationRequested)
        {

            //List<SensorData> data = modbustcp.getData();
            // 模拟生成 3 个传感器的数据
            //List<SensorData> sensors = Enumerable.Range(1, 10).Select(i => new SensorData
            //{
            //    SensorId = i,
            //    Temperature = data[0] / 10.0,
            //    XVibration = data[1] / 10.0, 
            //    YVibration = data[2] / 10.0,
            //    ZVibration = data[3] / 10.0,
            //    Vibration = data[4] / 10.0,
            //    Timestamp = DateTime.Now
            //}).ToList();
            //List<SensorData> sensors = 
            //var json = JsonSerializer.Serialize(sensors);
            //// 推送给所有前端客户端
            //await _hubContext.Clients.All.SendAsync("ReceiveSensorData", json);
            //_ = Task.Run(() => HandleClientAsync(data));
            await Task.Delay(2000); // 每 0.2 秒采样一次
        }
    }
    private async Task HandleClientAsync(List<SensorData> sensorList)
    {
        try
        {
            //foreach (var sensor in sensorList)
            //{
            //    sensor.Timestamp = DateTime.UtcNow;
            //}

            // 批量写入 InfluxDB
            await _influxService.WriteSensorBatchAsync(sensorList);
            var json = JsonSerializer.Serialize(sensorList);
            // 推送所有数据
            await _hubContext.Clients.All.SendAsync("ReceiveSensorDataBatch", json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"解析或推送数据失败: {ex.Message}");
        }
    }
}
