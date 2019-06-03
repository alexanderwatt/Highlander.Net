using System;
using System.Collections.Generic;
using System.Text;

namespace National.QRSC.AnalyticModels.Tests.Models.Credit
{
    public class CreditStaticTestData
    {
       public static string fxForwardTestTrade =
        "<trade_blob><trade>" +
        "<trade_static product=\"FX Forward\" source_system=\"BRIK\" source_id=\"1\" />" +
        "<trade_fields>" +
        "<trade_field name=\"RecAmount\" value=\"6510000\" />" + 
        "<trade_field name=\"RecCurrency\" value=\"USD\" />" +
        "<trade_field name=\"PayAmount\" value=\"10000000\" />" +
        "<trade_field name=\"PayCurrency\" value=\"AUD\" />" + 
        "<trade_field name=\"SettleDate\" value=\"2010-03-18\" />" +
        "</trade_fields>" +
        "<cashflows />" +
        "</trade></trade_blob>";


        /* Normal test data */
        public static string[] discountCurveNames = { "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", 
                                            "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", 
                                            "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", 
                                            "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", 
                                            "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", 
                                            "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", 
                                            "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", "RateCurve.AUD-LIBOR-BBA-3M.13/11/2008", 
                                          };

        public static string[] forwardCurveNames = discountCurveNames;
        public static string[] volaitilityCurveNames = discountCurveNames;

        public static string[] unadjustedStartDates = {
                                                 "2009-06-17",
"2009-12-17",
"2010-06-17",
"2010-12-17",
"2011-06-17",
"2011-12-17",
"2012-06-17",
"2012-12-17",
"2013-06-17",
"2013-12-17",
"2014-06-17",
"2014-12-17",
"2015-06-17",
"2015-12-17"
                                              };

        public static string[] unadjustedEndDates = {
                                                "2009-06-17",
"2009-12-17",
"2010-06-17",
"2010-12-17",
"2011-06-17",
"2011-12-17",
"2012-06-17",
"2012-12-17",
"2013-06-17",
"2013-12-17",
"2014-06-17",
"2014-12-17",
"2015-06-17",
"2015-12-17"
                                              };

        public static string[] paymentDates = {
                                               "2009-06-17",
"2009-12-17",
"2010-06-17",
"2010-12-17",
"2011-06-17",
"2011-12-17",
"2012-06-17",
"2012-12-17",
"2013-06-17",
"2013-12-17",
"2014-06-17",
"2014-12-17",
"2015-06-17",
"2015-12-17"
                                              };

        public static string[] unadjustedPaymentDates = {
                                             "2009-06-17",
"2009-12-17",
"2010-06-17",
"2010-12-17",
"2011-06-17",
"2011-12-17",
"2012-06-17",
"2012-12-17",
"2013-06-17",
"2013-12-17",
"2014-06-17",
"2014-12-17",
"2015-06-17",
"2015-12-17"
                                              };

        public static string[] adjustedStartDates = {
                                               "2009-06-17",
"2009-12-17",
"2010-06-17",
"2010-12-17",
"2011-06-17",
"2011-12-17",
"2012-06-17",
"2012-12-17",
"2013-06-17",
"2013-12-17",
"2014-06-17",
"2014-12-17",
"2015-06-17",
"2015-12-17"
                                              };

        public static string[] adjustedEndDates = 
             {
             "2009-06-17",
"2009-12-17",
"2010-06-17",
"2010-12-17",
"2011-06-17",
"2011-12-17",
"2012-06-17",
"2012-12-17",
"2013-06-17",
"2013-12-17",
"2014-06-17",
"2014-12-17",
"2015-06-17",
"2015-12-17"
             };

        public static string[] businessCenters = 
            {
                "AUSY",  "AUSY",  "AUSY",  "AUSY",  "AUSY",  "AUSY",  "AUSY",  "AUSY",  
                "AUSY",  "AUSY",  "AUSY",  "AUSY",  "AUSY",  "AUSY"  
            };

        public static decimal[] fixedRate = new decimal[] 
         {
               0.5M, 0.5M, 0.5M, 0.5M, 0.5M, 0.5M, 0.5M, 
               0.5M, 0.5M, 0.5M, 0.5M, 0.5M, 0.5M, 0.5M 
         };

        public static decimal[] floatingRate = new decimal[14];

        //BusinessDayConventionEnum[] dateAdjustmentConventions = 
        //    {
        //        BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  
        //        BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  
        //        BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING,
        //        BusinessDayConventionEnum.MODFOLLOWING,  BusinessDayConventionEnum.MODFOLLOWING
        //    };

