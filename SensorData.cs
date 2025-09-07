using MonitoringBackend.Models;

namespace MonitoringBackend
{
    public class SensorData
    {
        public long SensorId { get; set; }
        public string? SensorType {  get; set; }
        public string? Name { get; set; }
        public long DeviceId { get; set; }
        public long GatewayId { get; set; }
        public double Temperature { get; set; }
        public double XVibration { get; set; }
        public double YVibration { get; set; }
        public double ZVibration { get; set; }
        public double Vibration { get; set; }
        public int RPM { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
