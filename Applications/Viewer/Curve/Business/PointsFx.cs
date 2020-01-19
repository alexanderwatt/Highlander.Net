using System;
using System.Collections.Generic;
using System.Linq;
using nab.QDS.FpML.V47;

namespace Orion.WebViewer.Curve.Business
{
    public class PointFxCollection : PointBaseCollection
    {
        public PointFxCollection(PricingStructureValuation pricingStructure)
        {
            var fxCurve = (FxCurveValuation) pricingStructure;

            if (fxCurve.fxForwardCurve != null && fxCurve.fxForwardCurve.point != null
                && fxCurve.spotRate != null && fxCurve.spotRate.assetQuote != null)
            {
                TermPoint[] fxForwards = fxCurve.fxForwardCurve.point;
                BasicAssetValuation[] spotRates = fxCurve.spotRate.assetQuote;

                var combinedCurve
                    = from f in fxForwards
                      join s in spotRates on f.id equals s.objectReference.href into fs
                      from fs2 in fs.DefaultIfEmpty()
                      select new CurvePoint((DateTime) f.term.Items[0],
                                            (double) f.mid,
                                            new List<object>
                                                {
                                                    f.id,
                                                    (DateTime) f.term.Items[0],
                                                    f.mid,
                                                    fs2 == null ? null : (decimal?) fs2.quote[0].value
                                                });

                AddRange(combinedCurve);
            }
        }

        public override List<string> Titles
        {
            get
            {
                return new List<string>
                             {
                                 "Asset",
                                 "Date",
                                 "FX Forward Rate",
                                 "Input Rate",
                             };
            }
        }
    }
}
