namespace MonitoringBackend.Models
{
    public class Device
    {
        public long id { get; set; }
        public string name { get; set; } = "";
        public string? location { get; set; }
        public string? description { get; set; }
        public string? image { get; set; }
        public float x { get; set; } = 0.0f; // 默认值为0
        public float y { get; set; } = 0.0f; // 默认值为0
        public float width { get; set; } = 0.0f; // 默认值为0
        public float height { get; set; } = 0.0f; // 默认值为0
        public float radius { get; set; } = 0.0f; // 半径，默认为0
        public string type { get; set; } = "rect"; // 默认类型为矩形
    }
}