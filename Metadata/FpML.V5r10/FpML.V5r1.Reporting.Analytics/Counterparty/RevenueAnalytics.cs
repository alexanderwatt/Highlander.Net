using System;
using System.Collections.Generic;
using System.Linq;


namespace National.QRSC.Analytics.Counterparty
{
    /// <summary>
    /// Class to calculate revenue 
    /// </summary>
    public class RevenueAnalytics
    {

        /// <summary>
        /// Aggregate cashflow amts for master date set
        /// </summary>
        /// <param name="nettedCshflowArr"></param>
        /// <param name="masterDateSet"></param>
        /// <returns></returns>
        public static decimal[] AggregateAllCashflows(IDictionary<DateTime, decimal> nettedCshflowArr, 
                                                      DateTime[] masterDateSet)
        {
            decimal[] result = new decimal[masterDateSet.Length];

            if (nettedCshflowArr.ContainsKey(masterDateSet[0]))
                result[0] = nettedCshflowArr[masterDateSet[0]];
            else result[0] = 0.0M;

            for (int i = 0; i < (masterDateSet.Length - 1); i++)
            {
                DateTime startDate = masterDateSet[i];
                DateTime endDate = masterDateSet[i+1];
                result[i+1] = AggregateCashflows(nettedCshflowArr, startDate, endDate);
            }
            
            return result;
        }

        /// <summary>
        /// Aggregate cashflow amts within specified date range
        /// </summary>
        /// <param name="nettedCshflowArr"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public static decimal AggregateCashflows(IDictionary<DateTime, decimal> nettedCshflowArr,
                                                 DateTime startDate, DateTime endDate)
        {
            ICollection<DateTime> keys = nettedCshflowArr.Keys;
            IList<DateTime> keysInPeriod = new List<DateTime>();

            foreach (DateTime key in keys)
            {
                if ((key.CompareTo(startDate) > 0) && (key.CompareTo(endDate) <= 0)) keysInPeriod.Add(key);
            }

            decimal sum = 0.0M;
            foreach (DateTime key in keysInPeriod)
            {
                sum = sum + nettedCshflowArr[key];
            }
            return sum;
        }

        /// <summary>
        /// Function to calculate revenue in the interim 
        /// 
        /// Net projected cashflows are aggregated over (t,t+1) time period
        /// to obtain revenue at t.
        /// Cashflows are in revenueArr array in date, amount format.
        /// </summary>
        /// <param name="revenueArr">revenue, time bucket array</param>
        /// <param name="revenueBuckets">sorted buckets to calculate revenue at</param>
        /// <returns>revenue array at revenue buckets</returns>
        public static decimal[] CalculateRevenueInterim(IDictionary<int, decimal> revenueArr, int[] revenueBuckets)
        {
            decimal[] revenue = new decimal[revenueBuckets.Length];
            decimal sum = 0.0M;
        
            // aggregate revenue assuming that the first revenue arrival time is greater than zero 

            for (int j=0; j <  (revenueBuckets.Length-1); j++) {

                foreach (KeyValuePair<int, decimal> temp in revenueArr)
                {
                    if ((temp.Key < revenueBuckets[j+1]) &&
                        (temp.Key >= revenueBuckets[j]))
                    {
                        sum += temp.Value;
                    }

                    //make sure get value exactly on last bucket date
                    if (((j+1) == (revenueBuckets.Length-1)) &&
                        (temp.Key == revenueBuckets[j])) sum += temp.Value;
                }
               
                revenue[j] = sum;
                sum = 0.0M;
            }

            return revenue; 
            
        }

        /// <summary>
        /// Function to calculate revenue in the interim 
        ///
        /// </summary>
        /// <param name="revenueArr">revenue arrival amts in (date, amt) format</param>
        /// <param name="revenueBuckets">set of offsets to return values for</param>
        /// <param name="baseDate">base date to calculate offsets from</param>
        /// <returns>dictionary of revenue amounts indexed by day offset from base date</returns>
        public static IDictionary<int, decimal> CalculateRevenueInterim(IDictionary<DateTime, decimal> revenueArr, int[] revenueBuckets, DateTime baseDate)
        {
            IDictionary<int, decimal> revenue = ConvertCshFlowArray(revenueArr, baseDate, true);
            decimal[] bucketedRevenueAmts = CalculateRevenueInterim(revenue, revenueBuckets);
            IDictionary<int, decimal> bucketedRevenue = new SortedDictionary<int, decimal>();
            for (int i = 0; i < revenueBuckets.Length; i++)
            {
                bucketedRevenue.Add(revenueBuckets[i], bucketedRevenueAmts[i]);
            }
            return bucketedRevenue;
        }


