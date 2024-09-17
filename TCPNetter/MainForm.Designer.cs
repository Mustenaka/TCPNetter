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
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // TBox_Result
            // 
            TBox_Result.Font = new Font("Microsoft Sans Serif", 8.25F);
            TBox_Result.Location = new Point(12, 71);
            TBox_Result.Multiline = true;
            TBox_Result.Name = "TBox_Result";
            TBox_Result.Size = new Size(533, 687);
            TBox_Result.TabIndex = 4;
            // 
            // panel1
            // 
            panel1.Controls.Add(Btn_Send);
            panel1.Controls.Add(TBox_Input);
            panel1.Location = new Point(12, 12);
            panel1.Name = "panel1";
            panel1.Size = new Size(533, 43);
            panel1.TabIndex = 3;
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
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(567, 770);
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
    }
}
