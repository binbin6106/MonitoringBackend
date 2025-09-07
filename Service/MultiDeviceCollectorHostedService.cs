namespace MonitoringBackend.Service
{
    //这个类用来实现启动自动采集，前端使用Controller控制启动通知，本类也是通过调用MultiDeviceCollectorService控制启动
    public class MultiDeviceCollectorHostedService : BackgroundService
    {
        private readonly ILogger<MultiDeviceCollectorHostedService> _logger;
        private readonly MultiDeviceCollectorService _collector;

        public MultiDeviceCollectorHostedService(
            ILogger<MultiDeviceCollectorHostedService> logger,
            MultiDeviceCollectorService collector)
        {
            _logger = logger;
            _collector = collector;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Thread.Sleep(5000);
            _logger.LogInformation("后台设备采集服务启动");
            
            // 启动时调用一次
            await _collector.StartAllDevicesAsync();
        }
    }

}
