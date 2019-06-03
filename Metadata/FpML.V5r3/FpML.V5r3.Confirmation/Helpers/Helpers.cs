#region Usings

using System;
using FpML.V5r3.Confirmation.Extensions;

#endregion

namespace FpML.V5r3.Confirmation
{
    public class Helpers
    {
        #region Helpers

        ///<summary>
        /// Gets all the Vol surface curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetFxVolatilityMatrixName(FxRate fxRateIndex, string asset)
        {
            return PricingStructureTypeEnum.FxVolatilityMatrix + "." + fxRateIndex.quotedCurrencyPair.currency1.Value + fxRateIndex.quotedCurrencyPair.currency2.Value + "-" + asset;
        }

        ///<summary>
        /// Gets all the Vol surface curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetRateVolatilityMatrixName(ForecastRateIndex forecastRateIndex)
        {
            return GetForecastCurveName(PricingStructureTypeEnum.RateVolatilityMatrix, forecastRateIndex);
        }

        ///<summary>
        /// Gets all the Forecast curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetRateVolatilityMatrixName(FloatingRateIndex floatingRateIndex, Period indexTenor)
        {
            var result = GetRateVolatilityMatrixName(floatingRateIndex, new[] { indexTenor });
            return result;
        }

        ///<summary>
        /// Gets all the Forecast curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetRateVolatilityMatrixName(Swap swap)
        {
            AdjustableDate adjustableEffectiveDate = XsdClassesFieldResolver.CalculationPeriodDatesGetEffectiveDate(swap.swapStream[0].calculationPeriodDates);
            AdjustableDate adjustableTerminationDate = XsdClassesFieldResolver.CalculationPeriodDatesGetTerminationDate(swap.swapStream[0].calculationPeriodDates);
            var years = adjustableTerminationDate.unadjustedDate.Value.Year - adjustableEffectiveDate.unadjustedDate.Value.Year;
            var calculation = (Calculation)swap.swapStream[0].calculationPeriodAmount.Item;
            var notional = XsdClassesFieldResolver.CalculationGetNotionalSchedule(calculation);
            var currency = notional.notionalStepSchedule.currency.Value;
            return "RateVolatilityMatrix." + currency + "-IRSwap-" + years + "Y";
        }

        ///<summary>
        /// Gets all the Forecast curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetForecastCurveName(FloatingRateIndex floatingRateIndex, Period indexTenor)
        {
            string result = indexTenor != null ? GetForecastCurveName(floatingRateIndex, new[] { indexTenor }) : GetForecastCurveName(PricingStructureTypeEnum.RateCurve, floatingRateIndex);
            return result;
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetDiscountCurveName(Currency currency)
        {
            var result = GetDiscountCurveName(currency.Value);
            return result;
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetCounterPartyCurveName(string currency, string counterparty, string seniority)
        {
            return "DiscountCurve." + currency + "-" + counterparty + "-" + seniority;
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetDiscountCurveName(string currency)
        {
            return GetDiscountCurveName(currency, true);
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        private static string GetDiscountCurveName(string currency, Boolean isSimple)
        {
            var result = "Unknown Discount Curve.";
            if (isSimple)
            {
                result = "DiscountCurve." + currency + "-" + "LIBOR-SENIOR";
            }
            return result;
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetForecastCurveName(FloatingRateCalculation floatingRateIndex)
        {
            return GetForecastCurveName(PricingStructureTypeEnum.RateCurve, floatingRateIndex);
        }

        ///<summary>
        /// Gets all the Forecast curve name.
        ///</summary>
        ///<returns></returns>
        private static string GetForecastCurveName(PricingStructureTypeEnum curveType, FloatingRateCalculation floatingRateIndex)
        {
            return curveType + "." + floatingRateIndex.floatingRateIndex.Value + "-" + floatingRateIndex.indexTenor.ToString();
        }

        ///<summary>
        /// Gets all the forecast curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetForecastCurveName(ForecastRateIndex forecastRateIndex)
        {
            return GetForecastCurveName(PricingStructureTypeEnum.RateCurve, forecastRateIndex);
        }

        ///<summary>
        /// Gets all the Forecast curve name.
        ///</summary>
        ///<returns></returns>
        private static string GetForecastCurveName(PricingStructureTypeEnum curveType, ForecastRateIndex floatingRateIndex)
        {
            return curveType + "." + floatingRateIndex.floatingRateIndex.Value + "-" + floatingRateIndex.indexTenor.ToString();
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetRateVolatilityMatrixName(FloatingRateIndex floatingRateIndex, Period[] periods)
        {
            return GetForecastCurveName(PricingStructureTypeEnum.RateVolatilityMatrix, floatingRateIndex, periods);
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetForecastCurveName(FloatingRateIndex floatingRateIndex, Period[] periods)
        {
            return GetForecastCurveName(PricingStructureTypeEnum.RateCurve, floatingRateIndex, periods);
        }

        ///<summary>
        /// Gets all the Forecast curve name.
        ///</summary>
        ///<returns></returns>
        private static string GetForecastCurveName(PricingStructureTypeEnum curveType, FloatingRateIndex floatingRateIndex, Period[] periods)
        {
            return curveType + "." + floatingRateIndex.Value + "-" + periods[0].ToString();
        }

        ///<summary>
        /// Gets all the Forecast curve name.
        ///</summary>
        ///<returns></returns>
        private static string GetForecastCurveName(PricingStructureTypeEnum curveType, FloatingRateIndex floatingRateIndex)
        {
            return curveType + "." + floatingRateIndex.Value;
        }

        #endregion
    }
}
