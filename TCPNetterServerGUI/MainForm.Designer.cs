namespace TCPNetterServerGUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            splitContainer1 = new SplitContainer();
            Table_Device = new AntdUI.Table();
            SPanel_Controller = new AntdUI.StackPanel();
            TBox_Console = new TextBox();
            Btn_Clean = new AntdUI.Button();
            label2 = new AntdUI.Label();
            panel1 = new AntdUI.Panel();
            Btn_Send = new AntdUI.Button();
            Inp_SendMessage = new AntdUI.Input();
            label1 = new AntdUI.Label();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SPanel_Controller.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(Table_Device);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(SPanel_Controller);
            splitContainer1.Size = new Size(892, 1042);
            splitContainer1.SplitterDistance = 400;
            splitContainer1.TabIndex = 0;
            // 
            // Table_Device
            // 
            Table_Device.Dock = DockStyle.Fill;
            Table_Device.Location = new Point(0, 0);
            Table_Device.Name = "Table_Device";
            Table_Device.Size = new Size(400, 1042);
            Table_Device.TabIndex = 0;
            Table_Device.Text = "table1";
            // 
            // SPanel_Controller
            // 
            SPanel_Controller.Controls.Add(TBox_Console);
            SPanel_Controller.Controls.Add(Btn_Clean);
            SPanel_Controller.Controls.Add(label2);
            SPanel_Controller.Controls.Add(panel1);
            SPanel_Controller.Controls.Add(label1);
            SPanel_Controller.Dock = DockStyle.Fill;
            SPanel_Controller.Location = new Point(0, 0);
            SPanel_Controller.Name = "SPanel_Controller";
            SPanel_Controller.Size = new Size(488, 1042);
            SPanel_Controller.TabIndex = 0;
            SPanel_Controller.Text = "stackPanel1";
            SPanel_Controller.Vertical = true;
            // 
            // TBox_Console
            // 
            TBox_Console.Dock = DockStyle.Fill;
            TBox_Console.Location = new Point(3, 313);
            TBox_Console.Multiline = true;
            TBox_Console.Name = "TBox_Console";
            TBox_Console.Size = new Size(482, 726);
            TBox_Console.TabIndex = 4;
            // 
            // Btn_Clean
            // 
            Btn_Clean.Location = new Point(3, 249);
            Btn_Clean.Name = "Btn_Clean";
            Btn_Clean.Size = new Size(482, 58);
            Btn_Clean.TabIndex = 3;
            Btn_Clean.Text = "清除";
            Btn_Clean.Click += Btn_Clean_Click;
            // 
            // label2
            // 
            label2.Location = new Point(3, 161);
            label2.Name = "label2";
            label2.Size = new Size(482, 82);
            label2.TabIndex = 2;
            label2.Text = "信息";
            // 
            // panel1
            // 
            panel1.Controls.Add(Btn_Send);
            panel1.Controls.Add(Inp_SendMessage);
            panel1.Location = new Point(3, 76);
            panel1.Name = "panel1";
            panel1.Size = new Size(482, 79);
            panel1.TabIndex = 1;
            panel1.Text = "panel1";
            // 
            // Btn_Send
            // 
            Btn_Send.Dock = DockStyle.Right;
            Btn_Send.Location = new Point(275, 0);
            Btn_Send.Name = "Btn_Send";
            Btn_Send.Size = new Size(207, 79);
            Btn_Send.TabIndex = 1;
            Btn_Send.Text = "发送";
            Btn_Send.Click += Btn_Send_Click;
            // 
            // Inp_SendMessage
            // 
            Inp_SendMessage.Dock = DockStyle.Left;
            Inp_SendMessage.Location = new Point(0, 0);
            Inp_SendMessage.Name = "Inp_SendMessage";
            Inp_SendMessage.Size = new Size(243, 79);
            Inp_SendMessage.TabIndex = 0;
            Inp_SendMessage.Text = "信息内容";
            // 
            // label1
            // 
            label1.Dock = DockStyle.Top;
            label1.Location = new Point(3, 3);
            label1.Name = "label1";
            label1.Size = new Size(482, 67);
            label1.TabIndex = 0;
            label1.Text = "操作控制";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(892, 1042);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            Text = "TCPNetter服务端口";
            FormClosing += MainForm_FormClosing;
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            SPanel_Controller.ResumeLayout(false);
            SPanel_Controller.PerformLayout();
            panel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private AntdUI.Table Table_Device;
        private AntdUI.StackPanel SPanel_Controller;
        private AntdUI.Label label1;
        private AntdUI.Panel panel1;
        private AntdUI.Button Btn_Send;
        private AntdUI.Input Inp_SendMessage;
        private AntdUI.Label label2;
        private AntdUI.Button Btn_Clean;
        private TextBox TBox_Console;
    }
}
