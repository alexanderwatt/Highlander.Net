using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base class which defines the data required by all asset controllers (i.e. Type A Models)
    /// </summary>
    public sealed class AssetControllerData : IAssetControllerData
    {
        /// <summary>
        /// Gets or sets the basic asset valuation.
        /// </summary>
        /// <value>The basic asset valuation.</value>
        public BasicAssetValuation BasicAssetValuation { get; set; }

        /// <summary>
        /// Gets or sets the valuation date.
        /// </summary>
        /// <value>The valuation date.</value>
        public DateTime ValuationDate { get; set; }

        /// <summary>
        /// Gets or sets the market environment.
        /// </summary>
        /// <value>The market environment.</value>
        public IMarketEnvironment MarketEnvironment { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetControllerData"/> class.
        /// </summary>
        public AssetControllerData() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetControllerData"/> class.
        /// </summary>
        /// <param name="basicAssetValuation">The basic asset valuation.</param>
        /// <param name="valutaionDate">The valutaion date.</param>
        /// <param name="market">The market.</param>
        public AssetControllerData(BasicAssetValuation basicAssetValuation, DateTime valutaionDate, IMarketEnvironment market)
        {
            BasicAssetValuation = basicAssetValuation;
            ValuationDate = valutaionDate;
            MarketEnvironment = market;
        }

        /// <summary>
        /// Creates the asset controller data.
        /// </summary>
        /// <param name="metrics">The metrics.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="market">The market.</param>
        /// <returns></returns>
        public IAssetControllerData CreateAssetControllerData(string[] metrics, DateTime baseDate, IMarketEnvironment market)
        {
            var bav = new BasicAssetValuation();

            var quotes = new BasicQuotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                var quotation = new BasicQuotation
                                    {
                                        valueSpecified = true,
                                        value = 0.0m,
                                        measureType = new AssetMeasureType
                                                          {
                                                              Value =
                                                                  metric
                                                          }
                                    };
                quotes[index] = quotation;
                index++;
            }
            bav.quote = quotes;
            return new AssetControllerData(bav, baseDate, market);
        }
    }
}