#region References

using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

#endregion

namespace Highlander.ChartControl
{
    /// <summary>
    /// ...
    /// </summary>
    public enum BackgroundImageStyle
    {
        CenterImage = 0,
        TileImage = 1,
        StretchImage = 2,
        UnscaledImage = 3
    }

    /// <summary>
    /// Specifies the types of tick marks for the chart control.
    /// </summary>
    public enum ChartTickMarkTypes
    {
        /// <summary>
        /// No tick marks are displayed.
        /// </summary>
        None,
        /// <summary>
        /// Tick marks are displayed inside the chart area.
        /// </summary>
        Inside,
        /// <summary>
        /// Tick marks are displayed outside the chart area.
        /// </summary>
        Outside,
        /// <summary>
        /// Tick marks are displayed both inside and outside the chart area.
        /// </summary>
        Cross
    }

    /// <summary>
    /// Specifies the styles of the grid lines for the chart control.
    /// </summary>
    public enum ChartGridStyles
    {
        /// <summary>
        /// No grid line is displayed.
        /// </summary>
        None,
        /// <summary>
        /// Minor grid lines are displayed.
        /// </summary>
        Minor,
        /// <summary>
        /// Major grid lines are displayed.
        /// </summary>
        Major
    }

    /// <summary>
    /// Specifies the time units for the time scale of the chart control.
    /// </summary>
    public enum ChartTimeUnits
    {
        /// <summary>
        /// The time scale is expressed in days.
        /// </summary>
        Days,
        /// <summary>
        /// The time scale is expressed in months.
        /// </summary>
        Months,
        /// <summary>
        /// The time scale is expressed in years.
        /// </summary>
        Years
    }

    /// <summary>
    /// Specifies the shapes of the value series marks for the chart control.
    /// </summary>
    public enum ChartMarkShapes
    {
        /// <summary>
        /// No marks are displayed.
        /// </summary>
        None,
        /// <summary>
        /// The marks are displayed as circles.
        /// </summary>
        Circle,
        /// <summary>
        /// The marks are displayed as diamonds.
        /// </summary>
        Diamond,
        /// <summary>
        /// The marks are displayed as squares.
        /// </summary>
        Square,
        /// <summary>
        /// The marks are displayed as triangles.
        /// </summary>
        Triangle
    }

    /// <summary>
    /// Specifies the position at which the legend will be displayed on the chart control.
    /// </summary>
    public enum ChartLegendPosition
    {
        /// <summary>
        /// The legend is displayed on top of the chart.
        /// </summary>
        Top,
        /// <summary>
        /// The legend is displayed on the left of the chart.
        /// </summary>
        Left,
        /// <summary>
        /// The legend is displayed on the right of the chart.
        /// </summary>
        Right,
        /// <summary>
        /// The legend is displayed bellow of the chart.
        /// </summary>
        Bottom
    }

    /// <summary>
    /// Represents a basic time series charting control.
    /// </summary>
    public class Chart : Control
    {
        #region Consts

        private const int MarkSize = 3;
        private const int LargeTickSize = 6;
        private const int SmallTickSize = 3;
        private const int BorderPaddingSize = 6;
        private const int TitlePaddingSize = 6;
        private const int LegendMargin = 4;
        private const float LegendMaxWidth = 0.25F; // max percents that legend can stretch
        private static readonly Color defaultBackColor = SystemColors.Window;
        private static readonly Color defaultForeColor = SystemColors.WindowText;
        private const BackgroundImageStyle DefaultBackgroundImageStyle = BackgroundImageStyle.CenterImage;

        #endregion

        #region Fields

        private BackgroundImageStyle _backgroundImageStyle = DefaultBackgroundImageStyle;
        private readonly ChartTitleSettings _title;
        private readonly ChartTimeAxisSettings _xAxis;
        private readonly ChartValueAxisSettings _yAxis;
        private readonly ChartGridSettings _grid;
        private readonly ChartLegendSettings _legend;
        private readonly ChartSeriesSettings _series;

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class Chart.
        /// </summary>
        public Chart()
        {
            // set control's name
            Name = GetType().Name;
            base.Text = Name;
            // set chart contents
            _title = new ChartTitleSettings(this);
            _xAxis = new ChartTimeAxisSettings(this);
            _yAxis = new ChartValueAxisSettings(this);
            _grid = new ChartGridSettings(this);
            _legend = new ChartLegendSettings(this);
            _series = new ChartSeriesSettings(this);
            // set control style to improve rendering (reduce flicker)
            SetStyle(ControlStyles.UserPaint, true); 
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            // initialize members
            base.BackColor = defaultBackColor;
            base.ForeColor = defaultForeColor;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets/sets the background image drawing style for the panel.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(DefaultBackgroundImageStyle)]
        [Description("Gets/sets the background image drawing style of the control.")]
        public BackgroundImageStyle BackgroundImageStyle
        {
            get => _backgroundImageStyle;
            set
            {
                if (_backgroundImageStyle == value) return;
                _backgroundImageStyle = value;
                Invalidate();
            }
        }

        /// <summary>
        /// Gets/sets the background color of the control.
        /// </summary>
        [Category("Appearance")]
        [Description("Gets/sets the background color of the control.")]
        public new Color BackColor
        {
            get => base.BackColor;
            set => base.BackColor = value == Color.Empty ? defaultBackColor : value;
        }

        protected bool ShouldSerializeBackColor()
        {
            return base.BackColor != defaultBackColor;
        }

        /// <summary>
        /// Gets/sets the foreground color of the control.  
        /// </summary>
        [Category("Appearance")]
        [Description("Gets/sets the foreground color of the control.")]
        public new Color ForeColor
        {
            get => base.ForeColor;
            set => base.ForeColor = value == Color.Empty ? defaultForeColor : value;
        }
        protected bool ShouldSerializeForeColor()
        {
            return base.ForeColor != defaultForeColor;
        }

