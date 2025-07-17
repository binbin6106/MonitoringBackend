using Microsoft.AspNetCore.SignalR;
using MonitoringBackend.Data;
using MonitoringBackend.Models;

namespace MonitoringBackend.Service
{
    public class AlarmService
    {
        private readonly AlarmThresholdCache _cache;
        private readonly AppDbContext _db;
        private readonly IHubContext<AlarmHub> _hub;

        public AlarmService(AlarmThresholdCache cache, AppDbContext db, IHubContext<AlarmHub> hub)
        {
            _cache = cache;
            _db = db;
            _hub = hub;
        }

        public async Task ProcessDataAsync(long sensorId, string type, float value)
        {
            var threshold = _cache.Get(sensorId, type);
            if (threshold == null) return;

            var isAlarm = false;
            string level = "info";

            if (threshold.thresholdMax.HasValue && value > threshold.thresholdMax)
            {
                isAlarm = true;
                level = value > threshold.levelCritical ? "critical" :
                        value > threshold.levelWarning ? "warning" : "normal";
            }

            if (threshold.thresholdMin.HasValue && value < threshold.thresholdMin)
            {
                isAlarm = true;
                level = value < threshold.levelCritical ? "critical" :
                        value < threshold.levelWarning ? "warning" : "normal";
            }

            if (!isAlarm) return;

            var alarm = new AlarmRecord
            {
                sensor_id = sensorId,
                alarmType = type,
                alarmValue = value,
                thresholdMin = threshold.thresholdMin,
                thresholdMax = threshold.thresholdMax,
                alarmLevel = level,
                alarmTime = DateTime.UtcNow,
                message = $"Sensor {sensorId} value {value} triggered {level} alarm"
            };

            _db.AlarmRecords.Add(alarm);
            await _db.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("ReceiveAlarm", alarm);
        }
    }

}