        /// <summary>
        /// Function to calculate time bucket offsets for given 
        /// transaction value date and maturity date
        /// 
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="maturityDate"></param>
        /// <param name="freq"></param>
        /// <returns>vector of time bucket day offset values</returns>
        public static int[] GetTimeBuckets(DateTime valueDate, DateTime maturityDate, int freq)
        {
            //partition the time to maturity into monthly time periods based on freq (eg 3 months)
            int currentDaysOut = 0;
            List<int> result = new List<int>();
            DateTime offSetDate = new DateTime(valueDate.Year, valueDate.Month, valueDate.Day);

            while (offSetDate.CompareTo(maturityDate) <= 0)
            {   //add to result
                result.Add(currentDaysOut);
                //increment
                currentDaysOut = currentDaysOut + 30 * freq; //monthly period * freq
                offSetDate = offSetDate.AddDays(30 * freq);
            }
            return result.ToArray();
        }

        /// <summary>
        /// Function to convert revenue arrival in (date, amount) format
        /// to (day offset, amount) format where offset is calculated from
        /// a start date.
        /// 
        /// </summary>
        /// <param name="revenueArr">revenue arrival in (date, amount) format</param>
        /// <param name="startDate">date to use for calculating offsets</param>
        /// <param name="filter">flag to select (if true) to filter out revenue arrival amounts before the start date</param>
        /// <returns>revenue arrival in (day offset, amount) format</returns>
        public static IDictionary<int, decimal> ConvertCshFlowArray(IDictionary<DateTime, decimal> revenueArr, DateTime startDate, bool filter)
        {
            TimeSpan ts;
            IDictionary<int, decimal> result = new SortedDictionary<int,decimal>();

            foreach (KeyValuePair<DateTime, decimal> item in revenueArr)
            {
                ts = item.Key - startDate;
                if (filter) //filters out csh flow data before the start date
                {
                    if (item.Key.CompareTo(startDate) >= 0) result.Add(ts.Days, item.Value);
                }
                else result.Add(ts.Days, item.Value);
            }
            return result;
        }

        public static SortedDictionary<int, decimal> LoadDistributedRevenueArr(int[] revenueBuckets, decimal amount, decimal firstBucketPercentage)
        {
            SortedDictionary<int, decimal> result = new SortedDictionary<int, decimal>();

            decimal[] revenue = DistributeRevenueUsingPercentageSplit(revenueBuckets, amount, firstBucketPercentage);

            for (int j = 0; j < revenueBuckets.Length; j++)
            {
                if (revenueBuckets[j] != 0)
                {
                    result.Add(revenueBuckets[j], revenue[j]);
                }
            }
            return result;
        }

        public static decimal[] DistributeRevenueUsingPercentageSplit(int[] revenueBuckets, decimal amount, decimal firstBucketPercentage)
        {
            decimal[] revenue = new decimal[revenueBuckets.Length];
            decimal sAmt;
            int jStart = 0;

            if (revenueBuckets[0] == 0)
            {
                revenue[0] = 0.0M;
                revenue[1] = firstBucketPercentage * amount;
                jStart = 2;
                sAmt = (1.0M - firstBucketPercentage) / (revenueBuckets.Length - 2.0M) * amount;
            }
            else
            {
                revenue[0] = firstBucketPercentage * amount;
                jStart = 1;
                sAmt = (1.0M - firstBucketPercentage) / (revenueBuckets.Length - 1.0M) * amount;
            }

            for (int j = jStart; j < revenueBuckets.Length; j++)
            {
                revenue[j] = sAmt;
            }

            return revenue; 
        }

