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

namespace Orion.Analytics.Rates
{
    ///<summary>
    ///</summary>
    public class QuarterlyDiscounts
    {
        #region Constructors + Initialisations

        ///<summary>
        ///</summary>
        public QuarterlyDiscounts()
        {
            //Populate discount factors using RateCurve.
        }

        ///<summary>
        ///</summary>
        ///<param name="df"></param>
        public QuarterlyDiscounts(double[] df)
        {
            _discountFactors = df;
            _upperBound = _discountFactors.GetLength(0) - 1;
            if (_upperBound > 0)
            {
                //flat extrapolation beyond data rage
                _extrapolationRatio = _discountFactors[_upperBound] / _discountFactors[_upperBound - 1];
            }
            else
            {
                _extrapolationRatio = 1;
            }
        }

        #endregion

        #region Accessor Method

        ///<summary>
        ///</summary>
        ///<param name="quarters"></param>
        ///<returns></returns>
        public double Get(int quarters)
        {
            if (quarters <= _upperBound)
            {
                return _discountFactors[quarters];
            }
            double result = _discountFactors[_upperBound];
            for (int i = _upperBound; i < quarters; i++)
            {
                result = result * _extrapolationRatio;
            }
            return result;
        }

        #endregion

        #region Private Fields

        private readonly double[] _discountFactors;
        private readonly int _upperBound;
        private readonly double _extrapolationRatio;

        #endregion
    }
}