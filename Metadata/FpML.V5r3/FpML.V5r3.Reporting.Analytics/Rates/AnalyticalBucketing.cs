#region Usings

using System;
using System.Collections.Generic;
using Orion.Analytics.Dates;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Analytics.Rates
{
    public static class AnalyticalBucketing
    {
        //switch (BktType)
        //{
        //    case FUTURES_BUCKETS:
        //    default:
        //    pBuckets = new FuturesBuckets;
        //    break;
        //    case SWAP_BUCKETS:
        //    pBuckets = new SwapBuckets;
        //    break;
        //    case ABN_SWAP_BUCKETS:
        //    pBuckets = new ABNSwapBuckets;
        //    break;
        //    case VEGA_RISK_BUCKETS:
        //    pBuckets = new VegaRiskBuckets;
        //    break;
        //    case AAFP_RISK_BUCKETS:
        //    pBuckets = new AAFPBuckets;
        //    break;
        //}

    public static List<DateTime> GetBucketDates(IRateCurve pCurve, FuturesBuckets bucketType, int numberOfBucketDates)
        {
            List<DateTime> bucketDates = new List<DateTime>();
            // Find (or create) PVCurve
            if (pCurve == null)
                return null;
            FuturesBuckets pBuckets = Generate(pCurve, numberOfBucketDates);//Generate all the necessary futures dates
            int numBktDates = pBuckets->GetNumBucketDates();
            for (int i = 0; i < numBktDates; i++)
            {
                var bucketDate = DateHelper.SFEBillDates(i);
                bucketDates.Add(bucketDate);//long(BucketDate.GetJulian());
            }
            for (var i = numBktDates; i < numberOfBucketDates; i++)
            {
                bucketDates.Add(bucketDates[i - 1]);
            }
            return bucketDates;
        }

        /// <summary>
        /// Discount rate derivative.
        /// </summary>
        /// <param name="discountFactorBucketStartDate">The discount factor to the bucket start date</param>
        /// <param name="discountFactorFlowDate">The flow date should be after the bucket start date.</param>
        /// <param name="flowYearFraction">The year fraction between the bucket start date and the flow date</param>
        /// <param name="discountFactorBucketEndDate">The discount factor to the bucket end date</param>
        /// <param name="bucketYearFraction">The bucket year fraction.</param>
        /// <returns></returns>
        public static Decimal DeltaDiscFacWrtR(Decimal flowYearFraction, Decimal discountFactorFlowDate,
            Decimal discountFactorBucketStartDate, Decimal discountFactorBucketEndDate, Decimal bucketYearFraction)
        {
            if (flowYearFraction < 0)
                return 0;
            if (flowYearFraction > bucketYearFraction)	
            {
                var result =  - bucketYearFraction *
                              discountFactorBucketEndDate /      // D(j,A,r) 
                              discountFactorBucketStartDate *    // D(i,A,r) 
                              discountFactorFlowDate;           // D(t,A,r) 
                return result;
            }
            return 	- bucketYearFraction * discountFactorFlowDate * discountFactorFlowDate /
                   	discountFactorBucketEndDate *              // D(i,A,r)
                   	flowYearFraction / bucketYearFraction;      //(t-i)/(j-i)
        }

        /// <summary>
        /// A single index accruing over a coupon period.
        /// </summary>
        /// <param name="discountFactorIndexDates">This must include the index start date and end date. </param>
        /// <param name="indexCouponYearFractions">The year fractions between coupon dates. Te first number is between the index start date and the first coupon flow.</param>
        /// <param name="discountFactorBucketStartDate">The discount factor to the bucket start date.</param>
        /// <param name="discountFactorBucketEndDate">The discount factor to the bucket end date.</param>
        /// <param name="bucketYearFraction">The bucket year fraction.</param>
        /// <param name="indexRate">The underlying rate of the index.</param>
        /// <returns></returns>
        public static Decimal DeltaCouponIndexWrtR(List<Decimal> discountFactorIndexDates, 
            List<Decimal> indexCouponYearFractions, Decimal discountFactorBucketStartDate, 
            Decimal discountFactorBucketEndDate, Decimal bucketYearFraction, Decimal indexRate)
        {   
            Decimal sumDIdr = 0;  
            var arraySize = indexCouponYearFractions.Count; // indexDateList.Count - 1 ;  
            //	Define multipliers
            //	1.	D_StartMinusEnd = (D(1,B,r)-D(N+1,B,r))
            var dStartMinusEnd = discountFactorIndexDates[0] - discountFactorIndexDates[discountFactorIndexDates.Count-1];
            //	2.	I=I/(D(1,B,r)-D(N+1,B,r)) 
            var I = indexRate / dStartMinusEnd;
            //	3.	I=I*I/(D(1,B,r)-D(N+1,B,r))
            var squared = I * indexRate;   
            for (var i=0; i < arraySize; i++)
            {
                var flowYearFraction = 0.0m;
                flowYearFraction = indexCouponYearFractions[i] + flowYearFraction;
                if (i==0)			
                {
                    sumDIdr = sumDIdr + DeltaDiscFacWrtR(flowYearFraction, discountFactorIndexDates[i], discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction) * I;
                }
                else if (i == arraySize) 
                {
                    sumDIdr = sumDIdr - DeltaDiscFacWrtR(flowYearFraction, discountFactorIndexDates[i], discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction) *
                             (indexCouponYearFractions[i - 1] * squared+I);
                }
                else
                {					  
                    sumDIdr = sumDIdr - DeltaDiscFacWrtR(flowYearFraction, discountFactorIndexDates[i], discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction) *
                             (indexCouponYearFractions[i - 1]* squared);
                }
            }
            return sumDIdr;
        }

        /// <summary>
        /// The derivative with respect to the rate.
        /// </summary>
        /// <param name="discountFactorFlowDate">The discount factor to the flow date.</param>
        /// <param name="flowYearFraction">The year fraction to the flow date.</param>
        /// <param name="discountFactorIndexStartDate">The discount factor to the index start date. The index might be LIBOR or a Swap.</param>
        /// <param name="discountFactorIndexEndDate">The discount factor to the index end date. </param>
        /// <param name="indexYearFraction">The index year fraction. For example, this could be 0.35 or 3.0</param>
        /// <param name="bucketYearFraction">The bucket year fraction.</param>
        /// <param name="discountFactorBucketStartDate">The discount factor to the bucket start date.</param>
        /// <param name="discountFactorBucketEndDate">The discount factor to the bucket end date.</param>
        /// <returns></returns>
        public static Decimal DeltaIndexWrtR(Decimal discountFactorFlowDate, Decimal flowYearFraction, Decimal discountFactorIndexStartDate, Decimal discountFactorIndexEndDate, 
            Decimal indexYearFraction, Decimal bucketYearFraction, Decimal discountFactorBucketStartDate, Decimal discountFactorBucketEndDate)
        {
            // dD(x,B,r)/dr
            var derivativeX = DeltaDiscFacWrtR(flowYearFraction, discountFactorFlowDate, discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction);
            // dD(y,B,r)/dr  
            var derivativeY = DeltaDiscFacWrtR(flowYearFraction, discountFactorFlowDate, discountFactorBucketStartDate, discountFactorBucketEndDate, bucketYearFraction);
            return 1 / indexYearFraction * 1 / discountFactorIndexEndDate * derivativeX - 1 / indexYearFraction * 1 / 
                   discountFactorIndexEndDate * discountFactorIndexStartDate / discountFactorIndexEndDate * derivativeY;
        }

        public static Decimal AnalyticalBucket(DateTime valueDate, ARRAY* Parameters, ICurve curve,
                                            int isAnalytic, int greekType, int order, DateTime[] buckets,
                                            int bucketType, int bucketCurve, double bump)
        {

            g_BumpedPaymentCurve = -1;                  // Flag as not in use
            g_BumpedIndexCurve = -1;                    // Flag as not in use
            g_BumpedVolCurve = -1;                  // Flag as not in use
            /* added two new greek types in order to report sensitivities to implied FX volatilities and 
            implied interest rate/FX Rate Correlations
            4 - FX Vega
            5 - Correlation */
            if (greekType == 4 || greekType == 5)
            {
                /* if the index curve and the discount curve are the same there will be no
                FX Vol or FX correlation sensitivity */
                if ((int)Parameters->array[2] == (int)Parameters->array[11])
                    return;
                // get the input parameter that is going to be shifted -i.e FX correlations or FX volatilities
                double inputToShift = 0;
                //zero out first bucket
                Buckets->array[0] = 0;
                switch (greekType)
                {
                    case 4: //FX Vega
                        inputToShift = Parameters->array[34];
                        break;
                    case 5: //FX Correlation
                        inputToShift = Parameters->array[33];
                    default:
                        break;
                }
                //value the cashflow using these values
                ARRAY FXResults;
                FXResults.rows = 1;
                FXResults.columns = 2;
                ValueCashFlow(ValueDate, Parameters, CurvePath, &FXResults);
                double baseValue = FXResults.array[9];
                //Shift the input value by the amount Bump
                double shiftedInput = inputToShift + bump;
                switch (greekType)
                {
                    case 4: //FX Vega
                            // SAJ 3/12/96 Added swaption bumping as well - if a swaption object exists bump FX Volatility
                        if (g_pSwaption)
                            g_pSwaption->SetFXVolatility(shiftedInput);
                        // SAJ 3/12/96 End
                        Parameters->array[34] = shiftedInput;
                        break;
                    case 5: //FX Correlation
                            // SAJ 3/12/96 Added swaption bumping as well - if a swaption object exists bump FX Correlation
                        if (g_pSwaption)
                            g_pSwaption->SetFXCorrelation(shiftedInput);
                        // SAJ 3/12/96 End
                        Parameters->array[33] = shiftedInput;
                    default:
                        break;
                }
                //revalue the cashflow
                ValueCashFlow(ValueDate, Parameters, CurvePath, &FXResults);
                double greek = FXResults.array[9] - baseValue;
                //return this greek in the totals column  of the bucket array
                Buckets->array[0] = Greek;
                return;
            }
            if (BktType == SHIFT_CURVE)  //shifting the yield curve and the volatility curve for sensitivity analysis
            {
                ShiftCashFlow(ValueDate, Parameters, CurvePath, BktCurve, BktType, Buckets);
                return;
            }
            // Zero out Bucket array & save offset array
            double* BumpOffsets;
            BumpOffsets = new double[Buckets->columns + 1];

            BumpOffsets[0] = 0; // totals column
            for (var i = 1; i < Buckets->columns; i++)
            {
                BumpOffsets[i] = Buckets->array[i] + Bump;
                BumpOffsets[0] += BumpOffsets[i];   // Build total
                Buckets->array[i] = 0;
            }
            // Check if parameters is valid ARRAY or if there is anything to bucket
            if ((Parameters->rows == 0 || Parameters->columns == 0) ||
                    ((int)Parameters->array[2] != BktCurve && Order == 1) ||
                    ((int)Parameters->array[11] != BktCurve && Order == 0) ||
                    (Order == 3 && (GreekType != 0 && GreekType != 1)))
            {
                delete[] BumpOffsets;
                return;
            }
            if (BktType == VEGA_RISK_BUCKETS)   // Numerical vega risk bucketing only
            {
                if (GreekType == 3 && Order == 0)
                    VegaRiskBucketCashFlow(ValueDate, Parameters, CurvePath, BumpOffsets, Buckets);
                return;
            }
            if (!isAnalytic)    // Call numerical routines
            {
                NumericBucketCashFlow(ValueDate, Parameters, CurvePath, GreekType, Order,
                            BktType, BumpOffsets, Buckets);
                delete[] BumpOffsets;
                return;
            }
            g_BumpedPaymentCurve = -1;                  // Flag as not in use
            g_BumpedIndexCurve = -1;                    // Flag as not in use

            int m_Type = (int)Parameters->array[0];
            double m_Notional = Parameters->array[1];
            int m_PaymentCurve = (int)Parameters->array[2];
            long m_PaymentDate = (long)Parameters->array[3];
            long m_PaymentStart = (long)Parameters->array[4];
            long m_PaymentEnd = (long)Parameters->array[5];
            int m_PaymentDaycount = (int)Parameters->array[6];
            int m_PaymentTenor = (int)Parameters->array[7];
            double m_Margin = Parameters->array[8];
            double m_VolatilityMargin = Parameters->array[9];
            double m_PaymentRateSet = Parameters->array[10];
            int m_IndexCurve = (int)Parameters->array[11];
            long m_IndexSet = (long)Parameters->array[12];
            long m_IndexStart = (long)Parameters->array[13];
            long m_IndexEnd = (long)Parameters->array[14];
            int m_IndexDaycount = (int)Parameters->array[15];
            int m_IndexTenor = (int)Parameters->array[16];
            int m_IndexCoupon = (int)Parameters->array[17];
            double m_IndexRateSet = Parameters->array[18];
            int m_IndexPreset = (int)Parameters->array[19];
            double m_IndexStrike = Parameters->array[20];
            long m_IndexCities = (long)Parameters->array[21];
            int m_SecondaryCurve = (int)Parameters->array[22];
            long m_SecondarySet = (long)Parameters->array[23];
            long m_SecondaryStart = (long)Parameters->array[24];
            long m_SecondaryEnd = (long)Parameters->array[25];
            int m_SecondaryDaycount = (int)Parameters->array[26];
            int m_SecondaryTenor = (int)Parameters->array[27];
            int m_SecondaryCoupon = (int)Parameters->array[28];
            double m_SecondaryRateSet = Parameters->array[29];
            int m_SecondaryPreset = (int)Parameters->array[30];
            double m_SecondaryStrike = Parameters->array[31];
            int m_SecondaryCities = (int)Parameters->array[32];
            double m_CrossCorrelation = Parameters->array[33];
            double m_CrossVolatility = Parameters->array[34];
            double m_AvgFrequency = Parameters->array[35];
            int m_RollConvention = (int)Parameters->array[36];
            double m_BarrierRate = Parameters->array[37];

            ARRAY Results;
            Results.rows = 1;
            Results.columns = 18;
            ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
            double Payment_Dayfract = Results.array[1];
            double greek0, greek1;
            switch (greekType)
            {
                case 0:     // Delta
                default:
                    greek0 = Results.array[10];
                    greek1 = Results.array[7] * 1e-4;   // Future Value delta1 is computed in Delta_DiscFac_wrt_r()
                    break;
                case 1:     // Gamma
                    greek1 = Results.array[13];
                    greek0 = Results.array[12];
                    break;
                case 2:     // Theta
                    greek0 = Results.array[14];
                    greek1 = Results.array[15];
                    break;
                case 3:     // Vega
                    greek0 = Results.array[16];
                    greek1 = Results.array[17];
                    break;
            }
            if (order == 0 && greek0 == 0 || order == 1 && greek1 == 0 ||
                 (order == 3 && greek0 == 0 && greek1 == 0))
            {
                delete[] BumpOffsets;
                return;     // No greek to value
            }
            /*	Perform bucketing on calculated Greek0 or Greek1 */
            CString CurveName, SWIFTCode;
            SWIFTCode = GetSWIFT_String(m_IndexCurve);
            CurveName = CurvePath;
            CurveName += "YC_DATA\\PV." + SWIFTCode;
            PVCurve* pIndexCurve, *pPaymentCurve;
            pIndexCurve = GetCurve(CurveName);   // Find (or create) PVCurve

            SWIFTCode = GetSWIFT_String(m_PaymentCurve);
            CurveName = CurvePath;
            CurveName += "YC_DATA\\PV." + SWIFTCode;
            pPaymentCurve = GetCurve(CurveName);
            if (!pIndexCurve || !pPaymentCurve) // Invalid curves
            {
                delete[] BumpOffsets;
                return;
            }
            // Check if global bucket dates array is out of date
            if (g_BktType != BktType)
            {
                if (g_pBuckets)
                    delete g_pBuckets;
                switch (BktType)    // Create a new bucket date array object
                {
                    case FUTURES_BUCKETS:
                    default:
                        g_pBuckets = new FuturesBuckets;
                        break;
                    case SWAP_BUCKETS:
                        g_pBuckets = new SwapBuckets;
                        break;
                    case ABN_SWAP_BUCKETS:
                        g_pBuckets = new ABNSwapBuckets;
                        break;
                    case AAFP_RISK_BUCKETS:
                        g_pBuckets = new AAFPBuckets;
                        break;
                }
                g_pBuckets->Generate(*pIndexCurve, GBP, Buckets->columns - 1);    // Generate new list of bucket date
            }
            g_BktType = BktType;
            IsAnalytic = g_WasAnalytic;
            int NumBkts = g_pBuckets->GetNumBucketDates() - 1;

            if (m_Type == CASH)
            {
                m_IndexEnd = m_PaymentDate;
                m_IndexStart = ValueDate;
            }
            else if (m_Type == FXD)
            {
                m_IndexStart = m_PaymentStart;
                m_IndexEnd = m_PaymentEnd;
            }
            double TIndex = (m_IndexEnd - m_IndexStart) / 365.0;

            if (greekType != 0)
            {
                double greek = order == 0 ? greek0 : greek1;
                double valueSum = 0;
                double parallelSum = 0;
                for (int Bkt = 0; Bkt < g_pBuckets->GetNumBucketDates(); Bkt++)
                {

                    DateTime BktStart = g_pBuckets->GetDate(Bkt);
                    DateTime BktEnd = g_pBuckets->GetDate(Bkt + 1);

                    if (m_IndexEnd < (long)BktStart.GetJulian() || (long)BktEnd.GetJulian() < m_IndexStart)
                    {
                        Buckets->array[Bkt + 1] = 0;
                        continue;
                    }

                    double DF_Start = (double)pIndexCurve->GetLogLinearPoint((double)(m_IndexStart));
                    double DF_End = (double)pIndexCurve->GetLogLinearPoint((double)(m_IndexEnd));
                    double Rate = 0;
                    if (m_Type != CASH && m_Type != FXD)
                        Rate = (DF_Start / DF_End - 1) / TIndex;
                    if (order != 3)
                        Buckets->array[Bkt + 1] = BucketValue(m_IndexStart, m_IndexEnd, greek,
                                            BktStart.GetJulian(), BktEnd.GetJulian(), Rate, TIndex) * BumpOffsets[Bkt + 1];
                    else
                    {
                        Buckets->array[Bkt + 1] = BucketValue(m_IndexStart, m_IndexEnd, greek0,
                                            BktStart.GetJulian(), BktEnd.GetJulian(), Rate, TIndex) * BumpOffsets[Bkt + 1];
                        Buckets->array[Bkt + 1] += BucketValue(m_IndexStart, m_IndexEnd, greek1,
                                            BktStart.GetJulian(), BktEnd.GetJulian(), Rate, TIndex) * BumpOffsets[Bkt + 1];
                    }
                    valueSum += Buckets->array[Bkt + 1];
                    if (order == 3 && greekType == 1)
                    {
                        Buckets->array[Bkt + 1] = valueSum;
                        parallelSum += valueSum;
                    }
                }
                //Buckets->array[1] = (Greek - ValueSum);	// HACK
                if (order == 3 && greekType == 1)
                    Buckets->array[0] = parallelSum;
                else
                    Buckets->array[0] = valueSum;   // Total
                delete[] BumpOffsets;
                return;
            }
            /* 	Here, if the tenor of the instrument is greater than the coupon then assume that it is a 
                coupon bearing index! */
            double indexRate = 0;
            int zeroCoupon = FALSE;
            if (m_IndexTenor > m_IndexCoupon)
            {
                if (m_Type == CASH || m_Type == FXD)
                    indexRate = 1;
                else
                    indexRate = _GetIndex(ValueDate, m_IndexSet, m_IndexStart, m_IndexEnd, m_IndexTenor,
                                    m_IndexDaycount, m_IndexCoupon, m_IndexCoupon, *pIndexCurve, m_IndexRateSet);
                zeroCoupon = TRUE;
            }
            /************************************************************************************End of zero coupon selection*/
            double bucketTotal = 0;
            for (int bucket = 1; bucket < g_pBuckets->GetNumBucketDates(); bucket++)
            {
                DateTime bucketStartDate = g_pBuckets->GetDate(bucket - 1);
                long bucketStart = bucketStartDate.GetJulian();
                DateTime bucketEndDate = g_pBuckets->GetDate(bucket);
                long bucketEnd = bucketEndDate.GetJulian();
                switch (greekType)
                {
                    case 0: // Delta

                        if (order == 0)
                        {
                            if (greek0 == 0)
                                break;
                            if (m_Type != CASH && m_Type != FXD && Payment_Dayfract > 0 &&
                                                m_IndexEnd >= bucketStart && m_IndexStart <= bucketEnd)
                            {
                                if (!zeroCoupon)
                                    Buckets->array[bucket] += greek0 * DeltaIndexWrtR(m_PaymentDate, m_IndexStart, m_IndexEnd, m_IndexDaycount,
                                                                    m_IndexCoupon, bucketStart, bucketEnd, *pIndexCurve, *pPaymentCurve);
                                else
                                    Buckets->array[bucket] += greek0 * DeltaCouponIndexWrtR(m_PaymentDate, m_IndexStart, m_IndexEnd, m_IndexDaycount,
                                                                    m_IndexCoupon, bucketStart, bucketEnd, *pIndexCurve, *pPaymentCurve, indexRate);
                            }
                        }
                        else if (order == 1)
                            if (greek1 != 0 && m_PaymentDate > bucketStart)           /*	Delta1 */
                                Buckets->array[bucket] += greek1 * DeltaDiscFacWrtR(m_PaymentDate, bucketStart, bucketEnd, *pPaymentCurve);
                        break;
                }
                Buckets->array[bucket] *= BumpOffsets[bucket] / g_pBuckets->Deltas[bucket - 1];
                bucketTotal += Buckets->array[bucket];
            }
            Buckets->array[0] = bucketTotal;
            delete[] BumpOffsets;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////
        //
        //		NUMERICAL ROUTINES
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////
        public static void NumericBucketCashFlow(long ValueDate, ARRAY* Parameters, LPCSTR CurvePath,
                                            int GreekType, int Order,
                                            int BktType, double* BumpOffsets, ARRAY* Buckets)
        {
            // Get Dates for curve
            int CurveCcyCode;
            int PrimaryIndexCode = (int)Parameters->array[11];
            int PaymentCurveCode = (int)Parameters->array[2];

            switch (Order)
            {
                case 0:
                default:
                    CurveCcyCode = PrimaryIndexCode;        // Primary index curve
                    g_BumpedIndexCurve = CurveCcyCode;      // Flag as in use
                    break;
                case 1:
                    CurveCcyCode = PaymentCurveCode;        // Payment (discount) curve
                    g_BumpedPaymentCurve = CurveCcyCode;    // Flag as in use
                    break;
                case 3:
                    CurveCcyCode = PrimaryIndexCode;        // Primary index curve
                    g_BumpedIndexCurve = CurveCcyCode;      // Flag as in use
                    if (PaymentCurveCode == PrimaryIndexCode)
                    {
                        CurveCcyCode = PaymentCurveCode;        // Payment (discount) curve
                        g_BumpedPaymentCurve = CurveCcyCode;    // Flag as in use
                    }
                    break;
            }
            if (GreekType == 2 || GreekType == 3)       // Turn off for theta and vega
            {
                g_BumpedPaymentCurve = -1;
                g_BumpedIndexCurve = -1;
            }
            // SAJ 27/9/96 Changed the following code so that changing sources or file names trigger new curves to be built
            //	if (g_StdCcyCode != CurveCcyCode)	// Standard curve is out of date
            //	{
            //		CString  CurveName, SWIFTCode;
            //		SWIFTCode = GetSWIFT_String(CurveCcyCode);
            //		CurveName = CurvePath;
            //		CurveName += "YC_DATA\\PV." + SWIFTCode;
            //		g_pStdCurve = GetCurve(CurveName);
            //		if (!g_pStdCurve->IsValid())
            //		{
            //			g_BumpedPaymentCurve = -1;
            //			g_BumpedIndexCurve = -1;
            //			return;
            //		}
            //	}
            // SAJ 27/9/96 New Code here
            CString CurveName, SWIFTCode;
            SWIFTCode = GetSWIFT_String(CurveCcyCode);
            CurveName = CurvePath;
            CurveName += "YC_DATA\\PV." + SWIFTCode;
            CurveName.MakeUpper();
            g_pStdCurve = GetCurve(CurveName);
            if (!g_pStdCurve->IsValid())
            {
                g_BumpedPaymentCurve = -1;
                g_BumpedIndexCurve = -1;
                return;
            }
            // SAJ 27/9/96 End

            // Check if global bucket dates array is out of date
            if (g_BktType != BktType || g_WasAnalytic)
            {
                if (g_pBuckets)
                    delete g_pBuckets;
                switch (BktType)    // Create a new bucket date array object
                {
                    case FUTURES_BUCKETS:
                    default:
                        g_pBuckets = new FuturesBuckets;
                        break;
                    case SWAP_BUCKETS:
                        g_pBuckets = new SwapBuckets;
                        break;
                    case ABN_SWAP_BUCKETS:
                        g_pBuckets = new ABNSwapBuckets;
                        break;
                    case AAFP_RISK_BUCKETS:
                        g_pBuckets = new AAFPBuckets;
                        break;
                }
                g_pBuckets->Generate(*g_pStdCurve, GBP, Buckets->columns - 1);    // Generate new list of bucket date
            }
            // SAJ 27/9/96 Added variable to flag when a new base curve is created
            int NewBaseCurve = FALSE;
            if (g_BktType != BktType || g_WasAnalytic || g_StdCcyCode != CurveCcyCode || g_GreekType != GreekType
                            //SAJ 27/9/96 Added validity test to see if base curve source has changed
                            || !g_pBaseCurve->IsValid())
            {
                if (GreekType == 0 || GreekType == 1) // Only if delta or gamma
                {
                    if (g_pBaseCurve)
                        delete g_pBaseCurve;
                    g_pBaseCurve = g_pBuckets->GenerateBucketPV(0, g_pStdCurve, 0); // Make a coarse curve to use for computing greeks
                                                                                    //SAJ 27/9/96 Added validity test to see if base curve source has changed
                    NewBaseCurve = TRUE;
                }
            }
            g_GreekType = GreekType;
            if (GreekType == 0 || GreekType == 1)       // Deltas or Gammas - bump curves if needed
            {
                //		First get rid of any old bumped curves if they are not compatable to the current situation and then
                // 		create new Future Buckets and coarse curves

                if (g_StdCcyCode != CurveCcyCode || g_TotalBumps != BumpOffsets[0] || g_BktType != BktType || !g_BktPVCurvesExist
                            // SAJ 27/9/96 Create new curves if new base curve created
                            || NewBaseCurve)
                {       // Update old bucket curves
                    if (g_BktPVCurvesExist) // Old curves exist; delete them
                    {
                        for (int Bkt = 0; Bkt < g_pBuckets->GetNumBucketDates(); Bkt++)
                        {
                            delete g_pUpBktCurves[Bkt];
                            delete g_pDownBktCurves[Bkt];
                        }
                    }
                    for (int Bkt = 0; Bkt < g_pBuckets->GetNumBucketDates(); Bkt++)
                    {
                        g_pUpBktCurves[Bkt] = g_pBuckets->GenerateBucketPV(Bkt, g_pStdCurve, (int)BumpOffsets[Bkt + 1]);
                        g_pDownBktCurves[Bkt] = g_pBuckets->GenerateBucketPV(Bkt, g_pStdCurve, -(int)BumpOffsets[Bkt + 1]);
                    }
                    g_BktPVCurvesExist = TRUE;
                    g_StdVolCode = -1;
                }
            }
            g_StdCcyCode = CurveCcyCode;
            g_TotalBumps = BumpOffsets[0];
            g_BktType = BktType;
            g_WasAnalytic = FALSE;

            ARRAY Results;
            Results.rows = 1;
            Results.columns = 18;

            g_pBumpedCurve = g_pBaseCurve;

            ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);

            double BaseValue = Results.array[9];
            double Sum = 0;
            double ParallelSum = 0;
            long m_PaymentDate = (long)Parameters->array[3];        // Payment date
            long m_PaymentStart = (long)Parameters->array[4];
            long m_IndexStart = (long)Parameters->array[13];
            long m_IndexEnd = (long)Parameters->array[14];
            int m_Type = (int)Parameters->array[0];

            if (m_Type == CASH)
            {
                m_IndexStart = ValueDate;
                m_IndexEnd = m_PaymentDate;
            }
            else if (m_Type == FXD)
            {
                m_IndexStart = m_PaymentStart;
                m_IndexEnd = m_PaymentDate;
            }
            double TIndex = (m_IndexEnd - m_IndexStart) / 365.0;

            double Greek;
            switch (GreekType)
            {
                case 2: // Theta
                    if (Order == 1)
                    // SAJ 3/12/96 Added bumping for swaptions as well
                    {
                        Parameters->array[3]--;     // Payment date
                        if (g_pSwaption)
                            g_pSwaption->SetExpiryDate((long)Parameters->array[3]);
                    }
                    // SAJ 3/12/96 END
                    else if (Order == 0)
                        ValueDate++;
                    ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
                    Greek = Results.array[9] - BaseValue;
                    break;
                case 3: // Vega
                    if (Order == 1)
                    {
                        Greek = 0;      // No Vega1
                    }
                    else
                    {
                        Parameters->array[9] += BumpOffsets[1] / 100;       // Volatility margin
                        ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
                        Greek = Results.array[9] - BaseValue;
                    }
                    break;
                default:
                    break;
            }
            int NumBuckets = g_pBuckets->GetNumBucketDates();
            for (int Bkt = 0; Bkt < NumBuckets; Bkt++)
            {
                Date EndBkt, StartBkt = g_pBuckets->GetDate(Bkt);
                if (Bkt + 1 < NumBuckets)
                    EndBkt = g_pBuckets->GetDate(Bkt + 1);
                else
                    EndBkt.SetJulian(StartBkt.GetJulian() + 20 * 365);  // Add 20 years

                if (Bkt > 1 && m_PaymentDate <= (long)g_pBuckets->GetDate(Bkt - 1).GetJulian() &&
                                 m_IndexEnd <= (long)g_pBuckets->GetDate(Bkt - 1).GetJulian())
                {
                    Buckets->array[Bkt + 1] = 0;
                    continue;
                }

                double BumpValue, DownValue, DF_Start, DF_End;
                switch (GreekType)
                {
                    case 0: // Delta
                    default:
                        g_pBumpedCurve = g_pUpBktCurves[Bkt];

                        ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
                        BumpValue = Results.array[9];
                        Buckets->array[Bkt + 1] = BumpValue - BaseValue;

                        break;
                    case 1: // Gamma
                        g_pBumpedCurve = g_pUpBktCurves[Bkt];

                        ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
                        BumpValue = Results.array[9];

                        g_pBumpedCurve = g_pDownBktCurves[Bkt];

                        ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
                        DownValue = Results.array[9];
                        if (BumpOffsets[Bkt + 1] != 0)
                            Buckets->array[Bkt + 1] = (DownValue - 2 * BaseValue + BumpValue) / BumpOffsets[Bkt + 1];
                        else
                            Buckets->array[Bkt + 1] = 0;
                        break;
                    case 2:     // Theta
                    case 3:     // Vega
                        if (m_IndexEnd < (long)StartBkt.GetJulian() || (long)EndBkt.GetJulian() < m_IndexStart)
                        {
                            Buckets->array[Bkt + 1] = 0;
                            continue;
                        }

                        DF_Start = (double)g_pStdCurve->GetLogLinearPoint((double)(m_IndexStart));
                        DF_End = (double)g_pStdCurve->GetLogLinearPoint((double)(m_IndexEnd));

                        double Rate = 0;
                        if (m_Type != CASH && m_Type != FXD)
                            Rate = (DF_Start / DF_End - 1) / TIndex;
                        Buckets->array[Bkt + 1] = BucketValue(m_IndexStart, m_IndexEnd, Greek * BumpOffsets[Bkt + 1],
                                            StartBkt.GetJulian(), EndBkt.GetJulian(), Rate, TIndex);
                        break;
                }
                Buckets->array[Bkt + 1] /= g_pBuckets->Deltas[Bkt];
                Sum += Buckets->array[Bkt + 1];
                if (Order == 3 && GreekType == 1)
                {
                    Buckets->array[Bkt + 1] = Sum;
                    ParallelSum += Sum;
                }
            }
            if (GreekType == 2 || GreekType == 3)
                Buckets->array[0] = Greek;
            else if (Order == 3 && GreekType == 1)
                Buckets->array[0] = ParallelSum;
            else
                Buckets->array[0] = Sum;

            g_BumpedPaymentCurve = -1;                  // Flag as not in use
            g_BumpedIndexCurve = -1;                    // Flag as not in use
        }



        public static void VegaRiskBucketCashFlow(long ValueDate, ARRAY* Parameters, LPCSTR CurvePath,
                                            double* BumpOffsets, ARRAY* Buckets)
        {
            int CurveCcyCode = (int)Parameters->array[11];  // Primary index curve

            ARRAY Results;
            Results.rows = 1;
            Results.columns = 18;

            ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
            double BaseValue = Results.array[9];
            double Sum = 0;
            long m_PaymentDate = (long)Parameters->array[3];        // Payment date
            long m_IndexStart = (long)Parameters->array[13];
            long m_IndexEnd = (long)Parameters->array[14];

            unsigned long PaymentDate = (long)Parameters->array[3];
            int m_Type = (int)Parameters->array[0];
            long m_PaymentEnd = (long)Parameters->array[5];
            if (m_Type == CASH)
                m_IndexEnd = m_PaymentDate;
            else if (m_Type == FXD)
                m_IndexEnd = m_PaymentEnd;
            double TIndex = (m_IndexEnd - m_IndexStart) / 365.0;

            // Check if global bucket dates array is out of date
            if (g_BktType != VEGA_RISK_BUCKETS || g_WasAnalytic || !g_BktVolCurvesExist)
            {
                CString CurveName, SWIFTCode;
                SWIFTCode = GetSWIFT_String(CurveCcyCode);
                CurveName = CurvePath;
                CurveName += "YC_DATA\\PV." + SWIFTCode;
                g_pStdCurve = GetCurve(CurveName);
                if (!g_pStdCurve)
                    return;
                if (g_pBuckets)
                    delete g_pBuckets;
                g_pBuckets = new VegaRiskBuckets;
                g_pBuckets->Generate(*g_pStdCurve, GBP, Buckets->columns - 1);    // Generate new list of bucket date
            }

            if (g_StdVolCode != CurveCcyCode || g_TotalBumps != BumpOffsets[0] || g_BktType != VEGA_RISK_BUCKETS)
            {       // Update old bucket curves
                if (g_BktVolCurvesExist)    // Old vol curves exist; delete them
                {
                    for (int Bkt = 0; Bkt < g_pBuckets->GetNumBucketDates(); Bkt++)
                        delete g_pBktVolatilityCurves[Bkt];
                }
                g_BktVolCurvesExist = FALSE;
                if (!g_VolatilityCurves[CurveCcyCode].IsValid())
                // SAJ 20/11/96 Bug fix  -  g_StdVolCode should be set to -1 to indicate that there are no curves
                {
                    g_StdVolCode = -1;
                    return;
                }
                // SAJ 20/11/96 End
                for (int Bkt = 0; Bkt < g_pBuckets->GetNumBucketDates(); Bkt++)
                    g_pBktVolatilityCurves[Bkt] = g_VolatilityCurves[CurveCcyCode].GenerateBucketSurface(Bkt, g_pBuckets, (int)BumpOffsets[Bkt + 1]);
                g_BktVolCurvesExist = TRUE;
                g_StdCcyCode = -1;
            }
            g_BktType = VEGA_RISK_BUCKETS;
            g_TotalBumps = BumpOffsets[0];
            g_WasAnalytic = FALSE;
            g_BumpedVolCurve = CurveCcyCode;                // Flag as in use
            g_StdVolCode = CurveCcyCode;                    // Store current code

            for (int Bkt = 0; Bkt < g_pBuckets->GetNumBucketDates() - 1; Bkt++)
            {
                if (Bkt == 0 || Bkt == 17)  // Spot buckets - do always
                {
                    Date StartBkt = g_pBuckets->GetDate(Bkt - 1);   // (Bkt-1) to allow for interpolation effects at (Bkt) point

                    if (m_IndexEnd <= (long)StartBkt.GetJulian())
                    {
                        Buckets->array[Bkt + 1] = 0;
                        continue;
                    }
                }

                g_pBumpedVolCurve = g_pBktVolatilityCurves[Bkt];

                ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);

                Buckets->array[Bkt + 1] = Results.array[9] - BaseValue;

                if (BumpOffsets[Bkt + 1] < 0)
                    Buckets->array[Bkt + 1] *= -1;

                Sum += Buckets->array[Bkt + 1];
            }
            Buckets->array[0] = Sum;

            g_BumpedVolCurve = -1;                  // Flag as not in use
        }

        /*--------------------------------------------------------------------------------------------------------------------

         ShiftCashflow :
                            This function performs two dimensional stress testing on a cashflow
                            This enables parallel shifts and twists of the yield curve together with a parallel shift of 
                            the volatility surface.


                BktCurve:   The curve that is going to be bumped 
                BktType:	This is always going to be futures buckets 
                BucketBumps:An array of basis points that will be used to bump the buckets of the futures curve
                        This can be used to do twists of the yield curve.
                The changes in value of PValue,Delta,Gamma,Theta and Vega are put in the first 5 buckets of BucketBumps
        -----------------------------------------------------------------------------------------------------------------------*/

        public static void ShiftCashFlow(long ValueDate, ARRAY* Parameters, LPCSTR CurvePath,
                                                int BktCurve, int BktType, ARRAY* BucketBumps)
        {
            /* Check if parameters is valid ARRAY or if there is anything to bucket 
            i.e. neither the index or the payment curve are the same as the bucketing curve
            this bit of logic is slightly different from the logic in BucketCashflow */
            if ((Parameters->rows == 0 || Parameters->columns == 0) ||
                    ((int)Parameters->array[2] != BktCurve && (int)Parameters->array[11] != BktCurve))
            {
                //Zero out first five buckets
                for (int i = 0; i <= 4; i++)
                {
                    BucketBumps->array[i] = 0;
                }
                return;
            }
            // Get Dates for curve
            // The currency code will correspond to either the index curve or the discount curve code
            int CurveCcyCode;
            if ((int)Parameters->array[11] == BktCurve)
            {
                CurveCcyCode = (int)Parameters->array[11];    //Primary Index Curve
                g_BumpedIndexCurve = CurveCcyCode;            //Flag as in use
            }
            if ((int)Parameters->array[2] == BktCurve)
            {
                CurveCcyCode = (int)Parameters->array[2];  //Discount Curve
                g_BumpedPaymentCurve = CurveCcyCode;        // Flag as in use
            }
            if (g_StdCcyCode != CurveCcyCode)   // Standard curve is out of date
            {
                CString CurveName, SWIFTCode;
                SWIFTCode = GetSWIFT_String(CurveCcyCode);
                CurveName = CurvePath;
                CurveName += "YC_DATA\\PV." + SWIFTCode;
                g_pStdCurve = GetCurve(CurveName);
                if (!g_pStdCurve)
                {
                    //Zero out first five buckets
                    // 10/23/96 SAJ Changed == in for test to <=
                    for (int i = 0; i <= 4; i++)
                    {
                        BucketBumps->array[i] = 0;
                    }
                    return;
                }
            }
            // Check if global bucket dates array is out of date
            if (g_BktType != BktType)
            {
                if (g_pBuckets)
                    delete g_pBuckets;
                // Create a new bucket date array object
                g_pBuckets = new FuturesBuckets;
                g_pBuckets->Generate(*g_pStdCurve, GBP, BucketBumps->columns - 1);    // Generate new list of bucket date 
            }
            if (g_BktType != BktType || g_StdCcyCode != CurveCcyCode)
            {
                if (g_pBaseCurve)
                    delete g_pBaseCurve;
                g_pBaseCurve = g_pBuckets->GenerateBucketPV(0, g_pStdCurve, 0); // Make a coarse spot curve to use for computing greeks
            }
            // 	Create the new coarse curve by bumping each of the Future Buckets by the required number of basis points
            if (g_NewCurveExist)    // delete the previously bumped curve if it already exists
                delete g_pNewCurve;
            //Create new curve
            g_pNewCurve = g_pBuckets->GenerateBumpedPV(BucketBumps->array, g_pStdCurve);
            g_NewCurveExist = TRUE;
            g_StdCcyCode = CurveCcyCode;
            g_BktType = BktType;
            ARRAY Results;
            Results.rows = 1;
            Results.columns = 18;
            g_pBumpedCurve = g_pBaseCurve;
            // The unchanged cashflow must be valued with a zero vol margin
            // When it is revalued this field is used for the VolBump
            double m_VolatilityMargin = Parameters->array[9];
            Parameters->array[9] = 0;
            //  Value the cashflow with the base (unchanged) curve   	
            ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
            double baseValue = Results.array[9];
            double baseDelta = Results.array[10] + Results.array[11];
            double baseGamma = Results.array[12] + Results.array[13];
            double baseTheta = Results.array[14] + Results.array[15];
            double baseVega = Results.array[16] + Results.array[17];
            //Revalue with the new bumped curve and shift the volatility surface
            Parameters->array[9] = m_VolatilityMargin;
            g_pBumpedCurve = g_pNewCurve;
            ValueCashFlow(ValueDate, Parameters, CurvePath, &Results);
            double bumpValue = Results.array[9];
            double bumpDelta = Results.array[10] + Results.array[11];
            double bumpGamma = Results.array[12] + Results.array[13];
            double bumpTheta = Results.array[14] + Results.array[15];
            double bumpVega = Results.array[16] + Results.array[17];
            //  Put the change in values in the first 5 buckets
            BucketBumps->array[0] = bumpValue - baseValue;
            BucketBumps->array[1] = bumpDelta - baseDelta;
            BucketBumps->array[2] = bumpGamma - baseGamma;
            BucketBumps->array[3] = bumpTheta - baseTheta;
            BucketBumps->array[4] = bumpVega - baseVega;
            g_BumpedPaymentCurve = -1;                  // Flag as not in use
            g_BumpedIndexCurve = -1;                    // Flag as not in use
        }

        // SAJ 30/9/96 Added routine for Alex - no interaction inside the DLL
        //LC 15/10/96 Recoded the if statements with a switch as was giving an internal compiler error

        public static double ValueIndex(long ValueDate, int m_IndexCurve, long m_IndexSet,
                long m_IndexStart, long m_IndexEnd, int m_IndexDaycount, int m_IndexTenor,
                int m_IndexCoupon, double m_IndexRateSet, int m_IndexPreset, long m_IndexCities,
                int m_RollConvention, int m_Toggle, LPCSTR CurvePath)
        {


            PVCurve* pIndexCurve;


            CString CurveName, SWIFTCode;
            SWIFTCode = GetSWIFT_String(m_IndexCurve);
            CurveName = CurvePath;
            CurveName += "YC_DATA\\PV." + SWIFTCode;
            pIndexCurve = GetCurve(CurveName);   // Find (or create) PVCurve

            if (!pIndexCurve->IsValid())
                return -1;

            double m_pIndex = 0.0;


            switch (m_Toggle)
            {
                case 1:
                    {
                        if (m_IndexSet == ValueDate && m_IndexRateSet == 0)
                        {
                            m_pIndex = _GetIndex(ValueDate, ValueDate + 1, m_IndexStart, m_IndexEnd, m_IndexTenor, m_IndexDaycount,
                                                m_IndexCoupon, m_IndexCoupon, *pIndexCurve, m_IndexRateSet);
                        }
                        m_pIndex = _GetIndex(ValueDate, m_IndexSet, m_IndexStart, m_IndexEnd, m_IndexTenor, m_IndexDaycount,
                                                m_IndexCoupon, m_IndexCoupon, *pIndexCurve, m_IndexRateSet);
                        break;
                    }


                case 2:
                    {

                        long DaysToISet;
                        DaysToISet = m_IndexSet - ValueDate;

                        if (DaysToISet > 0)
                        {
                            CString SWIFTCode = GetSWIFT_String(m_IndexCurve);
                            CString CapsVolName = CurvePath;
                            CapsVolName += "CS_FILES\\" + SWIFTCode + "\\CAPFUT_V." + SWIFTCode;
                            CString SwaptVolName = CurvePath;
                            SwaptVolName += "ASCII\\" + SWIFTCode + ".svl";

                            long IndexDays = m_IndexEnd - m_IndexStart;
                            m_pIndex = GetVolatility(CapsVolName, SwaptVolName, DaysToISet, IndexDays, 0.0);

                            if (m_pIndex < 0)
                            {
                                m_pIndex = 0.0;
                            }
                        }
                        break;

                    }
                case 3:
                    {
                        long DaysToISet;

                        DaysToISet = m_IndexSet - ValueDate;

                        if (m_IndexSet <= ValueDate)
                        {
                            m_pIndex = 0;
                        }
                        else
                        {
                            m_pIndex = pIndexCurve->GetLogLinearPoint((double)(m_IndexSet));
                            m_pIndex = -log(m_pIndex) / DaysToISet * 365;
                        }

                    }
                default:
                    m_pIndex = 0.0;

            }
            return m_pIndex;
        }
    }

    public class FuturesBuckets
    {
    }
}
