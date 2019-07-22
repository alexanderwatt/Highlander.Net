using System;
using FpML.V5r10.Reporting.ModelFramework;

namespace Orion.Analytics.Interpolations

{
    /// <summary>
    /// Piece-wise Linear Interpolation.
    /// </summary>
    public class PiecewiseConstantZeroRateInterpolation : PiecewiseConstantInterpolation
    {
        /// <summary>
        /// The empty constructor
        /// </summary>
        public PiecewiseConstantZeroRateInterpolation()
        {
        }

        /// <param name="times">Sample time points (N), sorted ascending</param>
        /// <param name="dfs">Sample rates values (N) of each segment starting at the corresponding sample point.</param>
        protected PiecewiseConstantZeroRateInterpolation(double[] times, double[] dfs)
        {
            if (times.Length != dfs.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (times.Length < 2)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(times));
            }
            var rates = new double[dfs.Length];
            for (int i = 0; i < times.Length; i++)
            {
                if (Math.Abs(times[i]) > 0)
                {
                    rates[i] = -Math.Log(dfs[i]) / times[i];
                }
                rates[i] = -Math.Log(dfs[i + 1]) / times[i + 1];               
            }
            Times = times;
            Rates = rates;
        }

        /// <param name="times">Sample time points (N), sorted ascending</param>
        /// <param name="dfs">Sample rates values (N) of each segment starting at the corresponding sample point.</param>
        public override void Initialize(double[] times, double[] dfs)
        {
            if (times.Length != dfs.Length)
            {
                throw new ArgumentException("ArgumentVectorsSameLength");
            }
            if (times.Length < 2)
            {
                throw new ArgumentException(string.Format("ArrayTooSmall"), nameof(times));
            }
            var rates = new double[dfs.Length];
            for (int i = 0; i < times.Length; i++)
            {
                var interval = Math.Abs(times[i]);
                if (interval > 0)
                {
                    rates[i] = -Math.Log(dfs[i]) / times[i];
                }
                else
                {
                    if (i < times.Length - 1)
                    {
                        rates[i] = -Math.Log(dfs[i + 1]) / times[i + 1];
                    }
                    else
                    {
                        rates[i] = -Math.Log(dfs[i]) / .000001;
                    }
                }           
            }
            Times = times;
            Rates = rates;
        }

        ///<summary>
        /// The clone method makes a shallow copy of the current interpolation class.
        ///</summary>
        ///<returns></returns>
        public override IInterpolation Clone()
        {
            return new PiecewiseConstantZeroRateInterpolation(Times, Rates);
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time value</param>
        /// <param name="extrapolation">This is not currently implemented.</param>
        /// <returns></returns>
        public override double ValueAt(double time, bool extrapolation)
        {
            if (time <= Times[0])
            {
                return extrapolation ? ValueAt(Times[0]) : 0.0;
            }
            return ValueAt(time);
        }

        /// <summary>
        /// Perform a wing model interpolation on a vector of values
        /// We must assume the points are arranged x0 &lt;=x &lt;= x1 for this to work/>
        /// </summary>
        /// <param name="time">The time axis value</param>
        /// <returns></returns>
        public override double ValueAt(double time)
        {
            int k = LeftBracketIndex(time);
            return Math.Exp(Rates[k] * -time);
        }
    }
}