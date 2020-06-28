using System.Collections.Generic;
using Orion.Util.Helpers;

namespace FpML.V5r3.Confirmation
{
    public class FxRateSetFactory
    {
        private readonly List<Pair<Asset, BasicAssetValuation>> _assetAndQuotes = new List<Pair<Asset, BasicAssetValuation>>();

        public void AddAssetAndQuotes(Asset underlyingAsset, BasicAssetValuation quotes)
        {
            //TODO Parse the asset id to the type.
            _assetAndQuotes.Add(new Pair<Asset, BasicAssetValuation>(underlyingAsset, quotes));
        }

        public FxRateSet Create()
        {
            var result = new FxRateSet();
            var assets = new List<Asset>();
            var quotes = new List<BasicAssetValuation>();
            var types = new List<ItemsChoiceType22>();
            foreach (var assetAndQuote in _assetAndQuotes)
            {
                assets.Add(assetAndQuote.First);
                quotes.Add(assetAndQuote.Second);
            }
            var instrumenSet = new InstrumentSet {Items = assets.ToArray(), ItemsElementName = types.ToArray()};
            result.instrumentSet = instrumenSet;
            result.assetQuote = quotes.ToArray();           
            return result;
        }
    }
}