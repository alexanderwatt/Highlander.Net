using System;
using System.Linq;

namespace FpML.V5r10.Reporting
{
    /// <summary>
    /// Class simplifies strongly typed access to fields of the classes generated by Xsd.Exe.
    ///  
    /// Although above mentioned tool produces good resuls most of the time, sometimes 
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
    /// NB: Although underscores in member names are against our codestyle naming convention, 
    /// 
    /// I left it as it is because otherwise the intention of these methods would be difficult 
    /// to understand.
    /// </example>
    /// </summary>
    public  static class XsdClassesFieldResolver
    {
        public static Trade[] TradeValuationItemGetTradeArray(TradeValuationItem tradeValuationItem)
        {
            return tradeValuationItem.Items.Cast<Trade>().ToArray();
        }

        #region Trade GetXXX/SetXXX methods

        public static void TradeSetFxSingleLeg(Trade trade, FxSingleLeg swap)
        {
            trade.Item = swap;
            trade.ItemElementName = ItemChoiceType15.fxSingleLeg;
        }

        public static FxSingleLeg TradeGetFxSingleLeg(Trade trade)
        {
            return (FxSingleLeg)trade.Item;
        }

        public static void TradeSetSwap(Trade trade, Swap swap)
        {
            trade.Item = swap;
            trade.ItemElementName = ItemChoiceType15.swap;
        }

        public static Swap TradeGetSwap(Trade trade)
        {
            return (Swap)trade.Item;
        }

        public static void TradeSetCapFloor(Trade trade, CapFloor capFloor)
        {
            trade.Item = capFloor;
            trade.ItemElementName = ItemChoiceType15.capFloor;
        }

        public static void TradeSetSwaption(Trade trade, Swaption swaption)
        {
            trade.Item = swaption;
            trade.ItemElementName = ItemChoiceType15.swaption;
        }

        public static void TradeSetFra(Trade trade, Fra fra)
        {
            trade.Item = fra;
            trade.ItemElementName = ItemChoiceType15.fra;
        }

        #endregion


        public static void TradeIdentifierSetTradeId(TradeIdentifier tradeIdentifier, TradeId tradeId)
        {
            tradeIdentifier.Items = new object[]{tradeId};
        }

        #region QuotedAssetSet Get isValid

        public static Boolean QuotedAssetSetIsValid(QuotedAssetSet quotedAssetSet)
        {
            return quotedAssetSet?.assetQuote != null && quotedAssetSet.instrumentSet?.Items != null && quotedAssetSet.instrumentSet.Items.Length == quotedAssetSet.assetQuote.Length;
        }

        #endregion


        #region TimeDimension Get/Set date

        public static DateTime TimeDimensionGetDate(TimeDimension timeDimension)
        {
            return (DateTime)timeDimension.Items[0];
        }

        public static void TimeDimensionSetDate(TimeDimension timeDimension, DateTime value)
        {
            timeDimension.Items = new object[]{value};
        }

        #endregion

        public static AdjustableDate  CalculationPeriodDatesGetEffectiveDate(CalculationPeriodDates calculationPeriodDates)
        {
            return (AdjustableDate)calculationPeriodDates.Item;                        
        }

        public static void  CalculationPeriodDatesSetEffectiveDate(CalculationPeriodDates calculationPeriodDates, AdjustableDate value)
        {
            calculationPeriodDates.Item = value;                        
        }

        public static AdjustableDate CalculationPeriodDatesGetTerminationDate(CalculationPeriodDates calculationPeriodDates)
        {
            return (AdjustableDate)calculationPeriodDates.Item1;                        
        }

        public static void CalculationPeriodDatesSetTerminationDate(CalculationPeriodDates calculationPeriodDates, AdjustableDate value)
        {
            calculationPeriodDates.Item1 = value;                        
        }

        public static AdjustableDate CalculationPeriodDatesGetFirstPeriodStartDate(CalculationPeriodDates calculationPeriodDates)
        {
            return calculationPeriodDates.firstPeriodStartDate;
        }

