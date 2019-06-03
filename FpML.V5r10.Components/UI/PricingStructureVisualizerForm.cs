using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Core.Common;
using FpML.V5r3.Reporting;
using Orion.Util.Serialisation;

namespace Orion.UI
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
            //Instantiate the datagrid
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
