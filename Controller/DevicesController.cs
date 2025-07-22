using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Data;
using MonitoringBackend.Models;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

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
            var result = ApiResponse<object>.Success(await _context.Devices.ToListAsync());
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
            var device = await _context.Devices.FirstOrDefaultAsync(d => d.id == id);
            if (device == null) return NotFound();

            device.name = updated.name;
            device.location = updated.location;
            device.description = updated.description;
            device.image = updated.image;

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
