using TCPNetter.Model;
using TCPNetter.ViewModel;

namespace TCPNetter
{
    public partial class MainForm : Form
    {
        private Client.Client _client;
        private CancellationTokenSource _cancellationTokenSource;
        private SynchronizationContext _syncContext;

        private MessageModel _localModel;
        private bool _isReconnecting = false;
        private bool _isConnecting = false;

        public MainForm()
        {
            InitializeComponent();

            ConnectServer();

            _localModel = new MessageModel()
            {
                DeviceName = "1234",
                Id = "自己的这台电脑ID",
                MessageType = "Message",
                Message = "Ping!", // 心跳包的内容随便一点
            };
        }

        private async void ConnectServer()
        {
            try
            {
                _client = new();
                await _client.ConnectAsync("127.0.0.1", 8188);

                _syncContext = SynchronizationContext.Current;
                StartSendingHeartbeats();
                StartReceivingMessages();

                _isConnecting = true;
            }
            catch (Exception err)
            {
                Console.WriteLine($"Connection failed: {err.Message}");
                StartReconnection();  // 开始断线重连
            }
        }

        /// <summary>
        /// 断线重连机制
        /// </summary>
        private void StartReconnection()
        {
            // 正在连接着不需要进入断线重连
            if (_isConnecting)
            {
                return;
            }

            if (_isReconnecting)
            {
                return;  // 防止多个重连任务同时执行
            }

            _isReconnecting = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    // 正在连接着不需要进入断线重连
                    if (_isConnecting)
                    {
                        return;
                    }

                    Console.WriteLine("Attempting to reconnect...");
                    try
                    {
                        if (_client.IsServerCloese())
                        {
                            _client = new();
                        }
                        await _client.ConnectAsync("127.0.0.1", 8188);  // 尝试重新连接

                        // 通过 UI 线程重新获取 SynchronizationContext
                        this.Invoke((MethodInvoker)delegate
                        {
                            _syncContext = SynchronizationContext.Current;
                        });

                        _isReconnecting = false;
                        StartSendingHeartbeats();  // 重启心跳包
                        StartReceivingMessages();  // 重启接收消息
                        _isConnecting = true;

                        Console.WriteLine("Reconnected successfully!");
                        break;  // 成功重连后退出重连循环
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Reconnection failed. Retrying in 10 seconds...");
                        _isConnecting = false;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10));  // 每10秒重试一次
                }
            });
        }

        /// <summary>
        /// 接收端
        /// </summary>
        private void StartReceivingMessages()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        var obj = await _client.ReceiveMessageAsync();
                        if (obj is MessageModel model)
                        {
                            if (model != null)
                            {
                                switch (model.MessageType)
                                {
                                    case "Message": // 接受的消息是Message，需要改变自身
                                        // 更新UI
                                        _syncContext.Post(_ =>
                                        {
                                            TBox_Input.Text = model.Message;
                                        }, null);

                                        var response = new MessageModel()
                                        {
                                            MessageType = "Callback",
                                            Message = model.Message,
                                            DeviceName = _localModel.DeviceName,
                                            Target = model.Target,
                                        };

                                        await _client.SendMessageAsync(response);
                                        break;
                                }

                                // 更新UI
                                _syncContext.Post(_ =>
                                {
                                    TBox_Result.Text += (model.MessageType + "|" + model.Target + "|" + model.DeviceName + " | " + model.Message + "\u000D\u000A");
                                }, null);
                                //TBox_Result.Text += (model.MessageType + "|" + model.Target + "|" + model.DeviceName + " | " + model.Message + "\u000D\u000A");
                            }
                        }
                        else if (obj is List<MessageModel> list)
                        {
                            foreach (var mod in list)
                            {
                                // 更新UI
                                _syncContext.Post(_ =>
                                {
                                    TBox_Result.Text += (mod.Id + " | " + mod.DeviceName + " | " + mod.Message + "\u000D\u000A");
                                }, null);

                                //TBox_Result.Text += (mod.Id + " | " + mod.DeviceName + " | " + mod.Message + "\u000D\u000A");
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    //_syncContext.Post(_ =>
                    //{
                    //    MessageBox.Show($"Error receiving messages: {ex.Message}");
                    //}, null);

                    Console.WriteLine($"Error receiving messages: {ex.Message}");
                    _isConnecting = false;
                    StartReconnection();  // 当接收消息发生错误时，开始重连
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 心跳包
        /// </summary>
        private void StartSendingHeartbeats()
        {
            var heartbeatInterval = TimeSpan.FromSeconds(30);  // 每30秒发送一次心跳
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var heartbeatModel = new MessageModel
                    {
                        DeviceName = "1234",
                        Id = "自己的这台电脑ID",
                        MessageType = "Heartbeats",
                        Message = "Ping!"  // 心跳包的内容随便一点
                    };

                    try
                    {
                        await _client.SendMessageAsync(heartbeatModel);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending heartbeat: {ex.Message}");
                        _isConnecting = false;
                        StartReconnection();  // 当心跳发送失败时，开始重连
                        break;
                    }

                    await Task.Delay(heartbeatInterval);  // 等待下一个心跳时间
                }
            }, cancellationToken);
        }

        /// <summary>
        /// 发送信息按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Btn_Send_Click(object sender, EventArgs e)
        {
            try
            {
                var message = TBox_Input.Text;
                var type = CBox_MessageType.Text;
                var command = CBox_Command.Text;
                var target = Text_Target.Text;

                _localModel.Message = message;
                _localModel.MessageType = type;
                _localModel.Command = command;
                _localModel.Target = target;

                await _client.SendMessageAsync(_localModel);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

        }
    }

}
