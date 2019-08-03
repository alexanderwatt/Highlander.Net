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
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.CurveEngine.Factory;
using Orion.CurveEngine.Helpers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
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
    public class GenericVolatilityCurve : VolatilityCurve
    {
        #region Properties

        ///<summary>
        ///</summary>
        public string Handle { get; }

        #endregion

        #region NameValueSet-based constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="expiryTenorsWithVols">The expiry Tenors and the volatilities.</param>
        /// <param name="forecastCurve">The forecast rate curve.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        /// <param name="discountCurve">The discount rate curve.</param>
        [Description("public interface")]
        public GenericVolatilityCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, QuotedAssetSet expiryTenorsWithVols, 
            IRateCurve discountCurve, IRateCurve forecastCurve, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, new VolatilitySurfaceIdentifier(properties), fixingCalendar, rollCalendar)
        {
            DiscountCurve = discountCurve;
            ForecastCurve = forecastCurve;
            var curveId = GetCurveId();
            var volCurve = SetConfigurationData();
            if (properties != null)
            {
                var assetClass = properties.GetString(CurveProp.AssetClass, "Rates");
                var asset = EnumHelper.Parse<AssetClass>(assetClass);
                PricingStructureData = new PricingStructureData(CurveType.Parent, asset, properties);
                var quoteUnits = properties.GetValue(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString());
                IsFixedStrikeBootstrap = EnumHelper.Parse<StrikeQuoteUnitsEnum>(quoteUnits) == StrikeQuoteUnitsEnum.ATMFlatMoneyness;
                if (!IsFixedStrikeBootstrap) IsATMBootstrap = true;
                //Set the underlying asset information.
                var instrument = properties.GetString(CurveProp.Instrument, true);
                Asset = new AnyAssetReference { href = instrument };
                UnderlyingAssetDetails = CreateUnderlyingAssetWithProperties();
                QuoteUnits = EnumHelper.Parse<QuoteUnitsEnum>(properties.GetValue(CurveProp.QuoteUnits, QuoteUnitsEnum.LogNormalVolatility.ToString()));
                MeasureType = EnumHelper.Parse<MeasureTypesEnum>(properties.GetValue(CurveProp.MeasureType, MeasureTypesEnum.Volatility.ToString()));
                StrikeQuoteUnits = EnumHelper.Parse<StrikeQuoteUnitsEnum>(properties.GetValue(CurveProp.StrikeQuoteUnits, StrikeQuoteUnitsEnum.ATMFlatMoneyness.ToString()));
                IsFixedStrikeBootstrap = true;
                IsATMBootstrap = false;
                if (StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMFlatMoneyness || StrikeQuoteUnits == StrikeQuoteUnitsEnum.ATMMoneyness)
                {
                    IsFixedStrikeBootstrap = false;
                    IsATMBootstrap = true;
                }
                Handle = properties.GetString(CurveProp.EngineHandle, null);
            }
            //If there is a strike specified for the curve, use it!
            Strike = curveId.Strike;
            if (Strike != null)
            {
                ValidateStrike((decimal)Strike);
                IsFixedStrikeBootstrap = true;
                IsATMBootstrap = false;
            }
            BootstrapResults = new VolCurveBootstrapResults();
            PriceableOptionAssets = PriceableAssetFactory.CreatePriceableOptionAssets(logger, cache, nameSpace, curveId.BaseDate, Asset?.href, discountCurve, forecastCurve, Strike, expiryTenorsWithVols, fixingCalendar, rollCalendar);
            BootstrapResults.Results = VolatilityCurveBootstrapper.Bootstrap(PriceableOptionAssets);
            IsBootstrapSuccessful = true;
            InitialiseVolatilities(curveId.BaseDate, BootstrapResults.Results);          
            //The points use tenor strings for expiry.
            var termTenors = new List<String>();
            foreach (var daysDifference in VolatilityOffsets)
            {
                termTenors.Add(daysDifference + "D");
            }
            volCurve.point  = ProcessRawSurface(termTenors, Strike, VolatilityValues, curveId.StrikeQuoteUnits, curveId.UnderlyingAssetReference);
            // Interpolate the curve based on the respective curve interpolation 
            SetInterpolator(volCurve, curveId.Algorithm, curveId.PricingStructureType);
            CreatePricingStructure(curveId, volCurve, expiryTenorsWithVols);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericVolatilityCurve"/> class.
        /// This constructor is used to clone perturbed copies of a base curve.
        /// </summary>
        /// <param name="priceableRateOptionAssets">The priceableRateAssets.</param>
        /// <param name="pricingStructureAlgorithmsHolder">The pricingStructureAlgorithmsHolder.</param>
        /// <param name="curveProperties">The Curve Properties.</param>
        /// <param name="discountCurve">The discount rate curve.</param> 
        /// <param name="forecastCurve">The forecast rate curve.</param> 
        public GenericVolatilityCurve(NamedValueSet curveProperties, List<IPriceableOptionAssetController> priceableRateOptionAssets,
            IRateCurve discountCurve, IRateCurve forecastCurve, PricingStructureAlgorithmsHolder pricingStructureAlgorithmsHolder)
            : base(curveProperties, pricingStructureAlgorithmsHolder)
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
            BootstrapResults.Results = VolatilityCurveBootstrapper.Bootstrap(PriceableOptionAssets);
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
        /// Initializes a new instance of the <see cref="GenericVolatilityCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The nameSpace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties for the pricing structure.</param>
        /// <param name="fixingCalendar">The fixingCalendar. If the curve is already bootstrapped, then this can be null.</param>
        /// <param name="rollCalendar">The rollCalendar. If the curve is already bootstrapped, then this can be null.</param>
        public GenericVolatilityCurve(ILogger logger, ICoreCache cache, String nameSpace, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, NamedValueSet properties,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar)
        {
            PricingStructureData = new PricingStructureData(CurveType.Parent, AssetClass.Rates, properties);
            var curveId = GetCurveId();
            Initialize(properties, Holder);
            //Set the underlying asset information.
            if (properties != null)
            {
                var instrument = properties.GetString("Instrument", false);
                Asset = new AnyAssetReference { href = instrument };
            }
            if (fpmlData == null) return;
            FixingCalendar = fixingCalendar;
            PaymentCalendar = rollCalendar;
            if (DayCounter == null)
            {
                DayCounter = DayCounterHelper.Parse("ACT/365.FIXED");
                if (Holder != null)
                {
                    string dayCountBasis = Holder.GetValue("DayCounter");
                    DayCounter = DayCounterHelper.Parse(dayCountBasis);
                }
            }
            // the curve is already built, so don't rebuild
            //Process the matrix
            SetInterpolator(((VolatilityMatrix)PricingStructureValuation).dataPoints, curveId.Algorithm, curveId.PricingStructureType);
            SetFpMLData(fpmlData, false);
        }

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
            var volatilityCurve = new GenericVolatilityCurve(curveProperties, PriceableOptionAssets, DiscountCurve, ForecastCurve, Holder);
            return volatilityCurve;
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
            var priceableOptionAssets = new IPriceableOptionAssetController[PriceableOptionAssets.Count];
            PriceableOptionAssets.CopyTo(priceableOptionAssets);
            foreach (var instrument in priceableOptionAssets)
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
                PerturbedPriceableAssets(priceableOptionAssets.ToList(), perturbations);
                IPricingStructure rateCurve = new GenericVolatilityCurve(curveProperties, priceableOptionAssets.ToList(), DiscountCurve, ForecastCurve, Holder);
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
            //TODO This for backwards compatability. AnalyticalResults is the current interface.
            BootstrapResults = new VolCurveBootstrapResults();
            PriceableOptionAssets = PriceableAssetFactory.CreatePriceableOptionAssets(logger, cache, nameSpace, GetVolatilityMatrix().baseDate.Value, Asset?.href, DiscountCurve, ForecastCurve, Strike, GetVolatilityMatrix().inputs, fixingCalendar, rollCalendar);            
            //GetVolatilityMatrix().dataPoints = null;
            MultiDimensionalPricingData dataPoints = GetVolatilityMatrix().dataPoints;
            BootstrapResults.Results = VolatilityCurveBootstrapper.Bootstrap(PriceableOptionAssets);
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
