using Microsoft.AspNetCore.SignalR;
using MonitoringBackend;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MonitoringBackend.Data;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Service;
using MonitoringBackend.Models;

var builder = WebApplication.CreateBuilder(args);

// 注册 SignalR 和后台服务
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.Configure<InfluxSettings>(builder.Configuration.GetSection("InfluxDB"));
builder.Services.AddSingleton<InfluxService>();
//builder.Services.AddHostedService<TcpListenerService>();
builder.Services.AddSingleton<MultiDeviceCollectorService>();
builder.Services.AddSingleton<AlarmThresholdCache>();
builder.Services.AddScoped<AlarmService>();
// ✅ 注册 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:8848")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var config = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Issuer"],
            ValidAudience = config["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["SecretKey"]))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql("server=39.106.56.86;port=33060;database=monitor;user=root;password=zhangwenbin123;", ServerVersion.AutoDetect("server=39.106.56.86;port=33060;database=monitor;user=root;password=zhangwenbin123;")));
    //options.UseMySql("server=rm-m5e5bp66w746nb36muo.mysql.rds.aliyuncs.com;port=3306;database=monitor;user=monitor;password=Zhangwenbin123!;", ServerVersion.AutoDetect("server=rm-m5e5bp66w746nb36muo.mysql.rds.aliyuncs.com;port=3306;database=monitor;user=monitor;password=Zhangwenbin123!;")));
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ✅ 启用 CORS

app.UseRouting();

app.UseAuthentication();  // 启用认证（解析 Token）
app.UseAuthorization();   // 启用授权（处理 [Authorize] 特性）

app.UseCors();

app.MapHub<DeviceDataHub>("/sensorHub");

app.MapControllers();

app.UseStaticFiles();
app.Run();

