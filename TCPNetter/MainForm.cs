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
                var sendModel = new MessageModel()
                {
                    DeviceName = "1234",
                    Id = "自己的这台电脑ID",
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
