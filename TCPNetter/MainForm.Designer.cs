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
            splitContainer1 = new SplitContainer();
            DGV_Client = new DataGridView();
            TBox_Result = new TextBox();
            panel1 = new Panel();
            Btn_Send = new Button();
            TBox_Input = new TextBox();
            TBox_All = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)DGV_Client).BeginInit();
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
            splitContainer1.Panel1.Controls.Add(DGV_Client);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(TBox_Result);
            splitContainer1.Panel2.Controls.Add(panel1);
            splitContainer1.Panel2.Controls.Add(TBox_All);
            splitContainer1.Size = new Size(1037, 1031);
            splitContainer1.SplitterDistance = 471;
            splitContainer1.TabIndex = 0;
            // 
            // DGV_Client
            // 
            DGV_Client.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            DGV_Client.Dock = DockStyle.Fill;
            DGV_Client.Location = new Point(0, 0);
            DGV_Client.Name = "DGV_Client";
            DGV_Client.RowHeadersWidth = 62;
            DGV_Client.Size = new Size(471, 1031);
            DGV_Client.TabIndex = 0;
            // 
            // TBox_Result
            // 
            TBox_Result.Font = new Font("Microsoft Sans Serif", 8.25F);
            TBox_Result.Location = new Point(13, 743);
            TBox_Result.Multiline = true;
            TBox_Result.Name = "TBox_Result";
            TBox_Result.Size = new Size(537, 285);
            TBox_Result.TabIndex = 2;
            // 
            // panel1
            // 
            panel1.Controls.Add(Btn_Send);
            panel1.Controls.Add(TBox_Input);
            panel1.Location = new Point(13, 684);
            panel1.Name = "panel1";
            panel1.Size = new Size(533, 43);
            panel1.TabIndex = 1;
            // 
            // Btn_Send
            // 
            Btn_Send.Dock = DockStyle.Right;
            Btn_Send.Location = new Point(346, 0);
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
            TBox_Input.Size = new Size(242, 43);
            TBox_Input.TabIndex = 0;
            // 
            // TBox_All
            // 
            TBox_All.Location = new Point(13, 12);
            TBox_All.Multiline = true;
            TBox_All.Name = "TBox_All";
            TBox_All.Size = new Size(533, 656);
            TBox_All.TabIndex = 0;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1037, 1031);
            Controls.Add(splitContainer1);
            Name = "MainForm";
            Text = "TCP测试平台";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)DGV_Client).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitContainer1;
        private DataGridView DGV_Client;
        private TextBox TBox_Result;
        private Panel panel1;
        private Button Btn_Send;
        private TextBox TBox_Input;
        private TextBox TBox_All;
    }
}
