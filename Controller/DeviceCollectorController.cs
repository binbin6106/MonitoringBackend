using Microsoft.AspNetCore.Mvc;
using MonitoringBackend.Service;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Route("device-control")]
    public class DeviceCollectorController : ControllerBase
    {
        private readonly MultiDeviceCollectorService _collector;

        public DeviceCollectorController(MultiDeviceCollectorService collector)
        {
            _collector = collector;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start()
        {
            await _collector.StartAllDevicesAsync();
            return Ok("采集已启动");
        }

        [HttpPost("stop")]
        public IActionResult Stop()
        {
            _collector.StopAllDevices();
            return Ok("采集已停止");
        }

        //[HttpGet("status")]
        //public IActionResult Status(string deviceId)
        //{
        //    return Ok(_collector.IsDeviceRunning(deviceId) ? "采集中" : "未采集");
        //}

        //[HttpGet("list")]
        //public IActionResult List()
        //{
        //    return Ok(_collector.GetRunningDevices());
        //}
    }

}
