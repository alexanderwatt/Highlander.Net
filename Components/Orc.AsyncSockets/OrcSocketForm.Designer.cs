/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Highlander.Orc.AsyncSockets
{
    partial class OrcSocketForm
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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtRecvQueue = new System.Windows.Forms.TextBox();
            this.rbQueue = new System.Windows.Forms.RadioButton();
            this.rbPort = new System.Windows.Forms.RadioButton();
            this.txtServerRate = new System.Windows.Forms.TextBox();
            this.txtServerVolume = new System.Windows.Forms.TextBox();
            this.txtServerDuration = new System.Windows.Forms.TextBox();
            this.txtServerConns = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.btnServerStop = new System.Windows.Forms.Button();
            this.btnServerStart = new System.Windows.Forms.Button();
            this.txtListenPort = new System.Windows.Forms.TextBox();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.txtSendQueue = new System.Windows.Forms.TextBox();
            this.rbClientQueue = new System.Windows.Forms.RadioButton();
            this.rbClientPort = new System.Windows.Forms.RadioButton();
            this.txtClientPort = new System.Windows.Forms.TextBox();
            this.txtClientHost = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtMsgCount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtMsgSize = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.txtRequestMsg = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnClientConnect = new System.Windows.Forms.Button();
            this.btnClientDisconnect = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(526, 397);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.groupBox1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(518, 371);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Server";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtRecvQueue);
            this.groupBox1.Controls.Add(this.rbQueue);
            this.groupBox1.Controls.Add(this.rbPort);
            this.groupBox1.Controls.Add(this.txtServerRate);
            this.groupBox1.Controls.Add(this.txtServerVolume);
            this.groupBox1.Controls.Add(this.txtServerDuration);
            this.groupBox1.Controls.Add(this.txtServerConns);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.btnServerStop);
            this.groupBox1.Controls.Add(this.btnServerStart);
            this.groupBox1.Controls.Add(this.txtListenPort);
            this.groupBox1.Location = new System.Drawing.Point(6, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(506, 148);
            this.groupBox1.TabIndex = 9;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Setup";
            // 
            // txtRecvQueue
            // 
            this.txtRecvQueue.Location = new System.Drawing.Point(81, 45);
            this.txtRecvQueue.Name = "txtRecvQueue";
            this.txtRecvQueue.Size = new System.Drawing.Size(60, 20);
            this.txtRecvQueue.TabIndex = 18;
            this.txtRecvQueue.Text = "ioTest";
            // 
            // rbQueue
            // 
            this.rbQueue.AutoSize = true;
            this.rbQueue.Checked = true;
            this.rbQueue.Location = new System.Drawing.Point(9, 46);
            this.rbQueue.Name = "rbQueue";
            this.rbQueue.Size = new System.Drawing.Size(60, 17);
            this.rbQueue.TabIndex = 17;
            this.rbQueue.TabStop = true;
            this.rbQueue.Text = "Queue:";
            this.rbQueue.UseVisualStyleBackColor = true;
            // 
            // rbPort
            // 
            this.rbPort.AutoSize = true;
            this.rbPort.Location = new System.Drawing.Point(9, 20);
            this.rbPort.Name = "rbPort";
            this.rbPort.Size = new System.Drawing.Size(47, 17);
            this.rbPort.TabIndex = 16;
            this.rbPort.Text = "Port:";
            this.rbPort.UseVisualStyleBackColor = true;
            // 
            // txtServerRate
            // 
            this.txtServerRate.Location = new System.Drawing.Point(203, 97);
            this.txtServerRate.Name = "txtServerRate";
            this.txtServerRate.Size = new System.Drawing.Size(100, 20);
            this.txtServerRate.TabIndex = 15;
            // 
            // txtServerVolume
            // 
            this.txtServerVolume.Location = new System.Drawing.Point(68, 97);
            this.txtServerVolume.Name = "txtServerVolume";
            this.txtServerVolume.Size = new System.Drawing.Size(73, 20);
            this.txtServerVolume.TabIndex = 14;
            // 
            // txtServerDuration
            // 
            this.txtServerDuration.Location = new System.Drawing.Point(203, 71);
            this.txtServerDuration.Name = "txtServerDuration";
            this.txtServerDuration.Size = new System.Drawing.Size(100, 20);
            this.txtServerDuration.TabIndex = 13;
            // 
            // txtServerConns
            // 
            this.txtServerConns.Location = new System.Drawing.Point(81, 71);
            this.txtServerConns.Name = "txtServerConns";
            this.txtServerConns.Size = new System.Drawing.Size(60, 20);
            this.txtServerConns.TabIndex = 12;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(147, 100);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(33, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "Rate:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 100);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Received:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(147, 74);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "Duration:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 74);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(69, 13);
            this.label7.TabIndex = 8;
            this.label7.Text = "Connections:";
            // 
            // btnServerStop
            // 
            this.btnServerStop.Location = new System.Drawing.Point(203, 43);
            this.btnServerStop.Name = "btnServerStop";
            this.btnServerStop.Size = new System.Drawing.Size(100, 23);
            this.btnServerStop.TabIndex = 7;
            this.btnServerStop.Text = "Stop";
            this.btnServerStop.UseVisualStyleBackColor = true;
            this.btnServerStop.Click += new System.EventHandler(this.BtnServerStopClick);
            // 
            // btnServerStart
            // 
            this.btnServerStart.Location = new System.Drawing.Point(203, 17);
            this.btnServerStart.Name = "btnServerStart";
            this.btnServerStart.Size = new System.Drawing.Size(100, 23);
            this.btnServerStart.TabIndex = 6;
            this.btnServerStart.Text = "Start";
            this.btnServerStart.UseVisualStyleBackColor = true;
            this.btnServerStart.Click += new System.EventHandler(this.BtnServerStartClick);
            // 
            // txtListenPort
            // 
            this.txtListenPort.Location = new System.Drawing.Point(81, 19);
            this.txtListenPort.Name = "txtListenPort";
            this.txtListenPort.Size = new System.Drawing.Size(60, 20);
            this.txtListenPort.TabIndex = 2;
            this.txtListenPort.Text = "4099";
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(518, 371);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Client";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.txtSendQueue);
            this.groupBox2.Controls.Add(this.rbClientQueue);
            this.groupBox2.Controls.Add(this.rbClientPort);
            this.groupBox2.Controls.Add(this.txtClientPort);
            this.groupBox2.Controls.Add(this.txtClientHost);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.txtMsgCount);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.txtMsgSize);
            this.groupBox2.Controls.Add(this.button6);
            this.groupBox2.Controls.Add(this.button5);
            this.groupBox2.Controls.Add(this.txtRequestMsg);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.btnClientConnect);
            this.groupBox2.Controls.Add(this.btnClientDisconnect);
            this.groupBox2.Location = new System.Drawing.Point(6, 6);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(506, 256);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Client";
            // 
            // txtSendQueue
            // 
            this.txtSendQueue.Location = new System.Drawing.Point(84, 73);
            this.txtSendQueue.Name = "txtSendQueue";
            this.txtSendQueue.Size = new System.Drawing.Size(60, 20);
            this.txtSendQueue.TabIndex = 22;
            this.txtSendQueue.Text = "ioTest";
            // 
            // rbClientQueue
            // 
            this.rbClientQueue.AutoSize = true;
            this.rbClientQueue.Checked = true;
            this.rbClientQueue.Location = new System.Drawing.Point(6, 74);
            this.rbClientQueue.Name = "rbClientQueue";
            this.rbClientQueue.Size = new System.Drawing.Size(60, 17);
            this.rbClientQueue.TabIndex = 21;
            this.rbClientQueue.TabStop = true;
            this.rbClientQueue.Text = "Queue:";
            this.rbClientQueue.UseVisualStyleBackColor = true;
            // 
            // rbClientPort
            // 
            this.rbClientPort.AutoSize = true;
            this.rbClientPort.Location = new System.Drawing.Point(6, 48);
            this.rbClientPort.Name = "rbClientPort";
            this.rbClientPort.Size = new System.Drawing.Size(47, 17);
            this.rbClientPort.TabIndex = 20;
            this.rbClientPort.Text = "Port:";
            this.rbClientPort.UseVisualStyleBackColor = true;
            // 
            // txtClientPort
            // 
            this.txtClientPort.Location = new System.Drawing.Point(84, 47);
            this.txtClientPort.Name = "txtClientPort";
            this.txtClientPort.Size = new System.Drawing.Size(60, 20);
            this.txtClientPort.TabIndex = 19;
            this.txtClientPort.Text = "4099";
            // 
            // txtClientHost
            // 
            this.txtClientHost.Location = new System.Drawing.Point(44, 21);
            this.txtClientHost.Name = "txtClientHost";
            this.txtClientHost.Size = new System.Drawing.Size(100, 20);
            this.txtClientHost.TabIndex = 17;
            this.txtClientHost.Text = "localhost";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 16;
            this.label1.Text = "Host:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(266, 144);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(31, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "times";
            // 
            // txtMsgCount
            // 
            this.txtMsgCount.Location = new System.Drawing.Point(197, 141);
            this.txtMsgCount.Name = "txtMsgCount";
            this.txtMsgCount.Size = new System.Drawing.Size(63, 20);
            this.txtMsgCount.TabIndex = 14;
            this.txtMsgCount.Text = "100";
            this.txtMsgCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(114, 144);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "byte messages";
            // 
            // txtMsgSize
            // 
            this.txtMsgSize.Location = new System.Drawing.Point(45, 141);
            this.txtMsgSize.Name = "txtMsgSize";
            this.txtMsgSize.Size = new System.Drawing.Size(63, 20);
            this.txtMsgSize.TabIndex = 12;
            this.txtMsgSize.Text = "1000000";
            this.txtMsgSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(303, 138);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(116, 23);
            this.button6.TabIndex = 11;
            this.button6.Text = "Send multiple";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.Button6Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(425, 113);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(75, 23);
            this.button5.TabIndex = 8;
            this.button5.Text = "Send text";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.Button5Click);
            // 
            // txtRequestMsg
            // 
            this.txtRequestMsg.Location = new System.Drawing.Point(45, 115);
            this.txtRequestMsg.Name = "txtRequestMsg";
            this.txtRequestMsg.Size = new System.Drawing.Size(374, 20);
            this.txtRequestMsg.TabIndex = 7;
            this.txtRequestMsg.Text = "blah blah blah";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(31, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Text:";
            // 
            // btnClientConnect
            // 
            this.btnClientConnect.Location = new System.Drawing.Point(153, 19);
            this.btnClientConnect.Name = "btnClientConnect";
            this.btnClientConnect.Size = new System.Drawing.Size(75, 23);
            this.btnClientConnect.TabIndex = 4;
            this.btnClientConnect.Text = "Connect";
            this.btnClientConnect.UseVisualStyleBackColor = true;
            this.btnClientConnect.Click += new System.EventHandler(this.Button1Click);
            // 
            // btnClientDisconnect
            // 
            this.btnClientDisconnect.Location = new System.Drawing.Point(234, 19);
            this.btnClientDisconnect.Name = "btnClientDisconnect";
            this.btnClientDisconnect.Size = new System.Drawing.Size(75, 23);
            this.btnClientDisconnect.TabIndex = 5;
            this.btnClientDisconnect.Text = "Disconnect";
            this.btnClientDisconnect.UseVisualStyleBackColor = true;
            this.btnClientDisconnect.Click += new System.EventHandler(this.Button2Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(518, 371);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 3);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(512, 365);
            this.txtLog.TabIndex = 0;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.Timer1Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(526, 397);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnClientDisconnect;
        private System.Windows.Forms.Button btnClientConnect;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.TextBox txtRequestMsg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtMsgCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtMsgSize;
        private System.Windows.Forms.TextBox txtClientHost;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnServerStop;
        private System.Windows.Forms.Button btnServerStart;
        private System.Windows.Forms.TextBox txtListenPort;
        private System.Windows.Forms.TextBox txtClientPort;
        private System.Windows.Forms.TextBox txtServerDuration;
        private System.Windows.Forms.TextBox txtServerConns;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtServerRate;
        private System.Windows.Forms.TextBox txtServerVolume;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RadioButton rbQueue;
        private System.Windows.Forms.RadioButton rbPort;
        private System.Windows.Forms.TextBox txtRecvQueue;
        private System.Windows.Forms.TextBox txtSendQueue;
        private System.Windows.Forms.RadioButton rbClientQueue;
        private System.Windows.Forms.RadioButton rbClientPort;
    }
}

