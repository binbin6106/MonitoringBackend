namespace MonitoringBackend
{
    public class SensorData
    {
        public long SensorId { get; set; }
        public double Temperature { get; set; }
        public double XVibration { get; set; }
        public double YVibration { get; set; }
        public double ZVibration { get; set; }
        public double Vibration { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
