using Microsoft.AspNetCore.SignalR;
using MonitoringBackend;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MonitoringBackend.Data;
using Microsoft.EntityFrameworkCore;
using MonitoringBackend.Service;
using MonitoringBackend.Models;
using MonitoringBackend.Helpers;
using Microsoft.OpenApi.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
// 注册 SignalR 和后台服务
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // 读取 XML 注释
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // API 信息
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "我的 API 文档",
        Version = "v1",
        Description = "这是一个自动生成的 API 文档示例"
    });
});
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.Configure<InfluxSettings>(builder.Configuration.GetSection("InfluxDB"));
builder.Services.AddSingleton<InfluxDbHelper>();
//builder.Services.AddHostedService<TcpListenerService>();
builder.Services.AddSingleton<MultiDeviceCollectorService>();
//builder.Services.AddHostedService<MultiDeviceCollectorHostedService>();
//开发环境
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
//生产环境
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy
//            .WithOrigins("http://demo.cyblog.top")
//            .AllowAnyHeader()
//            .AllowAnyMethod()
//            .AllowCredentials();
//    });
//});

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
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
//options.UseMySql("server=127.0.0.1;port=33060;database=monitor;user=root;password=zhangwenbin123;", ServerVersion.AutoDetect("server=39.106.56.86;port=33060;database=monitor;user=root;password=zhangwenbin123;")));
//options.UseMySql("server=rm-m5e5bp66w746nb36muo.mysql.rds.aliyuncs.com;port=3306;database=monitor;user=monitor;password=Zhangwenbin123!;", ServerVersion.AutoDetect("server=rm-m5e5bp66w746nb36muo.mysql.rds.aliyuncs.com;port=3306;database=monitor;user=monitor;password=Zhangwenbin123!;")));
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ✅ 启用 CORS

app.UseRouting();

app.UseAuthentication();  // 启用认证（解析 Token）
app.UseAuthorization();   // 启用授权（处理 [Authorize] 特性）

app.UseCors(); //生产环境取消Cors

//app.MapHub<DeviceDataHub>("/sensorHub");
app.MapHub<GatewayHub>("/sensorHub");

app.MapControllers();

app.UseStaticFiles();
app.Run();

