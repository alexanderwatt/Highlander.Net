
#region Using directives

using System;
using System.Data;
using System.Diagnostics;
//using Orion.Analytics.Options;
using Orion.Numerics.Distributions.Continuous;

#endregion

namespace Orion.Analytics.PricingEngines
{
    /// <summary>
    /// European option engine using analytic formulas.
    /// </summary>
    public class EuropeanAnalyticalEngine : VanillaEngine
    {

        #region Implementation of IPricingEngine
        public override string UniqueId
        {
            get { return GetType().Name; }
        }

        public override void Calculate(DataRow args, DataRow results)
        {
            // parse Arguments from DataRow
            var voa = new VanillaOptionArguments(args);

            double dividendDiscount = Math.Exp(
                - voa.dividendYield * voa.residualTime );
            double riskFreeDiscount = Math.Exp(
                - voa.riskFreeRate * voa.residualTime );
            double stdDev = voa.volatility * Math.Sqrt(voa.residualTime);

            double D1 = Math.Log( voa.underlying/voa.strike) / stdDev +
                        stdDev/2.0 + (voa.riskFreeRate-voa.dividendYield) *
                                     voa.residualTime/stdDev;
            double D2 = D1-stdDev;

            double alpha, beta, NID1;
            switch (voa.optionType) 
            {
                case Option.Type.Call:
                    alpha = CumulativeNormalDistribution.Function(D1);
                    beta = CumulativeNormalDistribution.Function(D2);
                    NID1 = CumulativeNormalDistribution.FunctionDerivative(D1);
                    break;
                case Option.Type.Put:
                    alpha = CumulativeNormalDistribution.Function(D1) - 1.0;
                    beta = CumulativeNormalDistribution.Function(D2) - 1.0;
                    NID1 = CumulativeNormalDistribution.FunctionDerivative(D1);
                    break;
                case Option.Type.Straddle:
                    alpha = 2.0 * CumulativeNormalDistribution.Function(D1) - 1.0;
                    beta = 2.0 * CumulativeNormalDistribution.Function(D2) - 1.0;
                    NID1 = 2.0 * CumulativeNormalDistribution.FunctionDerivative(D1);
                    break;
                default:
                    // can't happen since args are validated before
                    Debug.Fail("Illegal option type");
                    return;
            }

            results["value"] = voa.underlying * dividendDiscount * alpha -
                               voa.strike * riskFreeDiscount * beta;

            //  only fill if column is in result row
            if( results.Table.Columns.IndexOf("delta") >= 0)
                results["delta"] = dividendDiscount * alpha;
            if( results.Table.Columns.IndexOf("gamma") >= 0)
                results["gamma"] = NID1 * dividendDiscount /
                                   (voa.underlying * stdDev);
            if( results.Table.Columns.IndexOf("theta") >= 0)
                results["theta"] = - voa.underlying * NID1 *
                                   voa.volatility * dividendDiscount /
                                   (2.0 * Math.Sqrt(voa.residualTime)) +
                                   voa.dividendYield * voa.underlying *
                                   alpha * dividendDiscount -
                                   voa.riskFreeRate * voa.strike *
                                   riskFreeDiscount * beta;
            if( results.Table.Columns.IndexOf("rho") >= 0)
                results["rho"] = voa.residualTime * riskFreeDiscount *
                                 voa.strike * beta;
            if( results.Table.Columns.IndexOf("dividendRho") >= 0)
                results["dividendRho"] = - voa.residualTime *
                                         dividendDiscount * voa.underlying * alpha;
            if( results.Table.Columns.IndexOf("vega") >= 0)
                results["vega"] = voa.underlying * NID1 *
                                  dividendDiscount * Math.Sqrt(voa.residualTime);
        }
        #endregion

    }
}