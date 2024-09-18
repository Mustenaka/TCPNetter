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
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }
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
                                        TBox_Input.Text = model.Message;

                                        var resopn = new MessageModel()
                                        {
                                            MessageType = "NoCallback",
                                            Message = model.Message,
                                            DeviceName = _localModel.DeviceName,
                                            Target = model.Target,
                                        };

                                        await _client.SendMessageAsync(resopn);

                                        break;
                                }
                                // 更新UI
                                _syncContext.Post(_ =>
                                {
                                    TBox_Result.Text += (model.MessageType + "|" + model.Target + "|" + model.DeviceName + " | " + model.Message + "\u000D\u000A");
                                }, null);
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
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    _syncContext.Post(_ =>
                    {
                        MessageBox.Show($"Error receiving messages: {ex.Message}");
                    }, null);
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

                    await _client.SendMessageAsync(heartbeatModel);
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
