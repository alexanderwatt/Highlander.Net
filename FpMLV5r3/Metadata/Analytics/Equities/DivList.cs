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

namespace Highlander.Reporting.Analytics.V5r3.Equities
{
    /// <summary>
    /// 
    /// </summary>
  public class DivList
  {
        private double[] _d;
        private double[] _t;

        public DivList()
        {
            DividendPoints = 0;
        }

        /// <summary>
        /// set the number of div points
        /// </summary>
        public int DividendPoints { get; set; }

        /// <summary>
        /// get rate item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetD(int idx)
        {
            if (idx < DividendPoints)
            {
                return _d[idx];
            }
            return 0.0;
        }

            /// <summary>
        /// get time item
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double GetT(int idx)
        {
            if (idx < DividendPoints)
            {
                return _t[idx];
            }
            return 0.0;
        }

            /// <summary>
        /// set the  dividend and time 
        /// </summary>
        /// <param name="idx"></param>
        /// <param name="d"></param>
        /// <param name="t"></param>
        public void SetD(int idx, double d, double t)
        {
            if (idx < DividendPoints)
            {
                _d[idx] = d;
                _t[idx] = t;
            }
        }

        /// <summary>
        /// make the arrays
        /// </summary>
        public void MakeArrays()
        {
          if (_d == null)
          {
              _d = new double[DividendPoints];
              _t = new double[DividendPoints];
          }
        }

        /// <summary>
        /// empty the arrays
        /// </summary>
        public void EmptyArrays()
        {
          _d = null;
          _t = null;
        }
  }
}
