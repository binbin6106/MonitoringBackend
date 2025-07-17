using Microsoft.AspNetCore.SignalR;
using MonitoringBackend.Helpers;
using MonitoringBackend.Models;
using System.Diagnostics;
using System.Text.Json;

namespace MonitoringBackend.Service
{
    public class MultiDeviceCollectorService
    {
        private readonly Dictionary<long, DeviceCollectorTask> _devices = new();
        private readonly ILogger<MultiDeviceCollectorService> _logger;
        private readonly IHubContext<DeviceDataHub> _hubContext;
        private readonly object _lock = new();
        private readonly AlarmService _alarmService;
        //private List<Device> devices = new List<Device>();

        public MultiDeviceCollectorService(ILogger<MultiDeviceCollectorService> logger, IHubContext<DeviceDataHub> hubContext, AlarmService alarmService)
        {
            _logger = logger;
            _hubContext = hubContext;
            _alarmService = alarmService;

        }

        private async Task<List<Device>> getDevices()
        {
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync("http://localhost:5000/devices");
            using JsonDocument doc = JsonDocument.Parse(response);
            List<Device> devices = new List<Device>();
            JsonElement root = doc.RootElement;
            try
            {
                JsonElement data = root.GetProperty("data");
                devices = JsonSerializer.Deserialize<List<Device>>(data.GetRawText());
                //foreach (Device item in devices)
                //{
                //    ModbusTcpHelper modbustcp = new ModbusTcpHelper(item);
                //}
                
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"操作无效：{ex.Message}");
            }
            return devices;
        }

        public async Task StartAllDevicesAsync()
        {
            List<Device> devices = await getDevices();
            lock (_lock)
            {
                foreach (Device item in devices)
                {
                    _devices[item.id] = new DeviceCollectorTask(item, _logger, _hubContext, _alarmService);
                }
            }
        }

        public void StopAllDevices()
        {
            lock (_lock)
            {
                foreach (var task in _devices.Values)
                {
                    task.Stop();
                }
                _devices.Clear();
            }
        }

        //public bool StopDevice(string deviceId)
        //{
        //    lock (_lock)
        //    {
        //        if (!_devices.TryGetValue(deviceId, out var task))
        //            return false;

        //        task.Stop();
        //        _devices.Remove(deviceId);
        //        return true;
        //    }
        //}

        //public List<string> GetRunningDevices()
        //{
        //    lock (_lock)
        //    {
        //        return _devices
        //            .Where(kvp => kvp.Value.IsRunning)
        //            .Select(kvp => kvp.Key)
        //            .ToList();
        //    }
        //}

        //public bool IsDeviceRunning(string deviceId)
        //{
        //    lock (_lock)
        //    {
        //        return _devices.ContainsKey(deviceId) && _devices[deviceId].IsRunning;
        //    }
        //}
    }

}
