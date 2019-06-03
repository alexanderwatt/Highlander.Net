#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using FpML.V5r10.Reporting.Identifiers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using Math = System.Math;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    /// <summary>
    /// A Cross Currency Spread Curve, based on RateCurve
    /// </summary>
    /// <remarks>
    /// See documentation "Cross Currency Basis Curves.pdf"
    /// 1. Synthetic swaps are created for the first year, at 1M, 2M, 3M, 6M, 9M.  These are 
    /// manufactured from the FX Forward curve, adjusted by the quote currency zero curve.
    /// 2. The spreads (swaps) that are passed in to the function are appended to these deposits.
    /// 3. Then a solver is used to find the discount factors after the spreads above are applied 
    /// to the base currency zero curve.
    /// 4. A new curve is constructed from the discount factors.  
    /// </remarks>
    public class XccySpreadCurve : RateCurve
    {
        private const string QuoteCurveName = "QuoteCurve";
        private const string BaseCurveName = "BaseCurve";
        private const string FxCurveName = "FxCurve";

        /// <summary>
        /// The Base Zero Curve
        /// </summary>
        public IRateCurve BaseCurve { get; private set; }

        /// <summary>
        /// The Quote Zero Curve (usually USD)
        /// </summary>
        public IRateCurve QuoteCurve { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="XccySpreadCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableRateAssets">The priceableRateAssets.</param>
        /// <param name="baseCurve">The XCcySpreadCurve that the new curve is based on</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        public XccySpreadCurve(NamedValueSet curveProperties, IRateCurve baseCurve,
            List<IPriceableRateAssetController> priceableRateAssets, 
            PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : base(curveProperties, pricingStructureAlgorithmsHolder)
        {
            BaseCurve = baseCurve;
            PriceableRateAssets = priceableRateAssets;
            CreateYieldCurve();
            Bootstrap((YieldCurveValuation)GetFpMLData().Second);
        }

        /// <summary>
        /// Perturbed curve
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the new curve.</param>
        /// <param name="baseCurve">The XCcySpreadCurve that the new curve is based on</param>
        /// <param name="perturbations">The pertubations to the values of the base curve</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public XccySpreadCurve(ILogger logger, ICoreCache cache, 
            String nameSpace, NamedValueSet properties, 
            XccySpreadCurve baseCurve, decimal[] perturbations, 
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties))
        {
            var assetQuote = ((YieldCurveValuation)baseCurve.PricingStructureValuation).inputs.assetQuote;
            string[] instruments = assetQuote.Where(a => a.definitionRef == null).Select(a => a.objectReference.href).ToArray();
            decimal[] values = assetQuote.Where(a => a.definitionRef == null).Select(a => a.quote[0].value).ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                values[i] += perturbations[i];
            }
            Initialize(logger, cache, nameSpace, properties, baseCurve.BaseCurve, baseCurve.QuoteCurve, instruments, values, null, fixingCalendar, rollCalendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XccySpreadCurve"/> class, 
        /// by applying spreads to an existing RateCurve. Using FX Curve to create synthetic swaps
        /// for the period under 1Y.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the new curve.</param>
        /// <param name="baseCurve">The base zero curve.</param>
        /// <param name="quoteCurve">The quote zero curve.</param>
        /// <param name="fxCurve">The FX curve, used for constructing synthetic deposits</param>
        /// <param name="inputs">The quoted asset instruments.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public XccySpreadCurve(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, QuotedAssetSet inputs, 
            IRateCurve baseCurve, IRateCurve quoteCurve, FxCurve fxCurve,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties))
        {
            BasicAssetValuation[] assetQuote = inputs.assetQuote;
            string[] instruments = ExtractInstruments(assetQuote, null);
            decimal[] values = ExtractValues(assetQuote, null);
            Initialize(logger, cache, nameSpace, properties, baseCurve, quoteCurve, instruments, values, fxCurve, fixingCalendar, rollCalendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XccySpreadCurve"/> class, 
        /// by applying spreads to an existing RateCurve. Using FX Curve to create synthetic swaps
        /// for the period under 1Y.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties of the new curve.</param>
        /// <param name="baseCurve">The base zero curve.</param>
        /// <param name="quoteCurve">The quote zero curve.</param>
        /// <param name="fxCurve">The FX curve, used for constructing synthetic deposits</param>
        /// <param name="instruments">The spread instruments.</param>
        /// <param name="values">The spread values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public XccySpreadCurve(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, IRateCurve baseCurve, IRateCurve quoteCurve, FxCurve fxCurve,
                               string[] instruments, decimal[] values, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties))
        {
            Initialize(logger, cache, nameSpace, properties, baseCurve, quoteCurve, instruments, values, fxCurve, fixingCalendar, rollCalendar);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XccySpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing strucuture.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public XccySpreadCurve(ILogger logger, ICoreCache cache, String nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, 
            NamedValueSet properties, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties))
        {
            SetFpMLData(fpmlData, false);
            var yieldCurveValuation = (YieldCurveValuation)fpmlData.Second;
            BasicAssetValuation[] assetQuote = yieldCurveValuation.inputs.assetQuote;
            string algorithm = ((YieldCurve)fpmlData.First).algorithm;
            DateTime baseDate = yieldCurveValuation.baseDate.Value;
            IRateCurve baseCurve = ExtractCurve(logger, cache, nameSpace, assetQuote, BaseCurveName, algorithm, baseDate, fixingCalendar, rollCalendar);
            IRateCurve quoteCurve = ExtractCurve(logger, cache, nameSpace, assetQuote, QuoteCurveName, algorithm, baseDate, fixingCalendar, rollCalendar);
            string[] fxInstruments = ExtractInstruments(assetQuote, FxCurveName);
            decimal[] fxValues = ExtractValues(assetQuote, FxCurveName);
            FxCurve fxCurve = null;
            if (fxInstruments.Any())
            {
                string curveName = $"{fxInstruments[0].Substring(0, 3)}-{fxInstruments[0].Substring(3, 3)}";
                var curveId = new FxCurveIdentifier(PricingStructureTypeEnum.FxCurve, curveName, baseDate);
                var qas = AssetHelper.ParseToFxRateSet(fxInstruments, fxValues, null);
                var fxCurveProperties = curveId.GetProperties();
                fxCurve = new FxCurve(logger, cache, nameSpace, fxCurveProperties, qas, fixingCalendar, rollCalendar);
            }
            var instruments = ExtractInstruments(assetQuote, null);
            var values = ExtractValues(assetQuote, null);
            Initialize(logger, cache, nameSpace, properties, baseCurve, quoteCurve, instruments, values, fxCurve, fixingCalendar, rollCalendar);
        }

        private static string[] ExtractInstruments(IEnumerable<BasicAssetValuation> assetQuote, string curveName)
        {
            string[] instruments = assetQuote.Where(a => a.definitionRef == curveName).Select(b => b.objectReference.href).ToArray();
            return instruments;
        }

        private static decimal[] ExtractValues(IEnumerable<BasicAssetValuation> assetQuote, string curveName)
        {
            decimal[] values = assetQuote.Where(a => a.definitionRef == curveName).Select(b => b.quote[0].value).ToArray();
            return values;
        }

        private static IRateCurve ExtractCurve(ILogger logger, ICoreCache cache, 
            string nameSpace, IEnumerable<BasicAssetValuation> assetQuote,
            string curveName, string algorithm, DateTime baseDate, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (assetQuote != null)
            {
                var basicAssetValuations = assetQuote as BasicAssetValuation[] ?? assetQuote.ToArray();
                string[] instruments = ExtractInstruments(basicAssetValuations, curveName);
                if (!instruments.Any())
                {
                    return null;
                }
                var curveId = new RateCurveIdentifier(PricingStructureTypeEnum.RateCurve, instruments[0], baseDate, algorithm);
                decimal[] values = ExtractValues(basicAssetValuations, curveName);
                var qas = AssetHelper.Parse(instruments, values, null);
                return new RateCurve(logger, cache, nameSpace, curveId, qas, fixingCalendar, rollCalendar);
            }
            return null;
        }

        private void Initialize(ILogger logger, ICoreCache cache, string nameSpace,  NamedValueSet properties, IRateCurve baseCurve, IRateCurve quoteCurve,
            string[] instruments, decimal[] values, FxCurve fxCurve, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var curveId = (RateCurveIdentifier) PricingStructureIdentifier;
            BaseCurve = baseCurve;
            QuoteCurve = quoteCurve;
            //Holder = new PricingStructureAlgorithmsHolder(logger, cache, curveId.PricingStructureType, curveId.Algorithm);
            Initialize(properties, Holder);
            var currency = properties.GetValue<string>(CurveProp.Currency1);
            if (fxCurve != null && baseCurve == null)
            {
                var holder = new GenericRateCurveAlgorithmHolder(logger, cache, nameSpace);
                // Create discount factors, without any input points
                RateCurve basisAdjustedDiscountCurve = CreateBasisAdjustedDiscountCurve(fxCurve, quoteCurve, currency, curveId.BaseDate, holder);
                TermCurve discountCurve = ((YieldCurveValuation)basisAdjustedDiscountCurve.PricingStructureValuation).discountFactorCurve;
                CreateYieldCurve();
                GetYieldCurveValuation().discountFactorCurve = discountCurve;
                SetInterpolator(discountCurve, curveId.Algorithm, PricingStructureTypeEnum.RateSpreadCurve);
                // Put FX curve into inputs
                var assetQuotes = new List<BasicAssetValuation>();
                BasicAssetValuation[] fxAssetQuotes = ((FxCurveValuation)fxCurve.GetFpMLData().Second).spotRate.assetQuote;
                foreach (BasicAssetValuation assetQuote in fxAssetQuotes)
                {
                    BasicAssetValuation clonedAssetQuote = XmlSerializerHelper.Clone(assetQuote);
                    clonedAssetQuote.definitionRef = FxCurveName;
                    assetQuotes.Add(clonedAssetQuote);
                }
                AddQuoteCurveInputs(assetQuotes);
                ((YieldCurveValuation)PricingStructureValuation).inputs = new QuotedAssetSet { assetQuote = assetQuotes.ToArray() };
            }
            else
            {
                List<IPriceableRateAssetController> swaps
                    = PriceableAssetFactory.CreatePriceableRateAssets(logger, cache, nameSpace, curveId.BaseDate, instruments, values, null, fixingCalendar, rollCalendar);
                if (fxCurve == null)
                {
                    // only use the input swaps
                    PriceableRateAssets = swaps;
                }
                else
                {
                    var holder = new GenericRateCurveAlgorithmHolder(logger, cache, nameSpace);
                    // Add synthetic input points
                    RateCurve basisAdjustedDiscountCurve = CreateBasisAdjustedDiscountCurve(fxCurve, quoteCurve, currency, curveId.BaseDate, holder);
                    //TODO Add some extra short end point: 1D and 1W
                    string[] syntheticSwapPoints = { "1M", "2M", "3M", "6M", "9M" };
                    PriceableRateAssets = CreateSyntheticSwaps(logger, cache, nameSpace, BaseCurve, basisAdjustedDiscountCurve,
                                                                currency, curveId.BaseDate, syntheticSwapPoints, fixingCalendar, rollCalendar);
                    PriceableRateAssets.AddRange(swaps);
                }
                // bootstrap to create the discount factors
                PriceableRateAssets = PriceableRateAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
                Bootstrap((YieldCurveValuation)GetFpMLData().Second);
            }
        }

        /// <summary>
        /// Bootstrap the curve, and save the discounts and zero rates
        /// </summary>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            Bootstrap(null);
        }

        private void Bootstrap(YieldCurveValuation yieldCurveValuation)
        {
            if (yieldCurveValuation == null || yieldCurveValuation.inputs != null && yieldCurveValuation.discountFactorCurve?.point == null)
            {
                CreateYieldCurve();
            }
            var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
            // Add extra input points for BaseCurve - if they exist
            List<BasicAssetValuation> assetQuotes = ((YieldCurveValuation)PricingStructureValuation).inputs.assetQuote.ToList();
            AddBaseCurveInputs(assetQuotes);
            // Add extra input points for QuoteCurve - if they exist
            AddQuoteCurveInputs(assetQuotes);
            ((YieldCurveValuation)PricingStructureValuation).inputs.assetQuote = assetQuotes.ToArray();
            TermCurve discountCurve = ((YieldCurveValuation)PricingStructureValuation).discountFactorCurve;
            SetInterpolator(discountCurve, curveId.Algorithm, PricingStructureTypeEnum.RateSpreadCurve);
        }

        private void AddQuoteCurveInputs(List<BasicAssetValuation> assetQuotes)
        {
            QuotedAssetSet quoteCurveInputs = ((YieldCurveValuation) QuoteCurve?.GetFpMLData().Second)?.inputs;
            if (quoteCurveInputs?.assetQuote != null)
            {
                foreach (BasicAssetValuation assetQuote in quoteCurveInputs.assetQuote)
                {
                    BasicAssetValuation clonedAssetQuote = XmlSerializerHelper.Clone(assetQuote);
                    clonedAssetQuote.definitionRef = QuoteCurveName;
                    assetQuotes.Add(clonedAssetQuote);
                }
            }
        }

        private void AddBaseCurveInputs(List<BasicAssetValuation> assetQuotes)
        {
            QuotedAssetSet baseCurveInputs = ((YieldCurveValuation) BaseCurve?.GetFpMLData().Second)?.inputs;
            if (baseCurveInputs?.assetQuote != null)
            {
                foreach (BasicAssetValuation assetQuote in baseCurveInputs.assetQuote)
                {
                    BasicAssetValuation clonedAssetQuote = XmlSerializerHelper.Clone(assetQuote);
                    clonedAssetQuote.definitionRef = BaseCurveName;
                    assetQuotes.Add(clonedAssetQuote);
                }
            }
        }

        private void CreateYieldCurve()
        {
            var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
            InterpolationMethod interpolationMethod =
                InterpolationMethodHelper.Parse(Holder.GetValue("BootstrapperInterpolation"));
            bool extrapolationPermitted = bool.Parse(Holder.GetValue("ExtrapolationPermitted"));
            TermPoint[] points = BaseCurve != null ? new RateBootstrapperNewtonRaphson().Bootstrap(PriceableRateAssets, BaseCurve, Holder) : null;
            //TermPoint[] points = BaseCurve != null ? RateXccySpreadBootstrapper.Bootstrap(PriceableRateAssets, BaseCurve, Holder) : null;
            var termCurve = new TermCurve
                                {
                                    extrapolationPermitted = extrapolationPermitted,
                                    extrapolationPermittedSpecified = true,
                                    interpolationMethod = interpolationMethod,
                                    point = points
                                };
            CreatePricingStructure(curveId, termCurve, PriceableRateAssets, null);
        }

        /// <summary>
        /// Creates synthetic swaps from FX curve for period under 1 year
        /// </summary>
        /// <param name="cache">The cache.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="baseCurve"></param>
        /// <param name="basisAdjustedDiscountCurve"></param>
        /// <param name="currency"></param>
        /// <param name="baseDate"></param>
        /// <param name="swapsRequired">Array of the names of the swaps required</param>
        /// <returns></returns>
        public static List<IPriceableRateAssetController> CreateSyntheticSwaps(ILogger logger, ICoreCache cache, string nameSpace, IRateCurve baseCurve,
            RateCurve basisAdjustedDiscountCurve, string currency, DateTime baseDate, string[] swapsRequired, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var dummyRates = new decimal[5];
            string[] swapIds = swapsRequired.Select(a => currency + "-XccySwap-" + a).ToArray();
            List<IPriceableRateAssetController> priceableRateAssets
                = PriceableAssetFactory.CreatePriceableRateAssets(logger, cache, nameSpace, baseDate, swapIds, dummyRates, null, fixingCalendar, rollCalendar);
            foreach (var priceableRateAssetController in priceableRateAssets)
            {
                var swap = (PriceableSimpleIRSwap) priceableRateAssetController;
                DateTime date0 = swap.AdjustedStartDate;
                DateTime date1 = swap.GetRiskMaturityDate();
                IDayCounter dayCounter = DayCounterHelper.Parse(swap.DayCountFraction.Value);
                double adjustedDiscountFactorStart = basisAdjustedDiscountCurve.GetDiscountFactor(date0);
                double adjustedDiscountFactorEnd = basisAdjustedDiscountCurve.GetDiscountFactor(date1);
                double term3 = 0;
                double term4 = 0;
                for (int i = 0; i < swap.AdjustedPeriodDates.Count - 1; i++)
                {
                    DateTime startDate = swap.AdjustedPeriodDates[i];
                    DateTime endDate = swap.AdjustedPeriodDates[i + 1];
                    if (startDate == endDate)
                    {
                        throw new InvalidOperationException("StartDate and EndDate cannot be the same");
                    }
                    double adjustedDiscountFactor = basisAdjustedDiscountCurve.GetDiscountFactor(endDate);
                    double baseDiscountFactorStart = baseCurve.GetDiscountFactor(startDate);
                    double baseDiscountFactorEnd = baseCurve.GetDiscountFactor(endDate);
                    double yearFraction = dayCounter.YearFraction(startDate, endDate);
                    double baseForwardRate = (1 / yearFraction) * (baseDiscountFactorStart / baseDiscountFactorEnd - 1);
                    term3 += yearFraction * adjustedDiscountFactor * baseForwardRate;
                    term4 += yearFraction * adjustedDiscountFactor;
                }
                swap.MarketQuote.value = (decimal)((adjustedDiscountFactorStart - adjustedDiscountFactorEnd - term3) / term4);
                swap.BasicAssetValuation.quote[0].value = swap.MarketQuote.value;
            }
            return priceableRateAssets;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fxCurve"></param>
        /// <param name="usdCurve"></param>
        /// <param name="currency"></param>
        /// <param name="baseDate"></param>
        /// <param name="algorithmHolder"></param>
        /// <returns></returns>
        public static RateCurve CreateBasisAdjustedDiscountCurve(FxCurve fxCurve, IRateCurve usdCurve, string currency, 
            DateTime baseDate, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            string quoteCurrency = ((FxCurveIdentifier) fxCurve.GetPricingStructureId()).QuoteCurrency.Value;
            double power = quoteCurrency == currency ? -1 : 1;
            double fxRate0 = Math.Pow(fxCurve.GetForward(baseDate), power);
            var newPoints = new Dictionary<DateTime, Pair<string, decimal>>();
            foreach (TermPoint point in fxCurve.GetTermCurve().point)
            {
                var pillarPoint = (DateTime)point.term.Items[0];
                double fxRateN = Math.Pow(fxCurve.GetForward(pillarPoint), power);
                double discountFactorN = usdCurve.GetDiscountFactor(pillarPoint);
                var quoteBasisAdjustedDiscountFactor = (decimal)(discountFactorN * fxRateN / fxRate0);
                newPoints.Add(pillarPoint, new Pair<string, decimal>(point.id, quoteBasisAdjustedDiscountFactor));
            }
            var newProperties
                = new Dictionary<string, object>
                      {
                          {CurveProp.PricingStructureType, "RateCurve"},
                          {CurveProp.Market, "DiscountCurveConstruction"},
                          {CurveProp.IndexTenor, "0M"},
                          {CurveProp.Currency1, currency},
                          {CurveProp.IndexName, "XXX-XXX"},
                          {CurveProp.Algorithm, "FastLinearZero"},
                          {CurveProp.BaseDate, baseDate},
                      };
            var namedValueSet = new NamedValueSet(newProperties);
            return new RateCurve(namedValueSet, algorithmHolder, newPoints);//There is no compounding frequency propertty
        }
    }
}
