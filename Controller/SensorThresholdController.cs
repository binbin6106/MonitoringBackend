using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Data;
using MonitoringBackend.Models;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class SensorThresholdController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SensorThresholdController> _logger;

        public SensorThresholdController(AppDbContext context, ILogger<SensorThresholdController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // 获取传感器的阈值配置
        [HttpGet("{sensorId}")]
        public async Task<ActionResult> GetSensorThreshold(long sensorId)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(sensorId);
                if (sensor == null)
                    return NotFound($"传感器 {sensorId} 不存在");

                var result = new
                {
                    sensor_id = sensor.id,
                    sensor_name = sensor.name,
                    low_threshold = sensor.low_threshold,
                    up_threshold = sensor.up_threshold,
                    // 解析阈值用于前端显示
                    low_thresholds = ParseThresholds(sensor.low_threshold),
                    up_thresholds = ParseThresholds(sensor.up_threshold),
                    channel_names = new[] { "温度", "X方向振动", "Y方向振动", "Z方向振动", "振动矢量和" }
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取传感器 {sensorId} 阈值失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        // 更新传感器阈值
        [HttpPut("{sensorId}")]
        public async Task<ActionResult> UpdateSensorThreshold(long sensorId, [FromBody] UpdateThresholdRequest request)
        {
            try
            {
                var sensor = await _context.Sensors.FindAsync(sensorId);
                if (sensor == null)
                    return NotFound($"传感器 {sensorId} 不存在");

                // 验证阈值数组长度
                if (request.LowThresholds?.Length != 5 || request.UpThresholds?.Length != 5)
                    return BadRequest("阈值数组必须包含5个元素（温度，X振动，Y振动，Z振动，振动矢量和）");

                // 更新阈值字符串
                sensor.low_threshold = string.Join(",", request.LowThresholds);
                sensor.up_threshold = string.Join(",", request.UpThresholds);

                await _context.SaveChangesAsync();

                _logger.LogInformation($"传感器 {sensorId} 阈值更新成功");
                return Ok(new { message = "阈值更新成功", sensor_id = sensorId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"更新传感器 {sensorId} 阈值失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        // 批量更新多个传感器阈值
        [HttpPut("batch")]
        public async Task<ActionResult> UpdateMultipleSensorThresholds([FromBody] List<UpdateThresholdRequest> requests)
        {
            try
            {
                var updatedCount = 0;
                foreach (var request in requests)
                {
                    var sensor = await _context.Sensors.FindAsync(request.SensorId);
                    if (sensor != null && request.LowThresholds?.Length == 5 && request.UpThresholds?.Length == 5)
                    {
                        sensor.low_threshold = string.Join(",", request.LowThresholds);
                        sensor.up_threshold = string.Join(",", request.UpThresholds);
                        updatedCount++;
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation($"批量更新了 {updatedCount} 个传感器的阈值");
                return Ok(new { message = $"成功更新 {updatedCount} 个传感器的阈值" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新传感器阈值失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        // 获取所有传感器列表及其阈值
        [HttpGet]
        public async Task<ActionResult> GetAllSensorThresholds()
        {
            try
            {
                var sensorsWithDevice = await (
                from s in _context.Sensors
                join d in _context.Devices on s.device_id equals d.id // 关键代码：定义两个表的连接条件
                select new
                {
                    sensor_id = s.id,
                    sensor_name = s.name,
                    device_id = s.device_id,
                    gateway_id = s.gateway_id,
                    type = s.type,
                    low_threshold = s.low_threshold,
                    up_threshold = s.up_threshold,
                    device_name = d.name // 直接从 join 后的 d (Device) 对象中获取 name
                })
                .ToListAsync();

                // 后续的内存处理部分与上一种方法完全相同
                var result = sensorsWithDevice.Select(s => new
                {
                    s.sensor_id,
                    s.sensor_name,
                    s.device_id,
                    s.gateway_id,
                    s.type,
                    s.low_threshold,
                    s.up_threshold,
                    s.device_name,
                    low_thresholds = ParseThresholds(s.low_threshold),
                    up_thresholds = ParseThresholds(s.up_threshold)
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有传感器阈值失败");
                return StatusCode(500, "服务器内部错误");
            }
        }

        // 工具方法：解析阈值字符串（改为静态方法）
        private static float[] ParseThresholds(string? thresholdString)
        {
            if (string.IsNullOrEmpty(thresholdString))
                return new float[5]; // 返回默认值

            try
            {
                var parts = thresholdString.Split(',');
                var result = new float[5];
                for (int i = 0; i < Math.Min(parts.Length, 5); i++)
                {
                    if (float.TryParse(parts[i], out var value))
                        result[i] = value;
                }
                return result;
            }
            catch
            {
                return new float[5]; // 解析失败返回默认值
            }
        }
    }

    // 请求模型
    public class UpdateThresholdRequest
    {
        public long SensorId { get; set; }
        public float[]? LowThresholds { get; set; }  // 5个通道的下限阈值
        public float[]? UpThresholds { get; set; }   // 5个通道的上限阈值
    }
}