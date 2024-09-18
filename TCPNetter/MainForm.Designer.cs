namespace TCPNetter
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
            TBox_Result = new TextBox();
            panel1 = new Panel();
            Btn_Send = new Button();
            TBox_Input = new TextBox();
            label1 = new AntdUI.Label();
            CBox_MessageType = new ComboBox();
            Text_Target = new TextBox();
            label2 = new AntdUI.Label();
            label3 = new AntdUI.Label();
            CBox_Command = new ComboBox();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // TBox_Result
            // 
            TBox_Result.Font = new Font("Microsoft Sans Serif", 8.25F);
            TBox_Result.Location = new Point(12, 285);
            TBox_Result.Multiline = true;
            TBox_Result.Name = "TBox_Result";
            TBox_Result.Size = new Size(692, 632);
            TBox_Result.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(Btn_Send);
            panel1.Controls.Add(TBox_Input);
            panel1.Location = new Point(12, 105);
            panel1.Name = "panel1";
            panel1.Size = new Size(692, 43);
            panel1.TabIndex = 3;
            // 
            // Btn_Send
            // 
            Btn_Send.Dock = DockStyle.Right;
            Btn_Send.Location = new Point(505, 0);
            Btn_Send.Name = "Btn_Send";
            Btn_Send.Size = new Size(187, 43);
            Btn_Send.TabIndex = 1;
            Btn_Send.Text = "发送";
            Btn_Send.UseVisualStyleBackColor = true;
            Btn_Send.Click += Btn_Send_Click;
            // 
            // TBox_Input
            // 
            TBox_Input.Dock = DockStyle.Left;
            TBox_Input.Font = new Font("Microsoft YaHei UI", 14F, FontStyle.Regular, GraphicsUnit.Point, 134);
            TBox_Input.Location = new Point(0, 0);
            TBox_Input.Name = "TBox_Input";
            TBox_Input.Size = new Size(401, 43);
            TBox_Input.TabIndex = 0;
            // 
            // label1
            // 
            label1.Location = new Point(12, 0);
            label1.Name = "label1";
            label1.Size = new Size(112, 34);
            label1.TabIndex = 5;
            label1.Text = "命令类型";
            // 
            // CBox_MessageType
            // 
            CBox_MessageType.FormattingEnabled = true;
            CBox_MessageType.Items.AddRange(new object[] { "Echo", "Command", "Message", "NoCallback" });
            CBox_MessageType.Location = new Point(12, 40);
            CBox_MessageType.Name = "CBox_MessageType";
            CBox_MessageType.Size = new Size(242, 32);
            CBox_MessageType.TabIndex = 6;
            // 
            // Text_Target
            // 
            Text_Target.Location = new Point(517, 40);
            Text_Target.Name = "Text_Target";
            Text_Target.Size = new Size(187, 30);
            Text_Target.TabIndex = 8;
            // 
            // label2
            // 
            label2.Location = new Point(286, 0);
            label2.Name = "label2";
            label2.Size = new Size(112, 34);
            label2.TabIndex = 9;
            label2.Text = "命令内容";
            // 
            // label3
            // 
            label3.Location = new Point(517, 0);
            label3.Name = "label3";
            label3.Size = new Size(126, 34);
            label3.TabIndex = 10;
            label3.Text = "目标id";
            // 
            // CBox_Command
            // 
            CBox_Command.FormattingEnabled = true;
            CBox_Command.Items.AddRange(new object[] { "Broadcast", "SendMessageById", "SendMessageByName", "GetAll", "GetMyHistory", "GetHistory" });
            CBox_Command.Location = new Point(286, 40);
            CBox_Command.Name = "CBox_Command";
            CBox_Command.Size = new Size(210, 32);
            CBox_Command.TabIndex = 11;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(716, 929);
            Controls.Add(CBox_Command);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(Text_Target);
            Controls.Add(CBox_MessageType);
            Controls.Add(label1);
            Controls.Add(TBox_Result);
            Controls.Add(panel1);
            Name = "MainForm";
            Text = "TCP测试平台";
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TBox_Result;
        private Panel panel1;
        private Button Btn_Send;
        private TextBox TBox_Input;
        private AntdUI.Label label1;
        private ComboBox CBox_MessageType;
        private TextBox Text_Target;
        private AntdUI.Label label2;
        private AntdUI.Label label3;
        private ComboBox CBox_Command;
    }
}
