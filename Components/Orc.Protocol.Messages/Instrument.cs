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

using System.Collections.Generic;
using System.Xml.Serialization;

namespace Highlander.Orc.Messages
{    
	#region Instrument Feed Subscription Toggle

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentFeedSubscriptionToggle : SendMessage
	{
   	public InstrumentFeedSubscriptionToggle()
		: base(MessageTypes.INSTRUMENT_FEED_TOGGLE)
		{
		}
     
        public string SubsID
        {          
            set => Header.PrivateKey = value;
        }

        [XmlElement(ElementName = "toggle")]
		public bool Toggle { get; set; }

        [XmlElement(ElementName = "assettype")]
		public string AssetType { get; set; }

        [field: XmlElement(ElementName = "instrument_list")]
        public InstrumentList InstrumentList { get; set; }

        [field: XmlElement(ElementName = "kind")]
        public string Kind { get; set; }

        [field: XmlElement(ElementName = "underlying")]
        public string Underlying { get; set; }

        [field: XmlElement(ElementName = "market")]
        public string Market { get; set; }

        [field: XmlElement(ElementName = "feedcode")]
        public string FeedCode { get; set; }
    }

	#endregion

	#region Instrument Feed

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentFeed : ReplyMessage
	{
		public InstrumentFeed()
			: base(MessageTypes.INSTRUMENT_FEED)
		{
		}

        [XmlElement(ElementName = "action")]
		public string Action { get; set; }

        [XmlElement(ElementName = "instrument_id")]
		public InstrumentID InstrumentID { get; set; }

        [XmlElement(ElementName = "instrument_attributes")]
		public InstrumentAttributes InstrumentAttributes { get; set; }

        [XmlElement(ElementName = "parameters")]
		public Parameters Parameters { get; set; }
    }

	#endregion

    #region Instrument Download

    [XmlRootAttribute(ElementName = "orc_message")]
    public class InstrumentDownload : SendMessage
    {
        public InstrumentDownload()
            : base(MessageTypes.INSTRUMENT_DOWNLOAD)
        {
        }

        [XmlElement(ElementName = "basecontract")]
        public string BaseContractTag { get; set; }

        [XmlElement(ElementName = "assettype")]
        public string AssetType { get; set; }

        [XmlElement(ElementName = "currency")]
        public string Currency { get; set; }

        [XmlElement(ElementName = "customer_unique_id")]
        public string CustomerUniqueId { get; set; }

        [XmlElement(ElementName = "download_mode")]
        public string DownloadMode { get; set; }

        [XmlElement(ElementName = "enforced_customer_id")]
        public string EnforcedCustomerID { get; set; }

        [XmlElement(ElementName = "exchange")]
        public string Exchange { get; set; }

        [XmlElement(ElementName = "expirydate_end")]
        public string ExpiryDateEnd { get; set; }

        [XmlElement(ElementName = "expirydate_start")]
        public string ExpiryDateStart { get; set; }

        [XmlElement(ElementName = "feedcode")]
        public string FeedCode { get; set; }

        [XmlElement(ElementName = "ignore_case")]
        public string IgnoreCase { get; set; }

        [XmlElement(ElementName = "isincode")]
        public string Isincode { get; set; }

        [XmlElement(ElementName = "issuer")]
        public string Issuer { get; set; }

        [XmlElement(ElementName = "is_clean_quoted")]
        public string IsCleanQuoted { get; set; }

        [XmlElement(ElementName = "items_per_message")]
        public string ItemsPerMessage { get; set; }

        [XmlElement(ElementName = "kind")]
        public string Kind { get; set; }

        [XmlElement(ElementName = "market")]
        public string Market { get; set; }

        [XmlElement(ElementName = "strikeprice_max")]
        public string StrikepriceMax { get; set; }

        [XmlElement(ElementName = "strikeprice_min")]
        public string StrikepriceMin { get; set; }

        [XmlElement(ElementName = "submarket")]
        public string Submarket { get; set; }

        [XmlElement(ElementName = "suggest_volume_logic")]
        public string SuggestVolumeLogic { get; set; }

