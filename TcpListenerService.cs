using Microsoft.AspNetCore.SignalR;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MonitoringBackend;

public class TcpListenerService : BackgroundService
{
    private readonly InfluxService _influxService;
    private readonly IHubContext<SensorDataHub> _hubContext;

    public TcpListenerService(
        InfluxService influxService,
        IHubContext<SensorDataHub> hubContext)
    {
        _influxService = influxService;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Random rand = new();

        while (!stoppingToken.IsCancellationRequested)
        {
            // 模拟生成 3 个传感器的数据
            List<SensorData> sensors = Enumerable.Range(1, 70).Select(i => new SensorData
            {
                SensorId = i,
                Temperature = rand.NextDouble() * 100,
                Vibration = rand.NextDouble(),
                Timestamp = DateTime.Now
            }).ToList();


            //var json = JsonSerializer.Serialize(sensors);
            //// 推送给所有前端客户端
            //await _hubContext.Clients.All.SendAsync("ReceiveSensorData", json);
            _ = Task.Run(() => HandleClientAsync(sensors));
            await Task.Delay(1000); // 每 0.2 秒采样一次
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
