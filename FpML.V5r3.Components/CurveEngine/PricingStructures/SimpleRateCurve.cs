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

#region Using directives

using System;
using System.Collections.Generic;
using System.Linq;
using Orion.Analytics.Interpolations;
using Orion.Analytics.DayCounters;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.ModelFramework;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;

#endregion

namespace Orion.CurveEngine.PricingStructures
{
    /// <summary>
    /// This is an extension to the Market structure used by CurveGen
    /// </summary>
    public class SimpleRateCurve : IPricingStructure//TODO remove linearinterpolation and daycount.
    {
        private IInterpolation _interp;

        private readonly Market _wrapped;

        #region Properties

        /// <summary>
        /// The CalculationDate for the YieldCurve
        /// </summary>
        public DateTime BaseDate => _wrapped.Items1[0].baseDate.Value;

        #endregion

        /// <summary>
        /// Default constructor for Serialization purposes
        /// </summary>
        public SimpleRateCurve(Market mkt)
        {
            _wrapped = mkt;
            _interp = null;
        }

        #region Public Methods

        /// <summary>
        /// Return an array containing the discount factors from this rate curve
        /// They are arranged in ascending tenor order
        /// </summary>
        /// <returns></returns>
        public double[] GetDiscountFactors()
        {
            double[] factors = null;
            if (_wrapped.Items1 != null)
            {
                factors = ((YieldCurveValuation) _wrapped.Items1[0]).discountFactorCurve.point.Select(a => (double) a.mid).ToArray();
            }
            return factors;
        }

        /// <summary>
        /// Return an array of elapsed days between the Calculation date and the date of each discount factor
        /// </summary>
        /// <returns></returns>
        public int[] GetDiscountFactorOffsets()
        {
            int[] offsets = null;
            if (_wrapped.Items1 != null)
            {
                var tc = ((YieldCurveValuation)_wrapped.Items1[0]).discountFactorCurve;
                var valDate = BaseDate;
                // Step through each point and calculate the elapsed days between each point term value and the base
                var dfo = tc.point.Select(p => ((DateTime) p.term.Items[0]).Subtract(valDate)).Select(elapsed => elapsed.Days).ToList();
                offsets = new int[dfo.Count];
                dfo.CopyTo(offsets);
            }
            return offsets;
        }

        /// <summary>
        /// Return an array of elapsed days between the Calculation date and the date of each discount factor
        /// </summary>
        /// <returns></returns>
        public DateTime[] GetDiscountFactorDates()
        {
            DateTime[] dates = null;
            if (_wrapped.Items1 != null)
            {
                var tc = ((YieldCurveValuation)_wrapped.Items1[0]).discountFactorCurve;
                var dfo = tc.point.Select(p => (DateTime) p.term.Items[0]).ToList();
                dates = new DateTime[dfo.Count];
                dfo.CopyTo(dates);
            }
            return dates;
        }

        ///<summary>
        /// The type of curve evolution to use. The default is ForwardToSpot
        ///</summary>
        public PricingStructureEvolutionType PricingStructureEvolutionType
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public PricingStructureData PricingStructureData => throw new NotImplementedException();

        ///<summary>
        ///</summary>
        ///<param name="pt"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IValue GetValue(IPoint pt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the spot date for this yield curve
        /// If there is no defined spot then return the base date
        /// </summary>
        /// <returns></returns>
        public DateTime GetSpotDate()
        {
            var spot = _wrapped.Items1[0].spotDate?.Value ?? _wrapped.Items1[0].baseDate.Value;
            return spot;
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IIdentifier GetPricingStructureId()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<param name="pt"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IList<IValue> GetClosestValues(IPoint pt)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IInterpolatedSpace GetInterpolatedSpace()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public Pair<PricingStructure, PricingStructureValuation> GetFpMLData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Market GetMarket()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IIdentifier PricingStructureIdentifier
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IInterpolatedSpace Interpolator
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        /// <summary>
        /// Get the base date for this yield curve
        /// </summary>
        /// <returns></returns>
        public DateTime GetBaseDate()
        {
            return _wrapped.Items1[0].baseDate.Value;
        }

        /// <summary>
        /// Calculate a forward rate from the spot date out to the number of days specified
        /// This is a simplified version of a forward rate between two arbitrary dates
        /// </summary>
        /// <param name="elapsedDays">The number of days from the spot date to calculate the forward rate to</param>
        /// <returns>The rate</returns>
        public double GetForwardRate(int elapsedDays)
        {
            var yf = GetDFDatesAsYF();
            var df = GetDiscountFactors();
            if (_interp == null)
            {
                _interp = new LinearInterpolation();
                _interp.Initialize(yf, df);
            }
            var accrual = elapsedDays/365.0;
            var fwdRate = (1.0d / _interp.ValueAt(accrual, true) - 1) / accrual;
            return fwdRate;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Get the discount factor dates and convert them to year fractions
        /// </summary>
        /// <returns></returns>
        private double[] GetDFDatesAsYF()
        {
            var dates = GetDiscountFactorDates();
            var baseDate = dates[0];
            var daycounts = Actual365.Instance;
            return dates.Select(date => daycounts.YearFraction(baseDate, date)).ToArray();
        }

        #endregion

        ///<summary>
        ///</summary>
        ///<param name="point"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public double Value(IPoint point)
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IInterpolation GetInterpolatingFunction()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public IDiscreteSpace GetDiscreteSpace()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public bool AllowExtrapolation()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a perturbed copy of the curve.
        /// </summary>
        /// <returns></returns>
        public Pair<NamedValueSet, Market> GetPerturbedCopy(Decimal[] values)
        {
            throw new NotImplementedException();
        }
    }
}