        public static int GetUpperRevenueLimitBucket(int val, int[] limitBuckets)
        {
            int result = 0;
            foreach (int key in limitBuckets)
            {
                if ((val <= key) && (val > result)) result = key; 
            }
            return result; 
        }

        

        /// <summary>
        /// Used for creating default cashflows in pricing centre calc
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="months"></param>
        /// <returns></returns>
        public static decimal[] CreateDefaultPrincipleAmts(decimal limit, int months)
        {
            IList<decimal> result = new List<decimal>();
         
            for (int i = 0; i < months; i++)
            {
                result.Add(limit);
            }
            return result.ToArray();
        }



        /// <summary>
        /// function to take principal profile, costs etc and generate vectors of: 
        /// revenue TRev, costs TCost, "coupon" + revenue + delta principal 
        /// put here to avoid repeated recalculation over the analysis 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="adjppl"></param>
        /// <param name="TRev"></param>
        /// <param name="TRev1"></param>
        /// <param name="TCost"></param>
        /// <param name="LFlow"></param>
        /// <param name="LFlow1"></param>
        public static void KMVLendFlows(DerivROEInput input,
                                        out double[] adjppl,
                                        out double[] TRev,
                                        out double[] TRev1,
                                        out double[] TCost,
                                        out double[] LFlow,
                                        out double[] LFlow1)
        {
            adjppl = new double[input.Months.Totalmonths+1];
            TRev = new double[input.Months.Totalmonths+1];
            TRev1 = new double[input.Months.Totalmonths+1];
            TCost = new double[input.Months.Totalmonths+1];
            LFlow = new double[input.Months.Totalmonths+1];
            LFlow1 = new double[input.Months.Totalmonths+1];

            int i = 0;
            double LiqCost = 0;
            double Utn = 0;
            double Rev = 0;
            double PpdCost = 0;
            double xIntRate = 0;
            int filenum = 0;
            int j = 0;
            double Localmax = 0;
            double[] Ppl1 = new double[input.Months.Totalmonths]; //need to check dim
            double[] CPFlow1 = new double[input.Months.Totalmonths]; //need to check dim

            // monthly equivalent 
            xIntRate = Math.Pow((1.0 + input.IntRate), (1.0 / 12.0)) - 1.0;
            Localmax = input.Ppl[input.Months.Totalmonths];

            if (input.Utilisation == "N/A")
            {

                for (i = input.Months.Totalmonths; i >= 1; i += -1)
                {
                    //**** 
                    //**** Regulatory Equity now will follow the profile 
                    //**** However, there is no concept of an undrawn amount 
                    //**** as the profile is adusted to reflect local maxima. 
                    //**** See Formula document for discussion of the calculation 

                    if (input.Ppl[i] > Localmax)
                    {

                        adjppl[i] = input.Ppl[i];
                        Localmax = input.Ppl[i];
                    }
                    else
                    {
                        adjppl[i] = Localmax;
                    }
                }
            } //endif

            if (input.credit_protection.BoughtProtection == "Y")
            {

                for (i = input.Months.Totalmonths; i >= 1; i += -1)
                {

                    Ppl1[i] = input.Ppl[i] - input.CPFlow[i];
                    CPFlow1[i] = input.CPFlow[i];

                    if (input.CPFlow[i] >= input.Ppl[i])
                    {
                        Ppl1[i] = 0;
                        CPFlow1[i] = input.Ppl[i];
                    }
                }
            } //endif


            for (i = input.Months.DelayMonths + 1; i <= input.Months.Totalmonths; i++)
            {

                // to cope with case where user specifies profile 
                // utilisation is set to N/A, and Ppl(i) defines the ut'n 
                //**** 

                if (input.Utilisation == "N/A")
                {
                    if (adjppl[i] > 0)
                    {
                        Utn = input.Ppl[i] / adjppl[i];
                    }
                    else
                    {
                        Utn = 0.0;
                    }

                    LiqCost = input.Ppl[i] * input.LiquidityCost / 12.0;

                    if (input.Roe_data.Revolving == "Y")
                    {

                        Rev = (input.Limit * input.Feelimit / 12) +
                              (input.Ppl[i] * input.FeeUsage + (adjppl[i] -
                                                                input.Ppl[i]) * input.FeeUndrawn) / 12.0;
                    }
                    else
                    {
                        Rev = (input.Limit * input.Feelimit / 12) +
                              (input.Ppl[i] * input.FeeUsage / 12.0);
                    }
                }

                else
                {

                    Utn = Convert.ToDouble(input.Utilisation);
                    Rev = input.Limit * (input.Feelimit + Utn * input.FeeUsage + (1 - Utn) * input.FeeUndrawn) / 12.0;
                    LiqCost = input.Limit * Utn * input.LiquidityCost / 12.0;
                } //end-if-else

                // get the loan revenue for this period 
                //*** Error in specification 
                //*** Fee Limit should be against limit 
                //*** Fee drawn = % of Limit if no profile or just % of Ppl(i) as this represents the drawn amount 
                //*** Fee Undrawn = (1-%) of Limit if no profile or just % of [Limit-Ppl(i)] as this represents the undrawn amount 

                // add in the point flows 
                //**** Rev/Trev(i) split October 2002 
                //**** Covers the bug where specialised Finance may put in a negative Tax Benefit (ie tax Cost) 
                //**** which can make the cost negative (ie effective a revenue!) 
                //**** I have made the call that a Tax Benefit/Cost has no bearing on operational processing 
                //**** and hence is not included in the variable cost calculation. 
                //**** 

                Rev = Rev + input.PointFs[i, 1];
                TRev[i] = Rev + input.PointFs[i, 3];
                TRev1[i] = 0.0;

                // liquidity cost 
                //*** Error in specification 

                //**** Liquidity cost is calcuated based on the drawn down amount only 

                // proportional costs for loans 
                PpdCost = Rev * input.VariableCost + LiqCost;
                // total costs 
                TCost[i] = PpdCost + input.PointFs[i, 2];


                // total flows- principal plus revenue plus attributed coupon 
                // note time zero flows are handled outside the main loop 
                // 
                // note: 
                // Vector Ppl is dim'd 1 to Mat, and Ppl(i) holds for the time i-1 to i 
                // that is, the ith period. (first interval runs from time zero to one) 
                // following depends on Ppl being dimensioned to (Mat+1) 

                if (input.credit_protection.BoughtProtection == "Y")
                {
                    LFlow[i] = Ppl1[i] - Ppl1[i + 1] + TRev[i] + Ppl1[i] * xIntRate;
                    LFlow1[i] = CPFlow1[i] - CPFlow1[i + 1] + CPFlow1[i] * xIntRate;
                }
                else
                {
                    LFlow[i] = input.Ppl[i] - input.Ppl[i + 1] + TRev[i] + input.Ppl[i] * xIntRate;
                }
            } //end-for

            return;
        }

