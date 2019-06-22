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

#region Usings

using System.Collections.Generic;
using System.Globalization;
using Orion.Analytics.Options;
using Orion.Analytics.Processes;
using Orion.Analytics.Rates;

#endregion

namespace Orion.Analytics.Stochastics.Pedersen
{
    /// <summary>
    /// Need to adjust for shift upon construction.
    /// </summary>
    public class CalibrationTargets
    {
        #region Constructor + Initialisation

        ///<summary>
        ///</summary>
        ///<param name="timeGrid"></param>
        ///<param name="capletData"></param>
        ///<param name="swaptionData"></param>
        public CalibrationTargets(PedersenTimeGrid timeGrid, double[,] capletData, double[,] swaptionData )
        {
            AdjustedForShift = false;
            var capletExpiry = new List<int>();
            var capletImpliedVol = new List<double>();
            var swaptionExpiry = new List<int>();
            var swaptionTenor = new List<int>();
            var swaptionImpliedVol = new List<double>();
            int tempExpiry;
            for (int i = capletData.GetLowerBound(0); i <= capletData.GetUpperBound(0); i++)
            {
                try
                {
                    tempExpiry = int.Parse(capletData[i, 0].ToString(CultureInfo.InvariantCulture));
                    if (tempExpiry <= timeGrid.MaxExpiry
                        && tempExpiry > 0)
                    {
                        capletExpiry.Add(tempExpiry);
                        capletImpliedVol.Add(capletData[i, 1]);
                    }
                }
                catch
                {
                }
            }
            for (int i = swaptionData.GetLowerBound(0); i <= swaptionData.GetUpperBound(0); i++)
            {
                try
                {
                    tempExpiry = int.Parse(swaptionData[i, 0].ToString(CultureInfo.InvariantCulture));
                    var tempTenor = int.Parse(swaptionData[i, 1].ToString(CultureInfo.InvariantCulture));
                    if (tempExpiry > 0
                        && tempTenor > 0
                        && tempExpiry <= timeGrid.MaxExpiry
                        && tempExpiry + tempTenor - 1 <= timeGrid.MaxTenor)
                    {
                        swaptionExpiry.Add(tempExpiry);
                        swaptionTenor.Add(tempTenor);
                        swaptionImpliedVol.Add(swaptionData[i, 2]);
                    }
                }
                catch
                {
                }
            }
            CapletExpiry = capletExpiry.ToArray();
            CapletImpliedVol = capletImpliedVol.ToArray();
            SwaptionExpiry = swaptionExpiry.ToArray();
            SwaptionTenor = swaptionTenor.ToArray();
            SwaptionImpliedVol = swaptionImpliedVol.ToArray();
            CapletCount = CapletImpliedVol.GetLength(0);
            SwaptionCount = SwaptionImpliedVol.GetLength(0);
        }

        #endregion

        #region Adjustment for Shift

        ///<summary>
        ///</summary>
        ///<param name="discount"></param>
        ///<param name="shift"></param>
        public void AdjustImpliedVolsForShift(QuarterlyDiscounts discount, QuarterlyShifts shift)
        {
            if (!AdjustedForShift)
            {
                AdjustedForShift = true;
                for (int i = 0; i < CapletCount; i++)
                {
                    CapletImpliedVol[i] = AdjustCapletImpliedVol(CapletExpiry[i], CapletImpliedVol[i], discount, shift);
                }
                for (int i = 0; i < SwaptionCount; i++)
                {
                    SwaptionImpliedVol[i] = AdjustSwaptionImpliedVol(SwaptionExpiry[i], SwaptionTenor[i], SwaptionImpliedVol[i], discount, shift);
                }
            }
        }

        private static double AdjustCapletImpliedVol(int expiry, double volatility, QuarterlyDiscounts discount, QuarterlyShifts shift)
        {
            double strike = RateAnalytics.CashForwardRate(expiry, discount);
            double capletPrice = BlackModel.GetValue(expiry / 4.0, strike, strike, volatility, PayStyle.Call);
            double result = BlackModel.GetImpliedVolatility(expiry / 4.0, strike + shift.Get(expiry), strike + shift.Get(expiry), capletPrice, volatility, PayStyle.Call);
            return result;
        }

        private static double AdjustSwaptionImpliedVol(int expiry, int tenor, double volatility, QuarterlyDiscounts discount, QuarterlyShifts shift)
        {
            double strike = RateAnalytics.SwapRate(expiry, tenor, discount);
            double swapShift = RateAnalytics.SwapShift(expiry, tenor, discount, shift);
            double swaptionPrice = BlackModel.GetValue(expiry / 4.0, strike, strike, volatility, PayStyle.Call);
            double result = BlackModel.GetImpliedVolatility(expiry / 4.0, strike + swapShift, strike + swapShift, swaptionPrice, volatility, PayStyle.Call);
            return result;
        }

        #endregion

        #region Accessor Methods

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public double AverageSwaptionImpliedVol()
        {
            if (SwaptionCount == 0)
            {
                return 0;
            }
            double result = 0;
            for (int i = 0; i < SwaptionCount; i++)
            {
                result += SwaptionImpliedVol[i];
            }
            result = result / SwaptionCount;
            return result;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public double AverageCapletImpliedVol()
        {
            if (CapletCount == 0)
            {
                return 0;
            }
            double result = 0;
            for (int i = 0; i < CapletCount; i++)
            {
                result += CapletImpliedVol[i];
            }
            result = result / CapletCount;
            return result;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public double AverageImpliedVol()
        {
            if (CapletCount + SwaptionCount == 0)
            {
                return 0;
            }
            double result = 0;
            for (int i = 0; i < SwaptionCount; i++)
            {
                result += SwaptionImpliedVol[i];
            }
            for (int i = 0; i < CapletCount; i++)
            {
                result += CapletImpliedVol[i];
            }
            result = result / (CapletCount+ SwaptionCount);
            return result;
        }

        ///<summary>
        ///</summary>
        ///<param name="expiry"></param>
        ///<returns></returns>
        public double GetCapletImpliedVol(int expiry)
        {
            for (int i = 0; i < CapletCount; i++)
            {
                if (CapletExpiry[i] == expiry)
                {
                    return CapletImpliedVol[i];
                }
            }
            return 0;
        }

        ///<summary>
        ///</summary>
        ///<param name="expiry"></param>
        ///<param name="tenor"></param>
        ///<returns></returns>
        public double GetSwaptionImpliedVol(int expiry, int tenor)
        {
            for (int i = 0; i < SwaptionCount; i++)
            {
                if (SwaptionExpiry[i] == expiry)
                {
                    if (SwaptionTenor[i] == tenor)
                    {
                        return SwaptionImpliedVol[i];
                    }
                }
            }
            return 0;
        }

        #endregion

        #region Private Fields

        ///<summary>
        ///</summary>
        public bool AdjustedForShift { get; private set; }

        ///<summary>
        ///</summary>
        public int[] CapletExpiry { get; }

        ///<summary>
        ///</summary>
        public double[] CapletImpliedVol { get; }

        ///<summary>
        ///</summary>
        public int[] SwaptionExpiry { get; }

        ///<summary>
        ///</summary>
        public int[] SwaptionTenor { get; }

        ///<summary>
        ///</summary>
        public double[] SwaptionImpliedVol { get; }

        ///<summary>
        ///</summary>
        public int CapletCount { get; }

        ///<summary>
        ///</summary>
        public int SwaptionCount { get; }

        #endregion
    }
}