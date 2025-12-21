using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Helpers;
using MonitoringBackend.Models;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class HistoryController : ControllerBase 
    {
        private readonly InfluxDbHelper _influxDbHelper;
        public HistoryController(InfluxDbHelper influxDbHelper)
        {
            _influxDbHelper = influxDbHelper;
        }
        /// <summary>
        /// 获取历史数据
        /// </summary>
        /// <remarks>
        /// 示例请求:
        ///
        ///     GET /history/data
        ///
        /// </remarks>
        /// <returns>返回某个设备的某个传感器在一段时间内的历史数据。</returns>
        /// <response code="200">成功获取产品列表。</response>
        /// <response code="404">未找到任何产品。</response>
        [HttpGet("data")]
        public async Task<IActionResult> GetHistoryData([FromQuery] int device_id, [FromQuery] int sensor_id, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var result = await _influxDbHelper.QuerySensorDataAsync(device_id, sensor_id, start, end);
            return Ok(ApiResponse<object>.Success(result));
        }

        /// <summary>
        /// datazoom 按时间范围返回历史数据
        /// </summary>
        /// <remarks>
        /// 示例请求:
        ///
        ///     GET /history/datazoom
        ///
        /// </remarks>
        /// <returns>返回某个设备的某个传感器在一段时间内的历史数据。</returns>
        /// <response code="200">成功获取数据。</response>
        [HttpGet("datazoom")]
        public async Task<IActionResult> GetHistoryDataByZoom(
            [FromQuery(Name = "device_id")] int device_id,
            [FromQuery(Name = "sensor_id")] int sensor_id,
            [FromQuery(Name = "from")] DateTime from,
            [FromQuery(Name = "to")] DateTime to)
        {
            var result = await _influxDbHelper.QuerySensorDataPagedAsync(device_id, sensor_id, from, to);
            return Ok(ApiResponse<object>.Success(result));
        }
    }
}

