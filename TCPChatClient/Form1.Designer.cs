namespace TCPChatClient
{
    partial class TCPClient
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ChatTextBox = new System.Windows.Forms.TextBox();
            this.ClientListBox = new System.Windows.Forms.ListBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ChatTextBox
            // 
            this.ChatTextBox.Location = new System.Drawing.Point(12, 12);
            this.ChatTextBox.Multiline = true;
            this.ChatTextBox.Name = "ChatTextBox";
            this.ChatTextBox.ReadOnly = true;
            this.ChatTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.ChatTextBox.Size = new System.Drawing.Size(537, 436);
            this.ChatTextBox.TabIndex = 0;
            // 
            // ClientListBox
            // 
            this.ClientListBox.FormattingEnabled = true;
            this.ClientListBox.ItemHeight = 16;
            this.ClientListBox.Location = new System.Drawing.Point(555, 12);
            this.ClientListBox.Name = "ClientListBox";
            this.ClientListBox.Size = new System.Drawing.Size(148, 436);
            this.ClientListBox.TabIndex = 1;
            // 
            // SendButton
            // 
            this.SendButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SendButton.Location = new System.Drawing.Point(556, 455);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(147, 44);
            this.SendButton.TabIndex = 3;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.Location = new System.Drawing.Point(12, 454);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.MessageTextBox.Size = new System.Drawing.Size(537, 45);
            this.MessageTextBox.TabIndex = 2;
            // 
            // ConnectButton
            // 
            this.ConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ConnectButton.Location = new System.Drawing.Point(12, 510);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(691, 43);
            this.ConnectButton.TabIndex = 4;
            this.ConnectButton.Text = "Connect ";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // TCPClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 565);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.MessageTextBox);
            this.Controls.Add(this.ClientListBox);
            this.Controls.Add(this.ChatTextBox);
            this.Name = "TCPClient";
            this.Text = "TCPClient";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ChatTextBox;
        private System.Windows.Forms.ListBox ClientListBox;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.Button ConnectButton;
    }
}

