using System;
using System.Diagnostics;
using FpML.V5r3.Reporting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Constants;
using Orion.UnitTestEnv;
using Orion.Util.Expressions;
using Orion.Util.Serialisation;
using Orion.V5r3.PublisherWebService;
using PublisherWebService.Tests.Properties;
using Exception = System.Exception;

namespace PublisherWebService.Tests
{
    [TestClass]
    public class CurveCalQ
    {
        private const string Currency = "AUD";
        private const string RateCurveMarketName = "LiveUnitTest";
        private const string RateCurveIndexName = "BBR-BBSW";
        //private const string IndexName = "AUD-BBR-BBSW";
        private static UnitTestEnvironment UTE { get; set; }
        private static PricingStructures Ps { get; set; }

        #region Test Initialize and Cleanup

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            UTE = new UnitTestEnvironment();
            Ps = new PricingStructures(UTE.Proxy);
        }

        [ClassCleanup]
        public static void Teardown()
        {
            // Release resources
            //Logger.Dispose()
            UTE.Dispose();
        }

        #endregion

        #region Live Curves

        [TestMethod]
        public void TraderMid3MTest()
        {
            string actual = CreateTraderMid3M();
            DeleteMarket(actual);
        }

        private string CreateTraderMid3M()
        {
            object[][] rateCurveValues
                = {
                      new object[] {"AUD-Deposit-1D", 0.045, 0},
                      new object[] {"AUD-Deposit-1M", 0.0459, 0},
                      new object[] {"AUD-Deposit-2M", 0.0468, 0},
                      new object[] {"AUD-Deposit-3M", 0.0474, 0},
                      new object[] {"AUD-IRFuture-IR-1", 0.0477500000000001, 0},
                      new object[] {"AUD-IRFuture-IR-2", 0.04785, 0},
                      new object[] {"AUD-IRFuture-IR-3", 0.048447509544, 0},
                      new object[] {"AUD-IRFuture-IR-4", 0.048740347623, 0},
                      new object[] {"AUD-IRFuture-IR-5", 0.0489285971530001, 0},
                      new object[] {"AUD-IRFuture-IR-6", 0.049311997644, 0},
                      new object[] {"AUD-IRFuture-IR-7", 0.049590809551, 0},
                      new object[] {"AUD-IRFuture-IR-8", 0.049765276462, 0},
                      new object[] {"AUD-IRSwap-3Y", 0.0493999999999999, 0},
                      new object[] {"AUD-IRSwap-4Y", 0.0502749999999999, 0},
                      new object[] {"AUD-IRSwap-5Y", 0.0511249999999999, 0},
                      new object[] {"AUD-IRSwap-6Y", 0.0517791666666667, 0},
                      new object[] {"AUD-IRSwap-7Y", 0.0523416666666667, 0},
                      new object[] {"AUD-IRSwap-8Y", 0.05265, 0},
                      new object[] {"AUD-IRSwap-9Y", 0.0528833333333333, 0},
                      new object[] {"AUD-IRSwap-10Y", 0.0531333333333334, 0},
                      new object[] {"AUD-IRSwap-12Y", 0.0536241666666667, 0},
                      new object[] {"AUD-IRSwap-15Y", 0.054, 0},
                      new object[] {"AUD-IRSwap-20Y", 0.05365, 0},
                      new object[] {"AUD-IRSwap-25Y", 0.0523958333333333, 0},
                      new object[] {"AUD-IRSwap-30Y", 0.0509583333333333, 0},
                      new object[] {"AUD-IRSwap-40Y", 0.0489687373645292, 0}
                  };
            var rateCurveProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateCurve"},
                          new object[] {"IndexTenor", "3M"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "LIBOR-BBA"},
                          new object[] {"Algorithm", "CalypsoAlgo4"},
                          new object[] {"MarketName", "TraderMid"}
                      };
            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", "TraderMid"}
                      };
            // Publish via web service
            var actual = Ps.PublishCurve(rateCurveProperties, publishProperties, rateCurveValues);
            Assert.AreEqual("Market.TraderMid.RateCurve.AUD-LIBOR-BBA-3M", actual);
            return actual;
        }

        [TestMethod]
        public void TraderMid1MTest()
        {
            string curve3M = CreateTraderMid3M();
            object[][] rateCurveValues
                = {
                      new object[] {"AUD-Deposit-1D", 0.045, 0},
                      new object[] {"AUD-Deposit-1M", 0.0459, 0},
                      new object[] {"AUD-Deposit-2M", 0.04615, 0},
                      new object[] {"AUD-Deposit-3M", 0.0463, 0},
                      new object[] {"AUD-Deposit-6M", 0.0467, 0},
                      new object[] {"AUD-BasisSwap-1Y-1M", 0.00135, 0},
                      new object[] {"AUD-BasisSwap-2Y-1M", 0.001325, 0},
                      new object[] {"AUD-BasisSwap-3Y-1M", 0.001275, 0},
                      new object[] {"AUD-BasisSwap-4Y-1M", 0.001225, 0},
                      new object[] {"AUD-BasisSwap-5Y-1M", 0.00115, 0},
                      new object[] {"AUD-BasisSwap-6Y-1M", 0.001075, 0},
                      new object[] {"AUD-BasisSwap-7Y-1M", 0.001, 0},
                      new object[] {"AUD-BasisSwap-8Y-1M", 0.00095, 0},
                      new object[] {"AUD-BasisSwap-9Y-1M", 0.0009, 0},
                      new object[] {"AUD-BasisSwap-10Y-1M", 0.00085, 0},
                      new object[] {"AUD-BasisSwap-12Y-1M", 0.000744497065703892, 0},
                      new object[] {"AUD-BasisSwap-15Y-1M", 0.000640595748637547, 0},
                      new object[] {"AUD-BasisSwap-20Y-1M", 0.000539134782200506, 0},
                      new object[] {"AUD-BasisSwap-25Y-1M", 0.000479202359060092, 0},
                      new object[] {"AUD-BasisSwap-30Y-1M", 0.000438905722637305, 0},
                      new object[] {"AUD-BasisSwap-40Y-1M", 0.000388227121535449, 0},
                      new object[] {null, null, null}
                  };
            var rateCurveProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateBasisCurve"},
                          new object[] {"IndexTenor", "1M"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "LIBOR-BBA"},
                          new object[] {"Algorithm", "CalypsoAlgo4"},
                          new object[] {"MarketName", "TraderMid"},
                          new object[] {"ReferenceCurveUniqueId", curve3M}
                      };
            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", "TraderMid"}
                      };
            // Publish via web service
            var actual = Ps.PublishCurve(rateCurveProperties, publishProperties, rateCurveValues);
            Assert.AreEqual("Market.TraderMid.RateCurve.AUD-LIBOR-BBA-1M", actual);
            DeleteMarket(actual);
        }

        [TestMethod]
        public void PublishSwaptionVolTest()
        {
            #region inputs

            var swaptionProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateATMVolatilityMatrix"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "LIBOR-BBA"},
                          new object[] {"MarketName", "Live"},
                          new object[] {"Instrument", "Swaption"},
                          // Need the following properties in the test only for XML comparison purposes
                          new object[] {"BuildDateTime", new DateTime(2010,9,7,14,18,22)}, 
                          new object[] {"BaseDate", new DateTime(2010,9,7)}
                      };

            object[][] data
                = {
                    new object[]{"Expiry","1Y","2Y","3Y","4Y","5Y","7Y","10Y","15Y","20Y"},
                    new object[]{"1M",0.173134247023696,0.184077990805836,0.188905171586698,0.182345674673924,0.179442017586033,0.175034313302459,0.172556305236928,0.17025352108125,0.171631333269796},
                    new object[]{"2M",0.179277716343017,0.182174188861554,0.188653544650265,0.182133684218621,0.180733954289418,0.177769676925,0.175283563383322,0.172975021589566,0.174410731184393},
                    new object[]{"3M",0.182089980429339,0.183332284198686,0.188019706630795,0.182035679640915,0.179086344736561,0.180579145171599,0.172230384569876,0.16997948184624,0.171416852480155},
                    new object[]{"6M",0.200280122803084,0.198404375356211,0.19330116186503,0.184449496822683,0.181528524454177,0.177939857617636,0.172787463379575,0.171112246293098,0.172952396642254},
                    new object[]{"1Y",0.217218062423376,0.212440339730523,0.206818984581716,0.194825289028441,0.18786125097534,0.177850189733278,0.171542486014626,0.170533782787442,0.172870188265416},
                    new object[]{"2Y",0.213951836421849,0.204717658394046,0.195444500781416,0.182571952719406,0.177161814989823,0.169377783319335,0.164893517522823,0.165283103739824,0.168562461268347},
                    new object[]{"3Y",0.194658470779212,0.186385631023403,0.179760621231912,0.17085151106272,0.167684145614881,0.161193045082049,0.156091151729022,0.157466958215653,0.16163725459035},
                    new object[]{"5Y",0.162525825284052,0.160691313766798,0.159829593674324,0.154742282352831,0.152465666894163,0.149147930104837,0.148255797984937,0.151426378255815,0.157599135003681},
                    new object[]{"7Y",0.159659781661822,0.157689978594064,0.154801478954388,0.149200829346026,0.147514443867689,0.14605406456858,0.147778807110965,0.152767418877533,0.159923596145401},
                    new object[]{"10Y",0.151211096658033,0.14906260069021,0.147122219889546,0.144420892693253,0.142858268689848,0.145706063256514,0.148453408815127,0.157619086923414,0.166637987047625},
                    new object[]{null, null, null, null, null, null, null, null, null, null}
                  };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", "TraderMid"}
                      };

            #endregion

            // Publish via web service
            var actualId = Ps.PublishCurve(swaptionProperties, publishProperties, data);
            Assert.AreEqual("Market.Live.RateATMVolatilityMatrix.AUD-LIBOR-BBA-Swaption", actualId);
            var market = Ps.GetCurve(actualId, false);
            Market fpml = market.GetMarket();
            string actual = XmlSerializerHelper.SerializeToString(fpml);
            string expected = Resources.PublishSwaptionVol_Expected;
            Assert.AreEqual(expected, actual);
            DeleteMarket(actual);
        }

        [TestMethod]
        public void PublishCapFloorVolTest()
        {
            #region inputs

            var surfaceProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateVolatilityMatrix"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "LIBOR-BBA"},
                          new object[] {"MarketName", "Live"},
                          new object[] {"Instrument", "CapFloor"},
                          new object[] {"StrikeQuoteUnits", "DecimalRate"},
                          new object[] {"CapFrequency", "3M"},
                          // Need the following properties in the test only for XML comparison purposes
                          new object[] {"BuildDateTime", new DateTime(2010,9,7,14,18,22)}, 
                          new object[] {"BaseDate", new DateTime(2010,9,7)}
                      };
            object[][] inputs
                = {
                      new object[] {"AUD-Caplet-1Y", 0.0482432257833451, 0.258917113332362},
                      new object[] {"AUD-Caplet-2Y", 0.0489047675330314, 0.252181607167987},
                      new object[] {"AUD-Caplet-3Y", 0.0494999996496402, 0.24595493086507},
                      new object[] {"AUD-Caplet-4Y", 0.0500767176395453, 0.235228761775413},
                      new object[] {"AUD-Caplet-5Y", 0.050908055135771, 0.22672862730331},
                      new object[] {"AUD-Caplet-7Y", 0.0519940295610645, 0.209829051906196},
                      new object[] {"AUD-Caplet-10Y", 0.0527829861943455, 0.197706060815361},
                      new object[] {"AUD-Caplet-15Y", 0.0536383983104803, 0.182761996168826},
                      new object[] {"AUD-Caplet-20Y", 0.0532931908550533, 0.183945839782503},
                      new object[] {"AUD-Caplet-25Y", 0.0520391302210335, 0.188378643241806},
                      new object[] {"AUD-Caplet-30Y", 0.0506331706871363, 0.193609458256038},
                      new object[] {null, null, null}
                  };
            object[][] data
                = {
                      new object[] {DateTime.Parse("2010-09-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2010-12-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-03-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-06-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-09-07"), 0.253606025847892},
                      new object[] {DateTime.Parse("2011-12-07"), 0.250657582889503},
                      new object[] {DateTime.Parse("2012-03-07"), 0.247857813993874},
                      new object[] {DateTime.Parse("2012-06-07"), 0.245362314572259},
                      new object[] {DateTime.Parse("2012-09-07"), 0.24396194632708},
                      new object[] {DateTime.Parse("2012-12-07"), 0.242681369711064},
                      new object[] {DateTime.Parse("2013-03-07"), 0.239680981323173},
                      new object[] {DateTime.Parse("2013-06-07"), 0.233915867350968},
                      new object[] {DateTime.Parse("2013-09-09"), 0.225180967697172},
                      new object[] {DateTime.Parse("2013-12-09"), 0.21698945730618},
                      new object[] {DateTime.Parse("2014-03-07"), 0.211047320868142},
                      new object[] {DateTime.Parse("2014-06-10"), 0.208768656780787},
                      new object[] {DateTime.Parse("2014-09-08"), 0.208730809964552},
                      new object[] {DateTime.Parse("2014-12-08"), 0.207029962602162},
                      new object[] {DateTime.Parse("2015-03-09"), 0.204130665748063},
                      new object[] {DateTime.Parse("2015-06-09"), 0.199692875407936},
                      new object[] {DateTime.Parse("2015-09-07"), 0.193752743474213},
                      new object[] {DateTime.Parse("2015-12-07"), 0.188009297591465},
                      new object[] {DateTime.Parse("2016-03-07"), 0.182933267922475},
                      new object[] {DateTime.Parse("2016-06-07"), 0.178756416240464},
                      new object[] {DateTime.Parse("2016-09-07"), 0.175389831055263},
                      new object[] {DateTime.Parse("2016-12-07"), 0.1736753841047},
                      new object[] {DateTime.Parse("2017-03-07"), 0.173469043393157},
                      new object[] {DateTime.Parse("2017-06-07"), 0.175052848911834},
                      new object[] {DateTime.Parse("2017-09-07"), 0.17638091320432},
                      new object[] {DateTime.Parse("2017-12-07"), 0.177178024517271},
                      new object[] {DateTime.Parse("2018-03-07"), 0.177932794687923},
                      new object[] {DateTime.Parse("2018-06-07"), 0.17859628828216},
                      new object[] {DateTime.Parse("2018-09-07"), 0.17879160950702},
                      new object[] {DateTime.Parse("2018-12-07"), 0.179024163932461},
                      new object[] {DateTime.Parse("2019-03-07"), 0.178930635076251},
                      new object[] {DateTime.Parse("2019-06-07"), 0.178427454575685},
                      new object[] {DateTime.Parse("2019-09-09"), 0.177462220176329},
                      new object[] {DateTime.Parse("2019-12-09"), 0.175942390534375},
                      new object[] {DateTime.Parse("2020-03-09"), 0.173780768553609},
                      new object[] {DateTime.Parse("2020-06-09"), 0.170934456653393},
                      new object[] {DateTime.Parse("2020-09-07"), 0.168311664511899},
                      new object[] {DateTime.Parse("2020-12-07"), 0.166193895531733},
                      new object[] {DateTime.Parse("2021-03-08"), 0.164178932238519},
                      new object[] {DateTime.Parse("2021-06-07"), 0.162269954080914},
                      new object[] {DateTime.Parse("2021-09-07"), 0.160503208381284},
                      new object[] {DateTime.Parse("2021-12-07"), 0.158906658190162},
                      new object[] {DateTime.Parse("2022-03-07"), 0.157460807985296},
                      new object[] {DateTime.Parse("2022-06-07"), 0.156193897739533},
                      new object[] {DateTime.Parse("2022-09-07"), 0.154493955337133},
                      new object[] {DateTime.Parse("2022-12-07"), 0.153671168768697},
                      new object[] {DateTime.Parse("2023-03-07"), 0.153077485123154},
                      new object[] {DateTime.Parse("2023-06-07"), 0.152747541527948},
                      new object[] {DateTime.Parse("2023-09-07"), 0.152705450371269},
                      new object[] {DateTime.Parse("2023-12-07"), 0.1529616513809},
                      new object[] {DateTime.Parse("2024-03-07"), 0.153539827550864},
                      new object[] {DateTime.Parse("2024-06-07"), 0.1544792472953},
                      new object[] {DateTime.Parse("2024-09-09"), 0.155797951891306},
                      new object[] {DateTime.Parse("2024-12-09"), 0.157450951355181},
                      new object[] {DateTime.Parse("2025-03-07"), 0.159535758171424},
                      new object[] {DateTime.Parse("2025-06-10"), 0.162100509714344},
                      new object[] {DateTime.Parse("2025-09-08"), 0.163844682734835},
                      new object[] {DateTime.Parse("2025-12-08"), 0.167197032062001},
                      new object[] {DateTime.Parse("2026-03-09"), 0.170524936712443},
                      new object[] {DateTime.Parse("2026-06-09"), 0.173771409395992},
                      new object[] {DateTime.Parse("2026-09-07"), 0.176914047993042},
                      new object[] {DateTime.Parse("2026-12-07"), 0.179965471334},
                      new object[] {DateTime.Parse("2027-03-08"), 0.18288435388934},
                      new object[] {DateTime.Parse("2027-06-07"), 0.185661699293317},
                      new object[] {DateTime.Parse("2027-09-07"), 0.188254030460216},
                      new object[] {DateTime.Parse("2027-12-07"), 0.190624618449705},
                      new object[] {DateTime.Parse("2028-03-07"), 0.192773256161753},
                      new object[] {DateTime.Parse("2028-06-07"), 0.194666755889739},
                      new object[] {DateTime.Parse("2028-09-07"), 0.196256259395276},
                      new object[] {DateTime.Parse("2028-12-07"), 0.197513248631659},
                      new object[] {DateTime.Parse("2029-03-07"), 0.198436047215291},
                      new object[] {DateTime.Parse("2029-06-07"), 0.198986412806554},
                      new object[] {DateTime.Parse("2029-09-07"), 0.199123719959376},
                      new object[] {DateTime.Parse("2029-12-07"), 0.19882914344097},
                      new object[] {DateTime.Parse("2030-03-07"), 0.198074792250601},
                      new object[] {DateTime.Parse("2030-06-07"), 0.196795465643404},
                      new object[] {DateTime.Parse("2030-09-09"), 0.197238648546916},
                      new object[] {DateTime.Parse("2030-12-09"), 0.198999722663323},
                      new object[] {DateTime.Parse("2031-03-07"), 0.200785505721051},
                      new object[] {DateTime.Parse("2031-06-10"), 0.202547128217045},
                      new object[] {DateTime.Parse("2031-09-08"), 0.204240828604028},
                      new object[] {DateTime.Parse("2031-12-08"), 0.205900951499538},
                      new object[] {DateTime.Parse("2032-03-08"), 0.207513143564333},
                      new object[] {DateTime.Parse("2032-06-07"), 0.209080901684006},
                      new object[] {DateTime.Parse("2032-09-07"), 0.210585084278363},
                      new object[] {DateTime.Parse("2032-12-07"), 0.212006250653813},
                      new object[] {DateTime.Parse("2033-03-07"), 0.213365921887108},
                      new object[] {DateTime.Parse("2033-06-07"), 0.214656269738596},
                      new object[] {DateTime.Parse("2033-09-07"), 0.215849796369967},
                      new object[] {DateTime.Parse("2033-12-07"), 0.21693642033077},
                      new object[] {DateTime.Parse("2034-03-07"), 0.217931896424674},
                      new object[] {DateTime.Parse("2034-06-07"), 0.218824755946314},
                      new object[] {DateTime.Parse("2034-09-07"), 0.219593271006106},
                      new object[] {DateTime.Parse("2034-12-07"), 0.220229901524817},
                      new object[] {DateTime.Parse("2035-03-07"), 0.220742601029082},
                      new object[] {DateTime.Parse("2035-06-07"), 0.221116006453559},
                      new object[] {DateTime.Parse("2035-09-07"), 0.222965392668606},
                      new object[] {DateTime.Parse("2035-12-07"), 0.224395949422802},
                      new object[] {DateTime.Parse("2036-03-07"), 0.225848819927075},
                      new object[] {DateTime.Parse("2036-06-10"), 0.227263496280415},
                      new object[] {DateTime.Parse("2036-09-08"), 0.228637101804104},
                      new object[] {DateTime.Parse("2036-12-08"), 0.229995376305613},
                      new object[] {DateTime.Parse("2037-03-09"), 0.231338479975263},
                      new object[] {DateTime.Parse("2037-06-09"), 0.232643112823334},
                      new object[] {DateTime.Parse("2037-09-07"), 0.233916322054121},
                      new object[] {DateTime.Parse("2037-12-07"), 0.235163364066334},
                      new object[] {DateTime.Parse("2038-03-08"), 0.236375939762703},
                      new object[] {DateTime.Parse("2038-06-07"), 0.237559173329076},
                      new object[] {DateTime.Parse("2038-09-07"), 0.238698639699525},
                      new object[] {DateTime.Parse("2038-12-07"), 0.239782750351049},
                      new object[] {DateTime.Parse("2039-03-07"), 0.240831636175851},
                      new object[] {DateTime.Parse("2039-06-07"), 0.241838764097603},
                      new object[] {DateTime.Parse("2039-09-07"), 0.242785673461772},
                      new object[] {DateTime.Parse("2039-12-07"), 0.243673169757376},
                      new object[] {DateTime.Parse("2040-03-07"), 0.244507642654647},
                      new object[] {DateTime.Parse("2040-06-07"), 0.245281442699493},
                      new object[] {null, null}
                  };
            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", "TraderMid"}
                      };

            #endregion

            // Publish via web service
            var actualId = Ps.PublishCurveAdditional(surfaceProperties, publishProperties, data, inputs);
            Assert.AreEqual("Market.Live.RateVolatilityMatrix.AUD-LIBOR-BBA-CapFloor", actualId);
            var market = Ps.GetCurve(actualId, false);
            Market fpml = market.GetMarket();
            string actual = XmlSerializerHelper.SerializeToString(fpml);
            //string expected = Resources.PublishCapFloorVol_Expected;
            //Assert.AreEqual(expected, actual);
            DeleteMarket(actual);
        }

        private const int HashNa = -2146826246;

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void PublishSwaptionVolInvalidTest()
        {
            #region inputs

            var swaptionProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateATMVolatilityMatrix"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "LIBOR-BBA"},
                          new object[] {"MarketName", "Live"},
                          new object[] {"Instrument", "Swaption"},
                          // Need the following properties in the test only for XML comparison purposes
                          new object[] {"BuildDateTime", new DateTime(2010,9,7,14,18,22)}, 
                          new object[] {"BaseDate", new DateTime(2010,9,7)}
                      };

            object[][] data
                = {
                    new object[]{"Expiry","1Y","2Y","3Y","4Y","5Y","7Y","10Y","15Y","20Y"},
                    new object[]{"1M",0.173134247023696,0.184077990805836,0.188905171586698,0.182345674673924,0.179442017586033,0.175034313302459,0.172556305236928,0.17025352108125,0.171631333269796},
                    new object[]{"2M",0.179277716343017,0.182174188861554,0.188653544650265,0.182133684218621,0.180733954289418,0.177769676925,0.175283563383322,0.172975021589566,0.174410731184393},
                    new object[]{"3M",0.182089980429339,0.183332284198686,0.188019706630795,0.182035679640915,0.179086344736561,0.180579145171599,0.172230384569876,0.16997948184624,0.171416852480155},
                    new object[]{"6M",0.200280122803084,0.198404375356211,0.19330116186503,0.184449496822683,0.181528524454177,0.177939857617636,0.172787463379575,0.171112246293098,0.172952396642254},
                    new object[]{"1Y",0.217218062423376,0.212440339730523,0.206818984581716,0.194825289028441,0.18786125097534,0.177850189733278,0.171542486014626,0.170533782787442,0.172870188265416},
                    new object[]{"2Y",0.213951836421849,0.204717658394046,0.195444500781416,0.182571952719406,0.177161814989823,0.169377783319335,0.164893517522823,0.165283103739824,0.168562461268347},
                    new object[]{"3Y",0.194658470779212,0.186385631023403,0.179760621231912,0.17085151106272,0.167684145614881,0.161193045082049,0.156091151729022,0.157466958215653,0.16163725459035},
                    new object[]{"5Y",0.162525825284052,0.160691313766798,0.159829593674324,0.154742282352831,0.152465666894163,0.149147930104837,0.148255797984937,0.151426378255815,0.157599135003681},
                    new object[]{"7Y",0.159659781661822,0.157689978594064,0.154801478954388,0.149200829346026,0.147514443867689,0.14605406456858,0.147778807110965,0.152767418877533,0.159923596145401},
                    new object[]{"10Y",HashNa,0.14906260069021,0.147122219889546,0.144420892693253,0.142858268689848,0.145706063256514,0.148453408815127,0.157619086923414,0.166637987047625}
                  };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", "TraderMid"}
                      };

            #endregion

            // Publish via web service
            // This should fail
            Ps.PublishCurve(swaptionProperties, publishProperties, data);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void PublishCapFloorVolInvalidInputsTest()
        {
            #region inputs

            var surfaceProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateVolatilityMatrix"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "LIBOR-BBA"},
                          new object[] {"MarketName", "Live"},
                          new object[] {"Instrument", "CapFloor"},
                          new object[] {"StrikeQuoteUnits", "DecimalRate"},
                          // Need the following properties in the test only for XML comparison purposes
                          new object[] {"BuildDateTime", new DateTime(2010,9,7,14,18,22)}, 
                          new object[] {"BaseDate", new DateTime(2010,9,7)}
                      };

            object[][] inputs
                = {
                      new object[] {"AUD-Caplet-1Y", HashNa, 0.258917113332362},
                      new object[] {"AUD-Caplet-2Y", 0.0489047675330314, 0.252181607167987},
                      new object[] {"AUD-Caplet-3Y", 0.0494999996496402, 0.24595493086507},
                      new object[] {"AUD-Caplet-4Y", 0.0500767176395453, 0.235228761775413},
                      new object[] {"AUD-Caplet-5Y", 0.050908055135771, 0.22672862730331},
                      new object[] {"AUD-Caplet-7Y", 0.0519940295610645, 0.209829051906196},
                      new object[] {"AUD-Caplet-10Y", 0.0527829861943455, 0.197706060815361},
                      new object[] {"AUD-Caplet-15Y", 0.0536383983104803, 0.182761996168826},
                      new object[] {"AUD-Caplet-20Y", 0.0532931908550533, 0.183945839782503},
                      new object[] {"AUD-Caplet-25Y", 0.0520391302210335, 0.188378643241806},
                      new object[] {"AUD-Caplet-30Y", 0.0506331706871363, 0.193609458256038}
                  };

            object[][] data
                = {
                      new object[] {DateTime.Parse("2010-09-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2010-12-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-03-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-06-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-09-07"), 0.253606025847892},
                      new object[] {DateTime.Parse("2011-12-07"), 0.250657582889503},
                      new object[] {DateTime.Parse("2012-03-07"), 0.247857813993874},
                      new object[] {DateTime.Parse("2012-06-07"), 0.245362314572259},
                      new object[] {DateTime.Parse("2012-09-07"), 0.24396194632708},
                      new object[] {DateTime.Parse("2012-12-07"), 0.242681369711064},
                      new object[] {DateTime.Parse("2013-03-07"), 0.239680981323173},
                      new object[] {DateTime.Parse("2013-06-07"), 0.233915867350968},
                      new object[] {DateTime.Parse("2013-09-09"), 0.225180967697172},
                      new object[] {DateTime.Parse("2013-12-09"), 0.21698945730618},
                      new object[] {DateTime.Parse("2014-03-07"), 0.211047320868142},
                      new object[] {DateTime.Parse("2014-06-10"), 0.208768656780787},
                      new object[] {DateTime.Parse("2014-09-08"), 0.208730809964552},
                      new object[] {DateTime.Parse("2014-12-08"), 0.207029962602162},
                      new object[] {DateTime.Parse("2015-03-09"), 0.204130665748063},
                      new object[] {DateTime.Parse("2015-06-09"), 0.199692875407936},
                      new object[] {DateTime.Parse("2015-09-07"), 0.193752743474213},
                      new object[] {DateTime.Parse("2015-12-07"), 0.188009297591465},
                      new object[] {DateTime.Parse("2016-03-07"), 0.182933267922475},
                      new object[] {DateTime.Parse("2016-06-07"), 0.178756416240464},
                      new object[] {DateTime.Parse("2016-09-07"), 0.175389831055263},
                      new object[] {DateTime.Parse("2016-12-07"), 0.1736753841047},
                      new object[] {DateTime.Parse("2017-03-07"), 0.173469043393157},
                      new object[] {DateTime.Parse("2017-06-07"), 0.175052848911834},
                      new object[] {DateTime.Parse("2017-09-07"), 0.17638091320432},
                      new object[] {DateTime.Parse("2017-12-07"), 0.177178024517271},
                      new object[] {DateTime.Parse("2018-03-07"), 0.177932794687923},
                      new object[] {DateTime.Parse("2018-06-07"), 0.17859628828216},
                      new object[] {DateTime.Parse("2018-09-07"), 0.17879160950702},
                      new object[] {DateTime.Parse("2018-12-07"), 0.179024163932461},
                      new object[] {DateTime.Parse("2019-03-07"), 0.178930635076251},
                      new object[] {DateTime.Parse("2019-06-07"), 0.178427454575685},
                      new object[] {DateTime.Parse("2019-09-09"), 0.177462220176329},
                      new object[] {DateTime.Parse("2019-12-09"), 0.175942390534375},
                      new object[] {DateTime.Parse("2020-03-09"), 0.173780768553609},
                      new object[] {DateTime.Parse("2020-06-09"), 0.170934456653393},
                      new object[] {DateTime.Parse("2020-09-07"), 0.168311664511899},
                      new object[] {DateTime.Parse("2020-12-07"), 0.166193895531733},
                      new object[] {DateTime.Parse("2021-03-08"), 0.164178932238519},
                      new object[] {DateTime.Parse("2021-06-07"), 0.162269954080914},
                      new object[] {DateTime.Parse("2021-09-07"), 0.160503208381284},
                      new object[] {DateTime.Parse("2021-12-07"), 0.158906658190162},
                      new object[] {DateTime.Parse("2022-03-07"), 0.157460807985296},
                      new object[] {DateTime.Parse("2022-06-07"), 0.156193897739533},
                      new object[] {DateTime.Parse("2022-09-07"), 0.154493955337133},
                      new object[] {DateTime.Parse("2022-12-07"), 0.153671168768697},
                      new object[] {DateTime.Parse("2023-03-07"), 0.153077485123154},
                      new object[] {DateTime.Parse("2023-06-07"), 0.152747541527948},
                      new object[] {DateTime.Parse("2023-09-07"), 0.152705450371269},
                      new object[] {DateTime.Parse("2023-12-07"), 0.1529616513809},
                      new object[] {DateTime.Parse("2024-03-07"), 0.153539827550864},
                      new object[] {DateTime.Parse("2024-06-07"), 0.1544792472953},
                      new object[] {DateTime.Parse("2024-09-09"), 0.155797951891306},
                      new object[] {DateTime.Parse("2024-12-09"), 0.157450951355181},
                      new object[] {DateTime.Parse("2025-03-07"), 0.159535758171424},
                      new object[] {DateTime.Parse("2025-06-10"), 0.162100509714344},
                      new object[] {DateTime.Parse("2025-09-08"), 0.163844682734835},
                      new object[] {DateTime.Parse("2025-12-08"), 0.167197032062001},
                      new object[] {DateTime.Parse("2026-03-09"), 0.170524936712443},
                      new object[] {DateTime.Parse("2026-06-09"), 0.173771409395992},
                      new object[] {DateTime.Parse("2026-09-07"), 0.176914047993042},
                      new object[] {DateTime.Parse("2026-12-07"), 0.179965471334},
                      new object[] {DateTime.Parse("2027-03-08"), 0.18288435388934},
                      new object[] {DateTime.Parse("2027-06-07"), 0.185661699293317},
                      new object[] {DateTime.Parse("2027-09-07"), 0.188254030460216},
                      new object[] {DateTime.Parse("2027-12-07"), 0.190624618449705},
                      new object[] {DateTime.Parse("2028-03-07"), 0.192773256161753},
                      new object[] {DateTime.Parse("2028-06-07"), 0.194666755889739},
                      new object[] {DateTime.Parse("2028-09-07"), 0.196256259395276},
                      new object[] {DateTime.Parse("2028-12-07"), 0.197513248631659},
                      new object[] {DateTime.Parse("2029-03-07"), 0.198436047215291},
                      new object[] {DateTime.Parse("2029-06-07"), 0.198986412806554},
                      new object[] {DateTime.Parse("2029-09-07"), 0.199123719959376},
                      new object[] {DateTime.Parse("2029-12-07"), 0.19882914344097},
                      new object[] {DateTime.Parse("2030-03-07"), 0.198074792250601},
                      new object[] {DateTime.Parse("2030-06-07"), 0.196795465643404},
                      new object[] {DateTime.Parse("2030-09-09"), 0.197238648546916},
                      new object[] {DateTime.Parse("2030-12-09"), 0.198999722663323},
                      new object[] {DateTime.Parse("2031-03-07"), 0.200785505721051},
                      new object[] {DateTime.Parse("2031-06-10"), 0.202547128217045},
                      new object[] {DateTime.Parse("2031-09-08"), 0.204240828604028},
                      new object[] {DateTime.Parse("2031-12-08"), 0.205900951499538},
                      new object[] {DateTime.Parse("2032-03-08"), 0.207513143564333},
                      new object[] {DateTime.Parse("2032-06-07"), 0.209080901684006},
                      new object[] {DateTime.Parse("2032-09-07"), 0.210585084278363},
                      new object[] {DateTime.Parse("2032-12-07"), 0.212006250653813},
                      new object[] {DateTime.Parse("2033-03-07"), 0.213365921887108},
                      new object[] {DateTime.Parse("2033-06-07"), 0.214656269738596},
                      new object[] {DateTime.Parse("2033-09-07"), 0.215849796369967},
                      new object[] {DateTime.Parse("2033-12-07"), 0.21693642033077},
                      new object[] {DateTime.Parse("2034-03-07"), 0.217931896424674},
                      new object[] {DateTime.Parse("2034-06-07"), 0.218824755946314},
                      new object[] {DateTime.Parse("2034-09-07"), 0.219593271006106},
                      new object[] {DateTime.Parse("2034-12-07"), 0.220229901524817},
                      new object[] {DateTime.Parse("2035-03-07"), 0.220742601029082},
                      new object[] {DateTime.Parse("2035-06-07"), 0.221116006453559},
                      new object[] {DateTime.Parse("2035-09-07"), 0.222965392668606},
                      new object[] {DateTime.Parse("2035-12-07"), 0.224395949422802},
                      new object[] {DateTime.Parse("2036-03-07"), 0.225848819927075},
                      new object[] {DateTime.Parse("2036-06-10"), 0.227263496280415},
                      new object[] {DateTime.Parse("2036-09-08"), 0.228637101804104},
                      new object[] {DateTime.Parse("2036-12-08"), 0.229995376305613},
                      new object[] {DateTime.Parse("2037-03-09"), 0.231338479975263},
                      new object[] {DateTime.Parse("2037-06-09"), 0.232643112823334},
                      new object[] {DateTime.Parse("2037-09-07"), 0.233916322054121},
                      new object[] {DateTime.Parse("2037-12-07"), 0.235163364066334},
                      new object[] {DateTime.Parse("2038-03-08"), 0.236375939762703},
                      new object[] {DateTime.Parse("2038-06-07"), 0.237559173329076},
                      new object[] {DateTime.Parse("2038-09-07"), 0.238698639699525},
                      new object[] {DateTime.Parse("2038-12-07"), 0.239782750351049},
                      new object[] {DateTime.Parse("2039-03-07"), 0.240831636175851},
                      new object[] {DateTime.Parse("2039-06-07"), 0.241838764097603},
                      new object[] {DateTime.Parse("2039-09-07"), 0.242785673461772},
                      new object[] {DateTime.Parse("2039-12-07"), 0.243673169757376},
                      new object[] {DateTime.Parse("2040-03-07"), 0.244507642654647},
                      new object[] {DateTime.Parse("2040-06-07"), 0.245281442699493}
                  };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", "TraderMid"}
                      };

            #endregion

            // This should fail
            Ps.PublishCurveAdditional(surfaceProperties, publishProperties, data, inputs);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), AllowDerivedTypes = true)]
        public void PublishCapFloorVolInvalidDataTest()
        {
            #region inputs

            var surfaceProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateVolatilityMatrix"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "LIBOR-BBA"},
                          new object[] {"MarketName", "Live"},
                          new object[] {"Instrument", "CapFloor"},
                          new object[] {"StrikeQuoteUnits", "DecimalRate"},
                          // Need the following properties in the test only for XML comparison purposes
                          new object[] {"BuildDateTime", new DateTime(2010,9,7,14,18,22)}, 
                          new object[] {"BaseDate", new DateTime(2010,9,7)}
                      };

            object[][] inputs
                = {
                      new object[] {"AUD-Caplet-1Y", 0.0482432257833451, 0.258917113332362},
                      new object[] {"AUD-Caplet-2Y", 0.0489047675330314, 0.252181607167987},
                      new object[] {"AUD-Caplet-3Y", 0.0494999996496402, 0.24595493086507},
                      new object[] {"AUD-Caplet-4Y", 0.0500767176395453, 0.235228761775413},
                      new object[] {"AUD-Caplet-5Y", 0.050908055135771, 0.22672862730331},
                      new object[] {"AUD-Caplet-7Y", 0.0519940295610645, 0.209829051906196},
                      new object[] {"AUD-Caplet-10Y", 0.0527829861943455, 0.197706060815361},
                      new object[] {"AUD-Caplet-15Y", 0.0536383983104803, 0.182761996168826},
                      new object[] {"AUD-Caplet-20Y", 0.0532931908550533, 0.183945839782503},
                      new object[] {"AUD-Caplet-25Y", 0.0520391302210335, 0.188378643241806},
                      new object[] {"AUD-Caplet-30Y", 0.0506331706871363, 0.193609458256038}
                  };

            object[][] data
                = {
                      new object[] {DateTime.Parse("2010-09-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2010-12-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-03-07"), HashNa},
                      new object[] {DateTime.Parse("2011-06-07"), 0.258917113332362},
                      new object[] {DateTime.Parse("2011-09-07"), 0.253606025847892},
                      new object[] {DateTime.Parse("2011-12-07"), 0.250657582889503},
                      new object[] {DateTime.Parse("2012-03-07"), 0.247857813993874},
                      new object[] {DateTime.Parse("2012-06-07"), 0.245362314572259},
                      new object[] {DateTime.Parse("2012-09-07"), 0.24396194632708},
                      new object[] {DateTime.Parse("2012-12-07"), 0.242681369711064},
                      new object[] {DateTime.Parse("2013-03-07"), 0.239680981323173},
                      new object[] {DateTime.Parse("2013-06-07"), 0.233915867350968},
                      new object[] {DateTime.Parse("2013-09-09"), 0.225180967697172},
                      new object[] {DateTime.Parse("2013-12-09"), 0.21698945730618},
                      new object[] {DateTime.Parse("2014-03-07"), 0.211047320868142},
                      new object[] {DateTime.Parse("2014-06-10"), 0.208768656780787},
                      new object[] {DateTime.Parse("2014-09-08"), 0.208730809964552},
                      new object[] {DateTime.Parse("2014-12-08"), 0.207029962602162},
                      new object[] {DateTime.Parse("2015-03-09"), 0.204130665748063},
                      new object[] {DateTime.Parse("2015-06-09"), 0.199692875407936},
                      new object[] {DateTime.Parse("2015-09-07"), 0.193752743474213},
                      new object[] {DateTime.Parse("2015-12-07"), 0.188009297591465},
                      new object[] {DateTime.Parse("2016-03-07"), 0.182933267922475},
                      new object[] {DateTime.Parse("2016-06-07"), 0.178756416240464},
                      new object[] {DateTime.Parse("2016-09-07"), 0.175389831055263},
                      new object[] {DateTime.Parse("2016-12-07"), 0.1736753841047},
                      new object[] {DateTime.Parse("2017-03-07"), 0.173469043393157},
                      new object[] {DateTime.Parse("2017-06-07"), 0.175052848911834},
                      new object[] {DateTime.Parse("2017-09-07"), 0.17638091320432},
                      new object[] {DateTime.Parse("2017-12-07"), 0.177178024517271},
                      new object[] {DateTime.Parse("2018-03-07"), 0.177932794687923},
                      new object[] {DateTime.Parse("2018-06-07"), 0.17859628828216},
                      new object[] {DateTime.Parse("2018-09-07"), 0.17879160950702},
                      new object[] {DateTime.Parse("2018-12-07"), 0.179024163932461},
                      new object[] {DateTime.Parse("2019-03-07"), 0.178930635076251},
                      new object[] {DateTime.Parse("2019-06-07"), 0.178427454575685},
                      new object[] {DateTime.Parse("2019-09-09"), 0.177462220176329},
                      new object[] {DateTime.Parse("2019-12-09"), 0.175942390534375},
                      new object[] {DateTime.Parse("2020-03-09"), 0.173780768553609},
                      new object[] {DateTime.Parse("2020-06-09"), 0.170934456653393},
                      new object[] {DateTime.Parse("2020-09-07"), 0.168311664511899},
                      new object[] {DateTime.Parse("2020-12-07"), 0.166193895531733},
                      new object[] {DateTime.Parse("2021-03-08"), 0.164178932238519},
                      new object[] {DateTime.Parse("2021-06-07"), 0.162269954080914},
                      new object[] {DateTime.Parse("2021-09-07"), 0.160503208381284},
                      new object[] {DateTime.Parse("2021-12-07"), 0.158906658190162},
                      new object[] {DateTime.Parse("2022-03-07"), 0.157460807985296},
                      new object[] {DateTime.Parse("2022-06-07"), 0.156193897739533},
                      new object[] {DateTime.Parse("2022-09-07"), 0.154493955337133},
                      new object[] {DateTime.Parse("2022-12-07"), 0.153671168768697},
                      new object[] {DateTime.Parse("2023-03-07"), 0.153077485123154},
                      new object[] {DateTime.Parse("2023-06-07"), 0.152747541527948},
                      new object[] {DateTime.Parse("2023-09-07"), 0.152705450371269},
                      new object[] {DateTime.Parse("2023-12-07"), 0.1529616513809},
                      new object[] {DateTime.Parse("2024-03-07"), 0.153539827550864},
                      new object[] {DateTime.Parse("2024-06-07"), 0.1544792472953},
                      new object[] {DateTime.Parse("2024-09-09"), 0.155797951891306},
                      new object[] {DateTime.Parse("2024-12-09"), 0.157450951355181},
                      new object[] {DateTime.Parse("2025-03-07"), 0.159535758171424},
                      new object[] {DateTime.Parse("2025-06-10"), 0.162100509714344},
                      new object[] {DateTime.Parse("2025-09-08"), 0.163844682734835},
                      new object[] {DateTime.Parse("2025-12-08"), 0.167197032062001},
                      new object[] {DateTime.Parse("2026-03-09"), 0.170524936712443},
                      new object[] {DateTime.Parse("2026-06-09"), 0.173771409395992},
                      new object[] {DateTime.Parse("2026-09-07"), 0.176914047993042},
                      new object[] {DateTime.Parse("2026-12-07"), 0.179965471334},
                      new object[] {DateTime.Parse("2027-03-08"), 0.18288435388934},
                      new object[] {DateTime.Parse("2027-06-07"), 0.185661699293317},
                      new object[] {DateTime.Parse("2027-09-07"), 0.188254030460216},
                      new object[] {DateTime.Parse("2027-12-07"), 0.190624618449705},
                      new object[] {DateTime.Parse("2028-03-07"), 0.192773256161753},
                      new object[] {DateTime.Parse("2028-06-07"), 0.194666755889739},
                      new object[] {DateTime.Parse("2028-09-07"), 0.196256259395276},
                      new object[] {DateTime.Parse("2028-12-07"), 0.197513248631659},
                      new object[] {DateTime.Parse("2029-03-07"), 0.198436047215291},
                      new object[] {DateTime.Parse("2029-06-07"), 0.198986412806554},
                      new object[] {DateTime.Parse("2029-09-07"), 0.199123719959376},
                      new object[] {DateTime.Parse("2029-12-07"), 0.19882914344097},
                      new object[] {DateTime.Parse("2030-03-07"), 0.198074792250601},
                      new object[] {DateTime.Parse("2030-06-07"), 0.196795465643404},
                      new object[] {DateTime.Parse("2030-09-09"), 0.197238648546916},
                      new object[] {DateTime.Parse("2030-12-09"), 0.198999722663323},
                      new object[] {DateTime.Parse("2031-03-07"), 0.200785505721051},
                      new object[] {DateTime.Parse("2031-06-10"), 0.202547128217045},
                      new object[] {DateTime.Parse("2031-09-08"), 0.204240828604028},
                      new object[] {DateTime.Parse("2031-12-08"), 0.205900951499538},
                      new object[] {DateTime.Parse("2032-03-08"), 0.207513143564333},
                      new object[] {DateTime.Parse("2032-06-07"), 0.209080901684006},
                      new object[] {DateTime.Parse("2032-09-07"), 0.210585084278363},
                      new object[] {DateTime.Parse("2032-12-07"), 0.212006250653813},
                      new object[] {DateTime.Parse("2033-03-07"), 0.213365921887108},
                      new object[] {DateTime.Parse("2033-06-07"), 0.214656269738596},
                      new object[] {DateTime.Parse("2033-09-07"), 0.215849796369967},
                      new object[] {DateTime.Parse("2033-12-07"), 0.21693642033077},
                      new object[] {DateTime.Parse("2034-03-07"), 0.217931896424674},
                      new object[] {DateTime.Parse("2034-06-07"), 0.218824755946314},
                      new object[] {DateTime.Parse("2034-09-07"), 0.219593271006106},
                      new object[] {DateTime.Parse("2034-12-07"), 0.220229901524817},
                      new object[] {DateTime.Parse("2035-03-07"), 0.220742601029082},
                      new object[] {DateTime.Parse("2035-06-07"), 0.221116006453559},
                      new object[] {DateTime.Parse("2035-09-07"), 0.222965392668606},
                      new object[] {DateTime.Parse("2035-12-07"), 0.224395949422802},
                      new object[] {DateTime.Parse("2036-03-07"), 0.225848819927075},
                      new object[] {DateTime.Parse("2036-06-10"), 0.227263496280415},
                      new object[] {DateTime.Parse("2036-09-08"), 0.228637101804104},
                      new object[] {DateTime.Parse("2036-12-08"), 0.229995376305613},
                      new object[] {DateTime.Parse("2037-03-09"), 0.231338479975263},
                      new object[] {DateTime.Parse("2037-06-09"), 0.232643112823334},
                      new object[] {DateTime.Parse("2037-09-07"), 0.233916322054121},
                      new object[] {DateTime.Parse("2037-12-07"), 0.235163364066334},
                      new object[] {DateTime.Parse("2038-03-08"), 0.236375939762703},
                      new object[] {DateTime.Parse("2038-06-07"), 0.237559173329076},
                      new object[] {DateTime.Parse("2038-09-07"), 0.238698639699525},
                      new object[] {DateTime.Parse("2038-12-07"), 0.239782750351049},
                      new object[] {DateTime.Parse("2039-03-07"), 0.240831636175851},
                      new object[] {DateTime.Parse("2039-06-07"), 0.241838764097603},
                      new object[] {DateTime.Parse("2039-09-07"), 0.242785673461772},
                      new object[] {DateTime.Parse("2039-12-07"), 0.243673169757376},
                      new object[] {DateTime.Parse("2040-03-07"), 0.244507642654647},
                      new object[] {DateTime.Parse("2040-06-07"), 0.245281442699493}
                  };

            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", "TraderMid"}
                      };

            #endregion

            // This should fail
            Ps.PublishCurveAdditional(surfaceProperties, publishProperties, data, inputs);
        }

        #endregion

        #region LPM

        private static readonly object[][] QueryRateCurveProperties3M
            = {
                new object[] {"PricingStructureType", "RateCurve"},
                new object[] {"IndexTenor", "3M"},
                new object[] {"Currency", Currency},
                new object[] {"Index", RateCurveIndexName},
                new object[] {"Market", RateCurveMarketName}
            };

        private static readonly object[][] RateCurveProperties3M
            = {
                      new object[] {"PricingStructureType", "RateCurve"},
                      new object[] {"IndexTenor", "3M"},
                      new object[] {"Currency", Currency},
                      new object[] {"Index", RateCurveIndexName},
                      new object[] {"Algorithm", "CalypsoAlgo4"},
                      new object[] {"MarketName", RateCurveMarketName},
                      // Following property is here so that data doesn't change
                      // not needed in actual sheet
                      new object[] {"BaseDate", new DateTime(2009, 12, 18)},
                      new object[] {"BuildDateTime", new DateTime(2009, 12, 18, 13, 18, 26)}
                  };

        private static readonly object[][] RateCurveProperties6M
            = {
                      new object[] {"PricingStructureType", "RateBasisCurve"},
                      new object[] {"IndexTenor", "6M"},
                      new object[] {"Currency", Currency},
                      new object[] {"Index", RateCurveIndexName},
                      new object[] {"Algorithm", "CalypsoAlgo4"},
                      new object[] {"MarketName", RateCurveMarketName},
                      new object[] {"ReferenceCurveUniqueId",
                          $"Market.{RateCurveMarketName}.RateCurve.{Currency}-{RateCurveIndexName}-3M"
                      },
                      // Following property is here so that data doesn't change
                      // not needed in actual sheet
                      new object[] {"BaseDate", new DateTime(2009, 12, 18)},
                      new object[] {"BuildDateTime", new DateTime(2009, 12, 18, 13, 18, 26)}
                  };

        [TestMethod]
        public void PublishLpmCurve3MTest()
        {
            string result = Lpm3MPublish();
            Assert.AreEqual("Market.LiveUnitTest.RateCurve.AUD-BBR-BBSW-3M", result);
            DeleteMarket(result);
        }

        [TestMethod]
        public void PublishLpmCurve6MTest()
        {
            // First have to publish the base curve
            string baseCurve = Lpm3MPublish();
            string result = Lpm6MPublish();
            Assert.AreEqual("Market.LiveUnitTest.RateCurve.AUD-BBR-BBSW-6M", result);
            DeleteMarket(result);
            DeleteMarket(baseCurve);
        }

        private string Lpm3MPublish()
        {
            object[][] rateCurveValues
                = {
                      new object[] {"AUD-Deposit-1D", 0.045, 0},
                      new object[] {"AUD-Deposit-1M", 0.0461, 0},
                      new object[] {"AUD-Deposit-2M", 0.0466, 0},
                      new object[] {"AUD-Deposit-3M", 0.0472, 0},
                      new object[] {"AUD-IRFuture-IR-1", 0.04725, 0},
                      new object[] {"AUD-IRFuture-IR-2", 0.04635, 0},
                      new object[] {"AUD-IRFuture-IR-3", 0.0462407354, 0},
                      new object[] {"AUD-IRFuture-IR-4", 0.0461301136, 0},
                      new object[] {"AUD-IRFuture-IR-5", 0.0464151526, 0},
                      new object[] {"AUD-IRFuture-IR-6", 0.0469953159, 0},
                      new object[] {"AUD-IRFuture-IR-7", 0.0474211841, 0},
                      new object[] {"AUD-IRFuture-IR-8", 0.0476433137, 0},
                      new object[] {"AUD-IRSwap-3Y", 0.046975, 0},
                      new object[] {"AUD-IRSwap-4Y", 0.0480293333, 0},
                      new object[] {"AUD-IRSwap-5Y", 0.04885, 0},
                      new object[] {"AUD-IRSwap-6Y", 0.0497616667, 0},
                      new object[] {"AUD-IRSwap-7Y", 0.0504833333, 0},
                      new object[] {"AUD-IRSwap-8Y", 0.050925, 0},
                      new object[] {"AUD-IRSwap-9Y", 0.05125, 0},
                      new object[] {"AUD-IRSwap-10Y", 0.0515575, 0},
                      new object[] {"AUD-IRSwap-12Y", 0.0520575, 0},
                      new object[] {"AUD-IRSwap-15Y", 0.0525583333, 0},
                      new object[] {"AUD-IRSwap-20Y", 0.0521833333, 0},
                      new object[] {"AUD-IRSwap-25Y", 0.0509491667, 0},
                      new object[] {"AUD-IRSwap-30Y", 0.0495291667, 0},
                      new object[] {"AUD-IRSwap-40Y", 0.0475787276, 0}
                  };
            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", RateCurveMarketName}
                          //new object[] {"LPMCopy", true},
                      };
            // Publish via web service
            return Ps.PublishCurve(RateCurveProperties3M, publishProperties, rateCurveValues);
        }

        private string Lpm6MPublish()
        {
            object[][] rateCurveValues
                = {
                      new object[] {"AUD-Deposit-1D", 0.0462508238, 0},
                      new object[] {"AUD-Deposit-1M", 0.0473508238, 0},
                      new object[] {"AUD-Deposit-2M", 0.0478508238, 0},
                      new object[] {"AUD-Deposit-3M", 0.0484508238, 0},
                      new object[] {"AUD-BasisSwap-6M-6M", 0.0012508238, 0},
                      new object[] {"AUD-BasisSwap-1Y-6M", 0.000975, 0},
                      new object[] {"AUD-BasisSwap-2Y-6M", 0.00095, 0},
                      new object[] {"AUD-BasisSwap-3Y-6M", 0.00095, 0},
                      new object[] {"AUD-BasisSwap-4Y-6M", 0.000925, 0},
                      new object[] {"AUD-BasisSwap-5Y-6M", 0.0009, 0},
                      new object[] {"AUD-BasisSwap-6Y-6M", 0.000875, 0},
                      new object[] {"AUD-BasisSwap-7Y-6M", 0.00085, 0},
                      new object[] {"AUD-BasisSwap-8Y-6M", 0.0008333333, 0},
                      new object[] {"AUD-BasisSwap-9Y-6M", 0.0008166667, 0},
                      new object[] {"AUD-BasisSwap-10Y-6M", 0.0008, 0},
                      new object[] {"AUD-BasisSwap-12Y-6M", 0.00075, 0},
                      new object[] {"AUD-BasisSwap-15Y-6M", 0.000675, 0},
                      new object[] {"AUD-BasisSwap-20Y-6M", 0.0006, 0},
                      new object[] {"AUD-BasisSwap-25Y-6M", 0.000525, 0},
                      new object[] {"AUD-BasisSwap-30Y-6M", 0.0005, 0},
                      new object[] {"AUD-BasisSwap-40Y-6M", 0.0003612775, 0}
                  };
            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", RateCurveMarketName}
                          //new object[] {"LPMCopy", true},
                      };
            // Publish via web service
            return Ps.PublishCurve(RateCurveProperties6M, publishProperties, rateCurveValues);
        }

        [TestMethod]
        public void PublishLpmCurveBondFuturesTest()
        {
            var structureProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", "RateCurve"},
                          new object[] {CurveProp.IndexTenor, "310Y"},
                          new object[] {"Currency", Currency},
                          new object[] {"Index", "BondFuturesPay"},
                          new object[] {"Algorithm", "Base algorithm"},
                          new object[] {"MarketName", RateCurveMarketName},
                          //new object[] {CurveProp.IndexName, IndexName},
                          new object[] {"Validity", "Official"},
                          // Following property is here so that data doesn't change
                          // not needed in actual sheet
                          new object[] {"BuildDateTime", new DateTime(2009, 12, 18, 13, 18, 26)}
                      };
            var publishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", RateCurveMarketName}
                          //new object[] {"LPMCopy", true},
                      };
            object[][] values
                = {
                      new object[] {"AUD-IRFuture-IR-H0", 4.95, 0},
                      new object[] {"AUD-IRFuture-IR-M0", 5.60, 0},
                      new object[] {null, null, null}
                  };
            // Publish via web service
            string result = Ps.PublishCurve(structureProperties, publishProperties, values);
            Assert.AreEqual("Market.LiveUnitTest.RateCurve.AUD-BondFuturesPay-310Y", result);
            DeleteMarket(result);
        }

        [TestMethod]
        public void PublishLpmCapFloorVolMatrixTest()
        {
            const string marketName = "LPMUnitTest";
            // Need the 6M curve published
            string rateCurve = Lpm3MPublish();
            const string curveType = "LpmCapFloorCurve";
            //const string instrument = "CapFloor";
            var matrixProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", curveType},
                          new object[] {"CapFrequency", "3M"},
                          new object[] {"Currency", Currency},
                          new object[] {"Instrument", "AUD-Xibor-3M"},
                          new object[] {"IndexTenor", "3M"},
                          new object[] {"IndexName", "AUD-BBR-BBSW"},
                          new object[] {"MarketName", marketName},
                          new object[] {"CapStartLag", (double) 1},
                          new object[] {"Source", "SydSPU"},
                          new object[] {"Handle", "AUD SydSPU ATM Bootstrap Settings"},
                          new object[] {"ParVolatilityInterpolation", "CubicHermiteSpline"},
                          new object[] {"RollConvention", "MODFOLLOWING"},
                          new object[] {"Calculation Date", new DateTime(2009, 12, 18)},
                          // Following property is here so that data doesn't change
                          // not needed in actual sheet
                          new object[] {"BuildDateTime", new DateTime(2009, 12, 18, 13, 18, 26)}
                      };
            var matrixPublishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", marketName}
                          //new object[] {"LPMCopy", true},
                      };
            //object[][] values =
            //{
            //    new object[] {"Expiry", "PPD"},
            //    new object[] { "AUD-IRFUTUREOPTION-IR-1", 6.80},
            //    new object[] { "AUD-IRFUTUREOPTION-IR-2", 7.80},
            //    new object[] { "AUD-IRFUTUREOPTION-IR-3", 8.05},
            //    new object[] { "AUD-IRFUTUREOPTION-IR-4", 8.05},
            //    new object[] { "AUD-IRCap-2Y", 8.00},
            //    new object[] { "AUD-IRCap-3Y", 7.90},
            //    new object[] { "AUD-IRCap-4Y", 7.80},
            //    new object[] { "AUD-IRCap-5Y", 7.60},
            //    new object[] { "AUD-IRCap-7Y", 7.40},
            //    new object[] { "AUD-IRCap-10Y", 7.30},
            //    new object[] { "AUD-IRCap-15Y", 6.80},
            //    new object[] { "AUD-IRCap-20Y", 6.55},
            //    new object[] { "AUD-IRCap-25Y", 6.30},
            //    new object[] { "AUD-IRCap-30Y", 6.05}
            //object[][] values =
            //    {
            //        new object[] {"Expiry", "PPD", "Type"},
            //        new object[] {"85d", 6.80, "AUD-IRFUTUREOPTION-IR-1"},
            //        new object[] {"176d", 7.80, "AUD-IRFUTUREOPTION-IR-2"},
            //        new object[] {"267d", 8.05, "AUD-IRFUTUREOPTION-IR-3"},
            //        new object[] {"359d", 8.05, "AUD-IRFUTUREOPTION-IR-4"},
            //        new object[] {"2Y", 8.00, "AUD-IRCap-2Y"},
            //        new object[] {"3Y", 7.90, "AUD-IRCap-3Y"},
            //        new object[] {"4Y", 7.80, "AUD-IRCap-4Y"},
            //        new object[] {"5Y", 7.60, "AUD-IRCap-5Y"},
            //        new object[] {"7Y", 7.40, "AUD-IRCap-7Y"},
            //        new object[] {"10Y", 7.30, "AUD-IRCap-10Y"},
            //        new object[] {"15Y", 6.80, "AUD-IRCap-15Y"},
            //        new object[] {"20Y", 6.55, "AUD-IRCap-20Y"},
            //        new object[] {"25Y", 6.30, "AUD-IRCap-25Y"},
            //        new object[] {"30Y", 6.05, "AUD-IRCap-30Y" }
            //    };
            object[][] values =
            {
                new object[] {"Expiry", "PPD", "Type"},
                new object[] {"1d", 6.80, "ETO"},
                new object[] {"85d", 6.80, "ETO"},
                new object[] {"176d", 7.80, "ETO"},
                new object[] {"267d", 8.05, "ETO"},
                new object[] {"359d", 8.05, "ETO"},
                new object[] {"2Y", 8.00, "Cap/Floor"},
                new object[] {"3Y", 7.90, "Cap/Floor"},
                new object[] {"4Y", 7.80, "Cap/Floor"},
                new object[] {"5Y", 7.60, "Cap/Floor"},
                new object[] {"7Y", 7.40, "Cap/Floor"},
                new object[] {"10Y", 7.30, "Cap/Floor"}
            };
            // publish the matrix
            string result = Ps.PublishLpmCapFloorVolMatrix(matrixProperties, matrixPublishProperties, values, QueryRateCurveProperties3M);// RateCurveProperties3M
            Debug.Print(result);
            Assert.AreEqual(marketName + ".AUD-BBR-BBSW.CapFloor.AUD.SydSPU.2009.Week51", result);
            DeleteMarket(rateCurve);
            DeleteMarket(result);
        }

        [TestMethod]
        public void PublishLpmSwaptionVolMatrixTest()
        {
            // Need the 3M curve published
            Lpm3MPublish();
            const string curveType = "LPMSwaptionCurve";
            const string instrument = "Swaption";
            const string marketName = "LPMUnitTest";
            var matrixProperties
                = new[]
                      {
                          new object[] {"PricingStructureType", curveType},
                          new object[] {"MarketName", marketName},
                          new object[] {"Source", "SydSPU"},
                          new object[] {"Instrument", instrument},
                          new object[] {"Currency", Currency},
                          new object[] {CurveProp.IndexName, "Volatility"},
                          new object[] {"Index", "LIBOR-BBA"},
                          // Following property is here so that data doesn't change
                          // not needed in actual sheet
                          new object[] {"BuildDateTime", new DateTime(2009, 12, 18, 13, 18, 26)}
                      };
            var matrixPublishProperties
                = new[]
                      {
                          new object[] {"ExpiryIntervalInMins", 1},
                          new object[] {"MarketName", marketName}
                          //new object[] {"LPMCopy", true},
                      };
            object[][] values
                = {
                      new object[] {"x", "1Y", "2Y", "3Y", "4Y", "5Y", "7Y", "10Y", "15Y", "20Y"},
                      new object[] {"1M", 8.80, 9.30, 9.70, 9.70, 9.70, 9.70, 9.60, 9.60, 9.60},
                      new object[] {"2M", 9.00, 9.60, 10.00, 10.00, 9.90, 9.80, 9.70, 9.70, 9.70},
                      new object[] {"3M", 9.10, 9.80, 10.00, 10.00, 9.90, 9.80, 9.70, 9.70, 9.70},
                      new object[] {"6M", 9.20, 9.50, 9.60, 9.50, 9.30, 9.30, 9.30, 9.30, 9.30},
                      new object[] {"1Y", 9.60, 9.40, 9.00, 8.80, 8.75, 8.70, 8.65, 8.65, 8.65},
                      new object[] {"2Y", 9.40, 9.10, 8.50, 8.20, 8.10, 8.00, 7.95, 7.95, 7.95},
                      new object[] {"3Y", 8.60, 8.00, 7.70, 7.50, 7.40, 7.30, 7.20, 7.20, 7.20},
                      new object[] {"5Y", 7.20, 7.00, 6.80, 6.70, 6.60, 6.50, 6.40, 6.40, 6.40},
                      new object[] {"7Y", 6.70, 6.50, 6.20, 6.20, 6.10, 6.00, 5.90, 5.90, 5.90},
                      new object[] {"10Y", 6.20, 6.00, 5.90, 5.80, 5.70, 5.50, 5.40, 5.40, 5.40}
                  };
            // publish the matrix
            string result = Ps.PublishLpmSwaptionVolMatrix(matrixProperties, matrixPublishProperties, values, RateCurveProperties3M);
            Assert.AreEqual(marketName + ".Volatility.Swaption.AUD.SydSPU.2009.Week51", result);
            DeleteMarket(result);
        }

        #endregion

        private static void DeleteMarket(string id)
        {
            IExpression query = Expr.IsEQU(Expr.SysPropItemName, id);
            General.PricingStructures.Cache.DeleteObjects<Market>(query);
        }
    }
}