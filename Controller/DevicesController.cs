using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Data;
using MonitoringBackend.Models;
using Newtonsoft.Json.Linq;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public DevicesController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetDevices()
        {
            var result = ApiResponse<object>.Success(await _context.Devices.Include(d => d.sensors).ToListAsync());
            return Ok(result);
        }
            

        [HttpPost]
        public async Task<IActionResult> AddDevice([FromBody] Device device)
        {
            _context.Devices.Add(device);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Success(""));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDevice(long id, [FromBody] Device updated)
        {
            var device = await _context.Devices.Include(d => d.sensors).FirstOrDefaultAsync(d => d.id == id);
            if (device == null) return NotFound();

            device.name = updated.name;
            device.location = updated.location;
            device.description = updated.description;
            device.ip = updated.ip;
            device.image = updated.image;

            // 先删除不存在的传感器
            device.sensors.RemoveAll(s => !updated.sensors.Any(us => us.id == s.id));
            // 添加或更新传感器
            foreach (var sensor in updated.sensors)
            {
                var existing = device.sensors.FirstOrDefault(s => s.id == sensor.id);
                if (existing != null)
                {
                    existing.name = sensor.name;
                    existing.type = sensor.type;
                    existing.unit = sensor.unit;
                }
                else
                {
                    sensor.device_id = device.id;
                    device.sensors.Add(sensor);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(device);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(long id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null) return NotFound();

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Success(""));
        }
    }

}
