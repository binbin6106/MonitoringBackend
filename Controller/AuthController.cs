using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonitoringBackend.Data;
using MonitoringBackend.Helpers;
using MonitoringBackend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly JwtHelper _jwt;
        private readonly AppDbContext _context;
        public AuthController(IConfiguration config, AppDbContext context)
        {
            _jwt = new JwtHelper(config);
            _context = context;
        }

        [HttpPost("login")]
        public async Task <IActionResult> Login([FromBody] Models.LoginRequest request)
        {
            // 从数据库查找用户
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.username == request.Username && u.password == request.Password);

            if (user == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("用户名或密码错误"));
            }
            if (!user.isEnabled)
            {
                return Unauthorized(ApiResponse<string>.Fail("用户已被禁用"));
            }
                
            /// 生成 JWT token
            var token = _jwt.GenerateToken(user);

            var result = ApiResponse<object>.Success(new
            {
                access_token = token
            });

            return Ok(result);
        }

        [Authorize]
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(ApiResponse<object>.Success("登出成功"));
        }

        [Authorize]
        [HttpGet("buttons")]
        public IActionResult GetUserInfo()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Ok($"你的用户ID是: {userId}");
        }

    }



}