        public static void CalculationPeriodDatesSetFirstPeriodStartDate(CalculationPeriodDates calculationPeriodDates, AdjustableDate value)
        {
            calculationPeriodDates.firstPeriodStartDate = value;
        }

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

        public static void CalculationPeriodDatesSetFirstRegularPeriodStartDate(CalculationPeriodDates calculationPeriodDates, DateTime value)
        {
            calculationPeriodDates.firstRegularPeriodStartDate = value;
            calculationPeriodDates.firstRegularPeriodStartDateSpecified = true;
        }

        public static void CalculationPeriodDatesSetLastRegularPeriodEndDate(CalculationPeriodDates calculationPeriodDates, DateTime value)
        {
            calculationPeriodDates.lastRegularPeriodEndDate = value;
            calculationPeriodDates.lastRegularPeriodEndDateSpecified = true;
        }

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


        public static void SetSpreadSchedule(InterestRateStream stream, Schedule spreadShedule)
        {
            Calculation calculation = CalculationPeriodAmountGetCalculation(stream.calculationPeriodAmount);
            FloatingRateCalculation floatingRateCalculation = CalculationGetFloatingRateCalculation(calculation);
            var schedule = new SpreadSchedule {initialValue = spreadShedule.initialValue, step = spreadShedule.step};
            floatingRateCalculation.spreadSchedule = new[] { schedule };
        }

        public static Schedule GetCalculationSpreadSchedule(Calculation calculation)
        {
            FloatingRateCalculation floatingRateCalculation = CalculationGetFloatingRateCalculation(calculation);
            return floatingRateCalculation.spreadSchedule[0];
        }

        public static void SetCalculationSpreadSchedule(Calculation calculation, Schedule schedule)
        {
            FloatingRateCalculation floatingRateCalculation = CalculationGetFloatingRateCalculation(calculation);
            var spreadSchedule = new SpreadSchedule {initialValue = schedule.initialValue, step = schedule.step};
            floatingRateCalculation.spreadSchedule = new[] { spreadSchedule };
        }

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
            var item = calculation.Item as FxLinkedNotionalSchedule;
            if (item != null)
            {
                result = item;
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
            var item = calculation.Item as Notional;
            if (item != null)
            {
                result = item;
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

        public static CalculationPeriod[] GetPaymentCalculationPeriodCalculationPeriodArray(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            return paymentCalculationPeriod.Items.Cast<CalculationPeriod>().ToArray();
        }

        public static void SetPaymentCalculationPeriodCalculationPeriodArray(PaymentCalculationPeriod paymentCalculationPeriod, CalculationPeriod[] value)
        {
            paymentCalculationPeriod.Items = value;
        }

        public static decimal[] GetPaymentCalculationPeriodFixedPaymentAmountArray(PaymentCalculationPeriod paymentCalculationPeriod)
        {
            return paymentCalculationPeriod.Items.Select(item => (decimal) item).ToArray();
        }


        #region StubValue

        public static decimal[] GetStubValueStubRateArray(StubValue stubValue)
        {
            return stubValue.Items.Select(item => (decimal) item).ToArray();
        }

        public static bool StubValueHasStubRateArray(StubValue stubValue)
        {
            return (stubValue.Items[0] is decimal);
        }

        public static Money[] GetStubValueStubAmountArray(StubValue stubValue)
        {
            return stubValue.Items.Cast<Money>().ToArray();
        }
        public static bool StubValueHasStubAmountArray(StubValue stubValue)
        {
            return (stubValue.Items[0] is Money);
        }
        
        public static FloatingRate[] GetStubValueFloatingRateArray(StubValue stubValue)
        {
            return stubValue.Items.Cast<FloatingRate>().ToArray();
        }

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
        
        public static void ExerciseProcedureSetAutomaticExercise(ExerciseProcedure exerciseProcedure, AutomaticExercise automaticExercise)
        {
            exerciseProcedure.Item = automaticExercise;
        }

        public static void ExerciseProcedureSetManualExercise(ExerciseProcedure exerciseProcedure, ManualExercise manualExercise)
        {
            exerciseProcedure.Item = manualExercise;
        }


    }
}