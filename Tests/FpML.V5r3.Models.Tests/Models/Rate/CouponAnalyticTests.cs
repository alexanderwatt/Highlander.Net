using System;
using Orion.Models.Rates.Coupons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.V5r3.Models.Tests.Models.Rate
{
    [TestClass]
    public class CouponAnalyticTests
    {
        private DateTime[] _bucketedDates;
        private Decimal[] _bucketedDiscountFactors;


        [TestInitialize]
        public void Initialisation()
        {
            DateTime[] temp =  {DateTime.Parse("22/07/2008"),
                                DateTime.Parse("21/10/2008"), DateTime.Parse("21/01/2009"), 
                                DateTime.Parse("21/04/2009"), DateTime.Parse("21/07/2009"), 
                                DateTime.Parse("21/10/2009"), DateTime.Parse("21/01/2010"), 
                                DateTime.Parse("21/04/2010"), DateTime.Parse("21/07/2010"), 
                                DateTime.Parse("21/10/2010"), DateTime.Parse("21/01/2011"), 
                                DateTime.Parse("21/04/2011"), DateTime.Parse("21/07/2011"), 
                                DateTime.Parse("21/10/2011"), DateTime.Parse("23/01/2012"), 
                                DateTime.Parse("23/04/2012"), DateTime.Parse("23/07/2012"), 
                                DateTime.Parse("22/10/2012"), DateTime.Parse("21/01/2013"), 
                                DateTime.Parse("22/04/2013"), DateTime.Parse("22/07/2013"), 
                                DateTime.Parse("21/10/2013"), DateTime.Parse("21/01/2014"), 
                                DateTime.Parse("22/04/2014"), DateTime.Parse("21/07/2014"), 
                                DateTime.Parse("21/10/2014"), DateTime.Parse("21/01/2015"), 
                                DateTime.Parse("21/04/2015"), DateTime.Parse("21/07/2015"), 
                                DateTime.Parse("21/10/2015"), DateTime.Parse("21/01/2016"), 
                                DateTime.Parse("21/04/2016"), DateTime.Parse("21/07/2016"), 
                                DateTime.Parse("21/10/2016"), DateTime.Parse("23/01/2017"), 
                                DateTime.Parse("21/04/2017"), DateTime.Parse("21/07/2017"), 
                                DateTime.Parse("23/10/2017"), DateTime.Parse("22/01/2018"), 
                                DateTime.Parse("23/04/2018"), DateTime.Parse("23/07/2018")
                               };

            _bucketedDates = temp;

            Decimal[] tempDiscountFactors = { 1.0m,
                                              0.981074m, 0.962186m, 0.9441m, 0.926295m, 
                                              0.90875m, 0.891658m, 0.875368m, 0.859296m, 
                                              0.843372m, 0.827826m, 0.812975m, 0.79831m, 
                                              0.783183m, 0.767968m, 0.753497m, 0.739275m, 
                                              0.725998m, 0.713017m, 0.700329m, 0.687923m, 
                                              0.675544m, 0.663285m, 0.651414m, 0.639911m, 
                                              0.628397m, 0.61712m, 0.606319m, 0.595618m, 
                                              0.584869m, 0.574328m, 0.564113m, 0.554096m, 
                                              0.544173m, 0.534236m, 0.525118m, 0.51587m, 
                                              0.50651m, 0.497628m, 0.488923m, 0.480387m
                                            };

            _bucketedDiscountFactors = tempDiscountFactors;
        }

        #region Bucketed Delta 01 (Fixed Leg)
        [TestMethod]
        public void TestBucketedDelta1()
        {
            FixedRateCouponAnalytic[] couponAnalytics;
            IRateCouponParameters[] analyticModelParameters;

            DateTime swapStartDate = new DateTime(2008, 7, 22);

            int len = _bucketedDates.Length;
            couponAnalytics = new FixedRateCouponAnalytic[len - 1];
            analyticModelParameters = new RateCouponParameters[len - 1];
            int index = 0;

            for (int i = 1; i < len; ++i)
            {
                IRateCouponParameters cp = new RateCouponParameters();
                cp.BucketedDates = CreateArray<DateTime>(_bucketedDates, 0, i + 1);
                cp.BucketedDiscountFactors = CreateArray<Decimal>(_bucketedDiscountFactors, 0, i + 1);

                analyticModelParameters[index] = cp;
              
                FixedRateCouponAnalytic am = new FixedRateCouponAnalytic();
                am.AnalyticParameters = cp;
                am.AnalyticParameters.YearFraction = (_bucketedDates[index +1] - _bucketedDates[index]).Days/365.0m;
                am.AnalyticParameters.StartDiscountFactor = _bucketedDiscountFactors[index];
                am.AnalyticParameters.EndDiscountFactor = _bucketedDiscountFactors[index + 1];
                am.AnalyticParameters.NotionalAmount = 1.0m;
                am.AnalyticParameters.Rate = 0.07431m;

                am.AnalyticParameters.CurveYearFraction = (_bucketedDates[i] - swapStartDate).Days/365;
                am.AnalyticParameters.PeriodAsTimesPerYear = 0.5m;

                couponAnalytics[index] = am;
                ++index;
            }

            Decimal bucketedDelta = 0.0m;
            for (int i = 0; i < analyticModelParameters.Length; ++i)
            {
                bucketedDelta += couponAnalytics[i].BucketedDelta1;
            }
        }
        #endregion

        #region Bucketed Delta 01 (Floating Leg)
        [TestMethod]
        public void TestBucketedDelta2()
        {
            FloatingRateCouponAnalytic[] couponAnalytics;
            IRateCouponParameters[] analyticModelParameters;

            DateTime swapStartDate = new DateTime(2008, 7, 22);

            int len = _bucketedDates.Length;
            couponAnalytics = new FloatingRateCouponAnalytic[len - 1];
            analyticModelParameters = new RateCouponParameters[len - 1];
            int index = 0;

            for (int i = 1; i < len; ++i)
            {
                IRateCouponParameters cp = new RateCouponParameters();
                cp.BucketedDates = CreateArray<DateTime>(_bucketedDates, 0, i + 1);
                cp.BucketedDiscountFactors = CreateArray<Decimal>(_bucketedDiscountFactors, 0, i + 1);

                analyticModelParameters[index] = cp;

                FloatingRateCouponAnalytic am = new FloatingRateCouponAnalytic();
                am.AnalyticParameters = cp;
                am.AnalyticParameters.YearFraction = (_bucketedDates[index + 1] - _bucketedDates[index]).Days / 365.0m;
                am.AnalyticParameters.StartDiscountFactor = _bucketedDiscountFactors[index];
                am.AnalyticParameters.EndDiscountFactor = _bucketedDiscountFactors[index + 1];
                am.AnalyticParameters.NotionalAmount = 1.0m;

                am.AnalyticParameters.CurveYearFraction = (_bucketedDates[i] - swapStartDate).Days / 365;
                am.AnalyticParameters.PeriodAsTimesPerYear = 0.5m;

                am.AnalyticParameters.Rate = 0.08m;
                //am.GetRate(_bucketedDates[index], _bucketedDates[index + 1], _bucketedDiscountFactors[index + 1]);

                couponAnalytics[index] = am;
                ++index;
            }
            Decimal bucketedDelta = 0.0m;
            for (int i = 0; i < analyticModelParameters.Length; ++i)
            {
                
                bucketedDelta += couponAnalytics[i].BucketedDelta1;
            }

        }
        #endregion


        #region Bucketed Delta 01 (Fixed Leg) Test 2
        public void Test2BucketedDelta1()
        {

            Decimal[] bucketedDFs = { 1.0m, 0.982613396m,0.967914626m,0.951866237m,
                                      0.935722838m,0.920271649m,0.90599351m,0.891917121m,
                                      0.876331328m,0.857381051m,0.838427108m,0.819274593m,
                                      0.800559587m,0.786315952m,0.77267004m,0.75896562m,
                                      0.745504268m,0.731916167m,0.719003072m,0.705894128m,
                                      0.693024188m,0.682143198m,0.671798015m,0.661269003m,
                                      0.650905012m,0.641254932m,0.632061478m,0.622696653m,
                                      0.61347058m };


            Decimal[] curveYearFractions = { 0.0m, 0.4958904110m,1.0m,1.495890411m,
                                             2.005479452m,2.495890411m,3.002739726m,3.501369863m,
                                             4.002739726m,4.498630137m,5.002739726m,5.498630137m,
                                             6.002739726m,6.498630137m}; 


            FixedRateCouponAnalytic[] couponAnalytics;
            IRateCouponParameters[] analyticModelParameters;

            int len = curveYearFractions.Length;
            analyticModelParameters = new RateCouponParameters[ len ];
            couponAnalytics = new FixedRateCouponAnalytic[ len ];

            for (int i = 0; i < len; ++i)
            {
                IRateCouponParameters cp = new RateCouponParameters();
                cp.BucketedDiscountFactors = bucketedDFs;

                analyticModelParameters[i] = cp;
                FixedRateCouponAnalytic am = new FixedRateCouponAnalytic();
                am.AnalyticParameters = cp;

                am.AnalyticParameters.StartDiscountFactor = bucketedDFs[0];
                am.AnalyticParameters.EndDiscountFactor = bucketedDFs[len - 1];

                
                am.AnalyticParameters.NotionalAmount = 10000000.0m;
                am.AnalyticParameters.Rate = 0.07m;
                am.AnalyticParameters.YearFraction = 0.495890411m;
                am.AnalyticParameters.CurveYearFraction = curveYearFractions[i];
                am.AnalyticParameters.PeriodAsTimesPerYear = 0.252054795m;

                couponAnalytics[i] = am;
            }

            Decimal bucketedDelta = 0.0m;
            for (int i = 0; i < analyticModelParameters.Length; ++i)
            {
                bucketedDelta += couponAnalytics[i].BucketedDelta1;
            }
        } 
        #endregion


        #region Bucketed Delta 01 (Floating Leg) Test 3
        public void Test3BucketedDelta1()
        {
            Decimal[] bucketedDFs = { 1.0m, 0.982613396m,0.967914626m,0.951866237m,
                                      0.935722838m,0.920271649m,0.90599351m,0.891917121m,
                                      0.876331328m,0.857381051m,0.838427108m,0.819274593m,
                                      0.800559587m,0.786315952m,0.77267004m,0.75896562m,
                                      0.745504268m,0.731916167m,0.719003072m,0.705894128m,
                                      0.693024188m,0.682143198m,0.671798015m,0.661269003m,
                                      0.650905012m,0.641254932m,0.632061478m,0.622696653m,
                                      0.61347058m };


            Decimal[] curveYearFractions = { 0.0m, 0.4958904110m,1.0m,1.495890411m,
                                             2.005479452m,2.495890411m,3.002739726m,3.501369863m,
                                             4.002739726m,4.498630137m,5.002739726m,5.498630137m,
                                             6.002739726m,6.498630137m};

            Decimal[] floatingRates = { 0.066847m,0.068245m,0.066172m,0.067239m,
                                        0.091327m,0.093843m,0.072271m,0.072278m,
                                        0.074327m,0.074361m,0.063716m,0.063673m,
                                        0.06012m,0.060115m};



            FloatingRateCouponAnalytic[] couponAnalytics;
            IRateCouponParameters[] analyticModelParameters;

            int len = curveYearFractions.Length;
            analyticModelParameters = new RateCouponParameters[len];
            couponAnalytics = new FloatingRateCouponAnalytic[len];

            for (int i = 0; i < len; ++i)
            {
                IRateCouponParameters cp = new RateCouponParameters();
                cp.BucketedDiscountFactors = bucketedDFs;

                analyticModelParameters[i] = cp;
                FloatingRateCouponAnalytic am = new FloatingRateCouponAnalytic();
                am.AnalyticParameters = cp;

                am.AnalyticParameters.StartDiscountFactor = bucketedDFs[0];
                am.AnalyticParameters.EndDiscountFactor = bucketedDFs[len - 1];


                am.AnalyticParameters.NotionalAmount = 10000000.0m;
                am.AnalyticParameters.Rate = floatingRates[i];
                am.AnalyticParameters.YearFraction = 0.495890411m;
                am.AnalyticParameters.CurveYearFraction = curveYearFractions[i];
                am.AnalyticParameters.PeriodAsTimesPerYear = 0.252054795m;

                couponAnalytics[i] = am;
            }

            Decimal bucketedDelta = 0.0m;
            for (int i = 0; i < analyticModelParameters.Length; ++i)
            {
                bucketedDelta += couponAnalytics[i].BucketedDelta1;
            }

        }
        #endregion

        [TestMethod]
        public void TestHelperFunction()
        {
            Decimal[] originialArray = { 1.0m, 2.0m, 3.0m, 4.0m, 5.0m };

            Decimal[] resultArray1 = CreateArray<Decimal>(originialArray, 0, 2);
            Decimal[] resultArray2 = CreateArray<decimal>(originialArray, 0, 3);
            Decimal[] resultArray3 = CreateArray<decimal>(originialArray, 0, 4);
        }


        #region Helper function
        private T[] CreateArray<T>(T[] theArray, int index, int length)
        {
            T[] newArray = new T[length];
            int j = 0;

            for (int i = index; i < length; ++i)
            {
                newArray[j] = theArray[i];
                ++j;
            }
            return newArray;
        }
        #endregion
    }
}