        [XmlElement(ElementName = "symbol")]
        public string Symbol { get; set; }

        [XmlElement(ElementName = "tick_rule")]
        public string TickRule { get; set; }

        [XmlElement(ElementName = "underlying")]
        public string Underlying { get; set; }
    }

    #endregion

	#region Instrument Attributes Set

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentAttributesSet : SendMessage
	{
		public InstrumentAttributesSet()
			: base(MessageTypes.INSTRUMENT_ATTRIBUTES_SET)
		{
		}

        [XmlElement(ElementName = "instrument_id")]
		public InstrumentID InstrumentID { get; set; }

        [XmlElement(ElementName = "instrument_attributes")]
		public InstrumentAttributes InstrumentAttributes { get; set; }
    }

	#endregion

	#region Instrument Parameters Set

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentParametersSet : SendMessage
	{
		public InstrumentParametersSet()
			: base(MessageTypes.INSTRUMENT_PARAMETERS_SET)
		{
		}

        [XmlElement(ElementName = "instrument_id")]
		public InstrumentID InstrumentID { get; set; }

        [XmlElement(ElementName = "parameters")]
		public Parameters Parameters { get; set; }
    }

	#endregion

	#region Instrument Create

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentCreate : SendMessage
	{
		public InstrumentCreate()
			: base(MessageTypes.INSTRUMENT_CREATE)
		{
		}

        [XmlElement(ElementName = "instrument_attributes")]
		public InstrumentAttributes InstrumentAttributes { get; set; }

        [XmlElement(ElementName = "parameters")]
		public Parameters Parameters { get; set; }
    }

	#endregion

	#region Instrument Delete

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentDelete : SendMessage
	{
		public InstrumentDelete()
			: base(MessageTypes.INSTRUMENT_DELETE)
		{
		}

        [XmlElement(ElementName = "instrument_id")]
		public InstrumentID InstrumentID { get; set; }
    }

	#endregion

	#region Instrument List

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentList : MessageBase
	{
        [XmlElement(ElementName = "instrument_id")]
		public List<InstrumentID> Instruments { get; set; }
    }

	#endregion

	#region Instrument Get

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentGet : SendMessage
	{
		public InstrumentGet() : base (MessageTypes.INSTRUMENT_GET)
		{
		}

        [XmlElement(ElementName = "instrument_id")]
		public InstrumentID InstrumentID { get; set; }
    }

	#endregion  

	#region Instrument Get Reply

	[XmlRootAttribute(ElementName = "orc_message")]
	public class InstrumentGetReply : ReplyMessage
	{
		public InstrumentGetReply()
			: base(MessageTypes.INSTRUMENT_GET)
		{
		}

        [XmlElement(ElementName = "instrument_id")]
		public InstrumentID InstrumentID { get; set; }

        [XmlElement(ElementName = "instrument_attributes")]
		public InstrumentAttributes Attributes { get; set; }

        [XmlElement(ElementName = "parameters")]
		public Parameters Parameters { get; set; }

        [XmlElement(ElementName = "tick_rule")]
		public string TickRule { get; set; }
    }

	#endregion

	#region InstrumentID

	[XmlRootAttribute(ElementName = "instrument_id")]
	public class InstrumentID : MessageBase
	{
        [XmlElement(ElementName = "assettype")]
		public string Assettype { get; set; }

        [XmlElement(ElementName = "currency")]
		public string Currency { get; set; }

        [XmlElement(ElementName = "customer_unique_id")]
		public string CustomerUniqueID { get; set; }

        [XmlElement(ElementName = "description")]
		public string Description { get; set; }

        [XmlElement(ElementName = "enforced_customer_id")]
		public string EnforcedCustomerID { get; set; }

        [XmlElement(ElementName = "exchange")]
		public string Exchange { get; set; }

        [XmlElement(ElementName = "expirydate")]
		public string ExpiryDate { get; set; }

        [XmlElement(ElementName = "expirytype")]
		public string ExpiryType { get; set; }

