#region Using directives

using System;
using System.Linq;
using FpML.V5r3.Codes;
using FpML.V5r3.Helpers;
using Orion.Util.Helpers;

#endregion

namespace FpML.V5r3.Confirmation
{
    public class ProductTypeHelper
    {
        public static TradeTypeEnum TradeTypeHelper(Product product)
        {
            if (product as Swap != null) return TradeTypeEnum.swap;
            if (product as TermDeposit != null) return TradeTypeEnum.termDeposit;
            if (product as BulletPayment != null) return TradeTypeEnum.bulletPayment;
            if (product as BondOption != null) return TradeTypeEnum.bondOption;
            if (product as BrokerEquityOption != null) return TradeTypeEnum.brokerEquityOption;
            if (product as CapFloor != null) return TradeTypeEnum.capFloor;
            if (product as CommodityForward != null) return TradeTypeEnum.commodityForward;
            if (product as CommodityOption != null) return TradeTypeEnum.commodityOption;
            if (product as CommoditySwap != null) return TradeTypeEnum.commoditySwap;
            if (product as CorrelationSwap != null) return TradeTypeEnum.correlationSwap;
            if (product as CreditDefaultSwap != null) return TradeTypeEnum.creditDefaultSwap;
            if (product as CreditDefaultSwapOption != null) return TradeTypeEnum.creditDefaultSwapOption;
            if (product as DividendSwapTransactionSupplement != null) return TradeTypeEnum.dividendSwapTransactionSupplement;
            if (product as EquityForward != null) return TradeTypeEnum.equityForward;
            if (product as EquityOption != null) return TradeTypeEnum.equityOption;
            if (product as EquityOptionTransactionSupplement != null) return TradeTypeEnum.equityOptionTransactionSupplement;
            if (product as ReturnSwap != null) return TradeTypeEnum.returnSwap;
            if (product as EquitySwapTransactionSupplement != null) return TradeTypeEnum.equitySwapTransactionSupplement;
            if (product as Fra != null) return TradeTypeEnum.fra;
            if (product as FxDigitalOption != null) return TradeTypeEnum.fxDigitalOption;
            if (product as FxOption != null) return TradeTypeEnum.fxOption;
            if (product as FxSingleLeg != null) return TradeTypeEnum.fxSingleLeg;
            if (product as FxSwap != null) return TradeTypeEnum.fxSwap;
            if (product as Strategy != null) return TradeTypeEnum.strategy;
            if (product as Swaption != null) return TradeTypeEnum.swaption;
            if (product as VarianceOptionTransactionSupplement != null) return TradeTypeEnum.varianceOptionTransactionSupplement;
            if (product as VarianceSwap != null) return TradeTypeEnum.varianceSwap;
            if (product as VarianceSwapTransactionSupplement != null) return TradeTypeEnum.varianceSwapTransactionSupplement;
            return TradeTypeEnum.swap;
        }

        public static ProductType Create(string productType)
        {
            var result = new ProductType { Value = productType };
            return result;
        }
    }

    public static class PartyFactory
    {
        public static Party Create(string id)
        {
            var partyOrTradeSideReference = new Party { id = id };
            return partyOrTradeSideReference;
        }

        public static Party Create(string id, string partyName)
        {
            var partyOrTradeSideReference = new Party { id = id };
            var party = new PartyId {Value = partyName};
            partyOrTradeSideReference.partyId = new[] { party };
            return partyOrTradeSideReference;
        }
    }

    //public class FxLegHelper
    //{
    //    public static FxLeg CreateForward(string exchangeCurrency1PayPartyReference,
    //        string exchangeCurrency2PayPartyReference,
    //        decimal exchangeCurrency1Amount,
    //        string exchangeCurrency1,
    //        string exchangeCurrency2,
    //        QuoteBasisEnum quoteBasis,
    //        DateTime valueDate,
    //        Decimal spotRate,
    //        Decimal forwardRate,
    //        Decimal forwardPoints)
    //    {
    //        decimal exchange2Amount;
    //        if (quoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
    //        {
    //            exchange2Amount = exchangeCurrency1Amount * forwardRate;
    //        }
    //        else
    //        {
    //            exchange2Amount = exchangeCurrency1Amount / forwardRate;
    //        }
    //        var fxforward = new FxLeg
    //        {
    //            exchangedCurrency1 = PaymentHelper.Create(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1, exchangeCurrency1Amount),
    //            exchangedCurrency2 = PaymentHelper.Create(exchangeCurrency2PayPartyReference, exchangeCurrency1PayPartyReference, exchangeCurrency2, exchange2Amount),
    //            Items = new[] { valueDate },
    //            exchangeRate = ExchangeRateHelper.Parse(exchangeCurrency1, exchangeCurrency2,
    //            quoteBasis, spotRate, forwardRate, forwardPoints)

