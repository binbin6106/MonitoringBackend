namespace MonitoringBackend.Models
{
    public class Device
    {
        public long id { get; set; }
        public string name { get; set; } = "";
        public string? location { get; set; }
        public string? description { get; set; }
        public string? model { get; set; }
        public int x { get; set; } = 0; // 默认值为0
        public int y { get; set; } = 0; // 默认值为0
        public int z { get; set; } = 0;
        public string line_start { get; set; } = "0,0,0";
        public string line_end { get; set; } = "0,0,0";
        public float width { get; set; } = 0.0f; // 默认值为0
        public float height { get; set; } = 0.0f; // 默认值为0
        public float radius { get; set; } = 0.0f; // 半径，默认为0
        public string type { get; set; } = "rect"; // 默认类型为矩形
    }
}