        [XmlElement(ElementName = "feedcode")]
		public string FeedCode { get; set; }

        [XmlElement(ElementName = "instrument_tag")]
		public int InstrumentTag { get; set; }

        [XmlElement(ElementName = "isincode")]
		public string Isincode { get; set; }

        [XmlElement(ElementName = "kind")]
		public string Kind { get; set; }

        [XmlElement(ElementName = "market")]
		public string Market { get; set; }

        [XmlElement(ElementName = "multiplier")]
		public double Multiplier { get; set; }

        [XmlElement(ElementName = "strikeprice")]
        public double Strikeprice { get; set; }

        [XmlElement(ElementName = "symbol")]
		public string Symbol { get; set; }

        [XmlElement(ElementName = "sedol")]
        public string Sedol { get; set; }

        [XmlElement(ElementName = "underlying")]
		public string Underlying { get; set; }

        [XmlElement(ElementName = "valoren")]
		public int Valoren { get; set; }
    }

	#endregion

	#region Instrument Attributes

	[XmlRootAttribute(ElementName = "instrument_attributes")]
	public class InstrumentAttributes : MessageBase
	{
        [XmlElement(ElementName = "assettype")]
		public string AssetType { get; set; }

        [XmlElement(ElementName = "kind")]
		public string Kind { get; set; }

        [XmlElement(ElementName = "underlying")]
		public string Underlying { get; set; }

        [XmlElement(ElementName = "expirydate")]
		public string ExpiryDate { get; set; }

        [XmlElement(ElementName = "strikeprice")]
		public double StrikePrice { get; set; }

        [XmlElement(ElementName = "currency")]
		public string Currency { get; set; }

        [XmlElement(ElementName = "expirytype")]
		public string ExpiryType { get; set; }

        [XmlElement(ElementName = "multiplier")]
		public double Multiplier { get; set; }

        [XmlElement(ElementName = "username")]
        public string UserName { get; set; }
    }

	#endregion

    #region Components

	[XmlRootAttribute(ElementName = "components")]
	public class Components : MessageBase
	{
        [XmlElement(ElementName = "component")]
		public List<Component> Component { get; set; }
    }

	#endregion

	#region Component

	[XmlRootAttribute(ElementName = "component")]
	public class Component : MessageBase
	{
        [XmlElement(ElementName = "instrument_id")]
		public InstrumentID InstrumentId { get; set; }

        [XmlElement(ElementName = "volume")]
		public double Volume { get; set; }

        [XmlElement(ElementName = "ranking")]
		public int Ranking { get; set; }
    }

	#endregion

	#region Dynamic Parameter

	[XmlRootAttribute(ElementName = "dynamic_parameter")]
	public class DynamicParameter : MessageBase
	{
        [XmlElement(ElementName = "name")]
		public string Name { get; set; }

        [XmlElement(ElementName = "value")]
		public string Value { get; set; }
    }

	#endregion

	#region Parameters

	[XmlRootAttribute(ElementName = "parameters")]
	public class Parameters : MessageBase
	{
        [XmlElement(ElementName = "accrued_rateday_convention")]
		public string AccruedRateDayConvention { get; set; }

        [XmlElement(ElementName = "apply_baseoffset_logic")]
		public string ApplyBaseOffsetLogic { get; set; }

        [XmlElement(ElementName = "askbaseoffset")]
		public double AskBaseOffset { get; set; }

        [XmlElement(ElementName = "askoffset")]
		public double AskOffset { get; set; }

        [XmlElement(ElementName = "askrateoffset")]
		public double AskRateOffset { get; set; }

        [XmlElement(ElementName = "auto_diming_ask_flag")]
		public bool AutoDimingAskFlag { get; set; }

        [XmlElement(ElementName = "auto_diming_bid_flag")]
		public bool AutoDimingBidFlag { get; set; }

        [XmlElement(ElementName = "auto_hedging_ask_flag")]
		public bool AutoHedgingAskFlag { get; set; }

