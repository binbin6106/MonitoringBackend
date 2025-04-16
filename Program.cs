using Microsoft.AspNetCore.SignalR;
using MonitoringBackend;

var builder = WebApplication.CreateBuilder(args);

// 注册 SignalR 和后台服务
builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.Configure<InfluxSettings>(builder.Configuration.GetSection("InfluxDB"));
builder.Services.AddSingleton<InfluxService>();
builder.Services.AddHostedService<TcpListenerService>();
// ✅ 注册 CORS 服务
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();;
    });
});

var app = builder.Build();

// ✅ 启用 CORS

app.UseRouting();

app.UseCors();

app.MapHub<SensorDataHub>("/sensorHub");

app.MapControllers();

app.Run();

