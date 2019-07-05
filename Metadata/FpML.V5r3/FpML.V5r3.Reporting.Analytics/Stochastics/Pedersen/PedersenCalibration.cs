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
using System.Diagnostics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.Optimization;
using Orion.Analytics.Processes;
using Orion.Analytics.Rates;
using Orion.Analytics.Stochastics.Volatilities;

#endregion

namespace Orion.Analytics.Stochastics.Pedersen
{
    public class PedersenCalibration
    {
        /// <summary>
        /// Main calibration method.
        /// </summary>
        /// <param name="timeGrid">Time discretisation</param>
        /// <param name="discount">Quarterly discount factors</param>
        /// <param name="shift">Quarterly shift values</param>
        /// <param name="correlation">Correlation matrix, corresponding the tenor time discretisation</param>
        /// <param name="targets">Caplet and swaption implied vols</param>
        /// <param name="parameters">Calibration settings</param>
        /// <param name="factors">Factors used</param>
        /// <param name="useCascade">Whether to apply cascade algorithm post-calibration</param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static InterestRateVolatilities Calibrate(PedersenTimeGrid timeGrid, QuarterlyDiscounts discount,
                                                         QuarterlyShifts shift, DenseMatrix correlation, CalibrationTargets targets,
                                                         CalibrationSettings parameters, int factors, bool useCascade, ref string output)
        {
            #region Initialisations

            targets.AdjustImpliedVolsForShift(discount, shift);
            var swaptionWeights = new SwaptionWeights(timeGrid.MaxExpiry, timeGrid.MaxTenor);
            swaptionWeights.Populate(discount, shift);
            var volatilities = new InterestRateVolatilities(timeGrid, factors, swaptionWeights, correlation);
            var objective = new CalibrationObjective(targets, timeGrid, volatilities, parameters);
            Func<Vector<double>, double> f = objective.ObjFun;
            Func<Vector<double>, Vector<double>> g = objective.ObjGrad;
            var initialGuess = new DenseVector(timeGrid.ExpiryCount * timeGrid.TenorCount);
            double guessValue = targets.AverageImpliedVol();
            if (guessValue == 0)
            {
                throw new Exception("The selected time range does not include any calibration targets.");
            }
            if (parameters.ExponentialForm)
            {
                for (var i = 0; i < initialGuess.Count; i++)
                {
                    initialGuess[i] = Math.Log(guessValue) / 20;
                }
            }
            else
            {
                for (var i = 0; i < initialGuess.Count; i++)
                {
                    initialGuess[i] = guessValue;
                }
            }
            Trace.WriteLine(String.Format("Calibration Starting..."));
            objective.StartOtherThreads();
            var solution = BfgsSolver.Solve(initialGuess, f, g);
            objective.Finished = true;
            Trace.WriteLine($"Value at extremum: {solution}");
            objective.PopulateVol(solution);
            objective.AverageAbsVol();

            #endregion

            #region Cascade

            if (useCascade)
            {
                Trace.WriteLine(String.Format("Applying Cascade Algorithm..."));
                var cascade = new CascadeAlgorithm(volatilities, timeGrid, swaptionWeights, targets, parameters.CascadeParam, factors);
                cascade.ApplyCascade();
            }

            #endregion

            #region Output

            output = objective.AverageAbsVol();
            return objective.ReturnVol(solution);

            #endregion

        }
    }
}