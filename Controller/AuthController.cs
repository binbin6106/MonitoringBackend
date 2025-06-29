using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
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

        public AuthController(IConfiguration config)
        {
            _jwt = new JwtHelper(config);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] Models.LoginRequest request)
        {
            Console.WriteLine(request);
            // 模拟用户验证（实际应查数据库）
            if (request.Username == "admin" && request.Password == Md5Helper.Encrypt("123456"));
            {
                var user = new User { Id = 1, Username = "admin" };
                var token = _jwt.GenerateToken(user);

                var result = ApiResponse<object>.Success(new { access_token = token });

                return Ok(result);  // 返回 JSON 格式
            }

            return Unauthorized("Invalid credentials");
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
