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

namespace Highlander.CurveManager.V5r3
{
    partial class CurveManagerForm
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
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.cbClientNameSpace = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkSelectedBaseCurvesOnlyToCopy = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nudRetainResultsDays = new System.Windows.Forms.NumericUpDown();
            this.chkGenerateEODCurves = new System.Windows.Forms.CheckBox();
            this.dtpBaseDate = new System.Windows.Forms.DateTimePicker();
            this.chkSelectedStressesOnly = new System.Windows.Forms.CheckBox();
            this.btnGenStressedCurves = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.chkUseSavedMarketData = new System.Windows.Forms.CheckBox();
            this.chkSaveMarketData = new System.Windows.Forms.CheckBox();
            this.chkSelectedCurvesOnly = new System.Windows.Forms.CheckBox();
            this.btnGenerateBaseCurves = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpCurveDef = new System.Windows.Forms.TabPage();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.lvCurveDef = new System.Windows.Forms.ListView();
            this.panelCurveDef = new System.Windows.Forms.Panel();
            this.tpBaseCurve = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lvBaseCurve = new System.Windows.Forms.ListView();
            this.pnlBaseCurve = new System.Windows.Forms.Panel();
            this.tpShockDef = new System.Windows.Forms.TabPage();
            this.lvShockDef = new System.Windows.Forms.ListView();
            this.panelShockDef = new System.Windows.Forms.Panel();
            this.tpStressCurve = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.lvStressCurve = new System.Windows.Forms.ListView();
            this.pnlStressCurve = new System.Windows.Forms.Panel();
            this.tpScenarioDef = new System.Windows.Forms.TabPage();
            this.lvScenarioDef = new System.Windows.Forms.ListView();
            this.panelScenarioDef = new System.Windows.Forms.Panel();
            this.tpDebugLog = new System.Windows.Forms.TabPage();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRetainResultsDays)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpCurveDef.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.tpBaseCurve.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tpShockDef.SuspendLayout();
            this.tpStressCurve.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.tpScenarioDef.SuspendLayout();
            this.tpDebugLog.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox8);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1633, 168);
            this.panel1.TabIndex = 0;
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.cbClientNameSpace);
            this.groupBox8.Controls.Add(this.label4);
            this.groupBox8.Controls.Add(this.chkSelectedBaseCurvesOnlyToCopy);
            this.groupBox8.Controls.Add(this.label3);
            this.groupBox8.Controls.Add(this.label2);
            this.groupBox8.Controls.Add(this.nudRetainResultsDays);
            this.groupBox8.Controls.Add(this.chkGenerateEODCurves);
            this.groupBox8.Controls.Add(this.dtpBaseDate);
            this.groupBox8.Controls.Add(this.chkSelectedStressesOnly);
            this.groupBox8.Controls.Add(this.btnGenStressedCurves);
            this.groupBox8.Controls.Add(this.label17);
            this.groupBox8.Controls.Add(this.chkUseSavedMarketData);
            this.groupBox8.Controls.Add(this.chkSaveMarketData);
            this.groupBox8.Controls.Add(this.chkSelectedCurvesOnly);
            this.groupBox8.Controls.Add(this.btnGenerateBaseCurves);
            this.groupBox8.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox8.Location = new System.Drawing.Point(0, 0);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(1633, 168);
            this.groupBox8.TabIndex = 45;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Curve Generation";
            // 
            // cbClientNameSpace
            // 
            this.cbClientNameSpace.FormattingEnabled = true;
            this.cbClientNameSpace.Items.AddRange(new object[] {
            "Highlander.V5r3",
            "Highlander",
            "FpML47"});
            this.cbClientNameSpace.Location = new System.Drawing.Point(638, 113);
            this.cbClientNameSpace.Name = "cbClientNameSpace";
            this.cbClientNameSpace.Size = new System.Drawing.Size(110, 21);
            this.cbClientNameSpace.TabIndex = 57;
            this.cbClientNameSpace.Text = "Orion.V5r3";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(635, 93);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 13);
            this.label4.TabIndex = 55;
            this.label4.Text = "Client Name Space:";
            // 
            // chkSelectedBaseCurvesOnlyToCopy
            // 
            this.chkSelectedBaseCurvesOnlyToCopy.AutoSize = true;
            this.chkSelectedBaseCurvesOnlyToCopy.Checked = true;
            this.chkSelectedBaseCurvesOnlyToCopy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectedBaseCurvesOnlyToCopy.Location = new System.Drawing.Point(638, 65);
            this.chkSelectedBaseCurvesOnlyToCopy.Name = "chkSelectedBaseCurvesOnlyToCopy";
            this.chkSelectedBaseCurvesOnlyToCopy.Size = new System.Drawing.Size(176, 17);
            this.chkSelectedBaseCurvesOnlyToCopy.TabIndex = 54;
            this.chkSelectedBaseCurvesOnlyToCopy.Text = "Copy only selected base curves";
            this.chkSelectedBaseCurvesOnlyToCopy.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(640, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 53;
            this.label3.Text = "Retain results for";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(775, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 52;
            this.label2.Text = "days";
            // 
            // nudRetainResultsDays
            // 
            this.nudRetainResultsDays.Increment = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudRetainResultsDays.Location = new System.Drawing.Point(732, 30);
            this.nudRetainResultsDays.Maximum = new decimal(new int[] {
            72,
            0,
            0,
            0});
            this.nudRetainResultsDays.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nudRetainResultsDays.Name = "nudRetainResultsDays";
            this.nudRetainResultsDays.Size = new System.Drawing.Size(37, 20);
            this.nudRetainResultsDays.TabIndex = 51;
            this.nudRetainResultsDays.Value = new decimal(new int[] {
            8,
            0,
            0,
            0});
            // 
            // chkGenerateEODCurves
            // 
            this.chkGenerateEODCurves.AutoSize = true;
            this.chkGenerateEODCurves.Location = new System.Drawing.Point(205, 42);
            this.chkGenerateEODCurves.Name = "chkGenerateEODCurves";
            this.chkGenerateEODCurves.Size = new System.Drawing.Size(179, 17);
            this.chkGenerateEODCurves.TabIndex = 46;
            this.chkGenerateEODCurves.Text = "Force generation of EOD curves";
            this.chkGenerateEODCurves.UseVisualStyleBackColor = true;
            // 
            // dtpBaseDate
            // 
            this.dtpBaseDate.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpBaseDate.Location = new System.Drawing.Point(82, 39);
            this.dtpBaseDate.Name = "dtpBaseDate";
            this.dtpBaseDate.Size = new System.Drawing.Size(86, 20);
            this.dtpBaseDate.TabIndex = 39;
            // 
            // chkSelectedStressesOnly
            // 
            this.chkSelectedStressesOnly.AutoSize = true;
            this.chkSelectedStressesOnly.Checked = true;
            this.chkSelectedStressesOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectedStressesOnly.Location = new System.Drawing.Point(420, 65);
            this.chkSelectedStressesOnly.Name = "chkSelectedStressesOnly";
            this.chkSelectedStressesOnly.Size = new System.Drawing.Size(188, 17);
            this.chkSelectedStressesOnly.TabIndex = 44;
            this.chkSelectedStressesOnly.Text = "Rebuild only selected base curves";
            this.chkSelectedStressesOnly.UseVisualStyleBackColor = true;
            // 
            // btnGenStressedCurves
            // 
            this.btnGenStressedCurves.Location = new System.Drawing.Point(420, 88);
            this.btnGenStressedCurves.Name = "btnGenStressedCurves";
            this.btnGenStressedCurves.Size = new System.Drawing.Size(178, 46);
            this.btnGenStressedCurves.TabIndex = 43;
            this.btnGenStressedCurves.Text = "Generate Stressed Curves";
            this.btnGenStressedCurves.UseVisualStyleBackColor = true;
            this.btnGenStressedCurves.Click += new System.EventHandler(this.BtnGenStressedCurvesClick1);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(18, 43);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(58, 13);
            this.label17.TabIndex = 38;
            this.label17.Text = "Base date:";
            // 
            // chkUseSavedMarketData
            // 
            this.chkUseSavedMarketData.AutoSize = true;
            this.chkUseSavedMarketData.Location = new System.Drawing.Point(10, 89);
            this.chkUseSavedMarketData.Name = "chkUseSavedMarketData";
            this.chkUseSavedMarketData.Size = new System.Drawing.Size(169, 17);
            this.chkUseSavedMarketData.TabIndex = 45;
            this.chkUseSavedMarketData.Text = "Use offline/saved market data";
            this.chkUseSavedMarketData.UseVisualStyleBackColor = true;
            // 
            // chkSaveMarketData
            // 
            this.chkSaveMarketData.AutoSize = true;
            this.chkSaveMarketData.Location = new System.Drawing.Point(9, 112);
            this.chkSaveMarketData.Name = "chkSaveMarketData";
            this.chkSaveMarketData.Size = new System.Drawing.Size(176, 17);
            this.chkSaveMarketData.TabIndex = 44;
            this.chkSaveMarketData.Text = "Save market data for offline use";
            this.chkSaveMarketData.UseVisualStyleBackColor = true;
            // 
            // chkSelectedCurvesOnly
            // 
            this.chkSelectedCurvesOnly.AutoSize = true;
            this.chkSelectedCurvesOnly.Checked = true;
            this.chkSelectedCurvesOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSelectedCurvesOnly.Location = new System.Drawing.Point(205, 65);
            this.chkSelectedCurvesOnly.Name = "chkSelectedCurvesOnly";
            this.chkSelectedCurvesOnly.Size = new System.Drawing.Size(177, 17);
            this.chkSelectedCurvesOnly.TabIndex = 43;
            this.chkSelectedCurvesOnly.Text = "Rebuild only selected definitions";
            this.chkSelectedCurvesOnly.UseVisualStyleBackColor = true;
            // 
            // btnGenerateBaseCurves
            // 
            this.btnGenerateBaseCurves.Location = new System.Drawing.Point(205, 88);
            this.btnGenerateBaseCurves.Name = "btnGenerateBaseCurves";
            this.btnGenerateBaseCurves.Size = new System.Drawing.Size(178, 46);
            this.btnGenerateBaseCurves.TabIndex = 42;
            this.btnGenerateBaseCurves.Text = "Generate Base Curves";
            this.btnGenerateBaseCurves.UseVisualStyleBackColor = true;
            this.btnGenerateBaseCurves.Click += new System.EventHandler(this.BtnRefreshRatesClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tpCurveDef);
            this.tabControl1.Controls.Add(this.tpBaseCurve);
            this.tabControl1.Controls.Add(this.tpShockDef);
            this.tabControl1.Controls.Add(this.tpStressCurve);
            this.tabControl1.Controls.Add(this.tpScenarioDef);
            this.tabControl1.Controls.Add(this.tpDebugLog);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 168);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1633, 645);
            this.tabControl1.TabIndex = 1;
            // 
            // tpCurveDef
            // 
            this.tpCurveDef.Controls.Add(this.groupBox6);
            this.tpCurveDef.Location = new System.Drawing.Point(4, 22);
            this.tpCurveDef.Name = "tpCurveDef";
            this.tpCurveDef.Padding = new System.Windows.Forms.Padding(3);
            this.tpCurveDef.Size = new System.Drawing.Size(1625, 619);
            this.tpCurveDef.TabIndex = 7;
            this.tpCurveDef.Text = "Curve Definitions";
            this.tpCurveDef.UseVisualStyleBackColor = true;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.lvCurveDef);
            this.groupBox6.Controls.Add(this.panelCurveDef);
            this.groupBox6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox6.Location = new System.Drawing.Point(3, 3);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(1619, 613);
            this.groupBox6.TabIndex = 34;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Curve Definitions";
            // 
            // lvCurveDef
            // 
            this.lvCurveDef.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvCurveDef.HideSelection = false;
            this.lvCurveDef.Location = new System.Drawing.Point(3, 48);
            this.lvCurveDef.Name = "lvCurveDef";
            this.lvCurveDef.Size = new System.Drawing.Size(1613, 562);
            this.lvCurveDef.TabIndex = 9;
            this.lvCurveDef.UseCompatibleStateImageBehavior = false;
            // 
            // panelCurveDef
            // 
            this.panelCurveDef.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelCurveDef.Location = new System.Drawing.Point(3, 16);
            this.panelCurveDef.Name = "panelCurveDef";
            this.panelCurveDef.Size = new System.Drawing.Size(1613, 32);
            this.panelCurveDef.TabIndex = 8;
            // 
            // tpBaseCurve
            // 
            this.tpBaseCurve.Controls.Add(this.groupBox1);
            this.tpBaseCurve.Location = new System.Drawing.Point(4, 22);
            this.tpBaseCurve.Name = "tpBaseCurve";
            this.tpBaseCurve.Padding = new System.Windows.Forms.Padding(3);
            this.tpBaseCurve.Size = new System.Drawing.Size(1625, 619);
            this.tpBaseCurve.TabIndex = 8;
            this.tpBaseCurve.Text = "Base Curves";
            this.tpBaseCurve.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lvBaseCurve);
            this.groupBox1.Controls.Add(this.pnlBaseCurve);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(3, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1619, 613);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Published Curves";
            // 
            // lvBaseCurve
            // 
            this.lvBaseCurve.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvBaseCurve.FullRowSelect = true;
            this.lvBaseCurve.HideSelection = false;
            this.lvBaseCurve.Location = new System.Drawing.Point(3, 48);
            this.lvBaseCurve.MultiSelect = false;
            this.lvBaseCurve.Name = "lvBaseCurve";
            this.lvBaseCurve.Size = new System.Drawing.Size(1613, 562);
            this.lvBaseCurve.TabIndex = 12;
            this.lvBaseCurve.UseCompatibleStateImageBehavior = false;
            // 
            // pnlBaseCurve
            // 
            this.pnlBaseCurve.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlBaseCurve.Location = new System.Drawing.Point(3, 16);
            this.pnlBaseCurve.Name = "pnlBaseCurve";
            this.pnlBaseCurve.Size = new System.Drawing.Size(1613, 32);
            this.pnlBaseCurve.TabIndex = 11;
            // 
            // tpShockDef
            // 
            this.tpShockDef.Controls.Add(this.lvShockDef);
            this.tpShockDef.Controls.Add(this.panelShockDef);
            this.tpShockDef.Location = new System.Drawing.Point(4, 22);
            this.tpShockDef.Name = "tpShockDef";
            this.tpShockDef.Padding = new System.Windows.Forms.Padding(3);
            this.tpShockDef.Size = new System.Drawing.Size(1625, 619);
            this.tpShockDef.TabIndex = 9;
            this.tpShockDef.Text = "Stress Definitions";
            this.tpShockDef.UseVisualStyleBackColor = true;
            // 
            // lvShockDef
            // 
            this.lvShockDef.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvShockDef.FullRowSelect = true;
            this.lvShockDef.HideSelection = false;
            this.lvShockDef.Location = new System.Drawing.Point(3, 35);
            this.lvShockDef.Name = "lvShockDef";
            this.lvShockDef.Size = new System.Drawing.Size(1619, 581);
            this.lvShockDef.TabIndex = 14;
            this.lvShockDef.UseCompatibleStateImageBehavior = false;
            // 
            // panelShockDef
            // 
            this.panelShockDef.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelShockDef.Location = new System.Drawing.Point(3, 3);
            this.panelShockDef.Name = "panelShockDef";
            this.panelShockDef.Size = new System.Drawing.Size(1619, 32);
            this.panelShockDef.TabIndex = 13;
            // 
            // tpStressCurve
            // 
            this.tpStressCurve.Controls.Add(this.groupBox4);
            this.tpStressCurve.Location = new System.Drawing.Point(4, 22);
            this.tpStressCurve.Name = "tpStressCurve";
            this.tpStressCurve.Padding = new System.Windows.Forms.Padding(3);
            this.tpStressCurve.Size = new System.Drawing.Size(1625, 619);
            this.tpStressCurve.TabIndex = 11;
            this.tpStressCurve.Text = "Stressed Curves";
            this.tpStressCurve.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.lvStressCurve);
            this.groupBox4.Controls.Add(this.pnlStressCurve);
            this.groupBox4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox4.Location = new System.Drawing.Point(3, 3);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(1619, 613);
            this.groupBox4.TabIndex = 2;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Published Curves";
            // 
            // lvStressCurve
            // 
            this.lvStressCurve.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvStressCurve.FullRowSelect = true;
            this.lvStressCurve.HideSelection = false;
            this.lvStressCurve.Location = new System.Drawing.Point(3, 48);
            this.lvStressCurve.MultiSelect = false;
            this.lvStressCurve.Name = "lvStressCurve";
            this.lvStressCurve.Size = new System.Drawing.Size(1613, 562);
            this.lvStressCurve.TabIndex = 12;
            this.lvStressCurve.UseCompatibleStateImageBehavior = false;
            // 
            // pnlStressCurve
            // 
            this.pnlStressCurve.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlStressCurve.Location = new System.Drawing.Point(3, 16);
            this.pnlStressCurve.Name = "pnlStressCurve";
            this.pnlStressCurve.Size = new System.Drawing.Size(1613, 32);
            this.pnlStressCurve.TabIndex = 11;
            // 
            // tpScenarioDef
            // 
            this.tpScenarioDef.Controls.Add(this.lvScenarioDef);
            this.tpScenarioDef.Controls.Add(this.panelScenarioDef);
            this.tpScenarioDef.Location = new System.Drawing.Point(4, 22);
            this.tpScenarioDef.Name = "tpScenarioDef";
            this.tpScenarioDef.Padding = new System.Windows.Forms.Padding(3);
            this.tpScenarioDef.Size = new System.Drawing.Size(1625, 619);
            this.tpScenarioDef.TabIndex = 13;
            this.tpScenarioDef.Text = "Scenario Definitions";
            this.tpScenarioDef.UseVisualStyleBackColor = true;
            // 
            // lvScenarioDef
            // 
            this.lvScenarioDef.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvScenarioDef.FullRowSelect = true;
            this.lvScenarioDef.HideSelection = false;
            this.lvScenarioDef.Location = new System.Drawing.Point(3, 35);
            this.lvScenarioDef.Name = "lvScenarioDef";
            this.lvScenarioDef.Size = new System.Drawing.Size(1619, 581);
            this.lvScenarioDef.TabIndex = 16;
            this.lvScenarioDef.UseCompatibleStateImageBehavior = false;
            // 
            // panelScenarioDef
            // 
            this.panelScenarioDef.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelScenarioDef.Location = new System.Drawing.Point(3, 3);
            this.panelScenarioDef.Name = "panelScenarioDef";
            this.panelScenarioDef.Size = new System.Drawing.Size(1619, 32);
            this.panelScenarioDef.TabIndex = 15;
            // 
            // tpDebugLog
            // 
            this.tpDebugLog.Controls.Add(this.txtLog);
            this.tpDebugLog.Location = new System.Drawing.Point(4, 22);
            this.tpDebugLog.Name = "tpDebugLog";
            this.tpDebugLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpDebugLog.Size = new System.Drawing.Size(1625, 619);
            this.tpDebugLog.TabIndex = 2;
            this.tpDebugLog.Text = "Log";
            this.tpDebugLog.UseVisualStyleBackColor = true;
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 3);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(1619, 613);
            this.txtLog.TabIndex = 0;
            // 
            // CurveManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1633, 813);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel1);
            this.Name = "CurveManagerForm";
            this.Text = "Highlander Curve Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1FormClosing);
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRetainResultsDays)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tpCurveDef.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.tpBaseCurve.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.tpShockDef.ResumeLayout(false);
            this.tpStressCurve.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.tpScenarioDef.ResumeLayout(false);
            this.tpDebugLog.ResumeLayout(false);
            this.tpDebugLog.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpDebugLog;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.DateTimePicker dtpBaseDate;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button btnGenerateBaseCurves;
        private System.Windows.Forms.CheckBox chkSelectedCurvesOnly;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.CheckBox chkUseSavedMarketData;
        private System.Windows.Forms.CheckBox chkSaveMarketData;
        private System.Windows.Forms.TabPage tpCurveDef;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.ListView lvCurveDef;
        private System.Windows.Forms.Panel panelCurveDef;
        private System.Windows.Forms.TabPage tpBaseCurve;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListView lvBaseCurve;
        private System.Windows.Forms.Panel pnlBaseCurve;
        private System.Windows.Forms.TabPage tpShockDef;
        private System.Windows.Forms.ListView lvShockDef;
        private System.Windows.Forms.Panel panelShockDef;
        private System.Windows.Forms.Button btnGenStressedCurves;
        private System.Windows.Forms.CheckBox chkSelectedStressesOnly;
        private System.Windows.Forms.TabPage tpStressCurve;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.ListView lvStressCurve;
        private System.Windows.Forms.Panel pnlStressCurve;
        private System.Windows.Forms.CheckBox chkGenerateEODCurves;
        private System.Windows.Forms.TabPage tpScenarioDef;
        private System.Windows.Forms.ListView lvScenarioDef;
        private System.Windows.Forms.Panel panelScenarioDef;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudRetainResultsDays;
        private System.Windows.Forms.CheckBox chkSelectedBaseCurvesOnlyToCopy;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cbClientNameSpace;
    }
}

