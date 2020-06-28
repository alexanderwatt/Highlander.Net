#region Usings

using System;
using System.Collections.Generic;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.Analytics.Rates
{
    public static class OsirisAnalytics
    {
        /// <summary>
        /// Analytic approximation of the continuous arithmetic average based on HULL's section
        /// 16.1 using the first 2 moments of the distribution.
        /// </summary>
        /// <param name="T"></param>
        /// <param name="R"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public static Double GetAverageVolatility(Double T, Double R, Double sigma)
        {
            // Analytic approximation of the continuous arithmetic average based on HULL's section
            // 16.1 using the first 2 moments of the distribution.
            if (T <= 0)
                return sigma;
            var sigmaSq = sigma * sigma;
            var m1 = (Math.Exp(R * T) - 1) / (R * T); // First Moment
            var m2 = 2 * Math.Exp((2 * R + sigmaSq) * T) / ((R + sigmaSq) * (2 * R + sigmaSq) * T * T) +
                     2 * (1 / (2 * R + sigmaSq) - Math.Exp(R * T) / (R + sigmaSq)) / (R * T * T);
            sigmaSq = Math.Log(m2 / (m1 * m1)) / T; // = [ln(M2) - 2*ln(M1)]/T = ln(M2)/T - 2(R - qA)
            var sigmaAvg = Math.Sqrt(sigmaSq);
            return sigmaAvg;
        }

        /// <summary>
        /// This routine finds the average of the index and the volatility between PaymentStart and PaymentEnd.
        /// AveFrequency determines the daily averaging frequency
        /// e.g.AveFrequency =  1 finds average of daily values;
        /// AveFrequency =  7 finds average of weekly values etc.
        /// PaymentEnd is not included in the averaging calculation.
        /// It is assumed that PaymentStart and PaymentEnd have already been adjusted to be business days
        /// Each AverageDate is calculated by adding on the correct number of business days
        /// PaymentRateSet is the sum of the rates that have already been fixed in the period.
        /// This is added onto the sum of the forward rates and the average calculated by dividing
        /// the total sum by the number of averaging points in the period
        /// Holidays:	Index Start is appropriate working day for the Average Date
        ///             Index Set is two working days prior to this Index Start
        ///             Index End is the appropriate working day following addition of the tenor
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="discountCurve"></param>
        /// <param name="indexCurve"></param>
        /// <param name="paymentStart"></param>
        /// <param name="paymentEnd"></param>
        /// <param name="indexDaycount"></param>
        /// <param name="indexTenor"></param>
        /// <param name="indexCoupon"></param>
        /// <param name="paymentRateSet"></param>
        /// <param name="indexRateSet"></param>
        /// <param name="averagingFrequency"></param>
        /// <param name="averagePreset"></param>
        /// <param name="indexCity"></param>
        /// <param name="convention"></param>
        /// <param name="volatilityMargin"></param>
        /// <param name="indexVolatility"></param>
        /// <returns></returns>
        public static Double? GetAverageRate(DateTime valueDate, IRateCurve discountCurve,
            IRateCurve indexCurve, DateTime paymentStart, DateTime paymentEnd, int indexDaycount, int indexTenor, int indexCoupon,
            double paymentRateSet, double indexRateSet, int averagingFrequency,
            int averagePreset, long indexCity, int convention, double volatilityMargin,
            IVolatilitySurface indexVolatility)
        {
            long indexSet, indexStart, indexEnd;
            double sumRate = 0.0;
            double sumVol = 0.0;
            double firstVol = 0.0;
            var averageDate = paymentStart;
            int numAvePoints = 0;
            if (indexCurve is null)
                return null;
            //Do simple approximation i.e. divide the sum of the start and end values by two 
            if (averagingFrequency <= 0)
            {

                DateTime Ave_Index_Start, Ave_Index_End;
                //Calculate the fixing date for the start date of the period
                long firstSet = AddWorkDays(PaymentStart, Ave_Preset, Index_City, CHoliday::PRECEEDING);
                //Calculate the fixing date for the end date of the period
                long lastSet = AddWorkDays(PaymentEnd, Ave_Preset, Index_City, CHoliday::PRECEEDING);
                if (valueDate >= lastSet)
                {
                    IndexVolatility = 0.0;
                    return Payment_Rateset / 2.0; //Both the start and the end rates have been fixed
                }

                double firstRate;
                if (valueDate < firstSet)
                {
                    Index_Start = PaymentStart;
                    Date Ave_Index_Start = Index_Start;
                    Date Ave_Index_End = Ave_Index_Start.AddMonths(ConvertToMonths(Index_Tenor));
                    Index_End = GoodDay(Ave_Index_End.GetJulian(), Index_City, Convention);
                    long Index_Tenor_Days = (long) (Index_End - Index_Start);
                    long IndexExpiry = (long) (firstSet - Value_Date);

                    firstRate = _GetIndex(Value_Date, firstSet, Index_Start, Index_End, Index_Tenor,
                        Index_Daycount, Index_Coupon, Index_Coupon, *pIndexCurve, Index_Rateset);
                    // SAJ 25/9/96  Added Volatility margin to volatility to allow for bumping
                    firstVol = GetVolatility(CapsVolName, SwaptVolName, IndexExpiry, Index_Tenor_Days, 0.0) +
                               VolatilityMargin;
                    // SAJ 25/9/96	Added following lines for convexity effect
                    double Weight, AdjForward;
                    GetModifiedForward(Value_Date, PaymentEnd, firstSet, Index_Start, Index_End,
                        Index_Tenor, Index_Coupon, Index_Daycount,
                        *pIndexCurve, firstRate, firstVol, Weight, AdjForward);
                    firstRate = Weight * firstRate + (1 - Weight) * AdjForward;
                    // SAJ 25/9/96 End
                }
                else
                {
                    firstRate = Payment_Rateset;
                    firstVol = 0.0;
                }

                Index_Start = PaymentEnd;
                Ave_Index_Start = Index_Start;
                Ave_Index_End = Ave_Index_Start.AddMonths(ConvertToMonths(Index_Tenor));
                Index_End = GoodDay(Ave_Index_End.GetJulian(), Index_City, Convention);
                long Index_Tenor_Days = (long) (Index_End - Index_Start);
                long IndexExpiry = (long) (lastSet - Value_Date);
                double lastRate = _GetIndex(Value_Date, lastSet, Index_Start, Index_End, Index_Tenor,
                    Index_Daycount, Index_Coupon, Index_Coupon, *pIndexCurve, Index_Rateset);
                // SAJ 25/9/96  Added Volatility margin to volatility to allow for bumping
                double LastVol = GetVolatility(CapsVolName, SwaptVolName, IndexExpiry, Index_Tenor_Days, 0.0) +
                                 VolatilityMargin;
                // SAJ 25/9/96	Added following lines for convexity effect
                double Weight, AdjForward;
                GetModifiedForward(Value_Date, PaymentEnd, lastSet, Index_Start, Index_End,
                    Index_Tenor, Index_Coupon, Index_Daycount,
                    *pIndexCurve, lastRate, LastVol, Weight, AdjForward);
                lastRate = Weight * lastRate + (1 - Weight) * AdjForward;
                // SAJ 25/9/96 End
                IndexVolatility = (firstVol + LastVol) / 2.0 + VolatilityMargin;
                return (firstRate + lastRate) / 2.0;
            }

            //Check if already into the averaging loop or the period is historical
            if (valueDate > PaymentStart)
            {
                sumRate = Payment_Rateset;
                sumVol = 0.0;
            }

            //Begin the averaging loop
            while (averageDate < PaymentEnd)
            {
                //Calculate the index setting date
                Index_Set = AddWorkDays(AverageDate, Ave_Preset, Index_City, CHoliday::PRECEEDING);
                if (Value_Date < Index_Set)
                {
                    Date Ave_Index_Start = AverageDate;
                    Date Ave_Index_End = Ave_Index_Start.AddMonths(ConvertToMonths(Index_Tenor));
                    Index_End = GoodDay(Ave_Index_End.GetJulian(), Index_City, Convention);
                    long Index_Tenor_Days = (long) (Index_End - AverageDate);
                    long IndexExpiry = (long) (Index_Set - Value_Date);
                    double NewRate = _GetIndex(Value_Date, Index_Set, AverageDate, Index_End, Index_Tenor,
                        Index_Daycount, Index_Coupon, Index_Coupon, *pIndexCurve, Index_Rateset);

                    // SAJ 25/9/96  Added Volatility margin to volatility to allow for bumping
                    double newVol = GetVolatility(CapsVolName, SwaptVolName, IndexExpiry, Index_Tenor_Days, 0.0) +
                                    VolatilityMargin;

                    // SAJ 25/9/96	Added following lines for convexity effect
                    double weight, adjForward;
                    GetModifiedForward(Value_Date, PaymentEnd, Index_Set, AverageDate, Index_End,
                        Index_Tenor, Index_Coupon, Index_Daycount,
                        *pIndexCurve, NewRate, newVol, weight, adjForward);
                    NewRate = weight * NewRate + (1 - weight) * adjForward;
                    // SAJ 25/9/96 End
                    sumRate += NewRate;
                    sumVol += (newVol * newVol);
                }

                numAvePoints++;
                // Increment average date by the averaging frequency
                // Here should make the convention Following otherwise will not get out of the end of the month
                // SAJ 25/9/96 Changed the following line to add AveFrequency days and then check good days
                //	    AverageDate = AddWorkDays(AverageDate, AveFrequency, Index_City, CHoliday::FOLLOWING);
                averageDate += AveFrequency;
                averageDate =
                    GoodDay(AverageDate, Index_City, CHoliday::FOLLOWING); // Use following to prevent chattering
                // SAJ 25/9/96 End
            }

            IndexVolatility = Math.Sqrt(sumVol / numAvePoints) + volatilityMargin;
            return sumRate / numAvePoints;
        }

        // *********************************MATHEMATICAL ROUTINES *************************

        /// <summary>
        /// Margrabe exchange cap.
        /// </summary>
        /// <param name="price1"></param>
        /// <param name="price2"></param>
        /// <param name="strike"></param>
        /// <param name="time"></param>
        /// <param name="volatility1"></param>
        /// <param name="volatility2"></param>
        /// <param name="corr"></param>
        /// <returns></returns>
        public static Double ExchangeCapValue(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)
        {
            var volatility = 0.0;
            double temp = volatility1 * volatility1 + volatility2 * volatility2 - 2 * corr * volatility1 * volatility2;
            if (Math.Abs(temp) > 0)
            {
                volatility = Math.Sqrt(temp);
            }

            var cap = NormalCapValue(price1, price2 + strike, time, volatility);
            return cap;
        }

        /// <summary>
        /// The exchange cap theta.
        /// </summary>
        /// <param name="price1"></param>
        /// <param name="price2"></param>
        /// <param name="strike"></param>
        /// <param name="time"></param>
        /// <param name="volatility1"></param>
        /// <param name="volatility2"></param>
        /// <param name="corr"></param>
        /// <returns></returns>
        public static Double ExchangeCapTheta(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)
        {
            var volatility = 0.0;
            double temp = volatility1 * volatility1 + volatility2 * volatility2 - 2 * corr * volatility1 * volatility2;
            if (Math.Abs(temp) > 0)
            {
                volatility = Math.Sqrt(temp);
            }

            var cap = NormalCapTheta(price1, price2 + strike, time, volatility);
            return cap;
        }

        /// <summary>
        /// The exchange cap delta.
        /// </summary>
        /// <param name="price1"></param>
        /// <param name="price2"></param>
        /// <param name="strike"></param>
        /// <param name="time"></param>
        /// <param name="volatility1"></param>
        /// <param name="volatility2"></param>
        /// <param name="corr"></param>
        /// <returns></returns>
        public static Double ExchangeCapDelta(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)
        {
            var volatility = 0.0;
            double temp = volatility1 * volatility1 + volatility2 * volatility2 - 2 * corr * volatility1 * volatility2;
            if (Math.Abs(temp) > 0)
            {
                volatility = Math.Sqrt(temp);
            }

            var cap = NormalCapDelta(price1, price2 + strike, time, volatility);
            return cap;
        }

        DllExport double DECORATE Exchange_Cap_Gamma(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Normal_Cap_Gamma(price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Cap_Vega(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Normal_Cap_Vega(price1, price2 + strike, time, volatility);

            return Cap;

        }

        // Spread Floor Value

        DllExport double DECORATE Exchange_Floor_Value(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Normal_Floor_Value(price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Floor_Theta(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Normal_Floor_Theta(price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Floor_Delta(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Normal_Floor_Delta(price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Floor_Gamma(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Normal_Cap_Gamma(price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Floor_Vega(double price1, double price2, double strike, double time,
            double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Normal_Floor_Vega(price1, price2 + strike, time, volatility);

            return Cap;

        }

        // Spread Digital Cap Value

        DllExport double DECORATE Exchange_Digital_Cap_Value(double payout, double price1, double price2, double strike,
            double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Cap_Value(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Cap_Theta(double payout, double price1, double price2, double strike,
            double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Cap_Theta(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Cap_Delta(double payout, double price1, double price2, double strike,
            double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Cap_Delta(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Cap_Gamma(double payout, double price1, double price2, double strike,
            double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Cap_Gamma(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Cap_Vega(double payout, double price1, double price2, double strike,
            double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Cap_Vega(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        // Exchange Digital Floor Value

        DllExport double DECORATE Exchange_Digital_Floor_Value(double payout, double price1, double price2,
            double strike, double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Floor_Value(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Floor_Theta(double payout, double price1, double price2,
            double strike, double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Floor_Theta(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Floor_Delta(double payout, double price1, double price2,
            double strike, double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Floor_Delta(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Floor_Gamma(double payout, double price1, double price2,
            double strike, double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Floor_Gamma(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        DllExport double DECORATE Exchange_Digital_Floor_Vega(double payout, double price1, double price2,
            double strike, double time, double volatility1, double volatility2, double corr)

        {
            double Cap = 0.0, volatility = 0.0;

            volatility = sqrt(volatility1 * volatility1 + volatility2 * volatility2 -
                              2 * corr * volatility1 * volatility2);

            Cap = Digital_Floor_Vega(payout, price1, price2 + strike, time, volatility);

            return Cap;

        }

        // *********************************INDEX ROUTINES *************************


        // XGetIndex

        DllExport double DECORATE XGetIndex(int cType, long ValueDate, long aSetdate, long aStartDate,
            long aMaturityDate,
            int aRollType, int DayCount, int aFrequency, int rFrequency, int aAccessCityCode,
            int aGenMethod, double aInitialprice, double aFinalprice, int Indextype, 

        const PVCurve  &DisCurve, const PVCurve  &IndexCurve, double aRateSet)
        {
            if (aSetdate <= ValueDate)
                return aRateSet;

            switch (cType)
            {
                case 1: // Libor-FRA
                    return GetFRAFloat(ValueDate, aStartDate, aMaturityDate, aFrequency, aRateSet, aRollType,
                        aAccessCityCode, aGenMethod, DayCount,
                        aInitialprice, aFinalprice, Indextype, DisCurve, IndexCurve);
                    break;
                case 2: // Libor-GenSwap
                    return
                        0.0; // GetGenSwap( ValueDate, aStartDate, aMaturityDate, aFrequency, aRollType, aAccessCityCode, aGenMethod, DayCount,
                    //		aInitialprice, aFinalprice, Indextype, Curve, DisCurve);
                    break;
                case 3: // Libor-Zero
                    return
                        0.0; // GetZero(ValueDate, aSetdate, aStartDate, aMaturityDate, aFrequency, DayCount, aFrequency, rFrequency, Curve, aIndexset);
                    break;
                case 4: // Libor-Fixed
                    return GetFixed(ValueDate, aStartDate, aMaturityDate, aFrequency, aRateSet, aRollType,
                        aAccessCityCode, aGenMethod, DayCount,
                        aInitialprice, aFinalprice, Indextype, DisCurve);
                    break;
                case 5: // Libor-Floating
                    return GetFloat(ValueDate, aStartDate, aMaturityDate, aFrequency, aRateSet, aRollType,
                        aAccessCityCode, aGenMethod, DayCount,
                        aInitialprice, aFinalprice, Indextype, DisCurve, IndexCurve);
                    break;
//		case 6:	// Bond
//			return XGetBond(startdate, enddate, Daycounttype, Curve, frequency);
//			break;
//		case 7:	// Libor-Futures
//			return XGetFuturesLibor(startdate, enddate, Daycounttype, Curve, frequency);
//			break;
//		case 8:	// Bond-Futures
//			return XGetFuturesBond(startdate, enddate, Daycounttype, Curve, frequency);
//			break;
//		case 9:	// Libor-LinearAmm
//			return XGetLinearAmm(startdate, enddate, Daycounttype, Curve, frequency);
//			break;
//		case 10:// Libor-GeometricAmm
//			return XGetGeometricAmm(startdate, enddate, Daycounttype, Curve, frequency);
//			break;
//		case 11:// Libor-Averaging
//			return XGetAverage(startdate, enddate, Daycounttype, Curve, frequency);
//			break;
                default:
                    return 0.0;
            }

        }


// AKW 03/10/96	Changed following line to add new swaption types
// AKW 03/10/96 End					


// Indextype is as follows:
//	0 = Value
//	1 = Index
//	2 = Delta0
//	3 = Delta1
//	4 = Delta01
//	5 = Gamma0
//	6 = Gamma1
//	7 = GammaX
//	8 = Gamma01X
//	9 = Theta0
//	10 = Theta1

        double GetFixed(long ValueDate, long aStartDate, long aMaturityDate, int aFrequency, double mRateSet,
            int aRollType, int aAccessCityCode, int aGenMethod, int DayCount,
            double aInitialprice, double aFinalprice, int Indextype,  const PVCurve  &DisCurve)
        {
            int ArraySize;
            double* PVArray;
            double* DayArray;
            double PVInitial;
            double PVFinal;
            double IndexNum;

            // First get an array of dates
            // First get an array of dates
            DateArray OurDates;

            XGenerateDates(OurDates, aGenMethod, aStartDate, aMaturityDate, aFrequency, aRollType, aAccessCityCode);

            ArraySize = OurDates.GetSize() - 1;

            PVArray = new double[ArraySize];
            DayArray = new double[ArraySize];

            for (short Index = 0; Index < ArraySize; Index++)
            {
                long lStartDate;
                long lEndDate;

                PVArray[Index] =
                    (double) DisCurve.GetLogLinearPoint((double) (((Date*) OurDates[Index + 1])->GetJulian()));

                lStartDate = ((Date*) OurDates[Index])->GetJulian();
                lEndDate = ((Date*) OurDates[Index + 1])->GetJulian();

                DayArray[Index] = Daycount(lStartDate, lEndDate, DayCount, aFrequency);
            }

            PVInitial = (double) DisCurve.GetLogLinearPoint((double) (aStartDate));
            PVFinal = PVArray[ArraySize - 1];

            switch (Indextype)
            {
                case 0: // Value
                    IndexNum = PVFinal * aFinalprice - PVInitial * aInitialprice +
                               (mRateSet * SwapDelta1(0.0, 0.0, PVArray, DayArray, ArraySize));
                    break;
                case 1: // Index
                    IndexNum = SwapRate1(PVInitial * aInitialprice, PVFinal * aFinalprice, PVArray, DayArray,
                        ArraySize);
                    break;
                case 2: // Delta0
                    IndexNum = 0.0;
                    break;
                case 3: //Delta1
                    IndexNum = (SwapDelta1(0.0, 0.0, PVArray, DayArray, ArraySize)) * 1e-4;
                    break;
                case 4: //Delta01
                    IndexNum = (SwapDelta1(PVInitial * aInitialprice, PVFinal * aFinalprice, PVArray, DayArray,
                                   ArraySize)) * 1e-4;
                    break;
                case 5: //Gamma0
                    IndexNum = 0.0;
                    break;
                case 6: //Gamma1
                    IndexNum = SwapGamma1(PVArray, DayArray, ArraySize) * 1e-8;
                    break;
                case 7: //GammaX
                    IndexNum = 0.0;
                    break;
                case 8: //Gamma01X
                    IndexNum = 0.0;
                    break;
                default:
                    IndexNum = 0.0;
            }

            OurDates.DeleteContents();
            delete[] PVArray;
            delete[] DayArray;

            return IndexNum;
        }

        double GetFloat(long ValueDate, long aStartDate, long aMaturityDate, int aFrequency, double mRateSet,
            int aRollType, int aAccessCityCode, int aGenMethod, int DayCount,
            double aInitialprice, double aFinalprice, int Indextype,  const PVCurve  &DisCurve, const PVCurve
  &IndexCurve)
        {
            int ArraySize;
            double* PVArray;
            double* IndexArray;
            double* DayArray;
            double PVInitial;
            double PVFinal;
            double IndexNum;

            // First get an array of dates
            // First get an array of dates
            DateArray OurDates;

            XGenerateDates(OurDates, aGenMethod, aStartDate, aMaturityDate, aFrequency, aRollType, aAccessCityCode);

            ArraySize = OurDates.GetSize() - 1;

            PVArray = new double[ArraySize];
            IndexArray = new double[ArraySize];
            DayArray = new double[ArraySize];

            for (short Index = 0; Index < ArraySize; Index++)
            {
                long lStartDate;
                long lEndDate;

                PVArray[Index] =
                    (double) DisCurve.GetLogLinearPoint((double) (((Date*) OurDates[Index + 1])->GetJulian()));

                lStartDate = ((Date*) OurDates[Index])->GetJulian();
                lEndDate = ((Date*) OurDates[Index + 1])->GetJulian();

                DayArray[Index] = Daycount(lStartDate, lEndDate, DayCount, aFrequency);
            }

            // Could use the first element in the PVArray.

            PVInitial = (double) DisCurve.GetLogLinearPoint((double) (aStartDate));
            PVFinal = PVArray[ArraySize - 1];

            if (IndexCurve.GetFileName() == DisCurve.GetFileName())

            {
                for (short Index = 0; Index < ArraySize; Index++)
                {
                    IndexArray[Index] = PVArray[Index];
                }
            }

            else

            {
                for (short Index = 0; Index < ArraySize; Index++)
                {
                    long lStartDate;
                    long lEndDate;

                    IndexArray[Index] =
                        (double) IndexCurve.GetLogLinearPoint((double) (((Date*) OurDates[Index + 1])->GetJulian()));

                    lStartDate = ((Date*) OurDates[Index])->GetJulian();
                    lEndDate = ((Date*) OurDates[Index + 1])->GetJulian();

                    DayArray[Index] = Daycount(lStartDate, lEndDate, DayCount, aFrequency);
                }
            }

            switch (Indextype)
            {
                case 0: // Value
                    IndexNum = PVFinal * (1.0 - aFinalprice) - PVInitial * (1.0 - aInitialprice) +
                               (mRateSet * SwapDelta1(0.0, 0.0, IndexArray, DayArray, ArraySize));
                    break;
                case 1: // Index
                    IndexNum =
                        SwapRate1(PVInitial * aInitialprice, PVFinal * aFinalprice, PVArray, DayArray, ArraySize) -
                        SwapRate1(PVInitial * aInitialprice, PVFinal * aFinalprice, IndexArray, DayArray, ArraySize);
                    break;
                case 2: // Delta0
                    IndexNum = -(SwapDelta1(0.0, 0.0, IndexArray, DayArray, ArraySize)) * 1e-4;
                    break;
                case 3: //Delta1
                    IndexNum = (SwapDelta1(0.0, 0.0, PVArray, DayArray, ArraySize)) * 1e-4;
                    break;
                case 4: //Delta01
                    IndexNum = (SwapDelta1(0.0, 0.0, PVArray, DayArray, ArraySize) -
                                SwapDelta1(0.0, 0.0, IndexArray, DayArray, ArraySize)) * 1e-4;
                    break;
                case 5: //Gamma0
                    IndexNum = 0.0;
                    break;
                case 6: //Gamma1
                    IndexNum = -SwapGamma1(PVArray, DayArray, ArraySize) * 1e-8;
                    break;
                case 7: //GammaX
                    IndexNum = SwapGamma1(IndexArray, DayArray, ArraySize) * 1e-8;
                    break;
                case 8: //Gamma01X
                    IndexNum = 0.0;
                    break;
                default:
                    IndexNum = 0;
            }

            OurDates.DeleteContents();
            delete[] PVArray;
            delete[] IndexArray;
            delete[] DayArray;

            return IndexNum;
        }

        double GetFRAFloat(long ValueDate, long aStartDate, long aMaturityDate, int aFrequency, double mRateSet,
            int aRollType, int aAccessCityCode, int aGenMethod, int DayCount,
            double aInitialprice, double aFinalprice, int Indextype,  const PVCurve  &DisCurve, const PVCurve
  &IndexCurve)
        {
            long ModStartDate;
            long ModEndDate;
            double DayArray;
            double IndexInitial;
            double IndexFinal;
            double PVInitial;
            double PVFinal;
            double SimpleIndexRate;
            double SimpleDiscountRate;
            double IndexNum;

            // First get an array of dates
            // First get an array of dates

            ModStartDate = GoodDay(aStartDate, aAccessCityCode, aRollType);
            ModEndDate = GoodDay(aMaturityDate, aAccessCityCode, aRollType);

            // Could use the first element in the PVArray.

            PVInitial = (double) DisCurve.GetLogLinearPoint((double) (ModStartDate));
            PVFinal = (double) DisCurve.GetLogLinearPoint((double) (ModEndDate));

            DayArray = Daycount(ModStartDate, ModEndDate, DayCount, aFrequency);

            if (IndexCurve.GetFileName() == DisCurve.GetFileName())

            {
                IndexInitial = PVInitial;
                IndexFinal = PVFinal;
            }

            else

            {
                IndexInitial = (double) IndexCurve.GetLogLinearPoint((double) (ModStartDate));
                IndexFinal = (double) IndexCurve.GetLogLinearPoint((double) (ModEndDate));

            }

            SimpleIndexRate = (IndexFinal / IndexInitial - 1) / DayArray;
            SimpleDiscountRate = (PVFinal / PVInitial - 1) / DayArray;

            switch (Indextype)
            {
                case 0: // Value
                    IndexNum = PVFinal * (1.0 - aFinalprice) - PVInitial * (1.0 - aInitialprice) +
                               (mRateSet * DayArray * IndexFinal);
                    break;
                case 1: // Index
                    IndexNum = SimpleIndexRate - SimpleDiscountRate;
                    break;
                case 2: // Delta0
                    IndexNum = -DayArray * PVFinal * 1e-4;
                    break;
                case 3: //Delta1
                    IndexNum = DayArray * IndexFinal * 1e-4;
                    break;
                case 4: //Delta01
                    IndexNum = (DayArray * IndexFinal - DayArray * PVFinal) * 1e-4;
                    break;
                case 5: //Gamma0
                    IndexNum = 0.0;
                    break;
                case 6: //Gamma1
                    IndexNum = 0.0;
                    break;
                case 7: //GammaX
                    IndexNum = 0.0;
                    break;
                case 8: //Gamma01X
                    IndexNum = 0.0;
                    break;
                default:
                    IndexNum = 0;
            }



            return IndexNum;
        }

// *********************************VALUE ROUTINES *************************  



// XReturnIndex - returns the index


        DllExport double DECORATE XReturnIndex(int cType, long ValueDate, int m_IndexCurve, int m_DiscountCurve,
            long m_IndexSet, long m_IndexStart, long m_IndexEnd, int m_IndexDaycount, int aGenMethod,
            int m_IndexCoupon, double m_IndexRateSet, long m_IndexCities, int m_RollConvention,
            double aInitialprice, double aFinalprice, int Indextype, int m_Toggle, LPCSTR CurvePath)


        {
            PVCurve* pIndexCurve;


            CString CurveName, SWIFTCode;
            SWIFTCode = GetSWIFT_String(m_IndexCurve);
            CurveName = CurvePath;
            CurveName += "YC_DATA\\PV." + SWIFTCode;
            pIndexCurve = GetCurve(CurveName); // Find (or create) PVCurve1

            if (!pIndexCurve->IsValid())
                return -1;

            PVCurve* pDisCurve;


            SWIFTCode = GetSWIFT_String(m_DiscountCurve);
            CurveName = CurvePath;
            CurveName += "YC_DATA\\PV." + SWIFTCode;
            pDisCurve = GetCurve(CurveName); // Find (or create) PVCurve2

            if (!pDisCurve->IsValid())
                return -1;

            double m_pIndex = 0.0;


            switch (m_Toggle)
            {
                case 1:
                {
                    if (m_IndexSet == ValueDate && m_IndexRateSet == 0)
                    {
                        m_pIndex = XGetIndex(cType, ValueDate, ValueDate + 1, m_IndexStart, m_IndexEnd,
                            m_RollConvention, m_IndexDaycount, m_IndexCoupon, m_IndexCoupon, m_IndexCities,
                            aGenMethod, aInitialprice, aFinalprice, Indextype,
                            *pIndexCurve, *pDisCurve, m_IndexRateSet);
                    }

                    m_pIndex = XGetIndex(cType, ValueDate, m_IndexSet, m_IndexStart, m_IndexEnd,
                        m_RollConvention, m_IndexDaycount, m_IndexCoupon, m_IndexCoupon, m_IndexCities,
                        aGenMethod, aInitialprice, aFinalprice, Indextype,
                        *pIndexCurve, *pDisCurve, m_IndexRateSet);
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
                        m_pIndex = pIndexCurve->GetLogLinearPoint((double) (m_IndexSet));
                        m_pIndex = -log(m_pIndex) / DaysToISet * 365;
                    }

                }

                default:
                    m_pIndex = 0.0;

            }


            return m_pIndex;
        }


        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="pVi">Present value vector</param>
        ///// <param name="di">The day fraction vector</param>
        ///// <param name="numberOfPoints"></param>
        ///// <returns></returns>
        //public static double SwapGamma1(double[] pVi, double[] di, int numberOfPoints)
        //{
        //    double swapGamma = 0.0;
        //    double sumPVDays = 0.0;
        //    double alpha = 0.0;
        //    for (int index = 0; index < numberOfPoints; index++)
        //    {
        //        alpha += di[index];
        //        swapGamma += Math.Abs(pVi[index] * di[index] * alpha);
        //    }
        //    return 2 * swapGamma;
        //}

