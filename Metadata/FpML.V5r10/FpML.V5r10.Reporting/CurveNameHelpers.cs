/*
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

#region Usings

using System;
using Orion.Constants;

#endregion

namespace FpML.V5r10.Reporting
{
    public class CurveNameHelpers
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
            return PricingStructureTypeEnum.RateVolatilityMatrix + "." + currency + "-IRSwap-" + years + "Y";
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
        public static string GetDiscountCurveName(Currency currency, Boolean isSimple)
        {
            var result = GetDiscountCurveName(currency.Value, isSimple);
            return result;
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetEquityCurveName(String currency, String ticker)
        {
            var result = PricingStructureTypeEnum.EquityCurve + "." + currency.ToUpper() + "-" + ticker.ToUpper();
            return result;
        }

        /// <summary>
        /// Gets a property forward curve.
        /// </summary>
        /// <param name="currency">The currency of the property.</param>
        /// <param name="propertyId">The property Id.</param>
        /// <returns></returns>
        public static string GetPropertyCurveName(String currency, String propertyId)
        {
            var result = PricingStructureTypeEnum.PropertyCurve + "." + currency.ToUpper()+ propertyId.ToUpper();
            return result;
        }

        /// <summary>
        /// Gets a bond forward curve.
        /// </summary>
        /// <param name="currency">The currency of the bond.</param>
        /// <param name="bondId">The bond Id.</param>
        /// <returns></returns>
        public static string GetBondCurveName(String currency, String bondId)
        {
            var result = PricingStructureTypeEnum.BondCurve + "." + GetBondCurveNameSimple(currency, bondId);
            return result;
        }

        /// <summary>
        /// Gets a bond forward curve.
        /// </summary>
        /// <param name="currency">The currency of the bond.</param>
        /// <param name="bondId">THe bond Id.</param>
        /// <returns></returns>
        public static string GetBondCurveNameSimple(String currency, String bondId)
        {
            var parts = bondId.Split('.');
            if (parts.Length > 4)
            {
                var result = currency.ToUpper() + "-" + parts[0] + ":" + parts[1] + ":" + parts[2] + ":" + parts[3] + "-" + parts[4];
                return result;
            }
            return null;
        }

        /// <summary>
        /// Gets a exchange traded  curve.
        /// </summary>
        /// <param name="currency">The currency of the bond.</param>
        /// <param name="exchange">THe exchange code e.g. ASX</param>
        /// <param name="exchangeFutureCode">The exchange traded future Id e.g. IR.</param>
        /// <returns></returns>
        public static string GetExchangeTradedCurveName(String currency, String exchange, String exchangeFutureCode)
        {
            var result = PricingStructureTypeEnum.ExchangeTradedCurve + "." + GetExchangeTradedCurveNameSimple(currency, exchange, exchangeFutureCode);
            return result;
        }

        /// <summary>
        /// Gets a exchange traded  curve.
        /// </summary>
        /// <param name="currency">The currency of the bond.</param>
        /// <param name="exchange">THe exchange code e.g. ASX</param>
        /// <param name="exchangeFutureCode">The exchange traded future Id e.g. IR.</param>
        /// <returns></returns>
        public static string GetExchangeTradedCurveNameSimple(String currency, String exchange, String exchangeFutureCode)
        {
            var result = currency.ToUpper() + "-" + exchange + "-" + exchangeFutureCode;
            return result;
        }

        /// <summary>
        ///  Gets all the Discount curve name.
        /// </summary>
        ///  <param name="currency">The currency of the bond.</param>
        ///  <param name="counterparty">THe exchange code e.g. ASX</param>
        /// <param name="seniority">The seniority</param>
        /// <returns></returns>
        public static string GetCounterPartyCurveName(string currency, string counterparty, string seniority)
        {
            return PricingStructureTypeEnum.DiscountCurve + "." + currency + "-" + counterparty + "-" + seniority;
        }

        ///<summary>
        /// Gets all the Discount curve name.
        ///</summary>
        ///<returns></returns>
        public static string GetDiscountCurveName(string currency, Boolean isSimple)
        {
            var result = PricingStructureTypeEnum.DiscountCurve + "." + currency + "-" + "OIS-SECURED";
            if (isSimple)
            {
                result = PricingStructureTypeEnum.DiscountCurve + "." + currency + "-" + "LIBOR-SENIOR";
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