    //        };

    //        return fxforward;
    //    }

    //    public static FxLeg CreateSpot(string exchangeCurrency1PayPartyReference,
    //        string exchangeCurrency2PayPartyReference,
    //        decimal exchangeCurrency1Amount,
    //        string exchangeCurrency1,
    //        string exchangeCurrency2,
    //        QuoteBasisEnum quoteBasis,
    //        DateTime valueDate,
    //        Decimal spotRate)
    //    {
    //        decimal exchange2Amount;
    //        if (quoteBasis == QuoteBasisEnum.Currency2PerCurrency1)
    //        {
    //            exchange2Amount = exchangeCurrency1Amount * spotRate;
    //        }
    //        else
    //        {
    //            exchange2Amount = exchangeCurrency1Amount / spotRate;
    //        }
    //        var fxforward = new FxLeg
    //        {
    //            exchangedCurrency1 = PaymentHelper.Create(exchangeCurrency1PayPartyReference, exchangeCurrency2PayPartyReference, exchangeCurrency1, exchangeCurrency1Amount),
    //            exchangedCurrency2 = PaymentHelper.Create(exchangeCurrency2PayPartyReference, exchangeCurrency1PayPartyReference, exchangeCurrency2, exchange2Amount),
    //            Items = new[] { valueDate },
    //            exchangeRate = ExchangeRateHelper.Parse(exchangeCurrency1, exchangeCurrency2,
    //            quoteBasis, spotRate)

    //        };

    //        return fxforward;
    //    }

    //}

    public static class ScheduleFactory
    {
        public static Schedule Create(Decimal initialValue)
        {
            var result = new Schedule {initialValue = initialValue};
            return result;
        }
    }

    public class IdentifiedDateHelper
    {
        public static IdentifiedDate Create(string id, DateTime value)
        {
            var identifiedDate = new IdentifiedDate {id = id, Value = value};
            return identifiedDate;
        }

        public static IdentifiedDate Create(DateTime value)
        {
            var identifiedDate = new IdentifiedDate {Value = value};
            return identifiedDate;
        }
    }

    public class PartyTradeIdentifierHelper
    {
        public static PartyTradeIdentifier Parse(string tradeId, string partyReference)
        {
            var result = new PartyTradeIdentifier {Items = new object[2]};
            result.Items[0] = PartyReferenceHelper.Parse(partyReference);
            result.Items[1] = TradeIdHelper.Parse(tradeId);
            return result;
        }
    }

    public class TradeIdHelper
    {
        public static TradeId Parse(string tradeId, string id, string tradeIdScheme)
        {
            var result = new TradeId {id = id, tradeIdScheme = tradeIdScheme, Value = tradeId};
            return result;
        }

        public static TradeId Parse(string tradeId)
        {
            var result = new TradeId {Value = tradeId};
            return result;
        }
    }

    public class XiborNodeStructHelper
    {
        public static XiborNodeStruct Parse(
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string term,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var instrument = new XiborNodeStruct
            {
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                RateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction)
            };
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            instrument.SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo);

