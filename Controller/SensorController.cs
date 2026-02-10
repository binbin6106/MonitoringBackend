using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Authorize]
    [Route("api/sensor")]
    public class SensorController : ControllerBase
    {
        private readonly InfluxService _influx;
        
        public SensorController(InfluxService influx)
        {
            _influx = influx;
        }

        [HttpPost("write")]
        public async Task<IActionResult> Write(SensorData data)
        {
            await _influx.WriteSensorDataAsync(data);
            return Ok();
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int sensorId, [FromQuery] DateTime start, [FromQuery] DateTime end)
        {
            var data = await _influx.QuerySensorDataAsync(sensorId, start, end);
            return Ok(data);
        }

        //[HttpGet("history")]
        //public async Task<IActionResult> GetHistory([FromQuery] int sensorId, DateTime from, DateTime to)
        //{
        //    var result = await _influx.QuerySensorDataAsync(sensorId, from, to);
        //    return Ok(result);
        //}
    }

}
