namespace KeyManagmentClient
{
    partial class ClientForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.butConnect = new System.Windows.Forms.Button();
            this.panAuth = new System.Windows.Forms.Panel();
            this.LogConsole = new System.Windows.Forms.TextBox();
            this.labLogin = new System.Windows.Forms.Label();
            this.Login = new System.Windows.Forms.TextBox();
            this.labPort = new System.Windows.Forms.Label();
            this.Port = new System.Windows.Forms.TextBox();
            this.labHost = new System.Windows.Forms.Label();
            this.Host = new System.Windows.Forms.TextBox();
            this.panWork = new System.Windows.Forms.Panel();
            this.labList = new System.Windows.Forms.Label();
            this.listUsers = new System.Windows.Forms.ListBox();
            this.tabWork = new System.Windows.Forms.TabControl();
            this.tabKeying = new System.Windows.Forms.TabPage();
            this.labKey = new System.Windows.Forms.TextBox();
            this.labPG = new System.Windows.Forms.TextBox();
            this.tabMessenger = new System.Windows.Forms.TabPage();
            this.SendMessage = new System.Windows.Forms.Button();
            this.NewMessage = new System.Windows.Forms.TextBox();
            this.MessageHistory = new System.Windows.Forms.TextBox();
            this.labTimestamp = new System.Windows.Forms.Label();
            this.Redraw = new System.Windows.Forms.Timer(this.components);
            this.panAuth.SuspendLayout();
            this.panWork.SuspendLayout();
            this.tabWork.SuspendLayout();
            this.tabKeying.SuspendLayout();
            this.tabMessenger.SuspendLayout();
            this.SuspendLayout();
            // 
            // butConnect
            // 
            this.butConnect.Location = new System.Drawing.Point(357, 21);
            this.butConnect.Name = "butConnect";
            this.butConnect.Size = new System.Drawing.Size(135, 40);
            this.butConnect.TabIndex = 0;
            this.butConnect.Text = "Подключиться";
            this.butConnect.UseVisualStyleBackColor = true;
            this.butConnect.Click += new System.EventHandler(this.butConnect_Click);
            // 
            // panAuth
            // 
            this.panAuth.Controls.Add(this.LogConsole);
            this.panAuth.Controls.Add(this.labLogin);
            this.panAuth.Controls.Add(this.Login);
            this.panAuth.Controls.Add(this.labPort);
            this.panAuth.Controls.Add(this.Port);
            this.panAuth.Controls.Add(this.labHost);
            this.panAuth.Controls.Add(this.Host);
            this.panAuth.Controls.Add(this.butConnect);
            this.panAuth.Location = new System.Drawing.Point(12, 16);
            this.panAuth.Name = "panAuth";
            this.panAuth.Size = new System.Drawing.Size(769, 419);
            this.panAuth.TabIndex = 1;
            // 
            // LogConsole
            // 
            this.LogConsole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LogConsole.Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.LogConsole.Location = new System.Drawing.Point(5, 84);
            this.LogConsole.Multiline = true;
            this.LogConsole.Name = "LogConsole";
            this.LogConsole.ReadOnly = true;
            this.LogConsole.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.LogConsole.Size = new System.Drawing.Size(758, 331);
            this.LogConsole.TabIndex = 7;
            // 
            // labLogin
            // 
            this.labLogin.AutoSize = true;
            this.labLogin.Location = new System.Drawing.Point(5, 61);
            this.labLogin.Name = "labLogin";
            this.labLogin.Size = new System.Drawing.Size(38, 13);
            this.labLogin.TabIndex = 6;
            this.labLogin.Text = "Логин";
            // 
            // Login
            // 
            this.Login.Location = new System.Drawing.Point(101, 58);
            this.Login.Name = "Login";
            this.Login.Size = new System.Drawing.Size(250, 20);
            this.Login.TabIndex = 5;
            // 
            // labPort
            // 
            this.labPort.AutoSize = true;
            this.labPort.Location = new System.Drawing.Point(5, 35);
            this.labPort.Name = "labPort";
            this.labPort.Size = new System.Drawing.Size(32, 13);
            this.labPort.TabIndex = 4;
            this.labPort.Text = "Порт";
            // 
            // Port
            // 
            this.Port.Location = new System.Drawing.Point(101, 32);
            this.Port.Name = "Port";
            this.Port.Size = new System.Drawing.Size(250, 20);
            this.Port.TabIndex = 3;
            this.Port.Text = "10200";
            // 
            // labHost
            // 
            this.labHost.AutoSize = true;
            this.labHost.Location = new System.Drawing.Point(5, 9);
            this.labHost.Name = "labHost";
            this.labHost.Size = new System.Drawing.Size(83, 13);
            this.labHost.TabIndex = 2;
            this.labHost.Text = "Адрес сервера";
            // 
            // Host
            // 
            this.Host.Location = new System.Drawing.Point(101, 6);
            this.Host.Name = "Host";
            this.Host.Size = new System.Drawing.Size(250, 20);
            this.Host.TabIndex = 1;
            this.Host.Text = "192.168.1.71";
            // 
            // panWork
            // 
            this.panWork.Controls.Add(this.labList);
            this.panWork.Controls.Add(this.listUsers);
            this.panWork.Controls.Add(this.tabWork);
            this.panWork.Controls.Add(this.labTimestamp);
            this.panWork.Location = new System.Drawing.Point(13, 13);
            this.panWork.Name = "panWork";
            this.panWork.Size = new System.Drawing.Size(775, 425);
            this.panWork.TabIndex = 2;
            this.panWork.Visible = false;
            // 
            // labList
            // 
            this.labList.AutoSize = true;
            this.labList.Location = new System.Drawing.Point(4, 46);
            this.labList.Name = "labList";
            this.labList.Size = new System.Drawing.Size(100, 13);
            this.labList.TabIndex = 6;
            this.labList.Text = "Список адресатов";
            // 
            // listUsers
            // 
            this.listUsers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.listUsers.Font = new System.Drawing.Font("Times New Roman", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.listUsers.FormattingEnabled = true;
            this.listUsers.ItemHeight = 31;
            this.listUsers.Location = new System.Drawing.Point(4, 68);
            this.listUsers.Name = "listUsers";
            this.listUsers.Size = new System.Drawing.Size(225, 314);
            this.listUsers.TabIndex = 5;
            this.listUsers.SelectedIndexChanged += new System.EventHandler(this.listUsers_SelectedIndexChanged);
            // 
            // tabWork
            // 
            this.tabWork.Controls.Add(this.tabKeying);
            this.tabWork.Controls.Add(this.tabMessenger);
            this.tabWork.Location = new System.Drawing.Point(231, 46);
            this.tabWork.Name = "tabWork";
            this.tabWork.SelectedIndex = 0;
            this.tabWork.Size = new System.Drawing.Size(541, 376);
            this.tabWork.TabIndex = 4;
            // 
            // tabKeying
            // 
            this.tabKeying.Controls.Add(this.labKey);
            this.tabKeying.Controls.Add(this.labPG);
            this.tabKeying.Location = new System.Drawing.Point(4, 22);
            this.tabKeying.Name = "tabKeying";
            this.tabKeying.Padding = new System.Windows.Forms.Padding(3);
            this.tabKeying.Size = new System.Drawing.Size(533, 350);
            this.tabKeying.TabIndex = 0;
            this.tabKeying.Text = "Ключи шифрования";
            this.tabKeying.UseVisualStyleBackColor = true;
            // 
            // labKey
            // 
            this.labKey.BackColor = System.Drawing.SystemColors.Window;
            this.labKey.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labKey.Location = new System.Drawing.Point(7, 162);
            this.labKey.Multiline = true;
            this.labKey.Name = "labKey";
            this.labKey.ReadOnly = true;
            this.labKey.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.labKey.Size = new System.Drawing.Size(520, 180);
            this.labKey.TabIndex = 1;
            // 
            // labPG
            // 
            this.labPG.BackColor = System.Drawing.SystemColors.Window;
            this.labPG.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labPG.Location = new System.Drawing.Point(6, 6);
            this.labPG.Multiline = true;
            this.labPG.Name = "labPG";
            this.labPG.ReadOnly = true;
            this.labPG.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.labPG.Size = new System.Drawing.Size(521, 150);
            this.labPG.TabIndex = 0;
            // 
            // tabMessenger
            // 
            this.tabMessenger.Controls.Add(this.SendMessage);
            this.tabMessenger.Controls.Add(this.NewMessage);
            this.tabMessenger.Controls.Add(this.MessageHistory);
            this.tabMessenger.Location = new System.Drawing.Point(4, 22);
            this.tabMessenger.Name = "tabMessenger";
            this.tabMessenger.Padding = new System.Windows.Forms.Padding(3);
            this.tabMessenger.Size = new System.Drawing.Size(533, 350);
            this.tabMessenger.TabIndex = 1;
            this.tabMessenger.Text = "Обмен сообщениями";
            this.tabMessenger.UseVisualStyleBackColor = true;
            // 
            // SendMessage
            // 
            this.SendMessage.Location = new System.Drawing.Point(406, 309);
            this.SendMessage.Name = "SendMessage";
            this.SendMessage.Size = new System.Drawing.Size(121, 32);
            this.SendMessage.TabIndex = 2;
            this.SendMessage.Text = "Отправить";
            this.SendMessage.UseVisualStyleBackColor = true;
            this.SendMessage.Click += new System.EventHandler(this.SendMessage_Click);
            // 
            // NewMessage
            // 
            this.NewMessage.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.NewMessage.Location = new System.Drawing.Point(4, 309);
            this.NewMessage.Name = "NewMessage";
            this.NewMessage.Size = new System.Drawing.Size(395, 32);
            this.NewMessage.TabIndex = 1;
            // 
            // MessageHistory
            // 
            this.MessageHistory.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MessageHistory.Location = new System.Drawing.Point(4, 7);
            this.MessageHistory.Multiline = true;
            this.MessageHistory.Name = "MessageHistory";
            this.MessageHistory.Size = new System.Drawing.Size(523, 296);
            this.MessageHistory.TabIndex = 0;
            // 
            // labTimestamp
            // 
            this.labTimestamp.AutoSize = true;
            this.labTimestamp.Location = new System.Drawing.Point(10, 25);
            this.labTimestamp.Name = "labTimestamp";
            this.labTimestamp.Size = new System.Drawing.Size(94, 13);
            this.labTimestamp.TabIndex = 0;
            this.labTimestamp.Text = "Время сервера =";
            // 
            // Redraw
            // 
            this.Redraw.Enabled = true;
            this.Redraw.Tick += new System.EventHandler(this.Redraw_Tick);
            // 
            // ClientForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.panAuth);
            this.Controls.Add(this.panWork);
            this.Name = "ClientForm";
            this.Text = "Клиент";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClientForm_FormClosing);
            this.panAuth.ResumeLayout(false);
            this.panAuth.PerformLayout();
            this.panWork.ResumeLayout(false);
            this.panWork.PerformLayout();
            this.tabWork.ResumeLayout(false);
            this.tabKeying.ResumeLayout(false);
            this.tabKeying.PerformLayout();
            this.tabMessenger.ResumeLayout(false);
            this.tabMessenger.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button butConnect;
        private System.Windows.Forms.Panel panAuth;
        private System.Windows.Forms.Label labLogin;
        private System.Windows.Forms.TextBox Login;
        private System.Windows.Forms.Label labPort;
        private System.Windows.Forms.TextBox Port;
        private System.Windows.Forms.Label labHost;
        private System.Windows.Forms.TextBox Host;
        private System.Windows.Forms.Panel panWork;
        private System.Windows.Forms.Label labTimestamp;
        private System.Windows.Forms.Timer Redraw;
        private System.Windows.Forms.TabControl tabWork;
        private System.Windows.Forms.TabPage tabKeying;
        private System.Windows.Forms.TabPage tabMessenger;
        private System.Windows.Forms.ListBox listUsers;
        private System.Windows.Forms.Label labList;
        private System.Windows.Forms.TextBox labPG;
        private System.Windows.Forms.TextBox labKey;
        private System.Windows.Forms.TextBox MessageHistory;
        private System.Windows.Forms.TextBox NewMessage;
        private System.Windows.Forms.Button SendMessage;
        private System.Windows.Forms.TextBox LogConsole;
    }
}

