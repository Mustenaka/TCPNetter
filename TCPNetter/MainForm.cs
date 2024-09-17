using TCPNetter.Model;
using TCPNetter.ViewModel;

namespace TCPNetter
{
    public partial class MainForm : Form
    {
        private Client.Client _client;
        private UIViewModel _viewModel;
        private CancellationTokenSource _cancellationTokenSource;
        private SynchronizationContext _syncContext;

        public MainForm()
        {
            InitializeComponent();

            ConnectServer();

            BindModel();
        }

        private async void ConnectServer()
        {
            _client = new();
            await _client.ConnectAsync("127.0.0.1", 8007);

            _syncContext = SynchronizationContext.Current;
            StartReceivingMessages();
            StartSendingHeartbeats();
        }

        private void BindModel()
        {
            // ������
            _viewModel = new UIViewModel();
            DGV_Client.DataSource = _viewModel.UIModels;
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
                        var model = await _client.ReceiveMessageAsync();
                        if (model != null)
                        {
                            // ����UI
                            _syncContext.Post(_ =>
                            {
                                TBox_Result.Text += (model.DeviceName + " | " + model.Message + "\u000D\u000A");
                            }, null);
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
                        Id = "�Լ�����̨����ID",
                        MessageType = "Heartbeats",
                        DeviceName = "None",
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
            var message = TBox_Input.Text;
            foreach (var obs in _viewModel.UIModels)
            {
                var sendModel = new MessageModel()
                {
                    DeviceName = obs.Id,
                    Id = "�Լ�����̨����ID",
                    MessageType = "Message",
                    Message = message,
                };

                await _client.SendMessageAsync(sendModel);
            }
        }
    }
}
