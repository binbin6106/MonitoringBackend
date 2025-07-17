namespace MonitoringBackend.Models
{
    public class AlarmThreshold
    {
        public long id { get; set; }
        public long sensor_id { get; set; }
        public string alarmType { get; set; } = null!;
        public float? thresholdMin { get; set; }
        public float? thresholdMax { get; set; }
        public float? levelWarning { get; set; }
        public float? levelCritical { get; set; }
        public DateTime updatedAt { get; set; } = DateTime.UtcNow;

        public Sensor sensor { get; set; } = null!;
    }

}
