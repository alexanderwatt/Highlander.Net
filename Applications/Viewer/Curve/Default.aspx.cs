using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Orion.WebViewer.Curve.Business;
using Orion.WebViewer.Properties;
using Nevron.Chart;
using System.Drawing;
using Nevron.GraphicsCore;

namespace Orion.WebViewer.Curve
{
    public partial class Default : System.Web.UI.Page
    {
        protected string CurveId { get; private set; }
        protected string ClipboardButtonStyle { get; private set;}

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var curveTypes = new List<ListItem> { new ListItem(Resources.UserInteface.AllItems) };
                curveTypes.AddRange(CurveProvider.SupportedPricingStructures.Select(a => new ListItem(a)));
                PricingStructureDropDownList.Items.AddRange(curveTypes.ToArray());
            }
        }

        protected void SubmitButtonClick(object sender, EventArgs e)
        {
            // Reset previous clicks on Search
            CurvesGridView.PageIndex = 0;
            CurvesGridView.SelectedIndex = -1;
            // Enable the binding
            SearchEnabledRadioButton.Checked = true;
        }

        protected void CurvesGridViewInit(object sender, EventArgs e)
        {
            // Set the pagesize from config settings
            CurvesGridView.PageSize = (new Settings()).PageSize;
        }

        protected void CurvesGridViewDataBinding(object sender, EventArgs e)
        {
            bool setChartVisible = false;
            if (CurvesGridView.SelectedValue != null)
            {
                // Set curve on chart
                var curveProvider = new CurveProvider();
                Business.Curve curve = curveProvider.GetCurve(CurvesGridView.SelectedValue.ToString());
                CurveId = CurvesGridView.SelectedValue.ToString();
                if (curve != null && curve.Points != null && curve.Points.Count != 0)
                {
                    setChartVisible = true;
                    PopulateTable(curve);
                    PopulateChart(curve);
                }
                else // must be a surface
                {
                    Surface surface = curveProvider.GetCurveSurface(CurvesGridView.SelectedValue.ToString());
                    if (surface.Strikes != null && surface.Rows != null)
                    {
                        setChartVisible = true;
                        PopulateTable(surface);
                        PopulateChart(surface);
                    }
                }
            }
            Chart.Visible = setChartVisible;
            ClipboardButtonStyle = setChartVisible ? "" : "display:none;";
        }

        protected void CurvesGridViewPageIndexChanged(object sender, EventArgs e)
        {
            // If you change pages, then remove the chosen trade
            CurvesGridView.SelectedIndex = -1;
        }

        protected void CurvesGridViewRowDataBound(object sender, GridViewRowEventArgs e)
        {
            // Handle clicking on a row to choose the trade
            if (e.Row.RowIndex > -1 && e.Row.RowIndex != CurvesGridView.SelectedIndex)
            {
                string reference = ClientScript.GetPostBackEventReference(CurvesGridView, "Select$" + e.Row.RowIndex);
                e.Row.Attributes.Add("onclick", reference);
            }
        }

        private void PopulateChart(Business.Curve curve)
        {
            if (!Chart.RequiresInitialization) return;

            NChart chart = InitializeCurve();

            chart.Axis(StandardAxis.PrimaryX).ScaleConfigurator.Title.Text = curve.XValueName;
            chart.Axis(StandardAxis.PrimaryY).ScaleConfigurator.Title.Text = curve.ValueName;
            Chart.DataBindingManager.AddBinding(0, 0, "XValues", curve.XValues, "");
            Chart.DataBindingManager.AddBinding(0, 0, "Values", curve.Values, "");
            ((NDateTimeScaleConfigurator)chart.Axis(StandardAxis.PrimaryX).ScaleConfigurator).Origin
                = new DateTime(curve.XValues[0].Year, 1, 1);
            //((NLinearScaleConfigurator)chart.Axis(StandardAxis.PrimaryY).ScaleConfigurator).CustomMajorTicks 
            //    = new NDoubleList(curve.ValueTicks.ToArray());
        }

        private NChart InitializeCurve()
        {
            NChart chart = Chart.Charts[0];
            chart.Enable3D = false;

            var dateTimeScale = new NDateTimeScaleConfigurator
                {
                    LabelStyle =
                        {
                            Angle = new NScaleLabelAngle(ScaleLabelAngleMode.View, 90),
                            ContentAlignment = ContentAlignment.MiddleLeft
                        },
                    UseOrigin = true
                };
            chart.Axis(StandardAxis.PrimaryX).ScaleConfigurator = dateTimeScale;

            var linearScale = new NLinearScaleConfigurator();
            //linearScale.UseOrigin = true;
            //linearScale.Origin = 0;
            //linearScale.MajorTickMode = MajorTickMode.CustomTicks;
            chart.Axis(StandardAxis.PrimaryY).ScaleConfigurator = linearScale;

            var line = (NLineSeries)chart.Series.Add(SeriesType.Line);

            line.UseXValues = true;
            line.DataLabelStyle.Visible = false;
            line.Legend.Mode = SeriesLegendMode.None;

            line.ShadowStyle.Type = ShadowType.GaussianBlur;
            line.ShadowStyle.Offset = new NPointL(new NLength(3, NGraphicsUnit.Pixel), new NLength(3, NGraphicsUnit.Pixel));
            line.ShadowStyle.FadeLength = new NLength(5, NGraphicsUnit.Pixel);
            line.ShadowStyle.Color = Color.FromArgb(55, 0, 0, 0);

            line.MarkerStyle.Visible = true;
            line.MarkerStyle.PointShape = PointShape.Cylinder;
            line.MarkerStyle.Height = new NLength(5);
            line.MarkerStyle.Width = new NLength(5);
            line.MarkerStyle.FillStyle = new NColorFillStyle(Color.LightSkyBlue);
            line.MarkerStyle.BorderStyle.Color = Color.Blue;

            return chart;
        }

        private void PopulateChart(Surface dataSurface)
        {
            if (!Chart.RequiresInitialization) return;

            Chart.BorderStyle = BorderStyle.None;
            Chart.BorderColor = Color.White;

            // setup chart
            NChart chart = Chart.Charts[0];
            chart.Enable3D = true;
            chart.Width = 60;
            chart.Depth = 60;
            chart.Height = 40;
            chart.LightModel.SetPredefinedLightModel(PredefinedLightModel.ShinyTopLeft);
            chart.Projection.SetPredefinedProjection(PredefinedProjection.PerspectiveTilted);
            chart.Projection.Elevation = 20;

            // setup Y axis
            var scaleY = new NLinearScaleConfigurator
                {
                    RoundToTickMax = false,
                    RoundToTickMin = false,
                    MajorGridStyle =
                        {
                            ShowAtWalls = new[] {ChartWallType.Left, ChartWallType.Back},
                            LineStyle = {Pattern = LinePattern.Dot}
                        }
                };
            chart.Axis(StandardAxis.PrimaryY).ScaleConfigurator = scaleY;
            scaleY.Title.Text = "Volatility";

            // setup X axis
            var scaleX = new NLinearScaleConfigurator
                {
                    RoundToTickMax = false,
                    RoundToTickMin = false,
                    MajorGridStyle =
                        {
                            ShowAtWalls = new[] {ChartWallType.Floor, ChartWallType.Back},
                            LineStyle = {Pattern = LinePattern.Dot}
                        },
                    Title = {Text = "Strike"}
                };
            chart.Axis(StandardAxis.PrimaryX).ScaleConfigurator = scaleX;

            // setup Z axis
            var scaleZ = new NDateTimeScaleConfigurator
                {
                    RoundToTickMax = false,
                    RoundToTickMin = false,
                    MajorGridStyle =
                        {
                            ShowAtWalls = new[] {ChartWallType.Floor, ChartWallType.Left},
                            LineStyle = {Pattern = LinePattern.Dot}
                        },
                    Title = {Text = "Tenor"},
                };
            chart.Axis(StandardAxis.Depth).ScaleConfigurator = scaleZ;

            // add the surface series
            var surface = (NTriangulatedSurfaceSeries)chart.Series.Add(Nevron.Chart.SeriesType.TriangulatedSurface);
            surface.Name = "Surface";
            surface.Legend.Mode = SeriesLegendMode.SeriesLogic;
            surface.Legend.TextStyle.FontStyle.EmSize = new NLength(8, NGraphicsUnit.Point);
            surface.PositionValue = 10.0;
            surface.SyncPaletteWithAxisScale = false;
            surface.ValueFormatter.FormatSpecifier = "0.00";
            surface.FillStyle = new NColorFillStyle(Color.YellowGreen);

            for (int i = dataSurface.Rows.Count - 1; i >= 0; i--)
            {
                foreach (CurvePoint point in dataSurface.Rows[i])
                {
                    //string toolTip = string.Format("Volatility: {0}\nStrike: {1}\nTenor: {2}",
                    //                               point.DiscountFactor, point.Strike, point.Tenor);

                    surface.XValues.Add(point.Values[0]);
                    surface.ZValues.Add(point.Values[1]);
                    surface.Values.Add(point.Values[2]);
                }
            }
        }

        private void PopulateTable(Business.Curve curve)
        {
            var copyText = new StringBuilder();

            var headerRow = new TableRow();
            foreach (string title in curve.Titles)
            {
                var headerCell = new TableCell
                {
                    Text = title,
                    CssClass = "th"
                };
                headerRow.Cells.Add(headerCell);
            }
            DataTable.Rows.Add(headerRow);

            bool alternateRow = false;
            foreach (CurvePoint point in curve.Points)
            {
                var row = new TableRow();

                var copyRow = new List<string>();
                for (int i = 0; i < curve.Titles.Count; i++)
                {
                    var cell = new TableCell{CssClass = alternateRow ? "RowStyle" : "RowStyleAlternate"};
                    if (point.Values.Count > i && point.Values[i] != null)
                    {
                        if (point.Values[i] is DateTime)
                        {
                            cell.Text = Common.FormatDate(point.Values[i]);
                        }
                        else if (point.Values[i] is decimal)
                        {
                            cell.Text = ((decimal)point.Values[i]).ToString(Resources.UserInteface.NumberFormat);
                        }
                        else
                        {
                            cell.Text = point.Values[i].ToString();
                        }
                        copyRow.Add(point.Values[i].ToString());
                    }
                    else
                    {
                        cell.Text = "";
                        copyRow.Add("");
                    }
                    row.Cells.Add(cell);
                }
                copyText.AppendLine(string.Join("\t", copyRow.ToArray()));
                alternateRow = !alternateRow;
                DataTable.Rows.Add(row);
            }

            ClipboardStore.Text = copyText.ToString();
        }

        private void PopulateTable(Surface surface)
        {
            var copyText = new StringBuilder();

            var headerRow = new TableRow();
            headerRow.Cells.Add(new TableCell());
            foreach (var strike in surface.Strikes)
            {
                var headerCell = new TableCell
                                           {
                                               Text = strike.ToString(CultureInfo.InvariantCulture),
                                               CssClass = "th"
                                           };
                headerRow.Cells.Add(headerCell);
            }
            DataTable.Rows.Add(headerRow);

            bool alternateRow = false;
            foreach (PointBaseCollection curvePoints in surface.Rows)
            {
                var row = new TableRow();
                var headerCell = new TableCell
                                           {
                                               Text = (string)curvePoints.First().Values[3],
                                               CssClass = "th"
                                           };
                row.Cells.Add(headerCell);

                var copyRow = new List<string>();
                foreach (CurvePoint point in curvePoints)
                {
                    string valueString = ((decimal)point.Values[2]).ToString(Resources.UserInteface.NumberFormat);
                    var cell = new TableCell
                                         {
                                             Text = valueString,
                                             CssClass = alternateRow ? "RowStyle" : "RowStyleAlternate"
                                         };
                    row.Cells.Add(cell);
                    copyRow.Add(point.Values[2].ToString());
                }
                copyText.AppendLine(string.Join("\t", copyRow.ToArray()));
                alternateRow = !alternateRow;
                DataTable.Rows.Add(row);
            }
            ClipboardStore.Text = copyText.ToString();
        }
    }
}
