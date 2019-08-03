/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Linq;
using Core.Common;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

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
    public class RateXccySpreadCurve : RateSpreadCurve
    {
        /// <summary>
        /// 
        /// </summary>
        public IFxCurve ReferenceFxCurve { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Period CutOverTerm { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public RateCurve Currency2Curve { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Boolean IsCurrency1RateCurve { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateXccySpreadCurve"/> class, 
        /// by applying spreads to an existing RateCurve. Using FX Curve to create synthetic swaps
        /// for the period under 1Y.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="properties">The properties of the new curve. One of the mandatory properties for this curve type: 
        /// CutOverTerm, is the tenor at which point all Fx Curve points are removed form the RateXccyCurve bootstrap.
        /// Normally this is 1Y.</param>
        /// <param name="currency1Curve">The base zero curve.</param>
        /// <param name="fxCurve">The FX curve, used for constructing synthetic deposits. The fx points map from the base curve to the non-base curve.</param>
        /// <param name="currency2Curve">The non-Base Curve.</param>
        /// <param name="spreadInstruments">The spread instruments and their values.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateXccySpreadCurve(ILogger logger, ICoreCache cache, String nameSpace, NamedValueSet properties, RateCurve currency1Curve, IFxCurve fxCurve,
                               RateCurve currency2Curve, QuotedAssetSet spreadInstruments, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new RateCurveIdentifier(properties))
        { 
            //Set the base curve.
            BaseCurve = currency1Curve;
            ReferenceCurveId = BaseCurve.GetPricingStructureId();
            //PricingStructureIdentifier = new RateCurveIdentifier(properties);
            Currency2Curve = currency2Curve;
            //Get the cut-over date.
            var cutOverTerm = properties.GetValue<string>("CutOverTerm");
            if (cutOverTerm != null)
            {
                CutOverTerm = PeriodHelper.Parse(cutOverTerm);
            }
            //set the fx curve.
            ReferenceFxCurve = fxCurve;
            IsCurrency1RateCurve = properties.GetValue<bool>("Currency1RateCurve");
            //Check the pricing structure type.
            var pricingStructureId = (RateCurveIdentifier)PricingStructureIdentifier;
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.RateXccyCurve ||
                ReferenceFxCurve == null) return;
            //There must be a valid quoted asset set in order to bootstrap.
            if (!XsdClassesFieldResolver.QuotedAssetSetIsValid(spreadInstruments)) return;
            PriceableRateSpreadAssets =
                PriceableAssetFactory.CreatePriceableRateSpreadAssets(logger, cache, nameSpace, pricingStructureId.BaseDate, spreadInstruments, fixingCalendar, rollCalendar);
            Build(logger, cache, nameSpace, fixingCalendar, rollCalendar);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="RateXccySpreadCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="baseCurveData">The market data. This must contain both the underlying base curve and the spread curve.
        /// Otherwise the RateBasisInterpolator can not instantiate.</param>
        /// <param name="referenceFxCurveData">The fxcurve referenced.</param>
        /// <param name="currency2CurveData">The curve data for the non base curve. This is normally the domestic curve i.e. AUD,
        /// as FX is quotes as xccy basis swaps adjust on the non-USD leg.</param>
        /// <param name="spreadCurveData">The spread Curve Data</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public RateXccySpreadCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> baseCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> referenceFxCurveData,
            Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> currency2CurveData,
        Triplet<PricingStructure, PricingStructureValuation, NamedValueSet> spreadCurveData, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, spreadCurveData, fixingCalendar, rollCalendar)
        {
            //Set the identifier.
            var nvs = spreadCurveData.Third;
            var pricingStructureId = new RateCurveIdentifier(nvs);
            PricingStructureIdentifier = pricingStructureId;
            var refCurveId = nvs.GetValue<string>(CurveProp.ReferenceCurveUniqueId);
            ReferenceCurveId = refCurveId != null ? new Identifier(refCurveId) : ReferenceCurveId = null;
            if (pricingStructureId.PricingStructureType != PricingStructureTypeEnum.RateXccyCurve) return;
            //Set the curve term.
            var cutOverTerm = spreadCurveData.Third.GetValue<string>("CutOverTerm");
            if (cutOverTerm != null)
            {
                CutOverTerm = PeriodHelper.Parse(cutOverTerm);
            }
            //Set the reference curve
            var baseCurveFpML = new Pair<PricingStructure, PricingStructureValuation>(baseCurveData.First, baseCurveData.Second);
            var baseCurveProps = baseCurveData.Third;
            BaseCurve = (IRateCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, baseCurveFpML, baseCurveProps);
            var fxCurveFpML = new Pair<PricingStructure, PricingStructureValuation>(referenceFxCurveData.First, referenceFxCurveData.Second);
            var fxCurveProps = referenceFxCurveData.Third;
            ReferenceFxCurve = (IFxCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, fxCurveFpML, fxCurveProps);
            //Set the reference curve
            var currency2CurveFpML = new Pair<PricingStructure, PricingStructureValuation>(currency2CurveData.First, currency2CurveData.Second);
            var currency2CurveProps = currency2CurveData.Third;
            Currency2Curve = (RateCurve)PricingStructureFactory.Create(logger, cache, nameSpace, fixingCalendar, rollCalendar, currency2CurveFpML, currency2CurveProps);
            //Get the spread Data
            var spreadCurveFpML = new Pair<PricingStructure, PricingStructureValuation>(spreadCurveData.First, spreadCurveData.Second);
            var spreadCurveProps = spreadCurveData.Third;
            IsCurrency1RateCurve = spreadCurveProps.GetValue<bool>("Currency1RateCurve");
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);TODO add this later.
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(nvs);
            var tempFpml = (YieldCurveValuation)spreadCurveFpML.Second;
            var spreadAssets = tempFpml.inputs;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            if (cache == null)
            {
                bootstrap = false;
            }
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quoted asset set in order to bootstrap.
                if (!XsdClassesFieldResolver.QuotedAssetSetIsValid(spreadAssets)) return;
                PriceableRateSpreadAssets = PriceableAssetFactory.CreatePriceableRateSpreadAssets(logger, cache, nameSpace, pricingStructureId.BaseDate, spreadAssets, fixingCalendar, rollCalendar);
                Build(logger, cache, nameSpace, fixingCalendar, rollCalendar);
            }
            else
            {
                if (cache != null)
                {
                    // the discount curve is already built, so don't rebuild
                    PriceableRateSpreadAssets = PriceableAssetFactory.CreatePriceableRateSpreadAssets(logger, cache, nameSpace, pricingStructureId.BaseDate, spreadAssets, fixingCalendar, rollCalendar);
                    CreatePricingStructure(pricingStructureId, tempFpml.discountFactorCurve, PriceableAssetFactory.Parse(PriceableRateSpreadAssets));
                    SetInterpolator(BaseCurve, pricingStructureId.PricingStructureType);
                }
                else
                {
                    CreatePricingStructure(pricingStructureId, tempFpml.discountFactorCurve, spreadAssets);
                    SetInterpolator(BaseCurve, pricingStructureId.PricingStructureType);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        public sealed override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)//TODO need to define the cutover point.
        {
            var pricingStructureId = (RateCurveIdentifier)PricingStructureIdentifier;
            //Order the assets.
            PriceableRateSpreadAssets = PriceableRateSpreadAssets.OrderBy(a => a.GetRiskMaturityDate()).ToList();
            //Generate the rate curve from the fx curve.
            var fxProps = ReferenceFxCurve.GetPricingStructureId().Properties;
            var quoteBasisEnum = PropertyHelper.ExtractQuoteBasis(fxProps);
            bool isBaseCurve = IsCurrency1RateCurve && quoteBasisEnum == QuoteBasisEnum.Currency2PerCurrency1;
            //var fxCurve = ReferenceFxCurve as FxCurve;
            //priceableAsset is PriceableCapRateAsset asset
            if (ReferenceFxCurve is FxCurve fxCurve)//fxCurve != null
            {
                var rateCurve = fxCurve.GenerateRateCurve(logger, cache, nameSpace, BaseCurve, isBaseCurve, pricingStructureId.GetProperties(), fixingCalendar, rollCalendar);
                //remove all points after the CutOverTerm.
                var rateCurvePoints = RemovePoints(rateCurve);
                //Retrieve the term curve.
                var termCurve = new TermCurve
                                    {
                                        extrapolationPermitted = rateCurve.GetTermCurve().extrapolationPermitted,
                                        extrapolationPermittedSpecified =
                                            rateCurve.GetTermCurve().extrapolationPermittedSpecified,
                                        interpolationMethod = rateCurve.GetTermCurve().interpolationMethod,
                                        point = rateCurvePoints
                                    };
                //Get the reference interpolated curve.
                termCurve.point = RateXccySpreadBootstrapper.Bootstrap(PriceableRateSpreadAssets,
                                                                       Currency2Curve,
                                                                       pricingStructureId.BaseDate,
                                                                       termCurve);
                CreatePricingStructure(pricingStructureId, termCurve, PriceableAssetFactory.Parse(PriceableRateSpreadAssets));
            }
            SetInterpolator(Currency2Curve, pricingStructureId.PricingStructureType);
        }

        /// <summary>
        /// Creates synthetic swaps from FX curve for period under 1 year
        /// </summary>
        /// <param name="rateCurve"></param>
        /// <returns></returns>
        public TermPoint[] RemovePoints(IRateCurve rateCurve)
        {
            var fxTermCurve = rateCurve.GetTermCurve();
            var curveId = GetRateCurveId();
            var termDate = CutOverTerm.Add(curveId.BaseDate);
            return (from point in fxTermCurve.point
                    let pillarPoint = (DateTime)point.term.Items[0]
                    where pillarPoint <= termDate
                    select point).ToArray();
        }

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>
        /// The <c>double</c> function value at the point
        /// </returns>
        public override double Value(IPoint point)
        {
            return Interpolator.Value(point);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseCurve"></param>
        /// <param name="psType"></param>
        internal void SetInterpolator(IRateCurve baseCurve, PricingStructureTypeEnum psType)
        {
            var interpDayCounter = Actual365.Instance;
            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            CompoundingFrequency = EnumHelper.Parse<CompoundingFrequencyEnum>(Holder.GetValue("CompoundingFrequency"));
            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            var discountFactorCurve = GetYieldCurveValuation().discountFactorCurve;
            Interpolator = new RateSpreadInterpolator(baseCurve, discountFactorCurve, GetBaseDate(), interpDayCounter);
        }
    }
}
