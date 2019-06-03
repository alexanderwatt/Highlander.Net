#region Usings

using System;
using System.Windows.Forms;
using FpML.V5r3.Codes;
using Orion.Util.Logging;

#endregion

namespace TradeViewer
{
    public partial class TradeViewerForm : Form
    {
        private ILogger _logger;
        private TradeQuery _api;

        public TradeViewerForm()
        {
            InitializeComponent();
        }

        private void Form1Load(object sender, EventArgs e)
        {
            _logger = new TextBoxLogger(txtLog);
            _api = new TradeQuery(_logger);
            dtpFrom.Value = DateTime.Today.AddDays(-1);
            dtpTo.Value = DateTime.Today;
        }

        private void BtnLoadClick(object sender, EventArgs e)
        {
            try
            {
                var query = new object[2, 3];
                DateTime dtFrom = dtpFrom.Value.Date;
                DateTime dtTo = dtpTo.Value.Date;
                query[0, 0] = TradeProp.TradeDate;
                query[0, 1] = "GEQ";
                query[0, 2] = dtFrom;
                query[1, 0] = TradeProp.TradeDate;
                query[1, 1] = "LSS";
                query[1, 2] = dtTo.AddDays(1);
                _logger.LogDebug("Loading trades where {0} <= TradeDate <= {1}", dtFrom.ToShortDateString(), dtTo.ToShortDateString());
                object[,] results = _api.QueryTradeIds(query);
                if (results != null)
                {
                    int rowMin = results.GetLowerBound(0);
                    int rowMax = results.GetUpperBound(0);
                    //int colMin = results.GetLowerBound(1);
                    //int colMax = results.GetUpperBound(1);
                    for (int row = rowMin; row <= rowMax; row++)
                        _logger.LogDebug("{0} {1} ({2}) [{3}]",
                            results[row, 0], results[row, 1], results[row, 2], results[row, 3]);
                    _logger.LogDebug("Loaded {0} trades.", (rowMax - rowMin + 1));
                }
                else
                {
                    _logger.LogError("results = (null)");
                }
            }
            catch (Exception excp)
            {
                _logger.Log(excp);
            }

        }
    }
}