        [XmlElement(ElementName = "auto_hedging_bid_flag")]
		public bool AutoHedgingBidFlag { get; set; }

        [XmlElement(ElementName = "auto_mass_quote_ask_flag")]
		public bool AutoMassQuoteAskFlag { get; set; }

        [XmlElement(ElementName = "auto_mass_quote_bid_flag")]
		public bool AutoMassQuoteBidFlag { get; set; }

        [XmlElement(ElementName = "auto_quote_flag")]
		public bool AutoQuoteFlag { get; set; }

        [XmlElement(ElementName = "auto_responding_ask_flag")]
		public bool AutoRespondingAskFlag { get; set; }

        [XmlElement(ElementName = "auto_responding_bid_flag")]
		public bool AutoRespondingBidFlag { get; set; }

        [XmlElement(ElementName = "auto_traded_allowed_volume")]
		public double AutoTradedAllowedVolume { get; set; }

        [XmlElement(ElementName = "auto_traded_volume_ask")]
		public double AutoTradedVolumeAsk { get; set; }

        [XmlElement(ElementName = "auto_traded_volume_bid")]
		public double AutoTradedVolumeBid { get; set; }

        [XmlElement(ElementName = "auto_trading_ask_flag")]
		public bool AutoTradingAskFlag { get; set; }

        [XmlElement(ElementName = "auto_trading_bid_flag")]
		public bool AutoTradingBidFlag { get; set; }

        [XmlElement(ElementName = "barrier")]
		public double Barrier { get; set; }

        [XmlElement(ElementName = "basecontract")]
		public InstrumentID BaseContract { get; set; }

        [XmlElement(ElementName = "baseoffset")]
		public double BaseOffset { get; set; }

        [XmlElement(ElementName = "bidbaseoffset")]
		public double BidBaseOffset { get; set; }

        [XmlElement(ElementName = "bidoffset")]
        public double BidOffset { get; set; }

        [XmlElement(ElementName = "bidrateoffset")]
        [CustomComparable("System.decimal")]
        public string BidRateOffset { get; set; }

        [XmlElement(ElementName = "components")]
        [NonComparable]
        public Components Components { get; set; }

        [XmlElement(ElementName = "convertible_from")]
		public string ConvertibleFrom { get; set; }

        [XmlElement(ElementName = "cusip")]
		public string Cusip { get; set; }

        [XmlElement(ElementName = "custom_tick_size")]
		public double CustomTickSize { get; set; }

        [XmlElement(ElementName = "custom_lot_size")]
		public double CustomLotSize { get; set; }

        [XmlElement(ElementName = "custom_volume_step")]
		public double CustomVolumeStep { get; set; }

        [XmlElement(ElementName = "customer_unique_id")]
		public string CustomerUniqueId { get; set; }

        [XmlElement(ElementName = "description")]
		public string Description { get; set; }

        [XmlElement(ElementName = "dividendcontract")]
		public InstrumentID DividendContract { get; set; }

        [XmlElement(ElementName = "divs_est_net")]
		public double DivsEstNet { get; set; }

        [XmlElement(ElementName = "divs_paid_to_holder")]
		public double DivsPaidToHolder { get; set; }

        [XmlArrayAttribute(ElementName = "dynamic_parameters"), XmlArrayItem(ElementName = "dynamic_parameter")]
		public List<DynamicParameter> DynamicParameters { get; set; }

        [XmlElement(ElementName = "end_date")]
		public string EndDate { get; set; }

        [XmlElement(ElementName = "enforced_customer_unique_id")]
		public string EnforcedCustomerUniqueId { get; set; }

        [XmlElement(ElementName = "exchange")]
		public string Exchange { get; set; }

        [XmlElement(ElementName = "expirytime")]
		public string ExpiryTime { get; set; }

        [XmlElement(ElementName = "feedcode")]
		public string FeedCode { get; set; }

        [XmlElement(ElementName = "final_settlement_days")]
		public int FinalSettlementDays { get; set; }

        [XmlElement(ElementName = "fixed_fx_rate")]
		public double FixedFxRate { get; set; }

