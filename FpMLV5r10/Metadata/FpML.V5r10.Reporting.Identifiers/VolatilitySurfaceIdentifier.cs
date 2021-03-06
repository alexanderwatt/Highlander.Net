﻿/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Constants;
using Orion.Util.NamedValues;
using FpML.V5r10.Reporting;

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// Volatility Id.
    /// </summary>
    public class VolatilitySurfaceIdentifier : PricingStructureIdentifier
    {
        ///<summary>
        /// The underlying instrument the volatility describes.
        ///</summary>
        public string Instrument { get; set; }

        /// <summary>
        /// The Name - read only
        /// </summary>
        public string Name => $"{Currency.Value}.{Instrument}";

        /// <summary>
        /// Gets the reference date.
        /// </summary>
        /// <value>The reference date.</value>
        public DateTime ReferenceDate { get; set; }//TODO make this redundant

        /// <summary>
        /// Gets the Business DayAdjustments.
        /// </summary>
        /// <value>The Business DayAdjustments.</value>
        public BusinessCenter BusinessCenter { get; set; }

        /// <summary>
        /// Gets the ExpiryTime.
        /// </summary>
        /// <value>The ExpiryTime.</value>
        public DateTime? ExpiryTime { get; set; }

        /// <summary>
        /// Gets the strike.
        /// </summary>
        /// <value>The strike.</value>
        public Decimal? Strike { get; set; }

        /// <summary>
        /// Gets the Time.
        /// </summary>
        /// <value>The Time.</value>
        public DateTime? Time { get; set; }

        /// <summary>
        /// Gets the ValuationDate.
        /// </summary>
        /// <value>The ValuationDate.</value>
        public DateTime? ValuationDate { get; set; }

        /// <summary>
        /// Gets the QuotationSideEnum.
        /// </summary>
        /// <value>The QuotationSideEnum.</value>
        public QuotationSideEnum? QuotationSide { get; set; }

        /// <summary>
        /// Gets the QuoteTiming.
        /// </summary>
        /// <value>The QuoteTiming.</value>
        public QuoteTiming QuoteTiming { get; set; }

        /// <summary>
        /// Gets the AssetMeasureType.
        /// </summary>
        /// <value>The AssetMeasureType.</value>
        public AssetMeasureType MeasureType { get; set; }

        /// <summary>
        /// Gets the PriceQuoteUnits.
        /// </summary>
        /// <value>The PriceQuoteUnits.</value>
        public PriceQuoteUnits QuoteUnits { get; set; }

        /// <summary>
        /// Gets the strike PriceQuoteUnits.
        /// </summary>
        /// <value>The strike PriceQuoteUnits.</value>
        public PriceQuoteUnits StrikeQuoteUnits { get; set; }

        /// <summary>
        /// Gets the CashflowType.
        /// </summary>
        /// <value>The CashflowType.</value>
        public CashflowType CashflowType { get; set; }

        /// <summary>
        /// Gets the QuotationSideEnum.
        /// </summary>
        /// <value>The QuotationSideEnum.</value>
        public QuotationSideEnum? Side{ get; set; }
        
        /// <summary>
        /// Gets the InformationSources.
        /// </summary>
        /// <value>The InformationSources.</value>
        public InformationSource[] InformationSources { get; set; }

        ///<summary>
        /// The underlying insgtrument the cvolatility describes.
        ///</summary>
        public AssetReference UnderlyingAssetReference { get; set; }

        ///<summary>
        /// The frequency
        ///</summary>
        public string CapFrequency { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilitySurfaceIdentifier"/> class.
        /// </summary>
        /// <param name="pricingStructureType"></param>
        /// <param name="curveName"></param>
        /// <param name="baseDate"></param>
        /// <param name="algorithm"></param>
        public VolatilitySurfaceIdentifier(PricingStructureTypeEnum pricingStructureType, string curveName, DateTime baseDate, string algorithm)
            : base(pricingStructureType, curveName, baseDate, algorithm)
        {
            var volCurveId = CurveName.Split('-');
            //Currency = CurrencyHelper.Parse(volCurveId[0]);
            Instrument = volCurveId[volCurveId.Length - 2];
            UnderlyingAssetReference = new AssetReference { href = Instrument };
            StrikeQuoteUnits = new PriceQuoteUnits { Value = "Absolute" };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilitySurfaceIdentifier"/> class.
        /// </summary>
        /// <param name="surfaceId">The surface id.</param>
        public VolatilitySurfaceIdentifier(string surfaceId)
            : base(surfaceId)
        {
            var volCurveId = surfaceId.Split('-');
            Instrument = volCurveId[volCurveId.Length - 2];
            UnderlyingAssetReference = new AssetReference { href = Instrument };
            StrikeQuoteUnits = new PriceQuoteUnits { Value = "Absolute" };
        }

        ///<summary>
        /// An id for a vol curve.
        ///</summary>
        ///<param name="properties">The properties. These need to be:
        /// PricingStructureType, CurveName and BuildDate.</param>
        public VolatilitySurfaceIdentifier(NamedValueSet properties)
            : base(properties)
        {
            SetProperties();
        }

        private void SetProperties()
        {
            Instrument = PropertyHelper.ExtractInstrument(Properties);
            BusinessCenter = PropertyHelper.ExtractBusinessCenter(Properties);
            ExpiryTime = PropertyHelper.ExtractExpiryTime(Properties);
            Time = PropertyHelper.ExtractTime(Properties);
            ValuationDate = PropertyHelper.ExtractValuationDate(Properties);
            QuotationSide = PropertyHelper.ExtractQuotationSide(Properties);
            QuoteTiming = PropertyHelper.ExtractQuoteTiming(Properties);
            MeasureType = PropertyHelper.ExtractMeasureType(Properties);
            QuoteUnits = PropertyHelper.ExtractQuoteUnits(Properties);
            CashflowType = PropertyHelper.ExtractCashflowType(Properties);
            InformationSources = PropertyHelper.ExtractInformationSources(Properties);
            StrikeQuoteUnits = PropertyHelper.ExtractStrikeQuoteUnits(Properties);
            UnderlyingAssetReference = new AssetReference { href = Instrument };
            CapFrequency = PropertyHelper.ExtractCapFrequency(Properties);
            Strike = PropertyHelper.ExtractStrike(Properties);
        }
    }
}