/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

namespace Highlander.AutoCurveBuilder.V5r3
{
    partial class CurveBuilderForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtBoxMarket = new System.Windows.Forms.TextBox();
            this.chkBoxWorkflow = new System.Windows.Forms.CheckBox();
            this.chkBoxDependentCurves = new System.Windows.Forms.CheckBox();
            this.buttonStopBuild = new System.Windows.Forms.Button();
            this.listBoxCurrencies = new System.Windows.Forms.ListBox();
            this.listBoxCurves = new System.Windows.Forms.ListBox();
            this.btnAutoCurveBuiler = new System.Windows.Forms.Button();
            this.txtChangePort = new System.Windows.Forms.TextBox();
            this.chkChangePort = new System.Windows.Forms.CheckBox();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtBoxMarket);
            this.panel1.Controls.Add(this.chkBoxWorkflow);
            this.panel1.Controls.Add(this.chkBoxDependentCurves);
            this.panel1.Controls.Add(this.buttonStopBuild);
            this.panel1.Controls.Add(this.listBoxCurrencies);
            this.panel1.Controls.Add(this.listBoxCurves);
            this.panel1.Controls.Add(this.btnAutoCurveBuiler);
            this.panel1.Controls.Add(this.txtChangePort);
            this.panel1.Controls.Add(this.chkChangePort);
            this.panel1.Controls.Add(this.btnStop);
            this.panel1.Controls.Add(this.btnStart);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(714, 113);
            this.panel1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Market";
            // 
            // txtBoxMarket
            // 
            this.txtBoxMarket.Location = new System.Drawing.Point(53, 75);
            this.txtBoxMarket.Name = "txtBoxMarket";
            this.txtBoxMarket.Size = new System.Drawing.Size(54, 20);
            this.txtBoxMarket.TabIndex = 4;
            this.txtBoxMarket.Text = "QR_LIVE";
            // 
            // chkBoxWorkflow
            // 
            this.chkBoxWorkflow.AutoSize = true;
            this.chkBoxWorkflow.Location = new System.Drawing.Point(12, 47);
            this.chkBoxWorkflow.Name = "chkBoxWorkflow";
            this.chkBoxWorkflow.Size = new System.Drawing.Size(318, 17);
            this.chkBoxWorkflow.TabIndex = 39;
            this.chkBoxWorkflow.Text = "Workflow Request.  Unchecked builds the base curve locally.";
            this.chkBoxWorkflow.UseVisualStyleBackColor = true;
            // 
            // chkBoxDependentCurves
            // 
            this.chkBoxDependentCurves.AutoSize = true;
            this.chkBoxDependentCurves.Location = new System.Drawing.Point(410, 19);
            this.chkBoxDependentCurves.Name = "chkBoxDependentCurves";
            this.chkBoxDependentCurves.Size = new System.Drawing.Size(115, 17);
            this.chkBoxDependentCurves.TabIndex = 38;
            this.chkBoxDependentCurves.Text = "Dependent Curves";
            this.chkBoxDependentCurves.UseVisualStyleBackColor = true;
            // 
            // buttonStopBuild
            // 
            this.buttonStopBuild.Location = new System.Drawing.Point(408, 75);
            this.buttonStopBuild.Name = "buttonStopBuild";
            this.buttonStopBuild.Size = new System.Drawing.Size(113, 30);
            this.buttonStopBuild.TabIndex = 37;
            this.buttonStopBuild.Text = "Stop AutoBuild";
            this.buttonStopBuild.UseVisualStyleBackColor = true;
            this.buttonStopBuild.Click += new System.EventHandler(this.ButtonStopBuildClick);
            // 
            // listBoxCurrencies
            // 
            this.listBoxCurrencies.FormattingEnabled = true;
            this.listBoxCurrencies.Items.AddRange(new object[] {
            "AUD",
            "USD",
            "EUR",
            "GBP",
            "JPY",
            "NZD",
            "CHF"});
            this.listBoxCurrencies.Location = new System.Drawing.Point(532, 9);
            this.listBoxCurrencies.MultiColumn = true;
            this.listBoxCurrencies.Name = "listBoxCurrencies";
            this.listBoxCurrencies.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxCurrencies.Size = new System.Drawing.Size(54, 95);
            this.listBoxCurrencies.TabIndex = 36;
            // 
            // listBoxCurves
            // 
            this.listBoxCurves.FormattingEnabled = true;
            this.listBoxCurves.Items.AddRange(new object[] {
            "CommodityCurve",
            "DiscountCurve",
            "EquityCurve",
            "FXCurve",
            "InflationCurve",
            "RateCurve"});
            this.listBoxCurves.Location = new System.Drawing.Point(599, 10);
            this.listBoxCurves.MultiColumn = true;
            this.listBoxCurves.Name = "listBoxCurves";
            this.listBoxCurves.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.listBoxCurves.Size = new System.Drawing.Size(103, 95);
            this.listBoxCurves.TabIndex = 35;
            // 
            // btnAutoCurveBuiler
            // 
            this.btnAutoCurveBuiler.Location = new System.Drawing.Point(408, 42);
            this.btnAutoCurveBuiler.Name = "btnAutoCurveBuiler";
            this.btnAutoCurveBuiler.Size = new System.Drawing.Size(113, 30);
            this.btnAutoCurveBuiler.TabIndex = 32;
            this.btnAutoCurveBuiler.Text = "AutoCurve Builder";
            this.btnAutoCurveBuiler.UseVisualStyleBackColor = true;
            this.btnAutoCurveBuiler.Click += new System.EventHandler(this.BtnAutoCurveBuilderClick);
            // 
            // txtChangePort
            // 
            this.txtChangePort.Location = new System.Drawing.Point(231, 16);
            this.txtChangePort.Name = "txtChangePort";
            this.txtChangePort.Size = new System.Drawing.Size(43, 20);
            this.txtChangePort.TabIndex = 31;
            this.txtChangePort.Text = "9999";
            // 
            // chkChangePort
            // 
            this.chkChangePort.AutoSize = true;
            this.chkChangePort.Location = new System.Drawing.Point(12, 18);
            this.chkChangePort.Name = "chkChangePort";
            this.chkChangePort.Size = new System.Drawing.Size(222, 17);
            this.chkChangePort.TabIndex = 30;
            this.chkChangePort.Text = "Change server port from default (nnnn) to:";
            this.chkChangePort.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(231, 75);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(84, 27);
            this.btnStop.TabIndex = 28;
            this.btnStop.Text = "Stop Server";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.BtnStopClick);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(126, 75);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(84, 27);
            this.btnStart.TabIndex = 27;
            this.btnStart.Text = "Start Server";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStartClick);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtLog);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 113);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(714, 604);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Log";
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(3, 16);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(708, 585);
            this.txtLog.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 717);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.panel1);
            this.Name = "Form1";
            this.Text = "Highlander Auto Curve Build Engine";
            this.Load += new System.EventHandler(this.Form1Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TextBox txtChangePort;
        private System.Windows.Forms.CheckBox chkChangePort;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button btnAutoCurveBuiler;
        private System.Windows.Forms.ListBox listBoxCurves;
        private System.Windows.Forms.ListBox listBoxCurrencies;
        private System.Windows.Forms.Button buttonStopBuild;
        private System.Windows.Forms.CheckBox chkBoxDependentCurves;
        private System.Windows.Forms.CheckBox chkBoxWorkflow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBoxMarket;
    }
}

