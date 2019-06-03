namespace Core.SvcGui
{
    partial class Form1
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtDbCfg = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cbServerMode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbEnvironment = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpCore = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtCoreSvrLog = new System.Windows.Forms.TextBox();
            this.tpAlerts = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtAlertSvrLog = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnAlertSvrStop = new System.Windows.Forms.Button();
            this.btnAlertSvrStart = new System.Windows.Forms.Button();
            this.tpDebug = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtDebugLog = new System.Windows.Forms.TextBox();
            this.btnDebugLoadAlertRules = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpCore.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tpAlerts.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tpDebug.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtDbCfg);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.cbServerMode);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.cbEnvironment);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(773, 67);
            this.panel1.TabIndex = 1;
            // 
            // txtDbCfg
            // 
            this.txtDbCfg.Location = new System.Drawing.Point(231, 34);
            this.txtDbCfg.Name = "txtDbCfg";
            this.txtDbCfg.Size = new System.Drawing.Size(166, 20);
            this.txtDbCfg.TabIndex = 5;
            this.txtDbCfg.Text = "localhost\\SQLEXPRESS";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(170, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Database:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(225, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(194, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "(note: server mode requires a database)";
            // 
            // cbServerMode
            // 
            this.cbServerMode.FormattingEnabled = true;
            this.cbServerMode.Location = new System.Drawing.Point(81, 34);
            this.cbServerMode.Name = "cbServerMode";
            this.cbServerMode.Size = new System.Drawing.Size(83, 21);
            this.cbServerMode.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 37);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Mode:";
            // 
            // cbEnvironment
            // 
            this.cbEnvironment.FormattingEnabled = true;
            this.cbEnvironment.Location = new System.Drawing.Point(81, 5);
            this.cbEnvironment.Name = "cbEnvironment";
            this.cbEnvironment.Size = new System.Drawing.Size(128, 21);
            this.cbEnvironment.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Environment:";
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(438, 32);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 1;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStopClick);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(438, 3);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 0;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpCore);
            this.tabControl1.Controls.Add(this.tpAlerts);
            this.tabControl1.Controls.Add(this.tpDebug);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(787, 536);
            this.tabControl1.TabIndex = 3;
            // 
            // tpCore
            // 
            this.tpCore.Controls.Add(this.groupBox1);
            this.tpCore.Controls.Add(this.panel1);
            this.tpCore.Location = new System.Drawing.Point(4, 22);
            this.tpCore.Name = "tpCore";
            this.tpCore.Padding = new System.Windows.Forms.Padding(3);
            this.tpCore.Size = new System.Drawing.Size(779, 510);
            this.tpCore.TabIndex = 0;
            this.tpCore.Text = "Core Server";
            this.tpCore.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtCoreSvrLog);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 70);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(773, 437);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log";
            // 
            // txtCoreSvrLog
            // 
            this.txtCoreSvrLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCoreSvrLog.Location = new System.Drawing.Point(3, 16);
            this.txtCoreSvrLog.Multiline = true;
            this.txtCoreSvrLog.Name = "txtCoreSvrLog";
            this.txtCoreSvrLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtCoreSvrLog.Size = new System.Drawing.Size(767, 418);
            this.txtCoreSvrLog.TabIndex = 0;
            // 
            // tpAlerts
            // 
            this.tpAlerts.Controls.Add(this.groupBox2);
            this.tpAlerts.Controls.Add(this.panel2);
            this.tpAlerts.Location = new System.Drawing.Point(4, 22);
            this.tpAlerts.Name = "tpAlerts";
            this.tpAlerts.Padding = new System.Windows.Forms.Padding(3);
            this.tpAlerts.Size = new System.Drawing.Size(779, 510);
            this.tpAlerts.TabIndex = 1;
            this.tpAlerts.Text = "Alert Server";
            this.tpAlerts.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtAlertSvrLog);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox2.Location = new System.Drawing.Point(3, 70);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(773, 437);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Log";
            // 
            // txtAlertSvrLog
            // 
            this.txtAlertSvrLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtAlertSvrLog.Location = new System.Drawing.Point(3, 16);
            this.txtAlertSvrLog.Multiline = true;
            this.txtAlertSvrLog.Name = "txtAlertSvrLog";
            this.txtAlertSvrLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtAlertSvrLog.Size = new System.Drawing.Size(767, 418);
            this.txtAlertSvrLog.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnAlertSvrStop);
            this.panel2.Controls.Add(this.btnAlertSvrStart);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(773, 67);
            this.panel2.TabIndex = 0;
            // 
            // btnAlertSvrStop
            // 
            this.btnAlertSvrStop.Location = new System.Drawing.Point(5, 32);
            this.btnAlertSvrStop.Name = "btnAlertSvrStop";
            this.btnAlertSvrStop.Size = new System.Drawing.Size(75, 23);
            this.btnAlertSvrStop.TabIndex = 1;
            this.btnAlertSvrStop.Text = "Stop";
            this.btnAlertSvrStop.UseVisualStyleBackColor = true;
            this.btnAlertSvrStop.Click += new System.EventHandler(this.BtnAlertSvrStopClick);
            // 
            // btnAlertSvrStart
            // 
            this.btnAlertSvrStart.Location = new System.Drawing.Point(5, 3);
            this.btnAlertSvrStart.Name = "btnAlertSvrStart";
            this.btnAlertSvrStart.Size = new System.Drawing.Size(75, 23);
            this.btnAlertSvrStart.TabIndex = 0;
            this.btnAlertSvrStart.Text = "Start";
            this.btnAlertSvrStart.UseVisualStyleBackColor = true;
            this.btnAlertSvrStart.Click += new System.EventHandler(this.BtnAlertSvrStartClick);
            // 
            // tpDebug
            // 
            this.tpDebug.Controls.Add(this.groupBox3);
            this.tpDebug.Controls.Add(this.panel3);
            this.tpDebug.Location = new System.Drawing.Point(4, 22);
            this.tpDebug.Name = "tpDebug";
            this.tpDebug.Padding = new System.Windows.Forms.Padding(3);
            this.tpDebug.Size = new System.Drawing.Size(779, 510);
            this.tpDebug.TabIndex = 2;
            this.tpDebug.Text = "Debug";
            this.tpDebug.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btnDebugLoadAlertRules);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(773, 162);
            this.panel3.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtDebugLog);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox3.Location = new System.Drawing.Point(3, 165);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(773, 342);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Log";
            // 
            // txtDebugLog
            // 
            this.txtDebugLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtDebugLog.Location = new System.Drawing.Point(3, 16);
            this.txtDebugLog.Multiline = true;
            this.txtDebugLog.Name = "txtDebugLog";
            this.txtDebugLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDebugLog.Size = new System.Drawing.Size(767, 323);
            this.txtDebugLog.TabIndex = 0;
            // 
            // btnDebugLoadAlertRules
            // 
            this.btnDebugLoadAlertRules.Location = new System.Drawing.Point(5, 3);
            this.btnDebugLoadAlertRules.Name = "btnDebugLoadAlertRules";
            this.btnDebugLoadAlertRules.Size = new System.Drawing.Size(150, 23);
            this.btnDebugLoadAlertRules.TabIndex = 0;
            this.btnDebugLoadAlertRules.Text = "Load Alert Rules";
            this.btnDebugLoadAlertRules.UseVisualStyleBackColor = true;
            this.btnDebugLoadAlertRules.Click += new System.EventHandler(this.BtnDebugLoadAlertRulesClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(787, 536);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpCore.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tpAlerts.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.tpDebug.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.ComboBox cbEnvironment;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDbCfg;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox cbServerMode;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpCore;
        private System.Windows.Forms.TabPage tpAlerts;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtCoreSvrLog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox txtAlertSvrLog;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnAlertSvrStop;
        private System.Windows.Forms.Button btnAlertSvrStart;
        private System.Windows.Forms.TabPage tpDebug;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtDebugLog;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnDebugLoadAlertRules;
    }
}

