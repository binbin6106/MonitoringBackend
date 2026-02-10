using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Data;
using MonitoringBackend.Models;
using Newtonsoft.Json.Linq;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class GatewaysController : ControllerBase
    {
        private readonly AppDbContext _context;
        public GatewaysController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetGateways()
        {
            var result = ApiResponse<object>.Success(await _context.Gateways.Include(d => d.sensors).ToListAsync());
            return Ok(result);
        }
            

        [HttpPost]
        public async Task<IActionResult> AddGateway([FromBody] Gateway gateway)
        {
            _context.Gateways.Add(gateway);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Success(""));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGateway(long id, [FromBody] Gateway updated)
        {
            var gateway = await _context.Gateways.Include(d => d.sensors).FirstOrDefaultAsync(d => d.id == id);
            if (gateway == null) return NotFound();

            gateway.name = updated.name;
            gateway.ip = updated.ip;

            // 先删除不存在的传感器
            gateway.sensors.RemoveAll(s => !updated.sensors.Any(us => us.id == s.id));
            // 添加或更新传感器
            foreach (var sensor in updated.sensors)
            {
                var existing = gateway.sensors.FirstOrDefault(s => s.id == sensor.id);
                if (existing != null)
                {
                    existing.name = sensor.name;
                    existing.type = sensor.type;
                    existing.device_id = sensor.device_id;
                }
                else
                {
                    gateway.sensors.Add(sensor);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(gateway);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGateway(long id)
        {
            var gateway = await _context.Gateways.FindAsync(id);
            if (gateway == null) return NotFound();

            _context.Gateways.Remove(gateway);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Success(""));
        }
    }

}