        [XmlElement(ElementName = "frn_spread")]
		public double FrnSpread { get; set; }

        [XmlElement(ElementName = "fx_spot_correlation")]
		public double FxSpotCorrelation { get; set; }

        [XmlElement(ElementName = "fx_volatility")]
		public double FxVolatility { get; set; }

        [XmlElement(ElementName = "hedgecontract")]
		public InstrumentID HedgeContract { get; set; }

        [XmlElement(ElementName = "isincode")]
		public string IsinCode { get; set; }

        [XmlElement(ElementName = "issuer")]
		public string Issuer { get; set; }

        [XmlElement(ElementName = "is_ytm_quoted")]
		public bool IsYtmQuoted { get; set; }

        [XmlElement(ElementName = "is_ytm_strike")]
		public bool IsYtmStrike { get; set; }

        [XmlElement(ElementName = "lower_barrier")]
		public double LowerBarrier { get; set; }

        [XmlElement(ElementName = "market")]
		public string Market { get; set; }

        [XmlElement(ElementName = "model")]
		public string Model { get; set; }

        [XmlElement(ElementName = "note_option_multiplier")]
		public double NoteOptionMultiplier { get; set; }

        [XmlElement(ElementName = "period_length")]
		public int PeriodLength { get; set; }

        [XmlElement(ElementName = "premium_payment_at_expiry")]
		public bool PremiumPaymentAtExpiry { get; set; }

        [XmlElement(ElementName = "price_display")]
		public double PriceDisplay { get; set; }

        [XmlElement(ElementName = "price_multiplier")]
		public double PriceMultiplier { get; set; }

        [XmlElement(ElementName = "quote_volatility_surface")]
		public string QuoteVolatilitySurface { get; set; }

        [XmlElement(ElementName = "quotelot_multiplier")]
		public double QuoteLotMultiplier { get; set; }

        [XmlElement(ElementName = "quotevolatility")]
		public double QuoteVolatility { get; set; }

        [XmlElement(ElementName = "quotevolatility_askoffset")]
        [CustomComparable("System.decimal")]
        public string QuoteVolitilityAskOffset { get; set; }

        [XmlElement(ElementName = "quotevolatility_bidoffset")]
        [CustomComparable("System.decimal")]
        public string QuoteVolitilityBidOffset { get; set; }

        [XmlElement(ElementName = "quote_askbaseoffset")]
		public double QuoteAskBaseOffset { get; set; }

        [XmlElement(ElementName = "quote_askoffset")]
		public double QuoteAskOffset { get; set; }

        [XmlElement(ElementName = "quote_bidbaseoffset")]
		public double QuoteBidBaseOffset { get; set; }

        [XmlElement(ElementName = "quote_bidoffset")]
		public double QuoteBidOffset { get; set; }

        //private double _quoteInstrumentdAllowedVolume;

		//[XmlElement(ElementName = "quote_instrument_allowed_volume")]
		//public double QuoteInstrumentdAllowedVolume
		//{
		//    get { return _quoteInstrumentdAllowedVolume; }
		//    set { _quoteInstrumentdAllowedVolume = value; }
		//}

		//private double _quoteInstrumentdVolumeAsk;

		//[XmlElement(ElementName = "quote_instrument_volume_ask")]
		//public double QuoteInstrumentdVolumeAsk
		//{
		//    get { return _quoteInstrumentdVolumeAsk; }
		//    set { _quoteInstrumentdVolumeAsk = value; }
		//}

		//private double _quoteInstrumentdVolumeBid;

		//[XmlElement(ElementName = "quote_instrument_volume_bid")]
		//public double QuoteInstrumentdVolumeBid
		//{
		//    get { return _quoteInstrumentdVolumeBid; }
		//    set { _quoteInstrumentdVolumeBid = value; }
		//}

        [XmlElement(ElementName = "rebate")]
		public double Rebate { get; set; }

        [XmlElement(ElementName = "redemption")]
		public double Redemption { get; set; }

        [XmlElement(ElementName = "risk_volitility_surface")]
		public string RiskVolitilitySurface { get; set; }

