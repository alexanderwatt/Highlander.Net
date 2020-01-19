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

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Highlander.Codes.V5r3;
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.Factory;
using Highlander.CurveEngine.V5r3.PricingStructures.Bootstrappers;
using Highlander.CurveEngine.V5r3.PricingStructures.Interpolators;
using Highlander.Reporting.Analytics.V5r3.Interpolations.Points;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.ModelFramework.V5r3.Assets;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Serialisation;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Constants;
using Highlander.CurveEngine.V5r3.Assets.FX;
using Highlander.CurveEngine.V5r3.Helpers;
using Highlander.Reporting.Analytics.V5r3.DayCounters;
using Highlander.Reporting.V5r3;
using AssetClass = Highlander.Constants.AssetClass;

#endregion

namespace Highlander.CurveEngine.V5r3.PricingStructures.Curves
{
    ///<summary>
    ///</summary>
    public class EquityCurve : CurveBase, IEquityCurve
    {
        #region Properties

        /// <summary>
        /// 
        /// </summary>
        public double Tolerance { get; set; }

        ///<summary>
        ///</summary>
        public string Algorithm 
        {
            get { return ((FxCurveIdentifier) PricingStructureIdentifier).Algorithm; }
        }

        /// <summary>
        /// The spot date.
        /// </summary>
        public DateTime SettlementDate { get; set; }

        ///<summary>
        ///</summary>
        public string UnderlyingInterpolatedCurve { get; private set; }

        ///<summary>
        /// The cached rate controllers for the fast bootstrapper.
        ///</summary>
        public List<IPriceableEquityAssetController> PriceableEquityAssets { get; set; }

