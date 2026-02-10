using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MenuController : ControllerBase
    {
        [HttpGet("list")]
        public IActionResult GetRoutes()
        {
            var username = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            var json = System.IO.File.ReadAllText("Data/routes.json"); // 或者直接在代码中写入字符串
            var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<object>(json);

            return Ok(new
            {
                code = 200,
                data = obj,
                msg = "成功"
            });
        }
    }

}
