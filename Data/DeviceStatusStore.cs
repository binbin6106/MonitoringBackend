using System.Collections.Concurrent;

public static class DeviceStatusStore
{
    // 存储设备在线状态（true=在线，false=离线）
    public static ConcurrentDictionary<long, bool> OnlineStatus = new ConcurrentDictionary<long, bool>();
}
