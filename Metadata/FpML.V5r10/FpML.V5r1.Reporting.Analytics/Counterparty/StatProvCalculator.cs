using System;
using System.Collections.Generic;
using System.Text;

namespace National.QRSC.Analytics.CreditMetrics
{
    /// <summary>
    /// Class to calculate statistical provisioning 
    /// </summary>
    public class StatProvCalculator
    {

        /// <summary>
        /// Method to calculate statistical provisioning
        /// 
        /// PD: setup PD vector using monthly MDP table
        /// LGD: use non-stressed LGD values
        /// 
        /// IStatProv - input data matching statprov calc interface
        /// ILGDTable - unstressed lgd table
        /// IMDPTable - MDP table
        /// </summary>
        /// 
        public static decimal[] CalculateStatProv(IStatProv inputData, ILGDTable lgdTable, IMDPTable pdTable)
        {

            //get lgd and pd values
            decimal lgdValue = lgdTable.GetLGD(lgdTable.TranslateToLendingCategory(inputData.LendingCategory),
                   lgdTable.TranslateToLGDCounterpartyType(inputData.LGDCounterpartyType));
            decimal[] mdpRange = PDTableUtils.SetupPDVector(
                                                  Convert.ToInt32(inputData.CounterpartyRatingID),
                                                  Convert.ToInt32(inputData.FrequencyOfFuturePoints),
                                                  inputData.timeBuckets.Length,
                                                  pdTable);
            //calculate stat prov           
            return CalculateStatProv(inputData.timeBuckets, mdpRange, lgdValue, inputData.epe, inputData.discountFactors);
        }


        /// <summary>
        /// Method to calculate statistical provisioning
        /// 
        /// IStatProv - input data matching statprov calc interface
        /// ILGDTable - unstressed lgd table
        /// IMDPTable - MDP table
        /// IGlobalSetup - global setup table - has CoC value
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="lgdTable"></param>
        /// <param name="pdTable"></param>
        /// <param name="globTable"></param>
        /// <returns></returns>
        public static decimal[] CalculateStatProv(IStatProv inputData, ILGDTable lgdTable, IMDPTable pdTable, IGlobalSetup globTable)
        {

            //get lgd and pd values
            decimal lgdValue = lgdTable.GetLGD(lgdTable.TranslateToLendingCategory(inputData.LendingCategory),
                   lgdTable.TranslateToLGDCounterpartyType(inputData.LGDCounterpartyType));
            decimal[] mdpRange = PDTableUtils.SetupPDVector(
                                                  Convert.ToInt32(inputData.CounterpartyRatingID),
                                                  Convert.ToInt32(inputData.FrequencyOfFuturePoints),
                                                  inputData.timeBuckets.Length,
                                                  pdTable);

            //get cost of capital discount factors
            decimal costCapital = globTable.GetCostOfCaptial(0);
            decimal[] cocDiscountFactors = ROECalculator.GetCostOfCapitalDFs(inputData.timeBuckets, costCapital); 
            //calculate stat prov           
            return CalculateStatProv(inputData.timeBuckets, mdpRange, lgdValue, inputData.epe, cocDiscountFactors);
        }

        /// <summary>
        /// Method to calculate statistical provisioning
        /// 
        /// PD: setup PD vector using monthly MDP table
        /// LGD: use non-stressed LGD values
        /// 
        /// IStatProv - input data matching statprov calc interface
        /// </summary>
        /// 
        public static decimal[] CalculateStatProv(IStatProv inputData)
        {

            //get lgd and pd values
            ILGDTable lgdTable = LGDTable.GetInstance();
            IMDPTable pdTable = MDPTable.GetInstance();
            pdTable.Validate();
            decimal lgdValue = lgdTable.GetLGD(lgdTable.TranslateToLendingCategory(inputData.LendingCategory),
                   lgdTable.TranslateToLGDCounterpartyType(inputData.LGDCounterpartyType));
            decimal[] mdpRange = PDTableUtils.SetupPDVector(
                                                  Convert.ToInt32(inputData.CounterpartyRatingID),
                                                  Convert.ToInt32(inputData.FrequencyOfFuturePoints),
                                                  inputData.timeBuckets.Length,
                                                  pdTable);
            //calculate stat prov
            return CalculateStatProv(inputData.timeBuckets, mdpRange, lgdValue, inputData.epe, inputData.discountFactors);

        }

