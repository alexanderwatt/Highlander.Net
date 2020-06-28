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

using Highlander.ChartControl;

namespace Highlander.CurveEngine.Tests.V5r3.Helpers
{
    partial class YieldCurveVisualizerForm
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
            this.chart1 = new Chart();
            this.SuspendLayout();
            // 
            // chart1
            // 
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chart1.Grid.YAxisStyle = ChartGridStyles.None;
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            this.chart1.Series.Labels.Visible = false;
            this.chart1.Size = new System.Drawing.Size(924, 468);
            this.chart1.TabIndex = 0;
            this.chart1.XAxis.Labels.Visible = false;
            this.chart1.XAxis.MajorTick = ChartTickMarkTypes.None;
            this.chart1.XAxis.Scale.AutoScale = false;
            this.chart1.XAxis.Scale.Maximum = new System.DateTime(2007, 9, 22, 0, 0, 0, 0);
            this.chart1.XAxis.Scale.Minimum = new System.DateTime(2007, 9, 22, 0, 0, 0, 0);
            this.chart1.YAxis.MajorTick = ChartTickMarkTypes.Inside;
            this.chart1.YAxis.MinorTick = ChartTickMarkTypes.Inside;
            // 
            // YieldCurveVisualizerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 468);
            this.Controls.Add(this.chart1);
            this.Name = "YieldCurveVisualizerForm";
            this.Text = "YieldCurveVisualizerForm";
            this.Load += new System.EventHandler(this.YieldCurveVisualizerFormLoad);
            this.ResumeLayout(false);

        }

        #endregion

        private Chart chart1;
    }
}