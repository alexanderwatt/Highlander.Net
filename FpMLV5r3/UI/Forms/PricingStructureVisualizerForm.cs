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

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Highlander.Core.Common;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Serialisation;

namespace Highlander.Reporting.UI.V5r3
{
    public partial class PricingStructureVisualizerForm : Form
    {
        private readonly ICoreItem _item;

        public PricingStructureVisualizerForm(ICoreItem coreItem)
        {
            _item = coreItem;
            InitializeComponent();
        }

        private void PricingStructureFormDisplayLoad(object sender, EventArgs e)
        {
            textBox1.Text = _item.Name;
            //Instantiate the data grid
            //
            //dataGridView1.Columns.Add("PropertyName", "Name");
            //dataGridView1.Columns.Add("PropertyValue", "Value");
            var properties = _item.AppProps.ToDictionary();
            foreach (var property in properties)
            {
                object[] row = {property.Key, property.Value.ToString()};
                dataGridView1.Rows.Add(row);
            }
            //Populate the market data
            //dataGridView2.Columns.Add("assetName", "Name");
            //dataGridView2.Columns.Add("assetValue", "Value");
            var marketData = XmlSerializerHelper.DeserializeFromString<Market>(_item.Text);
            if (marketData != null)
            {
                var quotedAssetSet = marketData.GetQuotedAssetSet();
                if (quotedAssetSet != null)
                {
                    //Populate the asset data grid
                    var index = 0;
                    foreach (var asset in quotedAssetSet.instrumentSet.Items)
                    {
                        object[] row = {asset.id, quotedAssetSet.assetQuote[index].quote[0].value};
                        dataGridView2.Rows.Add(row);
                        index++;
                    }
                }

                chart1.Name = "termCurveChart";
                chart1.Titles.Add("Zero Curve");
                //chart1.ChartAreas.Add("termCurveArea");
                //chart1.ChartAreas["termCurveArea"].AxisX.Minimum = 0;
                //chart1.ChartAreas["termCurveArea"].AxisX.Maximum = 10000;
                //chart1.ChartAreas["termCurveArea"].AxisX.Interval = 1;
                //chart1.ChartAreas["termCurveArea"].AxisY.Minimum = 0;
                //chart1.ChartAreas["termCurveArea"].AxisY.Maximum = .1;
                //chart1.ChartAreas["termCurveArea"].AxisY.Interval = .01;
                chart1.Series["Curve"].Color = Color.Blue;
                chart1.Series["Curve"].ChartType = SeriesChartType.Line;
                var zeroCurve = marketData.GetTermCurve();
                var baseDate = marketData.Items1[0].baseDate.Value;
                if (zeroCurve != null)
                {
                    //Populate the curve graph
                    var rates = zeroCurve.GetListMidValues();
                    var dates = zeroCurve.GetListTermDates();
                    if (rates != null && dates != null)
                    {
                        var index = 0;
                        foreach (var rate in rates)
                        {
                            var currentDate = dates[index];
                            var numDays = currentDate.Subtract(baseDate);
                            chart1.Series["Curve"].Points.AddXY(numDays.TotalDays, Convert.ToDouble(rate));
                            index++;
                        }
                    }
                }
            }
        }

        //private void UpdateChart()
        //{
        //    chart1.Series["Curve"].Points.Clear();
        //}

        private void Button1Click(object sender, EventArgs e)
        {
            //Start the subscription
            //UpdateChart();
            //kayChart cpuData = new kayChart(chart1, 60) { serieName = "CPU" };
            //Task.Factory.StartNew(() =>
            //{
            //    cpuData.updateChart(UpdateWithCpu, 1000);
            //});
        }

        //readonly PerformanceCounter _cpu = new PerformanceCounter("Processor Information", "% Processor Time", "_Total");

        //private double UpdateWithCpu()
        //{
        //    return _cpu.NextValue();
        //}

        //private double GenerateRandomNumber()
        //{
        //    var rnd = new Random();
        //    return rnd.Next(0, 5000);
        //}
    }
}
