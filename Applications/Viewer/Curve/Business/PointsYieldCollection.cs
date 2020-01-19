using System;
using System.Collections.Generic;
using System.Linq;
using nab.QDS.FpML.V47;

namespace Orion.WebViewer.Curve.Business
{
    public class PointYieldCollection : PointBaseCollection
    {
        private BasicAssetValuation[] Inputs { get; set; }
        private TermPoint[] ZeroPoints { get; set; }
        private TermPoint[] DiscountFactors { get; set; }

        public PointYieldCollection(PricingStructureValuation pricingStructure)
        {
            var yieldCurve = (YieldCurveValuation)pricingStructure;

            if (yieldCurve.discountFactorCurve != null
                && yieldCurve.discountFactorCurve.point != null)
            {
                DiscountFactors = yieldCurve.discountFactorCurve.point;
            }

            if (yieldCurve.zeroCurve != null
                && yieldCurve.zeroCurve.rateCurve != null
                && yieldCurve.zeroCurve.rateCurve.point != null)
            {
                ZeroPoints = yieldCurve.zeroCurve.rateCurve.point;
            }

            if (yieldCurve.inputs != null && yieldCurve.inputs.assetQuote != null)
            {
                Inputs = yieldCurve.inputs.assetQuote;
            }

            IEnumerable<CurvePoint> combinedCurve;
            if (DiscountFactors == null && Inputs == null)
            {
                return;
            }
            if (DiscountFactors == null)
            {
                combinedCurve = GetCombinedCurveInputs();    
            }
            else if (Inputs != null && ZeroPoints != null)
            {
                combinedCurve = GetCombinedCurveDiscountsZeroRatesInputs();
            }
            else if (ZeroPoints != null)
            {
                combinedCurve = GetCombinedCurveDiscountsZeroRates();
            }
            else if (Inputs != null)
            {
                combinedCurve = GetCombinedCurveDiscountsInputs();
            }
            else
            {
                combinedCurve = GetCombinedCurveDiscounts();
            }
            AddRange(combinedCurve);
        }

        private IEnumerable<CurvePoint> GetCombinedCurveInputs()
        {
            IEnumerable<CurvePoint> combinedCurve
                = from i in Inputs
                  select new CurvePoint(new DateTime(),
                                        0,
                                        new List<object>
                                            {
                                                i.objectReference.href,
                                                (decimal?) i.quote[0].value
                                            });
            return combinedCurve;
        }

        private IEnumerable<CurvePoint> GetCombinedCurveDiscounts()
        {
            IEnumerable<CurvePoint> combinedCurve
                = from d in DiscountFactors
                  select new CurvePoint((DateTime) d.term.Items[0],
                                        (double) d.mid,
                                        new List<object>
                                            {
                                                d.id,
                                                (DateTime) d.term.Items[0],
                                                d.mid
                                            });
            return combinedCurve;
        }

        private IEnumerable<CurvePoint> GetCombinedCurveDiscountsInputs()
        {
            IEnumerable<CurvePoint> combinedCurve
                = from d in DiscountFactors
                  join i in Inputs on d.id equals i.objectReference.href into zdi
                  from zdi2 in zdi.DefaultIfEmpty()
                  select new CurvePoint((DateTime) d.term.Items[0],
                                        (double) d.mid,
                                        new List<object>
                                            {
                                                d.id,
                                                (DateTime) d.term.Items[0],
                                                d.mid,
                                                zdi2 == null ? null : (decimal?) zdi2.quote[0].value,
                                            });
            return combinedCurve;
        }

        private IEnumerable<CurvePoint> GetCombinedCurveDiscountsZeroRates()
        {
            IEnumerable<CurvePoint> combinedCurve
                = from d in DiscountFactors
                  join z in ZeroPoints on d.id equals z.id
                  select new CurvePoint((DateTime) d.term.Items[0],
                                        (double) z.mid,
                                        new List<object>
                                            {
                                                d.id,
                                                (DateTime) d.term.Items[0],
                                                d.mid,
                                                z.mid
                                            });
            return combinedCurve;
        }

        private IEnumerable<CurvePoint> GetCombinedCurveDiscountsZeroRatesInputs()
        {
            IEnumerable<CurvePoint> combinedCurve
                = from z in ZeroPoints
                  join d in DiscountFactors on z.id equals d.id into zd
                  from zd2 in zd.DefaultIfEmpty()
                  join i in Inputs on z.id equals i.objectReference.href into zdi
                  from zdi2 in zdi.DefaultIfEmpty()
                  select new CurvePoint((DateTime) z.term.Items[0],
                                        (double) z.mid,
                                        new List<object>
                                            {
                                                z.id,
                                                (DateTime) z.term.Items[0],
                                                zd2 == null ? null : (decimal?) zd2.mid,
                                                z.mid,
                                                zdi2 == null ? null : (decimal?) zdi2.quote[0].value,
                                            });
            return combinedCurve;
        }

        public override List<string> Titles
        {
            get
            {
                var titles = new List<string> {"Asset"};

                if (DiscountFactors != null)
                {
                    titles.Add("Date");
                    titles.Add("Discount Factor");
                }
                               
                if (ZeroPoints != null)
                {
                    titles.Add("Zero Rate");
                }

                if (Inputs != null)
                {
                    titles.Add("Input Rate");
                }
                return titles;
            }
        }
    }
}
