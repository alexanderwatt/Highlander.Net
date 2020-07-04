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
using System.Linq;

#endregion

namespace Highlander.Reporting.V5r3
{
    /// <summary>
    /// Class simplifies strongly typed access to fields of the classes generated by Xsd.Exe.
    ///  
    /// Although above mentioned tool produces good results most of the time, sometimes 
    /// (when the schema is more complex than the tool could handle - e.g. mapping a mutually exclusive fields) 
    /// it generated "untyped" fields  with names which don't reflect the function of the data field:
    /// 
    /// see the example:
    /// <example>
    ///[System.Xml.Serialization.XmlElementAttribute("effectiveDate", typeof(AdjustableDate))]
    ///[System.Xml.Serialization.XmlElementAttribute("relativeEffectiveDate", typeof(AdjustedRelativeDateOffset))]
    ///public object Item {
    ///get {
    ///        return this.itemField;
    ///     }
    ///            set {
    ///                 this.itemField = value;
    ///            }
    ///        }
    ///        
    ///        /// <remarks/>
    ///        [System.Xml.Serialization.XmlElementAttribute("relativeTerminationDate", typeof(RelativeDateOffset))]
    ///        [System.Xml.Serialization.XmlElementAttribute("terminationDate", typeof(AdjustableDate))]
    ///        public object Item1 {
    ///            get {
    ///                return this.item1Field;
    ///            }
    ///            set {
    ///                this.item1Field = value;
    ///            }
    ///        }
    /// 
    /// 
    /// NB: Although underscores in member names are against our code style naming convention, 
    /// 
    /// I left it as it is because otherwise the intention of these methods would be difficult 
    /// to understand.
    /// </example>
    /// </summary>
    public  static class XsdClassesFieldResolver
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tradeValuationItem"></param>
        /// <returns></returns>
        public static Trade[] TradeValuationItemGetTradeArray(TradeValuationItem tradeValuationItem)
        {
            return tradeValuationItem.Items.Cast<Trade>().ToArray();
        }

