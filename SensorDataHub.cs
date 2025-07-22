using Microsoft.AspNetCore.SignalR;

namespace MonitoringBackend;

public class DeviceDataHub : Hub
{
    // 这里不写任何代码，仅作为推送通道使用
    public override async Task OnConnectedAsync()
    {
        // 添加用户到某个列表（最好使用带过期的缓存）
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // 清除用户相关资源、状态、订阅等
        // 如：从内存中的连接列表中移除
        await base.OnDisconnectedAsync(exception);
    }
}

public class GatewayHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // 添加用户到某个列表（最好使用带过期的缓存）
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // 清除用户相关资源、状态、订阅等
        // 如：从内存中的连接列表中移除
        await base.OnDisconnectedAsync(exception);
    }
    // 这里不写任何代码，仅作为推送通道使用
}
