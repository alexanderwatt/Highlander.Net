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

using System;

namespace Orion.CurveEngine.Helpers
{
    ///<summary>
    ///</summary>
    public class ArrayIndex
    {
        private readonly int[] _currentIndex;
        private readonly int[] _lowerBounds;
        private readonly int[] _upperBounds;

        ///<summary>
        ///</summary>
        ///<param name="array"></param>
        public ArrayIndex(Array array)
        {
            _currentIndex = new int[array.Rank];
            _lowerBounds = new int[array.Rank];
            _upperBounds = new int[array.Rank];

            for (int i = 0; i < array.Rank; ++i)
            {
                _currentIndex[i] = array.GetLowerBound(i);
                _lowerBounds[i] = array.GetLowerBound(i);
                _upperBounds[i] = array.GetUpperBound(i);
            }
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public int[] GetCurrentIndexes()
        {
            return _currentIndex;
        }

        ///<summary>
        ///</summary>
        ///<param name="numberOfShifts"></param>
        ///<returns></returns>
        public int[] GetNextIndexes(ref int numberOfShifts)
        {
            for (int i = _currentIndex.Length - 1; i >= 0; i--)
            {
                if (_currentIndex[i] != _upperBounds[i])
                {
                    _currentIndex[i] = ++_currentIndex[i];
                    return _currentIndex;
                }
                _currentIndex[i] = _lowerBounds[i];
                ++numberOfShifts;
                // increase previous index...
                //
            }
            return null; //no more elements
        }

        ///<summary>
        ///</summary>
        ///<param name="array"></param>
        ///<returns></returns>
        public object GetValue(Array array)
        {
            return array.GetValue(_currentIndex);
        }
    }
}