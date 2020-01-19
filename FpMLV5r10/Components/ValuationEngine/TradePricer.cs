#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using FpML.V5r10.Reporting.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r10.Reporting;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Instruments;
using Orion.ValuationEngine.Helpers;
using Orion.ValuationEngine.Reports;
using Orion.CalendarEngine.Helpers;
using FpML.V5r10.Reporting.Identifiers;
using Orion.ValuationEngine.Pricers.Assets;
using Orion.ValuationEngine.Pricers.Products;
using TradeIdentifier = FpML.V5r10.Reporting.Identifiers.TradeIdentifier;

#endregion

namespace Orion.ValuationEngine
{
    public class TradePricer : TradePricerBase
    {
        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public TradePricer()
        {}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"></param>
        /// <param name="trade"></param>
        /// <param name="tradeProps"></param>
        public TradePricer(ILogger logger, ICoreCache cache, String nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Trade trade, NamedValueSet tradeProps)
            : this(logger, cache, nameSpace, legCalendars, trade, tradeProps, false)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="legCalendars"></param>
        /// <param name="trade"></param>
        /// <param name="tradeProps"></param>
        /// <param name="forecastRateInterpolation"></param>
        public TradePricer(ILogger logger, ICoreCache cache, String nameSpace,
            List<Pair<IBusinessCalendar, IBusinessCalendar>> legCalendars,
            Trade trade, NamedValueSet tradeProps, bool forecastRateInterpolation)
        {
            if (tradeProps == null)
            {
                tradeProps = new NamedValueSet();//TODO Need to generate properties for the FpML examples.
            }
            var tradeIdentifier = new TradeIdentifier(tradeProps);
            TradeIdentifier = tradeIdentifier;
            TradeHeader = trade.tradeHeader;
            //Get the baseParty, which in GWML is the originating party.
            BaseParty = tradeProps.GetValue<string>(TradeProp.BaseParty, false) ?? TradeProp.Party1;
            var party1 = tradeProps.GetValue<string>(TradeProp.Party1, true);
            var party2 = tradeProps.GetValue<string>(TradeProp.Party2, true);
            Parties = new List<Party> { new Party { partyName = new PartyName { Value = party1 } }, new Party { partyName = new PartyName { Value = party2 } } };
            TradeType = trade.ItemElementName;
            //Determine the product type, so that the appropriate productPricer can be instantiated.
            //Set the product type
            var productType = tradeIdentifier.ProductType;
            //Check whether the business calendars list is null.
            Pair<IBusinessCalendar, IBusinessCalendar> firstCalendarPair = null;
            if (legCalendars?.Count > 0)
            {
                firstCalendarPair = legCalendars[0];
            }
            //Instantiate the productPricer.
            if (productType != null && productType != ProductTypeSimpleEnum.Undefined)
            {
                ProductType = (ProductTypeSimpleEnum)productType;
                switch (ProductType)
                {
                    case ProductTypeSimpleEnum.PropertyTransaction:
                        {
                            IBusinessCalendar settlementCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                            }
                            var property = (PropertyTransaction)trade.Item;
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate, false);
                            var referenceProperty = tradeProps.GetValue<String>(PropertyProp.ReferenceProperty, false);
                            //Get the instrument configuration data.
                            //Modify the pricer to include this data.
                            PriceableProduct = new PropertyTransactionPricer(logger, cache, nameSpace, tradeDate, referenceProperty, settlementCalendar, property, BaseParty, forecastRateInterpolation);
                            ProductReporter = new PropertyTransactionReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.EquityTransaction:
                        {
                            IBusinessCalendar settlementCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                            }
                            var equity = (EquityTransaction)trade.Item;
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate,true);
                            var effectiveDate = tradeProps.GetValue<DateTime>(TradeProp.EffectiveDate, true);
                            var referenceEquity = tradeProps.GetValue<String>(EquityProp.ReferenceEquity, false);
                            //Get the instrument configuration data.
                            //Modify the pricer to include this data.
                            PriceableProduct = new EquityTransactionPricer(logger, cache, nameSpace, tradeDate, effectiveDate, referenceEquity, settlementCalendar, equity, BaseParty, forecastRateInterpolation);
                            ProductReporter = new EquityTransactionReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.BondTransaction:
                        {
                            IBusinessCalendar settlementCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                            }
                            var bond = (BondTransaction)trade.Item;
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate, true);
                            var effectiveDate = tradeProps.GetValue<DateTime>(TradeProp.EffectiveDate, true);
                            var bondType = tradeProps.GetValue<string>(BondProp.BondType, false);
                            //Get the instrument configuration data.
                            //Modify the pricer to include this data.
                            PriceableProduct = new BondTransactionPricer(logger, cache, nameSpace, tradeDate, effectiveDate, settlementCalendar, settlementCalendar, bond, BaseParty, bondType, forecastRateInterpolation);
                            ProductReporter = new BondTransactionReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.FutureTransaction:
                        {
                            IBusinessCalendar settlementCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                            }
                            var future = (FutureTransaction)trade.Item;
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate, false);
                            var type = tradeProps.GetValue<String>(FuturesProp.FuturesType, true);
                            var futureType = EnumHelper.Parse<ExchangeContractTypeEnum>(type);
                            //Get the instrument configuration data.
                            //Modify the pricer to include this data.
                            PriceableProduct = new FutureTransactionPricer(logger, cache, nameSpace, tradeDate, futureType, settlementCalendar, future, BaseParty, forecastRateInterpolation);
                            ProductReporter = new FutureTransactionReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.InterestRateSwap:
                        {
                            var swap = (Swap)trade.Item;
                            PriceableProduct = new InterestRateSwapPricer(logger, cache, nameSpace, legCalendars, swap, BaseParty, forecastRateInterpolation);
                            ProductReporter = new InterestRateSwapReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.AssetSwap:
                        {
                            var swap = (Swap)trade.Item;
                            //TODO set for the payer. This needs to be modified for the base counterparty.
                            PriceableProduct = new AssetSwapPricer(logger, cache, nameSpace, legCalendars, swap, BaseParty, new Bond(), forecastRateInterpolation);
                            ProductReporter = new InterestRateSwapReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.CrossCurrencySwap:
                        {
                            var swap = (Swap)trade.Item;
                            //TODO set for the payer. This needs to be modified for the base counterparty.
                            PriceableProduct = new CrossCurrencySwapPricer(logger, cache, nameSpace, legCalendars, swap, BaseParty, forecastRateInterpolation);
                            ProductReporter = new InterestRateSwapReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.FRA: // todo
                        {
                            var fra = (Fra)trade.Item;
                            IBusinessCalendar fixingCalendar = null;
                            IBusinessCalendar paymentCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                fixingCalendar = firstCalendarPair.First;
                                paymentCalendar = firstCalendarPair.Second;
                            }
                            PriceableProduct = new FraPricer(logger, cache, fixingCalendar, paymentCalendar, fra, BaseParty, nameSpace)
                            {
                                ForecastRateInterpolation = forecastRateInterpolation
                            };
                            ProductReporter = new ForwardRateAgreementReporter();
                        }
                        break;
                    //case ProductTypeSimpleEnum.InflationSwap:
                    //    break;
                    //case ProductTypeSimpleEnum.CreditDefaultSwap:
                    //    break;
                    //case ProductTypeSimpleEnum.TotalReturnSwap:
                    //    break;
                    //case ProductTypeSimpleEnum.VarianceSwap:
                    //    break;
                    case ProductTypeSimpleEnum.CapFloor:
                        {
                            var capFloor = (CapFloor)trade.Item;
                            IBusinessCalendar fixingCalendar = null;
                            IBusinessCalendar paymentCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                fixingCalendar = firstCalendarPair.First;
                                paymentCalendar = firstCalendarPair.Second;
                            }
                            PriceableProduct = new CapFloorPricer(logger, cache, nameSpace, fixingCalendar, paymentCalendar, capFloor, BaseParty);
                            ProductReporter = new CapFloorReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.FxSpot:
                        {
                            var fxForward = (FxSingleLeg)trade.Item;
                            PriceableProduct = new FxSingleLegPricer(fxForward, BaseParty, ProductTypeSimpleEnum.FxSpot);
                            ProductReporter = new FxSingleLegReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.FxForward:
                        {
                            var fxForward = (FxSingleLeg)trade.Item;
                            PriceableProduct = new FxSingleLegPricer(fxForward, BaseParty, ProductTypeSimpleEnum.FxForward);
                            ProductReporter = new FxSingleLegReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.BulletPayment:
                        {
                            if (trade.Item is BulletPayment bullet)
                            {
                                IBusinessCalendar paymentCalendar = null;
                                if (firstCalendarPair != null)
                                {
                                    paymentCalendar = firstCalendarPair.Second;
                                }
                                //The calendars
                                if (paymentCalendar == null)
                                {
                                    if (bullet.payment.paymentDate != null)
                                    {
                                        var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(bullet.payment.paymentDate, ItemsChoiceType.dateAdjustments, out object dateAdjustments);
                                        if (containsPaymentDateAdjustments && dateAdjustments != null)
                                        {
                                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ((BusinessDayAdjustments)dateAdjustments).
                                                                                                      businessCenters, nameSpace);
                                        }
                                    }
                                }
                                PriceableProduct = new BulletPaymentPricer(bullet, BaseParty, paymentCalendar);
                                ProductReporter = new BulletPaymentReporter();
                            }
                        }
                        break;
                    case ProductTypeSimpleEnum.FxSwap:
                        {
                            var fxSwap = (FxSwap)trade.Item;
                            PriceableProduct = new FxSwapPricer(fxSwap, BaseParty);
                            ProductReporter = new FxSwapReporter();
                        }
                        break;
                    //case ProductTypeSimpleEnum.EquityOption:
                    //    break;
                    //case ProductTypeSimpleEnum.BondOption:
                    //    break;
                    case ProductTypeSimpleEnum.FxOption:
                        {
                            IBusinessCalendar fixingCalendar = null;
                            IBusinessCalendar paymentCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                fixingCalendar = firstCalendarPair.First;
                                paymentCalendar = firstCalendarPair.Second;
                            }
                            var fxOption = (FxOption)trade.Item;
                            PriceableProduct = new VanillaEuropeanFxOptionPricer( logger, cache, nameSpace, fixingCalendar, paymentCalendar, fxOption, BaseParty);
                            ProductReporter = new FxOptionLegReporter();
                        }
                        break;
                    //case ProductTypeSimpleEnum.FxOptionStrategy:
                    //    break;
                    //case ProductTypeSimpleEnum.CreditDefaultIndex:
                    //    break;
                    //case ProductTypeSimpleEnum.CreditDefaultIndexTranche:
                    //    break;
                    //case ProductTypeSimpleEnum.CreditDefaultBasket:
                    //    break;
                    //case ProductTypeSimpleEnum.CreditDefaultBasketTranche:
                    //    break;
                    //case ProductTypeSimpleEnum.CreditDefaultOption:
                    //    break;
                    //case ProductTypeSimpleEnum.EquityForward:
                    //    break;
                    case ProductTypeSimpleEnum.InterestRateSwaption:
                        {
                            var interestRateSwaption = (Swaption)trade.Item;
                            PriceableProduct = new InterestRateSwaptionPricer(logger, cache, nameSpace, interestRateSwaption, BaseParty, forecastRateInterpolation);
                            ProductReporter = new InterestRateSwaptionReporter();
                        }
                        break;
                    case ProductTypeSimpleEnum.TermDeposit:
                        {
                            //var party1 = tradeProps.GetValue<string>(TradeProp.Party1, true);
                            //var party2 = tradeProps.GetValue<string>(TradeProp.Party2, true);
                            //var reportingParty = baseParty == party1 ? "Party1" : "Party2"; // TODO this is for backward compatability.
                            var deposit = (TermDeposit)trade.Item;
                            PriceableProduct = new TermDepositPricer(logger, cache, deposit, TradeProp.Party1);//The payment date must be correct before calling this!
                            ProductReporter = new TermDepositReporter();
                        }
                        break;
                    //case ProductTypeSimpleEnum.DividendSwap:
                    //    break;
                    //case ProductTypeSimpleEnum.ConvertibleBondOption:
                    //    break;
                    //case ProductTypeSimpleEnum.Loan:
                    //    break;
                    //case ProductTypeSimpleEnum.Repo:
                    //    break;
                    default:
                        throw new NotSupportedException("Unsupported ProductType: " + ProductType);
                }
            }
            else
            {
                switch (TradeType)
                {
                    case ItemChoiceType15.propertyTransaction:
                        {
                            var equity = (PropertyTransaction)trade.Item;
                            IBusinessCalendar settlementCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                            }
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate, false);
                            var referenceProperty = tradeProps.GetValue<String>(PropertyProp.ReferenceProperty, false);
                            PriceableProduct = new PropertyTransactionPricer(logger, cache, nameSpace, tradeDate, referenceProperty, settlementCalendar, equity, BaseParty, forecastRateInterpolation);
                            ProductReporter = new PropertyTransactionReporter();
                        }
                        break;
                    case ItemChoiceType15.equityTransaction:
                        {
                            var equity = (EquityTransaction)trade.Item;
                            IBusinessCalendar settlementCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                            }
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate, false);
                            var effectiveDate = tradeProps.GetValue<DateTime>(TradeProp.EffectiveDate, true);
                            var referenceEquity = tradeProps.GetValue<String>(EquityProp.ReferenceEquity, false);
                            PriceableProduct = new EquityTransactionPricer(logger, cache, nameSpace, tradeDate, effectiveDate, referenceEquity, settlementCalendar, equity, BaseParty, forecastRateInterpolation);
                            ProductReporter = new EquityTransactionReporter();
                        }
                        break;
                    case ItemChoiceType15.bondTransaction:
                        {
                            var bond = (BondTransaction)trade.Item;
                            IBusinessCalendar settlementCalendar = null;
                            IBusinessCalendar paymentCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                                paymentCalendar = firstCalendarPair.Second;
                            }
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate, true);
                            var effectiveDate = tradeProps.GetValue<DateTime>(TradeProp.EffectiveDate, true);
                            var bondType = tradeProps.GetValue<string>(BondProp.BondType, false);
                            PriceableProduct = new BondTransactionPricer(logger, cache, nameSpace, tradeDate, effectiveDate, settlementCalendar, paymentCalendar, bond, BaseParty, bondType, forecastRateInterpolation);
                            ProductReporter = new BondTransactionReporter();
                        }
                        break;
                    case ItemChoiceType15.futureTransaction:
                        {
                            IBusinessCalendar settlementCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                settlementCalendar = firstCalendarPair.First;
                            }
                            var future = (FutureTransaction)trade.Item;
                            var tradeDate = tradeProps.GetValue<DateTime>(TradeProp.TradeDate, false);
                            var type = tradeProps.GetValue<String>(FuturesProp.FuturesType, true);
                            var futureType = EnumHelper.Parse<ExchangeContractTypeEnum>(type);
                            //Get the instrument configuration data.
                            //Modify the pricer to include this data.
                            PriceableProduct = new FutureTransactionPricer(logger, cache, nameSpace, tradeDate, futureType, settlementCalendar, future, BaseParty, forecastRateInterpolation);
                            ProductReporter = new FutureTransactionReporter();
                        }
                        break;
                    case ItemChoiceType15.swap:
                        {
                            var swap = (Swap)trade.Item;
                            //TODO this needs to be emnhanced
                            ProductType = ProductTypeSimpleEnum.InterestRateSwap;
                            PriceableProduct = new CrossCurrencySwapPricer(logger, cache, nameSpace, legCalendars, swap, BaseParty, forecastRateInterpolation);
                            ProductReporter = new InterestRateSwapReporter();
                            //var report =
                        }
                        break;
                    case ItemChoiceType15.fra: // todo
                        {
                            var fra = (Fra)trade.Item;
                            IBusinessCalendar fixingCalendar = null;
                            IBusinessCalendar paymentCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                fixingCalendar = firstCalendarPair.First;
                                paymentCalendar = firstCalendarPair.Second;
                            }
                            ProductType = ProductTypeSimpleEnum.FRA;
                            PriceableProduct = new FraPricer(logger, cache, fixingCalendar, paymentCalendar, fra, BaseParty)
                            {
                                ForecastRateInterpolation = forecastRateInterpolation
                            };
                            ProductReporter = new ForwardRateAgreementReporter();
                        }
                        break;
                    case ItemChoiceType15.capFloor:
                        {
                            var capFloor = (CapFloor)trade.Item;
                            IBusinessCalendar fixingCalendar = null;
                            IBusinessCalendar paymentCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                fixingCalendar = firstCalendarPair.First;
                                paymentCalendar = firstCalendarPair.Second;
                            }
                            ProductType = ProductTypeSimpleEnum.CapFloor;
                            PriceableProduct = new CapFloorPricer(logger, cache, nameSpace, fixingCalendar, paymentCalendar, capFloor, BaseParty);
                            ProductReporter = new CapFloorReporter();
                        }
                        break;
                    case ItemChoiceType15.fxSingleLeg:
                        {
                            var fxForward = (FxSingleLeg)trade.Item;
                            ProductType = ProductTypeSimpleEnum.FxSpot;
                            PriceableProduct = new FxSingleLegPricer(fxForward, BaseParty, ProductType);
                            ProductReporter = new FxSingleLegReporter();
                        }
                        break;
                    case ItemChoiceType15.fxSwap:
                        {
                            var fxSwap = (FxSwap)trade.Item;
                            ProductType = ProductTypeSimpleEnum.FxSwap;
                            PriceableProduct = new FxSwapPricer(fxSwap, BaseParty);
                            ProductReporter = new FxSwapReporter();
                        }
                        break;
                    case ItemChoiceType15.bulletPayment:
                        {
                            if (trade.Item is BulletPayment bullet)
                            {
                                IBusinessCalendar paymentCalendar = null;
                                if (firstCalendarPair != null)
                                {
                                    paymentCalendar = firstCalendarPair.Second;
                                }
                                //The calendars
                                if (paymentCalendar == null)
                                {
                                    if (bullet.payment.paymentDate != null)
                                    {
                                        var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(bullet.payment.paymentDate, ItemsChoiceType.dateAdjustments, out object dateAdjustments);
                                        if (containsPaymentDateAdjustments && dateAdjustments != null)
                                        {
                                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ((BusinessDayAdjustments)dateAdjustments).
                                                                                                      businessCenters, nameSpace);
                                        }
                                    }
                                }
                                ProductType = ProductTypeSimpleEnum.BulletPayment;
                                PriceableProduct = new BulletPaymentPricer(bullet, BaseParty, paymentCalendar);
                                ProductReporter = new BulletPaymentReporter();
                            }
                        }
                        break;
                    case ItemChoiceType15.termDeposit:
                        {
                            var deposit = (TermDeposit)trade.Item;
                            ProductType = ProductTypeSimpleEnum.TermDeposit;
                            PriceableProduct = new TermDepositPricer(logger, cache, deposit, TradeProp.Party1);//The payment date must be correct before calling this!
                            ProductReporter = new TermDepositReporter();
                        }
                        break;
                    case ItemChoiceType15.swaption:
                        {
                            var interestRateSwaption = (Swaption)trade.Item;
                            ProductType = ProductTypeSimpleEnum.InterestRateSwaption;
                            PriceableProduct = new InterestRateSwaptionPricer(logger, cache, nameSpace, interestRateSwaption, BaseParty, forecastRateInterpolation);
                            ProductReporter = new InterestRateSwaptionReporter();
                        }
                        break;
                    case ItemChoiceType15.fxOption:
                        {
                            IBusinessCalendar fixingCalendar = null;
                            IBusinessCalendar paymentCalendar = null;
                            if (firstCalendarPair != null)
                            {
                                fixingCalendar = firstCalendarPair.First;
                                paymentCalendar = firstCalendarPair.Second;
                            }
                            var fxOption = (FxOption)trade.Item;
                            ProductType = ProductTypeSimpleEnum.FxOption;
                            PriceableProduct = new VanillaEuropeanFxOptionPricer(logger, cache, nameSpace, fixingCalendar, paymentCalendar, fxOption, BaseParty);
                            ProductReporter = new FxOptionLegReporter();
                        }
                        break;
                    default:
                        throw new NotSupportedException("Unsupported TradeType: " + TradeType);
                }
                //Adds the extra party info now required.
                PriceableProduct.OrderedPartyNames.Add(party1);
                PriceableProduct.OrderedPartyNames.Add(party2);
                //Check if collateralised
                if (trade.collateral != null)
                {
                    PriceableProduct.IsCollateralised = true;
                }
            }
        }

        #endregion

        #region Pricing

        ///<summary>
        /// Prices the trade.
        ///</summary>
        ///<returns></returns>
        public override List<ValuationReport> Price(List<IInstrumentControllerData> modelData, ValuationReportType reportType)
        {
            switch (reportType)
            {
                //Creates a valuation report for each market
                case ValuationReportType.Full: return modelData.Select(element => Price(element, reportType)).ToList();
                //
                //Creates a single valuation report with sensitivities populated.///TODO
                case ValuationReportType.Summary: return modelData.Select(element => Price(element, reportType)).ToList();
                //TODO Convert all the individual reports into a single summary report.
                //The default    
                default: return modelData.Select(element => Price(element, reportType)).ToList();
            }
        }

        ///<summary>
        /// Prices the trade.
        ///</summary>
        ///<returns></returns>
        public override ValuationReport Price(IInstrumentControllerData modelData, ValuationReportType reportType)
        {
            //Price.
            if (TradeHelper.IsImplementedProductType(ProductType))
            {
                // A new valuationReport.
                var valuationReport = new ValuationReport();
                //var valSet = new ValuationSet();
                InstrumentControllerBase priceableProduct = PriceableProduct;
                if (priceableProduct == null)
                    throw new ApplicationException("PriceableProduct is null!");
                //This makes sure the marketenvironment has curves in it, otherwise the pricer will not function.
                if (modelData.MarketEnvironment == null)
                    throw new ApplicationException("MarketEnvironment is null!");
                //Set the appropriate Multiplier based on the reporting party
                var result = new AssetValuation();
                var reportingParty = modelData.BaseCalculationParty.Id;
                if(BaseParty == TradeProp.Party1)
                {
                    if (reportingParty == TradeProp.Party1)
                    {
                        result = priceableProduct.Calculate(modelData);
                    }
                    if(Parties[0].partyName.Value == reportingParty)
                    {
                        result = priceableProduct.Calculate(modelData);
                    }
                    if (Parties[1].partyName.Value == reportingParty || reportingParty == TradeProp.Party2)
                    {
                        priceableProduct.Multiplier = -1;
                        result = priceableProduct.Calculate(modelData);
                        priceableProduct.Multiplier = 1;
                    }
                }
                if (BaseParty == TradeProp.Party2)
                {
                    if (reportingParty == TradeProp.Party2)
                    {
                        result = priceableProduct.Calculate(modelData);
                    }
                    if (Parties[1].partyName.Value == reportingParty)
                    {
                        result = priceableProduct.Calculate(modelData);
                    }
                    if (Parties[0].partyName.Value == reportingParty || reportingParty == TradeProp.Party1)
                    {
                        priceableProduct.Multiplier = -1;
                        result = priceableProduct.Calculate(modelData);
                        priceableProduct.Multiplier = 1;
                    }
                }
                if (modelData.IsReportingCounterpartyRequired)
                {
                    priceableProduct.Multiplier = 0;
                    result = priceableProduct.Calculate(modelData);
                    priceableProduct.Multiplier = 1;
                }              
                var valSet = new ValuationSet { assetValuation = new[] { result } };
                //The tradevaluation item.
                var trade = new Trade { id = TradeIdentifier.UniqueIdentifier, tradeHeader = TradeHeader };
                //Checks to see if the deatil data is required and if so builds the product.//TODO Add other ItemChoice types.e.g. Fra
                if (reportType == ValuationReportType.Full)
                {
                    var item = PriceableProduct.BuildTheProduct();
                    trade.Item = item;
                    trade.ItemElementName = trade.GetTradeTypeFromItem();
                }
                var tradeValuationItem = new TradeValuationItem {Items = new object[] {trade}, valuationSet = valSet};
                valuationReport.tradeValuationItem = new[] { tradeValuationItem };
                return valuationReport;
            }
            throw new NotSupportedException("Product pricing is not supported!");
        }

        public static IInstrumentControllerData CreateInstrumentModelData(List<string> metrics, DateTime baseDate, IMarketEnvironment market, string reportingCurrency, string baseParty)
        {
            var bav = new AssetValuation();
            var currency = CurrencyHelper.Parse(reportingCurrency);
            var quotes = new Quotation[metrics.Count];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = QuotationHelper.Create(0.0m, metric, "DecimalValue", baseDate);
                index++;
            }
            bav.quote = quotes;
            return new InstrumentControllerData(bav, market, baseDate, currency, new PartyIdentifier(baseParty));
        }

        #endregion

        #region Implementation of IPriceable

        /// <summary>
        /// Returns the npv.
        /// </summary>
        /// <param name="reportingParty"></param>
        /// <param name="baseCurrency"></param>
        /// <param name="valuationDate"></param>
        /// <param name="market"></param>
        /// <returns></returns>
        public double GetNPV(string reportingParty, string baseCurrency, DateTime valuationDate, IMarketEnvironment market)
        {
            var modelData = CreateInstrumentModelData(new List<string> { InstrumentMetrics.NPV.ToString() }, valuationDate, market, baseCurrency, reportingParty);
            var av = PriceableProduct.Calculate(modelData);
            return (double)av.quote[0].value;
        }

        /// <summary>
        /// Prices the product.
        /// </summary>
        /// <param name="reportingParty"></param>
        /// <param name="valuationDate"></param>
        /// <param name="market"></param>
        /// <returns></returns>
        public double GetParRate(string reportingParty, DateTime valuationDate, IMarketEnvironment market)
        {
            var modelData = CreateInstrumentModelData(new List<string> { InstrumentMetrics.ImpliedQuote.ToString() }, valuationDate, market, PriceableProduct.PaymentCurrencies[0], reportingParty);
            var av = PriceableProduct.Calculate(modelData);
            return (double)av.quote[0].value;
        }

        #endregion

        #region ITradePricer Interface

        /// <summary>
        /// REturns the report for that particluar product type.
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        public override object DoReport(InstrumentControllerBase instrument)
        {
            return ProductReporter.DoReport(instrument);
        }

        /// <summary>
        /// REturns the report for that particluar product type.
        /// </summary>
        /// <param name="instrument"></param>
        /// <returns></returns>
        public override List<object[]> DoExpectedCashflowReport(InstrumentControllerBase instrument)
        {
            return ProductReporter.DoExpectedCashflowReport(instrument);
        }

        /// <summary>
        /// REturns the report for that particluar product type.
        /// </summary>
        /// <param name="product">The product.</param>
        /// <param name="properties">The properties.</param>
        /// <returns></returns>
        public override object[,] DoReport(Product product, NamedValueSet properties)
        {
            return ProductReporter.DoReport(product, properties);
        }

        /// <summary>
        /// REturns the report for that particluar product type.
        /// </summary>
        /// <returns></returns>
        public override object[,] DoXLReport(InstrumentControllerBase instrument)
        {
            return ProductReporter.DoXLReport(instrument);
        }
        
        #endregion

        #region IProduct

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            return PriceableProduct.BuildTheProduct();
        }

        #endregion
    }
}
