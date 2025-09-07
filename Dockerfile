# 使用官方 .NET ASP.NET 运行时镜像作为运行环境
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# 设置工作目录
WORKDIR /app

# 将发布好的应用文件复制到容器中
# 确保在运行 docker build 时，你的 publish 目录在当前上下文中
COPY . .

# 暴露端口
EXPOSE 5000

# 定义入口点
ENTRYPOINT ["dotnet", "MonitoringBackend.dll"]