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

using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Highlander.Orc.Messages
{
    #region TheoreticalCalculationMessage

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TheoreticalCalculationMessage : SendMessage
    {
        public TheoreticalCalculationMessage()
            : base(MessageTypes.THEORETICAL_CALCULATION)
        {
        }

        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }

        [XmlElement(ElementName = "analysis_date")]
        public string AnalysisDate { get; set; }

        [XmlElement(ElementName = "async")]
        public bool Async { get; set; }

        [XmlElement(ElementName = "basecontract")]
        public InstrumentID BaseContract { get; set; }

        [XmlElement(ElementName = "base_ytm_offset")]
        public string BaseYtmOffset { get; set; }

        [XmlElement(ElementName = "contract_price")]
        public string ContractPrice { get; set; }

        [XmlElement(ElementName = "dividendcontract")]
        public InstrumentID DividendContract { get; set; }

        [XmlElement(ElementName = "dividend_percentage")]
        public string DividendPercentage { get; set; }

        [XmlElement(ElementName = "feed")]
        public bool Feed { get; set; }

        [XmlElement(ElementName = "feed_minimum_hold")]
        public string FeedMinimumHold { get; set; }

        [XmlElement(ElementName = "financial_rate_offset")]
        public string FinancialRateOffset { get; set; }

        [XmlElement(ElementName = "portfolio")]
        public string Portfolio { get; set; }

        [XmlElement(ElementName = "portfolio_accrued")]
        public string PortfolioAccrued { get; set; }

        [XmlElement(ElementName = "portfolio_currency")]
        public string PortfolioCurrency { get; set; }

        [XmlElement(ElementName = "portfolio_invested")]
        public string PortfolioInvested { get; set; }

        [XmlElement(ElementName = "_portfolio_volume")]
        public string PortfolioVolume { get; set; }

        [XmlElement(ElementName = "result_currency")]
        public string ResultCurrency { get; set; }

        [XmlElement(ElementName = "simulated_model")]
        public string SimulatedModel { get; set; }

        [XmlElement(ElementName = "simulated_price")]
        public string SimulatedPrice { get; set; }

        [XmlElement(ElementName = "simulated_price")]
        public string SimulatedPriceMode { get; set; }

        [XmlElement(ElementName = "simulated_price_currency")]
        public string SimulatedPriceCurrency { get; set; }

        [XmlElement(ElementName = "simulated_strikeprice")]
        public string SimulatedStrikePrice { get; set; }

        [XmlElement(ElementName = "simulated_strikeprice_mode")]
        public string SimulatedStrikePriceMode { get; set; }

        [XmlElement(ElementName = "simulated_volatility")]
        public string SimulatedVolatility { get; set; }

        [XmlElement(ElementName = "simulated_volatility_mode")]
        public string SimulatedVolatilityMode { get; set; }

        [XmlElement(ElementName = "underlying")]
        public string Underlying { get; set; }

        [XmlElement(ElementName = "user_percentage")]
        public string UserPercentage { get; set; }

        [XmlElement(ElementName = "volatility_surface")]
        public string VolatilitySurface { get; set; }

        [XmlElement(ElementName = "yield_name")]
        public string YieldName { get; set; }
    }

    #endregion

    #region TheoreticalCalculationReply

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TheoreticalCalculationReply : ReplyMessage
    {
        public TheoreticalCalculationReply()
            : base(MessageTypes.THEORETICAL_CALCULATION)
        {
        }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }

        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        [XmlElement(ElementName = "underlying")]
        public string Underlying { get; set; }

        [XmlElement(ElementName = "yield_name")]
        public string YieldName { get; set; }

        [XmlElement(ElementName = "result_currency")]
        public string ResultCurrency { get; set; }

        [XmlElement(ElementName = "result")]
        public string Result { get; set; }

        [XmlElement(ElementName = "feedtag")]
        public string FeedTag { get; set; }
    }

    #endregion

    #region TheoreticalCalculationFeedStopMessage

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TheoreticalCalculationFeedStopMessage : SendMessage
    {
        public TheoreticalCalculationFeedStopMessage()
            : base(MessageTypes.THEORETICAL_CALCULATION_FEED_STOP)
        {
        }

        [XmlElement(ElementName = "feed_tag")]
        public string FeedTag { get; set; }
    }

    #endregion

    #region TheoreticalCalculationGroupMessage

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TheoreticalCalculationGroupMessage : SendMessage
    {
        public TheoreticalCalculationGroupMessage()
            : base(MessageTypes.THEORETICAL_CALCULATION_GROUP)
        {
        }

        public string SubsID
        {
            set => Header.PrivateKey = value;
        }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }

        [XmlArrayAttribute(ElementName = "actions"), XmlArrayItem(ElementName = "action")]
        public List<string> Actions { get; set; }

        [XmlElement(ElementName = "simulated_price")]
        public string SimulatedPrice { get; set; }

        [XmlElement(ElementName = "simulated_price_mode")]
        public string SimulatedPriceMode { get; set; }

        [XmlElement(ElementName = "simulated_price_currency")]
        public string SimulatedPriceCurrency { get; set; }

        [XmlElement(ElementName = "simulated_volatility")]
        public string SimulatedVolatility { get; set; }

        [XmlElement(ElementName = "simulated_volatility_mode")]
        public string SimulatedVolatilityMode { get; set; }

        [XmlElement(ElementName = "simulated_model")]
        public string SimulatedModel { get; set; }

        [XmlElement(ElementName = "simulated_strikeprice")]
        public string SimulatedStrikePrice { get; set; }

        [XmlElement(ElementName = "simulated_strikeprice_mode")]
        public string SimulatedStrikePriceMode { get; set; }

        [XmlElement(ElementName = "financial_rate_offset")]
        public string FinancialRateOffset { get; set; }

        [XmlElement(ElementName = "base_ytm_offset")]
        public string BaseYtmOffset { get; set; }

        [XmlElement(ElementName = "dividend_percentage")]
        public string DividendPercentage { get; set; }

        [XmlElement(ElementName = "analysis_date")]
        public string AnalysisDate { get; set; }

        [XmlElement(ElementName = "volatility_surface")]
        public string VolatilitySurface { get; set; }

        [XmlElement(ElementName = "contract_price")]
        public string ContractPrice { get; set; }

        [XmlElement(ElementName = "user_percentage")]
        public string UserPercentage { get; set; }

        [XmlElement(ElementName = "result_currency")]
        public string ResultCurrency { get; set; }

        [XmlElement(ElementName = "feed_minimum_hold")]
        public string FeedMinimumHold { get; set; }

        [XmlElement(ElementName = "_portfolio_volume")]
        public string PortfolioVolume { get; set; }

        [XmlElement(ElementName = "portfolio_accrued")]
        public string PortfolioAccrued { get; set; }

        [XmlElement(ElementName = "portfolio_invested")]
        public string PortfolioInvested { get; set; }

        [XmlElement(ElementName = "portfolio_currency")]
        public string PortfolioCurrency { get; set; }
    }

    #endregion

    #region TheoreticalCalculationGroupReply

    [XmlRootAttribute(ElementName = "orc_message")]
    public class TheoreticalCalculationGroupReply : ReplyMessage
    {
        public TheoreticalCalculationGroupReply()
            : base(MessageTypes.THEORETICAL_CALCULATION_GROUP)
        {
        }

        [XmlElement(ElementName = "instrument_id")]
        public InstrumentID InstrumentId { get; set; }

        [XmlElement(ElementName = "result_currency")]
        public string ResultCurrency { get; set; }

        [XmlArrayAttribute(ElementName = "calculation_results"), XmlArrayItem(ElementName = "calculation_result")]
        public List<CalculationResult> Results { get; set; }

        [XmlElement(ElementName = "feed_tag")]
        public int FeedTag { get; set; }
    }

    #endregion

    #region Actions

    [XmlRootAttribute(ElementName = "actions")]
    public class Actions : MessageBase
    {
        [XmlElement(ElementName = "action")]
        public List<string> Action { get; set; }
    }

    #endregion

    #region CalculationResults

    [XmlRootAttribute(ElementName = "calculation_results")]
    public class CalculationResults : MessageBase
    {
        [XmlElement(ElementName = "instrument_id")]
        public int InstrumentId { get; set; }

        [XmlElement(ElementName = "calculation_date")]
        public DateTime CalculationDate { get; set; }

        [XmlElement(ElementName = "analysis_date")]
        public DateTime AnalysisDate { get; set; }

        [XmlElement(ElementName = "calculation_step_id")]
        public int CalculationStepId { get; set; }

        [XmlElement(ElementName = "calculation_result")]
        public List<CalculationResult> Results { get; set; }
    }

    #endregion

    #region CalculationResult

    [XmlRootAttribute(ElementName = "calculation_result")]
    public class CalculationResult : MessageBase
    {
        [XmlElement(ElementName = "action")]
        public string Action { get; set; }

        [XmlElement(ElementName = "result")]
        public double Result { get; set; }

        [XmlElement(ElementName = "error")]
        public int Error { get; set; }
    }

    #endregion

    #region TheoreticalPrice

    [XmlRootAttribute(ElementName = "theoretical_price")]
    public class TheoreticalPrice : MessageBase
    {
        [XmlElement(ElementName = "instrument_id")]
        public int InstrumentId { get; set; }

        [XmlElement(ElementName = "calculation_date")]
        public string CalculationDate { get; set; }

        [XmlElement(ElementName = "price")]
        public double Price { get; set; }

        [XmlElement(ElementName = "actual_volatility")]
        public double ActualVolatility { get; set; }

        [XmlElement(ElementName = "volatility_surface_name")]
        public string VolatilitySurfaceName { get; set; }
    }

    #endregion
}
