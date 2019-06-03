using System;
using System.Collections;
// COM interop attributes
// some useful attributes


namespace Orion.Analytics.Processes
{
    /// <summary>
    /// Time grid class.
    /// </summary>
    // TODO: Maybe I should impl. ICollection (or even IList) 
    //       and inherit from ReadOnlyCollectionBase ??
    public class TimeGrid : ITimeGrid // public std::SparseVector<Time> 
    {
        ///<summary>
        ///</summary>
        public TimeGrid()
        { }

        /// <summary>
        /// Regularly spaced time-grid.
        /// </summary>
        /// <param name="end"></param>
        /// <param name="steps"></param>
        public TimeGrid(double end, int steps)
        {
            _timeSparseVector = new double[steps + 1];
            double dt = end / steps;
            for (int i = 0; i <= steps; i++)
                _timeSparseVector[i] = dt * i;
        }

        /// <summary>
        /// Time grid with mandatory time-points (regularly spaced between them).
        /// </summary>
        TimeGrid(IList times, int steps)
        {
            double dtMax = Double.MaxValue;
            // The resulting timegrid have points at times listed in the input
            // list. Between these points, there are inner-points which are
            // regularly spaced.
            if (steps == 0)
            {
                double prev = 0.0;
                foreach (double t in times)
                {
                    if (prev == t) continue;	// skip a 0.0 as first element
                    double diff = t - prev;
                    if (diff < dtMax) dtMax = diff;
                    prev = t;
                }
            }
            else
            {
                dtMax = (double)times[times.Count - 1] / steps;//TODO fix this.
            }

            //DoubleVector coll = new DoubleVector();
            //double periodBegin = 0.0;
            //foreach (double periodEnd in times)
            //{
            //    if (periodBegin >= periodEnd)
            //        continue;
            //    var nSteps = (int)((periodEnd - periodBegin) / dtMax + 1.0);
            //    double dt = (periodEnd - periodBegin) / nSteps;
            //    for (int n = 0; n < nSteps; n++)
            //        coll.Add(periodBegin + n * dt);
            //    periodBegin = periodEnd;
            //}
            //coll.Add(periodBegin); // Note periodBegin = periodEnd
            //timeSparseVector = new double[coll.Count];
            //coll.CopyTo(timeSparseVector);
        }

        private readonly double[] _timeSparseVector;

        public int Count => _timeSparseVector.Length;

        public double this[int i] => _timeSparseVector[i];

        public int FindIndex(double time)
        {
            // Is the tree sorted??
            // int i = Array.BinarySearch(timeSparseVector, time);
            int i = Array.IndexOf(_timeSparseVector, time);
            if (i >= 0) return i;
            throw new ApplicationException(
                "GridNotFound");
        }

        public double Dt(int i)
        {
            return _timeSparseVector[i + 1] - _timeSparseVector[i];
        }
    }
}