        /// <summary>
        /// The bootstrapper name.
        /// </summary>
        public String BootstrapperName { get; set; }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public override string GetAlgorithm()
        {
            return Algorithm;
        }

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FxCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="assetSet">The assetSet.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public EquityCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, 
            FxRateSet assetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            InitialiseInstance(logger, cache, nameSpace, properties, assetSet, fixingCalendar, rollCalendar);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        public EquityCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Equity, properties); 
            PricingStructureIdentifier = new FxCurveIdentifier(properties);
            Initialize(properties, algorithmHolder);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="algorithmHolder"></param>
        protected void Initialize(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder)
        {
            Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ?? GetDefaultTolerance(algorithmHolder);
            Holder = algorithmHolder;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableEquityAssets">The priceable equity Assets.</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        public EquityCurve(NamedValueSet curveProperties, List<IPriceableEquityAssetController> priceableEquityAssets, 
            PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : this(curveProperties, pricingStructureAlgorithmsHolder)
        {
            var curveId = GetEquityCurveId();
            var termCurve = SetConfigurationData();
            PriceableEquityAssets = priceableEquityAssets;
            termCurve.point = EquityBootstrapper.Bootstrap(PriceableEquityAssets, curveId.BaseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            CreatePricingStructure(curveId, termCurve, PriceableEquityAssets);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            DateTime baseDate = PricingStructureIdentifier.BaseDate;
            SetInterpolator(baseDate);
        }

        private void InitialiseInstance(ILogger logger, ICoreCache cache, string nameSpace,
            NamedValueSet properties, FxRateSet assetSet, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Equity, properties);
            var curveId = new EquityCurveIdentifier(properties);
            PricingStructureIdentifier = curveId;
            Holder = new PricingStructureAlgorithmsHolder(logger, cache, nameSpace, curveId.PricingStructureType, curveId.Algorithm);
            DateTime baseDate = PricingStructureIdentifier.BaseDate;
            //Set the spot date;
            SettlementDate = GetSettlementDate(logger, cache, nameSpace, GetEquityCurveId(), fixingCalendar, rollCalendar, baseDate);
            //TODO
            //FixingCalendar = null;
            //RollCalendar = null;
            // The bootstrapper to use
            BootstrapperName = Holder.GetValue("Bootstrapper");
            Tolerance = double.Parse(Holder.GetValue("Tolerance"));
            bool extrapolationPermitted = bool.Parse(Holder.GetValue("ExtrapolationPermitted"));
            InterpolationMethod interpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("BootstrapperInterpolation"));
            var termCurve = new TermCurve
            {
                extrapolationPermitted = extrapolationPermitted,
                extrapolationPermittedSpecified = true,
                interpolationMethod = interpolationMethod
            };
            PriceableEquityAssets = PriceableAssetFactory.CreatePriceableEquityAssets(logger, cache, nameSpace, baseDate, assetSet, fixingCalendar, rollCalendar);
            termCurve.point = EquityBootstrapper.Bootstrap(PriceableEquityAssets, baseDate, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod, Tolerance);
            // Pull out the fx curve and fx curve valuation
            Pair<PricingStructure, PricingStructureValuation> fpmlData
                = CreateFpmlPair(logger, cache, nameSpace, baseDate, (EquityCurveIdentifier)PricingStructureIdentifier, assetSet, termCurve, fixingCalendar, rollCalendar);
            SetFpmlData(fpmlData);
            // Interpolate the DiscountFactor curve based on the respective curve interpolation 
            SetInterpolator(baseDate);
        }

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FxCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public EquityCurve(ILogger logger, ICoreCache cache, string nameSpace,
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, new EquityCurveIdentifier(properties))
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Equity, properties); 
            if (properties == null)
            {
                properties = PricingStructurePropertyHelper.FxCurve(fpmlData);//TODO
            }  
            //Set the spot date;
            SettlementDate = GetSettlementDate(logger, cache, nameSpace, GetEquityCurveId(), fixingCalendar, rollCalendar, PricingStructureValuation.baseDate.Value);
            var fxCurveValuation = (FxCurveValuation)fpmlData.Second;
            // hack - set the base date - todo - is this the right place alex?
            PricingStructureValuation.baseDate = new IdentifiedDate { Value = PricingStructureIdentifier.BaseDate };
            SetFpmlData(fpmlData);
            TermCurve termCurve = SetConfigurationData();
            var bootstrap = PropertyHelper.ExtractBootStrapOverrideFlag(properties);
            var buildFlag = (fxCurveValuation.spotRate != null) && (fxCurveValuation.fxForwardCurve == null || fxCurveValuation.fxForwardCurve.point == null);
            // If there's no forward points - do build the curve.
            // - TODO what happened the central bank dates
            if ((bootstrap || buildFlag) && cache!=null)
            {
                // if (BootstrapperName == "FastBootstrapper") //TODO
                PriceableEquityAssets = PriceableAssetFactory.CreatePriceableEquityAssets(logger, cache, nameSpace,
                    fxCurveValuation.baseDate.Value, fxCurveValuation.spotRate, fixingCalendar, rollCalendar);
                fxCurveValuation.fxForwardCurve = termCurve;
                fxCurveValuation.fxForwardCurve.point = EquityBootstrapper.Bootstrap(
                    PriceableEquityAssets,
                    fxCurveValuation.baseDate.Value,
                    fxCurveValuation.fxForwardCurve.extrapolationPermitted,
                    fxCurveValuation.fxForwardCurve.interpolationMethod);
            }
            ((FxCurveValuation)PricingStructureValuation).fxForwardCurve = fxCurveValuation.fxForwardCurve;
            SetInterpolator(PricingStructureValuation.baseDate.Value);
        }

        #endregion

        #region FpML and Configuration

        /// <summary>
        /// Sets the interpolator.
        /// </summary>
        private void SetInterpolator(DateTime baseDate)
        {
            // The underlying curve and associated compounding frequency (compounding frequency required when underlying curve is a ZeroCurve)
            InterpolationMethod curveInterpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("CurveInterpolation"));
            IDayCounter dayCounter = DayCounterHelper.Parse(Holder.GetValue("DayCounter"));
            UnderlyingInterpolatedCurve = Holder.GetValue("UnderlyingCurve"); 
            // Retrieve the Discount factor curve and assign the curve interpolation we want to initiate
            // This depends on the underlying curve type (i.e. rate or discount factor)
            TermCurve termCurve = GetEquityCurveValuation().fxForwardCurve;
            termCurve.interpolationMethod = curveInterpolationMethod;
            // interpolate the DiscountFactor curve based on the respective curve interpolation 
            Interpolator = new FxCurveInterpolator(termCurve, baseDate, dayCounter);
        }

        private static Pair<PricingStructure, PricingStructureValuation> CreateFpmlPair(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate,
            EquityCurveIdentifier curveId, FxRateSet assetSet, TermCurve termCurve, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            Reporting.V5r3.FxCurve fxCurve = CreateEquityCurve(curveId.Id, curveId.CurveName, curveId.Currency);
            FxCurveValuation fxCurveValuation = CreateEquityCurveValuation(logger, cache, nameSpace, curveId, fixingCalendar, rollCalendar, baseDate, assetSet, fxCurve.id, termCurve);
            return new Pair<PricingStructure, PricingStructureValuation>(fxCurve, fxCurveValuation);
        }


        internal TermCurve SetConfigurationData()
        {
            // The bootstrapper to use
            BootstrapperName = Holder.GetValue("Bootstrapper");
            var termCurve = new TermCurve
            {
                extrapolationPermitted = bool.Parse(Holder.GetValue("ExtrapolationPermitted")),
                extrapolationPermittedSpecified = true,
                interpolationMethod = InterpolationMethodHelper.Parse(Holder.GetValue("BootstrapperInterpolation"))
            };
            return termCurve;
        }

        /// <summary>
        /// Sets the fpML data.
        /// </summary>
        /// <param name="fpmlData">The FPML data.</param>
        private void SetFpmlData(Pair<PricingStructure, PricingStructureValuation> fpmlData)
        {
            PricingStructure = fpmlData.First;
            PricingStructureValuation = fpmlData.Second;
        }

        #endregion

        #region Evaluate

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote()
        {
            if (PriceableEquityAssets != null)
            {
                return EvaluateImpliedQuote(this, PriceableEquityAssets.ToArray());
            }
            return null;
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        public NamedValueSet EvaluateImpliedQuote(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            if (PriceableEquityAssets != null)
            {
                return EvaluateImpliedQuote(this, PriceableEquityAssets.ToArray());
            }
            FxCurveValuation curveValuation = GetEquityCurveValuation();
            PriceableEquityAssets = PriceableAssetFactory.CreatePriceableEquityAssets(logger, cache, nameSpace, curveValuation.baseDate.Value, curveValuation.spotRate, fixingCalendar, rollCalendar);
            return EvaluateImpliedQuote(this, PriceableEquityAssets.ToArray());
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="fxCurve">The rate curve.</param>
        /// <param name="priceableAssets">The priceable assets.</param>
        public NamedValueSet EvaluateImpliedQuote(IEquityCurve fxCurve, IPriceableEquityAssetController[] priceableAssets)
        {
            return EvaluateImpliedQuote(priceableAssets, fxCurve);
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="priceableAssets">The priceable assets.</param>
        /// <param name="equityCurve"></param>
        public NamedValueSet EvaluateImpliedQuote(IPriceableEquityAssetController[] priceableAssets,
                                                                  IEquityCurve equityCurve)
        {
            var result = new NamedValueSet();
            foreach (IPriceableEquityAssetController priceableAsset in priceableAssets)
            {
                result.Set(priceableAsset.Id, priceableAsset.CalculateImpliedQuote(equityCurve));
            }
            return result;
        }

        /// <summary>
        /// Builds this instance.
        /// </summary>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            PriceableEquityAssets = PriceableAssetFactory.CreatePriceableEquityAssets(logger, cache, nameSpace,
                GetEquityCurveValuation().baseDate.Value, GetEquityCurveValuation().spotRate, fixingCalendar, rollCalendar);
            TermCurve termCurve = ((FxCurveValuation)PricingStructureValuation).fxForwardCurve;
            termCurve.point = EquityBootstrapper.Bootstrap(PriceableEquityAssets, GetEquityCurveValuation().baseDate.Value, termCurve.extrapolationPermitted,
                                                                termCurve.interpolationMethod);
            SetFpmlData(new Pair<PricingStructure, PricingStructureValuation>(PricingStructure, PricingStructureValuation));
            SetInterpolator(GetEquityCurveValuation().baseDate.Value);
        }

        #endregion

        #region Value and Forwards

        /// <summary>
        /// DFs the value.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        public double ForwardIndex(DateTime baseDate, DateTime date)
        {
            IPoint point = new DateTimePoint1D(baseDate, date);
            return Interpolator.Value(point);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="valuationDate">The valuation date.</param>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetForward(DateTime valuationDate, DateTime targetDate)
        {
            double value = 0d;

            if (PricingStructureEvolutionType == PricingStructureEvolutionType.ForwardToSpot)
            {
                var point = new DateTimePoint1D(valuationDate, targetDate);
                value = Value(point);
                return value;
            }
            if (PricingStructureEvolutionType == PricingStructureEvolutionType.SpotToForward)
            {
                var point1 = new DateTimePoint1D(GetBaseDate(), targetDate);
                double value1 = Value(point1);
                var point2 = new DateTimePoint1D(GetBaseDate(), valuationDate);
                double value2 = Value(point2);
                value = value1 / value2;
                return value;
            }

            return value;
        }

        /// <summary>
        /// For any point, there should exist a function value. The point can be multi-dimensional.
        /// </summary>
        /// <param name="point"><c>IPoint</c> A point must have at least one dimension.
        /// <seealso cref="IPoint"/> The interface for a multi-dimensional point.</param>
        /// <returns>The <c>double</c> function value at the point</returns>
        public override double Value(IPoint point)
        {
            double result = Interpolator.Value(point);
            return result;
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetForward(DateTime targetDate)
        {
            return GetForward(GetEquityCurveValuation().baseDate.Value, targetDate);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetForward(DateTime[] targetDates)
        {
            return GetForward(GetEquityCurveValuation().baseDate.Value, targetDates);
        }

        /// <summary>
        /// Get array of discount factors
        /// </summary>
        /// <param name="baseDate"></param>
        /// <param name="targetDates">The target dates.</param>
        /// <returns></returns>
        public double[] GetForward(DateTime baseDate, DateTime[] targetDates)
        {
            return targetDates.Select(targetDate => GetForward(baseDate, targetDate)).ToArray();
        }

        #endregion

        #region Helpers and Interface Properties

        /// <summary>
        /// Gets the yield curve valuation.
        /// </summary>
        /// <returns></returns>
        public FxCurveValuation GetEquityCurveValuation()
        {
            return (FxCurveValuation)PricingStructureValuation;
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
            double[] times = GetDiscreteSpace().GetCoordinateArray(1);
            double[] values = GetDiscreteSpace().GetFunctionValueArray();
            int index = Array.BinarySearch(times, pt.Coords[1]);
            if (index >= 0)
            {
                return null;
            }
            int nextIndex = ~index;
            int prevIndex = nextIndex - 1;
            //TODO check for DateTime1D point and return the date.
            var nextValue = new DoubleValue("next", values[nextIndex], new Point1D(times[nextIndex]));
            var prevValue = new DoubleValue("prev", values[prevIndex], new Point1D(times[prevIndex]));
            IList<IValue> result = new List<IValue> { prevValue, nextValue };
            return result;
        }

        /// <summary>
        /// </summary>
        /// <param name="id">The curve id.</param>
        /// <param name="name">The name.</param>
        /// <param name="currency"></param>
        /// <returns></returns>
        private static Reporting.V5r3.FxCurve CreateEquityCurve(string id, string name, Currency currency)
        {
            var fxCurve = new Reporting.V5r3.FxCurve
            {
                id = id,
                name = name,
                currency = currency,
            };
            return fxCurve;
        }

        /// <summary>
        /// Creates the equity curve.
        /// </summary>
        /// <param name="curveId">The curve id.</param>
        /// <returns></returns>
        protected static Reporting.V5r3.FxCurve CreateEquityCurve(EquityCurveIdentifier curveId)
        {
            var fxCurve = new Reporting.V5r3.FxCurve
            {
                id = curveId.Id,
                name = curveId.CurveName,
                currency = curveId.Currency,
            };
            return fxCurve;
        }

        /// <summary>
        /// Creates the equity curve.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="equityId">THe underlying curve asset.</param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="fxRates">The spot rate.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="termCurve">The bootstrapped term curve.</param>
        /// <returns></returns>
        private static FxCurveValuation CreateEquityCurveValuation(ILogger logger, ICoreCache cache, string nameSpace, EquityCurveIdentifier equityId,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate, FxRateSet fxRates, string curveId,
                                                               TermCurve termCurve)
        {
            DateTime settlementDate = GetSettlementDate(logger, cache, nameSpace, equityId, fixingCalendar, rollCalendar, baseDate);
            var fxCurveValuation = new FxCurveValuation
                                       {
                                           baseDate = IdentifiedDateHelper.Create(baseDate),
                                           buildDateTime = baseDate,
                                           buildDateTimeSpecified = true,
                                           spotRate = fxRates,
                                           id = curveId,
                                           fxForwardCurve = termCurve,
                                           spotDate = IdentifiedDateHelper.Create("SettlementDate", settlementDate)
                                       };
            return fxCurveValuation;
        }

        /// <summary>
        /// </summary>
        /// <param name="fxRates">The spot rate.</param>
        /// <param name="curveId">The curve id.</param>
        /// <param name="termCurve">The bootstrapped term curve.</param>
        /// <returns></returns>
        private FxCurveValuation CreateEquityCurveValuation(EquityCurveIdentifier curveId, FxRateSet fxRates, 
                                                               TermCurve termCurve)
        {
            var fxCurveValuation = new FxCurveValuation
            {
                baseDate = IdentifiedDateHelper.Create(curveId.BaseDate),
                buildDateTime = curveId.BaseDate,
                buildDateTimeSpecified = true,
                spotRate = fxRates,
                id = curveId.UniqueIdentifier,
                fxForwardCurve = termCurve,
                spotDate = IdentifiedDateHelper.Create("SettlementDate", SettlementDate),
            };
            return fxCurveValuation;
        }

        /////<summary>
        /////</summary>
        /////<returns></returns>
        //public decimal GetSpotRate()
        //{
        //    var fxVal = (FxCurveValuation)GetFpMLData().Second;
        //    BasicAssetValuation spotRateAsset = (from spotRateAssets in fxVal.spotRate.assetQuote
        //                                         where spotRateAssets.objectReference.href.EndsWith("-Equity-SP", StringComparison.InvariantCultureIgnoreCase)//TODO FIX This!
        //                                         select spotRateAssets).Single();
        //    decimal spotRate = spotRateAsset.quote[0].value;        
        //    return spotRate;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="equityId">THe curve asset.</param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        protected static DateTime GetSettlementDate(ILogger logger, ICoreCache cache, string nameSpace,
            EquityCurveIdentifier equityId, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate)
        {
            BasicAssetValuation bav = BasicAssetValuationHelper.Create(BasicQuotationHelper.Create(0, "MarketQuote", PriceQuoteUnitsEnum.Price.ToString()));
            var assetId = BuildSpotAssetId(equityId);
            var priceableAsset = (IPriceableEquityAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, assetId, baseDate, bav, fixingCalendar, rollCalendar);
            return priceableAsset.GetRiskMaturityDate();
        }

        private static string BuildSpotAssetId(EquityCurveIdentifier equityId)
        {
            return equityId.Currency.Value + "-Equity-" + equityId.EquityAsset;
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(EquityCurveIdentifier curveId, TermCurve termCurve, IEnumerable<IPriceableEquityAssetController> priceableEquityAssets)
        {
            FxRateSet quotedAssetSet = priceableEquityAssets != null ? PriceableAssetFactory.Parse(priceableEquityAssets) : null;
            CreatePricingStructure(curveId, termCurve, quotedAssetSet);
        }

        /// <summary>
        /// Creates the pricing structure.
        /// </summary>
        protected void CreatePricingStructure(EquityCurveIdentifier curveId, TermCurve termCurve, FxRateSet quotedAssetSet)
        {
            Reporting.V5r3.FxCurve fxCurve = CreateEquityCurve(curveId);
            FxCurveValuation fxCurveValuation = CreateEquityCurveValuation(curveId, quotedAssetSet, termCurve);
            var fpmlData = new Pair<PricingStructure, PricingStructureValuation>(fxCurve, fxCurveValuation);
            SetFpMLData(fpmlData, false);
        }

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns></returns>
        public override QuotedAssetSet GetQuotedAssetSet()
        {
            return GetEquityCurveValuation().spotRate;
        }

        /// <summary>
        /// Gets the term curve.
        /// </summary>
        /// <returns>An array of term points</returns>
        public override TermCurve GetTermCurve()
        {
            return GetEquityCurveValuation().fxForwardCurve;
        }

        /// <summary>
        /// Not implemented yet.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="values"></param>
        /// <param name="measureType"></param>
        /// <returns></returns>
        public override bool PerturbCurve(ILogger logger, ICoreCache cache, string nameSpace, decimal[] values, string measureType)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates the basic rate curve risk set.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of perturbed rate curves</returns>
        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)//TODO make proportional perturbations!
        {
            if (PriceableEquityAssets == null) return null;
            //Set the parameters and properties.
            decimal perturbation = basisPointPerturbation / 10000.0m;
            NamedValueSet properties = GetPricingStructureId().Properties.Clone();
            properties.Set("PerturbedAmount", basisPointPerturbation);
            string uniqueId = GetPricingStructureId().UniqueIdentifier;
            //Get the assets, BUT REMOVE ALL THE BASIS SWAPS, to prevent double accounting of risks.
            int index = 0;
            var structures = new List<IPricingStructure>();
            //Get the original quotes
            var quotes = GetMarketQuotes(PriceableEquityAssets);
            foreach (var instrument in PriceableEquityAssets)
            {
                var perturbations = new decimal[quotes.Count];
                quotes.CopyTo(perturbations);
                //Clone the properties.
                NamedValueSet curveProperties = properties.Clone();
                perturbations[index] = quotes[index] + perturbation;
                curveProperties.Set("PerturbedAsset", instrument.Id);
                curveProperties.Set("BaseCurve", uniqueId);
                curveProperties.Set(CurveProp.UniqueIdentifier, uniqueId + "." + instrument.Id);
                curveProperties.Set(CurveProp.Tolerance, Tolerance);
                //Create the new curve.
                //Perturb the quotes
                PerturbedPriceableAssets(PriceableEquityAssets, perturbations);
                IPricingStructure fxCurve = new EquityCurve(curveProperties, PriceableEquityAssets, Holder);
                structures.Add(fxCurve);
                //Set the counter.
                perturbations[index] = 0;
                index++;
            }
            return structures;
        }

        /// <summary>
        /// Clones the curve.
        /// </summary>
        /// <returns></returns>
        public object Clone()//THis does not instantiate any priceable assets or bootstrap the curve. This means no evaluation of component assets.
        {
            //Clone the properties and set values.
            //
            NamedValueSet properties = GetProperties().Clone();
            string baseCurveId = properties.GetString(CurveProp.UniqueIdentifier, true);
            string id = baseCurveId + ".Clone";
            properties.Set(CurveProp.UniqueIdentifier, id);
            //Clone the curve data.
            //
            Pair<PricingStructure, PricingStructureValuation> fpml = GetFpMLData();
            var fxCurve = new FxCurve(null, null, null, CloneCurve(fpml, id), properties, null, null, false);
            return fxCurve;
        }

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
                PricingStructureIdentifier = new FxCurveIdentifier(PricingStructureTypeEnum.FxCurve, PricingStructure.id, baseDate);
            }
        }

        /// <summary>
        /// Gets the equity curve id.
        /// </summary>
        /// <returns></returns>
        public EquityCurveIdentifier GetEquityCurveId()
        {
            return (EquityCurveIdentifier)PricingStructureIdentifier;
        }

        private double GetDefaultTolerance(PricingStructureAlgorithmsHolder algorithms)
        {
            string tolerance = algorithms.GetValue(AlgorithmsProp.Tolerance);
            return double.Parse(tolerance);
        }

        private static Pair<PricingStructure, PricingStructureValuation> CloneCurve(Pair<PricingStructure, PricingStructureValuation> equityCurve, string curveId)
        {
            var eqCurveCloned = XmlSerializerHelper.Clone(equityCurve.First);
            var eqvCurveCloned = XmlSerializerHelper.Clone(equityCurve.Second);
            //  assign id to the cloned YieldCurve
            //
            var eq = (Reporting.V5r3.FxCurve)eqCurveCloned;
            eq.id = curveId;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            var eqv = (FxCurveValuation)eqvCurveCloned;
            //Don't want to null the dfs
            //
            //fxv.fxForwardCurve.point = null;
            return new Pair<PricingStructure, PricingStructureValuation>(eq, eqv);
        }

        //Clones a curve, maps the quoted assets specified and then returns an FpML structure back.
        //
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace">THe client namespace</param>
        /// <param name="referenceCurve"></param>
        /// <param name="spreadValues"></param>
        /// <param name="id"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="rollCalendar"></param>
        /// <param name="baseDate"></param>
        /// <returns></returns>
        public static Pair<PricingStructure, PricingStructureValuation> ProcessQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, IFxCurve referenceCurve,
            FxRateSet spreadValues, string id, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate)
        {
            //Clone the ref curves.
            //
            var clonedCurve = referenceCurve.Clone();
            var fpml = ((IFxCurve)clonedCurve).GetFpMLData();
            var fxCurveCloned = (Reporting.V5r3.FxCurve)fpml.First;
            var fxvCurveCloned = (FxCurveValuation)fpml.Second;
            //  assign id to the cloned YieldCurve
            //
            fxCurveCloned.id = id;
            //  nullify the discount factor curve to make sure that bootstrapping will happen)
            //
            fxvCurveCloned.fxForwardCurve.point = null;
            fxvCurveCloned.fxForwardPointsCurve = null;
            //Manipulate the quoted asset set.
            //
            fxvCurveCloned.spotRate = MappedQuotedAssetSet(logger, cache, nameSpace, referenceCurve, spreadValues, fixingCalendar, rollCalendar, baseDate);
            return fpml;
        }


        //Backs out the implied quotes for the asset provided and adds the spread to it.
        //
        private static FxRateSet MappedQuotedAssetSet(ILogger logger, ICoreCache cache, 
            string nameSpace, IInterpolatedSpace referenceCurve, FxRateSet spreadValues,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar, DateTime baseDate)
        {
            var index = 0;
            //Find the backed out implied quote for each asset.
            //
            foreach (var asset in spreadValues.instrumentSet.Items)//TODO aDd in the spread.
            {
                NamedValueSet namedValueSet = PriceableAssetFactory.BuildPropertiesForAssets(nameSpace, asset.id, baseDate);
                var quote = spreadValues.assetQuote[index];
                //Get the implied quote to use as the input market quote. Make sure it is rate controller.
                var priceableAsset = (PriceableFxAssetController)PriceableAssetFactory.Create(logger, cache, nameSpace, quote, namedValueSet, fixingCalendar, rollCalendar);
                var value = priceableAsset.CalculateImpliedQuote(referenceCurve);
                //Replace the market quote in the bav and remove the spread.
                var quotes = new List<BasicQuotation>(quote.quote);
                var impliedQuote = MarketQuoteHelper.ReplaceQuotationByMeasureType("MarketQuote", quotes, value);
                var marketQuote = new List<BasicQuotation>(impliedQuote);
                spreadValues.assetQuote[index].quote = MarketQuoteHelper.MarketQuoteRemoveSpreadAndNormalise(marketQuote);
                index++;
            }
            return spreadValues;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override IDictionary<string, decimal> GetInputs()
        {
            Pair<PricingStructure, PricingStructureValuation> fpml = GetFpMLData();
            var fxCurve = fpml.Second as FxCurveValuation;
            if (fxCurve == null)
            {
                throw new NotImplementedException("Invalid Fpml for FX Curve");
            }
            BasicAssetValuation[] inputs = fxCurve.spotRate.assetQuote;
            Asset[] instrumentSet = fxCurve.spotRate.instrumentSet.Items;
            IEnumerable<KeyValuePair<string, Decimal>> results
                        = from a in instrumentSet
                          join i in inputs on a.id equals i.objectReference.href
                          where i.quote[0].measureType.Value == "MarketQuote"
                          select new KeyValuePair<string, Decimal>(a.id, i.quote[0].value);

            IDictionary<string, Decimal> data = results.ToDictionary(i => i.Key, i => i.Value);
            return data;
        }

        public double GetEquityFactor(DateTime baseDate, DateTime targetDate)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        protected void PerturbedPriceableAssets(List<IPriceableEquityAssetController> priceableEquityAssets, Decimal[] values)
        {
            var numControllers = priceableEquityAssets.Count;
            if ((values.Length != numControllers)) return;//(PriceableRateAssets == null) || 
            var index = 0;
            foreach (var rateController in priceableEquityAssets)
            {
                rateController.MarketQuote.value = values[index];
                index++;
            }
        }

        protected List<decimal> GetMarketQuotes(IEnumerable<IPriceableEquityAssetController> priceableEquityAssets)
        {
            return priceableEquityAssets.Select(asset => asset.MarketQuote.value).ToList();
        }

        #endregion
    }
}
