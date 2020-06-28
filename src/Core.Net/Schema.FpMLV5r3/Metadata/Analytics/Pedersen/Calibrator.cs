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

#region Usings

using System;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using MathNet.Numerics.LinearAlgebra.Double;
using Highlander.Reporting.Analytics.V5r3.Utilities;

#endregion

namespace Highlander.Reporting.Analytics.V5r3.Pedersen
{

    public class Calibrator
    {
        #region declarations

        private readonly Func<Vector<double>, double> _f;
        private readonly Func<Vector<double>, Vector<double>> _g;
        private DenseVector _initialGuess;
        private readonly ObjectiveFunction _objective;
        private readonly Parameters _p;
        public int Iteration { get; set; }

        #endregion

        public Calibrator(ObjectiveFunction obj, Parameters par)
        {
            _objective = obj;
            _f = _objective.ObjFun;
            _g = _objective.ObjGrad;
            _p = par;
        }

        public void Go()
        {
            _initialGuess = new DenseVector(_p.NumberOfExpiries * _p.NumberOfTenors);
            double initialGuess = _p.AverageSwaptionImpliedVolatility;
            if (initialGuess == 0)
            {
                initialGuess = _p.AverageCapletImpliedVolatility;
            }
            if (initialGuess == 0)
            {
                throw new Exception("The selected time range does not include any calibration targets.");
            }
            if (_objective.ExpForm)
            {
                for (var i = 0; i < _initialGuess.Count; i++)
                {
                    _initialGuess[i] = Math.Log(initialGuess) / 20;
                }
            }
            else
            {
                for (var i = 0; i < _initialGuess.Count; i++)
                {
                    _initialGuess[i] = initialGuess;
                }
            }
            var sw = new StopWatch();
            _objective.StartOtherThreads();
            var solution = BfgsSolver.Solve(_initialGuess, _f, _g);
            _objective.Finished = true;
            sw.Stop();
            TimeSpan ts = sw.GetElapsedTime();
            Pedersen.Write($"  Value x: {solution[0]}\n", "cal");
            _objective.OutputResult(solution);
            Pedersen.Write($"  Process Time: {ts.Hours:d2}:{ts.Minutes:d2}:{ts.Seconds:d2}:{ts.Milliseconds:d3}\n", "cal");
        }
    }
}
