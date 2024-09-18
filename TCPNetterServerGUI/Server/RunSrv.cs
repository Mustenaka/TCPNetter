using System.Net;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Channels;
using TCPNetterServerGUI.Server.Handler;
using TCPNetterServerGUI.Server.Params;

namespace TCPNetterServerGUI.Server;

public class RunSrv
{
    private MainForm _mainForm;

    public RunSrv(MainForm mainForm)
    {
        _mainForm = mainForm;
    }

    /// <summary>
    /// 服务器是否已运行
    /// </summary>
    private bool IsServerRunning = false;
    /// <summary>
    /// 关闭侦听器事件
    /// </summary>
    private ManualResetEvent ClosingArrivedEvent = new ManualResetEvent(false);

    /// <summary>
    /// 启动服务
    /// </summary>
    public void Start()
    {
        try
        {
            if (IsServerRunning)
            {
                ClosingArrivedEvent.Set();  // 停止侦听
            }
            else
            {
                IPAddress ServerIP = IPAddress.Parse("0.0.0.0"); // 服务器地址
                int ServerPort = 8188; // 服务器端口
                int Backlog = 100; // 最大连接等待数

                //线程池任务
                ThreadPool.QueueUserWorkItem(ThreadPoolCallback,
                    new TcpServerParams()
                    {
                        ServerIP = ServerIP,
                        ServerPort = ServerPort,
                        Backlog = Backlog
                    });
            }
        }
        catch (Exception exp)
        {
            Console.WriteLine(exp.ToString());
        }
    }

    private void ThreadPoolCallback(object state)
    {
        TcpServerParams Args = state as TcpServerParams;
        StartServerAsync(Args).Wait();
    }

    // 启动服务器
    public async Task StartServerAsync(TcpServerParams args)
    {
        IEventLoopGroup bossGroup;
        IEventLoopGroup workerGroup;

        bossGroup = new MultithreadEventLoopGroup(1);
        workerGroup = new MultithreadEventLoopGroup();

        try
        {
            var bootstrap = new ServerBootstrap();
            var _serverHandler = new NetterServerHandler(_mainForm);
            //var _serverHandler = new FlowServerHandler();

            bootstrap
                .Group(bossGroup, workerGroup)
                .Channel<TcpServerSocketChannel>() // 使用TCP服务
                .Option(ChannelOption.SoBacklog, args.Backlog)
                .Option(ChannelOption.SoKeepalive, true)//保持连接
                                                        //.Option(ChannelOption.AllowHalfClosure, true)
                .Handler(new LoggingHandler("SRV-LSTN"))//在主线程组上设置一个打印日志的处理器
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    // 设定Pipeline来处理入站和出站消息
                    var pipeline = channel.Pipeline;

                    //IdleStateHandler 心跳
                    pipeline.AddLast(new IdleStateHandler(150, 0, 0));//第一个参数为读，第二个为写，第三个为读写全部

                    //出栈消息，通过这个handler 在消息顶部加上消息的长度
                    //pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    //入栈消息通过该Handler,解析消息的包长信息，并将正确的消息体发送给下一个处理Handler
                    //pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));

                    pipeline.AddLast(new MessageDecoder()); // 添加解码器
                    pipeline.AddLast(new MessageEncoder()); // 添加编码器
                    pipeline.AddLast("NettyServer", new NetterServerHandler(_mainForm)); // 添加自定义处理器
                }));

            // 绑定服务器端口并启动
            var _serverChannel = await bootstrap.BindAsync(args.ServerPort);

            Console.WriteLine(@$"Server started and listening {args.ServerIP}:{args.ServerPort}");

            // 启动心跳包监控
            _serverHandler.StartHeartbeatMonitor();

            //运行至此处，服务启动成功
            IsServerRunning = true;

            ClosingArrivedEvent.Reset();
            ClosingArrivedEvent.WaitOne();
            await _serverChannel.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(@$"Server failed to start: {ex.Message}");
        }
        finally
        {
            await Task.WhenAll(
                bossGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)),
                workerGroup.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1)));
        }
    }
}