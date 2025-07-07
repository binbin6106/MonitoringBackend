namespace MonitoringBackend.Models
{
    public class Device
    {
        public long id { get; set; }
        public string name { get; set; } = "";
        public string ip { get; set; } = "";
        public string? location { get; set; }
        public string? description { get; set; }
        public List<Sensor> sensors { get; set; } = [];
        public string? image { get; set; }
    }
}
