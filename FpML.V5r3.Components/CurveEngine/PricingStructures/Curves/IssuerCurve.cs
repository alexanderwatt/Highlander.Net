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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.Analytics.Interpolations.Points;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.CurveEngine.Assets.Helpers;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.CurveEngine.PricingStructures.Interpolators;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using AssetClass = Orion.Constants.AssetClass;
using XsdClassesFieldResolver = FpML.V5r3.Reporting.XsdClassesFieldResolver;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    /// <summary>
    /// The advanced ratecurve class.
    /// </summary>
    public class IssuerCurve : CurveBase, IBondCurve
    {
        #region Properties

        /// <summary>
        /// The fixing calendar.
        /// </summary>
        public IBusinessCalendar SettlementCalendar { get; set; }

        /// <summary>
        /// The payment calendar.
        /// </summary>
        public IBusinessCalendar PaymentCalendar { get; set; }

        /// <summary>
        /// The underlying curve to use for interpolation.
        /// </summary>
        public string UnderlyingInterpolatedCurve = "ZeroCurve";

        /// <summary>
        /// The compounding frequency to use for interpolation.
        /// </summary>
        public CompoundingFrequencyEnum CompoundingFrequency { get; set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableBondAssetController> PriceableBondAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName = "FastBootstrapper";

        /// <summary>
        /// 
        /// </summary>
        public double Tolerance = 0.00000000001;

        /// <summary>
        /// 
        /// </summary>
        public bool ExtrapolationPermitted = true;

        /// <summary>
        /// 
        /// </summary>
        public InterpolationMethod InterpolationMethod = InterpolationMethodHelper.Parse("LinearRateInterpolation");

        /// <summary>
        /// The date array.
        /// </summary>
        public DateTime[] Dates
        {
            get
            {
                var pointCurve = GetTermCurve().point;
                return pointCurve.Select(point => (DateTime)point.term.Items[0]).ToArray();
            }
        }

        ///// <summary>
        ///// The zero rate array. Quarterly compounding is assumed.
        ///// </summary>
        //public double[] ZeroRates
        //{
        //    get
        //    {
        //        var rateList = new List<double>();
        //        TermPoint[] pointCurve = GetTermCurve().point;
        //        int index = 0;
        //        foreach (var point in pointCurve)
        //        {
        //            int time = ((DateTime)point.term.Items[0] - GetBaseDate()).Days;//This is risky...assumes an ordered term curve.                  
        //            if (time == 0)
        //            {
        //                var tempTime = ((DateTime)pointCurve[index + 1].term.Items[0] - GetBaseDate()).Days;
        //                var tempDiscountFactor = (double)pointCurve[index + 1].mid;
        //                rateList.Add(RateAnalytics.DiscountFactorToZeroRate(tempDiscountFactor, tempTime / 365.0, "Quarterly"));
        //            }
        //            else
        //            {
        //                rateList.Add(RateAnalytics.DiscountFactorToZeroRate((double)pointCurve[index].mid, time / 365.0, "Quarterly"));
        //            }
        //            index++;
        //        }
        //        return rateList.ToArray();
        //    }
        //}

        #endregion

        #region Methods

        ///// <summary>
        ///// Gets the days and zero rates.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="compounding">The compounding.</param>
        ///// <returns></returns>
        //public IDictionary<int, Double> GetDaysAndZeroRates(DateTime baseDate, string compounding)
        //{
        //    if (string.IsNullOrEmpty(compounding))
        //    {
        //        throw new ArgumentNullException("compounding", @"Compounding must be set for GetDaysAndZeroRates");
        //    }
        //    CompoundingFrequencyEnum compoundingFrequencyEnum = EnumParse.ToCompoundingFrequencyEnum(compounding);
        //    return GetDaysAndZeroRates(baseDate, compoundingFrequencyEnum);
        //}

        ///// <summary>
        ///// Gets the days and zero rates.
        ///// </summary>
        ///// <param name="startDate">The date the results start from.</param>
        ///// <param name="compounding">The compounding.</param>
        ///// <returns></returns>
        //public IDictionary<int, Double> GetDaysAndZeroRates(DateTime startDate, CompoundingFrequencyEnum compounding)
        //{
        //    IDictionary<int, Double> points = new Dictionary<int, Double>();
        //    TermPoint[] curve = GetYieldCurveValuation().discountFactorCurve.point;
        //    //if (UnderlyingInterpolatedCurve == "ZeroCurve" && GetYieldCurveValuation().zeroCurve != null)
        //    //{
        //    //    curve = GetYieldCurveValuation().zeroCurve.rateCurve.point;
        //    //}
        //    foreach (TermPoint point in curve)
        //    {
        //        double zeroRate;
        //        var date = (DateTime)point.term.Items[0];
        //        int days = (date - startDate).Days;
        //        if (days == 0)
        //        {
        //            date = (DateTime)curve[1].term.Items[0];
        //            var df = (double)curve[1].mid;
        //            zeroRate = RateAnalytics.DiscountFactorToZeroRate(df, (date - startDate).Days / 365d, compounding);
        //        }
        //        else
        //        {
        //            zeroRate = RateAnalytics.DiscountFactorToZeroRate((double)point.mid, days / 365d, compounding);
        //        }                
        //        points.Add(days, zeroRate);
        //    }
        //    return points;
        //}
        
        
        ///// <summary>
        ///// The curve point array. The calculation of zeroes rates is done on the fly.
        ///// </summary>
        //// What is this for???
        //public object[,] GetDateAndZeroRates(string compounding)
        //{

        //    var pointCurve = GetTermCurve().point;
        //    var points = new object[pointCurve.Length, 2];
        //    var index = 0;
        //    foreach (var point in pointCurve)
        //    {
        //        var time = ((DateTime)point.term.Items[0] - GetBaseDate()).Days;//This is risky...assumes an ordered termcurve.
        //        if (time == 0)
        //        {
        //            var tempTime = ((DateTime)pointCurve[index + 1].term.Items[0] - GetBaseDate()).Days;
        //            var discountFactor = (double)pointCurve[index + 1].mid;
        //            points[index, 0] = time;
        //            points[index, 1] = RateAnalytics.DiscountFactorToZeroRate(discountFactor, tempTime / 365d, compounding);
        //        }
        //        else
        //        {
        //            points[index, 0] = time;
        //            points[index, 1] = RateAnalytics.DiscountFactorToZeroRate((double)pointCurve[index].mid, time / 365d, compounding);
        //        }
        //        index++;
        //    }
        //    return points;
        //}

        /// <summary>
        /// Gets and sets members of the quoted asset set. Mainly for use the market data server.
        /// </summary>
        protected QuotedAssetSet QuotedAssetSet
        {
            get { return GetQuotedAssetSet(); }
            set { ((YieldCurveValuation)PricingStructureValuation).inputs = value; }
        }

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        public IssuerCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            PricingStructureIdentifier = new RateCurveIdentifier(properties);
            Initialize(properties, algorithmHolder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="algorithmHolder"></param>
        protected void Initialize(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            if (algorithmHolder != null)
            {
                Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ??
                            GetDefaultTolerance(algorithmHolder);
                string compoundingFrequency = properties.GetString("CompoundingFrequency", null);
                CompoundingFrequency = compoundingFrequency != null
                                           ? EnumHelper.Parse<CompoundingFrequencyEnum>(compoundingFrequency)
                                           : GetDefaultCompounding(algorithmHolder);
                Holder = algorithmHolder;
            }
            else
            {
                Tolerance = 10 ^ -9;
                CompoundingFrequency = CompoundingFrequencyEnum.Quarterly;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurveBase"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected IssuerCurve(ILogger logger, ICoreCache cache, String nameSpace, PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {
            var properties = curveIdentifier.GetProperties();
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ?? GetDefaultTolerance(Holder);
            string compoundingFrequency = properties.GetString("CompoundingFrequency", null);
            CompoundingFrequency = compoundingFrequency != null
                                       ? EnumHelper.Parse<CompoundingFrequencyEnum>(compoundingFrequency)
                                       : GetDefaultCompounding(Holder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        /// <param name="instrumentData">The instrument data.</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        [Description("public interface")]
        public IssuerCurve(ILogger logger, ICoreCache cache, string nameSpace,
            BondCurveIdentifier curveIdentifier, QuotedAssetSet instrumentData, 
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, curveIdentifier)
        {
            //PricingStructureIdentifier = curveIdentifier;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            var termCurve = SetConfigurationData();
            PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, curveIdentifier.BaseDate, instrumentData, settlementCalendar, rollCalendar);
            //PriceableBondAssets = PriceableAssetFactory.CreatePriceableRateAssetsWithBasisSwaps(logger, cache, nameSpace, curveIdentifier.ForecastRateIndex.indexTenor, instrumentData, curveIdentifier.BaseDate, fixingCalendar, rollCalendar);
            termCurve.point = BondBootstrapper.Bootstrap(PriceableBondAssets, curveIdentifier.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveIdentifier, termCurve, PriceableBondAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveIdentifier.Algorithm, curveIdentifier.PricingStructureType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instrumentData">The instrument data.</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        [Description("public interface")]
        public IssuerCurve(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, QuotedAssetSet instrumentData, 
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, new BondCurveIdentifier(properties))
        {
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            var curveId = GetRateCurveId();
            var termCurve = SetConfigurationData();
            PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, curveId.BaseDate, instrumentData, settlementCalendar, rollCalendar);
            termCurve.point = BondBootstrapper.Bootstrap(PriceableBondAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, PriceableBondAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableRateAssets">The priceableRateAssets.</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        public IssuerCurve(NamedValueSet curveProperties, List<IPriceableBondAssetController> priceableRateAssets, 
            PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : this(curveProperties, pricingStructureAlgorithmsHolder)
        {
            var curveId = GetRateCurveId();
            var termCurve = SetConfigurationData();
            PriceableBondAssets = priceableRateAssets;
            termCurve.point = BondBootstrapper.Bootstrap(PriceableBondAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, PriceableBondAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="RateCurve"/> class.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        ///// <param name="instrumentRates">The rates of each instrument provided.</param>
        ///// <param name="dates">The dates of the discount factors.</param>
        ///// <param name="discountFactors">The values.</param>
        ///// <param name="instrumentNames">THe names of each market instrument: AUD-Deposit-1M, AUD-IRSwap-5Y etc.</param>
        //public BondCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder, 
        //    IList<string> instrumentNames, IList<decimal> instrumentRates,
        //    IList<DateTime> dates, IList<decimal> discountFactors)
        //    : this(properties, pricingStructureAlgorithmsHolder)
        //{            
        //    var additional = new decimal[instrumentNames.Count];
        //    var qas = new QuotedAssetSet();
        //    if (instrumentNames.Count == instrumentRates.Count)
        //    {
        //        qas = AssetHelper.Parse(instrumentNames.ToArray(), instrumentRates.ToArray(),
        //                                                  additional);
        //    }
        //    // Create FPmL
        //    var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
        //    var termCurve = SetConfigurationData();
        //    var point = TermPointsFactory.Create(dates, discountFactors);
        //    termCurve.point = point;
        //    YieldCurve yieldCurve = CreateYieldCurve(curveId);
        //    YieldCurveValuation yieldCurveValuation = CreateYieldCurveValuation(curveId, qas, yieldCurve.id, termCurve);
        //    var fpmlData
        //        = new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, yieldCurveValuation);
        //    SetFpMLData(fpmlData, false);
        //    // Interpolate the DiscountFactor curve based on the respective curve interpolation 
        //    SetInterpolator(yieldCurveValuation.discountFactorCurve, curveId.Algorithm, curveId.PricingStructureType);
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="RateCurve"/> class.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        ///// <param name="discountFactors">The discount factors.</param>
        //public BondCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder, 
        //    Dictionary<DateTime, Pair<string, decimal>> discountFactors)
        //    : this(properties, pricingStructureAlgorithmsHolder)
        //{
        //    var point = TermPointsFactory.Create(discountFactors);
        //    Initialize(point);
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="RateCurve"/> class.
        ///// </summary>
        ///// <param name="properties">The properties.</param>
        ///// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        ///// <param name="dates">The dates of the discount factors.</param>
        ///// <param name="discountFactors">The values.</param>
        //public BondCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder, 
        //    IList<DateTime> dates, IList<decimal> discountFactors)
        //    : this(properties, pricingStructureAlgorithmsHolder)
        //{
        //    var point = TermPointsFactory.Create(dates, discountFactors);
        //    Initialize(point);
        //}

        //private void Initialize(TermPoint[] point)
        //{
        //    var curveId = (RateCurveIdentifier)PricingStructureIdentifier;
        //    var termCurve = SetConfigurationData();
        //    termCurve.point = point;
        //    YieldCurve yieldCurve = CreateYieldCurve(curveId);
        //    YieldCurveValuation yieldCurveValuation = CreateYieldCurveValuation(curveId, new QuotedAssetSet(), yieldCurve.id, termCurve);
        //    var fpmlData = new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, yieldCurveValuation);
        //    SetFpMLData(fpmlData, false);
        //    // Interpolate the DiscountFactor curve based on the respective curve interpolation 
        //    SetInterpolator(yieldCurveValuation.discountFactorCurve, curveId.Algorithm, curveId.PricingStructureType);
        //}

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing structure.</param>
        /// <param name="settlementCalendar">The settlementCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        public IssuerCurve(String nameSpace, PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : base(null, null, nameSpace, fpmlData, new BondCurveIdentifier(properties ?? PricingStructurePropertyHelper.RateCurve(fpmlData)))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            var curveId = GetRateCurveId();
            Initialize(properties, pricingStructureAlgorithmsHolder);
            if (fpmlData == null) return;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            // the discount curve is already built, so don't rebuild
            SetFpMLData(fpmlData, false);
            SetInterpolator(((YieldCurveValuation)PricingStructureValuation).discountFactorCurve, curveId.Algorithm, curveId.PricingStructureType);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing structure.</param>
        /// <param name="settlementCalendar">The settlementCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="buildAssets">This is a flag which allows no assets to be built. Mainly for derived rate curve from fx curve. </param>
        public IssuerCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar, bool buildAssets)
            : base(logger, cache, nameSpace, fpmlData, new BondCurveIdentifier(properties ?? PricingStructurePropertyHelper.RateCurve(fpmlData)))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties);
            var curveId = GetRateCurveId();
            Initialize(properties, Holder);
            if (fpmlData == null) return;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);//TODO removed optimisation as it means that partial hedges can not be undertaken.
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);
            var tempFpml = (YieldCurveValuation)fpmlData.Second;
            var termCurve = SetConfigurationData();
            var qas = tempFpml.inputs;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            //This is an override if the cache is null, as the bootstrapper will not work.
            if (cache == null)
            {
                //optimize = true;
                bootstrap = false;
            }
            //This ensures only building if the asset flag is true.
            bootstrap = bootstrap && buildAssets;
            bool validAssets = XsdClassesFieldResolver.QuotedAssetSetIsValid(qas);
            //Test to see if a bootstrap is required.
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quoted asset set in order to bootstrap.
                if (!validAssets) return;
                PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                termCurve.point = BondBootstrapper.Bootstrap(PriceableBondAssets, curveId.BaseDate,
                                                             termCurve.extrapolationPermitted,
                                                             termCurve.interpolationMethod,
                                                             Tolerance);
                CreatePricingStructure(curveId, termCurve, qas);
                SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
            }
            else
            {
                // the discount curve is already built, so don't rebuild
                SetFpMLData(fpmlData, false);
                SetInterpolator(((YieldCurveValuation)PricingStructureValuation).discountFactorCurve, curveId.Algorithm, curveId.PricingStructureType);
                //Set the priceable assets.
                if (validAssets && buildAssets)//!optimize && 
                {
                    PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing structure.</param>
        /// <param name="settlementCalendar">The fixingCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        public IssuerCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, new BondCurveIdentifier(properties ?? PricingStructurePropertyHelper.RateCurve(fpmlData)))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            var curveId = GetRateCurveId();
            Initialize(properties, Holder);
            if (fpmlData == null) return;
            SettlementCalendar = settlementCalendar;
            PaymentCalendar = rollCalendar;
            //Override properties.
            //var optimize = PropertyHelper.ExtractOptimizeBuildFlag(properties);//TODO removed optimisation as it means that partial hedges can not be undertaken.
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);           
            var tempFpml = (YieldCurveValuation)fpmlData.Second;          
            var termCurve = SetConfigurationData();
            var qas = tempFpml.inputs;
            //This is to catch it when there are no discount factor points.
            var discountsAbsent = tempFpml.discountFactorCurve?.point == null || tempFpml.discountFactorCurve.point.Length == 0;
            //This is an override if the cache is null, as the bootstrapper will not work.
            if (cache == null)
            {
                //optimize = true;
                bootstrap = false;
            }
            bool validAssets = XsdClassesFieldResolver.QuotedAssetSetIsValid(qas);
            //Test to see if a bootstrap is required.
            if (bootstrap || discountsAbsent)
            {
                //There must be a valid quoted asset set in order to bootstrap.
                if (!validAssets) return;
                PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                termCurve.point = BondBootstrapper.Bootstrap(PriceableBondAssets, curveId.BaseDate,
                                                             termCurve.extrapolationPermitted,
                                                             termCurve.interpolationMethod,
                                                             Tolerance);
                CreatePricingStructure(curveId, termCurve, qas);
                SetInterpolator(termCurve, curveId.Algorithm, curveId.PricingStructureType);
            }
            else
            {
                // the discount curve is already built, so don't rebuild
                SetFpMLData(fpmlData, false);
                SetInterpolator(((YieldCurveValuation)PricingStructureValuation).discountFactorCurve, curveId.Algorithm, curveId.PricingStructureType);
                //Set the priceable assets.
                if (validAssets)//!optimize && 
                {
                    PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, curveId.BaseDate, qas, settlementCalendar, rollCalendar);
                }
            }
        }

        #endregion

        #region Helpers

        internal TermCurve SetConfigurationData()
        {
            //GetRateCurveId();
            // The bootstrapper to use
            if (Holder != null)
            {
                var bootstrapper = Holder.GetValue("Bootstrapper");
                if (bootstrapper != null)
                {
                    BootstrapperName = Holder.GetValue("Bootstrapper");
                }
                var extrapolationPermitted = Holder.GetValue("ExtrapolationPermitted");
                if (extrapolationPermitted != null)
                {
                    ExtrapolationPermitted = bool.Parse(extrapolationPermitted);
                }
                var interpolation = InterpolationMethodHelper.Parse(
                    Holder.GetValue("BootstrapperInterpolation"));
                if (interpolation != null)
                {
                    InterpolationMethod = interpolation;
                }
            }
            var termCurve = new TermCurve
            {
                extrapolationPermitted =
                    ExtrapolationPermitted,
                extrapolationPermittedSpecified = true,
                interpolationMethod =InterpolationMethod
            };
            return termCurve;
        }

        private double GetDefaultTolerance(PricingStructureAlgorithmsHolder algorithms)
        {
            string tolerance = algorithms.GetValue(AlgorithmsProp.Tolerance);
            return double.Parse(tolerance);
        }

        private CompoundingFrequencyEnum GetDefaultCompounding(PricingStructureAlgorithmsHolder algorithms)
        {
            string compounding = algorithms.GetValue(AlgorithmsProp.CompoundingFrequency);
            return EnumHelper.Parse<CompoundingFrequencyEnum>(compounding);
        }

        private IDayCounter _dayCounter;

        public IDayCounter DayCounter
        {
            get
            {
                if (_dayCounter == null)
                {
                    _dayCounter = DayCounterHelper.Parse("ACT/365.FIXED");
                    if (Holder != null)
                    {
                        string dayCountBasis = Holder.GetValue("DayCounter");
                        _dayCounter = DayCounterHelper.Parse(dayCountBasis);
                    }
                }
                return _dayCounter;
            }
        }

        internal void SetInterpolator(TermCurve yieldToMaturityCurve, string algorithm, PricingStructureTypeEnum psType)
        {
            IDayCounter interpolateDayCounter = Actual365.Instance;
            Interpolator = new TermCurveInterpolator(yieldToMaturityCurve, GetBaseDate(), interpolateDayCounter);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(BondCurveIdentifier curveId, TermCurve termCurve, QuotedAssetSet quotedAssetSet)
        {
            YieldCurve yieldCurve = CreateYieldCurve(curveId);
            YieldCurveValuation yieldCurveValuation = CreateYieldCurveValuation(curveId, quotedAssetSet, yieldCurve.id, termCurve);
            var fpmlData = new Pair<PricingStructure, PricingStructureValuation>(yieldCurve, yieldCurveValuation);
            SetFpMLData(fpmlData, false);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(BondCurveIdentifier curveId, TermCurve termCurve, IEnumerable<IPriceableBondAssetController> priceableBondAssets)
        {
            QuotedAssetSet quotedAssetSet = priceableBondAssets != null ? PriceableAssetFactory.Parse(priceableBondAssets) : null;
            CreatePricingStructure(curveId, termCurve, quotedAssetSet);
        }

        #endregion

        #region Evaluations

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote()
        {
            if (PriceableBondAssets != null)
            {
                return EvaluateImpliedQuote(this, PriceableBondAssets.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (PriceableBondAssets != null)//Bootstrapper
            {
                return EvaluateImpliedQuote(this, PriceableBondAssets.ToArray());
            }
            var curveValuation = GetYieldCurveValuation();
            PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, curveValuation.baseDate.Value, curveValuation.inputs, fixingCalendar, rollCalendar);
            return EvaluateImpliedQuote(this, PriceableBondAssets.ToArray());

        }

        /// <summary>
        /// Evaluates the implied quotes.
        /// </summary>
        /// <param name="bondCurve">The bond curve.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        public NamedValueSet EvaluateImpliedQuote(IBondCurve bondCurve, IPriceableBondAssetController[] priceableAssets)
        {
            return EvaluateImpliedQuote(priceableAssets, bondCurve);
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="bondCurve"></param>
        public NamedValueSet EvaluateImpliedQuote(IPriceableBondAssetController[] priceableAssets,
                                                                    IBondCurve bondCurve)
        {
            var result = new NamedValueSet();
            foreach (var priceableAsset in priceableAssets)
            {
                result.Set(priceableAsset.Id, priceableAsset.CalculateImpliedQuote(bondCurve));
                //var marketQuote = MarketQuoteHelper.NormalisePriceUnits(priceableAsset.MarketQuote, "DecimalRate").value;
            }
            return result;
        }

        #endregion

        #region FpML

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="includeId">Include an id check.</param>
        protected void SetFpMLData(Pair<PricingStructure, PricingStructureValuation> fpmlData, Boolean includeId)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
            if (includeId)
            {
                DateTime baseDate = PricingStructureValuation.baseDate.Value;
                PricingStructureIdentifier = new RateCurveIdentifier(PricingStructure.id, baseDate);
            }
        }

        #endregion

        #region Discount Factor and Forward Rate Methods

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="dateTime1"></param>
        ///// <param name="dateTime2"></param>
        ///// <param name="dayCounter"></param>
        ///// <returns></returns>
        //public double GetForwardRate(DateTime dateTime1, DateTime dateTime2, string dayCounter)
        //{
        //    IDayCounter stronglyTypedDayCounter = DayCounterHelper.Parse(dayCounter);
        //    return GetForwardRate(dateTime1, dateTime2, stronglyTypedDayCounter);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="dateTime1"></param>
        ///// <param name="dateTime2"></param>
        ///// <param name="dayCounter"></param>
        ///// <returns></returns>
        //public double GetForwardRate(DateTime dateTime1, DateTime dateTime2, IDayCounter dayCounter)
        //{
        //    var df1 = GetYieldToMaturity(dateTime1);
        //    var df2 = GetYieldToMaturity(dateTime2);
        //    var yearFraction = dayCounter.YearFraction(dateTime1, dateTime2);
        //    var forwardRate = 0.0d;
        //    if (yearFraction != 0)
        //    {
        //        forwardRate = (df1 / df2 - 1) / yearFraction;
        //    }
        //    return forwardRate;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="curveId"></param>
        ///// <param name="rangeOfDates"></param>
        ///// <param name="dayCounter"></param>
        ///// <returns></returns>
        //public List<DoubleRangeItem> GetForwardRates(string curveId, List<DateTimePairRangeItem> rangeOfDates, string dayCounter)
        //{
        //    return rangeOfDates.Select(item => new DoubleRangeItem
        //    {
        //        Value = GetForwardRate(item.Value1, item.Value2, dayCounter)
        //    }).ToList();
        //}

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetYieldToMaturity(DateTime valuationDate, DateTime targetDate)
        {
            double value;
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                value = YieldToMaturityValue(valuationDate, targetDate);
            }
            else
            {
                var value1 = YieldToMaturityValue(GetBaseDate(), targetDate);
                var value2 = YieldToMaturityValue(GetBaseDate(), valuationDate);
                value = value1 / value2;
            }
            return value;
        }


        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        public double GetYieldToMaturity(double time)
        {
            var point = new Point1D(time);
            var value = Value(point);
            return value;
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetYieldToMaturity(DateTime targetDate)
        {
            return GetYieldToMaturity(GetBaseDate(), targetDate);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="valuationDate">THe valuation Date</param>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetYieldToMaturity(DateTime valuationDate, DateTime[] targetDates)
        {
            return targetDates.Select(targetDate => GetYieldToMaturity(valuationDate, targetDate)).ToArray();
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetYieldToMaturity(DateTime[] targetDates)
        {
            return GetYieldToMaturity(GetBaseDate(), targetDates);
        }

        /// <summary>
        /// DFs the value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public double YieldToMaturityValue(DateTime baseDate, DateTime date)
        {
            var point = new DateTimePoint1D(baseDate, date);
            var value = Value(point);
            return value;
        }

        #endregion

        #region Yield Curve Properties

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public YieldCurveValuation GetYieldCurveValuation()
        {
            return (YieldCurveValuation)PricingStructureValuation;
        }

        ///// <summary>
        ///// Gets the zero rate.
        ///// </summary>
        ///// <param name="targetDate">The target date.</param>
        ///// <returns></returns>
        //public double GetZeroRate(DateTime targetDate)
        //{
        //    return GetZeroRate(PricingStructureValuation.baseDate.Value, targetDate, CompoundingFrequency);
        //}

        ///// <summary>
        ///// Gets the zero rate.
        ///// </summary>
        ///// <param name="baseDate">The base date.</param>
        ///// <param name="targetDate">The target date.</param>
        ///// <param name="compoundingFrequency"></param>
        ///// <returns></returns>
        //public double GetZeroRate(DateTime baseDate, DateTime targetDate, CompoundingFrequencyEnum compoundingFrequency)
        //{
        //    var yearFraction = DayCounter.YearFraction(baseDate, targetDate);
        //    var discountFactor = GetYieldToMaturity(baseDate, targetDate);
        //    var zeroRate = RateAnalytics.DiscountFactorToZeroRate(discountFactor, yearFraction, compoundingFrequency);
        //    return zeroRate;
        //}

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public override QuotedAssetSet GetQuotedAssetSet()
        {
            return GetYieldCurveValuation().inputs;
        }

        /// <summary>
        /// Gets the quoted assets.
        /// </summary>
        /// <returns></returns>
        public Asset[] GetAssets()
        {
            return GetYieldCurveValuation().inputs.instrumentSet.Items;
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public override TermCurve GetTermCurve()
        {
            return GetYieldCurveValuation().discountFactorCurve;
        }

        public override bool PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, decimal[] values, string measureType)
        {
            throw new NotImplementedException();
        }

        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the quoted asset item.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Asset GetQuotedAsset(int item)
        {
            return GetQuotedAssetSet().instrumentSet.Items[item];
        }

        /// <summary>
        /// Gets the quoted asset valuation for a specific asset.
        /// </summary>
        /// <param name="item">The item to return. Must be zero based</param>
        /// <param name="measureType">The measureType of the quotation required.</param>
        /// <returns></returns>
        public Decimal GetAssetQuotation(int item, String measureType)
        {
            var numQuotes = GetQuotedAssetSet().assetQuote.Length;
            var result = 0m;
            if (item < numQuotes)
            {
                var quotes = GetQuotedAssetSet().assetQuote[item].quote;
                var quote = MarketQuoteHelper.FindQuotationByMeasureType(measureType, new List<BasicQuotation>(quotes));
                if (quote != null)
                {
                    result = quote.value;
                }
            }
            return result;
        }

        #endregion

        #region Light Perturbation Without Cache or BusinessCalendars

        ///// <summary>
        ///// Generates a perturbed curve for those items specified.
        ///// If the instruments are not valid they are excluded.
        ///// </summary>
        ///// <param name="perturbationArray">The perturbation Array: instrumentId and value.</param>
        ///// <returns></returns>
        //public IRateCurve PerturbCurve(List<Pair<string, decimal>> perturbationArray)
        //{
        //    if (PriceableBondAssets == null) return null;

        //    //Set the parameters and properties.
        //    NamedValueSet properties = GetPricingStructureId().Properties.Clone();
        //    string uniqueId = GetPricingStructureId().UniqueIdentifier;
        //    //Clone the properties.
        //    NamedValueSet curveProperties = properties.Clone();
        //    curveProperties.Set("PerturbedCurve", true);
        //    curveProperties.Set("BaseCurve", uniqueId);
        //    curveProperties.Set(CurveProp.UniqueIdentifier, uniqueId + "." + "PerturbedCurve");
        //    foreach (var instrument in perturbationArray)
        //    {
        //        //TODO clone the priceable assets.
        //        var asset = PriceableBondAssets.FindAll(a => a.Id.Equals(instrument.First));
        //        if (asset[0] == null) continue;
        //        var temp = asset[0];
        //        temp.MarketQuote.value = temp.MarketQuote.value + instrument.Second / 10000;
        //    }
        //    //Create the new curve.
        //    var xccySpreadCurve = this as XccySpreadCurve;
        //    //PerturbedPriceableAssets(PriceableRateAssets, perturbations);
        //    IRateCurve rateCurve = xccySpreadCurve != null ? new XccySpreadCurve(curveProperties, xccySpreadCurve.BaseCurve, PriceableBondAssets, Holder) : new RateCurve(curveProperties, PriceableBondAssets, Holder);
        //    return rateCurve;
        //}

        ///// <summary>
        ///// Creates a perturbed copy of the curve.
        ///// </summary>
        ///// <returns></returns>
        //protected void PerturbedPriceableAssets(List<IPriceableBondAssetController> priceableBondAssets, Decimal[] values)
        //{
        //    var numControllers = priceableBondAssets.Count;
        //    if ((values.Length != numControllers)) return;//(PriceableRateAssets == null) || 
        //    var index = 0;
        //    foreach (var rateController in priceableBondAssets)
        //    {
        //        rateController.MarketQuote.value = values[index];
        //        index++;
        //    }
        //}

        //protected List<decimal> GetMarketQuotes(IEnumerable<IPriceableBondAssetController> priceableBondAssets)
        //{
        //    return priceableBondAssets.Select(asset => asset.MarketQuote.value).ToList();
        //}

        ////TODO Get RId of This!!!!!
        ///// <summary>
        ///// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measuretype to determine which one.
        ///// The original curve is modified. This does not create a copy!!
        ///// </summary>
        ///// <param name="logger">The logger.</param>
        ///// <param name="cache">The cache.</param>
        ///// <param name="fixingCalendar">The fixingCalendar.</param>
        ///// <param name="rollCalendar">The rollCalendar.</param>
        /////  <param name="nameSpace">The client namespace</param>
        ///// <param name="assetItemIndex">The item to return. Must be zero based</param>
        ///// <param name="value">The value to update to.</param>
        ///// <param name="measureType">The measureType of the quotation required.</param>
        ///// <returns></returns>
        //public Boolean PerturbCurve(ILogger logger, ICoreCache cache, 
        //    string nameSpace, int assetItemIndex, Decimal value, String measureType,
        //    IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        //{
        //    var quotes = ((YieldCurveValuation)PricingStructureValuation).inputs.assetQuote[assetItemIndex].quote;
        //    var val = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType(measureType, new List<BasicQuotation>(quotes), value);
        //    ((YieldCurveValuation)PricingStructureValuation).inputs.assetQuote[assetItemIndex].quote = val;
        //    Build(logger, cache, nameSpace, fixingCalendar, rollCalendar);
        //    return true;
        //}

        ////TODO Get RId of This!!!!!
        ///// <summary>
        ///// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measuretype to determine which one.
        ///// The original curve is modified. This does not create a copy!!
        ///// </summary>
        ///// <param name="logger">The logger.</param>
        ///// <param name="cache">The cache.</param>
        /////  <param name="nameSpace">The client namespace</param>
        ///// <param name="values">The value to update to.</param>
        ///// <param name="measureType">The measureType of the quotation required.</param>
        ///// <returns></returns>
        //public override Boolean PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, Decimal[] values, String measureType)
        //{
        //    var quotes = ((YieldCurveValuation)PricingStructureValuation).inputs.assetQuote;
        //    if (values.Length == quotes.Length)
        //    {
        //        var index = 0;
        //        foreach (var quote in quotes)
        //        {
        //            var val = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType(measureType, new List<BasicQuotation>(quote.quote), values[index]);
        //            quote.quote = val;
        //            index++;
        //        }
        //    }
        //    Build(logger, cache, nameSpace, SettlementCalendar, PaymentCalendar);
        //    return true;
        //}

        ///// <summary>
        ///// Updates a basic quotation value and then perturbs and rebuilds the curve. Uses the measuretype to determine which one.
        ///// The original curve is modified. This does not create a copy!!
        ///// </summary>
        ///// <param name="logger">The logger.</param>
        ///// <param name="cache">The cache.</param>
        ///// <param name="nameSpace">The client namespace</param>
        ///// <param name="indices">An array of indices of assets to perturb.</param>
        ///// <param name="values">The value to update to.</param>
        ///// <param name="measureType">The measureType of the quotation required.</param>
        ///// <returns></returns>
        //public Boolean PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, int[] indices, Decimal[] values, String measureType)
        //{
        //    var quotes = ((YieldCurveValuation)PricingStructureValuation).inputs.assetQuote;
        //    if (values.Length == indices.Length)
        //    {
        //        var index = 0;
        //        foreach (var idx in indices)
        //        {
        //            if (idx > quotes.Length) continue;
        //            var val = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType(measureType, new List<BasicQuotation>(quotes[idx].quote),
        //                                                                           values[index]);
        //            quotes[idx].quote = val;
        //            index++;
        //        }
        //    }
        //    Build(logger, cache, nameSpace, SettlementCalendar, PaymentCalendar);
        //    return true;
        //}

        ///// <summary>
        ///// Creates the basic rate curve risk set, using the current curve as the base curve.
        ///// This function takes a curves, creates a rate curve for each instrument and applying 
        ///// supplied basis point pertubation/spread to the underlying instrument in the spread curve
        ///// </summary>
        ///// <param name="basisPointPerturbation">The basis point perturbation.</param>
        ///// <returns>A list of pertubed rate curves</returns>
        //public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        //{
        //    if (PriceableBondAssets == null) return null;
        //    //Set the parameters and properties.
        //    decimal perturbation = basisPointPerturbation / 10000.0m;
        //    NamedValueSet properties = GetPricingStructureId().Properties.Clone();
        //    properties.Set("PerturbedAmount", basisPointPerturbation);
        //    string uniqueId = GetPricingStructureId().UniqueIdentifier;
        //    //Get the assets, BUT REMOVE ALL THE BASIS SWAPS, to prevent double accounting of risks.
        //    int index = 0;
        //    var structures = new List<IPricingStructure>();
        //    //Get the original quotes
        //    var quotes = GetMarketQuotes(PriceableBondAssets);
        //    foreach (var instrument in PriceableBondAssets)
        //    {
        //        var perturbations = new decimal[quotes.Count];
        //        quotes.CopyTo(perturbations);
        //        //Clone the properties.
        //        NamedValueSet curveProperties = properties.Clone();
        //        perturbations[index] = quotes[index] + perturbation;
        //        curveProperties.Set("PerturbedAsset", instrument.Id);
        //        curveProperties.Set("BaseCurve", uniqueId);
        //        curveProperties.Set(CurveProp.UniqueIdentifier, uniqueId + "." + instrument.Id);
        //        curveProperties.Set(CurveProp.Tolerance, Tolerance);
        //        //Create the new curve.
        //        var xccySpreadCurve = this as XccySpreadCurve;
        //        //Perturb the quotes
        //        PerturbedPriceableAssets(PriceableBondAssets, perturbations);
        //        IPricingStructure rateCurve = xccySpreadCurve != null ?
        //            new XccySpreadCurve(curveProperties, xccySpreadCurve.BaseCurve, PriceableBondAssets, Holder)
        //            : new RateCurve(curveProperties, PriceableBondAssets, Holder);
        //        structures.Add(rateCurve);
        //        //Set the counter.
        //        perturbations[index] = 0;
        //        index++;
        //    }
        //    return structures;
        //}

        #endregion

        #region Build and Clone

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="settlementCalendar">The settlementCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar settlementCalendar, IBusinessCalendar rollCalendar)
        {
            PriceableBondAssets = PriceableAssetFactory.CreatePriceableBondAssets(logger, cache, nameSpace, GetYieldCurveValuation().baseDate.Value, GetYieldCurveValuation().inputs, settlementCalendar, rollCalendar);
            GetYieldCurveValuation().zeroCurve = null;
            TermCurve termCurve = GetTermCurve();
            DateTime baseDate = GetYieldCurveValuation().baseDate.Value;
            termCurve.point = BondBootstrapper.Bootstrap(PriceableBondAssets, baseDate, termCurve.extrapolationPermitted, termCurve.interpolationMethod, Tolerance);
            SetFpMLData(new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation), false);
            SetInterpolator(termCurve, ((RateCurveIdentifier)PricingStructureIdentifier).Algorithm, ((RateCurveIdentifier)PricingStructureIdentifier).PricingStructureType);
        }

        /// <summary>
        /// Clones the curve.
        /// </summary>
        /// <returns></returns>
        public object Clone()//TODO there may be a problem with this. The df curve is set to null.
        {
            //Clone the properties ande set values.
            //
            NamedValueSet properties = GetProperties().Clone();
            var baseCurveId = properties.GetString(CurveProp.UniqueIdentifier, true);
            var id = baseCurveId + ".Clone";
            properties.Set(CurveProp.UniqueIdentifier, id);
            //Clone the curve data.
            //
            var fpml = GetFpMLData();
            var rateCurve = new BondCurve(NameSpace, Holder, CloneCurve(fpml, id), properties, null, null);
            return rateCurve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> CloneCurve(Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            PricingStructure ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            PricingStructureValuation ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            //Dont want to null ther dfs
            //
            //ycv.discountFactorCurve.point = null;
            //ycv.zeroCurve = null;
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        #endregion

        #region IPricingStructure

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
            var value = Interpolator.Value(point);         
            return value;
        }

        /// <summary>
        /// Gets the rate curve id.
        /// </summary>
        /// <returns></returns>
        public BondCurveIdentifier GetRateCurveId()
        {
            return (BondCurveIdentifier)PricingStructureIdentifier;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="pt">The pt.</param>
        /// <returns></returns>
        public override IValue GetValue(IPoint pt)
        {
            return new DoubleValue("dummy", Value(pt), pt);
        }

        /// <summary>
        /// Returns a list of non-interpolated values found in a immediate vicinity of requested point.
        /// <remarks>
        /// If a GetValue method returns a exact match - this method should be returning null.
        /// </remarks>
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public override IList<IValue> GetClosestValues(IPoint pt)
        {
            GetDiscreteSpace().GetFunctionValueArray();
            var times = GetDiscreteSpace().GetCoordinateArray(1);
            var values = GetDiscreteSpace().GetFunctionValueArray();
            var index = Array.BinarySearch(times, pt.Coords[1]);
            if (index >= 0)
            {
                return null;
            }
            var nextIndex = ~index;
            var prevIndex = nextIndex - 1;
            var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));
            IList<IValue> result = new List<IValue> { prevValue, nextValue };
            return result;
        }

        /// <summary>
        /// Gets the curve algorithm.
        /// </summary>
        /// <returns></returns>
        public override string GetAlgorithm()
        {
            return ((BondCurveIdentifier)PricingStructureIdentifier).Algorithm;
        }

        #endregion

        #region Other Stuff

        /// <summary>
        /// Creates the yield curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <returns></returns>
        protected static YieldCurve CreateYieldCurve(BondCurveIdentifier curveId)
        {
            var yieldCurve = new YieldCurve
            {
                id = curveId.Id,
                name = curveId.CurveName,
                algorithm = curveId.Algorithm,
                currency = curveId.Currency,
            };
            return yieldCurve;
        }

        /// <summary>
        /// Creates the yield curve valuation.
        /// </summary>
        /// <param name="curveId"></param>
        /// <param name="quotedAssetSet">The quoted asset set.</param>
        /// <param name="yieldCurveId">The yield curve id.</param>
        /// <param name="discountFactorCurve"></param>
        /// <returns></returns>
        protected static YieldCurveValuation CreateYieldCurveValuation(PricingStructureIdentifier curveId, QuotedAssetSet quotedAssetSet,
                                                                       string yieldCurveId, TermCurve discountFactorCurve)
        {
            var yieldCurveValuation = new YieldCurveValuation
            {
                baseDate = IdentifiedDateHelper.Create(curveId.BaseDate),
                buildDateTime = curveId.BuildDateTime,
                buildDateTimeSpecified = true,
                inputs = quotedAssetSet,
                id = yieldCurveId,
                definitionRef = curveId.PricingStructureType.ToString(),
                discountFactorCurve = discountFactorCurve
            };
            return yieldCurveValuation;
        }

        /// <summary>
        /// Parses the underlying curve.
        /// </summary>
        /// <param name="underlyingCurveAsString">The underlying curve.</param>
        /// <returns></returns>
        protected static UnderlyingCurveTypes ParseUnderlyingCurve(string underlyingCurveAsString)
        {
            return EnumHelper.Parse<UnderlyingCurveTypes>(underlyingCurveAsString);
        }


        #endregion

        #region QuotedAssetSet Helpers

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The array of spread values must be the same length as the array of assets.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValues"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal[] spreadValues,
                                                                                     Pair<PricingStructure, PricingStructureValuation> bondCurve, BondCurveIdentifier curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId.Id;
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            if (spreadValues.Length == ycv.inputs.instrumentSet.Items.Length)
            {
                var index = 0;
                foreach (var bav in ycv.inputs.assetQuote)
                {
                    bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                      new List<BasicQuotation>(bav.quote),
                                                                                      spreadValues[index]);
                    index++;
                }
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The array of spread values must be the same length as the array of assets.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValueSet">THe assets must be the same as thoe in the reference curve. Any asset not the same the spread value will be excluded.
        /// and the base value reapplied. Also any basis swaps will be removed.</param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(QuotedAssetSet spreadValueSet,
                                                                                             Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            //This strips out the basis swaps.
            //
            ycv.inputs = AssetHelper.RemoveAssetsFromQuotedAssetSet(AssetTypesEnum.BasisSwap, ycv.inputs);
            var instruments = ycv.inputs.instrumentSet.Items.ToList();
            var index = 0;
            foreach (var asset in spreadValueSet.instrumentSet.Items)
            {                
                var idx = instruments.FindIndex(quotationItem => String.Compare(quotationItem.id, asset.id, StringComparison.OrdinalIgnoreCase) == 0);
                ycv.inputs.assetQuote[idx].quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                      new List<BasicQuotation>(ycv.inputs.assetQuote[idx].quote),
                                                                                      spreadValueSet.assetQuote[index].quote[0].value);
                index++;
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The array of spread values must be the same length as the array of assets.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValues"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal[] spreadValues,
                                                                                             Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            //This strips out the basis swaps.
            //
            ycv.inputs = AssetHelper.RemoveAssetsFromQuotedAssetSet(AssetTypesEnum.BasisSwap, ycv.inputs);
            if (spreadValues.Length == ycv.inputs.instrumentSet.Items.Length)
            {
                var index = 0;
                foreach (var bav in ycv.inputs.assetQuote)
                {
                    bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                      new List<BasicQuotation>(bav.quote),
                                                                                      spreadValues[index]);
                    index++;
                }
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValue"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal spreadValue,
                                                                                            Pair<PricingStructure, PricingStructureValuation> bondCurve, IIdentifier curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId.Id;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            foreach (var bav in ycv.inputs.assetQuote)
            {
                bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                  new List<BasicQuotation>(bav.quote),
                                                                                  spreadValue);
            }

            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Adds a spread to an asset. The spread value is added to each asset in the original curve.
        /// The curve is renamed with the id provided.
        /// </summary>
        /// <param name="spreadValue"></param>
        /// <param name="bondCurve"></param>
        /// <param name="curveId"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> AdjustMarketQuotes(Decimal spreadValue,
                                                                                            Pair<PricingStructure, PricingStructureValuation> bondCurve, string curveId)
        {
            var ycCurveCloned = XmlSerializerHelper.Clone(bondCurve.First);
            var ycvCurveCloned = XmlSerializerHelper.Clone(bondCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var yc = (YieldCurve)ycCurveCloned;
            yc.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var ycv = (YieldCurveValuation)ycvCurveCloned;
            ycv.discountFactorCurve.point = null;
            ycv.zeroCurve = null;
            ycv.forwardCurve = null;
            foreach (var bav in ycv.inputs.assetQuote)
            {
                bav.quote = MarketQuoteHelper.AddAndReplaceQuotationByMeasureType("MarketQuote",
                                                                                  new List<BasicQuotation>(bav.quote),
                                                                                  spreadValue);
            }
            return new Pair<PricingStructure, PricingStructureValuation>(yc, ycv);
        }

        /// <summary>
        /// Clones a curve, sets the quoted asset set specified and then returns an FpML structure back.
        /// </summary>
        /// <param name="referenceCurve"></param>
        /// <param name="cleanedAssetSet"></param>
        /// <returns></returns>
        protected static Pair<PricingStructure, PricingStructureValuation> ProcessCurveRiskSet(IBondCurve referenceCurve, QuotedAssetSet cleanedAssetSet)
        {
            //Clone the ref curves.
            //
            var clonedCurve = referenceCurve.Clone();
            var fpml = ((IBondCurve)clonedCurve).GetFpMLData();
            var ycvCurveCloned = (YieldCurveValuation)fpml.Second;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            ycvCurveCloned.discountFactorCurve.point = null;
            ycvCurveCloned.zeroCurve = null;
            ycvCurveCloned.forwardCurve = null;
            //Manipulate the quoted asset set.
            //
            ycvCurveCloned.inputs = cleanedAssetSet;
            return fpml;
        }

        #endregion

        #region Factory methods

        ///// <summary>
        ///// Creates a new Discount Curve by adjusting an existing Rate Curve.
        ///// </summary>
        ///// <param name="cache">The cache.</param>
        ///// <param name="logger">The logger.</param>
        /////  <param name="nameSpace">The client namespace</param>
        ///// <param name="fixingCalendar">The fixingCalendar.</param>
        ///// <param name="rollCalendar">The rollCalendar.</param>
        ///// <param name="baseCurve">The original rate curve the new curve is based on.</param>
        ///// <param name="properties">The properties of the new curve.</param>
        ///// <param name="spreads">The adjustments required.</param>
        ///// <returns>The newly created curve.</returns>
        ///// <remarks>
        ///// Create a new extended curve (Ca) from the base curve with extra points where the spreads are
        ///// Solve to find the zero rate spreads (Sa) required, interpolating between points
        ///// Retrieve the zero rates (Za) from curve Ca 
        ///// Add on the zero rate spreads Sa  onto Za, creating a new set of zero rates (Zb)
        ///// Convert the zero rates Zb into discount factors (Db)
        ///// Create a new rate curve from Db
        ///// Return this curve
        ///// </remarks>
        //public static RateCurve CreateAdjustedRateCurve(ILogger logger, ICoreCache cache, 
        //    string nameSpace, RateCurve baseCurve, NamedValueSet properties,
        //    IList<Pair<string, double>> spreads, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        //{
        //    var baseDate = properties.GetValue<DateTime>(CurveProp.BaseDate, true);
        //    var curveId = new RateCurveIdentifier(properties);
        //    //previously the RateCurve Holder was used.
        //    var algorithmHolder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, curveId.PricingStructureType, curveId.Algorithm);
        //    // Create a spread asset list
        //    var spreadAssets
        //        = spreads.Select(a => AssetHelper.ParseSimplified(a.First, 0, 0)).ToList();
        //    // Load back out the base curve, and populate an instrument set
        //    List<Triplet<string, decimal, int>> extendedCurvePoints
        //        = MakeExtendedCurve(logger, cache, nameSpace, baseCurve, baseDate, spreadAssets, fixingCalendar, rollCalendar);
        //    // Create Extended curve using the original rates
        //    string[] instruments = extendedCurvePoints.Select(a => a.First).ToArray();
        //    decimal[] rates = extendedCurvePoints.Select(a => a.Second).ToArray();
        //    // first create a list of zeros, these will then get populated in the adjustments loop
        //    decimal[] adjustments = rates.Select(a => 0m).ToArray();
        //    // Solve for each supplied spread
        //    // Finding the adjustments necessary to create a RateCurve that has Discounts
        //    // with the required spreads
        //    int endIndex = -1;
        //    for (int i = 0; i < spreadAssets.Count; i++)
        //    {
        //        endIndex = CalculateSpreads(logger, cache, nameSpace, baseDate, properties, extendedCurvePoints, 
        //            spreadAssets[i].First, spreadAssets[i].Second, spreads[i].Second, endIndex, adjustments, fixingCalendar, rollCalendar);
        //    }
        //    const CompoundingFrequencyEnum compounding = CompoundingFrequencyEnum.Quarterly;
        //    const int daysInYear = 365;
        //    //// Create extended curve using the original rates
        //    List<DateTime> dates = extendedCurvePoints.Select(a => baseDate.AddDays(a.Third)).ToList();
        //    var quotedAssetSet = AssetHelper.Parse(instruments, rates, adjustments);
        //    var extendedCurve = new RateCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
        //    List<double> extendedZeroRates = extendedCurve.GetDaysAndZeroRates(baseDate, compounding).Values.ToList();
        //    var newPoints = new Dictionary<DateTime, Pair<string, decimal>>();
        //    // Create adjusted extended curve using the solved adjustments
        //    for (int i = 0; i < extendedZeroRates.Count() - 1; i++)
        //    {
        //        double yearFraction = dates[i].Subtract(baseDate).Days / (double)daysInYear;
        //        double newZeroRate = extendedZeroRates[i + 1] + (double)adjustments[i];
        //        double discount = RateAnalytics.ZeroRateToDiscountFactor(newZeroRate, yearFraction, compounding);
        //        newPoints.Add(dates[i], new Pair<string, decimal>("", (decimal)discount));
        //    }
        //    properties.Set("Usage", "Adjusted");
        //    var discountCurve = new RateCurve(properties, algorithmHolder, newPoints);

        //    // Return the discount curve
        //    return discountCurve;
        //}

        //private static List<Triplet<string, decimal, int>> MakeExtendedCurve(ILogger logger, ICoreCache cache, string nameSpace, RateCurve baseCurve,
        //    DateTime baseDate, IEnumerable<Pair<string, BasicAssetValuation>> spreadAssets, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        //{
        //    List<IPriceableRateAssetController> priceableRateAssets = baseCurve.PriceableRateAssets;
        //    List<decimal> rates = priceableRateAssets.Select(a => a.MarketQuote.value).ToList();
        //    List<int> days = priceableRateAssets.Select(a => a.GetRiskMaturityDate().Subtract(baseDate).Days).ToList();
        //    List<string> instruments = priceableRateAssets.Select(a => a.Id).ToList();
        //    // Make a list of points of triplets of instrument, rate, days
        //    var extendedCurvePoints
        //        = instruments.Select((t, i) => new Triplet<string, decimal, int>(t, rates[i], days[i])).ToList();
        //    // Add in the spread points, if they aren't present
        //    foreach (var spreadAsset in spreadAssets)
        //    {
        //        var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, spreadAsset.First, baseDate);
        //        var priceableSpreadAsset = PriceableAssetFactory.Create(logger, cache, nameSpace, spreadAsset.Second, assetProperties, fixingCalendar, rollCalendar);
        //        DateTime maturity = priceableSpreadAsset.GetRiskMaturityDate();
        //        int spreadDays = maturity.Subtract(baseDate).Days;
        //        if (!days.Contains(spreadDays))
        //        {
        //            // Find the Implied Quote
        //            var value = priceableSpreadAsset.CalculateImpliedQuote(baseCurve);
        //            // Add the point
        //            var point = new Triplet<string, decimal, int>(spreadAsset.First, value, spreadDays);
        //            extendedCurvePoints.Add(point);
        //        }
        //    }
        //    // Sort the points
        //    extendedCurvePoints = extendedCurvePoints.OrderBy(a => a.Third).ToList();
        //    return extendedCurvePoints;
        //}

        //private static int CalculateSpreads(ILogger logger, ICoreCache cache, string nameSpace,
        //    DateTime baseDate, NamedValueSet properties, IList<Triplet<string, decimal, int>> assets,
        //    string spreadAssetId, BasicAssetValuation spreadAssetValuation, double spread, int previousEndIndex,
        //    decimal[] adjustments, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        //{
        //    var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, spreadAssetId, baseDate);
        //    IPriceableAssetController priceableAsset = PriceableAssetFactory.Create(logger, cache, nameSpace, spreadAssetValuation, assetProperties, fixingCalendar, rollCalendar);
        //    int periodDays = priceableAsset.GetRiskMaturityDate().Subtract(baseDate).Days;
        //    int endIndex = 0;
        //    for (int i = 0; i < assets.Count; i++)
        //    {
        //        if (assets[i].Third == periodDays)
        //        {
        //            endIndex = i;
        //            break;
        //        }
        //    }
        //    if (spread != 0)
        //    {
        //        int startIndex = previousEndIndex + 1;
        //        double accuracy = Math.Abs(spread) / 1000;
        //        double yearsOut = assets[startIndex].Third / 365d;
        //        double sign = spread / Math.Abs(spread);
        //        double variance = 2 * (spread + yearsOut * spread);
        //        double lowerBound = spread - sign * variance;
        //        double upperBound = spread + sign * variance;

        //        var tempSolverClass
        //            = new DiscountCurveSolver(logger, cache, nameSpace, baseDate, properties, assets, adjustments,
        //                spreadAssetId, spreadAssetValuation, periodDays, spread, startIndex, endIndex, fixingCalendar, rollCalendar);

        //        var solver = new Brent();
        //        solver.Solve(tempSolverClass, accuracy, spread, lowerBound, upperBound);
        //    }
        //    return endIndex;
        //}

        internal static double CalculateImpliedQuote(ILogger logger, ICoreCache cache, 
            string nameSpace, DateTime baseDate, NamedValueSet properties,
            string[] fraInstruments, decimal[] fraRates, decimal[] adjustments,
            string spreadAssetId, BasicAssetValuation spreadAssetValuation, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            properties.Set("Usage", "Solver");
            // create the pricing structure & put into Cache
            var quotedAssetSet = AssetHelper.Parse(fraInstruments, fraRates, adjustments);
            IPricingStructure fraPricingStructure = new RateCurve(logger, cache, nameSpace, properties, quotedAssetSet, fixingCalendar, rollCalendar);
            //Solve 
            var assetProperties = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, spreadAssetId, baseDate);
            IPriceableAssetController priceableAsset = PriceableAssetFactory.Create(logger, cache, nameSpace, spreadAssetValuation, assetProperties, fixingCalendar, rollCalendar);
            var value = (double)priceableAsset.CalculateImpliedQuote(fraPricingStructure);

            return value;
        }

        #endregion
    }
}