        public static Boolean[] rateObservationSpecified = 
            {
                false, false, false, false, false, false, false, 
                false, false, false, false, false, false, false 
            };

        public static decimal[] observedRates = new Decimal[14];

        public static string[] resetDates = 
            {
                "2008-12-17",
"2009-06-17",
"2009-12-17",
"2010-06-17",
"2010-12-17",
"2011-06-17",
"2011-12-17",
"2012-06-17",
"2012-12-17",
"2013-06-17",
"2013-12-17",
"2014-06-17",
"2014-12-17",
"2015-06-17"
            };

        public static decimal[] notionals = 
            {
                10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m, 
                10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m, 10000000.00m 
            };

        public static string[] currency = 
            {
                "AUD", "AUD", "AUD", "AUD", "AUD", "AUD", "AUD", "AUD", 
                "AUD", "AUD", "AUD", "AUD", "AUD", "AUD"
            };

        public static string[] rateIndexNames = 
            {
                "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", 
                "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", 
                "AUD-LIBOR-BBA", "AUD-LIBOR-BBA", "AUD-LIBOR-BBA"
            };

        public static string[] rateIndexTenors = 
            {
                "6M", "6M", "6M",  "6M", "6M", "6M", "6M", "6M", "6M", "6M", 
                "6M", "6M", "6M", "6M" 
            };

        public static string[] dayCountConventions = 
            {
                "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", 
                "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", 
                "ACT/365.FIXED", "ACT/365.FIXED"
            };

        public static string[] dateAdjustmentConventions = 
            {
                "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", 
                "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", "ACT/365.FIXED", 
                "ACT/365.FIXED", "ACT/365.FIXED"
            };

        public static string[] discountingTypes = 
            {
                "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard", "Standard"
            };

        public static decimal[] couponYearFraction = new Decimal[14];

        public static decimal[] margins = new Decimal[14];

        public static decimal[] strikes = 
            {
                0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m, 0.05m
            };

        //Counterparty details
        public static int CounterpartyRatingID = 0; //(Unrated)
        public static string LendingCategory = "A";
        public static string LGDCounterpartyType = "LARGE CORPORATE";
        public static string RegCapCounterpartyType = "CORPORATE";
        public static string Region = "Australia";
        public static string CapitalType = "TIER1";

        public static string IRSwap_scenario2 =
       "<trade_blob><trade><trade_static product=\"IR Swap\" source_system=\"CASPAR\" source_id=\"1\" />" +
       "<cashflows>" +
        "<cashflow flow_date=\"2009-01-21\" flow_type=\"FI\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.5\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
        "<cashflow flow_date=\"2009-07-21\" flow_type=\"FI\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.5\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
        "<cashflow flow_date=\"2010-01-21\" flow_type=\"FI\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.5\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
        "<cashflow flow_date=\"2010-07-21\" flow_type=\"FI\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.5\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
        "<cashflow flow_date=\"2009-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.0\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"AUD-LIBOR-BBA\" leg=\"0\" />" +
        "<cashflow flow_date=\"2009-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.0\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"AUD-LIBOR-BBA\" leg=\"0\" />" +
        "<cashflow flow_date=\"2010-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.0\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"AUD-LIBOR-BBA\" leg=\"0\" />" +
        "<cashflow flow_date=\"2010-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.0\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"AUD-LIBOR-BBA\" leg=\"0\" />" +
        "</cashflows></trade></trade_blob>";

