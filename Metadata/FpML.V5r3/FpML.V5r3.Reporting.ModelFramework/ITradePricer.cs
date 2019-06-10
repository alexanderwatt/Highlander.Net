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
using FpML.V5r3.Reporting.Helpers;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using FpML.V5r3.Codes;
using Orion.Constants;
using Orion.ModelFramework.Instruments;
using Orion.ModelFramework.Reports;

#endregion

namespace Orion.ModelFramework
{
    ///<summary>
    ///</summary>
    public interface ITradePricer
    {
        ///<summary>
        /// IsCollateralised.
        ///</summary>
        ///<returns></returns>
        bool IsCollateralised { get; }

        ///<summary>
        /// Gets the underlying product type of the trade.
        ///</summary>
        ///<returns></returns>
        ProductTypeSimpleEnum GetTradeType();

        ///<summary>
        /// Returns the relevant productPricer.
        ///</summary>
        ///<returns></returns>
        InstrumentControllerBase GetPriceableProduct();

        ///<summary>
        /// Returns the relevant trade id.
        ///</summary>
        ///<returns></returns>
        IIdentifier GetTradeIdentifier();

        /// <summary>
        /// Prices the trade.
        /// </summary>
        /// <param name="modelData"></param>
        /// <param name="reportType"></param>
        /// <returns></returns>
        List<ValuationReport> Price(List<IInstrumentControllerData> modelData, ValuationReportType reportType);

        ///<summary>
        /// Prices the trade.
        ///</summary>
        ///<returns></returns>
        ValuationReport Price(IInstrumentControllerData modelData,
                              ValuationReportType reportType);
    }

    ///<summary>
    ///</summary>
    public abstract class TradePricerBase : ReporterBase, ITradePricer, IProduct
    {
        ///<summary>
        /// IsCollateralised.
        ///</summary>
        ///<returns></returns>
        public bool IsCollateralised { get; protected set; }

        ///<summary>
        /// ProductType.
        ///</summary>
        ///<returns></returns>
        public ItemChoiceType15 TradeType { get; protected set; }

        ///<summary>
        /// ProductType.
        ///</summary>
        ///<returns></returns>
        public ProductTypeSimpleEnum ProductType { get; protected set; }

        /// <summary>
        /// The property for generating reports
        /// </summary>
        public ReporterBase ProductReporter { get; protected set; }

        ///<summary>
        /// The underlying product pricer.
        ///</summary>
        public InstrumentControllerBase PriceableProduct{ get; protected set; }

        ///<summary>
        /// The underlying trade headers.
        ///</summary>
        public TradeHeader TradeHeader { get; protected set; }

        /// <summary>
        /// The Parties
        /// </summary>
        public List<Party> Parties { get; protected set; }
        
        /// <summary>
        /// The base Parties
        /// </summary>
        public String BaseParty { get; protected set; }

        ///<summary>
        /// The trade id of the trade.
        ///</summary>
        public IIdentifier TradeIdentifier { get; protected set; }

        ///<summary>
        /// The underlying product properties.
        ///</summary>
        public NamedValueSet TradeProperties => TradeIdentifier.Properties;

        #region Implementation of ITradePricer

        ///<summary>
        /// Gets the underlying product type of the trade.
        ///</summary>
        ///<returns></returns>
        public ProductTypeSimpleEnum GetTradeType()
        {
            return ProductType;
        }

        ///<summary>
        /// Returns the relevant productPricer.
        ///</summary>
        ///<returns></returns>
        public InstrumentControllerBase GetPriceableProduct()
        {
            return PriceableProduct;
        }

        ///<summary>
        /// Returns the relevant trade id.
        ///</summary>
        ///<returns></returns>
        public IIdentifier GetTradeIdentifier()
        {
            return TradeIdentifier;
        }

        ///<summary>
        /// Prices the trade.
        ///</summary>
        ///<returns></returns>
        public abstract ValuationReport Price(IInstrumentControllerData modelData,
                                              ValuationReportType reportType);

        /// <summary>
        /// Prices the trade.
        /// </summary>
        /// <param name="modelData"></param>
        /// <param name="reportType"></param>
        /// <returns></returns>
        public abstract List<ValuationReport> Price(List<IInstrumentControllerData> modelData, ValuationReportType reportType);

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public abstract Product BuildTheProduct();

        #endregion

        protected static IInstrumentControllerData CreateInstrumentModelData(string[] metrics, DateTime valuationDate, IMarketEnvironment market, string reportingCurrency, IIdentifier baseCounterParty)
        {
            var bav = new AssetValuation();
            var currency = CurrencyHelper.Parse(reportingCurrency);

            var quotes = new Quotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = QuotationHelper.Create(0.0m, metric);
                index++;
            }
            bav.quote = quotes;
            return new InstrumentControllerData(bav, market, valuationDate, currency, baseCounterParty);
        }
    }
}