        /// <summary>
        /// function to take principal profile, costs etc and generate vectors of:
        /// revenue TRev, costs TCost, + revenue + delta principal
        /// put here to avoid repeated recalculation over the analysis.
        /// 
        /// Given a PpdRev amount positions the flows in time according
        /// to selected revenue type and treatment
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="PCE"></param>
        /// <param name="EExp"></param>
        /// <param name="RxmExp"></param>
        /// <param name="PpdRev"></param>
        /// <param name="TRev">Time based revenue profile</param>
        /// <param name="TCost">Time based cost profile</param>
        /// <param name="LFlow"></param>
        public static void KMVDerivFlows(DerivROEInput input,
                                         double[] PCE,
                                         out double[] EExp,
                                         out double RxmExp,
                                         double PpdRev,
                                         out double[] TRev,
                                         out double[] TCost,
                                         out double[] LFlow,
                                         double MTM)
        {
            EExp = new double[input.Months.Totalmonths + 1];
            TRev = new double[input.Months.Totalmonths + 1];
            TCost = new double[input.Months.Totalmonths + 1];
            LFlow = new double[input.Months.Totalmonths + 1];
            RxmExp = 0.0;

            double Rev, LastExp, MTMDisc;
            double Unused, ThisExp, TemRXM;
            double xIntRate, BondCoupon, temRxmExp;

            //for MTM, use interbank discount rate, consistent with bank approach
            //MTM = 0;
            LastExp = 0;
    
            //needed for bonds only, to make flows same as loans.
            //monthly equivalent
            xIntRate = Math.Pow((1 + input.IntRate) , (1.0 / 12.0)) - 1.0;
            MTMDisc = Math.Pow((1 + input.Drate) , (-1.0 / 12.0));

            for (int i = input.Months.Totalmonths; i >= input.Months.DelayMonths + 1; i--)
            {
                //get the revenue for this period
                Rev = 0;
                if (input.RevTreat == "Ann")
                {
                    if (input.RevenueType == "Currency")
                    {
                        Rev = PpdRev;
                    }
                    else
                    {
                        Rev = PpdRev * input.Ppl[i];
                    }
                }
                else
                {
                    if ( ((input.RevTreat == "Fwd") && (i == input.Months.Totalmonths)) ||
                         ((input.RevTreat == "Now") && (i == input.Months.DelayMonths + 1)))
                    {
                        Rev = PpdRev;
                    }
                }

                // exception code for Markets' physical securities, BONDS AND ZEROS
                // "Product" refers to calculation type
                if (input.Product == "BOND")
                {
                    BondCoupon = 0;
                    ThisExp = input.Ppl[i];
                    EExp[i] = input.Ppl[i];
                    //RXM exposure, allowing for possible eventual principal profile
                    if (ThisExp > RxmExp) RxmExp = ThisExp;
                    // to align with KMV loans
                    // "TxnProduct" refers to the specific product transacted
                    if (input.TxnProduct == "BOND")
                    {
                        // include a risk free coupon flow for KMV style calcs
                        // for coupon-bearing securities, to align with KMV loans
                        BondCoupon = input.Ppl[i] * xIntRate;
                    }
                    LFlow[i] = ThisExp - LastExp + BondCoupon + Rev;
                    LastExp = ThisExp;
                }
                else
                {
                    //derivatives
                    MTM = MTM * MTMDisc + input.PointFs[i, 1] + Rev;
                    // RXM, allowing for PCE + MTM
                    // strictly, the RxmCI should be within the PCE calculation- this may not
                    // extend to options
                    //*****
                    //*****  WARNING:    This will NOT reconcile with the RXM Values
                    //*****              Need to think how we would change this.
                    //*****              Problem will be exacerbated when RXM replacement is implemented.
                    //*****
                    temRxmExp = input.RxmCI * PCE[i] + MTM;
                    if (temRxmExp > RxmExp) RxmExp = temRxmExp;
                    PCECalcs.Exposures(MTM, PCE[i], out ThisExp, out Unused);
                    EExp[i] = ThisExp;
                    // derivative flows based on credit exposure, not revenue, but this
                    // is included via the movement in mtm over the product life
                    LFlow[i] = ThisExp - LastExp;
                    LastExp = ThisExp;
                }

                // 18 July
                // new variables to separate and report revenue and cost
                // treat tax benefit as a revenue item
                // calculated as after tax present value to shareholder
                // 2 November
                // derivatives costs turned into per period amounts
                // retain as need revenue effect on exposure and all revenue for returns
                // add in the point flows
                TRev[i] = Rev + input.PointFs[i, 1] + input.PointFs[i, 3];

                //****
                //****   Simply applying monthly activity based cost to future times points without consideration
                //****   of the time value of money - Clearly wrong but second order for moment.
                //****   This overcomes the problem of just having the cost as an NPV value.
                //****   This caused problems with the Relationship EVA/RAROC calculations.
                //****   CHANGED - 15/8/2002
                //****
                TCost[i] = input.PpdCost + input.PointFs[i, 2];

            } //end for-loop

            //EXIT SUB
        }

