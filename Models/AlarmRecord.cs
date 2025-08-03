namespace MonitoringBackend.Models
{
    public class AlarmRecord
    {
        public long id { get; set; }
        public long sensor_id { get; set; }
        public int channel_id { get; set; }
        public string? device_name { get; set; }
        public long? device_id { get; set; }
        public string? sensor_name { get; set; }
        public string? point_name { get; set; }
        public string alarmType { get; set; } = null!;
        public float alarmValue { get; set; }
        public float? thresholdMin { get; set; }
        public float? thresholdMax { get; set; }
        public string? alarmLevel { get; set; }
        public DateTime alarmTime { get; set; } = DateTime.UtcNow;
        public bool handled { get; set; } = false;
        public string? message { get; set; }
    }

}
