#region Using directive

using System;
using System.Collections;
using System.Windows.Forms;

#endregion

namespace Orion.UI
{
    public partial class CurveForm : Form
    {
        public CurveForm(IList dates, IList zeros)
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
                var cv = new WindowsForms.ChartControl.ChartSeriesValue
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