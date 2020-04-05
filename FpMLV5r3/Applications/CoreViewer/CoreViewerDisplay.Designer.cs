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

namespace Highlander.Core.Viewer.V5r3
{
    partial class CoreViewerForm
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
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewPropertiesStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewXmlStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewMarketStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.valueStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createTradeStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.nameSpaceTextBox1 = new System.Windows.Forms.TextBox();
            this.btnLoadConfigData = new System.Windows.Forms.Button();
            this.groupBoxValuation = new System.Windows.Forms.GroupBox();
            this.listBoxMetrics = new System.Windows.Forms.ListBox();
            this.lblMetrics = new System.Windows.Forms.Label();
            this.comboBoxParty = new System.Windows.Forms.ComboBox();
            this.txtBoxTradeDirectory = new System.Windows.Forms.TextBox();
            this.comboBoxMarket = new System.Windows.Forms.ComboBox();
            this.comboBoxCurrency = new System.Windows.Forms.ComboBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.dateTimePickerValuation = new System.Windows.Forms.DateTimePicker();
            this.btnLoadTrade = new System.Windows.Forms.Button();
            this.chkDebugRequests = new System.Windows.Forms.CheckBox();
            this.btnUnsubscribe = new System.Windows.Forms.Button();
            this.btnSubscribe = new System.Windows.Forms.Button();
            this.btnEventLogClear = new System.Windows.Forms.Button();
            this.btnDeleteObjects = new System.Windows.Forms.Button();
            this.btnClearAll = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.txtPropItemNameValue = new System.Windows.Forms.TextBox();
            this.chkPropItemName = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cbDataTypeValues = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtProp3Name = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtProp2Name = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.btnLoadObjects = new System.Windows.Forms.Button();
            this.txtProp4Value = new System.Windows.Forms.TextBox();
            this.txtProp4Name = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkProp4 = new System.Windows.Forms.CheckBox();
            this.txtProp3Value = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkProp3 = new System.Windows.Forms.CheckBox();
            this.txtProp2Value = new System.Windows.Forms.TextBox();
            this.chkProp2 = new System.Windows.Forms.CheckBox();
            this.label13 = new System.Windows.Forms.Label();
            this.chkProp1 = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.pnlRuntimeState = new System.Windows.Forms.TextBox();
            this.txtSpecificServers = new System.Windows.Forms.TextBox();
            this.rbSpecificServer = new System.Windows.Forms.RadioButton();
            this.rbLocalhost = new System.Windows.Forms.RadioButton();
            this.rbDefaultServers = new System.Windows.Forms.RadioButton();
            this.cbEnvId = new System.Windows.Forms.ComboBox();
            this.btnRestart = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.treeNavigation = new System.Windows.Forms.TreeView();
            this.txtNavDetail = new System.Windows.Forms.TextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.createTradeBtn = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBoxValuation.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.saveToFileToolStripMenuItem,
            this.viewPropertiesStripMenuItem,
            this.viewXmlStripMenuItem,
            this.viewMarketStripMenuItem,
            this.valueStripMenuItem,
            this.createTradeStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(165, 158);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.ContextMenuStrip1Opening);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(164, 22);
            this.toolStripMenuItem1.Text = "Delete object ...";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.ToolStripMenuItem1Click);
            // 
            // saveToFileToolStripMenuItem
            // 
            this.saveToFileToolStripMenuItem.Name = "saveToFileToolStripMenuItem";
            this.saveToFileToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.saveToFileToolStripMenuItem.Text = "Save to File...";
            this.saveToFileToolStripMenuItem.Click += new System.EventHandler(this.StripMenuItemSaveToFileToolClick);
            // 
            // viewPropertiesStripMenuItem
            // 
            this.viewPropertiesStripMenuItem.Name = "viewPropertiesStripMenuItem";
            this.viewPropertiesStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.viewPropertiesStripMenuItem.Text = "View Properties...";
            this.viewPropertiesStripMenuItem.Click += new System.EventHandler(this.StripMenuItemViewPropertiesClick);
            // 
            // viewXmlStripMenuItem
            // 
            this.viewXmlStripMenuItem.Name = "viewXmlStripMenuItem";
            this.viewXmlStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.viewXmlStripMenuItem.Text = "View Xml...";
            this.viewXmlStripMenuItem.Click += new System.EventHandler(this.StripMenuItemViewXmlClick);
            // 
            // viewMarketStripMenuItem
            // 
            this.viewMarketStripMenuItem.Name = "viewMarketStripMenuItem";
            this.viewMarketStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.viewMarketStripMenuItem.Text = "View Market...";
            this.viewMarketStripMenuItem.Click += new System.EventHandler(this.StripMenuItemViewMarketClick);
            // 
            // valueStripMenuItem
            // 
            this.valueStripMenuItem.Name = "valueStripMenuItem";
            this.valueStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.valueStripMenuItem.Text = "Value Trade...";
            this.valueStripMenuItem.Click += new System.EventHandler(this.StripMenuItemValueClick);
            // 
            // createTradeStripMenuItem
            // 
            this.createTradeStripMenuItem.Name = "createTradeStripMenuItem";
            this.createTradeStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.createTradeStripMenuItem.Text = "Create Trade...";
            this.createTradeStripMenuItem.Click += new System.EventHandler(this.StripMenuItemCloneTradeClick);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox5);
            this.panel1.Controls.Add(this.groupBox4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1312, 196);
            this.panel1.TabIndex = 25;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.nameSpaceTextBox1);
            this.groupBox5.Controls.Add(this.btnLoadConfigData);
            this.groupBox5.Controls.Add(this.groupBoxValuation);
            this.groupBox5.Controls.Add(this.btnLoadTrade);
            this.groupBox5.Controls.Add(this.chkDebugRequests);
            this.groupBox5.Controls.Add(this.btnUnsubscribe);
            this.groupBox5.Controls.Add(this.btnSubscribe);
            this.groupBox5.Controls.Add(this.btnEventLogClear);
            this.groupBox5.Controls.Add(this.btnDeleteObjects);
            this.groupBox5.Controls.Add(this.btnClearAll);
            this.groupBox5.Controls.Add(this.label11);
            this.groupBox5.Controls.Add(this.txtPropItemNameValue);
            this.groupBox5.Controls.Add(this.chkPropItemName);
            this.groupBox5.Controls.Add(this.label10);
            this.groupBox5.Controls.Add(this.cbDataTypeValues);
            this.groupBox5.Controls.Add(this.label8);
            this.groupBox5.Controls.Add(this.label7);
            this.groupBox5.Controls.Add(this.label3);
            this.groupBox5.Controls.Add(this.txtProp3Name);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Controls.Add(this.txtProp2Name);
            this.groupBox5.Controls.Add(this.label5);
            this.groupBox5.Controls.Add(this.btnLoadObjects);
            this.groupBox5.Controls.Add(this.txtProp4Value);
            this.groupBox5.Controls.Add(this.txtProp4Name);
            this.groupBox5.Controls.Add(this.label2);
            this.groupBox5.Controls.Add(this.chkProp4);
            this.groupBox5.Controls.Add(this.txtProp3Value);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Controls.Add(this.chkProp3);
            this.groupBox5.Controls.Add(this.txtProp2Value);
            this.groupBox5.Controls.Add(this.chkProp2);
            this.groupBox5.Controls.Add(this.label13);
            this.groupBox5.Controls.Add(this.chkProp1);
            this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox5.Location = new System.Drawing.Point(184, 0);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(1128, 196);
            this.groupBox5.TabIndex = 34;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Query definition";
            //this.groupBox5.Enter += new System.EventHandler(this.groupBox5_Enter);
            // 
            // nameSpaceTextBox1
            // 
            this.nameSpaceTextBox1.Location = new System.Drawing.Point(464, 16);
            this.nameSpaceTextBox1.Name = "nameSpaceTextBox1";
            this.nameSpaceTextBox1.Size = new System.Drawing.Size(106, 20);
            this.nameSpaceTextBox1.TabIndex = 69;
            this.nameSpaceTextBox1.Text = "Highlander.V5r3";
            // 
            // btnLoadConfigData
            // 
            this.btnLoadConfigData.Location = new System.Drawing.Point(580, 10);
            this.btnLoadConfigData.Name = "btnLoadConfigData";
            this.btnLoadConfigData.Size = new System.Drawing.Size(115, 33);
            this.btnLoadConfigData.TabIndex = 68;
            this.btnLoadConfigData.Text = "Load Configurations";
            this.btnLoadConfigData.UseVisualStyleBackColor = true;
            this.btnLoadConfigData.Click += new System.EventHandler(this.BtnLoadDataClick);
            // 
            // groupBoxValuation
            // 
            this.groupBoxValuation.Controls.Add(this.createTradeBtn);
            this.groupBoxValuation.Controls.Add(this.listBoxMetrics);
            this.groupBoxValuation.Controls.Add(this.lblMetrics);
            this.groupBoxValuation.Controls.Add(this.comboBoxParty);
            this.groupBoxValuation.Controls.Add(this.txtBoxTradeDirectory);
            this.groupBoxValuation.Controls.Add(this.comboBoxMarket);
            this.groupBoxValuation.Controls.Add(this.comboBoxCurrency);
            this.groupBoxValuation.Controls.Add(this.label14);
            this.groupBoxValuation.Controls.Add(this.label12);
            this.groupBoxValuation.Controls.Add(this.label9);
            this.groupBoxValuation.Controls.Add(this.label4);
            this.groupBoxValuation.Controls.Add(this.dateTimePickerValuation);
            this.groupBoxValuation.Location = new System.Drawing.Point(706, 0);
            this.groupBoxValuation.Name = "groupBoxValuation";
            this.groupBoxValuation.Size = new System.Drawing.Size(419, 190);
            this.groupBoxValuation.TabIndex = 67;
            this.groupBoxValuation.TabStop = false;
            this.groupBoxValuation.Text = "Trade Valuation";
            // 
            // listBoxMetrics
            // 
            this.listBoxMetrics.AllowDrop = true;
            this.listBoxMetrics.FormattingEnabled = true;
            this.listBoxMetrics.Items.AddRange(new object[] {
            "DiscountFactorAtMaturity",
            "ImpliedQuote",
            "MarketQuote",
            "BreakEvenRate",
            "AnalyticalDelta",
            "Delta1",
            "Delta0",
            "DeltaR",
            "FloatingNPV",
            "AccrualFactor",
            "HistoricalAccrualFactor",
            "HistoricalDelta0",
            "HistoricalDeltaR",
            "ExpectedValue",
            "HistoricalValue",
            "NFV",
            "NPV",
            "RiskNPV",
            "SimpleCVA",
            "BucketedDelta1",
            "BucketedDeltaVector",
            "BucketedDeltaVector2",
            "HistoricalDelta1",
            "Delta1PDH",
            "Delta0PDH",
            "LocalCurrencyAnalyticalDelta",
            "LocalCurrencyDelta1",
            "LocalCurrencyDelta0",
            "LocalCurrencyDeltaR",
            "LocalCurrencyFloatingNPV",
            "LocalCurrencyAccrualFactor",
            "LocalCurrencyHistoricalAccrualFactor",
            "LocalCurrencyExpectedValue",
            "LocalCurrencyCalculatedValue",
            "LocalCurrencyHistoricalValue",
            "LocalCurrencyNFV",
            "LocalCurrencyNPV",
            "LocalCurrencySimpleCVA",
            "LocalCurrencyBucketedDelta1",
            "LocalCurrencyBucketedDeltaVector",
            "LocalCurrencyBucketedDeltaVector2",
            "LocalCurrencyHistoricalDelta1",
            "LocalCurrencyDelta1PDH",
            "LocalCurrencyDelta0PDH",
            "BreakEvenStrike",
            "PCE",
            "PCETerm",
            "AnalyticalGamma",
            "Gamma1",
            "Gamma0",
            "Delta0Delta1",
            "LocalCurrencyAnalyticalGamma",
            "LocalCurrencyGamma1",
            "LocalCurrencyGamma0",
            "LocalCurrencyDelta0Delta1"});
            this.listBoxMetrics.Location = new System.Drawing.Point(262, 17);
            this.listBoxMetrics.Name = "listBoxMetrics";
            this.listBoxMetrics.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxMetrics.Size = new System.Drawing.Size(151, 134);
            this.listBoxMetrics.TabIndex = 75;
            // 
            // lblMetrics
            // 
            this.lblMetrics.AutoSize = true;
            this.lblMetrics.Location = new System.Drawing.Point(291, 2);
            this.lblMetrics.Name = "lblMetrics";
            this.lblMetrics.Size = new System.Drawing.Size(41, 13);
            this.lblMetrics.TabIndex = 74;
            this.lblMetrics.Text = "Metrics";
            // 
            // comboBoxParty
            // 
            this.comboBoxParty.FormattingEnabled = true;
            this.comboBoxParty.Items.AddRange(new object[] {
            "CBA",
            "Party1",
            "Party2"});
            this.comboBoxParty.Location = new System.Drawing.Point(104, 124);
            this.comboBoxParty.Name = "comboBoxParty";
            this.comboBoxParty.Size = new System.Drawing.Size(141, 21);
            this.comboBoxParty.TabIndex = 73;
            this.comboBoxParty.Text = "CBA";
            // 
            // txtBoxTradeDirectory
            // 
            this.txtBoxTradeDirectory.Location = new System.Drawing.Point(6, 162);
            this.txtBoxTradeDirectory.Name = "txtBoxTradeDirectory";
            this.txtBoxTradeDirectory.Size = new System.Drawing.Size(250, 20);
            this.txtBoxTradeDirectory.TabIndex = 59;
            this.txtBoxTradeDirectory.Text = "c:\\Development\\";
            // 
            // comboBoxMarket
            // 
            this.comboBoxMarket.FormattingEnabled = true;
            this.comboBoxMarket.Items.AddRange(new object[] {
            "QR_LIVE",
            "TEST_EOD"});
            this.comboBoxMarket.Location = new System.Drawing.Point(106, 89);
            this.comboBoxMarket.Name = "comboBoxMarket";
            this.comboBoxMarket.Size = new System.Drawing.Size(139, 21);
            this.comboBoxMarket.TabIndex = 72;
            this.comboBoxMarket.Text = "QR_LIVE";
            // 
            // comboBoxCurrency
            // 
            this.comboBoxCurrency.FormattingEnabled = true;
            this.comboBoxCurrency.Items.AddRange(new object[] {
            "AUD",
            "EUR",
            "GBP",
            "USD",
            "JPY",
            "NZD"});
            this.comboBoxCurrency.Location = new System.Drawing.Point(175, 55);
            this.comboBoxCurrency.Name = "comboBoxCurrency";
            this.comboBoxCurrency.Size = new System.Drawing.Size(70, 21);
            this.comboBoxCurrency.TabIndex = 71;
            this.comboBoxCurrency.Text = "AUD";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(13, 129);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(58, 13);
            this.label14.TabIndex = 67;
            this.label14.Text = "Base Party";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(11, 27);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 13);
            this.label12.TabIndex = 66;
            this.label12.Text = "Valuation";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(11, 59);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(98, 13);
            this.label9.TabIndex = 65;
            this.label9.Text = "Reporting Currency";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 64;
            this.label4.Text = "Market";
            // 
            // dateTimePickerValuation
            // 
            this.dateTimePickerValuation.Location = new System.Drawing.Point(65, 24);
            this.dateTimePickerValuation.Name = "dateTimePickerValuation";
            this.dateTimePickerValuation.Size = new System.Drawing.Size(180, 20);
            this.dateTimePickerValuation.TabIndex = 61;
            // 
            // btnLoadTrade
            // 
            this.btnLoadTrade.Location = new System.Drawing.Point(580, 159);
            this.btnLoadTrade.Name = "btnLoadTrade";
            this.btnLoadTrade.Size = new System.Drawing.Size(115, 31);
            this.btnLoadTrade.TabIndex = 60;
            this.btnLoadTrade.Text = "Load Trade";
            this.btnLoadTrade.UseVisualStyleBackColor = true;
            this.btnLoadTrade.Click += new System.EventHandler(this.BtnLoadTradeClick);
            // 
            // chkDebugRequests
            // 
            this.chkDebugRequests.AutoSize = true;
            this.chkDebugRequests.Location = new System.Drawing.Point(464, 162);
            this.chkDebugRequests.Name = "chkDebugRequests";
            this.chkDebugRequests.Size = new System.Drawing.Size(106, 17);
            this.chkDebugRequests.TabIndex = 58;
            this.chkDebugRequests.Text = "Debug Requests";
            this.chkDebugRequests.UseVisualStyleBackColor = true;
            // 
            // btnUnsubscribe
            // 
            this.btnUnsubscribe.Location = new System.Drawing.Point(580, 84);
            this.btnUnsubscribe.Name = "btnUnsubscribe";
            this.btnUnsubscribe.Size = new System.Drawing.Size(115, 33);
            this.btnUnsubscribe.TabIndex = 57;
            this.btnUnsubscribe.Text = "Stop Subscriptions";
            this.btnUnsubscribe.UseVisualStyleBackColor = true;
            this.btnUnsubscribe.Click += new System.EventHandler(this.BtnUnsubscribeClick);
            // 
            // btnSubscribe
            // 
            this.btnSubscribe.Location = new System.Drawing.Point(464, 84);
            this.btnSubscribe.Name = "btnSubscribe";
            this.btnSubscribe.Size = new System.Drawing.Size(106, 33);
            this.btnSubscribe.TabIndex = 56;
            this.btnSubscribe.Text = "Start Subscription";
            this.btnSubscribe.UseVisualStyleBackColor = true;
            this.btnSubscribe.Click += new System.EventHandler(this.BtnSubscribeClick);
            // 
            // btnEventLogClear
            // 
            this.btnEventLogClear.Location = new System.Drawing.Point(464, 123);
            this.btnEventLogClear.Name = "btnEventLogClear";
            this.btnEventLogClear.Size = new System.Drawing.Size(106, 33);
            this.btnEventLogClear.TabIndex = 55;
            this.btnEventLogClear.Text = "Clear Log";
            this.btnEventLogClear.UseVisualStyleBackColor = true;
            this.btnEventLogClear.Click += new System.EventHandler(this.BtnEventLogClearClick1);
            // 
            // btnDeleteObjects
            // 
            this.btnDeleteObjects.Location = new System.Drawing.Point(580, 47);
            this.btnDeleteObjects.Name = "btnDeleteObjects";
            this.btnDeleteObjects.Size = new System.Drawing.Size(115, 33);
            this.btnDeleteObjects.TabIndex = 54;
            this.btnDeleteObjects.Text = "Delete Objects";
            this.btnDeleteObjects.UseVisualStyleBackColor = true;
            this.btnDeleteObjects.Click += new System.EventHandler(this.BtnDeleteObjectsClick1);
            // 
            // btnClearAll
            // 
            this.btnClearAll.Location = new System.Drawing.Point(580, 122);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new System.Drawing.Size(115, 32);
            this.btnClearAll.TabIndex = 40;
            this.btnClearAll.Text = "Clear Browser";
            this.btnClearAll.UseVisualStyleBackColor = true;
            this.btnClearAll.Click += new System.EventHandler(this.BtnClearAllClick);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(30, 164);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(106, 13);
            this.label11.TabIndex = 51;
            this.label11.Text = "Item name starts with";
            // 
            // txtPropItemNameValue
            // 
            this.txtPropItemNameValue.Location = new System.Drawing.Point(142, 161);
            this.txtPropItemNameValue.Name = "txtPropItemNameValue";
            this.txtPropItemNameValue.Size = new System.Drawing.Size(312, 20);
            this.txtPropItemNameValue.TabIndex = 50;
            this.txtPropItemNameValue.Text = "FpML.Configuration";
            // 
            // chkPropItemName
            // 
            this.chkPropItemName.AutoSize = true;
            this.chkPropItemName.Location = new System.Drawing.Point(11, 163);
            this.chkPropItemName.Name = "chkPropItemName";
            this.chkPropItemName.Size = new System.Drawing.Size(29, 17);
            this.chkPropItemName.TabIndex = 48;
            this.chkPropItemName.Text = ":";
            this.chkPropItemName.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(18, 148);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(25, 13);
            this.label10.TabIndex = 44;
            this.label10.Text = "and";
            // 
            // cbDataTypeValues
            // 
            this.cbDataTypeValues.FormattingEnabled = true;
            this.cbDataTypeValues.Location = new System.Drawing.Point(142, 17);
            this.cbDataTypeValues.Name = "cbDataTypeValues";
            this.cbDataTypeValues.Size = new System.Drawing.Size(312, 21);
            this.cbDataTypeValues.TabIndex = 42;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(54, 20);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 13);
            this.label8.TabIndex = 41;
            this.label8.Text = "Data Type";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(117, 128);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(19, 13);
            this.label7.TabIndex = 39;
            this.label7.Text = "==";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(117, 92);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(19, 13);
            this.label3.TabIndex = 38;
            this.label3.Text = "==";
            // 
            // txtProp3Name
            // 
            this.txtProp3Name.Location = new System.Drawing.Point(73, 89);
            this.txtProp3Name.Name = "txtProp3Name";
            this.txtProp3Name.Size = new System.Drawing.Size(41, 20);
            this.txtProp3Name.TabIndex = 37;
            this.txtProp3Name.Text = "Owner";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(117, 55);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(19, 13);
            this.label6.TabIndex = 36;
            this.label6.Text = "==";
            // 
            // txtProp2Name
            // 
            this.txtProp2Name.Location = new System.Drawing.Point(47, 52);
            this.txtProp2Name.Name = "txtProp2Name";
            this.txtProp2Name.Size = new System.Drawing.Size(67, 20);
            this.txtProp2Name.TabIndex = 35;
            this.txtProp2Name.Text = "ProductType";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(117, 20);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 13);
            this.label5.TabIndex = 34;
            this.label5.Text = "==";
            // 
            // btnLoadObjects
            // 
            this.btnLoadObjects.Location = new System.Drawing.Point(464, 46);
            this.btnLoadObjects.Name = "btnLoadObjects";
            this.btnLoadObjects.Size = new System.Drawing.Size(106, 33);
            this.btnLoadObjects.TabIndex = 32;
            this.btnLoadObjects.Text = "Load Objects";
            this.btnLoadObjects.UseVisualStyleBackColor = true;
            this.btnLoadObjects.Click += new System.EventHandler(this.BtnLoadObjectsClick);
            // 
            // txtProp4Value
            // 
            this.txtProp4Value.Location = new System.Drawing.Point(142, 125);
            this.txtProp4Value.Name = "txtProp4Value";
            this.txtProp4Value.Size = new System.Drawing.Size(312, 20);
            this.txtProp4Value.TabIndex = 12;
            this.txtProp4Value.Text = "AUDSwap";
            // 
            // txtProp4Name
            // 
            this.txtProp4Name.Location = new System.Drawing.Point(47, 124);
            this.txtProp4Name.Name = "txtProp4Name";
            this.txtProp4Name.Size = new System.Drawing.Size(67, 20);
            this.txtProp4Name.TabIndex = 10;
            this.txtProp4Name.Text = "IndexName";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 111);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "and";
            // 
            // chkProp4
            // 
            this.chkProp4.AutoSize = true;
            this.chkProp4.Location = new System.Drawing.Point(11, 127);
            this.chkProp4.Name = "chkProp4";
            this.chkProp4.Size = new System.Drawing.Size(29, 17);
            this.chkProp4.TabIndex = 8;
            this.chkProp4.Text = ":";
            this.chkProp4.UseVisualStyleBackColor = true;
            // 
            // txtProp3Value
            // 
            this.txtProp3Value.Location = new System.Drawing.Point(142, 89);
            this.txtProp3Value.Name = "txtProp3Value";
            this.txtProp3Value.Size = new System.Drawing.Size(312, 20);
            this.txtProp3Value.TabIndex = 7;
            this.txtProp3Value.Text = "SydSwapDesk";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(25, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "and";
            // 
            // chkProp3
            // 
            this.chkProp3.AutoSize = true;
            this.chkProp3.Location = new System.Drawing.Point(11, 91);
            this.chkProp3.Name = "chkProp3";
            this.chkProp3.Size = new System.Drawing.Size(29, 17);
            this.chkProp3.TabIndex = 5;
            this.chkProp3.Text = ":";
            this.chkProp3.UseVisualStyleBackColor = true;
            // 
            // txtProp2Value
            // 
            this.txtProp2Value.Location = new System.Drawing.Point(142, 52);
            this.txtProp2Value.Name = "txtProp2Value";
            this.txtProp2Value.Size = new System.Drawing.Size(312, 20);
            this.txtProp2Value.TabIndex = 4;
            this.txtProp2Value.Text = "Swap";
            // 
            // chkProp2
            // 
            this.chkProp2.AutoSize = true;
            this.chkProp2.Location = new System.Drawing.Point(11, 55);
            this.chkProp2.Name = "chkProp2";
            this.chkProp2.Size = new System.Drawing.Size(29, 17);
            this.chkProp2.TabIndex = 3;
            this.chkProp2.Text = ":";
            this.chkProp2.UseVisualStyleBackColor = true;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(18, 39);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(25, 13);
            this.label13.TabIndex = 2;
            this.label13.Text = "and";
            // 
            // chkProp1
            // 
            this.chkProp1.AutoSize = true;
            this.chkProp1.Checked = true;
            this.chkProp1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProp1.Location = new System.Drawing.Point(11, 19);
            this.chkProp1.Name = "chkProp1";
            this.chkProp1.Size = new System.Drawing.Size(29, 17);
            this.chkProp1.TabIndex = 0;
            this.chkProp1.Text = ":";
            this.chkProp1.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.pnlRuntimeState);
            this.groupBox4.Controls.Add(this.txtSpecificServers);
            this.groupBox4.Controls.Add(this.rbSpecificServer);
            this.groupBox4.Controls.Add(this.rbLocalhost);
            this.groupBox4.Controls.Add(this.rbDefaultServers);
            this.groupBox4.Controls.Add(this.cbEnvId);
            this.groupBox4.Controls.Add(this.btnRestart);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Left;
            this.groupBox4.Location = new System.Drawing.Point(0, 0);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(184, 196);
            this.groupBox4.TabIndex = 33;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Connect to:";
            // 
            // pnlRuntimeState
            // 
            this.pnlRuntimeState.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRuntimeState.Location = new System.Drawing.Point(28, 161);
            this.pnlRuntimeState.Name = "pnlRuntimeState";
            this.pnlRuntimeState.Size = new System.Drawing.Size(143, 20);
            this.pnlRuntimeState.TabIndex = 38;
            this.pnlRuntimeState.Text = "<status>";
            this.pnlRuntimeState.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtSpecificServers
            // 
            this.txtSpecificServers.Location = new System.Drawing.Point(28, 89);
            this.txtSpecificServers.Name = "txtSpecificServers";
            this.txtSpecificServers.Size = new System.Drawing.Size(143, 20);
            this.txtSpecificServers.TabIndex = 33;
            this.txtSpecificServers.Text = "melwadqds01";
            // 
            // rbSpecificServer
            // 
            this.rbSpecificServer.AutoSize = true;
            this.rbSpecificServer.Location = new System.Drawing.Point(8, 92);
            this.rbSpecificServer.Name = "rbSpecificServer";
            this.rbSpecificServer.Size = new System.Drawing.Size(14, 13);
            this.rbSpecificServer.TabIndex = 32;
            this.rbSpecificServer.TabStop = true;
            this.rbSpecificServer.UseVisualStyleBackColor = true;
            // 
            // rbLocalhost
            // 
            this.rbLocalhost.AutoSize = true;
            this.rbLocalhost.Location = new System.Drawing.Point(8, 67);
            this.rbLocalhost.Name = "rbLocalhost";
            this.rbLocalhost.Size = new System.Drawing.Size(96, 17);
            this.rbLocalhost.TabIndex = 30;
            this.rbLocalhost.TabStop = true;
            this.rbLocalhost.Text = "Local host only";
            this.rbLocalhost.UseVisualStyleBackColor = true;
            // 
            // rbDefaultServers
            // 
            this.rbDefaultServers.AutoSize = true;
            this.rbDefaultServers.Checked = true;
            this.rbDefaultServers.Location = new System.Drawing.Point(8, 44);
            this.rbDefaultServers.Name = "rbDefaultServers";
            this.rbDefaultServers.Size = new System.Drawing.Size(102, 17);
            this.rbDefaultServers.TabIndex = 29;
            this.rbDefaultServers.TabStop = true;
            this.rbDefaultServers.Text = "Default server(s)";
            this.rbDefaultServers.UseVisualStyleBackColor = true;
            // 
            // cbEnvId
            // 
            this.cbEnvId.FormattingEnabled = true;
            this.cbEnvId.Location = new System.Drawing.Point(28, 17);
            this.cbEnvId.Name = "cbEnvId";
            this.cbEnvId.Size = new System.Drawing.Size(143, 21);
            this.cbEnvId.TabIndex = 28;
            // 
            // btnRestart
            // 
            this.btnRestart.Location = new System.Drawing.Point(28, 119);
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(143, 33);
            this.btnRestart.TabIndex = 27;
            this.btnRestart.Text = "Re-connect";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.BtnRestartClick1);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage5);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 196);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1312, 549);
            this.tabControl1.TabIndex = 26;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.txtLog);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1304, 523);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 3);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1298, 517);
            this.txtLog.TabIndex = 33;
            // 
            // tabPage5
            // 
            this.tabPage5.Controls.Add(this.splitContainer2);
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(1304, 523);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Browser";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.treeNavigation);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.txtNavDetail);
            this.splitContainer2.Size = new System.Drawing.Size(1298, 517);
            this.splitContainer2.SplitterDistance = 427;
            this.splitContainer2.TabIndex = 2;
            // 
            // treeNavigation
            // 
            this.treeNavigation.ContextMenuStrip = this.contextMenuStrip1;
            this.treeNavigation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeNavigation.Location = new System.Drawing.Point(0, 0);
            this.treeNavigation.Name = "treeNavigation";
            this.treeNavigation.Size = new System.Drawing.Size(427, 517);
            this.treeNavigation.TabIndex = 0;
            this.treeNavigation.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.TreeNavigationAfterSelect);
            this.treeNavigation.KeyUp += new System.Windows.Forms.KeyEventHandler(this.TreeNavigationKeyUp);
            // 
            // txtNavDetail
            // 
            this.txtNavDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtNavDetail.Location = new System.Drawing.Point(0, 0);
            this.txtNavDetail.Multiline = true;
            this.txtNavDetail.Name = "txtNavDetail";
            this.txtNavDetail.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtNavDetail.Size = new System.Drawing.Size(867, 517);
            this.txtNavDetail.TabIndex = 0;
            // 
            // createTradeBtn
            // 
            this.createTradeBtn.Location = new System.Drawing.Point(262, 157);
            this.createTradeBtn.Name = "createTradeBtn";
            this.createTradeBtn.Size = new System.Drawing.Size(151, 25);
            this.createTradeBtn.TabIndex = 76;
            this.createTradeBtn.Text = "Create Trade";
            this.createTradeBtn.UseVisualStyleBackColor = true;
            this.createTradeBtn.Click += new System.EventHandler(this.BtnCreateTradeClick);
            // 
            // CoreViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1312, 745);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "CoreViewerForm";
            this.Text = "Core Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CoreViewerFormFormClosing);
            this.Load += new System.EventHandler(this.CoreViewerFormLoad);
            this.contextMenuStrip1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBoxValuation.ResumeLayout(false);
            this.groupBoxValuation.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage5.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnEventLogClear;
        private System.Windows.Forms.Button btnDeleteObjects;
        private System.Windows.Forms.Button btnClearAll;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtPropItemNameValue;
        private System.Windows.Forms.CheckBox chkPropItemName;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.ComboBox cbDataTypeValues;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtProp3Name;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtProp2Name;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnLoadObjects;
        private System.Windows.Forms.TextBox txtProp4Value;
        private System.Windows.Forms.TextBox txtProp4Name;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkProp4;
        private System.Windows.Forms.TextBox txtProp3Value;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkProp3;
        private System.Windows.Forms.TextBox txtProp2Value;
        private System.Windows.Forms.CheckBox chkProp2;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.CheckBox chkProp1;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.RadioButton rbLocalhost;
        private System.Windows.Forms.RadioButton rbDefaultServers;
        private System.Windows.Forms.ComboBox cbEnvId;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView treeNavigation;
        private System.Windows.Forms.TextBox txtNavDetail;
        private System.Windows.Forms.Button btnUnsubscribe;
        private System.Windows.Forms.Button btnSubscribe;
        private System.Windows.Forms.CheckBox chkDebugRequests;
        private System.Windows.Forms.ToolStripMenuItem saveToFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewPropertiesStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewXmlStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewMarketStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem valueStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createTradeStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TextBox txtSpecificServers;
        private System.Windows.Forms.RadioButton rbSpecificServer;
        private System.Windows.Forms.TextBox pnlRuntimeState;
        private System.Windows.Forms.TextBox txtBoxTradeDirectory;
        private System.Windows.Forms.Button btnLoadTrade;
        private System.Windows.Forms.DateTimePicker dateTimePickerValuation;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBoxValuation;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.ComboBox comboBoxCurrency;
        private System.Windows.Forms.ComboBox comboBoxMarket;
        private System.Windows.Forms.ComboBox comboBoxParty;
        private System.Windows.Forms.Label lblMetrics;
        private System.Windows.Forms.ListBox listBoxMetrics;
        private bool _suppressWarningMessageBox = false;
        private System.Windows.Forms.Button btnLoadConfigData;
        private System.Windows.Forms.TextBox nameSpaceTextBox1;
        private System.Windows.Forms.Button createTradeBtn;
    }
}

