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

namespace Highlander.Equities
{
    /// <summary>
    /// </summary>
    public class ZeroCurve
    {
        private double[] _r; //Rate Vector
        private double[] _t; // Corresponding time vector

        /// <summary>
        /// set the number of div points
        /// </summary>
        public int RatePoints { get; set; } = 0;

        /// <summary>
        /// get rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetR(int idx)
        {
            return _r[idx];
        }

        /// <summary>
        /// get time item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetT(int idx)
        {
            return _t[idx];
        }

        /// <summary>
        /// set the  rate and time 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="r"></param>
        /// <param name="t"></param>
        public void SetR(int idx, double r, double t)
        {
            _r[idx] = r;
            _t[idx] = t;
        }


        /// <summary>
        /// make the arrays
        /// </summary>
        public void MakeArrays()
        {
            if (_r != null) return;
            _r = new double[RatePoints];
            _t = new double[RatePoints];
        }

        /// <summary>
        /// empty the arrays
        /// </summary>
        public void EmptyArrays()
        {
            _r = null;
            _t = null;
        }

        /// <summary>
        /// Linear interpolation
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double LinInterp(double x)
        {
            if (x < GetT(0))
            {
                return GetR(0);
            }
            if (x >= GetT(RatePoints - 1))
            {
                return GetR(RatePoints - 1);
            }
            for (int idx = 0; idx < RatePoints - 1; idx++)
            {
                if( x >= GetT(idx) && x < GetT(idx+1) )
                {
                    double temp = GetR(idx) + ( (GetR(idx+1) - GetR(idx) ) * (x - GetT(idx)) /
                                                (GetT(idx+1) - GetT(idx)) );
                    return temp;
                }
            }
            return 0.0;
         }

        /// <summary>
        /// compute the forward rate between t1 and t2
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public double ForwardRate(double t1, double t2)
        {
            if (t2 > t1)
            {
                double temp = (t2 * LinInterp(t2) - t1 * LinInterp(t1)) / (t2 - t1);
                return temp;
            }
            return 0.0;
        }
    }
}
