using TCPNetterServerGUI.GUI.Model;
using TCPNetterServerGUI.GUI.ViewModel;
using TCPNetterServerGUI.Server;
using TCPNetterServerGUI.Server.Handler;

namespace TCPNetterServerGUI
{
    public partial class MainForm : Form
    {
        private RunSrv run;     // �����������߼�
        private UIViewModel vm; // UI����ģ��

        public MainForm()
        {
            InitializeComponent();

            StartServer();

            Bind();
        }

        private void StartServer()
        {
            run = new RunSrv(this); 
            run.Start();
        }

        private void Bind()
        {
            vm = new UIViewModel();

            Table_Device.Columns = new()
            {
                new AntdUI.Column(nameof(UIModel.Id), "ͨ��ID"),
                new AntdUI.Column(nameof(UIModel.DeviceName), "�豸����"),
                new AntdUI.Column(nameof(UIModel.Message), "��Ϣ"),
            };

            Table_Device.Binding(vm.UIModels);
        }

        #region Event

        /// <summary>
        /// ������Netter������Ϣ | ���������������޸�
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Send_Click(object sender, EventArgs e)
        {
            var message = Inp_SendMessage.Text;

            // һ��һ�ķ���ȫ����Ϣ
            foreach (var model in vm.UIModels)
            {
                NetterServerHandler.SendMessageToClient(model.Id, message, "");

                var sendMessage = $@"����Message�������ݸ�{model.Id},����:{message}" + "\u000D\u000A";   // ���з�
                TBox_Console.Text += sendMessage;
            }

            // ֱ���ù㲥���ܷ�����Ϣ
            //NetterServerHandler.BroadcastMessage(message);
        }

        /// <summary>
        /// ��������ť
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Clean_Click(object sender, EventArgs e)
        {
            TBox_Console.Text = string.Empty;
        }

        #endregion

        #region ViewModelFunction


        // ���������
        public void AddConnection(string id, string deviceName, string message)
        {
            vm.AddConnection(id, deviceName, message);
        }

        // ��������״̬
        public void UpdateConnectionStatus(string id, string? deviceName, string? message)
        {
            vm.UpdateConnectionStatus(id, deviceName, message);
        }

        // ɾ������
        public void RemoveConnection(string id)
        {
            vm.RemoveConnection(id);
        }

        #endregion

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //await run.StopServerAsync();
        }
    }
}
