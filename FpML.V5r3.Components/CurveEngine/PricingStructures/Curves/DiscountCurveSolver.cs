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

using System;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Orion.Analytics.Interpolations;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.Analytics.Solvers;
using Orion.ModelFramework;

namespace Orion.CurveEngine.PricingStructures.Curves
{
    internal class DiscountCurveSolver : IObjectiveFunction
    {
        private readonly int[] _days;
        private readonly string[] _instruments;
        private readonly decimal[] _rates;
        private readonly decimal[] _adjustments;
        private readonly DateTime _baseDate;
        private readonly double _spread;
        private readonly string _spreadAssetId;
        private readonly BasicAssetValuation _spreadAssetValuation;
        private readonly double _valueForZeroAdjustment;
        private readonly int _spreadStartIndex;
        private readonly int _spreadEndIndex;
        private readonly int _spreadDays;
        private readonly NamedValueSet _properties;
        private readonly ILogger _logger;
        private readonly ICoreCache _cache;
        private readonly IBusinessCalendar _fixingCalendar;
        private readonly IBusinessCalendar _rollCalendar;
        private readonly string _nameSpace;

        public DiscountCurveSolver(ILogger logger, ICoreCache cache, string nameSpace, DateTime baseDate, NamedValueSet properties, IEnumerable<Triplet<string, decimal, int>> assets, 
            decimal[] adjustments, string spreadAssetId, BasicAssetValuation spreadAssetValuation,
            int spreadDays, double spread, int spreadStartIndex, int spreadEndIndex,
            IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
        {
            _nameSpace = nameSpace;
            _properties = properties;
            _spreadAssetId = spreadAssetId;
            _spreadAssetValuation = spreadAssetValuation;
            _baseDate = baseDate;
            _spread = spread;
            _spreadStartIndex = spreadStartIndex;
            _spreadEndIndex = spreadEndIndex;
            _adjustments = adjustments;
            _instruments = assets.Select(a => a.First).ToArray();
            _rates = assets.Select(a => a.Second).ToArray();
            _days = assets.Select(a => a.Third).ToArray();
            _spreadDays = spreadDays;
            _logger = logger;
            _cache = cache;
            _fixingCalendar = fixingCalendar;
            _rollCalendar = rollCalendar;
            // Calculate the function value for zero spread
            decimal[] zeroAdjustments = adjustments.Select(a => 0m).ToArray();
            _valueForZeroAdjustment
                = RateCurve.CalculateImpliedQuote(_logger, _cache, _nameSpace, _baseDate, _properties, _instruments, _rates, zeroAdjustments, _spreadAssetId, _spreadAssetValuation,
                _fixingCalendar, _rollCalendar);
        }

        public double Value(double trialAdjustment)
        {
            // Set the adjustments
            // Adjust the rates with the answer
            if (_spreadStartIndex == 0)
            {
                for (int i = 0; i <= _spreadEndIndex; i++)
                {
                    _adjustments[i] = (decimal)trialAdjustment;
                }
            }
            else
            {
                int startDays = _days[_spreadStartIndex - 1];

                // if there is only one point then don't interpolate
                if (_spreadDays == startDays)
                {
                    _adjustments[_spreadStartIndex] = (decimal)trialAdjustment;
                }
                else
                {
                    var x = new double[] { startDays, _spreadDays };
                    var y = new[] { (double)_adjustments[_spreadStartIndex - 1], trialAdjustment };
                    var interpolation = new LinearInterpolation();
                    interpolation.Initialize(x, y);

                    for (int i = _spreadStartIndex; i <= _spreadEndIndex; i++)
                    {
                        int days = _days[i];
                        _adjustments[i] = (decimal)interpolation.ValueAt(days, false);
                    }
                }
            }
            double valueForAdjustment = RateCurve.CalculateImpliedQuote(_logger, _cache, _nameSpace, _baseDate, _properties, _instruments, _rates, _adjustments, _spreadAssetId,
                _spreadAssetValuation, _fixingCalendar, _rollCalendar);
            return valueForAdjustment - _valueForZeroAdjustment - _spread;
        }

        //private readonly List<string> _fraInstruments;
        //private readonly List<decimal> _fraRates;
        //private readonly List<decimal> _adjustments;
        //private readonly string _spreadAssetId;
        //private readonly BasicAssetValuation _spreadAssetValuation;
        ////private readonly MarketEnvironment _marketEnvironment;
        //private readonly DateTime _baseDate;
        //private readonly double _spread;
        //private readonly double _valueForZeroAdjustment;
        //private readonly int _spreadStartIndex;
        //private readonly int _spreadEndIndex;

        //public DiscountCurveSolver(DateTime baseDate,
        //    List<string> fraInstruments, List<decimal> fraRates, List<decimal> adjustments,
        //    string spreadAssetId, BasicAssetValuation spreadAssetValuation,
        //    double spread, int spreadStartIndex, int spreadEndIndex)
        //{
        //    _fraInstruments = fraInstruments;
        //    _fraRates = fraRates;
        //    _spreadAssetId = spreadAssetId;
        //    _spreadAssetValuation = spreadAssetValuation;
        //    _baseDate = baseDate;
        //    _spread = spread;
        //    _spreadStartIndex = spreadStartIndex;
        //    _spreadEndIndex = spreadEndIndex;
        //    _adjustments = adjustments;

        //    // Calculate the function value for zero spread
        //    List<decimal> zeroAdjustments = adjustments.Select(a => 0m).ToList();
        //    _valueForZeroAdjustment = DiscountCurve.CalculateImpliedQuote(_baseDate, _fraInstruments, _fraRates, zeroAdjustments, _spreadAssetId, _spreadAssetValuation);
        //}

        //public double Value(double trialAdjustment)
        //{
        //    // Set the adjustments
        //    for (int i = _spreadStartIndex; i <= _spreadEndIndex; i++)
        //    {
        //        _adjustments[i] = (decimal)trialAdjustment;
        //    }
        //    double valueForAdjustment = DiscountCurve.CalculateImpliedQuote(_baseDate, _fraInstruments, _fraRates, _adjustments, _spreadAssetId, _spreadAssetValuation);
        //    return valueForAdjustment - _valueForZeroAdjustment - _spread;
        //}

        public double Derivative(double x)
        {
            throw new NotImplementedException();
        }
    }
}
