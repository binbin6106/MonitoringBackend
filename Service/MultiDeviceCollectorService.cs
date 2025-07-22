using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Data;
using MonitoringBackend.Helpers;
using MonitoringBackend.Models;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace MonitoringBackend.Service
{
    public class MultiDeviceCollectorService
    {
        private readonly Dictionary<long, GatewayCollectorTask> _gateways = new();
        private readonly ILogger<MultiDeviceCollectorService> _logger;
        private readonly IHubContext<GatewayHub> _hubContext;
        private readonly object _lock = new();
        //private List<Device> devices = new List<Device>();

        public MultiDeviceCollectorService(ILogger<MultiDeviceCollectorService> logger, IHubContext<GatewayHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
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
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"操作无效：{ex.Message}");
            }
            return devices;
        }

        private async Task<List<Gateway>> getGateways()
        {
            HttpClient client = new HttpClient();
            string response = await client.GetStringAsync("http://localhost:5000/gateways");
            using JsonDocument doc = JsonDocument.Parse(response);
            List<Gateway> gateways = new List<Gateway>();
            JsonElement root = doc.RootElement;
            try
            {
                JsonElement data = root.GetProperty("data");
                gateways = JsonSerializer.Deserialize<List<Gateway>>(data.GetRawText());
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"操作无效：{ex.Message}");
            }
            return gateways;
        }

        public async Task StartAllDevicesAsync()
        {
            List<Gateway> gateways = await getGateways();
            List<Device> devices = await getDevices();
            Dictionary<long, string> deviceData = devices.ToDictionary(d => d.id, d => d.name);


            foreach (var gateway in gateways)
            {
                foreach (var sensor in gateway.sensors)
                {
                    if (deviceData.TryGetValue(sensor.device_id, out var deviceName))
                    {
                        sensor.device_name = deviceName;
                    }
                    else
                    {
                        sensor.device_name = "未知设备";
                    }
                }
            }

            lock (_lock)
            {
                foreach (Gateway item in gateways)
                {
                    _gateways[item.id] = new GatewayCollectorTask(item, _logger, _hubContext);
                }
                //foreach (Device item in devices)
                //{
                //    _devices[item.id] = new DeviceCollectorTask(item, _logger, _hubContext);
                //}
            }
        }

        public void StopAllDevices()
        {
            lock (_lock)
            {
                foreach (var task in _gateways.Values)
                {
                    task.Stop();
                }
                _gateways.Clear();
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
