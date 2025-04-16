namespace MonitoringBackend
{
    public class SensorData
    {
        public int SensorId { get; set; }
        public double Temperature { get; set; }
        public double Vibration { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
