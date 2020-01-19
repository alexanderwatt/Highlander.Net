#region Using

using System;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;
using Orion.Util.Serialisation;
using Orion.Constants;
using Orion.CalendarEngine.Helpers;
using Orion.CurveEngine.Factory;
using Orion.UnitTestEnv;
using Orion.TestHelpers;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.ModelFramework.MarketEnvironments;
using FpML.V5r10.Reporting.ModelFramework.PricingStructures;
using Orion.CurveEngine.Assets;
using Orion.CurveEngine.Markets;
using Orion.CurveEngine.PricingStructures;
using Orion.CurveEngine.PricingStructures.Curves;
using Microsoft.XmlDiffPatch;
using FpML.V5r10.Reporting.Models.Assets;
using Exception = System.Exception;

#endregion

namespace Orion.CurveEngine.Tests 
{
    [TestClass]
    public class CurveEngineTests1
    {
        #region Constants, Fields

        private const string _floatingRateIndex = "AUD-BBR-BBSW";
        private const string _businessDayConvention = "MODFOLLOWING";
        private const string _businessCenters = "AUSY-GBLO";
        private const string _currency = "AUD";
        private const string _dayCountFraction = "ACT/365.FIXED";
        private const string _term = "3M";
        private const string _spotDays = "0D";
        private const string _spotDateTypeEnum = "Calendar";
        private const string _spotgbusinessDayConvention = "NONE";
        private const string _dayRelativeTo = "Today";
        private const string _oisIndexName = "AUD-AONIA-OIS-COMPOUND-3M";
        
        private const string _GBPliborIndexName = "GBP-LIBOR-ISDA";
        private const string _liborIndexName = "AUD-LIBOR-BBA-3M";

        private const string MarketFile = @"FpML.MarketTest.xml";

        private const double testrate = 0.05;
        private readonly InterpolationMethod interp = InterpolationMethodHelper.Parse("LinearInterpolation");
        private readonly InterpolationMethod interp2 = InterpolationMethodHelper.Parse("LinearRateInterpolation");
        private readonly double[] testtimes = { 0.0, 0.5, 1.0, 2.0, 3.5, 10.0 };
        private readonly double[] testtimes2 = { 0.0, 0.5, 1.0, 2.0, 3.5, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0, 10.0, 11.0, 12.0, 13.0, 14.0, 1.0, 20.0 };

        private readonly DateTime fxbaseDate = new DateTime(2010, 7, 23);
        private readonly string[] audFxInstruments
            = {
                  "AUDUSD-FxForward-ON",
                  "AUDUSD-FxForward-TN",
                  "AUDUSD-FxSpot-SP",
                  "AUDUSD-FxForward-1W",
                  "AUDUSD-FxForward-2W",
                  "AUDUSD-FxForward-3W",
                  "AUDUSD-FxForward-1M",
                  "AUDUSD-FxForward-2M",
                  "AUDUSD-FxForward-3M",
                  "AUDUSD-FxForward-4M",
                  "AUDUSD-FxForward-5M",
                  "AUDUSD-FxForward-6M",
                  "AUDUSD-FxForward-9M",
                  "AUDUSD-FxForward-15M",
                  "AUDUSD-FxForward-18M",
                  "AUDUSD-FxForward-21M",
                  "AUDUSD-FxForward-1Y",
                  "AUDUSD-FxForward-2Y",
                  "AUDUSD-FxForward-3Y",
                  "AUDUSD-FxForward-4Y",
                  "AUDUSD-FxForward-5Y",
                  "AUDUSD-FxForward-7Y",
                  "AUDUSD-FxForward-10Y",
                  "AUDUSD-FxForward-15Y"
              };

        private const string fxAlgo = "LinearForward";
        //private const decimal BaseRate = .07m;
        private readonly decimal[] fxvalues =
            new[]
                {
                    0.875552m,
                    0.875551m,
                    //0.875550m,
                    0.875545m,
                    0.874816m,
                    0.874192m,
                    0.8734555m,
                    0.872407m,
                    0.869036m,
                    0.865965m,
                    0.8626625m,
                    0.859145m,
                    0.856055m,
                    0.8466175m,
                    0.83733m,
                    0.8278265m,
                    0.818725m,
                    0.810341m,
                    0.8022545m,
                    0.773495m,
                    0.7467275m,
                    0.726071m,
                    0.6929225m,
                    0.6556605m,
                    0.6150845m,
                };
        private readonly string[] AUDdeposits = { "AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Deposit-1M", "AUD-Deposit-2M", "AUD-Deposit-3M", "AUD-Deposit-6M", "AUD-Deposit-9M", "AUD-Deposit-12M", "AUD-Deposit-24M" };
        
        private readonly string[] AUDbbsw = { "AUD-Xibor-1D", "AUD-Xibor-1W", "AUD-Xibor-1M", "AUD-Xibor-2M", "AUD-Xibor-3M", "AUD-Xibor-6M", "AUD-Xibor-9M", "AUD-Xibor-12M", "AUD-Xibor-24M" };
        
        private readonly string[] AUDFra = {"AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Deposit-1M",
                                            "AUD-Fra-1M-3M", "AUD-Fra-3M-3M", "AUD-Fra-6M-3M", 
                                            "AUD-Fra-9M-3M", "AUD-Fra-12M-3M", "AUD-Fra-15M-3M", 
                                            "AUD-Fra-18M-3M", "AUD-Fra-21M-3M" };

        private readonly string[] AUDZeroRates = { "AUD-ZeroRate-1D", "AUD-ZeroRate-1W", "AUD-ZeroRate-1M", "AUD-ZeroRate-2M", "AUD-ZeroRate-3M", "AUD-ZeroRate-6M", "AUD-ZeroRate-9M", "AUD-ZeroRate-12M", "AUD-ZeroRate-24M" };
        
        private readonly string[] AUDBillFra = {"AUD-BankBill-1D", "AUD-BankBill-1W", "AUD-BankBill-1M",
                                                "AUD-BillFra-1M-3M", "AUD-BillFra-3M-3M", "AUD-BillFra-6M-3M", 
                                                "AUD-BillFra-9M-3M", "AUD-BillFra-12M-3M", "AUD-BillFra-15M-3M", 
                                                "AUD-BillFra-18M-3M", "AUD-BillFra-21M-3M" };

        private readonly string[] AUDOis = { "AUD-OIS-1D", "AUD-OIS-1W", "AUD-OIS-1M", "AUD-OIS-2M", 
                                             "AUD-OIS-3M", "AUD-OIS-6M", "AUD-OIS-9M" };

        private readonly string[] AUDSimpleIRSwap = {"AUD-Deposit-1D", "AUD-Deposit-1M", "AUD-Deposit-3M", "AUD-Deposit-6M", 
                                                     "AUD-IRSwap-1Y", "AUD-IRSwap-2Y", "AUD-IRSwap-3Y", 
                                                     "AUD-IRSwap-5Y", "AUD-IRSwap-7Y", "AUD-IRSwap-10Y"
                                                    };
        private readonly string[] AUDIBFuture = { "AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Deposit-1M", 
                                                  "AUD-IRFuture-IB-J8", "AUD-IRFuture-IB-K8", "AUD-IRFuture-IB-M8", 
                                                  "AUD-IRFuture-IB-N8", "AUD-IRFuture-IB-Q8", "AUD-IRFuture-IB-U8", "AUD-IRFuture-IR-U8",
                                                  "AUD-IRFuture-IR-Z8", "AUD-IRFuture-IR-H9", "AUD-IRFuture-IR-M9", "AUD-IRFuture-IR-U9"};

        private readonly string[] AUDIRFuture = { "AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Deposit-1M", 
                                                  "AUD-IRFuture-IR-H8", "AUD-IRFuture-IR-M8", "AUD-IRFuture-IR-U8", "AUD-IRFuture-IR-Z8", 
                                                  "AUD-IRFuture-IR-H9", "AUD-IRFuture-IR-M9", "AUD-IRFuture-IR-U9", "AUD-IRFuture-IR-Z9",
                                                  "AUD-IRFuture-IR-H0", "AUD-IRFuture-IR-M0", "AUD-IRFuture-IR-U0", "AUD-IRFuture-IR-Z0"};

        private readonly string[] AUDMixedRates = { "AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Xibor-1M", "AUD-Xibor-2M", "AUD-Xibor-3M",
                                                    "AUD-IRFuture-IR-H8", "AUD-IRFuture-IR-M8", "AUD-IRFuture-IR-U8", "AUD-IRFuture-IR-Z8", 
                                                    "AUD-IRFuture-IR-H9", "AUD-IRFuture-IR-M9", "AUD-IRFuture-IR-U9", "AUD-IRFuture-IR-Z9",
                                                    "AUD-IRSwap-3Y", "AUD-IRSwap-5Y", "AUD-IRSwap-7Y", "AUD-IRSwap-10Y", };

        private readonly string[] GBPdeposits = { "GBP-Deposit-1D", "GBP-Deposit-2D", "GBP-Deposit-1M", "GBP-Deposit-2M", "GBP-Deposit-3M", "GBP-Deposit-6M", "GBP-Deposit-9M", "GBP-Deposit-12M", "GBP-Deposit-24M" };

        private readonly string[] GBPbbsw = { "GBP-Deposit-1D", "GBP-Deposit-1W", "GBP-Xibor-1M", "GBP-Xibor-2M", "GBP-Xibor-3M", "GBP-Xibor-6M", "GBP-Xibor-9M", "GBP-Xibor-12M", "GBP-Xibor-24M" };

        private readonly string[] GBPFra = {"GBP-Deposit-1D", "GBP-Deposit-2D", "GBP-Deposit-1M",
                                            "GBP-Fra-1M-3M", "GBP-Fra-3M-3M", "GBP-Fra-6M-3M", 
                                            "GBP-Fra-9M-3M", "GBP-Fra-12M-3M", "GBP-Fra-15M-3M", 
                                            "GBP-Fra-18M-3M", "GBP-Fra-21M-3M" };
        private readonly string[] GBPZeroRates = { "GBP-ZeroRate-1D", "GBP-ZeroRate-2D", "GBP-ZeroRate-1M", "GBP-ZeroRate-2M", "GBP-ZeroRate-3M", "GBP-ZeroRate-6M", "GBP-ZeroRate-9M", "GBP-ZeroRate-12M", "GBP-ZeroRate-24M" };


        private readonly string[] GBPSimpleIRSwap = {"GBP-Deposit-1D", "GBP-Deposit-1M", "GBP-Deposit-3M", "GBP-Deposit-6M", 
                                                     "GBP-IRSwap-1Y", "GBP-IRSwap-2Y", "GBP-IRSwap-3Y", 
                                                     "GBP-IRSwap-5Y", "GBP-IRSwap-7Y", "GBP-IRSwap-10Y"
                                                    };

        private readonly string[] GBPIRFuture = { "GBP-Deposit-1D", "GBP-Deposit-2D", "GBP-Deposit-1M", 
                                                  "GBP-IRFuture-L-H8", "GBP-IRFuture-L-M8", "GBP-IRFuture-L-U8", "GBP-IRFuture-L-Z8", 
                                                  "GBP-IRFuture-L-H9", "GBP-IRFuture-L-M9", "GBP-IRFuture-L-U9", "GBP-IRFuture-L-Z9",
                                                  "GBP-IRFuture-L-H0", "GBP-IRFuture-L-M0", "GBP-IRFuture-L-U0", "GBP-IRFuture-L-Z0"};

        private readonly string[] GBPMixedRates = { "GBP-Deposit-1D", "GBP-Deposit-2D", "GBP-Xibor-1M", "GBP-Xibor-2M", "GBP-Xibor-3M",                                                   
                                                    "GBP-IRFuture-L-H8", "GBP-IRFuture-L-M8", "GBP-IRFuture-L-U8", "GBP-IRFuture-L-Z8", 
                                                    "GBP-IRFuture-L-H9", "GBP-IRFuture-L-M9", "GBP-IRFuture-L-U9", "GBP-IRFuture-L-Z9",
                                                    "GBP-IRSwap-3Y", "GBP-IRSwap-5Y", "GBP-IRSwap-7Y", "GBP-IRSwap-10Y", };

        //private readonly string[] algoNames = { "Simple algorithm", "Algorithm1", "Algorithm2", "Algorithm3", "Algorithm4" };
        private readonly string[] algoNames = { "FlatForward",   "LinearZero"};

        private readonly string[] algoNames3 = { "SimpleGapStep" };

        private readonly DateTime BaseDate = new DateTime(2009, 6, 10);
        
        private readonly DateTime baseDate = new DateTime(2008, 3, 3);

        private readonly string[] AUDXccySwap = { "AUD-XccySwap-1Y", 
                                                  "AUD-XccySwap-2Y", 
                                                  "AUD-XccySwap-3Y", 
                                                  "AUD-XccySwap-5Y", 
                                                  "AUD-XccySwap-7Y", 
                                                  "AUD-XccySwap-10Y" };

        private readonly string[] AUDXccyBasis = { "AUD-XccyBasisSwap-3M", 
                                                   "AUD-XccyBasisSwap-6M", 
                                                   "AUD-XccyBasisSwap-1Y", 
                                                   "AUD-XccyBasisSwap-5Y", 
                                                   "AUD-XccyBasisSwap-7Y", 
                                                   "AUD-XccyBasisSwap-10Y" };

        private readonly string[] AUDInflationAssets = { "AUD-ZCCPISwap-1Y", 
                                                      "AUD-ZCCPISwap-2Y", 
                                                      "AUD-ZCCPISwap-3Y", 
                                                      "AUD-ZCCPISwap-5Y", 
                                                      "AUD-ZCCPISwap-7Y", 
                                                      "AUD-ZCCPISwap-10Y" };

        private readonly string[] AUDBasisAssets = { "AUD-BasisSwap-3M-1M", 
                                                   "AUD-BasisSwap-6M-1M", 
                                                   "AUD-BasisSwap-1Y-1M", 
                                                   "AUD-BasisSwap-5Y-1M", 
                                                   "AUD-BasisSwap-7Y-1M", 
                                                   "AUD-BasisSwap-10Y-1M" };

        private readonly string[] _metrics = new[] { "ImpliedQuote", "AccrualFactor" };

        private readonly string[] AUDinstruments = new[] { 
                                                    "AUD-SpreadFra-1M-3M", 
                                                    "AUD-SpreadFra-3M-3M", 
                                                    "AUD-SpreadFra-6M-3M", 
                                                    "AUD-SpreadFra-9M-3M", 
                                                    "AUD-SpreadFra-12M-3M", 
                                                    "AUD-SpreadFra-15M-3M", 
                                                    "AUD-SpreadFra-18M-3M", 
                                                    "AUD-SpreadFra-21M-3M" };

        private readonly string[] GBPinstruments = new[] { 
                                                    "GBP-SpreadFra-1M-3M", 
                                                    "GBP-SpreadFra-3M-3M", 
                                                    "GBP-SpreadFra-6M-3M", 
                                                    "GBP-SpreadFra-9M-3M", 
                                                    "GBP-SpreadFra-12M-3M", 
                                                    "GBP-SpreadFra-15M-3M", 
                                                    "GBP-SpreadFra-18M-3M", 
                                                    "GBP-SpreadFra-21M-3M" };

        private readonly string[] USDinstruments = new[] { 
                                                    "USD-SpreadFra-1M-3M", 
                                                    "USD-SpreadFra-3M-3M", 
                                                    "USD-SpreadFra-6M-3M", 
                                                    "USD-SpreadFra-9M-3M", 
                                                    "USD-SpreadFra-12M-3M", 
                                                    "USD-SpreadFra-15M-3M", 
                                                    "USD-SpreadFra-18M-3M", 
                                                    "USD-SpreadFra-21M-3M" };