        /// <summary>
        /// function added for Markets' Trades to spread cost over time 
        /// function to calculate cost by:
        /// placing per period cost over time, inflating at discount 
        /// present valuing at the after tax CoC 
        /// applying the effective tax rate 
        /// assumes costs arise at start of each period 
        /// Term is in months 
        /// </summary>
        /// <param name="Disc"></param>
        /// <param name="CoC"></param>
        /// <param name="Tax"></param>
        /// <param name="Franking"></param>
        /// <param name="Term"></param>
        /// <param name="PpdCost"></param>
        /// <returns></returns>
        public static double NPVCosts(double Disc,
                                      double CoC,
                                      double Tax,
                                      double Franking,
                                      int Term,
                                      double PpdCost)
        {
            double x = 0;
            double Sn = 0;

            x = Math.Pow(((1 + Disc) / (1 + CoC)), (1 / 12));

            Sn = (Math.Pow(x, Term) - 1) / (x - 1);

            return PpdCost * Sn * (1 - Tax * (1 - Franking));
        }


        /// <summary>
        /// Get Revenue amount based on type and treatment data.
        /// This was based on a pricing centre calc.
        /// 
        /// (For Markets transactions)
        /// (1) If revenue is expressed in currency amount:
        /// a)	The model assumes this to be an NPV amount valued at commencement date
        /// b)	This is converted to a cash flow based on Product Specific Rules
        ///      i)	    Now = Remains as a Time zero value
        ///      ii)	Fwd = Is future valued to the maturity of the transaction
        ///      iii)	Annuity = Converted to an “In arrears” annuity cash flow
        ///      iv)	See product specific treatment rules
        ///  
        /// (2) If revenue is expressed  in “Basis Points”. These are converted
        /// to currency amounts, and the flows positioned in time using the same Product
        /// Specific Rules as for currency amounts above
        /// 
        /// </summary>
        /// <param name="Revenue"></param>
        /// <param name="RevTreat">Can be "Ann" or "Now" or "Fwd"</param>
        /// <param name="RevenueType">Currency or Basis Points</param>
        /// <param name="Face">Face value of transaction</param>
        /// <param name="Life">Life of transaction in years</param>
        /// <param name="Drate"></param>
        /// <returns></returns>
        public static double RevAmount(double Revenue,
                                       string RevTreat,
                                       string RevenueType,
                                       double Face,
                                       double Life,
                                       double Drate)
        {
            double functionReturnValue = 0;
            double TemDrate = 0;
            double TemVar = 0;
            // note Life is in years 
            // used for markets 
            switch (RevTreat)
            {

                case "Ann": // annuity 
                    // determine the revenue type and treatment 
                    if (RevenueType == "Currency")
                    {
                        // set the monthly revenue if outright currency amount 
                        TemDrate = Math.Pow((1.0 + Drate), (1.0 / 12.0)) - 1.0;
                        functionReturnValue = -XPMT(TemDrate, Life * 12, Revenue, 0, 0);
                    }
                    else
                    {
                        // otherwise points per month 
                        functionReturnValue = Revenue / 120000.0;
                    }
                    break;

                case "Now": // revenue taken upfront at time = 0 
                    if (RevenueType == "Currency")
                    {
                        functionReturnValue = Revenue;
                    }
                    else
                    {
                        TemVar = Revenue * Face / 120000.0;
                        TemDrate = Math.Pow((1.0 + Drate), (1.0 / 12.0)) - 1.0;
                        functionReturnValue = -XPV(TemDrate, Life * 12.0, TemVar, 0, 0);
                    }
                    break;

                case "Fwd":  // revenue taken at end of life of txn 
                    if (RevenueType == "Currency")
                    {
                        functionReturnValue = Revenue * Math.Pow((1 + Drate), Life);
                    }
                    else
                    {
                        TemVar = Revenue * Face / 120000.0;
                        TemDrate = Math.Pow((1.0 + Drate), (1.0 / 12.0)) - 1.0;
                        functionReturnValue = -XFV(TemDrate, Life * 12.0, TemVar, 0, 0);
                    }
                    break;
                default:
                    break;
            }
            return functionReturnValue;
        }


