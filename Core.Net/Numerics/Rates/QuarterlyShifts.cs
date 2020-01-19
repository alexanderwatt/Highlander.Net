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
    public class QuarterlyShifts
    {
        #region Constructors

        /// <summary>
        /// Time dependent shift.
        /// </summary>
        /// <param name="shift"></param>
        public QuarterlyShifts(double[] shift)
        {
            _shift = shift;
            _const = false;
        }

        /// <summary>
        /// For a constant shift.
        /// </summary>
        /// <param name="shift"></param>
        public QuarterlyShifts(double shift)
        {
            _constShift = shift;
            _const = true;
        }

        #endregion

        #region Accessor Method

        ///<summary>
        ///</summary>
        ///<param name="quarters"></param>
        ///<returns></returns>
        public double Get(int quarters)
        {
            if (_const)
            {
                return _constShift;
            }
            if (quarters >= _shift.Length)
            {
                return _shift[_shift.Length - 1];
            }
            return _shift[quarters];
        }

        #endregion

        #region Private Fields

        private readonly bool _const;
        private readonly double _constShift;
        private readonly double[] _shift;

        #endregion
    }
}