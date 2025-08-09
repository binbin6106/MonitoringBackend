using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonitoringBackend.Data;
using MonitoringBackend.Helpers;
using MonitoringBackend.Models;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;

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
        [HttpGet("list")]
        public IActionResult GetRoutes()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var json = "";
            if (role == "admin")
            {
                json = System.IO.File.ReadAllText("Data/AdminRouter.json"); // 或者直接在代码中写入字符串
            }
            else if (role == "user")
            {
                json = System.IO.File.ReadAllText("Data/UserRouter.json"); // 或者直接在代码中写入字符串
            }
            else {; }

            //var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(json);
            var routes = JsonNode.Parse(json);
            var responseObject = new JsonObject
            {
                // 像字典一样直接添加属性
                ["code"] = 200,
                ["msg"] = "成功",
                ["data"] = routes // 直接把从文件读取的 JsonNode 赋值给 data 属性
            };
            return Ok(responseObject);
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
