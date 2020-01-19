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

namespace Highlander.Numerics.Rates
{
    ///<summary>
    ///</summary>
    public class SwaptionWeights
    {
        #region Constructor

        /// <summary>
        /// Computes the weights used to convert interest rate volatilities to swaption implied volatilities.
        /// Need to be populated upon construction.
        /// </summary>
        /// <param name="maxExpiry"></param>
        /// <param name="maxTenor"></param>
        public SwaptionWeights(int maxExpiry, int maxTenor)
        {
            _maxExpiry = maxExpiry;
            _maxTenor = maxTenor;
            _weights = new double[maxExpiry][][];
            for (int i = 0; i < maxExpiry; i++)
            {
                _weights[i] = new double[maxTenor - i][];
                for (int j = 0; j < maxTenor - i; j++)
                {
                    _weights[i][j] = new double[j + 1];
                }
            }
        }

        #endregion

        #region Weights Calculation and Access

        ///<summary>
        ///</summary>
        ///<param name="discount"></param>
        ///<param name="shift"></param>
        public void Populate(QuarterlyDiscounts discount, QuarterlyShifts shift)
        {
            var forward = new double[_maxTenor + 1];
            var accumForward = new double[_maxTenor + 1];
            var accumForwardTimesRate = new double[_maxTenor + 1];
            var accumForwardTimesShiftedRate = new double[_maxTenor + 1];
            accumForward[0] = 0;
            accumForwardTimesRate[0] = 0;
            accumForwardTimesShiftedRate[0] = 0;
            var w = new double[_maxTenor];
            var h = new double[_maxTenor];

            for (int i = 0; i < _maxExpiry; i++)
            {
                for (int j = 0; j < _maxTenor - i; j++)
                {
                    //See "Engineering BGM" page 34 for explicit formula. Function definitions can be found in chapter 4.
                    forward[j + 1] = RateAnalytics.ForwardContract(i + 1, i + j + 2, discount);
                    accumForward[j + 1] = accumForward[j] + forward[j + 1];
                    accumForwardTimesRate[j + 1]
                        = accumForwardTimesRate[j] + forward[j + 1] * RateAnalytics.CashForwardRate(i + j + 1, discount);
                    accumForwardTimesShiftedRate[j + 1]
                        = accumForwardTimesShiftedRate[j] + forward[j + 1] * RateAnalytics.ShiftedCashForwardRate(i + j + 1, discount, shift);
                    h[j] = 0.25 * RateAnalytics.ShiftedCashForwardRate(i + j + 1, discount, shift) / (1 + 0.25 * RateAnalytics.CashForwardRate(i + j + 1, discount));
                    for (int k = 0; k < j + 1; k++)
                    {
                        w[k] = forward[k + 1] * RateAnalytics.ShiftedCashForwardRate(i + k + 1, discount, shift) / accumForwardTimesShiftedRate[j + 1];
                        _weights[i][j][k] = w[k] - h[k] * ((accumForwardTimesRate[j + 1] - accumForwardTimesRate[k]) / accumForwardTimesShiftedRate[j + 1]
                                                           - (1 - accumForward[k] / accumForward[j + 1]) * accumForwardTimesRate[j + 1] / accumForwardTimesShiftedRate[j + 1]);
                    }
                }
            }
        }

        ///<summary>
        ///</summary>
        ///<param name="expiry"></param>
        ///<param name="tenor"></param>
        ///<param name="index"></param>
        ///<returns></returns>
        public double Get(int expiry, int tenor, int index)
        {
            return _weights[expiry - 1][tenor - 1][index - 1];
        }

        #endregion

        #region Private Fields

        private readonly double[][][] _weights;
        //_weighhts[i][j][k] is applied to the kth volatility term of a swaption with expiry at i, tenor j
        private readonly int _maxExpiry;
        private readonly int _maxTenor;

        #endregion
    }
}