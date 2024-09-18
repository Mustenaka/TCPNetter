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
                Id = "�Լ�����̨����ID",
                MessageType = "Message",
                Message = "Ping!", // ���������������һ��
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
        /// ���ն�
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
                                    case "Message": // ���ܵ���Ϣ��Message����Ҫ�ı�����
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
                                // ����UI
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
                                // ����UI
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
        /// ������
        /// </summary>
        private void StartSendingHeartbeats()
        {
            var heartbeatInterval = TimeSpan.FromSeconds(30);  // ÿ30�뷢��һ������
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var heartbeatModel = new MessageModel
                    {
                        DeviceName = "1234",
                        Id = "�Լ�����̨����ID",
                        MessageType = "Heartbeats",
                        Message = "Ping!"  // ���������������һ��
                    };

                    await _client.SendMessageAsync(heartbeatModel);
                    await Task.Delay(heartbeatInterval);  // �ȴ���һ������ʱ��
                }
            }, cancellationToken);
        }

        /// <summary>
        /// ������Ϣ��ť
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