        public static string IRSwap_scenario1 =
         "<trade_blob><trade><trade_static product=\"IR Swap\" source_system=\"CASPAR\" source_id=\"4\" /><trade_fields>" +
         "<trade_field name=\"BreakDate\" value=\"2010-07-17\" /><trade_field name=\"SwapStartDate\" value=\"2004-07-17\" />" +
         "<trade_field name=\"SwapEndDate\" value=\"2008-07-21\" /><trade_field name=\"Principal\" value=\"10000000\" />" +
         "<trade_field name=\"Currency\" value=\"USD\" /><trade_field name=\"PayFixedRate\" value=\"0.1\" />" +
         "<trade_field name=\"PayFloatFlag\" value=\"F\" /><trade_field name=\"PayFrequency\" value=\"2\" />" +
         "<trade_field name=\"PayArrearsFlag\" value=\"ARR\" /><trade_field name=\"PayInterestStyle\" value=\"S\" />" +
         "<trade_field name=\"PayBasis\" value=\"ACT365\" /><trade_field name=\"RecFixedRate\" value=\"0\" />" +
         "<trade_field name=\"RecFloatFlag\" value=\"T\" /><trade_field name=\"RecFloatRateSet\" value=\"ADV\" />" +
         "<trade_field name=\"RecRateIndex\" value=\"USD-SIBOR-SIBO\" /><trade_field name=\"RecFrequency\" value=\"2\" />" +
         "<trade_field name=\"RecArrearsFlag\" value=\"ARR\" /><trade_field name=\"RecInterestStyle\" value=\"S\" />" +
         "<trade_field name=\"RecBasis\" value=\"ACT365\" /><trade_field name=\"StubFlag\" value=\"BEG\" />" +
         "<trade_field name=\"PrincipalChangeRate\" value=\"0\" /><trade_field name=\"PrincipalChangeFlag\" value=\"N\" />" +
         "</trade_fields><cashflows>" +
         "<cashflow flow_date=\"2004-01-21\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2004-07-21\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2005-01-21\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2005-07-21\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2006-01-23\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-186\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2006-07-21\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-179\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2007-01-22\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-185\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2007-07-23\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2008-01-22\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2008-07-21\" flow_type=\"FI\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.1\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"\" leg=\"0\" />" +
         "<cashflow flow_date=\"2004-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2004-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2005-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2005-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2006-01-23\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-186\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2006-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-179\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2007-01-22\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-185\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2007-07-23\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2008-01-22\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "<cashflow flow_date=\"2008-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.0\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"USD-SIBOR-SIBO\" leg=\"0\" />" +
         "</cashflows></trade></trade_blob>";


        public static string CcySwap_scenario1 = "<trade_blob>" +
            "<trade>" +
              "<trade_static product=\"CC Swap\" source_system=\"CASPAR\" source_id=\"2\" />" +
              "<trade_fields>" +
                "<trade_field name=\"BreakDate\" value=\"2010-07-17\" />" +
                "<trade_field name=\"SwapStartDate\" value=\"2004-07-17\" />" +
                "<trade_field name=\"SwapEndDate\" value=\"2008-07-17\" />" +
                "<trade_field name=\"PayPrincipal\" value=\"600000000\" />" +
                "<trade_field name=\"PayCurrency\" value=\"USD\" />" +
                "<trade_field name=\"PayFixedRate\" value=\"0.0005\" />" +
                "<trade_field name=\"PayFloatFlag\" value=\"T\" />" +
                "<trade_field name=\"PayFrequency\" value=\"2\" />" +
                "<trade_field name=\"PayArrearsFlag\" value=\"T\" />" +
                "<trade_field name=\"PayBasis\" value=\"ACT365\" />" +
                "<trade_field name=\"RecPrincipal\" value=\"1000000000\" />" +
                "<trade_field name=\"RecCurrency\" value=\"AUD\" />" +
                "<trade_field name=\"RecFixedRate\" value=\"0.001\" />" +
                "<trade_field name=\"RecFloatFlag\" value=\"T\" />" +
                "<trade_field name=\"RecFrequency\" value=\"2\" />" +
                "<trade_field name=\"RecArrearsFlag\" value=\"T\" />" +
                "<trade_field name=\"RecBasis\" value=\"ACT365\" />" +
                "<trade_field name=\"StubFlag\" value=\"BEG\" />" +
                "<trade_field name=\"PayRateIndex\" value=\"USD-LIBOR-BBA\" />" +
                "<trade_field name=\"RecRateIndex\" value=\"AUD-BBR-BBSW\" />" +
                "<trade_field name=\"PrincipalChangeRate\" value=\"0\" />" +
                "<trade_field name=\"PrincipalChangeFlag\" value=\"N\" />" +
              "</trade_fields>" +
              "<cashflows>" +
                "<cashflow flow_date=\"2003-07-21\" flow_type=\"PE\" asset=\"USD\" amount=\"600000000\" rateset_days=\"0\" strike=\"0.0\" bought_sold=\"R\" leg=\"0\" />" +
                "<cashflow flow_date=\"2003-07-21\" flow_type=\"PE\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"0\" strike=\"0.0\" bought_sold=\"P\" leg=\"0\" />" +
                "<cashflow flow_date=\"2003-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-01-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-94\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-88\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-10-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-94\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-01-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-04-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-07-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-10-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-01-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-01-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-94\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-04-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-07-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-10-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-01-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-04-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-07-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-04-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-01-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-94\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-88\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-10-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-94\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-01-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-04-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-07-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-10-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-01-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-04-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-07-22\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-01-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-90\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-10-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-92\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-01-23\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-94\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-04-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-88\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-07-21\" flow_type=\"V\" asset=\"USD\" amount=\"600000000\" rateset_days=\"-91\" strike=\"0.0005\" bought_sold=\"P\" asset2=\"ACT365\" flags=\"USD-LIBOR-BBA\" leg=\"0\" />" +
                "<cashflow flow_date=\"2003-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-01-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-94\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-88\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-10-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-94\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-01-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-04-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-07-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-10-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-01-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-01-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-94\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-04-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-07-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-10-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-01-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-04-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-07-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-04-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-01-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-94\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-88\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-10-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-94\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-01-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-04-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-07-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-10-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-01-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-04-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-07-22\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-01-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-90\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-10-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-92\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-01-23\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-94\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-04-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-88\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-07-21\" flow_type=\"V\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"-91\" strike=\"0.001\" bought_sold=\"R\" asset2=\"ACT365\" flags=\"AUD-BBR-BBSW\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-07-21\" flow_type=\"PE\" asset=\"USD\" amount=\"600000000\" rateset_days=\"0\" strike=\"0.0\" bought_sold=\"P\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-07-21\" flow_type=\"PE\" asset=\"AUD\" amount=\"1000000000\" rateset_days=\"0\" strike=\"0.0\" bought_sold=\"R\" leg=\"0\" />" +
              "</cashflows>" +
            "</trade>" +
          "</trade_blob>";

