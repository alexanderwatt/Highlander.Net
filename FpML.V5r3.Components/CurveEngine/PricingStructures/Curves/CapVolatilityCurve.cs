/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

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
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Analytics.Options;
using Orion.CurveEngine.PricingStructures.Bootstrappers;
using Orion.Identifiers;
using Orion.ModelFramework;
using Orion.ModelFramework.Assets;
using Orion.ModelFramework.PricingStructures;
using AssetClass = Orion.Constants.AssetClass;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{

    /// <summary>
    /// The advanced ratecurve class.
    /// </summary>
    public class CapVolatilityCurve : VolatilityCurve
    {
        #region Properties

        ///<summary>
        ///</summary>
        public string Handle { get; }

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties">The properties.</param>
        /// <param name="algorithmHolder">The algorithmHolder.</param>
        public CapVolatilityCurve(NamedValueSet properties, PricingStructureAlgorithmsHolder algorithmHolder):base(properties, algorithmHolder)
        {
            BootstrapperName = "CapFloorBootstrapper";
            UnderlyingInterpolatedCurve = "CapVolatilityCurve";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        protected CapVolatilityCurve(ILogger logger, ICoreCache cache, String nameSpace, PricingStructureIdentifier curveIdentifier)
            : base(logger, cache, nameSpace, curveIdentifier)
        {
            BootstrapperName = "CapFloorBootstrapper";
            UnderlyingInterpolatedCurve = "CapVolatilityCurve";
            var properties = curveIdentifier.GetProperties();
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties); 
            Tolerance = PropertyHelper.ExtractDoubleProperty("Tolerance", properties) ?? GetDefaultTolerance(Holder);
            var compoundingFrequency = properties.GetString("CompoundingFrequency", null);
            CompoundingFrequency = compoundingFrequency != null
                                       ? EnumHelper.Parse<CompoundingFrequencyEnum>(compoundingFrequency)
                                       : GetDefaultCompounding(Holder);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="curveIdentifier">The curveIdentifier.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public CapVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace,
            VolatilitySurfaceIdentifier curveIdentifier, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, curveIdentifier, fixingCalendar, rollCalendar)
        {
            BootstrapperName = "CapFloorBootstrapper";
            UnderlyingInterpolatedCurve = "CapVolatilityCurve";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instrumentData">The instrument data.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="discountCurve">The discount rate curve.</param>
        [Description("public interface")]
        public CapVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, QuotedAssetSet instrumentData, 
            IRateCurve discountCurve, IRateCurve forecastCurve, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : this(logger, cache, nameSpace, new VolatilitySurfaceIdentifier(properties), fixingCalendar, rollCalendar)
        {
            DiscountCurve = discountCurve;
            ForecastCurve = forecastCurve;
            var curveId = GetCurveId();
            var volCurve = SetConfigurationData();
            //TODO This for backwards compatibility. AnalyticalResults is the current interface.
            Handle = properties.GetString(CurveProp.EngineHandle, null);
            BootstrapResults = new VolCurveBootstrapResults();
            //Set the underlying asset information.
            var instrument = properties.GetString(CurveProp.Instrument, true);
            Asset = new AnyAssetReference {href = instrument};
            UnderlyingAssetDetails = CreateUnderlyingAssetWithProperties();
            var extrapolationPermitted = ExtrapolationPermittedInput ?? ExtrapolationPermitted;
            PriceableOptionAssets = PriceableAssetFactory.CreatePriceableRateOptionAssets(logger, cache, nameSpace, curveId.BaseDate, instrumentData, fixingCalendar, rollCalendar);
            BootstrapResults.Results = CapFloorBootstrapper.Bootstrap(PriceableOptionAssets, properties, DiscountCurve, ForecastCurve, curveId.BaseDate, extrapolationPermitted,
                InterpolationMethod, Tolerance);
            IsBootstrapSuccessful = true;
            InitialiseVolatilities(curveId.BaseDate, BootstrapResults.Results);
            QuoteUnits = EnumHelper.Parse<QuoteUnitsEnum>(properties.GetValue(CurveProp.QuoteUnits, QuoteUnitsEnum.LogNormalVolatility.ToString()));
            MeasureType = EnumHelper.Parse<MeasureTypesEnum>(properties.GetValue(CurveProp.MeasureType, MeasureTypesEnum.Volatility.ToString()));
            StrikeQuoteUnits = EnumHelper.Parse<StrikeQuoteUnitsEnum>(properties.GetValue(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()));
            //If there is a strike specified for the curve, use it!
            IsATMBootstrap = true;
            Strike = curveId.Strike;
            if (Strike != null)
            {
                ValidateStrike((decimal)Strike);
                IsFixedStrikeBootstrap = true;
                IsATMBootstrap = false;
            }
            if (StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMFlatMoneyness || StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMMoneyness)
            {               
                IsFixedStrikeBootstrap = false;
            }
            //The points use tenor strings for expiry.
            var termTenors = new List<String>();
            foreach (var daysDifference in VolatilityOffsets)
            {
                termTenors.Add(daysDifference + "D");
            }
            volCurve.point  = ProcessRawSurface(termTenors, Strike, VolatilityValues, curveId.StrikeQuoteUnits, curveId.UnderlyingAssetReference);
            // Interpolate the curve based on the respective curve interpolation 
            SetInterpolator(volCurve, curveId.Algorithm, curveId.PricingStructureType);
            CreatePricingStructure(curveId, volCurve, instrumentData);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilityCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableRateOptionAssets">The priceableRateAssets.</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        /// <param name="discountCurve">The discount rate curve.</param> 
        /// <param name="forecastCurve">The forecast rate curve.</param> 
        public CapVolatilityCurve(NamedValueSet curveProperties, List<IPriceableOptionAssetController> priceableRateOptionAssets,
            IRateCurve discountCurve, IRateCurve forecastCurve, PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : this(curveProperties, pricingStructureAlgorithmsHolder)
        {
            DiscountCurve = discountCurve;
            ForecastCurve = forecastCurve;
            var curveId = GetCurveId();
            var volCurve = SetConfigurationData();
            Handle = curveProperties.GetString(CurveProp.EngineHandle, null);
            BootstrapResults = new VolCurveBootstrapResults();
            //Set the underlying asset information.
            var instrument = curveProperties.GetString(CurveProp.Instrument, true);
            Asset = new AnyAssetReference { href = instrument };
            UnderlyingAssetDetails = CreateUnderlyingAssetWithProperties();
            PriceableOptionAssets = priceableRateOptionAssets;
            var extrapolationPermitted = ExtrapolationPermittedInput ?? ExtrapolationPermitted;
            BootstrapResults.Results = CapFloorBootstrapper.Bootstrap(PriceableOptionAssets, curveProperties, DiscountCurve, ForecastCurve, curveId.BaseDate, extrapolationPermitted,
                InterpolationMethod, Tolerance);
            IsBootstrapSuccessful = true;
            InitialiseVolatilities(curveId.BaseDate, BootstrapResults.Results);
            QuoteUnits = EnumHelper.Parse<QuoteUnitsEnum>(curveProperties.GetValue(CurveProp.QuoteUnits, QuoteUnitsEnum.LogNormalVolatility.ToString()));
            MeasureType = EnumHelper.Parse<MeasureTypesEnum>(curveProperties.GetValue(CurveProp.MeasureType, MeasureTypesEnum.Volatility.ToString()));
            StrikeQuoteUnits = EnumHelper.Parse<StrikeQuoteUnitsEnum>(curveProperties.GetValue(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()));
            //If there is a strike specified for the curve, use it!
            Strike = curveId.Strike;
            if (Strike != null)
            {
                ValidateStrike((decimal)Strike);
                IsFixedStrikeBootstrap = true;
            }
            if (StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMFlatMoneyness || StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMMoneyness)
            {
                IsATMBootstrap = true;
                IsFixedStrikeBootstrap = false;
            }
            //The points use tenor strings for expiry.
            var termTenors = new List<String>();
            foreach (var daysDifference in VolatilityOffsets)
            {
                termTenors.Add(daysDifference + "D");
            }
            volCurve.point = ProcessRawSurface(termTenors, Strike, VolatilityValues, curveId.StrikeQuoteUnits, curveId.UnderlyingAssetReference);
            // Interpolate the curve based on the respective curve interpolation 
            SetInterpolator(volCurve, curveId.Algorithm, curveId.PricingStructureType);
            CreatePricingStructure(curveId, volCurve, PriceableOptionAssets);
        }

        #endregion

        #region Runtime Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CapVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing structure.</param>
        /// <param name="fixingCalendar">The fixingCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        public CapVolatilityCurve(ILogger logger, ICoreCache cache, String nameSpace, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar)
        {}

        #endregion

        #region Light Perturbation Without Cache or BusinessCalendars

        /// <summary>
        /// Generates a perturbed curve for those items specified.
        /// If the instruments are not valid they are excluded.
        /// </summary>
        /// <param name="perturbationArray">The perturbation Array: instrumentId and value.</param>
        /// <returns></returns>
        public IPricingStructure PerturbCurve(List<Pair<string, decimal>> perturbationArray)
        {
            if (PriceableOptionAssets == null) return null;
            //Set the parameters and properties.
            NamedValueSet properties = GetPricingStructureId().Properties.Clone();
            string uniqueId = GetPricingStructureId().UniqueIdentifier;
            //Clone the properties.
            NamedValueSet curveProperties = properties.Clone();
            curveProperties.Set("PerturbedCurve", true);
            curveProperties.Set("BaseCurve", uniqueId);
            curveProperties.Set(CurveProp.UniqueIdentifier, uniqueId + "." + "PerturbedCurve");
            foreach (var instrument in perturbationArray)
            {
                //TODO clone the priceable assets.
                var asset = PriceableOptionAssets.FindAll(a => a.Id.Equals(instrument.First));
                if (asset[0] == null) continue;
                var temp = asset[0];
                temp.MarketQuote.value = temp.MarketQuote.value + instrument.Second / 10000;
            }
            //Create the new curve.
            var capVolatilityCurve = new CapVolatilityCurve(curveProperties, PriceableOptionAssets, DiscountCurve, ForecastCurve, Holder);
            return capVolatilityCurve;
        }

        /// <summary>
        /// Creates the basic rate curve risk set, using the current curve as the base curve.
        /// This function takes a curves, creates a rate curve for each instrument and applying 
        /// supplied basis point perturbation/spread to the underlying instrument in the spread curve
        /// </summary>
        /// <param name="basisPointPerturbation">The basis point perturbation.</param>
        /// <returns>A list of perturbed rate curves</returns>
        public override List<IPricingStructure> CreateCurveRiskSet(decimal basisPointPerturbation)
        {
            if (PriceableOptionAssets == null) return null;
            //Set the parameters and properties.
            decimal perturbation = basisPointPerturbation / 10000.0m;
            NamedValueSet properties = GetPricingStructureId().Properties.Clone();
            properties.Set("PerturbedAmount", basisPointPerturbation);
            string uniqueId = GetPricingStructureId().UniqueIdentifier;
            //Get the assets
            int index = 0;
            var structures = new List<IPricingStructure>();
            //Get the original quotes
            var quotes = GetMarketQuotes(PriceableOptionAssets);
            //Copy the assets.
            var priceableRateOptionAssets = new IPriceableOptionAssetController[PriceableOptionAssets.Count];
            PriceableOptionAssets.CopyTo(priceableRateOptionAssets);
            foreach (var instrument in priceableRateOptionAssets)
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
                //Perturb the quotes
                PerturbedPriceableAssets(priceableRateOptionAssets.ToList(), perturbations);
                IPricingStructure rateCurve = new CapVolatilityCurve(curveProperties, priceableRateOptionAssets.ToList(), DiscountCurve, ForecastCurve, Holder);
                structures.Add(rateCurve);
                //Set the counter.
                perturbations[index] = 0;
                index++;
            }
            return structures;
        }

        #endregion

        #region Build and Clone

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        ///  <param name="nameSpace">The client namespace</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <returns></returns>
        public override void Build(ILogger logger, ICoreCache cache, string nameSpace, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            var curveId = (VolatilitySurfaceIdentifier)PricingStructureIdentifier;
            //TODO This for backwards compatibility. AnalyticalResults is the current interface.
            BootstrapResults = new VolCurveBootstrapResults();
            PriceableOptionAssets = PriceableAssetFactory.CreatePriceableRateOptionAssets(logger, cache, nameSpace, GetVolatilityMatrix().baseDate.Value, GetVolatilityMatrix().inputs, fixingCalendar, rollCalendar);            
            //GetVolatilityMatrix().dataPoints = null;
            MultiDimensionalPricingData dataPoints = GetVolatilityMatrix().dataPoints;
            DateTime baseDate = GetVolatilityMatrix().baseDate.Value;
            var interpolationMethod = InterpolationMethod;
            var extrapolationPermitted = ExtrapolationPermittedInput ?? ExtrapolationPermitted;
            BootstrapResults.Results = CapFloorBootstrapper.Bootstrap(PriceableOptionAssets, curveId.Properties, DiscountCurve, ForecastCurve, baseDate, extrapolationPermitted, interpolationMethod, Tolerance);
            IsBootstrapSuccessful = true;
            InitialiseVolatilities(curveId.BaseDate, BootstrapResults.Results);
            //The points use tenor strings for expiry.
            var termTenors = new List<String>();
            foreach (var daysDifference in VolatilityOffsets)
            {
                termTenors.Add(daysDifference + "D");
            }
            dataPoints.point = ProcessRawSurface(termTenors, Strike, VolatilityValues, curveId.StrikeQuoteUnits, curveId.UnderlyingAssetReference);
            SetInterpolator(dataPoints, curveId.Algorithm, curveId.PricingStructureType);
        }

        #endregion     
    }
}
