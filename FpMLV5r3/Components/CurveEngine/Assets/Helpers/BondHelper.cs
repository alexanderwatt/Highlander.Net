using System;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Analytics.V5r3.Dates;
using Highlander.Reporting.ModelFramework.V5r3;

namespace Highlander.CurveEngine.V5r3.Assets.Helpers
{
    ///<summary>
    ///</summary>
    public static class BondHelper
    {
        ///<summary>
        /// This is a coupon scheduler based on some old C++ code.
        ///</summary>
        ///<param name="valueDate">The value date of the bond.</param>
        ///<param name="settlementDate">The settlement date of the bond.</param>
        ///<param name="firstAccrualDate">The first accrual date of the bond.</param>
        ///<param name="firstCouponDate">The first coupon date of the bond. this may be null.</param>
        ///<param name="lastRegCouponDate">The last regular coupon date. This may be null.</param>
        ///<param name="maturityDate">The maturity date of the bond.</param>
        ///<param name="calendar">The payment calendar.</param>
        ///<param name="coupFreq">The coupon frequency.</param>
        ///<param name="rollDay">The roll day.</param>
        ///<param name="exdividendType">The ex div type.</param>
        ///<param name="prevCouponDate">The previous coupon date as calculated.</param>
        ///<param name="nextCouponDate">The next coupon date as calculated.</param>
        ///<param name="next2CouponDate">The next plus one coupon date as calculated.</param>
        ///<param name="regCouponsToMaturity">The regular coupons until maturity as calculated.</param>
        ///<param name="numCoupons">The number of coupon as calculated.</param>
        ///<param name="isXD">The exdiv state as calculated.</param>
        ///<returns>A boolean flag.</returns>
        ///<exception cref="Exception"></exception>
        public static Boolean GetBondCouponDates(DateTime valueDate, DateTime settlementDate, DateTime firstAccrualDate,
                                                 DateTime? firstCouponDate, DateTime? lastRegCouponDate, DateTime maturityDate, IBusinessCalendar calendar,
                                                 int coupFreq, int rollDay, ExDividendEnum exdividendType, out DateTime prevCouponDate,
                                                 out DateTime nextCouponDate, out DateTime? next2CouponDate, out int regCouponsToMaturity,
                                                 out int numCoupons, out bool isXD)
        {
            //var result = false;
            if (valueDate > maturityDate) throw new Exception("Bond has matured");
            var yf = maturityDate.Year;
            var mf = maturityDate.Month;
            var y = valueDate.Year;
            var m = valueDate.Month;
            var d = valueDate.Day;
            numCoupons = 0;
            regCouponsToMaturity = 0;
            isXD = false;
            prevCouponDate = firstAccrualDate;
            nextCouponDate = firstAccrualDate;
            next2CouponDate = firstAccrualDate;
            if (firstCouponDate != null)
            {
                //result = true;
                var coupon = (DateTime)firstCouponDate;
                var y1 = coupon.Year;
                var m1 = coupon.Month;
                if (valueDate < firstCouponDate) // in first coupon period
                {
                    nextCouponDate = (DateTime)firstCouponDate;
                    prevCouponDate = firstAccrualDate;
                    regCouponsToMaturity = (12 * (yf - y1) + mf - m1) / coupFreq;
                    next2CouponDate = new DateTime(yf, mf - (regCouponsToMaturity - 1) * coupFreq, rollDay);
                    if (next2CouponDate > maturityDate) next2CouponDate = maturityDate;
                    isXD = firstCouponDate < maturityDate && settlementDate >= new ExDivDate(exdividendType, nextCouponDate, calendar).Date;
                    numCoupons = (12 * (yf - y1) + mf - m1) / coupFreq - regCouponsToMaturity;
                    return true;//result
                }
            }
            else if (lastRegCouponDate != null && valueDate >= lastRegCouponDate) // in irregular final period
            {
                prevCouponDate = (DateTime)lastRegCouponDate;
                nextCouponDate = maturityDate;
                regCouponsToMaturity = -1;
                numCoupons = 1;
                next2CouponDate = null;
                return true;// result;
            }
            else // in regular part of schedule
            {
                regCouponsToMaturity = -BondAnalytics.CountCoupPdsEx(yf, mf, rollDay, y, m, d, coupFreq);
                nextCouponDate = new DateTime(yf, mf - coupFreq * regCouponsToMaturity, rollDay);
                next2CouponDate = new DateTime(yf, mf - coupFreq * (regCouponsToMaturity - 1), rollDay);
                prevCouponDate = new DateTime(yf, mf - coupFreq * (regCouponsToMaturity + 1), rollDay);
                isXD = regCouponsToMaturity > 0 && settlementDate >= new ExDivDate(exdividendType, nextCouponDate, calendar).Date;
                //result = true;
                return true; //result;
            }
            return false;
        }
    }
}