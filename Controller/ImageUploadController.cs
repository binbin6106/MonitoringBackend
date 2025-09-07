using Microsoft.AspNetCore.Mvc;
using MonitoringBackend.Models;

namespace MonitoringBackend.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class ImageUploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageUploadController(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }
        [Consumes("multipart/form-data")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] FileUploadRequest request)
        {
            if (request.file == null || request.file.Length == 0)
                return BadRequest("文件为空");

            // 创建保存路径
            var uploadsFolder = "/home/wwwroot/demo.cyblog.top/uploads/";
            //var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // 生成唯一文件名
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(request.file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // 保存图片
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.file.CopyToAsync(stream);
            }

            // 构造图片访问URL
            var request_1 = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request_1?.Scheme}://{request_1?.Host}";
            var imageUrl = $"{baseUrl}/uploads/{fileName}";

            return Ok(ApiResponse<object>.Success(new
            {
                fileUrl = imageUrl
            }));
        }
    }

    public class FileUploadRequest
    {
        // 属性名可以随意，但要与表单中的字段名对应
        public IFormFile file { get; set; }
    }
}