        /// <summary>
        /// to replace Excel's PMT function 
        /// returns the annuity payment, given a pv 
        /// primarily for Groupware use. Also for pure VB version 
        /// note fv not catered for 
        /// also assumes ALL cashflows are at the end of the period 
        /// </summary>
        /// <param name="rate_Renamed"></param>
        /// <param name="nper_Renamed"></param>
        /// <param name="pv_Renamed"></param>
        /// <param name="fv_Renamed"></param>
        /// <param name="xtype"></param>
        /// <returns></returns>
        public static double XPMT(double rate_Renamed,
                                  double nper_Renamed,
                                  double pv_Renamed,
                                  double fv_Renamed,
                                  int xtype)
        {
            double functionReturnValue = 0;

            // ERROR: Not supported in C#: OnErrorStatement 
            double r = 0;

            if (nper_Renamed < 0.0001)
            {
                // zero periods taken as single payment upfront 
                functionReturnValue = -pv_Renamed;
            }
            else
            {
                if (rate_Renamed < 0.0001)
                {
                    // zero interest rate 
                    functionReturnValue = -pv_Renamed / nper_Renamed;
                }
                else
                {
                    // usual annuity 
                    //***** 
                    //***** Assuming S(n) = a + ar + ar^2 + .... + ar^n-1 
                    //***** S(n) = a x (1-r^n) / (1-r) 
                    //***** But we have U = ar + ar^2 + .... + ar^n-1 ie sum of PMT x discount factor 
                    //***** Hence U = S(n) - a 
                    //***** and therefore a = (U/r) x (1-r) / (1-r^n-1) 
                    //***** 
                    r = 1.0 / (1.0 + rate_Renamed);
                    functionReturnValue = -(pv_Renamed / r) * (1 - r) / (1 - Math.Pow(r, nper_Renamed));
                }
            }
            return functionReturnValue;
        }