        /// <summary>
        /// Method to calculate statistical provisioning.
        /// 
        /// Statistical provisioning is calculated based on the expected loss:
        /// SP = PD (probability of default) * Exposure * LGD (loss given default)
        /// 
        /// where exposure is expected value of exposure at default.
        ///
        /// Note: Will generally use (expected exposure) EE value for expected exposure
        /// at t. So will have EE profile at different t values.
        /// 
        /// Note: Probability of default values are assumed undiscounted. A discount
        /// factor at time t based on cost of capital is used as modifer.
        /// 
        /// Note: All input parameter vectors should be consistent with each other
        /// and match monthly intervals required
        /// 
        /// </summary>
        /// <param name="timeBuckets">vector defining required time buckets (uses offsets in integer days)</param>
        /// <param name="PD">undiscounted probability of default (for counterparty)</param>
        /// <param name="LGD">loss given default. This is the percentage of
        /// exposure which will be lost after all recovery efforts</param>
        /// <param name="epe">expected exposure (EE) profile</param>
        /// <param name="df">discount factors based on cost of capital</param>
        /// <returns>Vector of Stat Prov values</returns>
        public static decimal[] CalculateStatProv(int[] timeBuckets, decimal[] pd, decimal LGD, decimal[] epe, decimal[] df)
        {
            if (pd.Length != timeBuckets.Length) throw new Exception("PD and timeBuckets lengths do not match for stat prov calc");
            if (epe.Length != timeBuckets.Length) throw new Exception("EPE and timeBuckets lengths do not match for stat prov calc");

            decimal[] sum = new decimal[timeBuckets.Length];

            for (int j = 0; j < timeBuckets.Length; ++j)
            {
                sum[j] = pd[j] * LGD * epe[j] * df[j];
            }

           return sum; 

        }

       

        /// <summary>
        /// Calculate stat prov at set of future dates
        /// 
        /// - based on expected loss.
        /// - undiscounted stat prov
        /// discounted at discount rate because shifting in time to
        /// give upfront stat prov amount.
        /// Note - all vectors should be the same length.
        /// </summary>
        /// <param name="pd">PD for set of future dates</param>
        /// <param name="LGD">LGD for set of future dates</param>
        /// <param name="exp">Exposure measure for set of future dates</param>
        /// <param name="df">Market discount rate for set of future dates</param>
        /// <returns></returns>
        public static double[] CalculateUndiscountedStatProv(double[] pd, double[] LGD, double[] exp)
        {
            double[] sum = new double[exp.Length];

            for (int j = 0; j < exp.Length; ++j)
            {
                sum[j] = pd[j] * LGD[j] * exp[j];
            }
            return sum;
        }

        /// <summary>
        /// Calculate stat prov at set of future dates
        /// 
        /// - based on expected loss.
        /// - discounted stat prov
        /// discounted at discount rate because shifting in time to
        /// give upfront stat prov amount.
        /// Note - all vectors should be the same length.
        /// </summary>
        /// <param name="pd">PD for set of future dates</param>
        /// <param name="LGD">LGD for set of future dates</param>
        /// <param name="exp">Exposure measure for set of future dates</param>
        /// <param name="df">Market discount rate for set of future dates</param>
        /// <returns></returns>
        public static double[] CalculateStatProv(double[] pd, double[] LGD, double[] exp, double[] df)
        {
            double[] sum = new double[exp.Length]; 

            for (int j = 0; j < exp.Length; ++j)
            {
                sum[j] = pd[j] * LGD[j] * exp[j] * df[j];
            }
            return sum; 
        }

        public static SortedDictionary<int,decimal> CalculateStatProv(decimal[] mdp, decimal LGD, decimal[] epe, decimal[] df, int[] timeBuckets)
        {
            if (timeBuckets.Length != mdp.Length) throw new Exception("TimeBuckets and MDP array lengths do not match for stat prov calc");
            if (timeBuckets.Length != epe.Length) throw new Exception("TimeBuckets and EPE array lengths do not match for stat prov calc");
            if (timeBuckets.Length != df.Length) throw new Exception("TimeBuckets and DF array lengths do not match for stat prov calc");

            SortedDictionary<int, decimal> result = new SortedDictionary<int, decimal>();
            for (int j = 0; j < epe.Length; j++)
            {
                result.Add(timeBuckets[j], mdp[j] * LGD * epe[j] * df[j]);
            }
            return result;
        }


        //used for matching EPE values with different lags
        public static decimal[] MapEPEValues(IDictionary<int, decimal> epeData, int[] timeBuckets)
        {
            List<decimal> result = new List<decimal>();
            int k = 0;
            decimal sum = 0.0M;
            foreach (KeyValuePair<int, decimal> item in epeData)
            {
                if (k >= timeBuckets.Length) break;

                if (item.Key < timeBuckets[k])
                {
                    sum = sum + item.Value;
                }
                else if (item.Key == timeBuckets[k])
                {
                    result.Add(sum + item.Value);
                    sum = 0.0M;
                    k++;
                }
                else
                {
                    result.Add(sum);
                    sum = item.Value;
                    k++;
                }
            }
            return result.ToArray();
        }
    }
}
