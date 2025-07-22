using System.ComponentModel.DataAnnotations.Schema;

namespace MonitoringBackend.Models
{
    public class Sensor
    {
        public long id { get; set; }
        public long device_id { get; set; }
        public long gateway_id { get; set; }
        public string name { get; set; } = "";
        public string type { get; set; } = "";
        public string? unit { get; set; }
        public string ? low_threshold { get; set; } //下限阈值
        public string? up_threshold { get; set; } // 上限阈值
       [NotMapped]
        public string? device_name { get; set; }
    }
}
