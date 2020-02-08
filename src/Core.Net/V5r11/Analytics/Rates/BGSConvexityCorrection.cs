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
using Highlander.Reporting.Analytics.V5r3.Distributions;

namespace Highlander.Reporting.Analytics.V5r3.Rates
{
    /// <summary>
    /// Class encapsulates the functionality to compute the convexity correction
    /// factor that is applied to a vanilla cross currency swap at each roll
    /// date. The convexity correction represents the multiplicative factor that
    /// must be applied to the vanilla cross currency swap payoff.
    /// There is no data validation by the class as it is anticipated that
    /// inputs have been validated elsewhere in the BGS data flow.
    /// Class is "lightweight" and does not persist any data.
    /// </summary>
    public class BGSConvexityCorrection
    {
        /// <summary>
        /// Arbitray Max value for infinity checking for the isLimitingCase when
        /// computing the convexity correction value
        /// </summary>
        private const double CleanupDifference = 0.02;

        // ---------------------------------------------------------------------
        // Business logic methods.
        // ---------------------------------------------------------------------
        /// <summary>
        /// Computes the BGS convexity correction. The magnitude of the
        /// correction, as described in the document "Pricing Analytics for a
        /// Balance Guaranteed Swap", prepared by George Scoufis,
        /// Reference Number GS-02042007/1 is:
        ///        F0*{1 - Phi(d1) - (L/B0)^(1 + 2*alpha/sigma^2)*phi(d2)}.
        /// Periods in the formulae for F0, d1 and d2 are computed with
        /// ACT/365 day count.</summary>
        /// <param name="tenor">The year fraction between the date of the most recent
        /// realised amortisation and  date to which the convexity  correction is required.</param>
        /// <param name="currentBondFactor">The current Bond Factor.</param>
        /// <param name="alpha">The paramter alpha in the Bond Factor curve
        /// model.</param>
        /// <param name="sigma">The paramter sigma in the Bond Factor curve
        /// model.</param>
        /// <param name="cleanUp">The clean up condition. If the current
        /// Bond Factor is less than or equal to the clean up, then the
        /// convexity correction is 0.0.</param>
        /// <returns>Convexity correction. Logic has been added to cater for
        /// the limiting cases: 1) sigma->0; 2) clean up ->0.</returns>
        public double ComputeBGSConvexityCorrection(double tenor,
                                                    double currentBondFactor,
                                                    double alpha,
                                                    double sigma,
                                                    double cleanUp)
        {
            // Declare and initialise the return variable.
            // The initialisation value has been chosen to correspond to the
            // case in which the clean up condition has been trigerred.
            double convexityCorrection = 0.0;

            // Compute the convexity correction.
            if (currentBondFactor > cleanUp)
            {
                // Clean up condition has not been trigerred.
                // Compute F0, the expected value of the Bond Factor.
                double f0 = ComputeF0(currentBondFactor, alpha, tenor);

                // Check for a limiting case.
                double moneyness = cleanUp / currentBondFactor;
                double exponent = 1 + 2 * alpha / (sigma * sigma);

                //bool isLimitingCase =
                //    (sigma <= double.Epsilon || cleanUp <= double.Epsilon ||
                //     double.IsInfinity(Math.Pow(moneyness, exponent)));
                bool isLimitingCase =
                    (sigma <= double.Epsilon || cleanUp <= double.Epsilon ||
                     (currentBondFactor - cleanUp) < CleanupDifference);

                if (isLimitingCase)
                {
                    // The two limiting cases can be absorbed into a single
                    // formula becase when L->0 the convexity correction
                    // reduces to f0, where L denotes the clean up barrier.
                    convexityCorrection = f0 * ((cleanUp - f0) < 0.0 ? 1.0 : 0.0);
                }
                else
                {
                    // Generic case.
                    // Compute phi(d1) and phi(d2).
                    double phid1 = ComputePhid1(cleanUp, f0, sigma, tenor);
                    double phid2 = ComputePhid2(cleanUp,
                                                f0,
                                                currentBondFactor,
                                                sigma,
                                                tenor);

                    // Compute the convexity correction.
                    convexityCorrection = f0 * (1 - phid1); // base correction

                    if (phid2 != 0.0)
                    {
                        // Full convexity correction.
                        convexityCorrection -=
                            f0 * Math.Pow(moneyness, exponent) * phid2;
                    }
                }
            }
            convexityCorrection = (convexityCorrection - cleanUp) < 0.0 ? 0.0 : convexityCorrection;

            return convexityCorrection;
        }

        // ---------------------------------------------------------------------
        // Private functions.
        // ---------------------------------------------------------------------
        /// <summary>
        /// Helper function used to compute F0 that is defined in the document 
        /// "Pricing Analytics for a Balance Guaranteed Swap", prepared by 
        /// George Scoufis, Reference Number: GS-02042007/1.
        /// </summary>
        /// <param name="currentBondFactor">The current Bond Factor.</param>
        /// <param name="alpha">The parameter alpha in the Bond Factor
        /// Model.</param>
        /// <param name="tenor">The tenor in ACT/365 day count from the
        /// most recent amortisation date to the required swap amortisation
        /// date.</param>
        /// <returns>Computed value of F0.</returns>
        private static double ComputeF0(double currentBondFactor,
                                        double alpha,
                                        double tenor)
        {
            double f0 = currentBondFactor * Math.Exp(alpha * tenor);

            return f0;
        }

        /// <summary>
        /// Helper function used to compute Phi(d1).
        /// If the tenor is 0, then the return value is 0.0.
        /// </summary>
        /// <param name="cleanUp">The clean up condition.</param>
        /// <param name="f0">The value of f0.</param>
        /// <param name="sigma">The parameter sigma in the Bond Factor
        /// curve model.</param>
        /// <param name="tenor">The tenor to which the convexity correction
        /// is required.</param>
        /// <returns>Phi(d1)</returns>
        private static double ComputePhid1(double cleanUp,
                                           double f0,
                                           double sigma,
                                           double tenor)
        {
            // Declare and initialise the return variable.
            double phid1 = 0.0;

            if (tenor > 0.0)
            {
                double numerator =
                    Math.Log(cleanUp / f0) - 0.5 * sigma * sigma * tenor;
                double denominator = sigma * Math.Sqrt(tenor);
                double d1 = numerator / denominator;

                // Compute phid1.
                phid1 = new NormalDistribution().CumulativeDistribution(d1);
            }

            return phid1;
        }

        /// <summary>
        /// Helper function used to compute Phi(d2).
        /// If the tenor is 0, then the return value is 0.0.
        /// </summary>
        /// <param name="cleanUp">The clean up condition.</param>
        /// <param name="f0">The value of f0.</param>
        /// <param name="currentBondFactor">The current Bond Factor.</param>
        /// <param name="sigma">The parameter sigma in the Bond Factor
        /// curve model.</param>
        /// <param name="tenor">The tenor to which the convexity correction
        /// is required.</param>
        /// <returns>Phi(d2)</returns>
        private static double ComputePhid2(double cleanUp,
                                           double f0,
                                           double currentBondFactor,
                                           double sigma,
                                           double tenor)
        {
            // Declare and initialise the return variable.
            double phid2 = 0.0;

            if (tenor > 0.0)
            {
                double numerator =
                    Math.Log(cleanUp * f0 / Math.Pow(currentBondFactor, 2.0)) +
                    0.5 * sigma * sigma * tenor;
                double denominator = sigma * Math.Sqrt(tenor);
                double d2 = numerator / denominator;

                // Compute phid2.
                phid2 = new NormalDistribution().CumulativeDistribution(d2);
            }

            return phid2;
        }
    }
}