﻿#region Using directives

using System;
using System.Runtime.InteropServices;
using HLV5r3.Helpers;
using Microsoft.Win32;
using Orion.Analytics.Rates;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Analytics
{
    /// <summary>
    /// A simple model for futures convexity correction.
    /// </summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("066D43D9-B355-4C75-899F-1CCFD7A85248")]
    public class Futures
    {
        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key.SetValue("", System.Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

        #endregion

        #region Constructor

        public Futures()
        {
        }

        #endregion

        #region Functions

        ///<summary>
        /// The main function to calculate the adjusted forward.
        ///</summary>
        ///<param name="futuresPriceArray"> Vector of futures prices.</param>
        ///<param name="volatilityRange"> Vector of futures</param>
        ///<param name="correlationRange"> Correlation between forwards. Can be a
        /// scalar input or a correlation matrix.</param>
        ///<param name="shiftArray"> The shift parameters from the BGM 
        /// calibration.</param>
        ///<param name="coverageArray"> The vector of time intervals between model 
        /// time nodes.</param>
        ///<param name="timeNodesArray"> The model time nodes</param>
        ///<returns> Output, a 2-D array of ouput data. The first column is the 
        /// vector of adjusted cash forwards. The second column is the error 
        /// from the optimisation (the implied futures rate minus the market 
        /// rate). The third column is the number of iterations taken by the
        /// optimisation routine to converge on each adjusted rate. In the first
        /// entry in the fourth and fifth column is the time taken to work 
        /// through the whole set of time nodes, and the program version.</returns>
        public object[,] CalculateCashForward(Excel.Range futuresPriceArray,
                                                        Excel.Range volatilityRange,
                                                        Excel.Range correlationRange,
                                                        Excel.Range shiftArray,
                                                        Excel.Range coverageArray,
                                                        Excel.Range timeNodesArray)
        {
            var futuresPrice = DataRangeHelper.StripDoubleRange(futuresPriceArray);
            var shift = DataRangeHelper.StripDoubleRange(shiftArray);
            var coverage = DataRangeHelper.StripDoubleRange(coverageArray);
            var timeNodes = DataRangeHelper.StripDoubleRange(timeNodesArray);
            var volatility = DataRangeHelper.ToMatrix<double>(volatilityRange);
            var correlation = DataRangeHelper.ToMatrix<double>(correlationRange);
            var result = CashForward.CalculateCashForward(futuresPrice.ToArray(), volatility,
                                                        correlation, shift.ToArray(), coverage.ToArray(), timeNodes.ToArray());
            return DataRangeHelper.ToRange(result);
        }

        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public Decimal FuturesMarginConvexityAdjustment(Decimal rate, double timeToExpiry, double volatility)
        {
            return FuturesAnalytics.FuturesMarginConvexityAdjustment(rate, timeToExpiry, volatility);
        }

        /// <summary>
        /// Evaluates the implied futures quote from a provided convexity adjusted forward.
        /// This currently only works for margined futures without an arrears adjustment.
        /// </summary>
        /// <param name="impliedRate"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public Decimal FuturesImpliedQuoteFromMarginAdjusted(Decimal impliedRate, double timeToExpiry, double volatility)
        {
            return FuturesAnalytics.FuturesImpliedQuoteFromMarginAdjusted(impliedRate, timeToExpiry, volatility);
        }

        /// <summary>
        /// Evaluates the implied futures quote from a provided convexity adjusted forward.
        /// This currently only works for margined futures without an arrears adjustment.
        /// </summary>
        /// <param name="impliedRate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public Decimal FuturesImpliedQuoteWithArrears(Decimal impliedRate, double yearFraction, double timeToExpiry, double volatility)
        {
            return FuturesAnalytics.FuturesImpliedQuoteWithArrears(impliedRate, yearFraction, timeToExpiry, volatility);
        }

        /// <summary>
        /// Evaluates the implied futures quote from a provided convexity adjusted forward.
        /// </summary>
        /// <param name="impliedRate"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="yearFraction"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public Decimal FuturesImpliedQuoteFromMarginAdjustedWithArrears(Decimal impliedRate, double yearFraction, double timeToExpiry, double volatility)
        {
            return FuturesAnalytics.FuturesImpliedQuoteFromMarginAdjustedWithArrears(impliedRate, yearFraction, timeToExpiry, volatility);
        }

        /// <summary>
        /// Evaluates the futures arrears convexity adjustment.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public Decimal FuturesArrearsConvexityAdjustment(Decimal rate, double yearFraction, double timeToExpiry, double volatility)
        {
            return FuturesAnalytics.FuturesArrearsConvexityAdjustment(rate, yearFraction, timeToExpiry, volatility);
        }

        /// <summary>
        /// Evaluates the futures arrears convexity adjustment.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public Decimal FuturesMarginWithArrearsConvexityAdjustment(Decimal rate, double yearFraction, double timeToExpiry, double volatility)
        {
            return FuturesAnalytics.FuturesMarginWithArrearsConvexityAdjustment(rate, yearFraction, timeToExpiry, volatility);
        }

        #endregion
    }
}