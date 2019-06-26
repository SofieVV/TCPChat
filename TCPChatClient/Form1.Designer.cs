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
            this.ClientListBox = new System.Windows.Forms.ListBox();
            this.SendButton = new System.Windows.Forms.Button();
            this.MessageTextBox = new System.Windows.Forms.TextBox();
            this.LoginButton = new System.Windows.Forms.Button();
            this.clientNameTextBox = new System.Windows.Forms.TextBox();
            this.ClientNameLable = new System.Windows.Forms.Label();
            this.ChatTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // ClientListBox
            // 
            this.ClientListBox.FormattingEnabled = true;
            this.ClientListBox.ItemHeight = 16;
            this.ClientListBox.Location = new System.Drawing.Point(556, 49);
            this.ClientListBox.Name = "ClientListBox";
            this.ClientListBox.Size = new System.Drawing.Size(148, 436);
            this.ClientListBox.TabIndex = 1;
            // 
            // SendButton
            // 
            this.SendButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.SendButton.Location = new System.Drawing.Point(556, 508);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(147, 45);
            this.SendButton.TabIndex = 3;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // MessageTextBox
            // 
            this.MessageTextBox.Location = new System.Drawing.Point(13, 508);
            this.MessageTextBox.Multiline = true;
            this.MessageTextBox.Name = "MessageTextBox";
            this.MessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.MessageTextBox.Size = new System.Drawing.Size(537, 45);
            this.MessageTextBox.TabIndex = 2;
            // 
            // LoginButton
            // 
            this.LoginButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LoginButton.Location = new System.Drawing.Point(557, 5);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(147, 38);
            this.LoginButton.TabIndex = 4;
            this.LoginButton.Text = "Login";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // clientNameTextBox
            // 
            this.clientNameTextBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.clientNameTextBox.Location = new System.Drawing.Point(73, 12);
            this.clientNameTextBox.Name = "clientNameTextBox";
            this.clientNameTextBox.Size = new System.Drawing.Size(477, 26);
            this.clientNameTextBox.TabIndex = 5;
            // 
            // ClientNameLable
            // 
            this.ClientNameLable.AutoSize = true;
            this.ClientNameLable.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ClientNameLable.Location = new System.Drawing.Point(9, 14);
            this.ClientNameLable.Name = "ClientNameLable";
            this.ClientNameLable.Size = new System.Drawing.Size(58, 20);
            this.ClientNameLable.TabIndex = 6;
            this.ClientNameLable.Text = "Name:";
            // 
            // ChatTextBox
            // 
            this.ChatTextBox.Location = new System.Drawing.Point(13, 49);
            this.ChatTextBox.Name = "ChatTextBox";
            this.ChatTextBox.ReadOnly = true;
            this.ChatTextBox.Size = new System.Drawing.Size(537, 436);
            this.ChatTextBox.TabIndex = 7;
            this.ChatTextBox.Text = "";
            // 
            // TCPClient
            // 
            this.AcceptButton = this.SendButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(715, 571);
            this.Controls.Add(this.ChatTextBox);
            this.Controls.Add(this.ClientNameLable);
            this.Controls.Add(this.clientNameTextBox);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.MessageTextBox);
            this.Controls.Add(this.ClientListBox);
            this.Name = "TCPClient";
            this.Text = "TCPClient";
            this.Load += new System.EventHandler(this.TCPClient_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox ClientListBox;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.TextBox MessageTextBox;
        private System.Windows.Forms.Button LoginButton;
        private System.Windows.Forms.TextBox clientNameTextBox;
        private System.Windows.Forms.Label ClientNameLable;
        private System.Windows.Forms.RichTextBox ChatTextBox;
    }
}