        public static string IRCapFloor_scenario1 = "<trade_blob>" +
            "<trade>" +
              "<trade_static product=\"IR Cap Floor\" source_system=\"CASPAR\" source_id=\"10\" />" +
              "<trade_fields>" +
                "<trade_field name=\"CapFloorFlag\" value=\"F\" />" +
                "<trade_field name=\"Principal\" value=\"10000000\" />" +
                "<trade_field name=\"DealCurrency\" value=\"USD\" />" +
                "<trade_field name=\"StrikeRate\" value=\"0.3\" />" +
                "<trade_field name=\"Margin\" value=\"0\" />" +
                "<trade_field name=\"Basis\" value=\"ACT365\" />" +
                "<trade_field name=\"CashflowFrequency\" value=\"2\" />" +
                "<trade_field name=\"StartDate\" value=\"2003-07-21\" />" +
                "<trade_field name=\"EndDate\" value=\"2033-07-21\" />" +
                "<trade_field name=\"BuySellFlag\" value=\"B\" />" +
                "<trade_field name=\"CapFloorType\" value=\"SAR\" />" +
                "<trade_field name=\"StubFlag\" value=\"BEG\" />" +
                "<trade_field name=\"PrincipalChangeRate\" value=\"0\" />" +
                "<trade_field name=\"PrincipalChangeFlag\" value=\"N\" />" +
              "</trade_fields>" +
              "<cashflows>" +
                "<cashflow flow_date=\"2004-01-21\" flow_type=\"SO\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"2.51042994288202E-02\" flags=\"F\" settle_date=\"2003-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2004-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2004-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2004-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2005-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2005-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-01-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-186\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2005-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2006-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-179\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2006-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-185\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2006-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2007-07-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2007-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2007-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2008-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2008-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2008-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2009-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2009-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2009-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2010-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2010-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2010-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2011-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2011-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-01-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-186\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2011-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2012-07-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2012-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2012-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2013-07-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2013-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2013-07-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2014-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2014-01-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2014-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2015-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2015-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2015-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2016-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2016-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-01-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-186\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2016-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2017-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-179\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2017-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-185\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2017-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2018-07-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2018-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2018-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2019-07-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2019-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2019-07-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2020-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2020-01-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2020-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2021-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2021-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2021-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2022-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2022-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-01-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-186\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2022-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2023-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-179\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2023-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2024-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-185\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2023-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2024-07-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2024-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2025-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2024-07-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2025-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2025-01-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2026-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2025-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2026-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2026-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2027-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2026-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2027-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2027-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2028-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2027-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2028-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2028-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2029-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-185\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2028-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2029-07-23\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2029-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2030-01-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2029-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2030-07-22\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2030-01-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2031-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-183\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2030-07-18\" leg=\"0\" />" +
                "<cashflow flow_date=\"2031-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2031-01-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2032-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2031-07-17\" leg=\"0\" />" +
                "<cashflow flow_date=\"2032-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-182\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2032-01-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2033-01-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-184\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2032-07-19\" leg=\"0\" />" +
                "<cashflow flow_date=\"2033-07-21\" flow_type=\"O\" asset=\"USD\" amount=\"10000000\" rateset_days=\"-181\" strike=\"0.3\" bought_sold=\"B\" asset2=\"ACT365\" amount2=\"0.0\" barrier=\"0\" flags=\"F\" settle_date=\"2033-01-19\" leg=\"0\" />" +
              "</cashflows>" +
            "</trade>" +
          "</trade_blob>";
    }
}