        private readonly string[] EURinstruments = new[] { 
                                                    "EUR-SpreadFra-1M-3M", 
                                                    "EUR-SpreadFra-3M-3M", 
                                                    "EUR-SpreadFra-6M-3M", 
                                                    "EUR-SpreadFra-9M-3M", 
                                                    "EUR-SpreadFra-12M-3M", 
                                                    "EUR-SpreadFra-15M-3M", 
                                                    "EUR-SpreadFra-18M-3M", 
                                                    "EUR-SpreadFra-21M-3M" };

        private readonly string[] JPYinstruments = new[] { 
                                                    "JPY-SpreadFra-1M-3M", 
                                                    "JPY-SpreadFra-3M-3M", 
                                                    "JPY-SpreadFra-6M-3M", 
                                                    "JPY-SpreadFra-9M-3M", 
                                                    "JPY-SpreadFra-12M-3M", 
                                                    "JPY-SpreadFra-15M-3M", 
                                                    "JPY-SpreadFra-18M-3M", 
                                                    "JPY-SpreadFra-21M-3M" };

        private readonly string[] NZDinstruments = new[] { 
                                                    "NZD-SpreadFra-1M-3M", 
                                                    "NZD-SpreadFra-3M-3M", 
                                                    "NZD-SpreadFra-6M-3M", 
                                                    "NZD-SpreadFra-9M-3M", 
                                                    "NZD-SpreadFra-12M-3M", 
                                                    "NZD-SpreadFra-15M-3M", 
                                                    "NZD-SpreadFra-18M-3M", 
                                                    "NZD-SpreadFra-21M-3M" };

        #endregion

        #region IRCap Fields

        private const string BusinessDays = "FOLLOWING";
        private const string Hols = "AUSY";
        private const string Dc = "ACT/365.FIXED";
        private const decimal Spread = 0.0025m;
        private const decimal Rate = 0.052205244110m;
        private const string Currency = "AUD";

        private readonly string[] _dates = {
                                              "13/04/2010",
                                              "12/07/2010",
                                              "10/10/2010",
                                              "8/01/2011",
                                              "8/04/2011",
                                              "7/07/2011",
                                              "5/10/2011",
                                              "3/01/2012",
                                              "2/04/2012",
                                              "1/07/2012",
                                              "29/09/2012",
                                              "28/12/2012",
                                              "28/03/2013",
                                              "26/06/2013",
                                              "24/09/2013",
                                              "23/12/2013",
                                              "23/03/2014",
                                              "21/06/2014",
                                              "19/09/2014",
                                              "18/12/2014",
                                              "18/03/2015"
                                          };

        private readonly string[] _audInstruments = {"AUD-Deposit-1D",
                                                     "AUD-Deposit-1W",
                                                     "AUD-Deposit-2W",
                                                     "AUD-Deposit-1M",
                                                     "AUD-Deposit-2M",
                                                     "AUD-Deposit-3M",
                                                     "AUD-IRSwap-3Y",
                                                     "AUD-IRSwap-4Y",
                                                     "AUD-IRSwap-5Y",
                                                     "AUD-IRSwap-6Y",
                                                     "AUD-IRSwap-7Y",
                                                     "AUD-IRSwap-8Y",
                                                     "AUD-IRSwap-9Y",
                                                     "AUD-IRSwap-10Y",
                                                     "AUD-IRSwap-15Y",
                                                     "AUD-IRSwap-20Y",
                                                     "AUD-IRFuture-IR-M0",
                                                     "AUD-IRFuture-IR-U0",
                                                     "AUD-IRFuture-IR-Z0",
                                                     "AUD-IRFuture-IR-H1",
                                                     "AUD-IRFuture-IR-M1",
                                                     "AUD-IRFuture-IR-U1",
                                                     "AUD-IRFuture-IR-Z1",
                                                     "AUD-IRFuture-IR-H2"};

        private readonly decimal[] _rates =      {0.05m,    0.05m,    0.05m,    0.05m, 
                                                  0.05m,    0.05m,   0.05m,    0.05m, 
                                                  0.05m,   0.05m,   0.05m,   0.05m,
                                                  0.05m,   0.05m,   0.05m,   0.05m,
                                                  0.05m,    0.05m,    0.05m,    0.05m,
                                                  0.05m,    0.05m,    0.05m,    0.05m};

        private readonly decimal[] _adjustments =      {0.00m,    0.00m,    0.00m,    0.00m, 
                                                  0.00m,    0.00m,   0.00m,    0.00m, 
                                                  0.00m,   0.00m,   0.00m,   0.00m,
                                                  0.00m,   0.00m,   0.00m,   0.00m,
                                                  0.15m,    0.15m,    0.15m,    0.15m,
                                                  0.15m,    0.15m,    0.15m,    0.15m};

        private readonly double[] _notionals = { 1 };

        private readonly double[] _tradestrikes = { 1 };

        private readonly string[] _expiries =      {"1D",    "1W",    "1M",    "2M", 
                                                  "3M",    "6M",   "1Y", 
                                                  "2Y",   "3Y",   "5Y",   "7Y",   "10Y"};

        private readonly string[] _strikes =      {"3.50",    "4.00",    "4.50",    "5.00", 
                                                  "5.50",    "6.00",   "6.50",    "7.00", 
                                                  "7.50"};

        private readonly decimal[] _result =      {-0.00492m,
                                                    0.00m,
                                                    0.00m,
                                                    -0.082852m,
                                                    -0.65574030m,
                                                    -1.43011059m,
                                                    -14.8743748m,
                                                    -90.9639m,
                                                    -264.06466808713m,
                                                    0.00m,
                                                    0.00m,
                                                    0.00m,
                                                    0.00m,
                                                    0.00m,
                                                    0.00m,
                                                    0.00m,
                                                    -1.0508265m,
                                                    -1.1904515773816m,
                                                    -0.97466652579073m,
                                                    -0.97065340860m,
                                                    -0.78087653865m,
                                                    -0.7445471144m,
                                                    -0.5506190305m,
                                                    -0.48285875097m
                                                    };


        readonly DateTime _baseDate = new DateTime(2010, 4, 12);

        readonly NamedValueSet _properties = new NamedValueSet();
        readonly NamedValueSet _volproperties = new NamedValueSet();
        double[,] _volatilities;

        #endregion

        #region Properties

        private static CurveUnitTestEnvironment UTE { get; set; }
        private static CurveEngine CurveEngine { get; set; }
        private static CalendarEngine.CalendarEngine CalendarEngine { get; set; }
        private static TimeSpan Retention { get; set; }
        private static IBusinessCalendar FixingCalendar { get; set; }
        private static IBusinessCalendar PaymentCalendar { get; set; }

