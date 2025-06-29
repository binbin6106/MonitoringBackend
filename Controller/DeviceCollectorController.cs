using Microsoft.AspNetCore.Mvc;
using MonitoringBackend.Service;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Route("api/device-collector")]
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

        //[HttpPost("stop")]
        //public IActionResult Stop(string deviceId)
        //{
        //    if (_collector.StopDevice(deviceId))
        //        return Ok($"[{deviceId}] 已停止采集");
        //    else
        //        return NotFound($"[{deviceId}] 没有在采集中");
        //}

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