            return instrument;
        }
    }

    public class InstrumentNodeHelper
    {
        public static XiborNodeStruct CreateXibor(
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string term,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var instrument = new XiborNodeStruct
            {
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                RateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo)
            };


            return instrument;
        }

        public static DepositNodeStruct CreateDeposit(
            string instrumentId,
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string term,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var deposit = DepositHelper.Parse(instrumentId, currency, dayCountFraction, term);

            var instrument = new DepositNodeStruct
            {
                Deposit = deposit,
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                UnderlyingRateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo)
            };


            return instrument;
        }

        public static BankBillNodeStruct CreateBankBill(
            string instrumentId,
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string term,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var deposit = DepositHelper.Parse(instrumentId, currency, dayCountFraction, term);

            var instrument = new BankBillNodeStruct
            {
                Deposit = deposit,
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                UnderlyingRateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo)
            };


            return instrument;
        }

        public static FxSpotNodeStruct CreateFxSpot(
            string currency1,
            string currency2,
            string quotationBasis,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var quoteBasis = EnumHelper.Parse<QuoteBasisEnum>(quotationBasis);

            var instrument = new FxSpotNodeStruct
            {
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo),
                QuotedCurrencyPair = QuotedCurrencyPair.Create(currency1, currency2, quoteBasis)
            };

            return instrument;
        }

        public static FxSpotNodeStruct CreateCommoditySpot(
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var instrument = new FxSpotNodeStruct
            {
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo)
            };

            return instrument;
        }

        public static SimpleFraNodeStruct CreateSmpleFra(
            string instrumentId,
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string startTerm,
            string endTerm,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var fra = SimpleFraHelper.Parse(instrumentId, currency, dayCountFraction, startTerm, endTerm);

            var instrument = new SimpleFraNodeStruct
            {
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                RateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo),
                SimpleFra = fra
            };

            return instrument;
        }

        public static SimpleBillFraNodeStruct CreateSmpleBillFra(
            string instrumentId,
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string startTerm,
            string endTerm,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var fra = SimpleFraHelper.Parse(instrumentId, currency, dayCountFraction, startTerm, endTerm);

            var instrument = new SimpleBillFraNodeStruct
            {
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                RateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo),
                SimpleFra = fra
            };

            return instrument;
        }

        public static FraNodeStruct CreateFra(
            string productType,
            Decimal fixedRate,
            DateTime adjustedEffectiveDate,
            DateTime adjustedTerminationDate,
            DateTime paymentDate,
            string paymentBusinessDayConvention,
            string paymentBusinessCenters,
            Decimal notional,
            string floatingRateIndex,
            string currency,
            string dayCountFraction,
            string indexTenor,
            string numberOfDays,
            string fixingDays,
            string fixingDayTypeEnum,
            string fixingBusinessDayConvention,
            string fixingBusinessCenters,
            string dateRelativeTo,
            string fraDiscounting)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(fixingDayTypeEnum);

            var discountingType = EnumHelper.Parse<FraDiscountingEnum>(fraDiscounting);

            var fra = new Fra
            {
                adjustedTerminationDate = adjustedTerminationDate,
                calculationPeriodNumberOfDays = numberOfDays,
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                fixedRate = fixedRate,
                fixingDateOffset =
                    RelativeDateOffsetHelper.Create(fixingDays, dayType, fixingBusinessDayConvention,
                                                    fixingBusinessCenters, dateRelativeTo),
                floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex),
                fraDiscounting = discountingType,
                indexTenor = new Period[1]
            };

            fra.indexTenor[0] = PeriodHelper.Parse(indexTenor);
            fra.notional = MoneyHelper.GetAmount(notional, currency);

            fra.adjustedEffectiveDate = new RequiredIdentifierDate
            {
                Value = adjustedEffectiveDate,
                id = "EffectiveDate"
            };

            fra.paymentDate = new AdjustableDate
            {
                unadjustedDate = IdentifiedDateHelper.Create("PaymentDate", paymentDate),
                dateAdjustments =
                    BusinessDayAdjustmentsHelper.Create(paymentBusinessDayConvention,
                                                        paymentBusinessCenters)
            };

            var instrument = new FraNodeStruct
            {
                Fra = fra
            };

            return instrument;
        }

        public static ZeroRateNodeStruct CreateZeroRate(
            Boolean adjustDates,
            string businessDayConvention,
            string businessCenters,
            string dayCountFraction,
            string frequency)
        {
            var instrument = new ZeroRateNodeStruct
            {
                AdjustDates = adjustDates,
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                DayCountFraction =
                    DayCountFractionHelper.Parse(dayCountFraction),
                CompoundingFrequency = CompoundingFrequency.Parse(frequency)
            };

            return instrument;
        }

        public static CommodityFutureNodeStruct CreateCommodityFuture(
            string clearanceSystem,
            string exchangeId,
            string businessDayConvention,
            string businessCenters,
            string currency,
            DateTime? expiry)
        {
            var future = new Future
            {
                currency = new IdentifiedCurrency { Value = currency },
                clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                exchangeId = ExchangeIdHelper.Parse(exchangeId)
            };

            if (expiry != null)
            {
                future.maturity = (DateTime)expiry;
                future.maturitySpecified = true;
            }

            var instrument = new CommodityFutureNodeStruct
            {
                Future = future,
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters)
            };

            return instrument;
        }

        public static IRFutureNodeStruct CreateIRFuture(
            string assetId,
            string clearanceSystem,
            string exchangeId,
            string businessDayConvention,
            string businessCenters,
            string currency,
            DateTime? expiry,
            string floatingRateIndex,
            string dayCountFraction,
            string term)
        {
            var future = new Future
            {
                currency = new IdentifiedCurrency { Value = currency },
                clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                exchangeId = ExchangeIdHelper.Parse(exchangeId)
            };

            if (expiry != null)
            {
                future.maturity = (DateTime)expiry;
                future.maturitySpecified = true;
            }

            var instrument = new IRFutureNodeStruct
            {
                RateIndex =
                    RateIndexHelper.Parse(assetId, floatingRateIndex, currency,
                                          dayCountFraction, term, term),
                Future = future,
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters)
            };

            return instrument;
        }

        public static RateOptionNodeStruct CreateRateOption(
            string instrumentId,
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string startTerm,
            string endTerm,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo,
            string resetDays,
            string resetDayTypeEnum,
            string resetBusinessDayConvention,
            string resetBusinessCenters,
            string resetDateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var resetDayType = EnumHelper.Parse<DayTypeEnum>(resetDayTypeEnum);

            var rateOption = SimpleFraHelper.Parse(instrumentId, currency, dayCountFraction, startTerm, endTerm);

            var instrument = new RateOptionNodeStruct
            {
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                RateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo),
                ResetDateAdjustment = RelativeDateOffsetHelper.Create(resetDays, resetDayType, resetBusinessDayConvention, resetBusinessCenters, resetDateRelativeTo),
                SimpleRateOption = rateOption
            };

            return instrument;
        }

        public static SimpleIRSwapNodeStruct CreateSimpleIRSwap(
            string instrumentId,
            string assetId,
            decimal fixedRate,
            decimal notional,
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string term,
            string paymentFrequency,
            string discountingType,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var discountType = EnumHelper.Parse<DiscountingTypeEnum>(discountingType);

            var money = MoneyHelper.GetAmount(notional, currency);

            var dayCount = DayCountFractionHelper.Parse(dayCountFraction);

            var irswap = SimpleIrsHelper.Parse(instrumentId, currency, dayCountFraction, term, paymentFrequency, assetId);

            var instrument = new SimpleIRSwapNodeStruct
            {
                DateAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                UnderlyingRateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo),
                Calculation = CalculationFactory.CreateFixed(fixedRate, money, dayCount, discountType),
                SimpleIRSwap = irswap
            };

            return instrument;
        }

        public static SimpleIRCapNodeStruct CreateSimpleIRCap(
            string instrumentId,
            string assetId,
            string assetReference,
            decimal fixedRate,
            decimal notional,
            string floatingRateIndex,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            string term,
            string paymentFrequency,
            string discountingType,
            string spotDays,
            string spotDayTypeEnum,
            string spotBusinessDayConvention,
            string spotBusinessCenters,
            string dateRelativeTo,
            Boolean includeFirstCaplet)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(spotDayTypeEnum);

            var discountType = EnumHelper.Parse<DiscountingTypeEnum>(discountingType);

            var money = MoneyHelper.GetAmount(notional, currency);

            var dayCount = DayCountFractionHelper.Parse(dayCountFraction);

            var ircap = SimpleIrsHelper.Parse(instrumentId, currency, dayCountFraction, term, paymentFrequency, assetId);

            var assetRef = new AssetReference { href = assetReference };

            var instrument = new SimpleIRCapNodeStruct
            {
                DateAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                UnderlyingRateIndex =
                    RateIndexHelper.Parse(floatingRateIndex, currency,
                                          dayCountFraction),
                SpotDate = RelativeDateOffsetHelper.Create(spotDays, dayType, spotBusinessDayConvention, spotBusinessCenters, dateRelativeTo),
                Calculation = CalculationFactory.CreateFixed(fixedRate, money, dayCount, discountType),
                SimpleIRCap = ircap,
                IncludeFirstCaplet = includeFirstCaplet,
                AssetReference = assetRef
            };

            return instrument;
        }

        public static BondNodeStruct CreateBond(
            string instrumentId,
            string description,
            string assetId,
            string clearanceSystem,
            string exchangeId,
            double couponRate,
            string couponType,
            double notional,
            string issuerName,
            string businessDayConvention,
            string businessCenters,
            string currency,
            string dayCountFraction,
            DateTime maturity,
            string creditSeniority,
            string paymentFrequency,
            string settlementDays,
            string settlementDayTypeEnum,
            string settlementBusinessDayConvention,
            string settlementBusinessCenters,
            string dateRelativeTo,
            string exDivDays,
            string exDivDayTypeEnum,
            string exDivBusinessDayConvention,
            string exDivBusinessCenters,
            string exDivdateRelativeTo)
        {
            var dayType = EnumHelper.Parse<DayTypeEnum>(settlementDayTypeEnum);

            var exdayType = EnumHelper.Parse<DayTypeEnum>(exDivDayTypeEnum);

            var bond = BondHelper.Parse(instrumentId, clearanceSystem, couponType, couponRate, currency, notional, maturity,
                paymentFrequency, dayCountFraction, creditSeniority, assetId, description, exchangeId, issuerName);

            var instrument = new BondNodeStruct
            {
                BusinessDayAdjustments =
                    BusinessDayAdjustmentsHelper.Create(businessDayConvention, businessCenters),
                ExDivDate = RelativeDateOffsetHelper.Create(exDivDays, exdayType, exDivBusinessDayConvention, exDivBusinessCenters, exDivdateRelativeTo),
                SettlementDate = RelativeDateOffsetHelper.Create(settlementDays, dayType, settlementBusinessDayConvention, settlementBusinessCenters, dateRelativeTo),
                Bond = bond
            };

            return instrument;
        }
    }

    public class InstrumentHelper
    {
        public static Instrument Create(
            string assetId,
            string currency,
            XiborNodeStruct xibor)
        {
            var instrument = new Instrument { AssetType = assetId, Currency = CurrencyHelper.Parse(currency), InstrumentNodeItem = xibor };

            return instrument;
        }

        public static Instrument Create(
            string assetId,
            string currency,
            string extraItem,
            XiborNodeStruct xibor)
        {
            var instrument = new Instrument
            {
                AssetType = assetId,
                Currency = CurrencyHelper.Parse(currency),
                ExtraItem = extraItem,
                InstrumentNodeItem = xibor
            };

            return instrument;
        }

        public static Instrument Create(
            string assetId,
            string currency,
            InstrumentNode asset)
        {
            var instrument = new Instrument { AssetType = assetId, Currency = CurrencyHelper.Parse(currency), InstrumentNodeItem = asset };

            return instrument;
        }

        public static Instrument Create(
            string assetId,
            string currency,
            string extraItem,
            InstrumentNode instrumentNode)
        {
            var instrument = new Instrument
            {
                AssetType = assetId,
                Currency = CurrencyHelper.Parse(currency),
                ExtraItem = extraItem,
                InstrumentNodeItem = instrumentNode
            };

            return instrument;
        }
    }

    public class FxRateAssetHelper
    {
        /// <summary>
        /// Creates an Fx asset.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <param name="clearanceSystem"></param>
        /// <param name="assetCurrency"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="fixingTime"></param>
        /// <param name="businessCeneterAsString"></param>
        /// <param name="primaryRateSourceProvider"></param>
        /// <param name="exchangeId"></param>
        /// <returns></returns>
        public static FxRateAsset Parse(string instrumentId,
            string clearanceSystem,
            string assetCurrency,
            string currency1,
            string currency2,
            QuoteBasisEnum quoteBasis,
            DateTime fixingTime,
            string businessCeneterAsString,
            string primaryRateSourceProvider,
            string exchangeId)
        {
            var fxRateAsset = new FxRateAsset
            {
                clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                currency = new IdentifiedCurrency { Value = assetCurrency },
                exchangeId = ExchangeIdHelper.Parse(exchangeId),
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                quotedCurrencyPair =
                    QuotedCurrencyPair.Create(currency1, currency2, quoteBasis),
                rateSource = FxSpotRateSourceHelper.Parse(businessCeneterAsString,
                                                          primaryRateSourceProvider),
                id = instrumentId
            };

            return fxRateAsset;
        }

        /// <summary>
        /// Creates an Fx asset.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <param name="clearanceSystem"></param>
        /// <param name="currency1"></param>
        /// <param name="currency2"></param>
        /// <param name="quoteBasis"></param>
        /// <param name="fixingTime"></param>
        /// <param name="businessCeneterAsString"></param>
        /// <param name="primaryRateSourceProvider"></param>
        /// <param name="exchangeId"></param>
        /// <returns></returns>
        public static FxRateAsset Parse(string instrumentId,
            string clearanceSystem,
            string currency1,
            string currency2,
            QuoteBasisEnum quoteBasis,
            DateTime fixingTime,
            string businessCeneterAsString,
            string primaryRateSourceProvider,
            string exchangeId)
        {
            var fxRateAsset = new FxRateAsset
            {
                clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                currency = new IdentifiedCurrency { Value = currency1 },
                exchangeId = ExchangeIdHelper.Parse(exchangeId),
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                quotedCurrencyPair =
                    QuotedCurrencyPair.Create(currency1, currency2, quoteBasis),
                rateSource = FxSpotRateSourceHelper.Parse(businessCeneterAsString,
                                                          primaryRateSourceProvider),
                id = instrumentId
            };

            return fxRateAsset;
        }
    }

    /// <summary>
    /// A helper class to create a FxSpotRateSource.
    /// </summary>
    public class FxSpotRateSourceHelper
    {
        public static FxSpotRateSource Parse(DateTime fixingTime, string businessCeneterAsString,
            string primaryRateSourceProvider, string primaryRateSourcePage, string primaryRateSource,
            string secondaryRateSourceProvider, string secondaryRateSourcePage, string secondaryRateSource)
        {
            var fxSpotRateSource = new FxSpotRateSource
            {
                fixingTime = BusinessCenterTimeHelper.Parse(businessCeneterAsString, fixingTime),
                primaryRateSource = InformationSourceHelper.Create(primaryRateSourceProvider, primaryRateSource, primaryRateSourcePage),
                secondaryRateSource = InformationSourceHelper.Create(secondaryRateSourceProvider, secondaryRateSourcePage, secondaryRateSource),
            };

            return fxSpotRateSource;
        }

        public static FxSpotRateSource Parse(string businessCeneterAsString,
            string primaryRateSourceProvider)
        {
            var fxSpotRateSource = new FxSpotRateSource
            {
                fixingTime = BusinessCenterTimeHelper.Parse(businessCeneterAsString),
                primaryRateSource = InformationSourceHelper.Create(primaryRateSourceProvider),
            };

            return fxSpotRateSource;
        }
    }

    public static class RateObservationHelper
    {
        public static RateObservation Parse(DateTime adjustedFixingDate, Decimal observedRate, string observationWeight)
        {
            var result = new RateObservation
            {
                adjustedFixingDate = adjustedFixingDate,
                adjustedFixingDateSpecified = true,
                observedRate = observedRate,
                observedRateSpecified = true,
                observationWeight = observationWeight,
                resetDate = adjustedFixingDate,
                resetDateSpecified = true
            };

            return result;
        }

        public static RateObservation Parse(DateTime adjustedFixingDate, Decimal? observedRate, string observationWeight)
        {
            RateObservation result = observedRate != null ? new RateObservation
            {
                adjustedFixingDate = adjustedFixingDate,
                adjustedFixingDateSpecified = true,
                observedRate = (Decimal)observedRate,
                observedRateSpecified = true,
                observationWeight = observationWeight,
                resetDate = adjustedFixingDate,
                resetDateSpecified = true
            } : new RateObservation
            {
                adjustedFixingDate = adjustedFixingDate,
                adjustedFixingDateSpecified = true,
                observedRateSpecified = false,
                observationWeight = observationWeight,
                resetDate = adjustedFixingDate,
                resetDateSpecified = true
            };

            return result;
        }
    }

    public class SimpleFraHelper
    {
        public static SimpleFra Parse(string instrumentId,
            string clearanceSystem,
            string currency,
            string dayCountFraction,
            string startTerm,
            string endTerm,
            string assetId,
            string description,
            string exchangeId)
        {
            var simpleFra = new SimpleFra
            {
                clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                description = description,
                endTerm = PeriodHelper.Parse(endTerm),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                startTerm = PeriodHelper.Parse(startTerm),
                exchangeId = ExchangeIdHelper.Parse(exchangeId)
            };

            return simpleFra;
        }

        public static SimpleFra Parse(string instrumentId,
            string currency,
            string dayCountFraction,
            string startTerm,
            string endTerm)
        {
            var simpleFra = new SimpleFra
            {
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                endTerm = PeriodHelper.Parse(endTerm),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                startTerm = PeriodHelper.Parse(startTerm)
            };

            return simpleFra;
        }
    }

    public class SimpleIrsHelper
    {
        public static SimpleIRSwap Parse(string instrumentId,
            string clearanceSystem,
            string currency,
            string dayCountFraction,
            string term,
            string paymentFrequency,
            string assetId,
            string description,
            string exchangeId)
        {
            var simpleIRSwap = new SimpleIRSwap
            {
                clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                description = description,
                term = PeriodHelper.Parse(term),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                paymentFrequency = PeriodHelper.Parse(paymentFrequency),
                exchangeId = ExchangeIdHelper.Parse(exchangeId)
            };
            return simpleIRSwap;
        }

        public static SimpleIRSwap Parse(string instrumentId,
            string currency,
            string dayCountFraction,
            string term,
            string paymentFrequency,
            string assetId)
        {
            var simpleIRSwap = new SimpleIRSwap
            {
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                term = PeriodHelper.Parse(term),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                paymentFrequency = PeriodHelper.Parse(paymentFrequency),
            };

            return simpleIRSwap;
        }
    }

    public static class ForecastRateIndexHelper
    {
        public static ForecastRateIndex Parse(string floatingRateIndex, string tenor)
        {
            var result = new ForecastRateIndex
                             {
                                 floatingRateIndex = FloatingRateIndexHelper.Parse(floatingRateIndex),
                                 indexTenor = PeriodHelper.Parse(tenor)
                             };
            return result;
        }
    }

    public class DepositHelper
    {
        public static Deposit Parse(string instrumentId,
            string clearanceSystem,
            string currency,
            string dayCountFraction,
            string term,
            string paymentFrequency,
            string assetId,
            string description,
            string exchangeId)
        {
            var deposit = new Deposit
            {
                clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                description = description,
                term = PeriodHelper.Parse(term),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                paymentFrequency = PeriodHelper.Parse(paymentFrequency),
                exchangeId = ExchangeIdHelper.Parse(exchangeId)
            };

            return deposit;
        }

        public static Deposit Parse(string instrumentId,
            string currency,
            string dayCountFraction,
            string term,
            string paymentFrequency)
        {
            var deposit = new Deposit
            {
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                term = PeriodHelper.Parse(term),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                paymentFrequency = PeriodHelper.Parse(paymentFrequency)
            };

            return deposit;
        }

        public static Deposit Parse(string instrumentId,
            string currency,
            string dayCountFraction,
            string term)
        {
            var deposit = new Deposit
            {
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                term = PeriodHelper.Parse(term),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                paymentFrequency = PeriodHelper.Parse(term)
            };

            return deposit;
        }
    }

    public class CashHelper
    {
        public static Cash Parse(string instrumentId,
            string currency)
        {
            var cash = new Cash
            {
                currency = CurrencyHelper.Parse(currency),
                id = instrumentId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId)
            };
            return cash;
        }
    }

    public class LinkIdHelper
    {
        public static LinkId Parse(string id, string value)
        {
            var result = new LinkId {id = id, Value = value};
            return result;
        }
    }

    public class ExchangeIdHelper
    {
        //private readonly ExchangeId _exchangeId;

        //public ExchangeIdHelper(ExchangeId exchangeId)
        //{
        //    _exchangeId = exchangeId;
        //}

        //public ExchangeIdHelper()
        //{ }

        //public ExchangeIdHelper(string value, string exchangeIdScheme)
        //{
        //    var result = new ExchangeId {Value = value, exchangeIdScheme = exchangeIdScheme};
        //    _exchangeId = result;
        //}

        public static ExchangeId Parse(string s)
        {
            var result = new ExchangeId {Value = s};
            return result;
        }

        public static ExchangeId Copy(ExchangeId exchangeId)
        {
            var result = new ExchangeId { Value = exchangeId.Value };
            return result;
        }

        //public ExchangeId ToFpML
        //{
        //    get { return _exchangeId; }
        //}
    }

    public class InstrumentIdHelper
    {
        public static InstrumentId Parse(string instrumentId)
        {
            return new InstrumentId { Value = instrumentId };
        }
    }

    public class InstrumentIdArrayHelper
    {
        public static InstrumentId[] Parse(string instrumentId)
        {
            return new[] { InstrumentIdHelper.Parse(instrumentId) };
        }

        public static InstrumentId[] Parse(string[] instrumentIds)
        {
            return instrumentIds.Select(InstrumentIdHelper.Parse).ToArray();
        }
    }

    public class FutureHelper
    {
        public static Future Parse(string instrumentId,
            string clearanceSystem,
            string currency,
            string multiplier,
            string definition,
            string description,
            string futureContractReference,
            string exchangeId)
        {
            var future = new Future
                             {
                                 clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                                 currency = new IdentifiedCurrency {Value = currency},
                                 description = description,
                                 exchangeId = ExchangeIdHelper.Parse(exchangeId),
                                 futureContractReference = futureContractReference,
                                 id = instrumentId,
                                 instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                                 multiplier = multiplier
                             };

            return future;
        }
    }

    public class CreditSeniorityHelper
    {
        public static CreditSeniority Parse(string creditSeniorityAsString)
        {
            var creditSeniorityEnum = EnumHelper.Parse<CreditSeniorityEnum>(creditSeniorityAsString, true);
            return ToCreditSeniority(creditSeniorityEnum);
        }

        public static CreditSeniority ToCreditSeniority(CreditSeniorityEnum creditSeniorityEnum)
        {
            string creditSeniorityAsString = CreditSeniorityScheme.GetEnumString(creditSeniorityEnum);
            var result = new CreditSeniority { Value = creditSeniorityAsString };
            return result;
        }
    }

    public class CouponTypeHelper
    {
        public static CouponType Parse(string couponTypeAsString)
        {
            var couponTypeEnum = EnumHelper.Parse<CouponTypeEnum>(couponTypeAsString, true);
            return ToCouponType(couponTypeEnum);
        }

        public static CouponType ToCouponType(CouponTypeEnum couponTypeEnum)
        {
            string couponTypeAsString = CouponTypeScheme.GetEnumString(couponTypeEnum);
            var result = new CouponType { Value = couponTypeAsString };
            return result;
        }
    }

    public class ClearanceSystemHelper
    {
        public static ClearanceSystem Parse(string s)
        {
            var result = new ClearanceSystem { Value = s };
            return result;
        }
    }

    public class BondHelper
    {
        /// <summary>
        /// Creates a bond.
        /// </summary>
        /// <param name="instrumentId"></param>
        /// <param name="clearanceSystem"></param>
        /// <param name="couponType"></param>
        /// <param name="couponRate"></param>
        /// <param name="currency"></param>
        /// <param name="faceAmount"></param>
        /// <param name="maturity"></param>
        /// <param name="paymentFrequency"></param>
        /// <param name="dayCountFraction"></param>
        /// <param name="creditSeniority"></param>
        /// <param name="assetId"></param>
        /// <param name="description"></param>
        /// <param name="exchangeId"></param>
        /// <param name="issuerName"></param>
        /// <returns></returns>
        public static Bond Parse(string instrumentId, string clearanceSystem, string couponType, 
            double couponRate, string currency, double faceAmount, DateTime maturity, 
            string paymentFrequency, string dayCountFraction,
            string creditSeniority, string assetId,
            string description, string exchangeId, string issuerName)
        {
            var bond = new Bond
                           {
                               clearanceSystem = ClearanceSystemHelper.Parse(clearanceSystem),
                               couponType = CouponTypeHelper.Parse(couponType),
                               couponRateSpecified = true,
                               couponRate = ((decimal) couponRate),
                               currency = new IdentifiedCurrency { Value = currency },
                               dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                               description = description,
                               exchangeId = ExchangeIdHelper.Parse(exchangeId),
                               faceAmount = ((decimal) faceAmount),
                               faceAmountSpecified = true,
                               id = assetId,
                               instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                               maturity = maturity,
                               maturitySpecified = true,
                               parValue = 100.0m,
                               Item = issuerName,
                               parValueSpecified = false,
                               paymentFrequency = PeriodHelper.Parse(paymentFrequency),
                               seniority = CreditSeniorityHelper.Parse(creditSeniority)
                           };
            return bond;
        }

        /// <summary>
        /// Creates a bond.
        /// </summary>
        /// <param name="instrumentId">This is an array of values</param>
        /// <param name="clearanceSystem">Can be null.</param>
        /// <param name="couponType">CouponTypeEnum -> Can be: Fixed, Floating or Struct</param>
        /// <param name="couponRate"></param>
        /// <param name="currency"></param>
        /// <param name="faceAmount"></param>
        /// <param name="maturity"></param>
        /// <param name="paymentFrequency"></param>
        /// <param name="dayCountFraction"></param>
        /// <param name="creditSeniority">CreditSeniorityEnum: Senior, SubTier1, SubUpperTier2, SubLowerTier2, SubTier3</param>
        /// <param name="assetId"></param>
        /// <param name="description">This can be null</param>
        /// <param name="exchangeId">Cannot be null.</param>
        /// <param name="issuerName"></param>
        /// <returns></returns>
        public static Bond Create(string[] instrumentId, string clearanceSystem, string couponType,
            decimal? couponRate, string currency, double faceAmount, DateTime maturity,
            string paymentFrequency, string dayCountFraction,
            string creditSeniority, string assetId,
            string description, string exchangeId, string issuerName)
        {
            ClearanceSystem clearanceSys = null;
            if(clearanceSystem != null)
            {
                clearanceSys = ClearanceSystemHelper.Parse(clearanceSystem);
            }
            var bond = new Bond
            {
                clearanceSystem = clearanceSys,
                couponType = CouponTypeHelper.Parse(couponType),
                currency = new IdentifiedCurrency { Value = currency },
                dayCountFraction = DayCountFractionHelper.Parse(dayCountFraction),
                description = description,
                exchangeId = ExchangeIdHelper.Parse(exchangeId),
                faceAmount = ((decimal)faceAmount),
                faceAmountSpecified = true,
                id = assetId,
                instrumentId = InstrumentIdArrayHelper.Parse(instrumentId),
                maturity = maturity,
                maturitySpecified = true,
                parValue = 100.0m,
                Item = issuerName,
                parValueSpecified = false,
                paymentFrequency = PeriodHelper.Parse(paymentFrequency),
                seniority = CreditSeniorityHelper.Parse(creditSeniority)
            };
            if (couponRate != null)
            {
                bond.couponRate = (decimal)couponRate;
                bond.couponRateSpecified = true;
            }
            return bond;
        }
    }

}