        /// <summary>
        /// Gets the text associated with this control.
        /// </summary>
        [Browsable(false)]
        [Description("Gets the text associated with this control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text => base.Text;

        /// <summary>
        /// Determines the display settings for the title of the chart control.
        /// </summary>
        [Browsable(true)]
        [Category("Chart")]
        [Description("Determines the display settings for the title of the chart control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartTitleSettings Title => _title;

        /// <summary>
        /// Determines the display settings for the time axis of the chart control.
        /// </summary>
        [Category("Chart")]
        [Description("Determines the display settings for the time axis of the chart control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartTimeAxisSettings XAxis => _xAxis;

        /// <summary>
        /// Determines the display settings for the value axis of the chart control.
        /// </summary>
        [Category("Chart")]
        [Description("Determines the display settings for the value axis of the chart control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartValueAxisSettings YAxis => _yAxis;

        /// <summary>
        /// Determines the display settings for the grid of the chart control.
        /// </summary>
        [Category("Chart")]
        [Description("Determines the display settings for the grid of the chart control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartGridSettings Grid => _grid;

        /// <summary>
        /// Determines the display settings for the legend of the chart control.
        /// </summary>
        [Category("Chart")]
        [Description("Determines the display settings for the legend of the chart control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartLegendSettings Legend => _legend;

        /// <summary>
        /// Determines the display settings for the series of the chart control.
        /// </summary>
        [Category("Chart")]
        [Description("Determines the display settings for the series of the chart control.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ChartSeriesSettings Series => _series;

        #endregion

        #region Methods
        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        protected override void OnPaint(PaintEventArgs pe)
        {
            // call base class implementation
            base.OnPaint(pe);
            // get drawing rectangle
            Rectangle chartArea = ClientRectangle;
            // process background drawing
            CustomGraphics.DrawControlBackgroundImage(pe.Graphics, this, chartArea, BackgroundImage, _backgroundImageStyle);
            // exclude border padding from drawing area
            chartArea.Inflate(-BorderPaddingSize, -BorderPaddingSize);
            // determine chart title's rectangle
            Rectangle titleRect = chartArea;
            if (_title.Text.Length != 0)
            {
                // adjust rectangle to fit chart title
                int titleHeight = (int)pe.Graphics.MeasureString(_title.Text, _title.Font, 0, StringFormat.GenericTypographic).Height;
                titleHeight += TitlePaddingSize;
                titleRect.Height = titleHeight;
                // exclude chart title from drawing area
                chartArea.Y += titleHeight;
                chartArea.Height -= titleHeight;
            }
            // draw chart title
            MeasureChartEventArgs me = new MeasureChartEventArgs(pe.Graphics, titleRect);
            OnMeasureChartTitle(me);
            OnDrawChartTitle(new DrawChartEventArgs(pe.Graphics, me));
            // check if chart legend is displayed
            if (_legend.Visible)
            {
                // determine chart legend drawing area
                Size itemSize = GetLegendItemSize(pe.Graphics);
                // determine legend border size
                int borderWidth = (int)Math.Ceiling(_legend.Border.Weight);
                int legendWidth = borderWidth > 0 ? borderWidth - 1 : 0;
                int legendHeight = legendWidth;
                legendWidth += LegendMargin * 4;
                legendWidth += MarkSize * 4;
                legendWidth += itemSize.Width;
                legendHeight += LegendMargin * 2;
                legendHeight += itemSize.Height;
                // declare legend area rectangle
                Rectangle legendArea = chartArea;
                switch (_legend.Position)
                {
                    case ChartLegendPosition.Top:
                        // determine the legend rectangle
                        legendArea.X = (legendArea.Right - legendWidth) / 2;
                        // exclude legend from drawing area
                        chartArea.Y += legendHeight;
                        chartArea.Height -= legendHeight;
                        break;
                    case ChartLegendPosition.Left:
                        // determine the legend rectangle
                        legendArea.Y = (legendArea.Bottom - legendHeight) / 2;
                        // exclude legend from drawing area
                        chartArea.X += legendWidth;
                        chartArea.Width -= legendWidth;
                        break;
                    case ChartLegendPosition.Right:
                        // determine the legend rectangle
                        legendArea.X = legendArea.Right - legendWidth;
                        legendArea.Y = (legendArea.Bottom - legendHeight) / 2;
                        // exclude legend from drawing area
                        chartArea.Width -= legendWidth;
                        break;
                    case ChartLegendPosition.Bottom:
                        // determine the legend rectangle
                        legendArea.X = (legendArea.Right - legendWidth) / 2;
                        legendArea.Y = legendArea.Bottom - legendHeight;
                        // exclude legend from drawing area
                        chartArea.Height -= legendHeight;
                        break;
                }
                // determine legend size
                legendArea.Size = new Size(legendWidth, legendHeight);
                // draw chart legend
                OnMeasureChartLegend(me = new MeasureChartEventArgs(pe.Graphics, legendArea));
                OnDrawChartLegend(new DrawChartEventArgs(pe.Graphics, me));
            }
            // determine X axis title's rectangle
            Rectangle xTitleRect = chartArea;
            if (_xAxis.Title.Text.Length != 0)
            {
                // NOTE: X axis title affect the chart's height (Y axis size)
                // adjust rectangle to fit X axis title
                int xTitleHeight = (int)pe.Graphics.MeasureString(_xAxis.Title.Text, _xAxis.Title.Font, 0, StringFormat.GenericTypographic).Height;
                xTitleHeight += TitlePaddingSize;
                xTitleRect.Y = chartArea.Bottom - xTitleHeight;
                xTitleRect.Height = xTitleHeight;
                // exclude X axis title from drawing area
                chartArea.Height -= xTitleHeight;
            }
            // determine X axis weight
            int xAxisWeight = (int)Math.Ceiling(_xAxis.Line.Weight);
            // check if Y axis labels are visible
            if (_yAxis.Labels.Visible)
            {
                // NOTE: Y axis labels affect the chart's width (X axis size)
                float maxWidth = pe.Graphics.MeasureString(_yAxis.Scale.Maximum.ToString(), _yAxis.Labels.Font, 0, StringFormat.GenericTypographic).Width;
                xAxisWeight += (int)maxWidth;
            }
            // draw X axis title
            OnMeasureChartXAxisTitle(me = new MeasureChartEventArgs(pe.Graphics, xTitleRect));
            OnDrawChartXAxisTitle(new DrawChartEventArgs(pe.Graphics, me));
            // determine Y axis title's rectangle
            Rectangle yTitleRect = chartArea;
            if (_yAxis.Title.Text.Length != 0)
            {
                // NOTE: Y axis title affect the chart's width (X axis size)
                // adjust rectangle to fit Y axis title
                int yTitleHeight = (int)pe.Graphics.MeasureString(_yAxis.Title.Text, _yAxis.Title.Font, 0, StringFormat.GenericTypographic).Height;
                yTitleHeight += TitlePaddingSize;
                yTitleRect.Width = yTitleHeight;
                // exclude Y axis title from drawing area
                chartArea.X += yTitleHeight;
                chartArea.Width -= yTitleHeight;
            }
            // determine Y axis weight
            int yAxisWeight = (int)Math.Ceiling(_yAxis.Line.Weight);
            // check if X axis labels are visible
            if (_xAxis.Labels.Visible)
            {
                // NOTE: X axis labels affect the chart's height (Y axis size)
                float maxHeight = pe.Graphics.MeasureString(_xAxis.Scale.Maximum.ToString(CultureInfo.InvariantCulture), _xAxis.Labels.Font, 0, StringFormat.GenericTypographic).Height;
                yAxisWeight += (int)maxHeight;
            }
            // draw Y axis title
            OnMeasureChartYAxisTitle(me = new MeasureChartEventArgs(pe.Graphics, yTitleRect));
            OnDrawChartYAxisTitle(new DrawChartEventArgs(pe.Graphics, me));
            // adjust chart drawing region
            chartArea.Inflate(-LargeTickSize, -LargeTickSize);
            chartArea.X += xAxisWeight;
            chartArea.Width -= xAxisWeight;
            chartArea.Height -= yAxisWeight;
            // draw chart X axis
            OnMeasureChartXAxis(me = new MeasureChartEventArgs(pe.Graphics, chartArea));
            OnDrawChartXAxis(new DrawChartEventArgs(pe.Graphics, me));
            // draw chart Y axis
            OnMeasureChartYAxis(me = new MeasureChartEventArgs(pe.Graphics, chartArea));
            OnDrawChartYAxis(new DrawChartEventArgs(pe.Graphics, me));
            // draw chart grid lines
            OnMeasureChartGrid(me = new MeasureChartEventArgs(pe.Graphics, chartArea));
            OnDrawChartGrid(new DrawChartEventArgs(pe.Graphics, me));
            // draw chart value lines
            OnMeasureChartLines(me = new MeasureChartEventArgs(pe.Graphics, chartArea));
            OnDrawChartLines(new DrawChartEventArgs(pe.Graphics, me));
        }

        /// <summary>
        /// Raises the MeasureChartTitle event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartTitle(MeasureChartEventArgs e)
        {
            // check if there are subscribers
            // raise the MeasureChartTitle event.
            MeasureChartTitle?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartTitle event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartTitle(DrawChartEventArgs e)
        {
            // check if there are subscribers
            // raise the DrawChartTitle event.
            DrawChartTitle?.Invoke(this, e);

            // check if there is text to draw
            if (e.Cancel == false && _title.Text.Length != 0)
            {
                // initialize graphic resources
                Brush textBrush = new SolidBrush(_title.Color);
                try
                {
                    // draw the chart's title
                    StringFormat format = CustomGraphics.StringFormat(ContentAlignment.MiddleCenter);
                    e.Graphics.DrawString(_title.Text, _title.Font, textBrush, e.Bounds, format);
                }
                finally
                {
                    // free graphic resources
                    textBrush.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the MeasureChartLegend event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartLegend(MeasureChartEventArgs e)
        {
            // check if there are subscribers for the event
            // raise the MeasureChartLegend event.
            MeasureChartLegend?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartLegend event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartLegend(DrawChartEventArgs e)
        {
            // check if there are subscribers for the event
            // raise the DrawChartLegend event.
            DrawChartLegend?.Invoke(this, e);

            // check if there is text to draw
            if (e.Cancel == false && _legend.Visible)
            {
                // initialize graphic resources
                Pen borderPen = _legend.Border.ToPen();
                Brush textBrush = new SolidBrush(_series.Title.Color);
                Pen linePen = _series.Line.ToPen();
                Pen markBorderPen = new Pen(_series.Mark.BorderColor);
                Brush markFillPen = new SolidBrush(_series.Mark.FillColor);
                try
                {
                    // draw the chart's legend
                    StringFormat format = CustomGraphics.StringFormat(ContentAlignment.MiddleLeft);
                    format.Trimming = StringTrimming.EllipsisCharacter;
                    // determine legend item size
                    Size itemSize = GetLegendItemSize(e.Graphics);
                    int borderWidth = (int)Math.Ceiling(_legend.Border.Weight / 2);
                    borderWidth = borderWidth > 0 ? borderWidth - 1 : 0;
                    Point lineCap = e.Bounds.Location;
                    lineCap.Offset(borderWidth, borderWidth);
                    lineCap.Offset(LegendMargin, LegendMargin);
                    lineCap.Offset(0, Math.Max(itemSize.Height / 2, MarkSize));
                    Point lineCap2 = lineCap;
                    lineCap2.Offset(MarkSize * 4, 0);
                    // draw sample series line & mark
                    e.Graphics.DrawLine(linePen, lineCap, lineCap2);
                    lineCap.Offset(MarkSize * 2, 0);
                    DoDrawChartMark(e.Graphics, markBorderPen, markFillPen, lineCap, _series.Mark.Shape, MarkSize);
                    lineCap2.Offset(LegendMargin, 0);
                    // draw series title
                    Rectangle itemRect = new Rectangle(lineCap2, new Size(itemSize.Width + LegendMargin * 2, 0));
                    e.Graphics.DrawString(_series.Title.Text, _series.Title.Font, textBrush, itemRect, format);
                    // check if we need to draw the legend's border
                    if (_legend.Border.Visible && _legend.Border.Weight > 0)
                    {
                        // draw legend's border
                        e.Graphics.DrawRectangle(borderPen, e.Bounds);
                    }
                }
                finally
                {
                    // free graphic resources
                    borderPen.Dispose();
                    textBrush.Dispose();
                    linePen.Dispose();
                    markBorderPen.Dispose();
                    markFillPen.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the MeasureChartXAxisTitle event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartXAxisTitle(MeasureChartEventArgs e)
        {
            // check if there are subscribers
            // raise the MeasureChartXAxisTitle event.
            MeasureChartXAxisTitle?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartXAxisTitle event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartXAxisTitle(DrawChartEventArgs e)
        {
            // check if there are subscribers
            // raise the DrawChartXAxisTitle event.
            DrawChartXAxisTitle?.Invoke(this, e);
            // check if there is text to draw
            if (e.Cancel == false && _xAxis.Title.Text.Length != 0)
            {
                // initialize graphic resources
                Brush textBrush = new SolidBrush(_xAxis.Title.Color);
                try
                {
                    // draw the time axis title
                    StringFormat format = CustomGraphics.StringFormat(ContentAlignment.MiddleCenter);
                    e.Graphics.DrawString(_xAxis.Title.Text, _xAxis.Title.Font, textBrush, e.Bounds, format);
                }
                finally
                {
                    // free graphics resources
                    textBrush.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the MeasureChartXAxis event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartXAxis(MeasureChartEventArgs e)
        {
            // check if there are subscribers
            // raise the MeasureChartXAxis event.
            MeasureChartXAxis?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartXAxis event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartXAxis(DrawChartEventArgs e)
        {
            // check if there are subscribers
            // raise the DrawChartXAxis event.
            DrawChartXAxis?.Invoke(this, e);
            // check if we need draw
            if (e.Cancel == false && _xAxis.Line.Visible && _xAxis.Line.Weight > 0)
            {
                // determine chart's origin points
                Point start = e.Bounds.Location;
                start.Offset(0, e.Bounds.Height);
                Point end = e.Bounds.Location;
                end.Offset(e.Bounds.Width, e.Bounds.Height);
                // initialize graphic resources
                Pen linePen = _xAxis.Line.ToPen();
                Brush labelBrush = new SolidBrush(_xAxis.Labels.Color);
                try
                {
                    // draw the axis line
                    e.Graphics.DrawLine(linePen, start, end);
                    // initialize variable for tick marks drawing
                    DateTime minDate = _xAxis.Scale.Minimum;
                    DateTime maxDate = _xAxis.Scale.Maximum;
                    TimeSpan dateDiff = (maxDate - minDate);
                    int min = 0;
                    int max = dateDiff.Days;
                    int length = end.X - start.X;
                    int step = _xAxis.Scale.MajorUnit;
                    if (step > 0 && max > min)
                    {
                        // loop to draw the major tick marks (if needed)
                        for (int tick = min; tick <= max; tick += step)
                        {
                            // calculate tick's position on the axis
                            double proc = (double)(tick - min) / (max - min);
                            int tickOffset = (int)Math.Floor(proc * length);
                            DateTime tickDate = minDate.AddDays(tick);
                            string tickLabel = tickDate.ToString("d/MM");
                            Point tickCap = start;
                            tickCap.Offset(tickOffset, 0);
                            // draw major tick marks (with labels, if required)
                            DoDrawAxisTick(e.Graphics,
                                                linePen,
                                                tickCap,
                                                _xAxis.MajorTick,
                                                true,
                                                false,
                                                _xAxis.Labels.Visible ? tickLabel : null,
                                                _xAxis.Labels.Rotation,
                                                labelBrush,
                                                _xAxis.Labels.Font);
                        }
                    }
                    step = _xAxis.Scale.MinorUnit;
                    if (step > 0 && max > min)
                    {
                        // loop to draw the minor tick marks (if needed)
                        if (_xAxis.MinorTick != ChartTickMarkTypes.None)
                        {
                            for (int tick = min; tick <= max; tick += step)
                            {
                                // calculate tick's position on the axis
                                double proc = (double)(tick - min) / (max - min);
                                int tickOffset = (int)Math.Floor(proc * length);
                                Point tickCap = start;
                                tickCap.Offset(tickOffset, 0);
                                // draw minor tick marks
                                DoDrawAxisTick(e.Graphics,
                                                    linePen,
                                                    tickCap,
                                                    _xAxis.MinorTick,
                                                    false,
                                                    false,
                                                    null,
                                                    _xAxis.Labels.Rotation,
                                                    null,
                                                    null);
                            }
                        }
                    }
                }
                finally
                {
                    // free graphics resources
                    linePen.Dispose();
                    labelBrush.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the MeasureChartYAxisTitle event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartYAxisTitle(MeasureChartEventArgs e)
        {
            // check if there are subscribers
            // raise the MeasureChartYAxisTitle event.
            MeasureChartYAxisTitle?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartYAxisTitle event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartYAxisTitle(DrawChartEventArgs e)
        {
            // check if there are subscribers
            // raise the DrawChartYAxisTitle event.
            DrawChartYAxisTitle?.Invoke(this, e);
            // check if there is text to draw
            if (e.Cancel == false && _yAxis.Title.Text.Length != 0)
            {
                // initialize graphic resources
                Brush textBrush = new SolidBrush(_yAxis.Title.Color);
                try
                {
                    // draw the value axis title
                    StringFormat format = CustomGraphics.StringFormat(ContentAlignment.MiddleCenter);
                    format.FormatFlags |= StringFormatFlags.DirectionVertical;
                    e.Graphics.DrawString(_yAxis.Title.Text, _yAxis.Title.Font, textBrush, e.Bounds, format);
                    format.FormatFlags &= ~StringFormatFlags.DirectionVertical;
                }
                finally
                {
                    // free graphics resources
                    textBrush.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the MeasureChartYAxis event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartYAxis(MeasureChartEventArgs e)
        {
            // check if there are subscribers
            // raise the MeasureChartYAxis event.
            MeasureChartYAxis?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartYAxis event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartYAxis(DrawChartEventArgs e)
        {
            // check if there are subscribers
            // raise the DrawChartYAxis event.
            DrawChartYAxis?.Invoke(this, e);
            // check if we need draw
            if (e.Cancel == false && _yAxis.Line.Visible && _yAxis.Line.Weight > 0)
            {
                // determine chart's origin points
                Point start = e.Bounds.Location;
                start.Offset(0, 0);
                Point end = e.Bounds.Location;
                end.Offset(0, e.Bounds.Height - 0);
                // initialize graphic resources
                Pen linePen = _yAxis.Line.ToPen();
                Brush labelBrush = new SolidBrush(_yAxis.Labels.Color);
                try
                {
                    // draw the axis line
                    e.Graphics.DrawLine(linePen, start, end);
                    // initialize variable for tick marks drawing
                    int min = _yAxis.Scale.Minimum;
                    int max = _yAxis.Scale.Maximum;
                    int length = end.Y - start.Y;
                    int step = _yAxis.Scale.MajorUnit;
                    if (step > 0 && max > min)
                    {
                        // loop to draw the major tick marks (if needed)
                        for (int tick = min; tick <= max; tick += step)
                        {
                            // calculate tick's position on the axis
                            double proc = (double)(tick - min) / (max - min);
                            int tickOffset = (int)Math.Floor(proc * length);
                            Point tickCap = end;
                            tickCap.Offset(0, -tickOffset);
                            // draw major tick marks (with labels, if required)
                            DoDrawAxisTick(e.Graphics,
                                                linePen,
                                                tickCap,
                                                _yAxis.MajorTick,
                                                true,
                                                true,
                                                _yAxis.Labels.Visible ? tick.ToString() : null,
                                                _yAxis.Labels.Rotation,
                                                labelBrush,
                                                _yAxis.Labels.Font);
                        }
                    }
                    step = _yAxis.Scale.MinorUnit;
                    if (step > 0 && max > min)
                    {
                        // loop to draw the minor tick marks (if needed)
                        if (_yAxis.MinorTick != ChartTickMarkTypes.None)
                        {
                            for (int tick = min; tick <= max; tick += step)
                            {
                                // calculate tick's position on the axis
                                double proc = (double)(tick - min) / (max - min);
                                int tickOffset = (int)Math.Floor(proc * length);
                                Point tickCap = end;
                                tickCap.Offset(0, -tickOffset);
                                // draw minor tick marks
                                DoDrawAxisTick(e.Graphics,
                                                    linePen,
                                                    tickCap,
                                                    _yAxis.MinorTick,
                                                    false,
                                                    true,
                                                    null,
                                                    _yAxis.Labels.Rotation,
                                                    null,
                                                    null);
                            }
                        }
                    }
                }
                finally
                {
                    // free graphics resources
                    linePen.Dispose();
                    labelBrush.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the MeasureChartGrid event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartGrid(MeasureChartEventArgs e)
        {
            // check if there are subscribers
            // raise the MeasureChartGrid event.
            MeasureChartGrid?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartGrid event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartGrid(DrawChartEventArgs e)
        {
            // check if there are subscribers
            // raise the DrawChartGrid event.
            DrawChartGrid?.Invoke(this, e);
            // check if we need draw
            if (e.Cancel == false && _grid.Line.Visible && _grid.Line.Weight > 0)
            {
                // determine chart's origin points
                Point start = e.Bounds.Location;
                start.Offset(0, e.Bounds.Height);
                Point end = e.Bounds.Location;
                end.Offset(e.Bounds.Width, 0);
                // initialize graphic resources
                Pen linePen = _grid.Line.ToPen();
                try
                {
                    // draw the grid lines for the time axis
                    if (_grid.XAxisStyle != ChartGridStyles.None)
                    {
                        // init scale variables
                        DateTime minDate = _xAxis.Scale.Minimum;
                        DateTime maxDate = _xAxis.Scale.Maximum;
                        TimeSpan dateDiff = (maxDate - minDate);
                        int min = 0;
                        int max = dateDiff.Days;
                        int length = end.X - start.X;
                        // check if valid scale
                        if (max > min && length > 0)
                        {
                            switch (_grid.XAxisStyle)
                            {
                                case ChartGridStyles.Major:
                                    if (_xAxis.Scale.MajorUnit > 0)
                                    {
                                        // draw major grid lines for time axis
                                        DoDrawXGridLines(e.Graphics, linePen, start, end, min, max, length, _xAxis.Scale.MajorUnit);
                                    }
                                    break;

                                case ChartGridStyles.Minor:
                                    if (_xAxis.Scale.MinorUnit > 0)
                                    {
                                        // draw minor grid lines for time axis
                                        DoDrawXGridLines(e.Graphics, linePen, start, end, min, max, length, _xAxis.Scale.MinorUnit);
                                    }
                                    break;
                            }
                        }
                    }
                    // draw the grid lines for the value axis
                    if (_grid.YAxisStyle != ChartGridStyles.None)
                    {
                        // init scale variables
                        int min = _yAxis.Scale.Minimum;
                        int max = _yAxis.Scale.Maximum;
                        int length = start.Y - end.Y;
                        // check if valid scale
                        if (max > min && length > 0)
                        {
                            switch (_grid.YAxisStyle)
                            {
                                case ChartGridStyles.Major:
                                    if (_yAxis.Scale.MajorUnit > 0)
                                    {
                                        // draw major grid lines for value axis
                                        DoDrawYGridLines(e.Graphics, linePen, start, end, min, max, length, _yAxis.Scale.MajorUnit);
                                    }
                                    break;

                                case ChartGridStyles.Minor:
                                    if (_yAxis.Scale.MinorUnit > 0)
                                    {
                                        // draw minor grid lines for value axis
                                        DoDrawYGridLines(e.Graphics, linePen, start, end, min, max, length, _yAxis.Scale.MinorUnit);
                                    }
                                    break;
                            }
                        }
                    }
                }
                finally
                {
                    // free graphics resources
                    linePen.Dispose();
                }
            }
        }

        /// <summary>
        /// Raises the MeasureChartLines event.
        /// </summary>
        /// <param name="e">A MeasureChartEventArgs object that contains the event data.</param>
        protected virtual void OnMeasureChartLines(MeasureChartEventArgs e)
        {
            // check if there are subscribers
            // raise the MeasureChartLines event.
            MeasureChartLines?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the DrawChartLines event.
        /// </summary>
        /// <param name="e">A DrawChartEventArgs object that contains the event data.</param>
        protected virtual void OnDrawChartLines(DrawChartEventArgs e)
        {
            // check if there are subscribers
            // raise the DrawChartLines event.
            DrawChartLines?.Invoke(this, e);
            // check if we need draw
            if (e.Cancel == false && _series.Values.Count > 0)
            {
                // calculate new clipping rectangle
                Rectangle clipRect = e.Bounds;
                clipRect.Inflate(LargeTickSize, LargeTickSize);
                // reset clipping region
                Region oldClip = e.Graphics.Clip;
                e.Graphics.Clip = new Region(clipRect);
                try
                {
                    // determine coordinates
                    Point start = e.Bounds.Location;
                    start.Offset(0, e.Bounds.Height);
                    Point end = e.Bounds.Location;
                    end.Offset(e.Bounds.Width, 0);

                    // calculate parameters for X scale
                    DateTime minDate = _xAxis.Scale.Minimum;
                    DateTime maxDate = _xAxis.Scale.Maximum;
                    TimeSpan dateDiff = maxDate - minDate;
                    int xMin = 0;
                    int xMax = dateDiff.Days;
                    int xLen = end.X - start.X;
                    // calculate parameters for Y scale
                    double yMin = _yAxis.Scale.Minimum;
                    double yMax = _yAxis.Scale.Maximum;
                    double yLen = start.Y - end.Y;
                    // check if we have something to draw
                    if (xMax > xMin && yMax > yMin)
                    {
                        // initialize graphics resources
                        Pen linePen = _series.Line.ToPen();
                        Pen borderPen = new Pen(_series.Mark.BorderColor);
                        Brush fillBrush = new SolidBrush(_series.Mark.FillColor);
                        Brush labelBrush = new SolidBrush(_series.Labels.Color);
                        Pen projLine = _series.Projections.Line.ToPen();
                        Brush projBrush = new SolidBrush(_series.Projections.Labels.Color);
                        try
                        {
                            // declare residual mark
                            Point prev = Point.Empty;
                            string dateLabel = string.Empty;
                            string valueLabel = string.Empty;
                            // sort list by date (bubble sort)
                            int count = _series.Values.Count;
                            ChartSeriesValue[] sortedValues = new ChartSeriesValue[count];
                            _series.Values.CopyTo(sortedValues);
                            // actual sorting loop (bubble sort)
                            for (int bubble = 0; bubble < count; bubble++)
                            {
                                for (int lookup = bubble + 1; lookup < count; lookup++)
                                {
                                    if (sortedValues[bubble].Date < sortedValues[lookup].Date)
                                    {
                                        ChartSeriesValue temp = sortedValues[bubble];
                                        sortedValues[bubble] = sortedValues[lookup];
                                        sortedValues[lookup] = temp;
                                    }
                                }
                            }
                            // draw chart line
                            foreach (ChartSeriesValue seriesValue in sortedValues)
                            {
                                // determine position on time scale
                                dateDiff = seriesValue.Date - minDate;
                                int xPos = dateDiff.Days;
                                double xProc = (double)(xPos - xMin) / (xMax - xMin);
                                int xValue = start.X + (int)Math.Floor(xProc * xLen);
                                // determine position on value scale
                                double yProc = ((double)seriesValue.Value - yMin) / (yMax - yMin);
                                int yValue = start.Y - (int)Math.Floor(yProc * yLen);
                                Point crt = new Point(xValue, yValue);
                                if (prev.IsEmpty == false)
                                {
                                    // draw chart line
                                    e.Graphics.DrawLine(linePen, prev, crt);
                                    // draw mark projection
                                    DoDrawChartProjection(e.Graphics,
                                                               projLine,
                                                               projBrush,
                                                               _series.Projections.Labels.Font,
                                                               prev,
                                                               start.Y,
                                                               _series.Projections.Labels.Rotation,
                                                               dateLabel);
                                    // draw current mark
                                    DoDrawChartMark(e.Graphics,
                                                         borderPen,
                                                         fillBrush,
                                                         prev,
                                                         _series.Mark.Shape,
                                                         MarkSize);
                                    // check if series labels are required
                                    if (_series.Labels.Visible)
                                    {
                                        // draw the label for current mark
                                        DoDrawChartMarkLabel(e.Graphics,
                                                                  labelBrush,
                                                                  _series.Labels.Font,
                                                                  prev,
                                                                  MarkSize,
                                                                  _series.Labels.Rotation,
                                                                  valueLabel);
                                    }
                                }
                                // store value for use in the next loop
                                dateLabel = seriesValue.Date.ToString("d/MM");
                                valueLabel = seriesValue.Value.ToString(CultureInfo.InvariantCulture);
                                prev = crt;
                            }
                            // end the chart (use the values from the last loop from previous foreach statement)
                            if (prev.IsEmpty == false)
                            {
                                // draw mark projection
                                DoDrawChartProjection(e.Graphics,
                                                           projLine,
                                                           projBrush,
                                                           _series.Projections.Labels.Font,
                                                           prev,
                                                           start.Y,
                                                           _series.Projections.Labels.Rotation,
                                                           dateLabel);
                                // draw residual mark
                                DoDrawChartMark(e.Graphics,
                                                     borderPen,
                                                     fillBrush,
                                                     prev,
                                                     _series.Mark.Shape,
                                                     MarkSize);
                                // check if series labels are required
                                if (_series.Labels.Visible)
                                {
                                    // draw label for residual mark
                                    DoDrawChartMarkLabel(e.Graphics,
                                                              labelBrush,
                                                              _series.Labels.Font,
                                                              prev,
                                                              MarkSize,
                                                              _series.Labels.Rotation,
                                                              valueLabel);
                                }
                            }
                        }
                        finally
                        {
                            // free graphics resources
                            linePen.Dispose();
                            borderPen.Dispose();
                            fillBrush.Dispose();
                            labelBrush.Dispose();
                            projLine.Dispose();
                            projBrush.Dispose();
                        }
                    }
                }
                finally
                {
                    // restore old clipping region 
                    e.Graphics.Clip = oldClip;
                }
            }
        }
		
        /// <summary>
        /// Draws the grid lines for the time axis.
        /// </summary>
        private void DoDrawXGridLines(Graphics g,
                                      Pen pen,
                                      Point start,
                                      Point end,
                                      int min,
                                      int max,
                                      int length,
                                      int step)
        {
            // get the width of the axis
            int axisWidth = (int)_yAxis.Line.Weight;
            // draw all grid lines
            for (int line = min + step; line <= max; line += step)
            {
                // calculate the offset from the chart's origin
                double proc = (double)(line - min) / (max - min);
                int gridOffset = (int)Math.Floor(proc * length);
                // determine grid lines caps
                Point gridCap = start;
                gridCap.Offset(gridOffset, -axisWidth);
                Point gridCap2 = end;
                gridCap2.Offset(gridOffset - length, 0);
                // draw the grid line
                g.DrawLine(pen, gridCap, gridCap2);
            }
        }
		
        /// <summary>
        /// Draws the grid lines for the value axis.
        /// </summary>
        private void DoDrawYGridLines(Graphics g,
                                      Pen pen,
                                      Point start,
                                      Point end,
                                      int min,
                                      int max,
                                      int length,
                                      int step)
        {
            // get the width of the axis
            int axisWidth = (int)_xAxis.Line.Weight;
            // draw all grid lines
            for (int line = min + step; line <= max; line += step)
            {
                // calculate the offset from the chart's origin
                double proc = (double)(line - min) / (max - min);
                int gridOffset = (int)Math.Floor(proc * length);
                // determine grid lines caps
                Point gridCap = start;
                gridCap.Offset(axisWidth, -gridOffset);
                Point gridCap2 = end;
                gridCap2.Offset(0, length - gridOffset);
                // draw the grid line
                g.DrawLine(pen, gridCap, gridCap2);
            }
        }

        /// <summary>
        /// Draws the tick marks for time or values axis.
        /// </summary>
        private void DoDrawAxisTick(Graphics g,
                                    Pen pen,
                                    Point point,
                                    ChartTickMarkTypes tickType,
                                    bool major,
                                    bool vertical,
                                    string label,
                                    float rotation,
                                    Brush labelBrush,
                                    Font labelFont)
        {
            // determine tick size
            int offset = major ? LargeTickSize : SmallTickSize;
            // declare tick line caps
            Point tickCapIn = point;
            Point tickCapOut = point;
            // determine tick line caps (based on tick type settings)
            switch (tickType)
            {
                case ChartTickMarkTypes.Outside:
                    if (vertical)
                    {
                        tickCapOut.Offset(-offset, 0);
                    }
                    else
                    {
                        tickCapOut.Offset(0, offset);
                    }
                    break;
                case ChartTickMarkTypes.Inside:
                    if (vertical)
                    {
                        tickCapIn.Offset(offset, 0);
                    }
                    else
                    {
                        tickCapIn.Offset(0, -offset);
                    }
                    break;
                case ChartTickMarkTypes.Cross:
                    if (vertical)
                    {
                        tickCapIn.Offset(offset, 0);
                        tickCapOut.Offset(-offset, 0);
                    }
                    else
                    {
                        tickCapIn.Offset(0, -offset);
                        tickCapOut.Offset(0, offset);
                    }
                    break;
            }
            // check if we need to draw the tick mark
            if (_yAxis.MajorTick != ChartTickMarkTypes.None)
            {
                g.DrawLine(pen, tickCapIn, tickCapOut);
            }
            // draw tick mark label (if required)
            if (label != null)
            {
                StringFormat format = CustomGraphics.StringFormat(vertical ? ContentAlignment.MiddleRight : ContentAlignment.TopCenter);
                if (rotation != 0.0F)
                {
                    g.TranslateTransform(tickCapOut.X, tickCapOut.Y);
                    g.RotateTransform(rotation);
                    g.DrawString(label, labelFont, labelBrush, Point.Empty, format);
                    g.ResetTransform();
                }
                else
                {
                    g.DrawString(label, labelFont, labelBrush, tickCapOut, format);
                }
            }
        }

        /// <summary>
        /// Draws the projection from a chart series mark to the time axis.
        /// </summary>
        private void DoDrawChartProjection(Graphics g,
                                           Pen linePen,
                                           Brush labelBrush,
                                           Font labelFont,
                                           Point chartPoint,
                                           int yAxisOrigin,
                                           float rotation,
                                           string label)
        {
            // get the width of the axis
            int axisWidth = (int)_xAxis.Line.Weight;
            // check if we need to draw projections
            if (_series.Projections.Visible && _series.Projections.Line.Weight > 0)
            {
                // determine projection
                Point projection = chartPoint;
                projection.Y = yAxisOrigin - axisWidth;
                // draw projection line
                g.DrawLine(linePen, projection, chartPoint);
                // draw projection label (if required)
                if (_series.Projections.Labels.Visible)
                {
                    if (rotation != 0.0F)
                    {
                        g.TranslateTransform(projection.X, projection.Y);
                        g.RotateTransform(rotation);
                        StringFormat format = CustomGraphics.StringFormat(ContentAlignment.MiddleLeft);
                        g.DrawString(label, labelFont, labelBrush, Point.Empty, format);
                        g.ResetTransform();
                    }
                    else
                    {
                        StringFormat format = CustomGraphics.StringFormat(ContentAlignment.BottomLeft);
                        g.DrawString(label, labelFont, labelBrush, projection, format);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the chart series mark using the specified shape.
        /// </summary>
        private void DoDrawChartMark(Graphics g,
                                     Pen borderPen,
                                     Brush fillBrush,
                                     Point markPoint,
                                     ChartMarkShapes shape,
                                     int markSize)
        {
            switch (shape)
            {
                case ChartMarkShapes.Circle:
                    Rectangle circle = new Rectangle(markPoint.X - markSize,
                                                     markPoint.Y - markSize,
                                                     markSize * 2,
                                                     markSize * 2);
                    g.FillEllipse(fillBrush, circle);
                    g.DrawEllipse(borderPen, circle);
                    break;
                case ChartMarkShapes.Diamond:
                    Point[] diamond = new Point[4];
                    diamond[0] = markPoint;
                    diamond[0].Offset(0, markSize);
                    diamond[1] = markPoint;
                    diamond[1].Offset(markSize, 0);
                    diamond[2] = markPoint;
                    diamond[2].Offset(0, -markSize);
                    diamond[3] = markPoint;
                    diamond[3].Offset(-markSize, 0);
                    g.FillPolygon(fillBrush, diamond);
                    g.DrawPolygon(borderPen, diamond);
                    break;
                case ChartMarkShapes.Square:
                    Rectangle square = new Rectangle(markPoint.X - markSize,
                                                     markPoint.Y - markSize,
                                                     markSize * 2,
                                                     markSize * 2);
                    g.FillRectangle(fillBrush, square);
                    g.DrawRectangle(borderPen, square);
                    break;
                case ChartMarkShapes.Triangle:
                    Point[] triangle = new Point[3];
                    triangle[0] = markPoint;
                    triangle[0].Offset(0, -markSize);
                    triangle[1] = markPoint;
                    triangle[1].Offset(markSize, markSize);
                    triangle[2] = markPoint;
                    triangle[2].Offset(-markSize, markSize);
                    g.FillPolygon(fillBrush, triangle);
                    g.DrawPolygon(borderPen, triangle);
                    break;
            }
        }

        /// <summary>
        /// Draws the chart series mark label if required.
        /// </summary>
        private void DoDrawChartMarkLabel(Graphics g,
                                          Brush labelBrush,
                                          Font labelFont,
                                          Point markPoint,
                                          int markSize,
                                          float rotation,
                                          string label)
        {
            // determine label position relative to mark position
            Point labelPos = markPoint;
            labelPos.Offset(markSize, -markSize);
            // draw the label besides the mark
            StringFormat format = CustomGraphics.StringFormat(ContentAlignment.BottomCenter);
            if (rotation != 0.0F)
            {
                g.TranslateTransform(labelPos.X, labelPos.Y);
                g.RotateTransform(rotation);
                g.DrawString(label, labelFont, labelBrush, Point.Empty, format);
                g.ResetTransform();
            }
            else
            {
                g.DrawString(label, labelFont, labelBrush, labelPos, format);
            }
        }

        /// <summary>
        /// Determines the size of the chart legend item.
        /// </summary>
        /// <returns>A Size object representing the size of the chart legend item.</returns>
        private Size GetLegendItemSize(Graphics g)
        {
            // determine the size of the title of the series
            SizeF stringSize = g.MeasureString(_series.Title.Text, _series.Title.Font, 0, StringFormat.GenericTypographic);
            // determine the legend item's width
            float maxEstate = (float)Math.Ceiling(LegendMaxWidth * Width);
            int itemWidth = (int)Math.Min(maxEstate, Math.Ceiling(stringSize.Width));
            // determine the legend item's width
            int itemHeight = Math.Max(MarkSize, (int)Math.Ceiling(stringSize.Height));
            // return the computed size of the legend item
            return new Size(itemWidth, itemHeight);
        }

        #endregion

        #region Events
        /// <summary>
        /// Occurs before the chart title is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart title is about to be drawn.")]
        public event DrawChartEventHandler DrawChartTitle;

        /// <summary>
        /// Occurs after the area where chart title will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart title will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartTitle;

        /// <summary>
        /// Occurs before the chart legend is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart legend is about to be drawn.")]
        public event DrawChartEventHandler DrawChartLegend;

        /// <summary>
        /// Occurs after the area where chart legend will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart legend will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartLegend;

        /// <summary>
        /// Occurs before the chart X axis is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart time axis is about to be drawn.")]
        public event DrawChartEventHandler DrawChartXAxis;

        /// <summary>
        /// Occurs after the area where chart X axis will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart time axis will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartXAxis;

        /// <summary>
        /// Occurs before the chart time axis title is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart time axis title is about to be drawn.")]
        public event DrawChartEventHandler DrawChartXAxisTitle;

        /// <summary>
        /// Occurs after the area where chart time axis title will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart time axis title will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartXAxisTitle;

        /// <summary>
        /// Occurs before the chart values axis is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart values axis is about to be drawn.")]
        public event DrawChartEventHandler DrawChartYAxis;

        /// <summary>
        /// Occurs after the area where chart values axis will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart values axis will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartYAxis;

        /// <summary>
        /// Occurs before the chart values axis title is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart values axis title is about to be drawn.")]
        public event DrawChartEventHandler DrawChartYAxisTitle;

        /// <summary>
        /// Occurs after the area where chart values axis title will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart values axis title will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartYAxisTitle;

        /// <summary>
        /// Occurs before the chart grid is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart grid is about to be drawn.")]
        public event DrawChartEventHandler DrawChartGrid;

        /// <summary>
        /// Occurs after the area where chart grid will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart grid will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartGrid;

        /// <summary>
        /// Occurs before the chart lines is about to be drawn.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs before the chart lines is about to be drawn.")]
        public event DrawChartEventHandler DrawChartLines;

        /// <summary>
        /// Occurs after the area where chart lines will be drawn is measured.
        /// </summary>
        [Category("Drawing")]
        [Description("Occurs after the area where chart lines will be drawn is measured.")]
        public event MeasureChartEventHandler MeasureChartLines;

        #endregion
    }

    /// <summary>
    /// Provides data for the MeasureChart event.  
    /// </summary>
    public class MeasureChartEventArgs : EventArgs
    {
        #region Fields

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class MeasureChartEventArgs.
        /// </summary>
        /// <param name="graphics">A Graphics object used to perform drawing on the </param>
        /// <param name="bounds">A Rectangle object representing the bounding rectangle of the item being drawn.</param>
        public MeasureChartEventArgs(Graphics graphics, Rectangle bounds)
        {
            Graphics = graphics;
            Bounds = bounds;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Represents the Graphics object used to perform drawing on the 
        /// </summary>
        public Graphics Graphics { get; }

        /// <summary>
        /// Represents the bounding rectangle of the item being drawn.
        /// </summary>
        public Rectangle Bounds { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents the method that will handle the MeasureChart event of a   
    /// </summary>
    public delegate void MeasureChartEventHandler(object sender, MeasureChartEventArgs e);

    /// <summary>
    /// Provides data for the DrawChart event.  
    /// </summary>
    public class DrawChartEventArgs : EventArgs
    {
        #region Fields

        private readonly MeasureChartEventArgs _measure;

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of class DrawChartEventArgs.
        /// </summary>
        /// <param name="graphics">A Graphics object used to perform drawing on the </param>
        /// <param name="measure">A MeasureChartEventArgs object representing the measures used to perform drawing of current item.</param>
        public DrawChartEventArgs(Graphics graphics, MeasureChartEventArgs measure)
        {
            Graphics = graphics;
            _measure = measure;
        }

        #endregion

        #region Properties
        /// <summary>
        /// Allows the user to cancel the default drawing of the current item.
        /// </summary>
        public bool Cancel { get; set; } = false;

        /// <summary>
        /// Represents the Graphics object used to perform drawing on the chart.
        /// </summary>
        public Graphics Graphics { get; }

        /// <summary>
        /// Represents the bounding rectangle of current item.
        /// </summary>
        public Rectangle Bounds => _measure.Bounds;

        /// <summary>
        /// Represents the measures used to perform drawing of current item.
        /// </summary>
        public MeasureChartEventArgs MeasureEventArgs => _measure;

        #endregion
    }

    /// <summary>
    /// Represents the method that will handle the DrawChart event of a chart.  
    /// </summary>
    public delegate void DrawChartEventHandler(object sender, DrawChartEventArgs e);
}