        #endregion

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new CurveUnitTestEnvironment();
            //Set the calendar engine
            CurveEngine = new CurveEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            CalendarEngine = new CalendarEngine.CalendarEngine(UTE.Logger, UTE.Cache, UTE.NameSpace);
            // Set the Retention
            Retention = TimeSpan.FromHours(1);
            const string center = "AUSY";
            FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(center), UTE.NameSpace);
            PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(UTE.Cache, BusinessCentersHelper.Parse(center), UTE.NameSpace);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            //Logger.Dispose()
            UTE.Dispose();
        }

        #endregion

        #region Generic Instrument Tests

        #region Instrument tests Without Calendars

        [TestMethod]
        public void XiborConfig()
        {
            var xibor = XiborNodeStructHelper.Parse("AUD-BBR-BBSW", "MODFOLLOWING", "AUSY",
                                                    "AUD", "ACT/365.FIXED", "3M", "0D", "Calendar", "NONE", "AUSY", "Today");
            var result = XmlSerializerHelper.SerializeToString(xibor);
            Debug.Print(result);
        }

        [TestMethod]
        public void AssetSetConfiguration()
        {
            var assetSet = new AssetSetConfiguration();
            var instruments = new Instrument[3];

            var xibor = InstrumentNodeHelper.CreateXibor(_floatingRateIndex, _businessDayConvention, _businessCenters,
                                                         _currency, _dayCountFraction, _term, _spotDays, _spotDateTypeEnum, _spotgbusinessDayConvention, _businessCenters, _dayRelativeTo);

            var deposit = InstrumentNodeHelper.CreateDeposit("AUD-Deposit", _floatingRateIndex, _businessDayConvention,
                                                             _businessCenters, _currency, _dayCountFraction, _term, _spotDays,
                                                             _spotDateTypeEnum, _spotgbusinessDayConvention, _businessCenters,
                                                             _dayRelativeTo);
            var fxspot = InstrumentNodeHelper.CreateFxSpot("AUD", "USD", "Currency1PerCurrency2", _spotDays, _spotDateTypeEnum,
                                                           _businessDayConvention,
                                                           _businessCenters, _dayRelativeTo);

            instruments[0] = InstrumentHelper.Create("Xibor", "AUD", xibor);
            instruments[1] = InstrumentHelper.Create("Deposit", "AUD", deposit);
            instruments[1] = InstrumentHelper.Create("FxSpot", "AUDUSD", fxspot);
            assetSet.Instruments = instruments;
            var result = XmlSerializerHelper.SerializeToString(assetSet);
            Debug.Print(result);
        }

        [TestMethod]
        public void AssetSetConfigurationWithItem()
        {
            var assetSet = new AssetSetConfiguration();
            var instruments = new Instrument[2];

            var xibor = InstrumentNodeHelper.CreateXibor(_floatingRateIndex, _businessDayConvention, _businessCenters,
                                                         _currency, _dayCountFraction, _term, _spotDays, _spotDateTypeEnum, _spotgbusinessDayConvention, _businessCenters, _dayRelativeTo);

            var deposit = InstrumentNodeHelper.CreateDeposit("AUD-Deposit", _floatingRateIndex, _businessDayConvention,
                                                             _businessCenters, _currency, _dayCountFraction, _term, _spotDays,
                                                             _spotDateTypeEnum, _spotgbusinessDayConvention, _businessCenters,
                                                             _dayRelativeTo);

            instruments[0] = InstrumentHelper.Create("Xibor", "AUD", "1D,2D", xibor);
            instruments[1] = InstrumentHelper.Create("Deposit", "AUD", "1D,2D", deposit);
            assetSet.Instruments = instruments;
            var result = XmlSerializerHelper.SerializeToString(assetSet);
            Debug.Print(result);
        }

        [TestMethod]
        public void SaveToFile()
        {
            var assetSet = new AssetSetConfiguration();
            var instruments = new Instrument[2];

            var xibor1 = XiborNodeStructHelper.Parse("AUD-BBR-BBSW", "MODFOLLOWING", "AUSY",
                                                     "AUD", "ACT/365.FIXED", "3M", "0D", "Calendar", "NONE", "AUSY", "Today");

            var xibor2 = XiborNodeStructHelper.Parse("AUD-BBR-BBSW", "MODFOLLOWING", "AUSY",
                                                     "AUD", "ACT/365.FIXED", "3M", "0D", "Calendar", "NONE", "AUSY", "Today");

            instruments[0] = InstrumentHelper.Create("Xibor", "AUD", "1D,2D", xibor1);
            instruments[1] = InstrumentHelper.Create("OIS", "AUD", "1D,2D", xibor2);
            assetSet.Instruments = instruments;
            XmlSerializerHelper.SerializeToFile(assetSet, "AssetConfig.config");
        }

        /// <summary>
        /// Checks that all of the Instruments Config can serialize correctly
        /// </summary>
        [TestMethod]
        public void CheckFpmlOfAllAssetsTest()
        {
            IExpression query = Expr.ALL;
            List<Instrument> instruments = UTE.Cache.LoadObjects<Instrument>(query);
            Assert.AreNotEqual(0, instruments.Count);
            foreach (Instrument instrument in instruments)
            {
                string expectedXml = XmlSerializerHelper.SerializeToString(instrument);
                var newInstrument = XmlSerializerHelper.DeserializeFromString<Instrument>(expectedXml);
                string actualXml = XmlSerializerHelper.SerializeToString(newInstrument);
                var actualDoc = new XmlDocument();
                actualDoc.LoadXml(actualXml);
                var expectedDoc = new XmlDocument();
                expectedDoc.LoadXml(expectedXml);
                var xmlDiff = new XmlDiff(XmlDiffOptions.IgnoreChildOrder
                    | XmlDiffOptions.IgnoreNamespaces
                    | XmlDiffOptions.IgnoreXmlDecl) {Algorithm = XmlDiffAlgorithm.Precise};
                var differences = new StringBuilder();
                XmlWriter xmlDifferences = XmlWriter.Create(differences);
                bool pass = xmlDiff.Compare(expectedDoc, actualDoc, xmlDifferences);
                if (!pass)
                {
                    Debug.Print(differences.ToString());
                    Debug.Print("Actual (object serialized):");
                    Debug.Print(actualXml);
                    Debug.Print("Expected (from store):");
                    Debug.Print(expectedXml);
                    string message = string.Format("Failed for Asset '{0}', Currency '{1}', MaturityTenor '{2}'", 
                        instrument.AssetType, instrument.Currency.Value, instrument.ExtraItem);
                    Assert.Fail(message);
                }
            }
        }

        [TestMethod]
        public void CreateSimpleCommodityTests()
        {
            //const string assetId = "USD-CommoditySpot-ICE_B";
            var offset = new Offset
                {
                    dayType = DayTypeEnum.Business,
                    dayTypeSpecified = true,
                    period = PeriodEnum.D,
                    periodMultiplier = "1",
                    periodSpecified = true
                };
            var result = CommodityHelper.Create("OIL", "Ice Brent", "USD", 1, SpecifiedPriceEnum.Spot,  offset, PriceQuoteUnitsEnum.BBL, DeliveryDatesEnum.Spot, "ICE", null);
            Debug.Print(XmlSerializerHelper.SerializeToString(result));
        }

        [TestMethod]
        public void CreateSimpleFraTests()
        {
            const string assetId = "AUD-Fra-6M-3M";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);
            var result = CurveEngine.CreatePriceableAsset(bav, nvs);
            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void CreateBillFraTests()
        {
            const string assetId = "AUD-BillFra-6M-3M";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);
            var result = CurveEngine.CreatePriceableAsset(bav, nvs);
            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void CreateDepositTest()
        {
            const string assetId = "AUD-Deposit-3M";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);
            var result = CurveEngine.CreatePriceableAsset(bav, nvs);
            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void CreateIRSwapTest()
        {
            const string assetId = "AUD-IRSwap-3Y";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);
            var result = CurveEngine.CreatePriceableAsset(bav, nvs);
            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void GetSwapInstrumentTests()
        {
            string[] instruments1 = { "IRSwap" };
            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK" };
            string[] maturities = { "1Y", "2Y", "3Y", "4Y", "5Y", "6Y", "7Y", "8Y", "9Y", "10Y" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }

            }
        }

        [TestMethod]
        public void GetInflationSwapInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "ZCCPISwap" };

            string[] currencies = { "AUD", "GBP" };

            string[] maturities = { "1Y", "2Y", "3Y", "4Y", "5Y", "6Y", "7Y", "8Y", "9Y", "10Y" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }

            }
        }

        [TestMethod]
        public void GetDepositInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Deposit", "ZeroRate", "Xibor", "OIS" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1D", "1W", "1M", "3M", "6M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("Name : {0} MarketQuote : {1}", ccy + '-' + instrument + '-' + maturity, result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetInflationIndexInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "CPIndex" };

            string[] currencies = { "AUD", "GBP" };

            string[] maturities = { "1D", "1W", "1M", "3M", "6M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }

            }
        }

        [TestMethod]
        public void GetFraInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Fra", "SpreadFra" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetBillFraInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "BillFra" };

            string[] currencies = { "AUD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetRateOptionInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Caplet", "Floorlet" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);
            //nvs.Set("Strike", "0.07");

            var bq1 = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bq2 = BasicQuotationHelper.Create(0.15m, "Volatility", "DecimalRate");
            var bqs = new BasicQuotation[2];
            bqs[0] = bq1;
            bqs[1] = bq2;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M" + '-' + "0.07";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("Strike", "0.07");
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetAUDBillRateOptionInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "BillCaplet", "BillFloorlet" };

            string[] currencies = { "AUD", "NZD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);
            //nvs.Set("Strike", "0.07");

            var bq1 = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bq2 = BasicQuotationHelper.Create(0.15m, "Volatility", "DecimalRate");
            var bqs = new BasicQuotation[2];
            bqs[0] = bq1;
            bqs[1] = bq2;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M" + '-' + "0.07";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("Strike", "0.07");
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetFuturesInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] currencies = { "AUD", "AUD", "USD", "GBP", "EUR", "JPY", "CHF", "SEK", "HKD", "NZD" };

            string[] codes = { "IR", "IB", "ED", "L", "ER", "EY", "ES", "RA", "HR", "ZB" };

            string[] maturities = { "Z9", "H0", "M0", "U0", "Z0", "H1", "M1", "U1", "Z1", "H2" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq1 = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bq2 = BasicQuotationHelper.Create(0.15m, "Volatility", "DecimalRate");
            var bqs = new BasicQuotation[2];
            bqs[0] = bq1;
            bqs[1] = bq2;
            var bav = BasicAssetValuationHelper.Create(bqs);

            var index = 0;

            foreach (var code in codes)
            {
                foreach (var maturity in maturities)
                {
                    string assetId = currencies[index] + "-IRFuture-" + code + '-' + maturity;
                    var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                    //nvs.Set("Strike", "0.07");
                    //nvs.Set("AssetType", "IRFuture");
                    //nvs.Set("Currency", currencies[index]);
                    //nvs.Set("ExtraItem", code);
                    //nvs.Set("ExpiryCode", maturity);
                    //nvs.Set("AssetId", currencies[index] + "-IRFuture-" + code + '-' + maturity);
                    var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                    Debug.Print("Name: {0} MarketQuote : {1}", currencies[index] + "-IRFuture-" + code + '-' + maturity, result.MarketQuote.value);
                }
                index++;
            }
        }

        [TestMethod]
        public void GetZeroRateInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "ZeroRate" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetOISInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "OIS" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetXiborInstrumentTests()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Xibor" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CurveEngine.CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        #endregion

        #region Instrument tests With Calendars

        [TestMethod]
        public void CreateSimpleFraTests2()
        {
            const string assetId = "AUD-Fra-6M-3M";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);
            var result = CreatePriceableAsset(bav, nvs);
            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void CreateBillFraTests2()
        {
            const string assetId = "AUD-BillFra-6M-3M";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);
            var result = CreatePriceableAsset(bav, nvs);
            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void CreateDepositTest2()
        {
            const string assetId = "AUD-Deposit-3M";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);

            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);
            //nvs.Set("AssetType", "Deposit");
            //nvs.Set("AssetId", "AUD-Deposit-3M");
            //nvs.Set("Currency", "AUD");
            //nvs.Set("ExtraItem", "3M");

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            var result = CreatePriceableAsset(bav, nvs);

            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void CreateIRSwapTest2()
        {
            const string assetId = "AUD-IRSwap-3Y";
            var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);

            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);
            //nvs.Set("AssetType", "IRSwap");
            //nvs.Set("AssetId", "AUD-IRSwap-3Y");
            //nvs.Set("Currency", "AUD");
            //nvs.Set("ExtraItem", "3Y");

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            var result = CreatePriceableAsset(bav, nvs);

            Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
        }

        [TestMethod]
        public void GetSwapInstrumentTests2()
        {
            string[] instruments1 = { "IRSwap" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK" };

            string[] maturities = { "1Y", "2Y", "3Y", "4Y", "5Y", "6Y", "7Y", "8Y", "9Y", "10Y" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }

            }
        }

        [TestMethod]
        public void GetInflationSwapInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "ZCCPISwap" };

            string[] currencies = { "AUD", "GBP" };

            string[] maturities = { "1Y", "2Y", "3Y", "4Y", "5Y", "6Y", "7Y", "8Y", "9Y", "10Y" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }

            }
        }

        [TestMethod]
        public void GetDepositInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Deposit", "ZeroRate", "Xibor", "OIS" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1D", "1W", "1M", "3M", "6M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("Name : {0} MarketQuote : {1}", ccy + '-' + instrument + '-' + maturity, result.MarketQuote.value);
                    }
                }

            }
        }

        [TestMethod]
        public void GetInflationIndexInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "CPIndex" };

            string[] currencies = { "AUD", "GBP" };

            string[] maturities = { "1D", "1W", "1M", "3M", "6M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }

            }
        }

        [TestMethod]
        public void GetFraInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Fra", "SpreadFra" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetBillFraInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "BillFra" };

            string[] currencies = { "AUD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetRateOptionInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Caplet", "Floorlet" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);
            //nvs.Set("Strike", "0.07");

            var bq1 = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bq2 = BasicQuotationHelper.Create(0.15m, "Volatility", "DecimalRate");
            var bqs = new BasicQuotation[2];
            bqs[0] = bq1;
            bqs[1] = bq2;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M" + '-' + "0.07";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("Strike", "0.07");
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetAUDBillRateOptionInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "BillCaplet", "BillFloorlet" };

            string[] currencies = { "AUD", "NZD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);
            //nvs.Set("Strike", "0.07");

            var bq1 = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bq2 = BasicQuotationHelper.Create(0.15m, "Volatility", "DecimalRate");
            var bqs = new BasicQuotation[2];
            bqs[0] = bq1;
            bqs[1] = bq2;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity + '-' + "3M" + '-' + "0.07";
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("Strike", "0.07");
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("StartTerm", maturity);
                        //nvs.Set("ExtraItem", "3M");
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity + '-' + "3M");
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetFuturesInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] currencies = { "AUD", "AUD", "USD", "GBP", "EUR", "JPY", "CHF", "SEK", "HKD", "NZD" };

            string[] codes = { "IR", "IB", "ED", "L", "ER", "EY", "ES", "RA", "HR", "ZB" };

            string[] maturities = { "Z9", "H0", "M0", "U0", "Z0", "H1", "M1", "U1", "Z1", "H2" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq1 = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bq2 = BasicQuotationHelper.Create(0.15m, "Volatility", "DecimalRate");
            var bqs = new BasicQuotation[2];
            bqs[0] = bq1;
            bqs[1] = bq2;
            var bav = BasicAssetValuationHelper.Create(bqs);

            var index = 0;

            foreach (var code in codes)
            {
                foreach (var maturity in maturities)
                {
                    string assetId = currencies[index] + "-IRFuture-" + code + '-' + maturity;
                    var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                    //nvs.Set("Strike", "0.07");
                    //nvs.Set("AssetType", "IRFuture");
                    //nvs.Set("Currency", currencies[index]);
                    //nvs.Set("ExtraItem", code);
                    //nvs.Set("ExpiryCode", maturity);
                    //nvs.Set("AssetId", currencies[index] + "-IRFuture-" + code + '-' + maturity);
                    var result = CreatePriceableAsset(bav, nvs);
                    Debug.Print("Name: {0} MarketQuote : {1}", currencies[index] + "-IRFuture-" + code + '-' + maturity, result.MarketQuote.value);
                }
                index++;
            }
        }

        [TestMethod]
        public void GetZeroRateInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "ZeroRate" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M", "25M", "50M", "66M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetOISInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "OIS" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        [TestMethod]
        public void GetXiborInstrumentTests2()
        {
            //var assetFactory = new PriceableAssetFactory();

            string[] instruments1 = { "Xibor" };

            string[] currencies = { "AUD", "USD", "GBP", "EUR", "NZD", "JPY", "CHF", "SEK", "HKD" };

            string[] maturities = { "1M", "3M", "6M", "12M" };


            //var nvs = new NamedValueSet();
            //nvs.Set(CurveProp.CurveName, "Test");
            //nvs.Set(CurveProp.BaseDate, _baseDate);

            var bq = BasicQuotationHelper.Create(0.07m, "MarketQuote", "DecimalRate");
            var bqs = new BasicQuotation[1];
            bqs[0] = bq;
            var bav = BasicAssetValuationHelper.Create(bqs);

            foreach (var ccy in currencies)
            {
                foreach (var maturity in maturities)
                {
                    foreach (var instrument in instruments1)
                    {
                        string assetId = ccy + '-' + instrument + '-' + maturity;
                        var nvs = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, assetId, BaseDate);
                        //nvs.Set("AssetType", instrument);
                        //nvs.Set("Currency", ccy);
                        //nvs.Set("ExtraItem", maturity);
                        //nvs.Set("AssetId", ccy + '-' + instrument + '-' + maturity);
                        var result = CreatePriceableAsset(bav, nvs);
                        Debug.Print("MarketQuote : {0}", result.MarketQuote.value);
                    }
                }
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Get the rates or vols out of the resource file
        /// </summary>
        internal static double[] GetRates(string volsList)
        {
            string[] vols = volsList.Split(';');
            double[] theVols = GetInputs<double>(vols);
            return theVols;
        }

        /// <summary>
        /// Get the dates out of resource file and return that
        /// in the format of DateTime
        /// </summary>
        internal static DateTime[] GetDates(string rollDatesList)
        {
            string[] rollDates = rollDatesList.Split(';');
            DateTime[] dates = GetInputs<DateTime>(rollDates);
            return dates;
        }

        /// <summary>
        /// Convert from string to different types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData"></param>
        private static T[] GetInputs<T>(IEnumerable<string> inputData)
        {
            var outputData = new List<T>();
            foreach (string data in inputData)
            {
                if (!string.IsNullOrEmpty(data))
                {
                    try
                    {
                        outputData.Add((T)Convert.ChangeType(data, typeof(T)));
                    }
                    catch (Exception ex)
                    {
                        throw new FormatException(string.Format("Conversion failed for '{0}'", data), ex);
                    }
                }
            }
            return outputData.ToArray();
        }


        /// <summary>
        /// Gets the executing class.
        /// </summary>
        /// <returns></returns>
        static internal string GetExecutingClass()
        {
            var st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            return sf.GetMethod().DeclaringType.Name;
        }

        private static void CreateAsset(string id)
        {
            DateTime baseDate = DateTime.Today;
            decimal[] values = { 0.0285m };
            decimal[] additionals = { 0m };
            var ids = new[] { id };
            const double maxTimeAllowed = 1;
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var assets = CurveEngine.CreatePriceableRateAssets(baseDate, ids, values, FixingCalendar, PaymentCalendar);
            stopwatch.Stop();

            Assert.IsNotNull(assets.Single());
            Debug.Print("ID: {0}, Time (s):{1}", id, stopwatch.Elapsed.TotalSeconds);
            Assert.IsTrue(stopwatch.Elapsed.TotalSeconds < maxTimeAllowed);
        }

        public IPriceableAssetController CreatePriceableAsset(BasicAssetValuation bav, NamedValueSet nvs)
        {
            var result = CurveEngine.CreatePriceableAsset(bav, nvs, FixingCalendar, PaymentCalendar);
            return result;
        }

        public List<IPriceableRateAssetController> CreatePriceableRateAssets(DateTime date, string[] ids, decimal[] vals)
        {
            var result = CurveEngine.CreatePriceableRateAssets(date, ids, vals, FixingCalendar, PaymentCalendar);
            return result;
        }

        public IPriceableAssetController CreatePriceableSurface(SimpleFra cap, decimal? notional, decimal strike, DateTime date, BasicQuotation quote)
        {
            var priceableAsset =
                        (IPriceableRateOptionAssetController)CurveEngine.CreatePriceableSurfaceAsset(cap, notional, strike, date, quote, FixingCalendar, PaymentCalendar);
            return priceableAsset;
        }

        static public IAssetControllerData CreateModelData(string[] metrics, DateTime baseDate, IMarketEnvironment market)
        {
            var bav = new BasicAssetValuation();
            var quotes = new BasicQuotation[metrics.Length];
            var index = 0;
            foreach (var metric in metrics)
            {
                quotes[index] = BasicQuotationHelper.Create(0.0m, metric);
                index++;
            }
            bav.quote = quotes;
            return new AssetControllerData(bav, baseDate, market);
        }

        static public ISimpleRateMarketEnvironment CreateSimpleRateMarketEnvironment(DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new SimpleRateMarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LogLinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure("DiscountCurve", curve);
            market.PricingStructureIdentifier = "DiscountCurve";
            return market;
        }

        static public IMarketEnvironment CreateDFandVolCurves(string curveName, string volcurveName, DateTime baseDate, Double[] times, Double[] dfs, Double[] voltimes, Double[] volstrikes, Double[,] vols)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            var volcurve = new SimpleVolatilitySurface(baseDate, interpMethod, true, voltimes, volstrikes, vols);
            market.AddPricingStructure(curveName, curve);
            market.AddPricingStructure(volcurveName, volcurve);
            return market;
        }

        static public IMarketEnvironment CreateSimpleFxCurve(string curveName, DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleFxCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure(curveName, curve);
            return market;
        }

        static public IMarketEnvironment CreateSimpleCommodityCurve(string curveName, DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleCommodityCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure(curveName, curve);
            return market;
        }

        static public IMarketEnvironment CreateSimpleInflationCurve(string curveName, DateTime baseDate, Double[] times, Double[] dfs)
        {
            var market = new MarketEnvironment();
            var interpMethod = InterpolationMethodHelper.Parse("LinearInterpolation");
            var curve = new SimpleDiscountFactorCurve(baseDate, interpMethod, true, times, dfs);
            market.AddPricingStructure(curveName, curve);
            return market;
        }

        static public void ProcessAssetControllerResults(IPriceableAssetController assetController, string[] metrics, DateTime baseDate)
        {
            Assert.IsNotNull(assetController);

            Double[] times = { 0, 1, 5 };
            Double[] dfs = { 1, 0.9, 0.3 };

            ISimpleRateMarketEnvironment market = CreateSimpleRateMarketEnvironment(baseDate, times, dfs);
            IAssetControllerData controllerData = CreateModelData(metrics, baseDate, market);
            Assert.IsNotNull(controllerData);
            Assert.AreEqual(controllerData.BasicAssetValuation.quote.Length, metrics.Length);
            Assert.AreEqual(controllerData.BasicAssetValuation.quote[0].measureType.Value, metrics[0]);
            BasicAssetValuation results = assetController.Calculate(controllerData);
            Debug.Print("Id : {0}", assetController.Id);
            foreach (var metric in results.quote)
            {
                Debug.Print("Id : {0} Metric Name : {1} Metric Value : {2}", assetController.Id, metric.measureType.Value, metric.value);
            }
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateBankBill(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateBankBillWithNotional(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateDeposit(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateDepositWithNotional(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateZeroRate(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateZeroRateWithNotional(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

         /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableCommodityAssetController> CreateCommodityForward(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableCommodityAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableCommodityAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableCommodityAssetController> CreateCommodityFuture(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableCommodityAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableCommodityAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableCommodityAssetController> CreateCommoditySpot(string curveName1,
                                                                                    DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableCommodityAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableCommodityAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateEDFuture(string curveName, DateTime date, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            var  controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateEDFutureWithPosition(string curveName,
                                                                                      DateTime date, BasicQuotation[] quotations, BasicQuotation[] volatilities, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateERFuture(string curveName, DateTime date, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateESFuture(string curveName, DateTime date, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateESFutureWithPosition(string curveName,
                                                                                      DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateEYFuture(string curveName, DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateEYFutureWithPosition(string curveName,
                                                                                      DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateHRFuture(string curveName, DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateHRFutureWithPosition(string curveName,
                                                                                      DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateIBFuture(string curveName, DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            List<IPriceableRateAssetController> controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateIRFuture(string curveName, DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            List<IPriceableRateAssetController> controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateEDFuture(DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateRAFuture(string curveName, DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateRAFutureWithPosition(string curveName,
                                                                                      DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateZBFuture(string curveName, DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateZBFutureWithPosition(string curveName,
                                                                                      DateTime baseDate, BasicQuotation[] quotations, BasicQuotation[] volatilities, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index], volatilities[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

         /// <summary>
         /// Creates the deposit.
         /// </summary>
         /// <returns></returns>
         private IEnumerable<IPriceableFxAssetController> CreateFxForward(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers)
         {
             var controllers = new List<IPriceableFxAssetController>();
             var index = 0;
             foreach (var identifier in identifiers)
             {
                 var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                 var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                 var priceableAsset = (IPriceableFxAssetController)CreatePriceableAsset(bav, properties);
                 controllers.Add(priceableAsset);
                 index++;
             }
             return controllers;
         }

         /// <summary>
         /// Creates the deposit.
         /// </summary>
         /// <returns></returns>
         private List<IPriceableFxAssetController> CreateFxSpot(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers)
         {
             var controllers = new List<IPriceableFxAssetController>();
             var index = 0;
             foreach (var identifier in identifiers)
             {
                 var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                 var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                 var priceableAsset = (IPriceableFxAssetController)CreatePriceableAsset(bav, properties);
                 controllers.Add(priceableAsset);
                 index++;
             }
             return controllers;
         }

         /// <summary>
         /// Creates the deposit.
         /// </summary>
         /// <returns></returns>
         private List<IPriceableRateAssetController> CreateCPIndex(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers)
         {
             var controllers = new List<IPriceableRateAssetController>();
             int index = 0;
             foreach (string identifier in identifiers)
             {
                 var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                 var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                 var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                 controllers.Add(priceableAsset);
                 index++;
             }
             return controllers;
         }

         /// <summary>
         /// Creates the deposit.
         /// </summary>
         /// <returns></returns>
         private List<IPriceableRateAssetController> CreateCPIndexWithNotional(DateTime baseDate,
                                                                                      BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
         {
             var controllers = new List<IPriceableRateAssetController>();
             int index = 0;
             foreach (string identifier in identifiers)
             {
                 var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                 var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                 var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                 controllers.Add(priceableAsset);
                 index++;
             }
             return controllers;
         }

         /// <summary>
         /// Creates the deposit.
         /// </summary>
         /// <returns></returns>
         private List<IPriceableInflationAssetController> CreateXibor(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers)
         {
             var controllers = new List<IPriceableInflationAssetController>();
             int index = 0;
             foreach (string identifier in identifiers)
             {
                 var bav = new BasicAssetValuation { quote = new[] { quotations[index] } };
                 var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                 var priceableAsset = (IPriceableInflationAssetController)CreatePriceableAsset(bav, properties);
                 controllers.Add(priceableAsset);
                 index++;
             }
             return controllers;
         }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
       private List<IPriceableRateAssetController> CreateSimpleCPISwap(
            DateTime baseDate, BasicQuotation[] quotations, 
            string[] identifiers)
        {
            List<IPriceableRateAssetController> controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate
                    );
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateSimpleZCCPISwap(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableInflationAssetController> CreateSimpleZCCPISwapAsInflation(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers)
        {
            var controllers = new List<IPriceableInflationAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableInflationAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableInflationAssetController> CreateSimpleZCCPISwapAsInflationWithNotional(string curveName,
                                                                                                             DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableInflationAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableInflationAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        internal List<IPriceableRateAssetController> CreateSimpleBasisSwap(string curveName,
                                                                               DateTime baseDate, BasicQuotation[] quotations, string[] identifiers)
        {
            List<IPriceableRateAssetController> controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                NamedValueSet properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                BasicAssetValuation bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                PriceableBasisSwap priceableAsset = (PriceableBasisSwap)CreatePriceableAsset(bav, properties);
                priceableAsset.CurveName = curveName;
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateBasiswapWithNotional(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            List<IPriceableRateAssetController> controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                NamedValueSet properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                BasicAssetValuation bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                IPriceableRateAssetController priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the simple fra.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns></returns>
        private List<IPriceableSpreadAssetController2> CreateSimpleSpreadFra(DateTime baseDate, BasicQuotation[] quotations, string[] identifiers)
        {
            var controllers = new List<IPriceableSpreadAssetController2>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableSpreadAssetController2)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        internal List<IPriceableRateAssetController> CreateSimpleXccyBasisSwap(string curveName,
                                                                               DateTime baseDate, BasicQuotation[] quotations, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                NamedValueSet properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                BasicAssetValuation bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (PriceableXccyBasisSwap)CreatePriceableAsset(bav, properties);
                priceableAsset.CurveName = curveName;
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateSimpleXccySwapWithNotional(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            List<IPriceableRateAssetController> controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                NamedValueSet properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                BasicAssetValuation bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                IPriceableRateAssetController priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
         private List<IPriceableRateAssetController> CreateXibor(string curveName, DateTime baseDate, BasicQuotation[] quotations, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateXiborWithNotional(string curveName,
                                                                                   DateTime baseDate, BasicQuotation[] quotations, string[] identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        private string CreateCurve(MarketEnvironment marketEnvironment, DateTime baseDate)
        {
            object[,] structurePropertiesRange = 
                {
                    {"PricingStructureType","RateCurve"},
                    {"IndexTenor","3M"},
                    {"Currency","AUD"},
                    {"Index","LIBOR-BBA"},
                    {"Algorithm","FastLinearZero"},
                    {"MarketName",marketEnvironment.Id},
                    {"BaseDate",baseDate},
                };
            NamedValueSet properties = new NamedValueSet(structurePropertiesRange);

            string[] instruments = 
                {   "AUD-Deposit-1D",
                    "AUD-Deposit-1M",
                    "AUD-Deposit-2M",
                    "AUD-Deposit-3M",
                    "AUD-IRFuture-IR-Z9",
                    "AUD-IRFuture-IR-H0",
                    "AUD-IRFuture-IR-M0",
                    "AUD-IRFuture-IR-U0",
                    "AUD-IRFuture-IR-Z0",
                    "AUD-IRFuture-IR-H1",
                    "AUD-IRFuture-IR-M1",
                    "AUD-IRFuture-IR-U1",
                    "AUD-IRSwap-3Y",
                    "AUD-IRSwap-4Y",
                    "AUD-IRSwap-5Y",
                    "AUD-IRSwap-7Y",
                    "AUD-IRSwap-10Y",
                    "AUD-IRSwap-12Y",
                    "AUD-IRSwap-15Y",
                    "AUD-IRSwap-20Y",
                    "AUD-IRSwap-25Y",
                    "AUD-IRSwap-30Y",
                };
            List<decimal> values = new List<decimal>();
            foreach (string instrument in instruments)
            {
                values.Add(0.03m);
            }
            var rateCurve = CurveEngine.CreateCurve(properties, instruments, values.ToArray(), null, FixingCalendar, PaymentCalendar);
            //ObjectCacheHelper.SetPricingStructureAsSerialisable(rateCurve);
            string id = rateCurve.GetPricingStructureId().UniqueIdentifier;
            marketEnvironment.AddPricingStructure(id, rateCurve);
            return id;
        }

        /// <summary>
        /// Creates the swap.
        /// </summary>
        /// <returns></returns>
        private void GetIRCapPDH(RateCurve curve, IVolatilitySurface volcurve, DateTime baseDate)
        {
            var businessDayAdjustments = BusinessDayAdjustmentsHelper.Create(BusinessDays,
                                                                Hols);
            //Create a dummy rate.
            var bav = new BasicAssetValuation();
            var quote = BasicQuotationHelper.Create(Rate, "MarketQuote", "DecimalRate");
            var spreadquote = BasicQuotationHelper.Create(Spread, "Spread", "DecimalRate");
            bav.quote = new[] { quote, spreadquote };
            var dates = new List<DateTime>();
            foreach (var date in _dates)
            {
                dates.Add(Convert.ToDateTime(date));
            }
            //create the swap.
            //TODO check the volproperties as the interface has changed.
            var cap = CurveEngine.CreateIRCap("Local", baseDate, Currency, dates,
                                                                    new List<double>(_notionals), new List<double>(_tradestrikes), null, null,
                                                                    businessDayAdjustments, Dc, null, FixingCalendar, _volproperties);
            var valuation = cap.CalculateRatePDH(baseDate, curve, curve, volcurve, CurvePerturbation.ForecastCurve);
            var index = 0;
            foreach (var key in valuation.Keys)
            {
                Assert.AreEqual(valuation[key], (double)_result[index], 0.001d);
                index++;
            }
        }

        private static double[,] GenerateVols(double vol)
        {
            var result = new double[12, 9];
            for (int i = 0; i < 12; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    result[i, j] = vol;
                }
            }
            return result;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateOis(string curveName, DateTime baseDate, BasicQuotation[] quotations, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateOisWithNotional(string curveName,
                                                                                 DateTime baseDate, BasicQuotation[] quotations, string[] identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            var index = 0;
            foreach (var identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateSimpleXccySwap(string curveName,
                                                                              DateTime baseDate, BasicQuotation[] quotations, string[] identifiers)
        {
            var controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the deposit.
        /// </summary>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateSimpleXccySwapWithNotional(string curveName,
                                                                                          DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        //BMK Helpers

        #endregion

        #endregion

        #region PriceableAsset Tests

        #region General AssetTimer Tests

        [TestMethod]
        public void CreatePriceableRateAssetsTest()
        {
            // Now test assets
            CreateAsset("AUD-Deposit-1D");
            CreateAsset("AUD-Deposit-3M");
            CreateAsset("AUD-IRFuture-IR-1");
            CreateAsset("AUD-IRFuture-IR-2");
            CreateAsset("AUD-IRSwap-3Y");
            CreateAsset("AUD-IRSwap-30Y");

            //DataCollection.StopProfile(ProfileLevel.Global, DataCollection.CurrentId);
        }

        #endregion

        #region Bond Asset Tests

        [TestMethod]
        public void CreateBond()
        {
            var bond = BondHelper.Parse("AUD-GOVT-Bond", "ASX", "Fixed", 8.0d, "AUD", 100.0d,
                                        new DateTime(2019, 4, 15), "6M", "ACT/365.FIXED", "SENIOR", "ISIN0000001",
                                        "Aud Government Bond",
                                        "ASX", "AUDGOVTBOND");
            var result = XmlSerializerHelper.SerializeToString(bond);
            Debug.Print(result);
        }

        [TestMethod]
        public void TestCreateBonds()
        {

            var date = new DateTime(2008, 2, 20);
            var instruments = new[] { 
                                        "AUD-Bond-AGB-.08-25/09/2020", 
                                        "EUR-Bond-BOBL-08-25/09/2020", 
                                        "GBP-Bond-Gilt-08-25/09/2020", 
                                        "USD-Bond-Treasury-08-25/09/2020"};

            var coupons = new[] { .07m, .07m, .07m, .07m };

            var dates = new[] { new DateTime(2010, 03, 03), new DateTime(2012, 03, 03), new DateTime(2014, 03, 03), new DateTime(2019, 03, 03) };

            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(100.0m, "MarketQuote", "DirtyPrice"), 
                                        BasicQuotationHelper.Create(100.0m, "MarketQuote", "DirtyPrice"), 
                                        BasicQuotationHelper.Create(100.0m, "MarketQuote", "DirtyPrice"), 
                                        BasicQuotationHelper.Create(100.0m, "MarketQuote", "DirtyPrice")
                                    };

            IEnumerable<IPriceableBondAssetController> priceableControllers = CreateSimpleBonds(date, bq, instruments, dates, coupons);

            foreach (IPriceableBondAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "MarketQuote" };
                ProcessAssetControllerResults(priceableController, metrics, date);
                metrics = new[] { "ImpliedQuote", "AssetSwapSpread", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, date);
            }
        }

        /// <summary>
        /// Creates the simple bond.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <param name="maturityDates">The maturity dates.</param>
        /// <param name="coupons">The coupons.</param>
        /// <returns></returns>
        static private IEnumerable<IPriceableBondAssetController> CreateSimpleBonds(DateTime baseDate,
                                                                                BasicQuotation[] quotations, IEnumerable<string> identifiers, DateTime[] maturityDates, decimal[] coupons)
        {
            var controllers = new List<IPriceableBondAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForBondAssets(UTE.NameSpace, identifier, baseDate, coupons[index], maturityDates[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableBondAssetController)CurveEngine.CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        #endregion

        #region Fra Tests

        [TestMethod]
        public void TestCreateAUDFastFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-Fra-1M-3M", 
                                                    "AUD-Fra-3M-3M", 
                                                    "AUD-Fra-6M-3M", 
                                                    "AUD-Fra-9M-3M", 
                                                    "AUD-Fra-12M-3M", 
                                                    "AUD-Fra-15M-3M", 
                                                    "AUD-Fra-18M-3M", 
                                                    "AUD-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                var curve = new SimpleDiscountFactorCurve(baseDate,
                                                            InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                            true, new List<DateTime>(dates), values);
                var dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam, impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                var dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2, impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id, dfam2 - dfam, impquote2 - impquote);
            }
        }

        [TestMethod]
        public void TestCreateAUDSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-Fra-1M-3M", 
                                                    "AUD-Fra-3M-3M", 
                                                    "AUD-Fra-6M-3M", 
                                                    "AUD-Fra-9M-3M", 
                                                    "AUD-Fra-12M-3M", 
                                                    "AUD-Fra-15M-3M", 
                                                    "AUD-Fra-18M-3M", 
                                                    "AUD-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDSimpleFraWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-Fra-1M-3M", 
                                                    "AUD-Fra-3M-3M", 
                                                    "AUD-Fra-6M-3M", 
                                                    "AUD-Fra-9M-3M", 
                                                    "AUD-Fra-12M-3M", 
                                                    "AUD-Fra-15M-3M", 
                                                    "AUD-Fra-18M-3M", 
                                                    "AUD-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            IEnumerable<IPriceableRateAssetController> priceableControllers = CreateSimpleFraWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateUSDSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "USD-Fra-1M-3M", 
                                                    "USD-Fra-3M-3M", 
                                                    "USD-Fra-6M-3M", 
                                                    "USD-Fra-9M-3M", 
                                                    "USD-Fra-12M-3M", 
                                                    "USD-Fra-15M-3M", 
                                                    "USD-Fra-18M-3M", 
                                                    "USD-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateEURSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "EUR-Fra-1M-3M", 
                                                    "EUR-Fra-3M-3M", 
                                                    "EUR-Fra-6M-3M", 
                                                    "EUR-Fra-9M-3M", 
                                                    "EUR-Fra-12M-3M", 
                                                    "EUR-Fra-15M-3M", 
                                                    "EUR-Fra-18M-3M", 
                                                    "EUR-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateGBPSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "GBP-Fra-1M-3M", 
                                                    "GBP-Fra-3M-3M", 
                                                    "GBP-Fra-6M-3M", 
                                                    "GBP-Fra-9M-3M", 
                                                    "GBP-Fra-12M-3M", 
                                                    "GBP-Fra-15M-3M", 
                                                    "GBP-Fra-18M-3M", 
                                                    "GBP-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateNZDSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "NZD-Fra-1M-3M", 
                                                    "NZD-Fra-3M-3M", 
                                                    "NZD-Fra-6M-3M", 
                                                    "NZD-Fra-9M-3M", 
                                                    "NZD-Fra-12M-3M", 
                                                    "NZD-Fra-15M-3M", 
                                                    "NZD-Fra-18M-3M", 
                                                    "NZD-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateJPYSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "JPY-Fra-1M-3M", 
                                                    "JPY-Fra-3M-3M", 
                                                    "JPY-Fra-6M-3M", 
                                                    "JPY-Fra-9M-3M", 
                                                    "JPY-Fra-12M-3M", 
                                                    "JPY-Fra-15M-3M", 
                                                    "JPY-Fra-18M-3M", 
                                                    "JPY-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateCHFSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "CHF-Fra-1M-3M", 
                                                    "CHF-Fra-3M-3M", 
                                                    "CHF-Fra-6M-3M", 
                                                    "CHF-Fra-9M-3M", 
                                                    "CHF-Fra-12M-3M", 
                                                    "CHF-Fra-15M-3M", 
                                                    "CHF-Fra-18M-3M", 
                                                    "CHF-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateSEKSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "SEK-Fra-1M-3M", 
                                                    "SEK-Fra-3M-3M", 
                                                    "SEK-Fra-6M-3M", 
                                                    "SEK-Fra-9M-3M", 
                                                    "SEK-Fra-12M-3M", 
                                                    "SEK-Fra-15M-3M", 
                                                    "SEK-Fra-18M-3M", 
                                                    "SEK-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateHKDSimpleFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "HKD-Fra-1M-3M", 
                                                    "HKD-Fra-3M-3M", 
                                                    "HKD-Fra-6M-3M", 
                                                    "HKD-Fra-9M-3M", 
                                                    "HKD-Fra-12M-3M", 
                                                    "HKD-Fra-15M-3M", 
                                                    "HKD-Fra-18M-3M", 
                                                    "HKD-Fra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDDiscountFra()
        {
            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-BillFra-1M-3M", 
                                                    "AUD-BillFra-3M-3M", 
                                                    "AUD-BillFra-6M-3M", 
                                                    "AUD-BillFra-9M-3M", 
                                                    "AUD-BillFra-12M-3M", 
                                                    "AUD-BillFra-15M-3M", 
                                                    "AUD-BillFra-18M-3M", 
                                                    "AUD-BillFra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                var curve = new SimpleDiscountFactorCurve(baseDate,
                                                            InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                            true, new List<DateTime>(dates), values);
                var dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam, impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                var dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2, impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id, dfam2 - dfam, impquote2 - impquote);

            }
        }

        [TestMethod]
        public void TestCreateSimpleDiscountFra()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-BillFra-1M-3M", 
                                                    "AUD-BillFra-3M-3M", 
                                                    "AUD-BillFra-6M-3M", 
                                                    "AUD-BillFra-9M-3M", 
                                                    "AUD-BillFra-12M-3M", 
                                                    "AUD-BillFra-15M-3M", 
                                                    "AUD-BillFra-18M-3M", 
                                                    "AUD-BillFra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleFra(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateSimpleDiscountFraWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-BillFra-1M-3M", 
                                                    "AUD-BillFra-3M-3M", 
                                                    "AUD-BillFra-6M-3M", 
                                                    "AUD-BillFra-9M-3M", 
                                                    "AUD-BillFra-12M-3M", 
                                                    "AUD-BillFra-15M-3M", 
                                                    "AUD-BillFra-18M-3M", 
                                                    "AUD-BillFra-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            IEnumerable<IPriceableRateAssetController> priceableControllers = CreateSimpleFraWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        /// <summary>
        /// Creates the simple fra.
        /// </summary>
        /// <param name="curveName">Name of the curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns></returns>
        private List<IPriceableRateAssetController> CreateSimpleFra(DateTime baseDate, BasicQuotation[] quotations, string[] identifiers)
        {
            List<IPriceableRateAssetController> controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the simple fra.
        /// </summary>
        /// <param name="curveName">Name of the curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns></returns>
        private IEnumerable<IPriceableRateAssetController> CreateSimpleFraWithNotional(DateTime baseDate, BasicQuotation[] quotations, string[] identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, baseDate, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        #endregion

        #region Caplet Tests

        [TestMethod]
        public void PriceableAUDCapletImpliedVol()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new[] { 
                                                    "AUD-Caplet-1M-3M-0.1067"};
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.002993m, "MarketQuote", "Premium")};

            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateCaplet(baseDate, bq, instruments);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                var metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDCaplet()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-Caplet-1M-3M-0.07", 
                                                    "AUD-Caplet-3M-3M-0.07", 
                                                    "AUD-Caplet-6M-3M-0.07", 
                                                    "AUD-Caplet-9M-3M-0.07", 
                                                    "AUD-Caplet-12M-3M-0.07", 
                                                    "AUD-Caplet-15M-3M-0.07", 
                                                    "AUD-Caplet-18M-3M-0.07", 
                                                    "AUD-Caplet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };

            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateCaplet(baseDate, bq, instruments);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDCapletWithStrike()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var strikes = new[] { 0.07m, 0.07m, 0.07m, 0.07m, 0.07m, 0.07m, 0.07m, 0.07m };
            var instruments = new[] { 
                                        "AUD-Caplet-1M-3M", 
                                        "AUD-Caplet-3M-3M", 
                                        "AUD-Caplet-6M-3M", 
                                        "AUD-Caplet-9M-3M", 
                                        "AUD-Caplet-12M-3M", 
                                        "AUD-Caplet-15M-3M", 
                                        "AUD-Caplet-18M-3M", 
                                        "AUD-Caplet-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };

            List<IPriceableRateOptionAssetController> priceableControllers = CreateCaplet(baseDate, bq, instruments, strikes);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDCapletWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-Caplet-1M-3M-0.07", 
                                                    "AUD-Caplet-3M-3M-0.07", 
                                                    "AUD-Caplet-6M-3M-0.07", 
                                                    "AUD-Caplet-9M-3M-0.07", 
                                                    "AUD-Caplet-12M-3M-0.07", 
                                                    "AUD-Caplet-15M-3M-0.07", 
                                                    "AUD-Caplet-18M-3M-0.07", 
                                                    "AUD-Caplet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateSimpleCapletWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDCapletWithStrikeandNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var strikes = new[] { 0.07m, 0.07m, 0.07m, 0.07m, 0.07m, 0.07m, 0.07m, 0.07m };
            var instruments = new[] { 
                                        "AUD-Caplet-1M-3M", 
                                        "AUD-Caplet-3M-3M", 
                                        "AUD-Caplet-6M-3M", 
                                        "AUD-Caplet-9M-3M", 
                                        "AUD-Caplet-12M-3M", 
                                        "AUD-Caplet-15M-3M", 
                                        "AUD-Caplet-18M-3M", 
                                        "AUD-Caplet-21M-3M" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };

            List<IPriceableRateOptionAssetController> priceableControllers = CreateSimpleCapletWithNotional(baseDate, bq, instruments, notional, strikes);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDDiscountCaplet()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-BillCaplet-1M-3M-0.07", 
                                                    "AUD-BillCaplet-3M-3M-0.07", 
                                                    "AUD-BillCaplet-6M-3M-0.07", 
                                                    "AUD-BillCaplet-9M-3M-0.07", 
                                                    "AUD-BillCaplet-12M-3M-0.07", 
                                                    "AUD-BillCaplet-15M-3M-0.07", 
                                                    "AUD-BillCaplet-18M-3M-0.07", 
                                                    "AUD-BillCaplet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };

            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateCaplet(baseDate, bq, instruments);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDDiscountCapletWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-BillCaplet-1M-3M-0.07", 
                                                    "AUD-BillCaplet-3M-3M-0.07", 
                                                    "AUD-BillCaplet-6M-3M-0.07", 
                                                    "AUD-BillCaplet-9M-3M-0.07", 
                                                    "AUD-BillCaplet-12M-3M-0.07", 
                                                    "AUD-BillCaplet-15M-3M-0.07", 
                                                    "AUD-BillCaplet-18M-3M-0.07", 
                                                    "AUD-BillCaplet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateSimpleCapletWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        /// <summary>
        /// Creates the simple fra.
        /// </summary>
        /// <param name="date">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns></returns>
        private IEnumerable<IPriceableRateOptionAssetController> CreateCaplet(DateTime date, BasicQuotation[] quotations, string[] identifiers)
        {
            var controllers = new List<IPriceableRateOptionAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateOptionAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the simple fra.
        /// </summary>
        /// <param name="date">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns></returns>
        private IEnumerable<IPriceableRateOptionAssetController> CreateSimpleCapletWithNotional(DateTime date, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts)
        {
            var controllers = new List<IPriceableRateOptionAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                var properties = PriceableAssetFactory.BuildPropertiesForAssets(UTE.NameSpace, identifier, date, amounts[index]);
                var bav = BasicAssetValuationHelper.Create(new[] { quotations[index] });
                var priceableAsset = (IPriceableRateOptionAssetController)CreatePriceableAsset(bav, properties);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the simple fra.
        /// </summary>
        /// <param name="curveName">Name of the curve.</param>
        /// <param name="volcurveName">The vol curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns></returns>
        private List<IPriceableRateOptionAssetController> CreateCaplet(DateTime baseDate, BasicQuotation[] quotations, string[] identifiers, decimal[] strikes)
        {
            var controllers = new List<IPriceableRateOptionAssetController>();
            int index = 0;
            foreach (var identifier in identifiers)
            {
                foreach (var strike in strikes)
                {
                    var fra = new SimpleFra
                    {
                        id = identifier,
                        currency = new IdentifiedCurrency() { Value = identifier.Split('-')[0] },
                        startTerm = PeriodHelper.Parse(identifier.Split('-')[2])
                    };
                    var indexTenor = identifier.Split('-')[3];
                    fra.endTerm = fra.startTerm.Sum(PeriodHelper.Parse(indexTenor));
                    var priceableAsset =
                        (IPriceableRateOptionAssetController)
                        CreatePriceableSurface(fra, null, strike, baseDate,
                                                                quotations[index]);
                    controllers.Add(priceableAsset);
                }
                index++;
            }
            return controllers;
        }

        /// <summary>
        /// Creates the simple fra.
        /// </summary>
        /// <param name="curveName">Name of the curve.</param>
        /// <param name="baseDate">The base date.</param>
        /// <param name="quotations">The quotations.</param>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns></returns>
        private List<IPriceableRateOptionAssetController> CreateSimpleCapletWithNotional(DateTime baseDate, BasicQuotation[] quotations, IEnumerable<string> identifiers, decimal[] amounts, decimal[] strikes)
        {
            List<IPriceableRateOptionAssetController> controllers = new List<IPriceableRateOptionAssetController>();
            int index = 0;
            foreach (string identifier in identifiers)
            {
                SimpleFra fra = new SimpleFra();
                fra.id = identifier;
                fra.currency = new IdentifiedCurrency() { Value = identifier.Split('-')[0] };
                fra.startTerm = PeriodHelper.Parse(identifier.Split('-')[2]);
                string indexTenor = identifier.Split('-')[3];
                fra.endTerm = fra.startTerm.Sum(PeriodHelper.Parse(indexTenor));
                IPriceableRateOptionAssetController priceableAsset = (IPriceableRateOptionAssetController)CreatePriceableSurface(fra, amounts[index], strikes[index], baseDate, quotations[index]);
                controllers.Add(priceableAsset);
                index++;
            }
            return controllers;
        }

        #endregion

        #region Floor Tests

        [TestMethod]
        public void TestCreateAUDFloorlet()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-Floorlet-1M-3M-0.07", 
                                                    "AUD-Floorlet-3M-3M-0.07", 
                                                    "AUD-Floorlet-6M-3M-0.07", 
                                                    "AUD-Floorlet-9M-3M-0.07", 
                                                    "AUD-Floorlet-12M-3M-0.07", 
                                                    "AUD-Floorlet-15M-3M-0.07", 
                                                    "AUD-Floorlet-18M-3M-0.07", 
                                                    "AUD-Floorlet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };

            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateCaplet(baseDate, bq, instruments);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDFloorletWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-Floorlet-1M-3M-0.07", 
                                                    "AUD-Floorlet-3M-3M-0.07", 
                                                    "AUD-Floorlet-6M-3M-0.07", 
                                                    "AUD-Floorlet-9M-3M-0.07", 
                                                    "AUD-Floorlet-12M-3M-0.07", 
                                                    "AUD-Floorlet-15M-3M-0.07", 
                                                    "AUD-Floorlet-18M-3M-0.07", 
                                                    "AUD-Floorlet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateSimpleCapletWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDDiscountFloorlet()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-BillFloorlet-1M-3M-0.07", 
                                                    "AUD-BillFloorlet-3M-3M-0.07", 
                                                    "AUD-BillFloorlet-6M-3M-0.07", 
                                                    "AUD-BillFloorlet-9M-3M-0.07", 
                                                    "AUD-BillFloorlet-12M-3M-0.07", 
                                                    "AUD-BillFloorlet-15M-3M-0.07", 
                                                    "AUD-BillFloorlet-18M-3M-0.07", 
                                                    "AUD-BillFloorlet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };
            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateCaplet(baseDate, bq, instruments);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDDiscountFloorletWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { 
                                                    "AUD-BillFloorlet-1M-3M-0.07", 
                                                    "AUD-BillFloorlet-3M-3M-0.07", 
                                                    "AUD-BillFloorlet-6M-3M-0.07", 
                                                    "AUD-BillFloorlet-9M-3M-0.07", 
                                                    "AUD-BillFloorlet-12M-3M-0.07", 
                                                    "AUD-BillFloorlet-15M-3M-0.07", 
                                                    "AUD-BillFloorlet-18M-3M-0.07", 
                                                    "AUD-BillFloorlet-21M-3M-0.07" };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"), 
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalVolatility")
                                    };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            IEnumerable<IPriceableRateOptionAssetController> priceableControllers = CreateSimpleCapletWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateOptionAssetController priceableController in priceableControllers)
            {
                string[] metrics = new string[1] { "VolatilityAtExpiry" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        #endregion

        #region Cash Tests

        [TestMethod]
        public void TestCreateFastBankBill()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[]
                                    {
                                        "AUD-BankBill-1D", "AUD-BankBill-1W",
                                        "AUD-BankBill-1M", "AUD-BankBill-3M", "AUD-BankBill-6M"
                                    };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };
            var priceableControllers = CreateBankBill(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                var curve = new SimpleDiscountFactorCurve(baseDate,
                                                            InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                            true, new List<DateTime>(dates), values);
                var dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam,
                            impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                var dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2,
                            impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id,
                            dfam2 - dfam, impquote2 - impquote);

            }
        }

        [TestMethod]
        public void TestCreateBankBill()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-BankBill-1D", "AUD-BankBill-1W", 
                                        "AUD-BankBill-1M", "AUD-BankBill-3M", "AUD-BankBill-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateBankBill(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);

                //TestHelper.SerializeResults(priceableController.Id, results);
            }

        }

        [TestMethod]
        public void TestCreateBankBillWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-BankBill-1D", "AUD-BankBill-1W", 
                                        "AUD-BankBill-1M", "AUD-BankBill-3M", "AUD-BankBill-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateBankBillWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);

                //TestHelper.SerializeResults(priceableController.Id, results);
            }
        }

        [TestMethod]
        public void PriceableAUDDepositsWithSpot()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[]
                                    {
                                        "AUD-Deposit-1D", "AUD-Deposit-TN", "AUD-Deposit-1M", "AUD-Deposit-3M",
                                        "AUD-Deposit-6M"
                                    };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateFastAUDDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[]
                                    {
                                        "AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Deposit-1M", "AUD-Deposit-3M",
                                        "AUD-Deposit-6M"
                                    };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Deposit-1M", "AUD-Deposit-3M", "AUD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateAUDDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-Deposit-1D", "AUD-Deposit-1W", "AUD-Deposit-1M", "AUD-Deposit-3M", "AUD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);
            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateUSDDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "USD-Deposit-1D", "USD-Deposit-1W", "USD-Deposit-1M", "USD-Deposit-3M", "USD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateUSDDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "USD-Deposit-1D", "USD-Deposit-1W", "USD-Deposit-1M", "USD-Deposit-3M", "USD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }


        [TestMethod]
        public void TestCreateEURDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "EUR-Deposit-1D", "EUR-Deposit-1W", "EUR-Deposit-1M", "EUR-Deposit-3M", "EUR-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateEURDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "EUR-Deposit-1D", "EUR-Deposit-1W", "EUR-Deposit-1M", "EUR-Deposit-3M", "EUR-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateGBPDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "GBP-Deposit-1D", "GBP-Deposit-1W", "GBP-Deposit-1M", "GBP-Deposit-3M", "GBP-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateGBPDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "GBP-Deposit-1D", "GBP-Deposit-1W", "GBP-Deposit-1M", "GBP-Deposit-3M", "GBP-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateCHFDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "CHF-Deposit-1D", "CHF-Deposit-1W", "CHF-Deposit-1M", "CHF-Deposit-3M", "CHF-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateCHFDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "CHF-Deposit-1D", "CHF-Deposit-1W", "CHF-Deposit-1M", "CHF-Deposit-3M", "CHF-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateSEKDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "SEK-Deposit-1D", "SEK-Deposit-1W", "SEK-Deposit-1M", "SEK-Deposit-3M", "SEK-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateSEKDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "SEK-Deposit-1D", "SEK-Deposit-1W", "SEK-Deposit-1M", "SEK-Deposit-3M", "SEK-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateHKDDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "HKD-Deposit-1D", "HKD-Deposit-1W", "HKD-Deposit-1M", "HKD-Deposit-3M", "HKD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateHKDDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "HKD-Deposit-1D", "HKD-Deposit-1W", "HKD-Deposit-1M", "HKD-Deposit-3M", "HKD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateNZDDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "NZD-Deposit-1D", "NZD-Deposit-1W", "NZD-Deposit-1M", "NZD-Deposit-3M", "NZD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateNZDDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "NZD-Deposit-1D", "NZD-Deposit-1W", "NZD-Deposit-1M", "NZD-Deposit-3M", "NZD-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateJPYDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "JPY-Deposit-1D", "JPY-Deposit-1W", "JPY-Deposit-1M", "JPY-Deposit-3M", "JPY-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateJPYDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "JPY-Deposit-1D", "JPY-Deposit-1W", "JPY-Deposit-1M", "JPY-Deposit-3M", "JPY-Deposit-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestConfiguration()
        {
            string xmlToTest =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
    <Instrument xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <AssetType xmlns=""http://www.fpml.org/FpML-5/reporting"">FxSpot</AssetType>
    <Currency xmlns=""http://www.fpml.org/FpML-5/reporting"">AUDUSD</Currency>
    <InstrumentNodeItem xsi:type=""DepositNodeStruct"" xmlns=""http://www.fpml.org/FpML-5/reporting"">
    <SpotDate>
        <periodMultiplier>2</periodMultiplier>
        <period>D</period>
        <dayType>Business</dayType>
        <businessDayConvention>FOLLOWING</businessDayConvention>
        <businessCenters>
        <businessCenter>AUSY</businessCenter>
        <businessCenter>GBLO</businessCenter>
        <businessCenter>USNY</businessCenter>
        </businessCenters>
    </SpotDate>
    <Deposit xmlns=""http://www.fpml.org/FpML-5/reporting"">
        <currency>AUD</currency>
        <dayCountFraction>ACT/365.FIXED</dayCountFraction>
    </Deposit>
    <BusinessDayAdjustments xmlns=""http://www.fpml.org/FpML-5/reporting"">
        <businessDayConvention>FOLLOWING</businessDayConvention>
        <businessCenters>
        <businessCenter>AUSY</businessCenter>
        <businessCenter>GBLO</businessCenter>
        <businessCenter>USNY</businessCenter>
        </businessCenters>
    </BusinessDayAdjustments>
    </InstrumentNodeItem>
    </Instrument>";

            // test deserialization
            var instrument = XmlSerializerHelper.DeserializeFromString<Instrument>(xmlToTest);
            var deposit = (DepositNodeStruct)instrument.InstrumentNodeItem;

            RelativeDateOffset spotDate = deposit.SpotDate;
            Assert.AreEqual("2", spotDate.periodMultiplier);
            Assert.AreEqual(PeriodEnum.D, spotDate.period);
            Assert.AreEqual(3, spotDate.businessCenters.businessCenter.Length);

            Deposit depo = deposit.Deposit;
            Assert.AreEqual("AUD", depo.currency.Value);

            BusinessDayAdjustments adjust = deposit.BusinessDayAdjustments;
            Assert.AreEqual(3, adjust.businessCenters.businessCenter.Length);
        }

        [TestMethod]
        public void PriceableAssetConfigurationTest()
        {
            string[] ids =
                {
                    "AUD-DEPOSIT-1D", "AUD-DEPOSIT-2D", "AUD-DEPOSIT-1W",
                    "AUD-XccyDepo-1D", "AUD-XccyDepo-2D", "AUD-XccyDepo-1W",
                };

            decimal[] values = { 0.03795m, 0.04m, 0.05m, 0.03795m, 0.04m, 0.05m };
            DateTime baseDate = new DateTime(2010, 2, 22);
            List<IPriceableRateAssetController> assets = CreatePriceableRateAssets(baseDate, ids,
                                                                                                values);

            PriceableDeposit asset0 = (PriceableDeposit)(assets.Where(a => a.Id == ids[0]).Single());
            Assert.AreEqual("0", asset0.SpotDateOffset.periodMultiplier);

            PriceableDeposit asset1 = (PriceableDeposit)(assets.Where(a => a.Id == ids[1]).Single());
            Assert.AreEqual("1", asset1.SpotDateOffset.periodMultiplier);

            PriceableDeposit asset2 = (PriceableDeposit)(assets.Where(a => a.Id == ids[2]).Single());
            Assert.AreEqual("1", asset2.SpotDateOffset.periodMultiplier);

            PriceableDeposit asset3 = (PriceableDeposit)(assets.Where(a => a.Id == ids[3]).Single());
            Assert.AreEqual("0", asset3.SpotDateOffset.periodMultiplier);

            PriceableDeposit asset4 = (PriceableDeposit)(assets.Where(a => a.Id == ids[4]).Single());
            Assert.AreEqual("0", asset4.SpotDateOffset.periodMultiplier);

            PriceableDeposit asset5 = (PriceableDeposit)(assets.Where(a => a.Id == ids[5]).Single());
            Assert.AreEqual("2", asset5.SpotDateOffset.periodMultiplier);
        }

        [TestMethod]
        public void TestCreateFastAUDXccyDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[]
                                    {
                                        "AUD-XccyDepo-1D", "AUD-XccyDepo-1W", "AUD-XccyDepo-1M", "AUD-XccyDepo-3M",
                                        "AUD-XccyDepo-6M"
                                    };
            BasicQuotation[] bq = {
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"),
                                        BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate")
                                    };

            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDXccyDeposit()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-XccyDepo-1D", "AUD-XccyDepo-1W", "AUD-XccyDepo-1M", "AUD-XccyDepo-3M", "AUD-XccyDepo-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateDeposit(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateAUDXccyDepositWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-XccyDepo-1D", "AUD-XccyDepo-1W", "AUD-XccyDepo-1M", "AUD-XccyDepo-3M", "AUD-XccyDepo-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateDepositWithNotional(baseDate, bq, instruments, notional);
            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateFastAUDZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-ZeroRate-1D", "AUD-ZeroRate-1W", "AUD-ZeroRate-1M", "AUD-ZeroRate-3M", 
                                        "AUD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateAUDZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-ZeroRate-1D", "AUD-ZeroRate-1W", "AUD-ZeroRate-1M", "AUD-ZeroRate-3M", 
                                        "AUD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateAUDZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-ZeroRate-1D", "AUD-ZeroRate-1W", "AUD-ZeroRate-1M", "AUD-ZeroRate-3M", 
                                        "AUD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateUSDZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "USD-ZeroRate-1D", "USD-ZeroRate-1W", "USD-ZeroRate-1M", "USD-ZeroRate-3M", 
                                        "USD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateUSDZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "USD-ZeroRate-1D", "USD-ZeroRate-1W", "USD-ZeroRate-1M", "USD-ZeroRate-3M", 
                                        "USD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }


        [TestMethod]
        public void TestCreateGBPZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "GBP-ZeroRate-1D", "GBP-ZeroRate-1W", "GBP-ZeroRate-1M", "GBP-ZeroRate-3M", 
                                        "GBP-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateGBPZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "GBP-ZeroRate-1D", "GBP-ZeroRate-1W", "GBP-ZeroRate-1M", "GBP-ZeroRate-3M", 
                                        "GBP-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateEURZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "EUR-ZeroRate-1D", "EUR-ZeroRate-1W", "EUR-ZeroRate-1M", "EUR-ZeroRate-3M", 
                                        "EUR-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateEURZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "EUR-ZeroRate-1D", "EUR-ZeroRate-1W", "EUR-ZeroRate-1M", "EUR-ZeroRate-3M", 
                                        "EUR-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateCHFZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "CHF-ZeroRate-1D", "CHF-ZeroRate-1W", "CHF-ZeroRate-1M", "CHF-ZeroRate-3M", 
                                        "CHF-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateCHFZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "CHF-ZeroRate-1D", "CHF-ZeroRate-1W", "CHF-ZeroRate-1M", "CHF-ZeroRate-3M", 
                                        "CHF-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateSEKZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "SEK-ZeroRate-1D", "SEK-ZeroRate-1W", "SEK-ZeroRate-1M", "SEK-ZeroRate-3M", 
                                        "SEK-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateSEKZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "SEK-ZeroRate-1D", "SEK-ZeroRate-1W", "SEK-ZeroRate-1M", "SEK-ZeroRate-3M", 
                                        "SEK-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateNZDZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "NZD-ZeroRate-1D", "NZD-ZeroRate-1W", "NZD-ZeroRate-1M", "NZD-ZeroRate-3M", 
                                        "NZD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateNZDZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "NZD-ZeroRate-1D", "NZD-ZeroRate-1W", "NZD-ZeroRate-1M", "NZD-ZeroRate-3M", 
                                        "NZD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateHKDZeroRate()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "HKD-ZeroRate-1D", "HKD-ZeroRate-1W", "HKD-ZeroRate-1M", "HKD-ZeroRate-3M", 
                                        "HKD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            var priceableControllers = CreateZeroRate(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateHKDZeroRateWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "HKD-ZeroRate-1D", "HKD-ZeroRate-1W", "HKD-ZeroRate-1M", "HKD-ZeroRate-3M", 
                                        "HKD-ZeroRate-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateZeroRateWithNotional(baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        #endregion

        #region Commodities Tests

        [TestMethod]
        public void TestCreateCommodityForward()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "USD-CommodityForward-CME.W-1D", "USD-CommodityForward-CME.W-1W", "USD-CommodityForward-CME.W-1M", 
                                        "USD-CommodityForward-CME.W-3M", "USD-CommodityForward-CME.W-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate") };
            var curveName1 = "USD-CommodityCurve-Wheat";
            //var curveName1 = "DiscountCurve1";
            //var curveName2 = "DiscountCurve2";

            var priceableControllers = CreateCommodityForward(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                var metrics = new string[1] { "IndexAtMaturity" };//TODO check this..

                Double[] times = { 0, 1 };
                Double[] dfs = { .8, 0.78 };

                var market = CreateSimpleCommodityCurve(curveName1, baseDate, times, dfs);
                var controllerData = CreateModelData(metrics, baseDate, market);
                Assert.IsNotNull(controllerData);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote.Length, metrics.Length);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote[0].measureType.Value, metrics[0]);

                var dfam = priceableController.IndexAtMaturity;

                times = new Double[2] { 0, 1 };
                dfs = new Double[2] { 0.8, 0.7 };

                metrics = new string[1] { "ImpliedQuote" };
                market = CreateSimpleCommodityCurve(curveName1, baseDate, times, dfs);
                controllerData = CreateModelData(metrics, baseDate, market);
                ((PriceableCommodityForward)priceableController).CommodityCurveName = curveName1;
                var results = priceableController.Calculate(controllerData);

                Assert.IsNotNull(results);
                Assert.AreEqual(results.quote[0].measureType.Value, metrics[0]);
                AssertExtension.Greater(results.quote[0].value, 0);

                Debug.Print("{0} ID : {1} IndexAtMaturity : {2} ImpliedQuote :",
                            priceableController.Id, dfam, results.quote[0].value);//TODO find the id.

                //TestHelper.SerializeResults(priceableController.Id, results);
            }
        }       

        [TestMethod]
        public void TestCreateCommodityFuture()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "USD-CommodityFuture-CME.W-Z8", "USD-CommodityFuture-CME.W-H9", "USD-CommodityFuture-CME.W-M9", 
                                        "USD-CommodityFuture-CME.W-U9", "USD-CommodityFuture-CME.W-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(100m, "MarketQuote", "DecimalRate"), BasicQuotationHelper.Create(100m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(100m, "MarketQuote", "DecimalRate"), BasicQuotationHelper.Create(100m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(100m, "MarketQuote", "DecimalRate") };
            var curveName1 = "USD-CommodityCurve-Wheat";
            //var curveName1 = "DiscountCurve1";
            //var curveName2 = "DiscountCurve2";

            var priceableControllers = CreateCommodityFuture(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                var metrics = new string[1] { "IndexAtMaturity" };//TODO check this..

                Double[] times = { 0, 1 };
                Double[] dfs = { 100, 100 };

                var market = CreateSimpleCommodityCurve(curveName1, baseDate, times, dfs);
                var controllerData = CreateModelData(metrics, baseDate, market);
                Assert.IsNotNull(controllerData);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote.Length, metrics.Length);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote[0].measureType.Value, metrics[0]);

                var dfam = priceableController.IndexAtMaturity;

                times = new Double[2] { 0, 1 };
                dfs = new Double[2] { 100, 100 };

                metrics = new string[1] { "ImpliedQuote"};
                market = CreateSimpleCommodityCurve(curveName1, baseDate, times, dfs);
                controllerData = CreateModelData(metrics, baseDate, market);
                ((PriceableWheatFuture)priceableController).CurveName = curveName1;
                var results = priceableController.Calculate(controllerData);

                Assert.IsNotNull(results);
                Assert.AreEqual(results.quote[0].measureType.Value, metrics[0]);
                AssertExtension.Greater(results.quote[0].value, 0);

                Debug.Print("{0} ID : {1} IndexAtMaturity : {2} ImpliedQuote :",
                            priceableController.Id, dfam, results.quote[0].value);//TODO find the id.

                //TestHelper.SerializeResults(priceableController.Id, results);
            }
        }

        [TestMethod]
        public void TestCreateCommoditySpot()
        {
            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "USD-CommoditySpot-CME.W" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.79m, "MarketQuote", "DecimalRate") };
            var curveName1 = "USD-CommodityCurve-Wheat";
            //var curveName2 = "DiscountCurve2";

            var priceableControllers = CreateCommoditySpot(curveName1, baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                var metrics = new string[1] { "IndexAtMaturity" };//TODO check this..

                Double[] times = { 0, 1 };
                Double[] dfs = { .8, 0.78 };

                var market = CreateSimpleCommodityCurve(curveName1, baseDate, times, dfs);
                var controllerData = CreateModelData(metrics, baseDate, market);
                Assert.IsNotNull(controllerData);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote.Length, metrics.Length);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote[0].measureType.Value, metrics[0]);

                var dfam = priceableController.IndexAtMaturity;

                times = new Double[2] { 0, 1 };
                dfs = new Double[2] { 0.8, 0.7 };

                metrics = new string[1] { "ImpliedQuote" };
                market = CreateSimpleCommodityCurve(curveName1, baseDate, times, dfs);
                controllerData = CreateModelData(metrics, baseDate, market);
                ((PriceableCommoditySpot)priceableController).CommodityCurveName = curveName1;
                var results = priceableController.Calculate(controllerData);

                Assert.IsNotNull(results);
                Assert.AreEqual(results.quote[0].measureType.Value, metrics[0]);
                AssertExtension.Greater(results.quote[0].value, 0);

                Debug.Print("{0} ID : {1} IndexAtMaturity : {2} ImpliedQuote :",
                            priceableController.Id, dfam, results.quote[0].value);//TODO find the id.

                //TestHelper.SerializeResults(priceableController.Id, results);
            }

        }

        #endregion

        #region IR Futures Tests

        [TestMethod]
        public void TestCreateEDFuture()
        {
            string[] instruments = new string[] { "USD-IRFuture-ED-Z8", "USD-IRFuture-ED-H9", 
                                                    "USD-IRFuture-ED-M9", "USD-IRFuture-ED-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";

            IEnumerable<IPriceableRateAssetController> priceableControllers = CreateEDFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateEDFutureWithPosition()
        {
            string[] instruments = new string[] { "USD-IRFuture-ED-Z8", "USD-IRFuture-ED-H9", 
                                                    "USD-IRFuture-ED-M9", "USD-IRFuture-ED-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";
            var position = new[] { 100.0m, 100.0m, 100.0m, 100.0m };
            IEnumerable<IPriceableRateAssetController> priceableControllers = CreateEDFutureWithPosition(curveName, baseDate, bq, vols, instruments, position);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateERFuture()
        {
            string[] instruments = new string[] { "EUR-IRFuture-ER-Z8", "EUR-IRFuture-ER-H9", 
                                                    "EUR-IRFuture-ER-M9", "EUR-IRFuture-ER-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            string curveName = "DiscountCurve";

            IEnumerable<IPriceableRateAssetController> priceableControllers = CreateERFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateESFuture()
        {
            string[] instruments = new string[] { "CHF-IRFuture-ES-Z8", "CHF-IRFuture-ES-H9", 
                                                    "CHF-IRFuture-ES-M9", "CHF-IRFuture-ES-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";

            IEnumerable<IPriceableRateAssetController> priceableControllers = CreateESFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateESFutureWithPosition()
        {
            string[] instruments = new string[] { "CHF-IRFuture-ES-Z8", "CHF-IRFuture-ES-H9", 
                                                    "CHF-IRFuture-ES-M9", "CHF-IRFuture-ES-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";
            var position = new[] { 100.0m, 100.0m, 100.0m, 100.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateESFutureWithPosition(curveName, baseDate, bq, vols, instruments, position);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateEYFuture()
        {
            string[] instruments = new string[] { "JPY-IRFuture-EY-Z8", "JPY-IRFuture-EY-H9", 
                                                    "JPY-IRFuture-EY-M9", "JPY-IRFuture-EY-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateEYFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateEYFutureWithPosition()
        {
            string[] instruments = new string[] { "JPY-IRFuture-EY-Z8", "JPY-IRFuture-EY-H9", 
                                                    "JPY-IRFuture-EY-M9", "JPY-IRFuture-EY-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";
            var position = new[] { 100.0m, 100.0m, 100.0m, 100.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateEYFutureWithPosition(curveName, baseDate, bq, vols, instruments, position);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateHRFuture()
        {
            string[] instruments = new string[] { "HKD-IRFuture-HR-Z8", "HKD-IRFuture-HR-H9", 
                                                    "HKD-IRFuture-HR-M9", "HKD-IRFuture-HR-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateHRFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateHRFutureWithPosition()
        {
            string[] instruments = new string[] { "HKD-IRFuture-HR-Z8", "HKD-IRFuture-HR-H9", 
                                                    "HKD-IRFuture-HR-M9", "HKD-IRFuture-HR-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";
            var position = new[] { 100.0m, 100.0m, 100.0m, 100.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateHRFutureWithPosition(curveName, baseDate, bq, vols, instruments, position);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateIBFuture()
        {
            string[] instruments = new string[] { "AUD-IRFuture-IB-Z8", "AUD-IRFuture-IB-H9", 
                                                    "AUD-IRFuture-IB-M9", "AUD-IRFuture-IB-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatiltiy"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatiltiy"),                               
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatiltiy"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatiltiy")};
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateIBFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateFastIRFuture()
        {
            string[] instruments = new string[] { "AUD-IRFuture-IR-Z8", "AUD-IRFuture-IR-H9", "AUD-IRFuture-IR-M9", "AUD-IRFuture-IR-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateIRFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateIRFuture()
        {
            string[] instruments = new string[] { "AUD-IRFuture-IR-Z8", "AUD-IRFuture-IR-H9", "AUD-IRFuture-IR-M9", "AUD-IRFuture-IR-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateIRFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateLFuture()
        {
            var instruments = new[] { "GBP-IRFuture-L-Z8", "GBP-IRFuture-L-H9", 
                                                    "GBP-IRFuture-L-M9", "GBP-IRFuture-L-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};

            List<IPriceableRateAssetController> priceableControllers = CreateEDFuture(baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateRAFuture()
        {
            string[] instruments = new string[] { "SEK-IRFuture-RA-Z8", "SEK-IRFuture-RA-H9", 
                                                    "SEK-IRFuture-RA-M9", "SEK-IRFuture-RA-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateRAFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateRAFutureWithPosition()
        {
            string[] instruments = new string[] { "SEK-IRFuture-RA-Z8", "SEK-IRFuture-RA-H9", 
                                                    "SEK-IRFuture-RA-M9", "SEK-IRFuture-RA-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";
            var position = new[] { 100.0m, 100.0m, 100.0m, 100.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateRAFutureWithPosition(curveName, baseDate, bq, vols, instruments, position);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateZBFuture()
        {
            string[] instruments = new string[] { "NZD-IRFuture-ZB-Z8", "NZD-IRFuture-ZB-H9", 
                                                    "NZD-IRFuture-ZB-M9", "NZD-IRFuture-ZB-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateZBFuture(curveName, baseDate, bq, vols, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateZBFutureWithPosition()
        {
            string[] instruments = new string[] { "NZD-IRFuture-ZB-Z8", "NZD-IRFuture-ZB-H9", 
                                                    "NZD-IRFuture-ZB-M9", "NZD-IRFuture-ZB-Z9" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            BasicQuotation[] vols = { BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility"), 
                                        BasicQuotationHelper.Create(.2m, "Volatility", "LogNormalVolatility")};
            const string curveName = "DiscountCurve";
            var position = new[] { 100.0m, 100.0m, 100.0m, 100.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateZBFutureWithPosition(curveName, baseDate, bq, vols, instruments, position);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        #endregion

        #region FX Tests

        [TestMethod]
        public void TestCreateFxForward()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUDUSD-FxForward-ON", "AUDUSD-FxForward-TN", "AUDUSD-FxSpot-SP", 
                                        "AUDUSD-FxForward-3M", "AUDUSD-FxForward-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.791m, "MarketQuote", "DecimalRate") };
            const string curveName1 = "AUD-USD";
            //var curveName1 = "DiscountCurve1";
            //var curveName2 = "DiscountCurve2";

            var priceableControllers = CreateFxForward(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                var metrics = new string[1] { "ForwardAtMaturity" };//TODO check this..

                Double[] times = { 0, 1 };
                Double[] dfs = { .8, 0.78 };

                var market = CreateSimpleFxCurve(curveName1, baseDate, times, dfs);
                var controllerData = CreateModelData(metrics, baseDate, market);
                Assert.IsNotNull(controllerData);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote.Length, metrics.Length);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote[0].measureType.Value, metrics[0]);

                var dfam = priceableController.ForwardAtMaturity;

                times = new Double[] { 0, 1 };
                dfs = new[] { 0.8, 0.7 };

                metrics = new[] { "ImpliedQuote" };
                market = CreateSimpleFxCurve(curveName1, baseDate, times, dfs);
                controllerData = CreateModelData(metrics, baseDate, market);
                ((PriceableFxForwardRate)priceableController).FxCurveName = curveName1;
                var results = priceableController.Calculate(controllerData);

                Assert.IsNotNull(results);
                Assert.AreEqual(results.quote[0].measureType.Value, metrics[0]);
                AssertExtension.Greater(results.quote[0].value, 0);

                Debug.Print("ID : {0} ForwardAtMaturity :  {1} ImpliedQuote : {2}   RiskDate : {3}",
                            priceableController.Id, dfam, results.quote[0].value, priceableController.GetRiskMaturityDate());//TODO find the id.

                //TestHelper.SerializeResults(priceableController.Id, results);
            }

        }

        [TestMethod]
        public void TestCreateFxSpot()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUDUSD-FxSpot-SP" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.79m, "MarketQuote", "DecimalRate") };
            var curveName1 = "AUD-USD";
            //var curveName2 = "DiscountCurve2";

            var priceableControllers = CreateFxSpot(baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                var metrics = new string[1] { "ForwardAtMaturity" };//TODO check this..

                Double[] times = { 0, 1 };
                Double[] dfs = { .8, 0.78 };

                var market = CreateSimpleFxCurve(curveName1, baseDate, times, dfs);
                var controllerData = CreateModelData(metrics, baseDate, market);
                Assert.IsNotNull(controllerData);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote.Length, metrics.Length);
                Assert.AreEqual(controllerData.BasicAssetValuation.quote[0].measureType.Value, metrics[0]);

                var dfam = priceableController.ForwardAtMaturity;

                times = new Double[2] { 0, 1 };
                dfs = new Double[2] { 0.8, 0.7 };

                metrics = new string[1] { "ImpliedQuote" };
                market = CreateSimpleFxCurve(curveName1, baseDate, times, dfs);
                controllerData = CreateModelData(metrics, baseDate, market);
                ((PriceableFxSpotRate)priceableController).FxCurveName = curveName1;
                var results = priceableController.Calculate(controllerData);

                Assert.IsNotNull(results);
                Assert.AreEqual(results.quote[0].measureType.Value, metrics[0]);
                AssertExtension.Greater(results.quote[0].value, 0);

                Debug.Print("{0} ID : {1} ForwardAtMaturity : {2} ImpliedQuote :",
                            priceableController.Id, dfam, results.quote[0].value);//TODO find the id.

                //TestHelper.SerializeResults(priceableController.Id, results);
            }

        }

        #endregion

        #region Inflation Tests

        [TestMethod]
        public void TestCreateCPIndex()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-CPIndex-1D", "AUD-CPIndex-1W", "AUD-CPIndex-1M", "AUD-CPIndex-3M", "AUD-CPIndex-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            List<IPriceableRateAssetController> priceableControllers = CreateCPIndex(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateCPIndexWithNotional()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-CPIndex-1D", "AUD-CPIndex-1W", "AUD-CPIndex-1M", "AUD-CPIndex-3M", "AUD-CPIndex-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateCPIndexWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateInflationXibor()
        {

            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "AUD-CPIndex-1D", "AUD-CPIndex-1W", "AUD-CPIndex-1M", "AUD-CPIndex-3M", "AUD-CPIndex-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            List<IPriceableInflationAssetController> priceableControllers = CreateXibor(baseDate, bq, instruments);

            foreach (IPriceableInflationAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateCPISwap()
        {
            string[] instruments = AUDInflationAssets;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleCPISwap(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateCPISwapAsAnInflationController()
        {
            string[] instruments = AUDInflationAssets;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            var priceableControllers = CreateSimpleZCCPISwapAsInflation(baseDate, bq, instruments);

            foreach (IPriceableInflationAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateCPISwapAsAnInflationControllerWithNotional()
        {
            string[] instruments = AUDInflationAssets;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateSimpleZCCPISwapAsInflationWithNotional(curveName, baseDate, bq, instruments, notional);

            foreach (IPriceableInflationAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateCPISwapAsARateController()
        {
            string[] instruments = AUDInflationAssets;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                    BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleZCCPISwap(baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        //BMK 2

        #endregion

        #region Spread Asset Tests

        [TestMethod]
        public void TestCreateFastAUDBasisSwap()
        {
            string[] instruments = AUDBasisAssets;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate") };

            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleBasisSwap(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                SimpleDiscountFactorCurve curve = new SimpleDiscountFactorCurve(baseDate,
                                                                                InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                                                true, new List<DateTime>(dates), values);
                decimal dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                decimal impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam, impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                decimal dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                decimal impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2, impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id, dfam2 - dfam, impquote2 - impquote);
            }
        }

        [TestMethod]
        public void TestCreateAUDBasisSwap()
        {
            string[] instruments = AUDBasisAssets;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate") };

            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleBasisSwap(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                string[] metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDBasisSwapWithNotional()
        {
            string[] instruments = AUDBasisAssets;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate") };

            decimal[] notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateBasiswapWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                string[] metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void PriceableAUDSpreadfra()
        {

            var baseDate = new DateTime(2008, 2, 20);

            BasicQuotation[] bq = {
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread")
                                };

            var priceableControllers = CreateSimpleSpreadFra(baseDate, bq, AUDinstruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);

                ProcessAssetControllerResults(priceableController, _metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void PriceableUSDSpreadfra()
        {

            var baseDate = new DateTime(2008, 2, 20);

            BasicQuotation[] bq = {
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread")
                                };

            var priceableControllers = CreateSimpleSpreadFra(baseDate, bq, USDinstruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, _metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void PriceableGBPSpreadfra()
        {

            var baseDate = new DateTime(2008, 2, 20);

            BasicQuotation[] bq = {
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread")
                                };

            var priceableControllers = CreateSimpleSpreadFra(baseDate, bq, GBPinstruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, _metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void PriceableNZDSpreadfra()
        {

            var baseDate = new DateTime(2008, 2, 20);

            BasicQuotation[] bq = {
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread")
                                };

            var priceableControllers = CreateSimpleSpreadFra(baseDate, bq, NZDinstruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, _metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void PriceableJPYSpreadfra()
        {

            var baseDate = new DateTime(2008, 2, 20);

            BasicQuotation[] bq = {
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread")
                                };

            var priceableControllers = CreateSimpleSpreadFra(baseDate, bq, JPYinstruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, _metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void PriceableEURSpreadfra()
        {

            var baseDate = new DateTime(2008, 2, 20);

            BasicQuotation[] bq = {
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"), 
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread"),
                                    BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalSpread")
                                };

            var priceableControllers = CreateSimpleSpreadFra(baseDate, bq, EURinstruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, _metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void TestCreateFastAUDXccyBasisSwap()
        {
            string[] instruments = AUDXccyBasis;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate") };

            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleXccyBasisSwap(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                SimpleDiscountFactorCurve curve = new SimpleDiscountFactorCurve(baseDate,
                                                                                InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                                                true, new List<DateTime>(dates), values);
                decimal dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                decimal impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam, impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                decimal dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                decimal impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2, impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id, dfam2 - dfam, impquote2 - impquote);
            }
        }

        [TestMethod]
        public void TestCreateAUDXccyBasisSwap()
        {
            string[] instruments = AUDXccyBasis;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate") };

            const string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateSimpleXccyBasisSwap(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                string[] metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDXccyBasisSwapWithNotional()
        {
            string[] instruments = AUDXccyBasis;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.01m, "MarketQuote", "DecimalRate") };

            decimal[] notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateSimpleXccySwapWithNotional(baseDate, bq, instruments, notional);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                string[] metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        #endregion

        #region Xibor Tests

        [TestMethod]
        public void GetForwardRateTest()
        {
            const string MarketEnvironmentId = "GetForwardRateTest";
            MarketEnvironment marketEnvironment = new MarketEnvironment(MarketEnvironmentId);

            DateTime startDate = new DateTime(2011, 5, 11);
            DateTime forwardDate = new DateTime(2011, 8, 17);
            DateTime valuationDate = new DateTime(2010, 5, 9);
            string curveName = CreateCurve(marketEnvironment, valuationDate);
            const decimal Rate = 0;
            DayCountFraction dayCountFraction = DayCountFractionHelper.ToDayCountFraction(DayCountFractionEnum.ACT_365_FIXED);

            PriceableXibor priceableRate = new PriceableXibor("MyXibor", Rate, startDate, forwardDate, dayCountFraction);
            priceableRate.CurveName = curveName;
            string[] metrics = new[] { RateMetrics.ImpliedQuote.ToString() };
            IAssetControllerData a = new AssetControllerData();
            var assetModelData = a.CreateAssetControllerData(metrics, valuationDate, marketEnvironment);
            priceableRate.Calculate(assetModelData);
            decimal result = priceableRate.CalculationResults.ImpliedQuote;

            Assert.AreEqual(0.03d, (double)result, 1e-5);
        }

        [TestMethod]
        public void TestCreateFastAUDXibor()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "AUD-Xibor-1D", "AUD-Xibor-1W", "AUD-Xibor-1M", "AUD-Xibor-3M", "AUD-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                var curve = new SimpleDiscountFactorCurve(baseDate,
                                                            InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                            true, new List<DateTime>(dates), values);
                var dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam, impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                var dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2, impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id, dfam2 - dfam, impquote2 - impquote);
            }

        }

        [TestMethod]
        public void TestCreateAUDXibor()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "AUD-Xibor-1D", "AUD-Xibor-1W", "AUD-Xibor-1M", "AUD-Xibor-3M", "AUD-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                //TestHelper.SerializeResults(priceableController.Id, results);
            }

        }

        [TestMethod]
        public void TestCreateAUDXiborWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "AUD-Xibor-1D", "AUD-Xibor-1W", "AUD-Xibor-1M", "AUD-Xibor-3M", "AUD-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateXiborWithNotional(curveName, baseDate, bq, instruments, notional);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateUSDXibor()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "USD-Xibor-1D", "USD-Xibor-1W", "USD-Xibor-1M", "USD-Xibor-3M", "USD-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateEURXibor()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "EUR-Xibor-1D", "EUR-Xibor-1W", "EUR-Xibor-1M", "EUR-Xibor-3M", "EUR-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateGBPXibor()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "GBP-Xibor-1D", "GBP-Xibor-1W", "GBP-Xibor-1M", "GBP-Xibor-3M", "GBP-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }


        [TestMethod]
        public void TestCreateJPYXibor()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "JPY-Xibor-1D", "JPY-Xibor-1W", "JPY-Xibor-1M", "JPY-Xibor-3M", "JPY-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }


        [TestMethod]
        public void TestCreateNZDXibor()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "NZD-Xibor-1D", "NZD-Xibor-1W", "NZD-Xibor-1M", "NZD-Xibor-3M", "NZD-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateHKDXibor()
        {
            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "HKD-Xibor-1D", "HKD-Xibor-1W", "HKD-Xibor-1M", "HKD-Xibor-3M", "HKD-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";
            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);
            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }


        [TestMethod]
        public void TestCreateCHFXibor()
        {
            var baseDate = new DateTime(2008, 2, 20);
            var instruments = new[] { "CHF-Xibor-1D", "CHF-Xibor-1W", "CHF-Xibor-1M", "CHF-Xibor-3M", "CHF-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            const string curveName = "DiscountCurve";
            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, baseDate, bq, instruments);
            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }

        }

        [TestMethod]
        public void TestCreateSEKXibor()
        {
            var date = new DateTime(2008, 2, 20);
            var instruments = new[] { "SEK-Xibor-1D", "SEK-Xibor-1W", "SEK-Xibor-1M", "SEK-Xibor-3M", "SEK-Xibor-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";
            List<IPriceableRateAssetController> priceableControllers = CreateXibor(curveName, date, bq, instruments);
            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, date);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, date);
            }
        }

        #endregion

        #region Commodity Tests

        [TestMethod]
        public void CommodityAverageSwapTest()
        {
            var offset = new Offset
                {
                    dayType = DayTypeEnum.Business,
                    dayTypeSpecified = true,
                    period = PeriodEnum.D,
                    periodMultiplier = "0",
                    periodSpecified = true
                };
            var floatingindex = new FloatingRateIndex {Value = "PLATTS"};
            var period = PeriodHelper.Parse("2D");
            var priceableRate = CommodityAverageSwapHelper.Create("CommodityAverageSwap", "ICE Brent", "USD", 25, SpecifiedPriceEnum.Spot, offset, PriceQuoteUnitsEnum.BBL,
                DeliveryDatesEnum.Spot, "IEPA", "Platts", "MODFOLLOWING", "GBLO", "1M", DayTypeEnum.Business, "CalculationEndDate", floatingindex, period,
                AveragingMethodEnum.Unweighted, 1.0m, null, null, null, null);
            var result = XmlSerializerHelper.SerializeToString(priceableRate);
            Debug.Print(result);
        }

        #endregion

        #endregion

        #region PriceableInstrument Tests

        #region Interest Rate Cap Tests

        //ToDo reinstate test
            //[TestMethod]
            public void TestIRCap()
            {
                _properties.Set(CurveProp.PricingStructureType, "RateCurve");
                _properties.Set("BuildDateTime", _baseDate);
                _properties.Set(CurveProp.Market, CurveConst.QR_LIVE);
                _properties.Set("Identifier", "RateCurve.AUD-LIBOR-BBA-3M");
                _properties.Set(CurveProp.IndexName, "AUD-LIBOR-BBA");
                _properties.Set(CurveProp.IndexTenor, "3M");
                _properties.Set(CurveProp.CurveName, "AUD-LIBOR-BBA-3M");
                _properties.Set("Algorithm", "FastLinearZero");

                var ratecurve = (RateCurve)CurveEngine.CreateCurve(_properties, _audInstruments, _rates, _adjustments, FixingCalendar, PaymentCalendar);
                //Get the curve.
                //var ratecurve = (IRateCurve)ObjectCacheHelper.GetPricingStructureFromSerialisable(curveid);//This requires an IPricingStructure to be cached.

                _volproperties.Set(CurveProp.PricingStructureType, "RateVolatilityMatrix");
                _volproperties.Set("BuildDateTime", _baseDate);
                _volproperties.Set("BaseDate", _baseDate);
                _volproperties.Set(CurveProp.Market, "LIVE");
                _volproperties.Set(CurveProp.MarketDate, _baseDate);
                _volproperties.Set("Instrument", "AUD-Xibor-3M");
                _volproperties.Set("Source", "SydneySwapDesk");
                _volproperties.Set(CurveProp.CurveName, "AUD-Xibor-3M-SydneySwapDesk");
                _volproperties.Set("Identifier", "RateVolatilityMatrix.AUD-Xibor-3M");
                _volproperties.Set("Algorithm", "Linear");
                _volproperties.Set("Currency", "AUD");
                _volproperties.Set("QuoteUnits", "LogNormalVolatility");
                _volproperties.Set("InformationSource", "SwapDesk");
                _volproperties.Set("MeasureType", "Volatility");
                _volproperties.Set("StrikeQuoteUnits", "Absolute");
                _volproperties.Set("QuotationSide", "Mid");
                _volproperties.Set("Timing", "Close");
                _volproperties.Set("ValuationDate", _baseDate);
                _volproperties.Set("BusinessCenter", "Sydney");

                _volatilities = GenerateVols(0.12);

                //var volcurveId = ObjectCacheHelper.CreateVolatilitySurfaceWithProperties(_volproperties, _expiries, _strikes, _volatilities);
                //Get the curve.
                var volcurve = (IVolatilitySurface)CurveEngine.CreateVolatilitySurface(_volproperties, _expiries, _strikes, _volatilities);

                GetIRCapPDH(ratecurve, volcurve, _baseDate);
                //GetVolPDH(volcurve, _baseDate);


            }

        #endregion

        #region Cross Currency Swap Tests

        [TestMethod]
        public void TestCreateFastAUDXccySwap()
        {
            string[] instruments = AUDXccySwap;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            const string curveName = "DiscountCurve";

            var priceableControllers = CreateSimpleXccySwap(curveName, baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                var curve = new SimpleDiscountFactorCurve(baseDate,
                                                            InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                            true, new List<DateTime>(dates), values);
                var dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam, impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                var dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2, impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id, dfam2 - dfam, impquote2 - impquote);
            }
        }

        [TestMethod]
        public void TestCreateAUDXccySwap()
        {
            string[] instruments = AUDXccySwap;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            const string curveName = "DiscountCurve";

            var priceableControllers = CreateSimpleXccySwap(curveName, baseDate, bq, instruments);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateAUDIRSwapWithNotional()
        {
            string[] instruments = AUDXccySwap;
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };

            const string curveName = "DiscountCurve";
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            var priceableControllers = CreateSimpleXccySwapWithNotional(curveName, baseDate, bq, instruments, notional);

            foreach (var priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        #endregion

        #region OIS Tests

        [TestMethod]
        public void TestCreateFastAUDOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "AUD-OIS-1D", "AUD-OIS-1W", "AUD-OIS-1M", "AUD-OIS-3M", "AUD-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                Assert.IsNotNull(priceableController);

                DateTime[] dates = { baseDate, baseDate.AddMonths(12), baseDate.AddMonths(24) };
                double[] values = { 1, 0.9, 0.8 };
                var curve = new SimpleDiscountFactorCurve(baseDate,
                                                            InterpolationMethodHelper.Parse("LogLinearInterpolation"),
                                                            true, new List<DateTime>(dates), values);
                var dfam = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam, impquote);
                priceableController.MarketQuote.value = priceableController.MarketQuote.value + 0.001m;
                var dfam2 = priceableController.CalculateDiscountFactorAtMaturity(curve);
                var impquote2 = priceableController.CalculateImpliedQuote(curve);
                Debug.Print("{0} DiscountFactorAtMaturity : {1} ImpliedQuote : {2}", priceableController.Id, dfam2, impquote2);
                Debug.Print("{0} DiscountFactorAtMaturityDiff : {1} ImpliedQuoteDiff : {2}", priceableController.Id, dfam2 - dfam, impquote2 - impquote);
            }
        }


        [TestMethod]
        public void TestCreateAUDOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "AUD-OIS-1D", "AUD-OIS-1W", "AUD-OIS-1M", "AUD-OIS-3M", "AUD-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new string[2] { "ImpliedQuote", "AccrualFactor" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }


        [TestMethod]
        public void TestCreateAUDOisWithNotional()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "AUD-OIS-1D", "AUD-OIS-1W", "AUD-OIS-1M", "AUD-OIS-3M", "AUD-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";
            var notional = new[] { 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m, 10000000.0m };
            List<IPriceableRateAssetController> priceableControllers = CreateOisWithNotional(curveName, baseDate, bq, instruments, notional);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
            }
        }

        [TestMethod]
        public void TestCreateUSDOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "USD-OIS-1D", "USD-OIS-1W", "USD-OIS-1M", "USD-OIS-3M", "USD-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }


        [TestMethod]
        public void TestCreateEUROis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "EUR-OIS-1D", "EUR-OIS-1W", "EUR-OIS-1M", "EUR-OIS-3M", "EUR-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }


        [TestMethod]
        public void TestCreateGBPOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "GBP-OIS-1D", "GBP-OIS-1W", "GBP-OIS-1M", "GBP-OIS-3M", "GBP-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void TestCreateNZDOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "NZD-OIS-1D", "NZD-OIS-1W", "NZD-OIS-1M", "NZD-OIS-3M", "NZD-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void TestCreateJPYOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "JPY-OIS-1D", "JPY-OIS-1W", "JPY-OIS-1M", "JPY-OIS-3M", "JPY-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void TestCreateCHFOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "CHF-OIS-1D", "CHF-OIS-1W", "CHF-OIS-1M", "CHF-OIS-3M", "CHF-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void TestCreateSEKOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "SEK-OIS-1D", "SEK-OIS-1W", "SEK-OIS-1M", "SEK-OIS-3M", "SEK-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }

        [TestMethod]
        public void TestCreateHKDOis()
        {

            DateTime baseDate = new DateTime(2008, 2, 20);
            string[] instruments = new string[] { "HKD-OIS-1D", "HKD-OIS-1W", "HKD-OIS-1M", "HKD-OIS-3M", "HKD-OIS-6M" };
            BasicQuotation[] bq = { BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate"), 
                                BasicQuotationHelper.Create(.07m, "MarketQuote", "DecimalRate") };
            string curveName = "DiscountCurve";

            List<IPriceableRateAssetController> priceableControllers = CreateOis(curveName, baseDate, bq, instruments);

            foreach (IPriceableRateAssetController priceableController in priceableControllers)
            {
                var metrics = new[] { "DiscountFactorAtMaturity" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);
                metrics = new[] { "ImpliedQuote", "AccrualFactor", "DeltaR", "NPV" };
                ProcessAssetControllerResults(priceableController, metrics, baseDate);//TODO find the id.
            }
        }

    #endregion

        #endregion
    }
}
