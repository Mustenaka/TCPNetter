using TCPNetterServerGUI.GUI.Model;
using TCPNetterServerGUI.GUI.ViewModel;
using TCPNetterServerGUI.Server;
using TCPNetterServerGUI.Server.Handler;

namespace TCPNetterServerGUI
{
    public partial class MainForm : Form
    {
        private RunSrv run;     // 服务器启动逻辑
        private UIViewModel vm; // UI数据模型

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
                new AntdUI.Column(nameof(UIModel.Id), "通信ID"),
                new AntdUI.Column(nameof(UIModel.DeviceName), "设备名称"),
                new AntdUI.Column(nameof(UIModel.Message), "消息"),
            };

            Table_Device.Binding(vm.UIModels);
        }

        #region Event

        /// <summary>
        /// 主动给Netter发送消息 | 案例，可以自主修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Send_Click(object sender, EventArgs e)
        {
            var message = Inp_SendMessage.Text;

            // 一对一的发送全部消息
            foreach (var model in vm.UIModels)
            {
                NetterServerHandler.SendMessageToClient(model.Id, message, "");

                var sendMessage = $@"发送Message类型数据给{model.Id},内容:{message}" + "\u000D\u000A";   // 换行符
                TBox_Console.Text += sendMessage;
            }

            // 直接用广播功能发送消息
            //NetterServerHandler.BroadcastMessage(message);
        }

        /// <summary>
        /// 清空输出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Clean_Click(object sender, EventArgs e)
        {
            TBox_Console.Text = string.Empty;
        }

        #endregion

        #region ViewModelFunction


        // 添加新连接
        public void AddConnection(string id, string deviceName, string message)
        {
            vm.AddConnection(id, deviceName, message);
        }

        // 更新连接状态
        public void UpdateConnectionStatus(string id, string? deviceName, string? message)
        {
            vm.UpdateConnectionStatus(id, deviceName, message);
        }

        // 删除连接
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