        #region Trade GetXXX/SetXXX methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="swap"></param>
        public static void TradeSetFxSingleLeg(Trade trade, FxSingleLeg swap)
        {
            trade.Item = swap;
            trade.ItemElementName = ItemChoiceType15.fxSingleLeg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade"></param>
        /// <returns></returns>
        public static FxSingleLeg TradeGetFxSingleLeg(Trade trade)
        {
            return (FxSingleLeg)trade.Item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="swap"></param>
        public static void TradeSetSwap(Trade trade, Swap swap)
        {
            trade.Item = swap;
            trade.ItemElementName = ItemChoiceType15.swap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade"></param>
        /// <returns></returns>
        public static Swap TradeGetSwap(Trade trade)
        {
            return (Swap)trade.Item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="capFloor"></param>
        public static void TradeSetCapFloor(Trade trade, CapFloor capFloor)
        {
            trade.Item = capFloor;
            trade.ItemElementName = ItemChoiceType15.capFloor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="swaption"></param>
        public static void TradeSetSwaption(Trade trade, Swaption swaption)
        {
            trade.Item = swaption;
            trade.ItemElementName = ItemChoiceType15.swaption;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="fra"></param>
        public static void TradeSetFra(Trade trade, Fra fra)
        {
            trade.Item = fra;
            trade.ItemElementName = ItemChoiceType15.fra;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tradeIdentifier"></param>
        /// <param name="tradeId"></param>
        public static void TradeIdentifierSetTradeId(TradeIdentifier tradeIdentifier, TradeId tradeId)
        {
            tradeIdentifier.Items = new object[]{tradeId};
        }

        #region QuotedAssetSet Get isValid

        /// <summary>
        /// 
        /// </summary>
        /// <param name="quotedAssetSet"></param>
        /// <returns></returns>
        public static Boolean QuotedAssetSetIsValid(QuotedAssetSet quotedAssetSet)
        {
            return quotedAssetSet?.assetQuote != null && quotedAssetSet.instrumentSet?.Items != null && quotedAssetSet.instrumentSet.Items.Length == quotedAssetSet.assetQuote.Length;
        }

        #endregion


        #region TimeDimension Get/Set date

        /// <summary>
        /// 
        /// </summary>
        /// <param name="timeDimension"></param>
        /// <returns></returns>
        public static DateTime TimeDimensionGetDate(TimeDimension timeDimension)
        {
            return (DateTime)timeDimension.Items[0];
        }

        public static void TimeDimensionSetDate(TimeDimension timeDimension, DateTime value)
        {
            timeDimension.Items = new object[]{value};
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <returns></returns>
        public static AdjustableDate  CalculationPeriodDatesGetEffectiveDate(CalculationPeriodDates calculationPeriodDates)
        {
            return (AdjustableDate)calculationPeriodDates.Item;                        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <param name="value"></param>
        public static void  CalculationPeriodDatesSetEffectiveDate(CalculationPeriodDates calculationPeriodDates, AdjustableDate value)
        {
            calculationPeriodDates.Item = value;                        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <returns></returns>
        public static AdjustableDate CalculationPeriodDatesGetTerminationDate(CalculationPeriodDates calculationPeriodDates)
        {
            return (AdjustableDate)calculationPeriodDates.Item1;                        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <param name="value"></param>
        public static void CalculationPeriodDatesSetTerminationDate(CalculationPeriodDates calculationPeriodDates, AdjustableDate value)
        {
            calculationPeriodDates.Item1 = value;                        
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <returns></returns>
        public static AdjustableDate CalculationPeriodDatesGetFirstPeriodStartDate(CalculationPeriodDates calculationPeriodDates)
        {
            return calculationPeriodDates.firstPeriodStartDate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <param name="value"></param>
        public static void CalculationPeriodDatesSetFirstPeriodStartDate(CalculationPeriodDates calculationPeriodDates, AdjustableDate value)
        {
            calculationPeriodDates.firstPeriodStartDate = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <returns></returns>
        public static DateTime? CalculationPeriodDatesGetFirstRegularPeriodStartDate(CalculationPeriodDates calculationPeriodDates)
        {
            DateTime? result = null;
            var goodDate = calculationPeriodDates.firstRegularPeriodStartDate!=new DateTime();
            if (calculationPeriodDates.firstRegularPeriodStartDateSpecified || goodDate)
            {
                result = calculationPeriodDates.firstRegularPeriodStartDate;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <param name="value"></param>
        public static void CalculationPeriodDatesSetFirstRegularPeriodStartDate(CalculationPeriodDates calculationPeriodDates, DateTime value)
        {
            calculationPeriodDates.firstRegularPeriodStartDate = value;
            calculationPeriodDates.firstRegularPeriodStartDateSpecified = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <param name="value"></param>
        public static void CalculationPeriodDatesSetLastRegularPeriodEndDate(CalculationPeriodDates calculationPeriodDates, DateTime value)
        {
            calculationPeriodDates.lastRegularPeriodEndDate = value;
            calculationPeriodDates.lastRegularPeriodEndDateSpecified = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodDates"></param>
        /// <returns></returns>
        public static DateTime? CalculationPeriodDatesGetLastRegularPeriodEndDate(CalculationPeriodDates calculationPeriodDates)
        {
            DateTime? result = null;
            bool goodDate = calculationPeriodDates.lastRegularPeriodEndDate != new DateTime();
            if (calculationPeriodDates.lastRegularPeriodEndDateSpecified || goodDate)
            {
                result = calculationPeriodDates.lastRegularPeriodEndDate;
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculationPeriodAmount"></param>
        /// <returns></returns>
        public static Calculation CalculationPeriodAmountGetCalculation(CalculationPeriodAmount calculationPeriodAmount)
        {
            return (Calculation)calculationPeriodAmount.Item;                        
        }


//        /// <remarks/>
//        [System.Xml.Serialization.XmlElementAttribute("fxLinkedNotionalSchedule", typeof(FxLinkedNotionalSchedule))]
//        [System.Xml.Serialization.XmlElementAttribute("notionalSchedule", typeof(Notional))]
//        public object Item
//
//        /// <remarks/>
//        [System.Xml.Serialization.XmlElementAttribute("fixedRateSchedule", typeof(Schedule))]
//        [System.Xml.Serialization.XmlElementAttribute("floatingRateCalculation", typeof(FloatingRateCalculation))]
//        [System.Xml.Serialization.XmlElementAttribute("inflationRateCalculation", typeof(InflationRateCalculation))]
//        [System.Xml.Serialization.XmlElementAttribute("rateCalculation", typeof(Rate))]
//        public object Item1

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="spreadSchedule"></param>
        public static void SetSpreadSchedule(InterestRateStream stream, Schedule spreadSchedule)
        {
            Calculation calculation = CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);

            FloatingRateCalculation floatingRateCalculation = CalculationGetFloatingRateCalculation(calculation);

            var schedule = new SpreadSchedule {initialValue = spreadSchedule.initialValue, step = spreadSchedule.step};

            floatingRateCalculation.spreadSchedule = new[] { schedule };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculation"></param>
        /// <returns></returns>
        public static Schedule GetCalculationSpreadSchedule(Calculation calculation)
        {
            FloatingRateCalculation floatingRateCalculation = CalculationGetFloatingRateCalculation(calculation);

            return floatingRateCalculation.spreadSchedule[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculation"></param>
        /// <param name="schedule"></param>
        public static void SetCalculationSpreadSchedule(Calculation calculation, Schedule schedule)
        {
            FloatingRateCalculation floatingRateCalculation = CalculationGetFloatingRateCalculation(calculation);

            var spreadSchedule = new SpreadSchedule {initialValue = schedule.initialValue, step = schedule.step};

            floatingRateCalculation.spreadSchedule = new[] { spreadSchedule };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="calculation"></param>
        /// <returns></returns>
        public static bool CalculationHasSpreadSchedule(Calculation calculation)
        {
            FloatingRateCalculation floatingRateCalculation = CalculationGetFloatingRateCalculation(calculation);

            if (null != floatingRateCalculation.spreadSchedule &&
                floatingRateCalculation.spreadSchedule.Length > 0 &&
                null != floatingRateCalculation.spreadSchedule[0] 
                )
            {
                return true;
            }

            return false;
        }

        #region Calculation Get/Set/Has FixedRateSchedule

        //TODO this is a hack and needs fixing.
        public static Schedule CalculationGetFixedRateSchedule(Calculation calculation)
        {
            return (Schedule) calculation.Items?[0];
        }

        public static void CalculationSetFixedRateSchedule(Calculation calculation, Schedule value)
        {
            calculation.Items = new object[] { value };
        }

        public static bool CalculationHasFixedRateSchedule(Calculation calculation)
        {
            return calculation.Items?[0] is Schedule;
        }

        #endregion

        #region Calculation Get/Set/Has DiscountingTypeEnum and DiscountRate

        public static Discounting CalculationGetDiscounting(Calculation calculation)
        {
            return calculation.discounting;
        }

        public static void CalculationSetDiscounting(Calculation calculation, Discounting discounting)
        {
            calculation.discounting = discounting;
        }

        public static bool CalculationHasDiscounting(Calculation calculation)
        {
            bool result = calculation.discounting != null;
            return result;
        }

        #endregion

        #region Calculation Get/Set/Has FloatingRateCalculation

        public static FloatingRateCalculation CalculationGetFloatingRateCalculation(Calculation calculation)
        {
            return (FloatingRateCalculation) calculation.Items?[0];
        }

        public static void CalculationSetFloatingRateCalculation(Calculation calculation, FloatingRateCalculation floatingRateCalculation)
        {
            calculation.Items = new object[] { floatingRateCalculation };
        }

        public static bool CalculationHasFloatingRateCalculation(Calculation calculation)
        {
            return calculation.Items?[0] is FloatingRateCalculation;
        }

        #endregion

        #region Calculation Get/Set NotionalSchedule

        public static FxLinkedNotionalSchedule CalculationGetFxLinkedNotionalSchedule(Calculation calculation)
        {
            FxLinkedNotionalSchedule result = null;
            if (calculation.Item is FxLinkedNotionalSchedule)
            {
                result = (FxLinkedNotionalSchedule)calculation.Item;
            }
            return result;
        }

        public static void CalculationSetFxLinkedNotionalSchedule(Calculation calculation, FxLinkedNotionalSchedule value)
        {
            calculation.Item = value;
        }

        public static Notional CalculationGetNotionalSchedule(Calculation calculation)
        {
            Notional result = null;
            if (calculation.Item is Notional)
            {
                result = (Notional)calculation.Item;
            }
            return result;
        }

        public static void CalculationSetNotionalSchedule(Calculation calculation, Notional value)
        {
            calculation.Item = value;
        }

        #endregion

        #region Get/Set/Has CalculationPeriod_FixedRate

        public static void SetCalculationPeriodFixedRate(CalculationPeriod calculationPeriod, decimal value)
        {
            calculationPeriod.Item1 = value;
        }

        public static bool CalculationPeriodHasFixedRate(CalculationPeriod calculationPeriod)
        {
            return calculationPeriod.Item1 is decimal;
        }
        
        public static decimal CalculationPeriodGetFixedRate(CalculationPeriod calculationPeriod)
        {
            return (decimal)calculationPeriod.Item1;
        }

        #endregion

        #region Get/Has CalculationPeriod_ForecastAmount

        public static bool CalculationPeriodHasForecastAmount(CalculationPeriod calculationPeriod)
        {
            return calculationPeriod.forecastAmount != null;
        }

        public static Money CalculationPeriodGetForecastAmount(CalculationPeriod calculationPeriod)
        {
            return calculationPeriod.forecastAmount;
        }

        #endregion

        #region Get/Set/Has CalculationPeriod_FloatingRateDefinition

        public static void CalculationPeriodSetFloatingRateDefinition(CalculationPeriod calculationPeriod, FloatingRateDefinition value)
        {
            calculationPeriod.Item1 = value;
        }

        public static FloatingRateDefinition CalculationPeriodGetFloatingRateDefinition(CalculationPeriod calculationPeriod)
        {
            return (FloatingRateDefinition)calculationPeriod.Item1;
        }

        public static bool CalculationPeriodHasFloatingRateDefinition(CalculationPeriod calculationPeriod)
        {
            return calculationPeriod.Item1 is FloatingRateDefinition;
        }

        #endregion

        #region CalculationPeriod Get/Set NotionalAmount

        public static void CalculationPeriodSetNotionalAmount(CalculationPeriod calculationPeriod, decimal value)
        {
            calculationPeriod.Item = value;
        }
        public static decimal  CalculationPeriodGetNotionalAmount(CalculationPeriod calculationPeriod)
        {
            return (decimal)calculationPeriod.Item;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentCalculationPeriod"></param>
        /// <returns></returns>
        public static CalculationPeriod[] GetPaymentCalculationPeriodCalculationPeriodArray(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            return paymentCalculationPeriod.Items.Cast<CalculationPeriod>().ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentCalculationPeriod"></param>
        /// <param name="value"></param>
        public static void SetPaymentCalculationPeriodCalculationPeriodArray(PaymentCalculationPeriod paymentCalculationPeriod, CalculationPeriod[] value)
        {
            paymentCalculationPeriod.Items = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="paymentCalculationPeriod"></param>
        /// <returns></returns>
        public static decimal[] GetPaymentCalculationPeriodFixedPaymentAmountArray(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            return paymentCalculationPeriod.Items.Select(item => (decimal) item).ToArray();
        }


        #region StubValue

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stubValue"></param>
        /// <returns></returns>
        public static decimal[] GetStubValueStubRateArray(StubValue stubValue)
        {
            return stubValue.Items.Select(item => (decimal) item).ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stubValue"></param>
        /// <returns></returns>
        public static bool StubValueHasStubRateArray(StubValue stubValue)
        {
            return (stubValue.Items[0] is decimal);
        }
    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stubValue"></param>
        /// <returns></returns>
        public static Money[] GetStubValueStubAmountArray(StubValue stubValue)
        {
            return stubValue.Items.Cast<Money>().ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stubValue"></param>
        /// <returns></returns>
        public static bool StubValueHasStubAmountArray(StubValue stubValue)
        {
            return (stubValue.Items[0] is Money);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="stubValue"></param>
        /// <returns></returns>
        public static FloatingRate[] GetStubValueFloatingRateArray(StubValue stubValue)
        {
            return stubValue.Items.Cast<FloatingRate>().ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stubValue"></param>
        /// <returns></returns>
        public static bool StubValueHasFloatingRateArray(StubValue stubValue)
        {
            return (stubValue.Items[0] is FloatingRate);
        }

        #endregion StubValue

        //class Swaption {
        //        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise))]
        //        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise))]
        //        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise))]
        //        public Exercise Item {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="swaption"></param>
        /// <param name="europeanExercise"></param>
        public static void SwaptionSetEuropeanExercise(Swaption swaption, EuropeanExercise europeanExercise)
        {
            swaption.Item = europeanExercise;
        }



//                        /// <remarks/>
//        [System.Xml.Serialization.XmlElementAttribute("automaticExercise", typeof(AutomaticExercise))]
//        [System.Xml.Serialization.XmlElementAttribute("manualExercise", typeof(ManualExercise))]
//        public object Item {
//            get {
//                return this.itemField;
//            }
//            set {
//                this.itemField = value;
//            }
//        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="exerciseProcedure"></param>
        /// <param name="automaticExercise"></param>
        public static void ExerciseProcedureSetAutomaticExercise(ExerciseProcedure exerciseProcedure, AutomaticExercise automaticExercise)
        {
            exerciseProcedure.Item = automaticExercise;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exerciseProcedure"></param>
        /// <param name="manualExercise"></param>
        public static void ExerciseProcedureSetManualExercise(ExerciseProcedure exerciseProcedure, ManualExercise manualExercise)
        {
            exerciseProcedure.Item = manualExercise;
        }
    }
}