using TCPNetterServerGUI.GUI.Model;
using TCPNetterServerGUI.GUI.ViewModel;
using TCPNetterServerGUI.Server;

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
            run.StartServerAsync(8188);
        }

        private void Bind()
        {
            vm = new UIViewModel();

            Table_Device.Columns = new ()
            {
                new AntdUI.Column(nameof(UIModel.Id), "ID"),
                new AntdUI.Column(nameof(UIModel.DeviceName), "设备名称"),
                new AntdUI.Column(nameof(UIModel.Message), "消息"),
            };

            Table_Device.Binding(vm.UIModels);
        }

        #region Event

        private void Btn_Send_Click(object sender, EventArgs e)
        {

        }

        private void Btn_Clean_Click(object sender, EventArgs e)
        {

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
    }
}
