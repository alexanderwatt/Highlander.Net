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
                else
                {
                    _currentIndex[i] = _lowerBounds[i];

                    ++numberOfShifts;

                    // increase previous index...
                    //
                    continue;
                }
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