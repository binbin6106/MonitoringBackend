namespace MonitoringBackend.Models
{
    public class Gateway
    {
        public long id { get; set; }
        public string name { get; set; } = "";
        public string ip { get; set; } = "";
        public int cannel_count { get; set; }
        public List<Sensor> sensors { get; set; } = [];
    }
}