        /// <summary>
        /// to replace Excel's PV function 
        /// returns the pv of an annuity stream of pmt 
        /// primarily for Groupware use. Also for pure VB version 
        /// note fv not catered for 
        /// </summary>
        /// <param name="rate_Renamed"></param>
        /// <param name="nper_Renamed"></param>
        /// <param name="pmt_Renamed"></param>
        /// <param name="fv_Renamed"></param>
        /// <param name="xtype"></param>
        /// <returns></returns>
        public static double XPV(double rate_Renamed,
                                 double nper_Renamed,
                                 double pmt_Renamed,
                                 double fv_Renamed,
                                 int xtype)
        {
            double functionReturnValue = 0;

            // ERROR: Not supported in C#: OnErrorStatement 
            double i = 0;
            if (nper_Renamed < 0.002)
            {
                // no periods- 
                // but use pmt un-npv'd 
                functionReturnValue = -pmt_Renamed;
            }
            else
            {
                if (rate_Renamed < 0.0001)
                {
                    // zero rate- no effective discounting 
                    functionReturnValue = -nper_Renamed * pmt_Renamed;
                }
                else
                {
                    // usual pv of series 
                    i = 1 / Math.Pow((1 + rate_Renamed), nper_Renamed);
                    functionReturnValue = -pmt_Renamed * (1 - i) / rate_Renamed * Math.Pow((1 + rate_Renamed), xtype);
                }
            }
            return functionReturnValue;
        }

        /// <summary>
        ///  to replace Excel's FV function 
        ///  returns the pv of an annuity stream of pmt 
        ///  primarily for Groupware use. Also for pure VB version 
        ///  note pv not catered for 
        /// </summary>
        /// <param name="rate_Renamed"></param>
        /// <param name="nper_Renamed"></param>
        /// <param name="pmt_Renamed"></param>
        /// <param name="pv_Renamed"></param>
        /// <param name="xtype"></param>
        /// <returns></returns>
        public static double XFV(double rate_Renamed,
                                 double nper_Renamed,
                                 double pmt_Renamed,
                                 double pv_Renamed,
                                 int xtype)
        {
            double functionReturnValue = 0;

            // ERROR: Not supported in C#: OnErrorStatement 
            double i = 0;
            if (nper_Renamed < 0.002)
            {
                // no periods- 
                // but use pmt un-npv'd 
                functionReturnValue = -pmt_Renamed;
            }
            else
            {
                if (rate_Renamed < 0.0001)
                {
                    // zero rate- no effective forward valuing 
                    functionReturnValue = -nper_Renamed * pmt_Renamed;
                }
                else
                {
                    // usual pv of series 
                    i = Math.Pow((1 + rate_Renamed), nper_Renamed);
                    functionReturnValue = -pmt_Renamed * (i - 1) / rate_Renamed * Math.Pow((1 + rate_Renamed), xtype);
                }
            }
            return functionReturnValue;
        } 
    }
}