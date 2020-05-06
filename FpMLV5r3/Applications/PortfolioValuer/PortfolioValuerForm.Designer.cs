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

namespace Highlander.PortfolioValuer.V5r3
{
    partial class PortfolioValuerForm
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
            this.rbMarketSydneyLive = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.chkCalcAllFXStresses = new System.Windows.Forms.CheckBox();
            this.chkCalcAllIRStresses = new System.Windows.Forms.CheckBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.cbMarketDateEnd = new System.Windows.Forms.CheckBox();
            this.dtpMarketEndDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.dtpMarketQRDate = new System.Windows.Forms.DateTimePicker();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBoxBaseParty = new System.Windows.Forms.ComboBox();
            this.comboBoxReportingCurrency = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.dtpBaseDate = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.cbClientNameSpace = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnAutoValue = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.bnTradeHistoricalValuation = new System.Windows.Forms.Button();
            this.nudRetainResultsHours = new System.Windows.Forms.NumericUpDown();
            this.btnTradeCalcValuations = new System.Windows.Forms.Button();
            this.chkRunAtServer = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbMarketQREODDated = new System.Windows.Forms.RadioButton();
            this.rbMarketQREODLatest = new System.Windows.Forms.RadioButton();
            this.rbMarketQRLive = new System.Windows.Forms.RadioButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpTradeAll = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lvTradeAll = new System.Windows.Forms.ListView();
            this.panel4 = new System.Windows.Forms.Panel();
            this.btnTradeAddAll = new System.Windows.Forms.Button();
            this.btnTradeAddSelected = new System.Windows.Forms.Button();
            this.pnlTradeAll = new System.Windows.Forms.Panel();
            this.tpTradeSel = new System.Windows.Forms.TabPage();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.lvTradeSel = new System.Windows.Forms.ListView();
            this.panel5 = new System.Windows.Forms.Panel();
            this.btnTradeClearAll = new System.Windows.Forms.Button();
            this.btnTradeRemoveSelected = new System.Windows.Forms.Button();
            this.pnlTradeSel = new System.Windows.Forms.Panel();
            this.tpCurveRaw = new System.Windows.Forms.TabPage();
            this.lvCurveRaw = new System.Windows.Forms.ListView();
            this.pnlCurveRaw = new System.Windows.Forms.Panel();
            this.tpValueRaw = new System.Windows.Forms.TabPage();
            this.lvValueRaw = new System.Windows.Forms.ListView();
            this.panelValueRaw = new System.Windows.Forms.Panel();
            this.tpPortfolios = new System.Windows.Forms.TabPage();
            this.lvPortfolio = new System.Windows.Forms.ListView();
            this.panelPortfolio = new System.Windows.Forms.Panel();
            this.tpRequests = new System.Windows.Forms.TabPage();
            this.lvProgress = new System.Windows.Forms.ListView();
            this.panelProgress = new System.Windows.Forms.Panel();
            this.tpDebugLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRetainResultsHours)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tpTradeAll.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.panel4.SuspendLayout();
            this.tpTradeSel.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.panel5.SuspendLayout();
            this.tpCurveRaw.SuspendLayout();
            this.tpValueRaw.SuspendLayout();
            this.tpPortfolios.SuspendLayout();
            this.tpRequests.SuspendLayout();
            this.tpDebugLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rbMarketSydneyLive);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1875, 167);
            this.panel1.TabIndex = 1;
            // 
            // rbMarketSydneyLive
            // 
            this.rbMarketSydneyLive.AutoSize = true;
            this.rbMarketSydneyLive.Location = new System.Drawing.Point(32, 127);
            this.rbMarketSydneyLive.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbMarketSydneyLive.Name = "rbMarketSydneyLive";
            this.rbMarketSydneyLive.Size = new System.Drawing.Size(206, 21);
            this.rbMarketSydneyLive.TabIndex = 13;
            this.rbMarketSydneyLive.Text = "Sydney Live (Reuters/Desk)";
            this.rbMarketSydneyLive.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.groupBox7);
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.btnAutoValue);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.bnTradeHistoricalValuation);
            this.groupBox1.Controls.Add(this.nudRetainResultsHours);
            this.groupBox1.Controls.Add(this.btnTradeCalcValuations);
            this.groupBox1.Controls.Add(this.chkRunAtServer);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(16, 4);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox1.Size = new System.Drawing.Size(1841, 156);
            this.groupBox1.TabIndex = 45;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Trade Valuation";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1573, 32);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(138, 17);
            this.label6.TabIndex = 74;
            this.label6.Text = "Market Environment:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(1721, 27);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(111, 22);
            this.textBox1.TabIndex = 73;
            this.textBox1.Text = "QR_LIVE";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.chkCalcAllFXStresses);
            this.groupBox7.Controls.Add(this.chkCalcAllIRStresses);
            this.groupBox7.Location = new System.Drawing.Point(941, 0);
            this.groupBox7.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox7.Size = new System.Drawing.Size(257, 87);
            this.groupBox7.TabIndex = 72;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Stress Data";
            // 
            // chkCalcAllFXStresses
            // 
            this.chkCalcAllFXStresses.AutoSize = true;
            this.chkCalcAllFXStresses.Location = new System.Drawing.Point(20, 27);
            this.chkCalcAllFXStresses.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkCalcAllFXStresses.Name = "chkCalcAllFXStresses";
            this.chkCalcAllFXStresses.Size = new System.Drawing.Size(184, 21);
            this.chkCalcAllFXStresses.TabIndex = 45;
            this.chkCalcAllFXStresses.Text = "Calculate all FX stresses";
            this.chkCalcAllFXStresses.UseVisualStyleBackColor = true;
            // 
            // chkCalcAllIRStresses
            // 
            this.chkCalcAllIRStresses.AutoSize = true;
            this.chkCalcAllIRStresses.Location = new System.Drawing.Point(20, 57);
            this.chkCalcAllIRStresses.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkCalcAllIRStresses.Name = "chkCalcAllIRStresses";
            this.chkCalcAllIRStresses.Size = new System.Drawing.Size(180, 21);
            this.chkCalcAllIRStresses.TabIndex = 44;
            this.chkCalcAllIRStresses.Text = "Calculate all IR stresses";
            this.chkCalcAllIRStresses.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.cbMarketDateEnd);
            this.groupBox6.Controls.Add(this.dtpMarketEndDate);
            this.groupBox6.Controls.Add(this.label1);
            this.groupBox6.Controls.Add(this.dtpMarketQRDate);
            this.groupBox6.Controls.Add(this.checkBox1);
            this.groupBox6.Location = new System.Drawing.Point(1272, 0);
            this.groupBox6.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox6.Size = new System.Drawing.Size(300, 110);
            this.groupBox6.TabIndex = 71;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Historial Data";
            // 
            // cbMarketDateEnd
            // 
            this.cbMarketDateEnd.AutoSize = true;
            this.cbMarketDateEnd.Location = new System.Drawing.Point(11, 54);
            this.cbMarketDateEnd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbMarketDateEnd.Name = "cbMarketDateEnd";
            this.cbMarketDateEnd.Size = new System.Drawing.Size(141, 21);
            this.cbMarketDateEnd.TabIndex = 66;
            this.cbMarketDateEnd.Text = "Market date end :";
            this.cbMarketDateEnd.UseVisualStyleBackColor = true;
            // 
            // dtpMarketEndDate
            // 
            this.dtpMarketEndDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpMarketEndDate.Location = new System.Drawing.Point(155, 53);
            this.dtpMarketEndDate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dtpMarketEndDate.Name = "dtpMarketEndDate";
            this.dtpMarketEndDate.Size = new System.Drawing.Size(136, 22);
            this.dtpMarketEndDate.TabIndex = 61;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(33, 28);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 17);
            this.label1.TabIndex = 65;
            this.label1.Text = "Market date start :";
            // 
            // dtpMarketQRDate
            // 
            this.dtpMarketQRDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpMarketQRDate.Location = new System.Drawing.Point(156, 25);
            this.dtpMarketQRDate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dtpMarketQRDate.Name = "dtpMarketQRDate";
            this.dtpMarketQRDate.Size = new System.Drawing.Size(136, 22);
            this.dtpMarketQRDate.TabIndex = 12;
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(11, 89);
            this.checkBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(223, 21);
            this.checkBox1.TabIndex = 62;
            this.checkBox1.Text = "Calculate all historical stresses";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.comboBoxBaseParty);
            this.groupBox3.Controls.Add(this.comboBoxReportingCurrency);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.dtpBaseDate);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.cbClientNameSpace);
            this.groupBox3.Location = new System.Drawing.Point(299, 0);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox3.Size = new System.Drawing.Size(263, 156);
            this.groupBox3.TabIndex = 70;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Basic Data";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(20, 89);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(81, 17);
            this.label5.TabIndex = 57;
            this.label5.Text = "Base Party:";
            // 
            // comboBoxBaseParty
            // 
            this.comboBoxBaseParty.FormattingEnabled = true;
            this.comboBoxBaseParty.Items.AddRange(new object[] {
            "CBA",
            "Party1",
            "Party2"});
            this.comboBoxBaseParty.Location = new System.Drawing.Point(173, 86);
            this.comboBoxBaseParty.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxBaseParty.Name = "comboBoxBaseParty";
            this.comboBoxBaseParty.Size = new System.Drawing.Size(79, 24);
            this.comboBoxBaseParty.TabIndex = 55;
            this.comboBoxBaseParty.Text = "Party1";
            // 
            // comboBoxReportingCurrency
            // 
            this.comboBoxReportingCurrency.FormattingEnabled = true;
            this.comboBoxReportingCurrency.Items.AddRange(new object[] {
            "AUD",
            "USD",
            "GBP",
            "EUR",
            "CHF"});
            this.comboBoxReportingCurrency.Location = new System.Drawing.Point(173, 52);
            this.comboBoxReportingCurrency.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.comboBoxReportingCurrency.Name = "comboBoxReportingCurrency";
            this.comboBoxReportingCurrency.Size = new System.Drawing.Size(79, 24);
            this.comboBoxReportingCurrency.TabIndex = 54;
            this.comboBoxReportingCurrency.Text = "AUD";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(20, 55);
            this.label18.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(133, 17);
            this.label18.TabIndex = 42;
            this.label18.Text = "Reporting currency:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(20, 21);
            this.label17.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(76, 17);
            this.label17.TabIndex = 38;
            this.label17.Text = "Base date:";
            // 
            // dtpBaseDate
            // 
            this.dtpBaseDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBaseDate.Location = new System.Drawing.Point(123, 18);
            this.dtpBaseDate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dtpBaseDate.Name = "dtpBaseDate";
            this.dtpBaseDate.Size = new System.Drawing.Size(129, 22);
            this.dtpBaseDate.TabIndex = 39;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-5, 126);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(132, 17);
            this.label4.TabIndex = 50;
            this.label4.Text = "Client Name Space:";
            // 
            // cbClientNameSpace
            // 
            this.cbClientNameSpace.FormattingEnabled = true;
            this.cbClientNameSpace.Items.AddRange(new object[] {
            "Highlander.V5r3",
            "Highlander",
            "FpML47"});
            this.cbClientNameSpace.Location = new System.Drawing.Point(123, 119);
            this.cbClientNameSpace.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbClientNameSpace.Name = "cbClientNameSpace";
            this.cbClientNameSpace.Size = new System.Drawing.Size(129, 24);
            this.cbClientNameSpace.TabIndex = 56;
            this.cbClientNameSpace.Text = "Highlander.V5r3";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(755, 108);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(139, 41);
            this.button1.TabIndex = 69;
            this.button1.Text = "Stop Auto Value";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.BtnStopBuildClick);
            // 
            // btnAutoValue
            // 
            this.btnAutoValue.Location = new System.Drawing.Point(608, 108);
            this.btnAutoValue.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnAutoValue.Name = "btnAutoValue";
            this.btnAutoValue.Size = new System.Drawing.Size(139, 41);
            this.btnAutoValue.TabIndex = 68;
            this.btnAutoValue.Text = "Start Auto Value";
            this.btnAutoValue.UseVisualStyleBackColor = true;
            this.btnAutoValue.Click += new System.EventHandler(this.BtnAutoValueClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(799, 64);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 47;
            this.label2.Text = "hours";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(609, 64);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 17);
            this.label3.TabIndex = 48;
            this.label3.Text = "Retain results for";
            // 
            // bnTradeHistoricalValuation
            // 
            this.bnTradeHistoricalValuation.Location = new System.Drawing.Point(1272, 112);
            this.bnTradeHistoricalValuation.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.bnTradeHistoricalValuation.Name = "bnTradeHistoricalValuation";
            this.bnTradeHistoricalValuation.Size = new System.Drawing.Size(300, 37);
            this.bnTradeHistoricalValuation.TabIndex = 67;
            this.bnTradeHistoricalValuation.Text = "Value Selected Portfolio: Historical";
            this.bnTradeHistoricalValuation.UseVisualStyleBackColor = true;
            this.bnTradeHistoricalValuation.Click += new System.EventHandler(this.BtnTradeHistoricalValuation);
            // 
            // nudRetainResultsHours
            // 
            this.nudRetainResultsHours.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudRetainResultsHours.Location = new System.Drawing.Point(732, 62);
            this.nudRetainResultsHours.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.nudRetainResultsHours.Maximum = new decimal(new int[] {
            72,
            0,
            0,
            0});
            this.nudRetainResultsHours.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRetainResultsHours.Name = "nudRetainResultsHours";
            this.nudRetainResultsHours.Size = new System.Drawing.Size(49, 22);
            this.nudRetainResultsHours.TabIndex = 46;
            this.nudRetainResultsHours.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // btnTradeCalcValuations
            // 
            this.btnTradeCalcValuations.Location = new System.Drawing.Point(941, 110);
            this.btnTradeCalcValuations.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTradeCalcValuations.Name = "btnTradeCalcValuations";
            this.btnTradeCalcValuations.Size = new System.Drawing.Size(259, 41);
            this.btnTradeCalcValuations.TabIndex = 3;
            this.btnTradeCalcValuations.Text = "Value Selected Portfolio: Stress";
            this.btnTradeCalcValuations.UseVisualStyleBackColor = true;
            this.btnTradeCalcValuations.Click += new System.EventHandler(this.BtnTradeCalcValuationsClick);
            // 
            // chkRunAtServer
            // 
            this.chkRunAtServer.AutoSize = true;
            this.chkRunAtServer.Location = new System.Drawing.Point(608, 27);
            this.chkRunAtServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.chkRunAtServer.Name = "chkRunAtServer";
            this.chkRunAtServer.Size = new System.Drawing.Size(225, 21);
            this.chkRunAtServer.TabIndex = 49;
            this.chkRunAtServer.Text = "Process request on server/grid";
            this.chkRunAtServer.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbMarketQREODDated);
            this.groupBox2.Controls.Add(this.rbMarketQREODLatest);
            this.groupBox2.Controls.Add(this.rbMarketQRLive);
            this.groupBox2.Location = new System.Drawing.Point(8, 15);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox2.Size = new System.Drawing.Size(273, 142);
            this.groupBox2.TabIndex = 44;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Market";
            // 
            // rbMarketQREODDated
            // 
            this.rbMarketQREODDated.AutoSize = true;
            this.rbMarketQREODDated.Location = new System.Drawing.Point(8, 80);
            this.rbMarketQREODDated.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbMarketQREODDated.Name = "rbMarketQREODDated";
            this.rbMarketQREODDated.Size = new System.Drawing.Size(124, 21);
            this.rbMarketQREODDated.TabIndex = 2;
            this.rbMarketQREODDated.Text = "QR EOD dated";
            this.rbMarketQREODDated.UseVisualStyleBackColor = true;
            // 
            // rbMarketQREODLatest
            // 
            this.rbMarketQREODLatest.AutoSize = true;
            this.rbMarketQREODLatest.Location = new System.Drawing.Point(8, 52);
            this.rbMarketQREODLatest.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbMarketQREODLatest.Name = "rbMarketQREODLatest";
            this.rbMarketQREODLatest.Size = new System.Drawing.Size(127, 21);
            this.rbMarketQREODLatest.TabIndex = 1;
            this.rbMarketQREODLatest.Text = "QR EOD Latest";
            this.rbMarketQREODLatest.UseVisualStyleBackColor = true;
            // 
            // rbMarketQRLive
            // 
            this.rbMarketQRLive.AutoSize = true;
            this.rbMarketQRLive.Checked = true;
            this.rbMarketQRLive.Location = new System.Drawing.Point(8, 23);
            this.rbMarketQRLive.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.rbMarketQRLive.Name = "rbMarketQRLive";
            this.rbMarketQRLive.Size = new System.Drawing.Size(196, 21);
            this.rbMarketQRLive.TabIndex = 0;
            this.rbMarketQRLive.TabStop = true;
            this.rbMarketQRLive.Text = "QR Live (Bloomberg/MDS)";
            this.rbMarketQRLive.UseVisualStyleBackColor = true;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpTradeAll);
            this.tabControl1.Controls.Add(this.tpTradeSel);
            this.tabControl1.Controls.Add(this.tpCurveRaw);
            this.tabControl1.Controls.Add(this.tpValueRaw);
            this.tabControl1.Controls.Add(this.tpPortfolios);
            this.tabControl1.Controls.Add(this.tpRequests);
            this.tabControl1.Controls.Add(this.tpDebugLog);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 167);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1875, 811);
            this.tabControl1.TabIndex = 2;
            // 
            // tpTradeAll
            // 
            this.tpTradeAll.Controls.Add(this.groupBox4);
            this.tpTradeAll.Location = new System.Drawing.Point(4, 25);
            this.tpTradeAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpTradeAll.Name = "tpTradeAll";
            this.tpTradeAll.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpTradeAll.Size = new System.Drawing.Size(1867, 782);
            this.tpTradeAll.TabIndex = 5;
            this.tpTradeAll.Text = "Available Trades";
            this.tpTradeAll.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lvTradeAll);
            this.groupBox4.Controls.Add(this.panel4);
            this.groupBox4.Controls.Add(this.pnlTradeAll);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(4, 4);
            this.groupBox4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox4.Size = new System.Drawing.Size(1859, 774);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Available Trades";
            // 
            // lvTradeAll
            // 
            this.lvTradeAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTradeAll.FullRowSelect = true;
            this.lvTradeAll.HideSelection = false;
            this.lvTradeAll.Location = new System.Drawing.Point(4, 58);
            this.lvTradeAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvTradeAll.Name = "lvTradeAll";
            this.lvTradeAll.Size = new System.Drawing.Size(1851, 673);
            this.lvTradeAll.TabIndex = 10;
            this.lvTradeAll.UseCompatibleStateImageBehavior = false;
            this.lvTradeAll.DoubleClick += new System.EventHandler(this.LvTradeAllDoubleClick1);
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.btnTradeAddAll);
            this.panel4.Controls.Add(this.btnTradeAddSelected);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel4.Location = new System.Drawing.Point(4, 731);
            this.panel4.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1851, 39);
            this.panel4.TabIndex = 9;
            // 
            // btnTradeAddAll
            // 
            this.btnTradeAddAll.Location = new System.Drawing.Point(4, 7);
            this.btnTradeAddAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTradeAddAll.Name = "btnTradeAddAll";
            this.btnTradeAddAll.Size = new System.Drawing.Size(227, 28);
            this.btnTradeAddAll.TabIndex = 1;
            this.btnTradeAddAll.Text = "Add all to portfolio";
            this.btnTradeAddAll.UseVisualStyleBackColor = true;
            this.btnTradeAddAll.Click += new System.EventHandler(this.BtnTradeAddAllClick1);
            // 
            // btnTradeAddSelected
            // 
            this.btnTradeAddSelected.Location = new System.Drawing.Point(239, 7);
            this.btnTradeAddSelected.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTradeAddSelected.Name = "btnTradeAddSelected";
            this.btnTradeAddSelected.Size = new System.Drawing.Size(227, 28);
            this.btnTradeAddSelected.TabIndex = 0;
            this.btnTradeAddSelected.Text = "Add selected to portfolio";
            this.btnTradeAddSelected.UseVisualStyleBackColor = true;
            this.btnTradeAddSelected.Click += new System.EventHandler(this.BtnTradeAddSelectedClick1);
            // 
            // pnlTradeAll
            // 
            this.pnlTradeAll.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTradeAll.Location = new System.Drawing.Point(4, 19);
            this.pnlTradeAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlTradeAll.Name = "pnlTradeAll";
            this.pnlTradeAll.Size = new System.Drawing.Size(1851, 39);
            this.pnlTradeAll.TabIndex = 6;
            // 
            // tpTradeSel
            // 
            this.tpTradeSel.Controls.Add(this.groupBox5);
            this.tpTradeSel.Location = new System.Drawing.Point(4, 25);
            this.tpTradeSel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpTradeSel.Name = "tpTradeSel";
            this.tpTradeSel.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpTradeSel.Size = new System.Drawing.Size(1867, 782);
            this.tpTradeSel.TabIndex = 6;
            this.tpTradeSel.Text = "Portfolio trades";
            this.tpTradeSel.UseVisualStyleBackColor = true;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.lvTradeSel);
            this.groupBox5.Controls.Add(this.panel5);
            this.groupBox5.Controls.Add(this.pnlTradeSel);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(4, 4);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.groupBox5.Size = new System.Drawing.Size(1859, 774);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Selected Trades";
            // 
            // lvTradeSel
            // 
            this.lvTradeSel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvTradeSel.FullRowSelect = true;
            this.lvTradeSel.HideSelection = false;
            this.lvTradeSel.Location = new System.Drawing.Point(4, 58);
            this.lvTradeSel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvTradeSel.Name = "lvTradeSel";
            this.lvTradeSel.Size = new System.Drawing.Size(1851, 673);
            this.lvTradeSel.TabIndex = 10;
            this.lvTradeSel.UseCompatibleStateImageBehavior = false;
            this.lvTradeSel.DoubleClick += new System.EventHandler(this.LvTradeSelDoubleClick);
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.btnTradeClearAll);
            this.panel5.Controls.Add(this.btnTradeRemoveSelected);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(4, 731);
            this.panel5.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1851, 39);
            this.panel5.TabIndex = 9;
            // 
            // btnTradeClearAll
            // 
            this.btnTradeClearAll.Location = new System.Drawing.Point(239, 7);
            this.btnTradeClearAll.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTradeClearAll.Name = "btnTradeClearAll";
            this.btnTradeClearAll.Size = new System.Drawing.Size(227, 28);
            this.btnTradeClearAll.TabIndex = 2;
            this.btnTradeClearAll.Text = "Remove all trades";
            this.btnTradeClearAll.UseVisualStyleBackColor = true;
            this.btnTradeClearAll.Click += new System.EventHandler(this.BtnTradeClearAllClick1);
            // 
            // btnTradeRemoveSelected
            // 
            this.btnTradeRemoveSelected.Location = new System.Drawing.Point(4, 7);
            this.btnTradeRemoveSelected.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnTradeRemoveSelected.Name = "btnTradeRemoveSelected";
            this.btnTradeRemoveSelected.Size = new System.Drawing.Size(227, 28);
            this.btnTradeRemoveSelected.TabIndex = 1;
            this.btnTradeRemoveSelected.Text = "Remove selected trades";
            this.btnTradeRemoveSelected.UseVisualStyleBackColor = true;
            this.btnTradeRemoveSelected.Click += new System.EventHandler(this.BtnTradeRemoveSelectedClick1);
            // 
            // pnlTradeSel
            // 
            this.pnlTradeSel.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTradeSel.Location = new System.Drawing.Point(4, 19);
            this.pnlTradeSel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlTradeSel.Name = "pnlTradeSel";
            this.pnlTradeSel.Size = new System.Drawing.Size(1851, 39);
            this.pnlTradeSel.TabIndex = 6;
            // 
            // tpCurveRaw
            // 
            this.tpCurveRaw.Controls.Add(this.lvCurveRaw);
            this.tpCurveRaw.Controls.Add(this.pnlCurveRaw);
            this.tpCurveRaw.Location = new System.Drawing.Point(4, 25);
            this.tpCurveRaw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpCurveRaw.Name = "tpCurveRaw";
            this.tpCurveRaw.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpCurveRaw.Size = new System.Drawing.Size(1867, 782);
            this.tpCurveRaw.TabIndex = 10;
            this.tpCurveRaw.Text = "Curves";
            this.tpCurveRaw.UseVisualStyleBackColor = true;
            // 
            // lvCurveRaw
            // 
            this.lvCurveRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvCurveRaw.HideSelection = false;
            this.lvCurveRaw.Location = new System.Drawing.Point(4, 41);
            this.lvCurveRaw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvCurveRaw.Name = "lvCurveRaw";
            this.lvCurveRaw.Size = new System.Drawing.Size(1859, 737);
            this.lvCurveRaw.TabIndex = 9;
            this.lvCurveRaw.UseCompatibleStateImageBehavior = false;
            // 
            // pnlCurveRaw
            // 
            this.pnlCurveRaw.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlCurveRaw.Location = new System.Drawing.Point(4, 4);
            this.pnlCurveRaw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.pnlCurveRaw.Name = "pnlCurveRaw";
            this.pnlCurveRaw.Size = new System.Drawing.Size(1859, 37);
            this.pnlCurveRaw.TabIndex = 9;
            // 
            // tpValueRaw
            // 
            this.tpValueRaw.Controls.Add(this.lvValueRaw);
            this.tpValueRaw.Controls.Add(this.panelValueRaw);
            this.tpValueRaw.Location = new System.Drawing.Point(4, 25);
            this.tpValueRaw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpValueRaw.Name = "tpValueRaw";
            this.tpValueRaw.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpValueRaw.Size = new System.Drawing.Size(1867, 782);
            this.tpValueRaw.TabIndex = 4;
            this.tpValueRaw.Text = "Valuations";
            this.tpValueRaw.UseVisualStyleBackColor = true;
            // 
            // lvValueRaw
            // 
            this.lvValueRaw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvValueRaw.HideSelection = false;
            this.lvValueRaw.Location = new System.Drawing.Point(4, 41);
            this.lvValueRaw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvValueRaw.Name = "lvValueRaw";
            this.lvValueRaw.Size = new System.Drawing.Size(1859, 737);
            this.lvValueRaw.TabIndex = 9;
            this.lvValueRaw.UseCompatibleStateImageBehavior = false;
            // 
            // panelValueRaw
            // 
            this.panelValueRaw.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelValueRaw.Location = new System.Drawing.Point(4, 4);
            this.panelValueRaw.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelValueRaw.Name = "panelValueRaw";
            this.panelValueRaw.Size = new System.Drawing.Size(1859, 37);
            this.panelValueRaw.TabIndex = 8;
            // 
            // tpPortfolios
            // 
            this.tpPortfolios.Controls.Add(this.lvPortfolio);
            this.tpPortfolios.Controls.Add(this.panelPortfolio);
            this.tpPortfolios.Location = new System.Drawing.Point(4, 25);
            this.tpPortfolios.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpPortfolios.Name = "tpPortfolios";
            this.tpPortfolios.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpPortfolios.Size = new System.Drawing.Size(1867, 782);
            this.tpPortfolios.TabIndex = 9;
            this.tpPortfolios.Text = "Portfolios";
            this.tpPortfolios.UseVisualStyleBackColor = true;
            // 
            // lvPortfolio
            // 
            this.lvPortfolio.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvPortfolio.HideSelection = false;
            this.lvPortfolio.Location = new System.Drawing.Point(4, 41);
            this.lvPortfolio.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvPortfolio.Name = "lvPortfolio";
            this.lvPortfolio.Size = new System.Drawing.Size(1859, 737);
            this.lvPortfolio.TabIndex = 15;
            this.lvPortfolio.UseCompatibleStateImageBehavior = false;
            // 
            // panelPortfolio
            // 
            this.panelPortfolio.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelPortfolio.Location = new System.Drawing.Point(4, 4);
            this.panelPortfolio.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelPortfolio.Name = "panelPortfolio";
            this.panelPortfolio.Size = new System.Drawing.Size(1859, 37);
            this.panelPortfolio.TabIndex = 14;
            // 
            // tpRequests
            // 
            this.tpRequests.Controls.Add(this.lvProgress);
            this.tpRequests.Controls.Add(this.panelProgress);
            this.tpRequests.Location = new System.Drawing.Point(4, 25);
            this.tpRequests.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpRequests.Name = "tpRequests";
            this.tpRequests.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpRequests.Size = new System.Drawing.Size(1867, 782);
            this.tpRequests.TabIndex = 8;
            this.tpRequests.Text = "Requests";
            this.tpRequests.UseVisualStyleBackColor = true;
            // 
            // lvProgress
            // 
            this.lvProgress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvProgress.HideSelection = false;
            this.lvProgress.Location = new System.Drawing.Point(4, 41);
            this.lvProgress.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lvProgress.Name = "lvProgress";
            this.lvProgress.Size = new System.Drawing.Size(1859, 737);
            this.lvProgress.TabIndex = 13;
            this.lvProgress.UseCompatibleStateImageBehavior = false;
            // 
            // panelProgress
            // 
            this.panelProgress.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelProgress.Location = new System.Drawing.Point(4, 4);
            this.panelProgress.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panelProgress.Name = "panelProgress";
            this.panelProgress.Size = new System.Drawing.Size(1859, 37);
            this.panelProgress.TabIndex = 12;
            // 
            // tpDebugLog
            // 
            this.tpDebugLog.Controls.Add(this.txtLog);
            this.tpDebugLog.Location = new System.Drawing.Point(4, 25);
            this.tpDebugLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpDebugLog.Name = "tpDebugLog";
            this.tpDebugLog.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tpDebugLog.Size = new System.Drawing.Size(1867, 782);
            this.tpDebugLog.TabIndex = 2;
            this.tpDebugLog.Text = "Log";
            this.tpDebugLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(4, 4);
            this.txtLog.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1859, 774);
            this.txtLog.TabIndex = 0;
            // 
            // PortfolioValuerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1875, 978);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "PortfolioValuerForm";
            this.Text = "Portfolio Valuer Test Harness";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRetainResultsHours)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tpTradeAll.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.tpTradeSel.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.tpCurveRaw.ResumeLayout(false);
            this.tpValueRaw.ResumeLayout(false);
            this.tpPortfolios.ResumeLayout(false);
            this.tpRequests.ResumeLayout(false);
            this.tpDebugLog.ResumeLayout(false);
            this.tpDebugLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnTradeCalcValuations;
        private System.Windows.Forms.DateTimePicker dtpBaseDate;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpValueRaw;
        private System.Windows.Forms.ListView lvValueRaw;
        private System.Windows.Forms.Panel panelValueRaw;
        private System.Windows.Forms.TabPage tpDebugLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabPage tpTradeAll;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView lvTradeAll;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Button btnTradeAddAll;
        private System.Windows.Forms.Button btnTradeAddSelected;
        private System.Windows.Forms.Panel pnlTradeAll;
        private System.Windows.Forms.TabPage tpTradeSel;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.ListView lvTradeSel;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.Button btnTradeClearAll;
        private System.Windows.Forms.Button btnTradeRemoveSelected;
        private System.Windows.Forms.Panel pnlTradeSel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton rbMarketSydneyLive;
        private System.Windows.Forms.RadioButton rbMarketQREODDated;
        private System.Windows.Forms.RadioButton rbMarketQREODLatest;
        private System.Windows.Forms.RadioButton rbMarketQRLive;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.CheckBox chkCalcAllFXStresses;
        private System.Windows.Forms.CheckBox chkCalcAllIRStresses;
        private System.Windows.Forms.CheckBox chkRunAtServer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudRetainResultsHours;
        private System.Windows.Forms.TabPage tpRequests;
        private System.Windows.Forms.ListView lvProgress;
        private System.Windows.Forms.Panel panelProgress;
        private System.Windows.Forms.TabPage tpPortfolios;
        private System.Windows.Forms.ListView lvPortfolio;
        private System.Windows.Forms.Panel panelPortfolio;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBoxReportingCurrency;
        private System.Windows.Forms.ComboBox comboBoxBaseParty;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbClientNameSpace;
        private System.Windows.Forms.Button bnTradeHistoricalValuation;
        private System.Windows.Forms.Button btnAutoValue;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TabPage tpCurveRaw;
        private System.Windows.Forms.ListView lvCurveRaw;
        private System.Windows.Forms.Panel pnlCurveRaw;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.CheckBox cbMarketDateEnd;
        private System.Windows.Forms.DateTimePicker dtpMarketEndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dtpMarketQRDate;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox1;
    }
}