        [XmlElement(ElementName = "sedol")]
		public string Sedol { get; set; }

        [XmlElement(ElementName = "settlement_calendar")]
		public string SettlementCalendar { get; set; }

        [XmlElement(ElementName = "settlement_days")]
		public int SettlementDays { get; set; }

        [XmlElement(ElementName = "settlement_style")]
		public string SettlemetStyle { get; set; }

        [XmlElement(ElementName = "spread")]
		public double Spread { get; set; }

        [XmlElement(ElementName = "spread_bias")]
		public double SpreadBias { get; set; }

        [XmlElement(ElementName = "spread_table")]
		public string SpreadTable { get; set; }

        [XmlElement(ElementName = "start_date")]
		public string StartDate { get; set; }

        [XmlElement(ElementName = "strike_currency")]
		public string StrikeCurrency { get; set; }

        [XmlElement(ElementName = "strike_yield_name")]
		public string StrikeYieldName { get; set; }

        [XmlElement(ElementName = "submarket")]
		public string Submarket { get; set; }

        [XmlElement(ElementName = "suggest_logic")]
		public string SuggestLogic { get; set; }

        [XmlElement(ElementName = "suggest_volume_logic")]
		public string SuggestVolumeLogic { get; set; }

        [XmlElement(ElementName = "suggest_volume_min")]
		public double SuggestVolumeMin { get; set; }

        [XmlElement(ElementName = "symbol")]
		public string Symbol { get; set; }

        [XmlElement(ElementName = "tick_rule")]
		public string TickRule { get; set; }

        [XmlElement(ElementName = "trading_calendar")]
		public string TradingCalendar { get; set; }

        [XmlElement(ElementName = "underlying_expiry")]
		public string UnderlyingExpiry { get; set; }

        [XmlElement(ElementName = "underlying_rate")]
		public double UnderlyingRate { get; set; }

        [XmlElement(ElementName = "underlying_rate_mode")]
		public string UnderlyingRateModel { get; set; }

        [XmlElement(ElementName = "underlying_strike")]
		public double UnderlyingStrike { get; set; }

        [XmlElement(ElementName = "upper_Barrier")]
		public double UpperBarrier { get; set; }

        [XmlElement(ElementName = "use_volatility_surface")]
		public bool UseVolatilitySurface { get; set; }

        [XmlElement(ElementName = "use_volatility_surface_for_quoting")]
		public bool UseVolatilitySurfaceForQuoting { get; set; }

        [XmlElement(ElementName = "valoren")]
		public int Valoren { get; set; }

        [XmlElement(ElementName = "volatility")]
		public double Volatility { get; set; }

        [XmlElement(ElementName = "volatility_askoffset")]
		public double VolatilityAskOffset { get; set; }

        [XmlElement(ElementName = "volatility_bidoffset")]
		public double VolatilityBidOffset { get; set; }

        [XmlElement(ElementName = "volatility_day_convention")]
		public string VolatilityDayConvention { get; set; }

        [XmlElement(ElementName = "volatility_decrease_end")]
		public string VolatilityDecreaseEnd { get; set; }

        [XmlElement(ElementName = "volatility_decrease_start")]
		public string VolatilityDecreaseStart { get; set; }

        [XmlElement(ElementName = "volatility_time_mode")]
		public string VolatilityTimeMode { get; set; }

        [XmlElement(ElementName = "volume_limit")]
		public double VolumeLimit { get; set; }

        [XmlElement(ElementName = "yield_name")]
		public string YieldName { get; set; }

        [XmlElement(ElementName = "yield_offset")]
		public double YieldOffset { get; set; }

        [XmlElement(ElementName = "ytm_display")]
		public string YtmDisplay { get; set; }

        [XmlElement(ElementName = "ytm_rateday_convention")]
		public string YtmRatedayConvention { get; set; }

        [XmlElement(ElementName = "ytm_ratetype_convention")]
		public string YtmRateTypeConvention { get; set; }
    }
	#endregion
}
