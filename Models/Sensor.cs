namespace MonitoringBackend.Models
{
    public class Sensor
    {
        public long id { get; set; }
        public long device_id { get; set; }
        public string name { get; set; } = "";
        public string type { get; set; } = "";
        public string? unit { get; set; }
    }
}
