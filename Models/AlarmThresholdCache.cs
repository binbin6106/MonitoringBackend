namespace MonitoringBackend.Models
{
    public class AlarmThresholdCache
    {
        private readonly Dictionary<string, AlarmThreshold> _thresholds = new();

        public void Load(IEnumerable<AlarmThreshold> thresholds)
        {
            _thresholds.Clear();
            foreach (var t in thresholds)
            {
                _thresholds[$"{t.sensor_id}:{t.alarmType}"] = t;
            }
        }

        public AlarmThreshold? Get(long sensorId, string type)
        {
            _thresholds.TryGetValue($"{sensorId}:{type}", out var value);
            return value;
        }

        public void Update(AlarmThreshold t)
        {
            _thresholds[$"{t.sensor_id}:{t.alarmType}"] = t;
        }
    }

}
