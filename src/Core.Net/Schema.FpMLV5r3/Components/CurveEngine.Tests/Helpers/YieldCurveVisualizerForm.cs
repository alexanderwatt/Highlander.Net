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

#region Using directive

using System;
using System.Collections;
using System.Windows.Forms;
using Highlander.ChartControl;

#endregion

namespace Highlander.CurveEngine.Tests.V5r3.Helpers
{
    public partial class YieldCurveVisualizerForm : Form
    {
        public YieldCurveVisualizerForm(IList dates, IList zeros)
        {
            InitializeComponent();

            chart1.Name = "chart1";
            chart1.TabIndex = 0;
            chart1.XAxis.Scale.Minimum = (DateTime)dates[0];
            chart1.XAxis.Scale.Maximum = (DateTime)dates[dates.Count - 1];

            chart1.YAxis.Scale.Minimum = 700;
            chart1.YAxis.Scale.Maximum = 900;
            chart1.YAxis.Scale.MajorUnit = 10;
            chart1.YAxis.Scale.MinorUnit = 2;

            chart1.Series.Line.Color = System.Drawing.Color.Blue;
            chart1.Series.Mark.BorderColor = System.Drawing.Color.DarkBlue;
            chart1.Series.Mark.FillColor = System.Drawing.Color.DeepSkyBlue;
            chart1.Series.Projections.Labels.Rotation = -30F;
            chart1.Series.Projections.Line.Color = System.Drawing.SystemColors.GrayText;
            chart1.Series.Projections.Line.Dash = System.Drawing.Drawing2D.DashStyle.Dash;
            chart1.Series.Projections.Visible = true;
            chart1.Series.Title.Text = "Series 1";

            for (int i = 0; i < dates.Count; i++)
            {
                var cv = new ChartSeriesValue
                             {
                                 Date = (DateTime) dates[i],
                                 Value = 10000.0m*Convert.ToDecimal(zeros[i])
                             };

                chart1.Series.Values.Add(cv);
            }
            chart1.Update();

        }

        private void YieldCurveVisualizerFormLoad(object sender, EventArgs e)
        {}
    }
}