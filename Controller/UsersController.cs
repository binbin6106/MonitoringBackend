using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Data;
using MonitoringBackend.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UsersController(AppDbContext context) => _context = context;

        // 获取所有用户
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var result = ApiResponse<object>.Success(await _context.Users.ToListAsync());
            return Ok(result);
        }

        // 添加用户
        [HttpPost]
        public async Task<IActionResult> AddUser([FromBody] User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Success(""));
        }

        // 更新用户
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(long id, [FromBody] User updated)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.id == id);
            if (user == null) return NotFound();

            user.username = updated.username;
            user.email = updated.email;
            user.role = updated.role;
            user.password = updated.password;
            user.isEnabled = updated.isEnabled;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Success(user));
        }

        //[HttpPost("change")]
        //public async Task<IActionResult> changeUser([FromBody] ChangeUserDto dto)
        //{
        //    var user = await _context.Users.FirstOrDefaultAsync(u => u.id == dto.Id);
        //    if (user == null) return NotFound();
        //    user.isEnabled = dto.Enabled;
        //    return Ok(ApiResponse<object>.Success(""));
        //}
        [Authorize]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteUsers([FromBody] DeleteUserRequest request)
        {
            if (request.id == null || request.id.Count == 0)
                return BadRequest(ApiResponse<string>.Fail("缺少要删除的用户 ID"));

            var users = await _context.Users
                .Where(u => request.id.Contains(u.id))
                .ToListAsync();

            if (users.Count == 0)
                return NotFound(ApiResponse<string>.Fail("未找到对应的用户"));

            _context.Users.RemoveRange(users);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.Success("删除成功"));
        }



        // 切换启用状态
        [HttpPatch("{id}/toggle")]
        public async Task<IActionResult> ToggleUserStatus(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            user.isEnabled = !user.isEnabled;
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.Success(new { user.id, user.isEnabled }));
        }
    }
    public class ChangeUserDto
    {
        public long id { get; set; }
        public bool enable { get; set; }
    }

    public class DeleteUserRequest
    {
        public List<long> id { get; set; }
    }

}
