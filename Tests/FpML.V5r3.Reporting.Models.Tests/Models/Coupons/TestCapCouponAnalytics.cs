using System;
using System.Diagnostics;
using Orion.Models.Rates.Coupons;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Models.Tests.Models.Coupons
{
    [TestClass]
    public class TestCapCouponAnalytics
    {
        private readonly Decimal[] paramRange = {0.0m, 0.25m, 0.5m, 0.75m,1.0m, 2.0m} ;

        [TestMethod]
        public void CapletRateCouponAnalyicsNonDiscounted()
        {
            foreach (var val in paramRange)
            {
                IRateCouponParameters analyticModelParameters
                    = new RateCouponParameters
                          {
                              Rate = .05m,
                              DiscountType = DiscountType.None,
                              IsCall = true,
                              YearFraction = 0.25m,
                              NotionalAmount = 10000000m,
                              HasReset = false,
                              Volatility = .20m,
                              Strike = .05m,
                              EndDiscountFactor = 0.9m,
                              PaymentDiscountFactor = 0.99m,
                              StartDiscountFactor = 1.0m,
                              ExpiryYearFraction = val,
                              CurveYearFraction = val,
                              PeriodAsTimesPerYear = 0.25m
                          };
                var model = new RateOptionCouponAnalytic
                                {
                                    AnalyticParameters = analyticModelParameters
                                };

                var result = model.ExpectedValue;
                var delta1 = model.Delta1;
                var delta0 = model.Delta0;
                Debug.Print("ExpectedValue1 : {0} Delta1 : {1} Delta0 : {3} CurveYearFraction : {2} ", result, delta1, 
                            analyticModelParameters.CurveYearFraction, delta0);
            }
            foreach (var val in paramRange)
            {
                IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                    {
                                                                        Rate = .05m,
                                                                        DiscountType = DiscountType.None,
                                                                        IsCall = true,
                                                                        YearFraction = val,
                                                                        NotionalAmount = 10000000m,
                                                                        HasReset = false,
                                                                        Volatility = .20m,
                                                                        Strike = .05m,
                                                                        EndDiscountFactor = 0.9m,
                                                                        PaymentDiscountFactor = 0.99m,
                                                                        StartDiscountFactor = 1.0m,
                                                                        ExpiryYearFraction = val,
                                                                        CurveYearFraction = val,
                                                                        PeriodAsTimesPerYear = 0.25m
                                                                    };
                var model = new RateOptionCouponAnalytic
                                {
                                    AnalyticParameters = analyticModelParameters
                                };

                var result = model.ExpectedValue;
                var delta1 = model.Delta1;
                var delta0 = model.Delta0;
                Debug.Print("ExpectedValue2 : {0} Delta1 : {1} Delta0 : {4} CurveYearFraction : {2} YearFraction : {3}", result, delta1, analyticModelParameters.CurveYearFraction,
                            analyticModelParameters.YearFraction, delta0);
            }
            foreach (var val in paramRange)
            {
                IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                    {
                                                                        Rate = .05m,
                                                                        DiscountType = DiscountType.None,
                                                                        IsCall = true,
                                                                        YearFraction = 0.25m,
                                                                        NotionalAmount = 10000000m,
                                                                        HasReset = false,
                                                                        Volatility = .20m,
                                                                        Strike = val,
                                                                        EndDiscountFactor = 0.9m,
                                                                        PaymentDiscountFactor = 0.99m,
                                                                        StartDiscountFactor = 1.0m,
                                                                        ExpiryYearFraction = 3.0m,
                                                                        CurveYearFraction = 3.0m,
                                                                        PeriodAsTimesPerYear = 0.25m
                                                                    };
                var model = new RateOptionCouponAnalytic
                                {
                                    AnalyticParameters = analyticModelParameters
                                };

                var result = model.ExpectedValue;
                var delta1 = model.Delta1;
                var delta0 = model.Delta0;
                Debug.Print("ExpectedValue3 : {0} Delta1 : {1} Delta0 : {4} ForwardRate : {2} Strike : {3}", result, delta1, analyticModelParameters.Rate,
                            analyticModelParameters.Strike, delta0);
            }
            foreach (var val in paramRange)
            {
                IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                    {
                                                                        Rate = .05m,
                                                                        DiscountType = DiscountType.None,
                                                                        IsCall = true,
                                                                        YearFraction = 0.25m,
                                                                        NotionalAmount = 10000000m,
                                                                        HasReset = false,
                                                                        Volatility = val,
                                                                        Strike = .05m,
                                                                        EndDiscountFactor = 0.9m,
                                                                        PaymentDiscountFactor = 0.99m,
                                                                        StartDiscountFactor = 1.0m,
                                                                        ExpiryYearFraction = 3.0m,
                                                                        CurveYearFraction = 3.0m,
                                                                        PeriodAsTimesPerYear = 0.25m
                                                                    };
                var model = new RateOptionCouponAnalytic
                                {
                                    AnalyticParameters = analyticModelParameters
                                };

                var result = model.ExpectedValue;
                var delta1 = model.Delta1;
                var delta0 = model.Delta0;
                Debug.Print("ExpectedValue4 : {0} Delta1 : {1} Delta0 : {3} Volatility : {2}", result, delta1, 
                            analyticModelParameters.Volatility, delta0);
            }
            foreach (var val in paramRange)
            {
                IRateCouponParameters analyticModelParameters = new RateCouponParameters
                                                                    {
                                                                        Rate = .05m,
                                                                        DiscountType = DiscountType.None,
                                                                        IsCall = true,
                                                                        YearFraction = 0.25m,
                                                                        NotionalAmount = 10000000m,
                                                                        HasReset = false,
                                                                        Volatility = 0.2m,
                                                                        Strike = val,
                                                                        EndDiscountFactor = 0.9m,
                                                                        PaymentDiscountFactor = 0.99m,
                                                                        StartDiscountFactor = 1.0m,
                                                                        ExpiryYearFraction = 3.0m,
                                                                        CurveYearFraction = 3.0m,
                                                                        PeriodAsTimesPerYear = 0.25m
                                                                    };
                var model = new RateOptionCouponAnalytic
                                {
                                    AnalyticParameters = analyticModelParameters
                                };

                var result = model.ExpectedValue;
                var delta1 = model.Delta1;
                var delta0 = model.Delta0;
                Debug.Print("ExpectedValue5 : {0} Delta1 : {1} Delta0 : {3} Strike : {2}", result, delta1,
                            analyticModelParameters.Strike, delta0);
            }
        }
    }
}