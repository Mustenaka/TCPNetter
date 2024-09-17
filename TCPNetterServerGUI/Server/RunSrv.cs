using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using TCPNetterServerGUI.Server.Handler;

namespace TCPNetterServerGUI.Server;

public class RunSrv
{
    private IChannel _serverChannel;
    private MultithreadEventLoopGroup _bossGroup;
    private MultithreadEventLoopGroup _workerGroup;
    private NetterServerHandler _serverHandler;

    private MainForm _mainForm;

    public RunSrv(MainForm mainForm)
    {
        _mainForm = mainForm;
    }

    // 启动服务器
    public async Task StartServerAsync(int port)
    {
        _bossGroup = new MultithreadEventLoopGroup(1);
        _workerGroup = new MultithreadEventLoopGroup();

        try
        {
            var bootstrap = new ServerBootstrap();
            _serverHandler = new NetterServerHandler(_mainForm); // 创建服务器处理器

            bootstrap
                .Group(_bossGroup, _workerGroup)
                .Channel<TcpServerSocketChannel>()  // 使用TCP服务
                .Option(ChannelOption.SoBacklog, 100)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    // 设定Pipeline来处理入站和出站消息
                    var pipeline = channel.Pipeline;
                    pipeline.AddLast(new MessageDecoder());  // 添加解码器
                    pipeline.AddLast(new MessageEncoder());  // 添加编码器
                    pipeline.AddLast(_serverHandler);         // 添加自定义处理器
                }));

            // 绑定服务器端口并启动
            _serverChannel = await bootstrap.BindAsync(port);
            Console.WriteLine($"Server started and listening on port {port}");

            // 启动心跳包监控
            _serverHandler.StartHeartbeatMonitor();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Server failed to start: {ex.Message}");
        }
    }

    // 停止服务器
    public async Task StopServerAsync()
    {
        try
        {
            if (_serverChannel != null)
            {
                await _serverChannel.CloseAsync();
            }
        }
        finally
        {
            await Task.WhenAll(
                _bossGroup.ShutdownGracefullyAsync(),
                _workerGroup.ShutdownGracefullyAsync());
        }
    }
}