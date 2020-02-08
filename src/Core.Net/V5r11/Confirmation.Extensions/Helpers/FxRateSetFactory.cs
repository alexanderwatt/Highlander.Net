using System.Collections.Generic;
using Orion.Util.Helpers;
using nab.QDS.FpML.V47;

namespace nab.QDS.FpML.V47
{
    public class FxRateSetFactory
    {
        private readonly List<Pair<Asset, BasicAssetValuation>> _assetAndQuotes = new List<Pair<Asset, BasicAssetValuation>>();

        public void AddAssetAndQuotes(Asset underlyingAsset, BasicAssetValuation quotes)
        {
            _assetAndQuotes.Add(new Pair<Asset, BasicAssetValuation>(underlyingAsset, quotes));       
        }

        public FxRateSet Create()
        {
            var result = new FxRateSet();

            var assets = new List<Asset>();
            var quotes = new List<BasicAssetValuation>();

            foreach (var assetAndQuote in _assetAndQuotes)
            {
                assets.Add(assetAndQuote.First);
                quotes.Add(assetAndQuote.Second);
            }

            result.instrumentSet = assets.ToArray();
            result.assetQuote = quotes.ToArray();
            
            return result;
        }
    }
}