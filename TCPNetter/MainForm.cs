using TCPNetter.Model;
using TCPNetter.ViewModel;

namespace TCPNetter
{
    public partial class MainForm : Form
    {
        private Client.Client _client;
        private CancellationTokenSource _cancellationTokenSource;
        private SynchronizationContext _syncContext;

        public MainForm()
        {
            InitializeComponent();

            ConnectServer();
        }

        private async void ConnectServer()
        {
            try
            {
                _client = new();
                await _client.ConnectAsync("127.0.0.1", 8188);

                _syncContext = SynchronizationContext.Current;
                StartSendingHeartbeats();
            }catch(Exception err)
            {
                Console.WriteLine(err.Message);
            }
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
                var sendModel = new MessageModel()
                {
                    DeviceName = "1234",
                    Id = "�Լ�����̨����ID",
                    MessageType = "Message",
                    Message = message,
                };

                await _client.SendMessageAsync(sendModel);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

        }
    }
}
