/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Highlander.Core.NetMon
{
    partial class NetMonForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetMonForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpAlertRules = new System.Windows.Forms.TabPage();
            this.lvAlertRule = new System.Windows.Forms.ListView();
            this.panelAlertRule = new System.Windows.Forms.Panel();
            this.tpAlertStatus = new System.Windows.Forms.TabPage();
            this.lvAlertSignal = new System.Windows.Forms.ListView();
            this.panelAlertSignal = new System.Windows.Forms.Panel();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.txtMainLog = new System.Windows.Forms.TextBox();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.lvLogEvent = new System.Windows.Forms.ListView();
            this.panelLogEvent = new System.Windows.Forms.Panel();
            this.tpPingServers = new System.Windows.Forms.TabPage();
            this.txtServer2OtherInfo = new System.Windows.Forms.TextBox();
            this.txtServer3OtherInfo = new System.Windows.Forms.TextBox();
            this.txtServer4OtherInfo = new System.Windows.Forms.TextBox();
            this.txtServer5OtherInfo = new System.Windows.Forms.TextBox();
            this.txtServer1OtherInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtServer0OtherInfo = new System.Windows.Forms.TextBox();
            this.txtServer5Status = new System.Windows.Forms.TextBox();
            this.txtServer5LastReplied = new System.Windows.Forms.TextBox();
            this.txtServer5LastChecked = new System.Windows.Forms.TextBox();
            this.chkServer5Ping = new System.Windows.Forms.CheckBox();
            this.txtServer5Address = new System.Windows.Forms.TextBox();
            this.txtServer4Status = new System.Windows.Forms.TextBox();
            this.txtServer4LastReplied = new System.Windows.Forms.TextBox();
            this.txtServer4LastChecked = new System.Windows.Forms.TextBox();
            this.chkServer4Ping = new System.Windows.Forms.CheckBox();
            this.txtServer4Address = new System.Windows.Forms.TextBox();
            this.txtServer3Status = new System.Windows.Forms.TextBox();
            this.txtServer3LastReplied = new System.Windows.Forms.TextBox();
            this.txtServer3LastChecked = new System.Windows.Forms.TextBox();
            this.chkServer3Ping = new System.Windows.Forms.CheckBox();
            this.txtServer3Address = new System.Windows.Forms.TextBox();
            this.txtServer2Status = new System.Windows.Forms.TextBox();
            this.txtServer2LastReplied = new System.Windows.Forms.TextBox();
            this.txtServer2LastChecked = new System.Windows.Forms.TextBox();
            this.chkServer2Ping = new System.Windows.Forms.CheckBox();
            this.txtServer2Address = new System.Windows.Forms.TextBox();
            this.txtServer1Status = new System.Windows.Forms.TextBox();
            this.txtServer1LastReplied = new System.Windows.Forms.TextBox();
            this.txtServer1LastChecked = new System.Windows.Forms.TextBox();
            this.chkServer1Ping = new System.Windows.Forms.CheckBox();
            this.txtServer1Address = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtServer0Status = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.txtServer0LastReplied = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.txtServer0LastChecked = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkServer0Ping = new System.Windows.Forms.CheckBox();
            this.txtServer0Address = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnPingAll = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tpAlertRules.SuspendLayout();
            this.tpAlertStatus.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage6.SuspendLayout();
            this.tpPingServers.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpAlertRules);
            this.tabControl1.Controls.Add(this.tpAlertStatus);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage6);
            this.tabControl1.Controls.Add(this.tpPingServers);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1133, 425);
            this.tabControl1.TabIndex = 0;
            // 
            // tpAlertRules
            // 
            this.tpAlertRules.Controls.Add(this.lvAlertRule);
            this.tpAlertRules.Controls.Add(this.panelAlertRule);
            this.tpAlertRules.Location = new System.Drawing.Point(4, 22);
            this.tpAlertRules.Name = "tpAlertRules";
            this.tpAlertRules.Padding = new System.Windows.Forms.Padding(3);
            this.tpAlertRules.Size = new System.Drawing.Size(1125, 399);
            this.tpAlertRules.TabIndex = 3;
            this.tpAlertRules.Text = "Alert Rules";
            this.tpAlertRules.UseVisualStyleBackColor = true;
            // 
            // lvAlertRule
            // 
            this.lvAlertRule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvAlertRule.Location = new System.Drawing.Point(3, 33);
            this.lvAlertRule.Name = "lvAlertRule";
            this.lvAlertRule.Size = new System.Drawing.Size(1119, 363);
            this.lvAlertRule.TabIndex = 6;
            this.lvAlertRule.UseCompatibleStateImageBehavior = false;
            // 
            // panelAlertRule
            // 
            this.panelAlertRule.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAlertRule.Location = new System.Drawing.Point(3, 3);
            this.panelAlertRule.Name = "panelAlertRule";
            this.panelAlertRule.Size = new System.Drawing.Size(1119, 30);
            this.panelAlertRule.TabIndex = 5;
            // 
            // tpAlertStatus
            // 
            this.tpAlertStatus.Controls.Add(this.lvAlertSignal);
            this.tpAlertStatus.Controls.Add(this.panelAlertSignal);
            this.tpAlertStatus.Location = new System.Drawing.Point(4, 22);
            this.tpAlertStatus.Name = "tpAlertStatus";
            this.tpAlertStatus.Padding = new System.Windows.Forms.Padding(3);
            this.tpAlertStatus.Size = new System.Drawing.Size(1125, 399);
            this.tpAlertStatus.TabIndex = 10;
            this.tpAlertStatus.Text = "Alert Status";
            this.tpAlertStatus.UseVisualStyleBackColor = true;
            // 
            // lvAlertSignal
            // 
            this.lvAlertSignal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvAlertSignal.Location = new System.Drawing.Point(3, 33);
            this.lvAlertSignal.Name = "lvAlertSignal";
            this.lvAlertSignal.Size = new System.Drawing.Size(1119, 363);
            this.lvAlertSignal.TabIndex = 7;
            this.lvAlertSignal.UseCompatibleStateImageBehavior = false;
            // 
            // panelAlertSignal
            // 
            this.panelAlertSignal.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelAlertSignal.Location = new System.Drawing.Point(3, 3);
            this.panelAlertSignal.Name = "panelAlertSignal";
            this.panelAlertSignal.Size = new System.Drawing.Size(1119, 30);
            this.panelAlertSignal.TabIndex = 6;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.txtMainLog);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1125, 399);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Log";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // txtMainLog
            // 
            this.txtMainLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMainLog.Location = new System.Drawing.Point(3, 3);
            this.txtMainLog.Multiline = true;
            this.txtMainLog.Name = "txtMainLog";
            this.txtMainLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMainLog.Size = new System.Drawing.Size(1119, 393);
            this.txtMainLog.TabIndex = 1;
            // 
            // tabPage6
            // 
            this.tabPage6.Controls.Add(this.lvLogEvent);
            this.tabPage6.Controls.Add(this.panelLogEvent);
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(1125, 399);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "Network Log";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // lvLogEvent
            // 
            this.lvLogEvent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvLogEvent.Location = new System.Drawing.Point(3, 33);
            this.lvLogEvent.Name = "lvLogEvent";
            this.lvLogEvent.Size = new System.Drawing.Size(1119, 363);
            this.lvLogEvent.TabIndex = 1;
            this.lvLogEvent.UseCompatibleStateImageBehavior = false;
            // 
            // panelLogEvent
            // 
            this.panelLogEvent.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLogEvent.Location = new System.Drawing.Point(3, 3);
            this.panelLogEvent.Name = "panelLogEvent";
            this.panelLogEvent.Size = new System.Drawing.Size(1119, 30);
            this.panelLogEvent.TabIndex = 0;
            // 
            // tpPingServers
            // 
            this.tpPingServers.Controls.Add(this.txtServer2OtherInfo);
            this.tpPingServers.Controls.Add(this.txtServer3OtherInfo);
            this.tpPingServers.Controls.Add(this.txtServer4OtherInfo);
            this.tpPingServers.Controls.Add(this.txtServer5OtherInfo);
            this.tpPingServers.Controls.Add(this.txtServer1OtherInfo);
            this.tpPingServers.Controls.Add(this.label1);
            this.tpPingServers.Controls.Add(this.txtServer0OtherInfo);
            this.tpPingServers.Controls.Add(this.txtServer5Status);
            this.tpPingServers.Controls.Add(this.txtServer5LastReplied);
            this.tpPingServers.Controls.Add(this.txtServer5LastChecked);
            this.tpPingServers.Controls.Add(this.chkServer5Ping);
            this.tpPingServers.Controls.Add(this.txtServer5Address);
            this.tpPingServers.Controls.Add(this.txtServer4Status);
            this.tpPingServers.Controls.Add(this.txtServer4LastReplied);
            this.tpPingServers.Controls.Add(this.txtServer4LastChecked);
            this.tpPingServers.Controls.Add(this.chkServer4Ping);
            this.tpPingServers.Controls.Add(this.txtServer4Address);
            this.tpPingServers.Controls.Add(this.txtServer3Status);
            this.tpPingServers.Controls.Add(this.txtServer3LastReplied);
            this.tpPingServers.Controls.Add(this.txtServer3LastChecked);
            this.tpPingServers.Controls.Add(this.chkServer3Ping);
            this.tpPingServers.Controls.Add(this.txtServer3Address);
            this.tpPingServers.Controls.Add(this.txtServer2Status);
            this.tpPingServers.Controls.Add(this.txtServer2LastReplied);
            this.tpPingServers.Controls.Add(this.txtServer2LastChecked);
            this.tpPingServers.Controls.Add(this.chkServer2Ping);
            this.tpPingServers.Controls.Add(this.txtServer2Address);
            this.tpPingServers.Controls.Add(this.txtServer1Status);
            this.tpPingServers.Controls.Add(this.txtServer1LastReplied);
            this.tpPingServers.Controls.Add(this.txtServer1LastChecked);
            this.tpPingServers.Controls.Add(this.chkServer1Ping);
            this.tpPingServers.Controls.Add(this.txtServer1Address);
            this.tpPingServers.Controls.Add(this.label6);
            this.tpPingServers.Controls.Add(this.txtServer0Status);
            this.tpPingServers.Controls.Add(this.label5);
            this.tpPingServers.Controls.Add(this.txtServer0LastReplied);
            this.tpPingServers.Controls.Add(this.label4);
            this.tpPingServers.Controls.Add(this.txtServer0LastChecked);
            this.tpPingServers.Controls.Add(this.label3);
            this.tpPingServers.Controls.Add(this.chkServer0Ping);
            this.tpPingServers.Controls.Add(this.txtServer0Address);
            this.tpPingServers.Controls.Add(this.label2);
            this.tpPingServers.Controls.Add(this.btnPingAll);
            this.tpPingServers.Location = new System.Drawing.Point(4, 22);
            this.tpPingServers.Name = "tpPingServers";
            this.tpPingServers.Padding = new System.Windows.Forms.Padding(3);
            this.tpPingServers.Size = new System.Drawing.Size(1125, 399);
            this.tpPingServers.TabIndex = 9;
            this.tpPingServers.Text = "Ping Servers";
            this.tpPingServers.UseVisualStyleBackColor = true;
            // 
            // txtServer2OtherInfo
            // 
            this.txtServer2OtherInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer2OtherInfo.Location = new System.Drawing.Point(565, 76);
            this.txtServer2OtherInfo.Name = "txtServer2OtherInfo";
            this.txtServer2OtherInfo.Size = new System.Drawing.Size(442, 20);
            this.txtServer2OtherInfo.TabIndex = 44;
            this.txtServer2OtherInfo.Text = "<info>";
            // 
            // txtServer3OtherInfo
            // 
            this.txtServer3OtherInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer3OtherInfo.Location = new System.Drawing.Point(565, 102);
            this.txtServer3OtherInfo.Name = "txtServer3OtherInfo";
            this.txtServer3OtherInfo.Size = new System.Drawing.Size(442, 20);
            this.txtServer3OtherInfo.TabIndex = 43;
            this.txtServer3OtherInfo.Text = "<info>";
            // 
            // txtServer4OtherInfo
            // 
            this.txtServer4OtherInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer4OtherInfo.Location = new System.Drawing.Point(565, 128);
            this.txtServer4OtherInfo.Name = "txtServer4OtherInfo";
            this.txtServer4OtherInfo.Size = new System.Drawing.Size(442, 20);
            this.txtServer4OtherInfo.TabIndex = 42;
            this.txtServer4OtherInfo.Text = "<info>";
            // 
            // txtServer5OtherInfo
            // 
            this.txtServer5OtherInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer5OtherInfo.Location = new System.Drawing.Point(565, 154);
            this.txtServer5OtherInfo.Name = "txtServer5OtherInfo";
            this.txtServer5OtherInfo.Size = new System.Drawing.Size(442, 20);
            this.txtServer5OtherInfo.TabIndex = 41;
            this.txtServer5OtherInfo.Text = "<info>";
            // 
            // txtServer1OtherInfo
            // 
            this.txtServer1OtherInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer1OtherInfo.Location = new System.Drawing.Point(565, 50);
            this.txtServer1OtherInfo.Name = "txtServer1OtherInfo";
            this.txtServer1OtherInfo.Size = new System.Drawing.Size(442, 20);
            this.txtServer1OtherInfo.TabIndex = 40;
            this.txtServer1OtherInfo.Text = "<info>";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(562, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(90, 13);
            this.label1.TabIndex = 39;
            this.label1.Text = "Other information:";
            // 
            // txtServer0OtherInfo
            // 
            this.txtServer0OtherInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer0OtherInfo.Location = new System.Drawing.Point(565, 24);
            this.txtServer0OtherInfo.Name = "txtServer0OtherInfo";
            this.txtServer0OtherInfo.Size = new System.Drawing.Size(442, 20);
            this.txtServer0OtherInfo.TabIndex = 38;
            this.txtServer0OtherInfo.Text = "<info>";
            // 
            // txtServer5Status
            // 
            this.txtServer5Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer5Status.Location = new System.Drawing.Point(431, 154);
            this.txtServer5Status.Name = "txtServer5Status";
            this.txtServer5Status.Size = new System.Drawing.Size(128, 20);
            this.txtServer5Status.TabIndex = 37;
            this.txtServer5Status.Text = "<status>";
            // 
            // txtServer5LastReplied
            // 
            this.txtServer5LastReplied.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer5LastReplied.Location = new System.Drawing.Point(297, 156);
            this.txtServer5LastReplied.Name = "txtServer5LastReplied";
            this.txtServer5LastReplied.Size = new System.Drawing.Size(128, 13);
            this.txtServer5LastReplied.TabIndex = 36;
            this.txtServer5LastReplied.Text = "<last replied>";
            // 
            // txtServer5LastChecked
            // 
            this.txtServer5LastChecked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer5LastChecked.Location = new System.Drawing.Point(163, 156);
            this.txtServer5LastChecked.Name = "txtServer5LastChecked";
            this.txtServer5LastChecked.Size = new System.Drawing.Size(128, 13);
            this.txtServer5LastChecked.TabIndex = 35;
            this.txtServer5LastChecked.Text = "<last checked>";
            // 
            // chkServer5Ping
            // 
            this.chkServer5Ping.AutoSize = true;
            this.chkServer5Ping.Location = new System.Drawing.Point(142, 156);
            this.chkServer5Ping.Name = "chkServer5Ping";
            this.chkServer5Ping.Size = new System.Drawing.Size(15, 14);
            this.chkServer5Ping.TabIndex = 34;
            this.chkServer5Ping.UseVisualStyleBackColor = true;
            // 
            // txtServer5Address
            // 
            this.txtServer5Address.Location = new System.Drawing.Point(8, 153);
            this.txtServer5Address.Name = "txtServer5Address";
            this.txtServer5Address.Size = new System.Drawing.Size(128, 20);
            this.txtServer5Address.TabIndex = 33;
            // 
            // txtServer4Status
            // 
            this.txtServer4Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer4Status.Location = new System.Drawing.Point(431, 128);
            this.txtServer4Status.Name = "txtServer4Status";
            this.txtServer4Status.Size = new System.Drawing.Size(128, 20);
            this.txtServer4Status.TabIndex = 32;
            this.txtServer4Status.Text = "<status>";
            // 
            // txtServer4LastReplied
            // 
            this.txtServer4LastReplied.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer4LastReplied.Location = new System.Drawing.Point(297, 130);
            this.txtServer4LastReplied.Name = "txtServer4LastReplied";
            this.txtServer4LastReplied.Size = new System.Drawing.Size(128, 13);
            this.txtServer4LastReplied.TabIndex = 31;
            this.txtServer4LastReplied.Text = "<last replied>";
            // 
            // txtServer4LastChecked
            // 
            this.txtServer4LastChecked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer4LastChecked.Location = new System.Drawing.Point(163, 130);
            this.txtServer4LastChecked.Name = "txtServer4LastChecked";
            this.txtServer4LastChecked.Size = new System.Drawing.Size(128, 13);
            this.txtServer4LastChecked.TabIndex = 30;
            this.txtServer4LastChecked.Text = "<last checked>";
            // 
            // chkServer4Ping
            // 
            this.chkServer4Ping.AutoSize = true;
            this.chkServer4Ping.Checked = true;
            this.chkServer4Ping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServer4Ping.Location = new System.Drawing.Point(142, 130);
            this.chkServer4Ping.Name = "chkServer4Ping";
            this.chkServer4Ping.Size = new System.Drawing.Size(15, 14);
            this.chkServer4Ping.TabIndex = 29;
            this.chkServer4Ping.UseVisualStyleBackColor = true;
            // 
            // txtServer4Address
            // 
            this.txtServer4Address.Location = new System.Drawing.Point(8, 127);
            this.txtServer4Address.Name = "txtServer4Address";
            this.txtServer4Address.Size = new System.Drawing.Size(128, 20);
            this.txtServer4Address.TabIndex = 28;
            this.txtServer4Address.Text = "STG;sydwadbrl01:8412";
            // 
            // txtServer3Status
            // 
            this.txtServer3Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer3Status.Location = new System.Drawing.Point(431, 102);
            this.txtServer3Status.Name = "txtServer3Status";
            this.txtServer3Status.Size = new System.Drawing.Size(128, 20);
            this.txtServer3Status.TabIndex = 27;
            this.txtServer3Status.Text = "<status>";
            // 
            // txtServer3LastReplied
            // 
            this.txtServer3LastReplied.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer3LastReplied.Location = new System.Drawing.Point(297, 104);
            this.txtServer3LastReplied.Name = "txtServer3LastReplied";
            this.txtServer3LastReplied.Size = new System.Drawing.Size(128, 13);
            this.txtServer3LastReplied.TabIndex = 26;
            this.txtServer3LastReplied.Text = "<last replied>";
            // 
            // txtServer3LastChecked
            // 
            this.txtServer3LastChecked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer3LastChecked.Location = new System.Drawing.Point(163, 104);
            this.txtServer3LastChecked.Name = "txtServer3LastChecked";
            this.txtServer3LastChecked.Size = new System.Drawing.Size(128, 13);
            this.txtServer3LastChecked.TabIndex = 25;
            this.txtServer3LastChecked.Text = "<last checked>";
            // 
            // chkServer3Ping
            // 
            this.chkServer3Ping.AutoSize = true;
            this.chkServer3Ping.Checked = true;
            this.chkServer3Ping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServer3Ping.Location = new System.Drawing.Point(142, 104);
            this.chkServer3Ping.Name = "chkServer3Ping";
            this.chkServer3Ping.Size = new System.Drawing.Size(15, 14);
            this.chkServer3Ping.TabIndex = 24;
            this.chkServer3Ping.UseVisualStyleBackColor = true;
            // 
            // txtServer3Address
            // 
            this.txtServer3Address.Location = new System.Drawing.Point(8, 101);
            this.txtServer3Address.Name = "txtServer3Address";
            this.txtServer3Address.Size = new System.Drawing.Size(128, 20);
            this.txtServer3Address.TabIndex = 23;
            this.txtServer3Address.Text = "SIT;sydwatqur01:8312";
            // 
            // txtServer2Status
            // 
            this.txtServer2Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer2Status.Location = new System.Drawing.Point(431, 76);
            this.txtServer2Status.Name = "txtServer2Status";
            this.txtServer2Status.Size = new System.Drawing.Size(128, 20);
            this.txtServer2Status.TabIndex = 22;
            this.txtServer2Status.Text = "<status>";
            // 
            // txtServer2LastReplied
            // 
            this.txtServer2LastReplied.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer2LastReplied.Location = new System.Drawing.Point(297, 78);
            this.txtServer2LastReplied.Name = "txtServer2LastReplied";
            this.txtServer2LastReplied.Size = new System.Drawing.Size(128, 13);
            this.txtServer2LastReplied.TabIndex = 21;
            this.txtServer2LastReplied.Text = "<last replied>";
            // 
            // txtServer2LastChecked
            // 
            this.txtServer2LastChecked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer2LastChecked.Location = new System.Drawing.Point(163, 78);
            this.txtServer2LastChecked.Name = "txtServer2LastChecked";
            this.txtServer2LastChecked.Size = new System.Drawing.Size(128, 13);
            this.txtServer2LastChecked.TabIndex = 20;
            this.txtServer2LastChecked.Text = "<last checked>";
            // 
            // chkServer2Ping
            // 
            this.chkServer2Ping.AutoSize = true;
            this.chkServer2Ping.Checked = true;
            this.chkServer2Ping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServer2Ping.Location = new System.Drawing.Point(142, 78);
            this.chkServer2Ping.Name = "chkServer2Ping";
            this.chkServer2Ping.Size = new System.Drawing.Size(15, 14);
            this.chkServer2Ping.TabIndex = 19;
            this.chkServer2Ping.UseVisualStyleBackColor = true;
            // 
            // txtServer2Address
            // 
            this.txtServer2Address.Location = new System.Drawing.Point(8, 75);
            this.txtServer2Address.Name = "txtServer2Address";
            this.txtServer2Address.Size = new System.Drawing.Size(128, 20);
            this.txtServer2Address.TabIndex = 18;
            this.txtServer2Address.Text = "DEV;lonbqrdl2j";
            // 
            // txtServer1Status
            // 
            this.txtServer1Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer1Status.Location = new System.Drawing.Point(431, 50);
            this.txtServer1Status.Name = "txtServer1Status";
            this.txtServer1Status.Size = new System.Drawing.Size(128, 20);
            this.txtServer1Status.TabIndex = 17;
            this.txtServer1Status.Text = "<status>";
            // 
            // txtServer1LastReplied
            // 
            this.txtServer1LastReplied.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer1LastReplied.Location = new System.Drawing.Point(297, 52);
            this.txtServer1LastReplied.Name = "txtServer1LastReplied";
            this.txtServer1LastReplied.Size = new System.Drawing.Size(128, 13);
            this.txtServer1LastReplied.TabIndex = 16;
            this.txtServer1LastReplied.Text = "<last replied>";
            // 
            // txtServer1LastChecked
            // 
            this.txtServer1LastChecked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer1LastChecked.Location = new System.Drawing.Point(163, 52);
            this.txtServer1LastChecked.Name = "txtServer1LastChecked";
            this.txtServer1LastChecked.Size = new System.Drawing.Size(128, 13);
            this.txtServer1LastChecked.TabIndex = 15;
            this.txtServer1LastChecked.Text = "<last checked>";
            // 
            // chkServer1Ping
            // 
            this.chkServer1Ping.AutoSize = true;
            this.chkServer1Ping.Checked = true;
            this.chkServer1Ping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServer1Ping.Location = new System.Drawing.Point(142, 52);
            this.chkServer1Ping.Name = "chkServer1Ping";
            this.chkServer1Ping.Size = new System.Drawing.Size(15, 14);
            this.chkServer1Ping.TabIndex = 14;
            this.chkServer1Ping.UseVisualStyleBackColor = true;
            // 
            // txtServer1Address
            // 
            this.txtServer1Address.Location = new System.Drawing.Point(8, 49);
            this.txtServer1Address.Name = "txtServer1Address";
            this.txtServer1Address.Size = new System.Drawing.Size(128, 20);
            this.txtServer1Address.TabIndex = 13;
            this.txtServer1Address.Text = "DEV;sydwadqds01";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(428, 7);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(83, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "Connect Status:";
            // 
            // txtServer0Status
            // 
            this.txtServer0Status.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer0Status.Location = new System.Drawing.Point(431, 24);
            this.txtServer0Status.Name = "txtServer0Status";
            this.txtServer0Status.Size = new System.Drawing.Size(128, 20);
            this.txtServer0Status.TabIndex = 11;
            this.txtServer0Status.Text = "<status>";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(294, 7);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(61, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Last replied";
            // 
            // txtServer0LastReplied
            // 
            this.txtServer0LastReplied.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer0LastReplied.Location = new System.Drawing.Point(297, 26);
            this.txtServer0LastReplied.Name = "txtServer0LastReplied";
            this.txtServer0LastReplied.Size = new System.Drawing.Size(128, 13);
            this.txtServer0LastReplied.TabIndex = 9;
            this.txtServer0LastReplied.Text = "<last replied>";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(163, 7);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Last checked";
            // 
            // txtServer0LastChecked
            // 
            this.txtServer0LastChecked.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtServer0LastChecked.Location = new System.Drawing.Point(163, 26);
            this.txtServer0LastChecked.Name = "txtServer0LastChecked";
            this.txtServer0LastChecked.Size = new System.Drawing.Size(128, 13);
            this.txtServer0LastChecked.TabIndex = 7;
            this.txtServer0LastChecked.Text = "<last checked>";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(123, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(34, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Ping?";
            // 
            // chkServer0Ping
            // 
            this.chkServer0Ping.AutoSize = true;
            this.chkServer0Ping.Checked = true;
            this.chkServer0Ping.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkServer0Ping.Location = new System.Drawing.Point(142, 26);
            this.chkServer0Ping.Name = "chkServer0Ping";
            this.chkServer0Ping.Size = new System.Drawing.Size(15, 14);
            this.chkServer0Ping.TabIndex = 5;
            this.chkServer0Ping.UseVisualStyleBackColor = true;
            // 
            // txtServer0Address
            // 
            this.txtServer0Address.Location = new System.Drawing.Point(8, 23);
            this.txtServer0Address.Name = "txtServer0Address";
            this.txtServer0Address.Size = new System.Drawing.Size(128, 20);
            this.txtServer0Address.TabIndex = 4;
            this.txtServer0Address.Text = "DEV;localhost:8212";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 7);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Server:";
            // 
            // btnPingAll
            // 
            this.btnPingAll.Location = new System.Drawing.Point(8, 179);
            this.btnPingAll.Name = "btnPingAll";
            this.btnPingAll.Size = new System.Drawing.Size(128, 23);
            this.btnPingAll.TabIndex = 0;
            this.btnPingAll.Text = "Ping All";
            this.btnPingAll.UseVisualStyleBackColor = true;
            this.btnPingAll.Click += new System.EventHandler(this.BtnPingAllClick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1133, 425);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Highlander Network Monitor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.tabControl1.ResumeLayout(false);
            this.tpAlertRules.ResumeLayout(false);
            this.tpAlertStatus.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.tabPage6.ResumeLayout(false);
            this.tpPingServers.ResumeLayout(false);
            this.tpPingServers.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TextBox txtMainLog;
        private System.Windows.Forms.TabPage tpAlertRules;
        private System.Windows.Forms.TabPage tabPage6;
        private System.Windows.Forms.ListView lvLogEvent;
        private System.Windows.Forms.Panel panelLogEvent;
        private System.Windows.Forms.TabPage tpPingServers;
        private System.Windows.Forms.TextBox txtServer5Status;
        private System.Windows.Forms.TextBox txtServer5LastReplied;
        private System.Windows.Forms.TextBox txtServer5LastChecked;
        private System.Windows.Forms.CheckBox chkServer5Ping;
        private System.Windows.Forms.TextBox txtServer5Address;
        private System.Windows.Forms.TextBox txtServer4Status;
        private System.Windows.Forms.TextBox txtServer4LastReplied;
        private System.Windows.Forms.TextBox txtServer4LastChecked;
        private System.Windows.Forms.CheckBox chkServer4Ping;
        private System.Windows.Forms.TextBox txtServer4Address;
        private System.Windows.Forms.TextBox txtServer3Status;
        private System.Windows.Forms.TextBox txtServer3LastReplied;
        private System.Windows.Forms.TextBox txtServer3LastChecked;
        private System.Windows.Forms.CheckBox chkServer3Ping;
        private System.Windows.Forms.TextBox txtServer3Address;
        private System.Windows.Forms.TextBox txtServer2Status;
        private System.Windows.Forms.TextBox txtServer2LastReplied;
        private System.Windows.Forms.TextBox txtServer2LastChecked;
        private System.Windows.Forms.CheckBox chkServer2Ping;
        private System.Windows.Forms.TextBox txtServer2Address;
        private System.Windows.Forms.TextBox txtServer1Status;
        private System.Windows.Forms.TextBox txtServer1LastReplied;
        private System.Windows.Forms.TextBox txtServer1LastChecked;
        private System.Windows.Forms.CheckBox chkServer1Ping;
        private System.Windows.Forms.TextBox txtServer1Address;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtServer0Status;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtServer0LastReplied;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtServer0LastChecked;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkServer0Ping;
        private System.Windows.Forms.TextBox txtServer0Address;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnPingAll;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtServer0OtherInfo;
        private System.Windows.Forms.TextBox txtServer2OtherInfo;
        private System.Windows.Forms.TextBox txtServer3OtherInfo;
        private System.Windows.Forms.TextBox txtServer4OtherInfo;
        private System.Windows.Forms.TextBox txtServer5OtherInfo;
        private System.Windows.Forms.TextBox txtServer1OtherInfo;
        private System.Windows.Forms.TabPage tpAlertStatus;
        private System.Windows.Forms.ListView lvAlertRule;
        private System.Windows.Forms.Panel panelAlertRule;
        private System.Windows.Forms.ListView lvAlertSignal;
        private System.Windows.Forms.Panel panelAlertSignal;
    }
}

