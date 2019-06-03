using System;
using System.Collections.Generic;
using Orion.Analytics.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.Analytics.Tests.Rates
{
    [TestClass]
    public class CrossCurrencySwapTests
    {
        #region Private Fields

        private List<decimal> floatingYearFractions;
        private List<decimal> floatingCashflows;
        private List<decimal> floatingDiscountFactors;

        private List<decimal> fixedYearFractions;
        private List<decimal> fixedCashflows;
        private List<decimal> fixedDiscountFactors; 

        #endregion

        #region Setup

        [TestInitialize]
        public void Initialisation()
        {
            #region Initialise Floating Leg Input Data
            decimal[] floatingYearFractionArray = { 0.26111111m,0.25277778m,0.25277778m,0.25555556m,
                                                    0.25m,0.25277778m,0.25277778m,0.25555556m,
                                                    0.25m,0.25555556m,0.25277778m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m,
                                                    0.25555556m,0.26111111m,0.24444444m,0.25277778m,
                                                    0.26111111m,0.25277778m,0.25277778m,0.25555556m,
                                                    0.25m,0.25555556m,0.25m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m };

            floatingYearFractions = new List<decimal>(floatingYearFractionArray.Length);
            floatingYearFractions.AddRange(floatingYearFractionArray);


            decimal[] floatingCashflowArray = { 1553.46m,1633.96m,2483.08m,3211.28m,
                                                4053.23m,5040.15m,5992.54m,6907.73m,
                                                7562.34m,8421.02m,8991.87m,9457.90m,
                                                9926.61m,9851.25m,10197.00m,10866.80m,
                                                10762.46m,11246.78m,11469.53m,12054.20m,
                                                11253.33m,11858.32m,11436.01m,12106.33m,
                                                11725.59m,11604.12m,11850.69m,12232.34m,
                                                11477.69m,11930.17m,11864.55m,12193.52m,
                                                11737.68m,11891.24m,11779.80m,12057.15m,
                                                11994.78m,12129.21m,11994.02m,12254.90m };

            floatingCashflows = new List<decimal>(floatingCashflowArray.Length);
            floatingCashflows.AddRange(floatingCashflowArray);


            decimal[] floatingDiscountFactorArray = {1.0m, 0.99841522m,0.99679246m,0.99432711m,0.99114816m,
                                                    0.98715823m,0.98221354m,0.976367m,0.96966417m,
                                                    0.96239311m,0.95435646m,0.94585149m,0.93698953m,
                                                    0.92777983m,0.91872919m,0.90945548m,0.89967884m,
                                                    0.89009918m,0.88019977m,0.87021877m,0.85985393m,
                                                    0.85028539m,0.84032059m,0.8308277m,0.82086252m,
                                                    0.81134897m,0.80204365m,0.7926502m,0.78306971m,
                                                    0.77418663m,0.76505934m,0.75608869m,0.74698037m,
                                                    0.73831427m,0.72963797m,0.72114305m,0.7125517m,
                                                    0.7041061m,0.6956682m,0.68742323m,0.67910092m};


            floatingDiscountFactors = new List<decimal>(floatingDiscountFactorArray.Length);
            floatingDiscountFactors.AddRange(floatingDiscountFactorArray);

            #endregion

            #region Initialise Fixed Leg Input Data
            decimal[] fixedYearFractionArray = { 0.26027397m,0.24657534m,0.25205479m,0.24657534m,
                                                 0.25205479m,0.24931507m,0.24657534m,0.24931507m,
                                                 0.25205479m,0.24931507m,0.24931507m,0.24931507m,
                                                 0.25205479m,0.25205479m,0.24657534m,0.24931507m,
                                                 0.25205479m,0.25205479m,0.24657534m,0.24931507m,
                                                 0.25205479m,0.25753425m,0.25205479m,0.23835616m,
                                                 0.26027397m,0.24657534m,0.24931507m,0.24931507m,
                                                 0.25205479m,0.24931507m,0.24657534m,0.24931507m,
                                                 0.25205479m,0.25205479m,0.24657534m,0.24931507m,
                                                 0.25205479m,0.25205479m,0.24657534m,0.24931507m };

            fixedYearFractions = new List<decimal>(fixedYearFractionArray.Length);
            fixedYearFractions.AddRange(fixedYearFractionArray);



            decimal[] fixedCashflowArray = {13620.1762m,12903.32482m,13190.06526m,12903.32482m,
                                            13190.06526m,13046.6953m,12903.32482m,13046.6953m,
                                            13190.06526m,13046.6953m,13046.6953m,13046.6953m,
                                            13190.06526m,13190.06526m,12903.32482m,13046.6953m,
                                            13190.06526m,13190.06526m,12903.32482m,13046.6953m,
                                            13190.06526m,13476.80623m,13190.06526m,12473.21388m,
                                            13620.1762m,12903.32482m,13046.6953m,13046.6953m,
                                            13190.06526m,13046.6953m,12903.32482m,13046.6953m,
                                            13190.06526m,13190.06526m,12903.32482m,13046.6953m,
                                            13190.06526m,13190.06526m,12903.32482m,13046.6953m};

            fixedCashflows = new List<decimal>(fixedCashflowArray.Length);
            fixedCashflows.AddRange(fixedCashflowArray);

            //List<decimal> fixedCashflows = new List<decimal>(40);
            //for (int i = 0; i < 40; ++i)
            //    fixedCashflows.Add(52330.151m);


            decimal[] fixedDiscountFactorArray = { 1.0m, 0.99027518m,0.98271311m,0.97435044m,0.96421236m,
                                                0.95349128m,0.94199884m,0.92993055m,0.917154m,
                                                0.90355519m,0.88925513m,0.87435515m,0.85891584m,
                                                0.84491695m,0.83067313m,0.81652154m,0.80204904m,
                                                0.7889026m,0.77572821m,0.76282154m,0.74978594m,
                                                0.7380862m,0.726232m,0.71472898m,0.70395458m,
                                                0.69287932m,0.68251371m,0.67215744m,0.66193918m,
                                                0.65229242m,0.64289524m,0.6337407m,0.62462314m,
                                                0.61555291m,0.60662104m,0.59801509m,0.58944841m,
                                                0.58133153m,0.57335512m,0.56568541m,0.5580702m };


            fixedDiscountFactors = new List<decimal>(fixedDiscountFactorArray.Length);
            fixedDiscountFactors.AddRange(fixedDiscountFactorArray);



            #endregion

        }

        [TestMethod]
        public void FloatingPaymentStreamConstructorTest()
        {
            PaymentStream.Direction direction = PaymentStream.Direction.Payer;
            decimal notional = 1000000.0m;

            List<decimal> notionals = new List<decimal>();
            for (int i = 0; i < 40; ++i)
                notionals.Add(notional);

            DateTime referenceDate = new DateTime(2009, 7, 1);


            FloatingPaymentStream floatingPaymentStream = new FloatingPaymentStream(direction,
                                                                                    notionals,
                                                                                    floatingYearFractions,
                                                                                    floatingCashflows,
                                                                                    floatingDiscountFactors,
                                                                                    0.0m);

            #region expected forward rates

            decimal[] expectedForwardRates = { 0.59494m/100.0m,0.6464m/100.0m,0.98232m/100.0m,1.25659m/100.0m,
                                                1.62129m/100.0m,1.99391m/100.0m,2.37068m/100.0m,2.70303m/100.0m,
                                                3.02493m/100.0m,3.29518m/100.0m,3.55722m/100.0m,3.74159m/100.0m,
                                                3.88433m/100.0m,3.85484m/100.0m,4.0788m/100.0m,4.29895m/100.0m,
                                                4.2114m/100.0m,4.40091m/100.0m,4.58781m/100.0m,4.76869m/100.0m,
                                                4.40348m/100.0m,4.54149m/100.0m,4.67837m/100.0m,4.78932m/100.0m,
                                                4.49065m/100.0m,4.59064m/100.0m,4.68818m/100.0m,4.78657m/100.0m,
                                                4.59108m/100.0m,4.66833m/100.0m,4.74582m/100.0m,4.82381m/100.0m,
                                                4.59301m/100.0m,4.6531m/100.0m,4.71192m/100.0m,4.76986m/100.0m,
                                                4.69361m/100.0m,4.74621m/100.0m,4.79761m/100.0m,4.84809m/100.0m };

            #endregion
            List<decimal> calculatedForwardRates = new List<decimal>();
            calculatedForwardRates.AddRange(floatingPaymentStream.ForwardRates);

            int len = floatingPaymentStream.ForwardRates.Count ;
            for (int i = 0; i < len ; ++i)
            {
                Assert.AreEqual(Convert.ToDouble(calculatedForwardRates[i]), Convert.ToDouble(expectedForwardRates[i]), 5e-3);
            } 



        }

        [TestMethod]
        public void FixedPaymentStreamLevelFunctionTest()
        {
            PaymentStream.Direction direction = PaymentStream.Direction.Payer;
            decimal notional = 1239771.88m;
            List<decimal> notionals = new List<decimal>(40);
            for (int i = 0; i < 40; ++i)
                notionals.Add(notional);

            DateTime referenceDate = new DateTime(2009, 7, 1);


            FixedPaymentStream fixedPaymentStream = new FixedPaymentStream(direction,
                                                                           notionals,
                                                                           fixedYearFractions,
                                                                           fixedCashflows,
                                                                           fixedDiscountFactors,
                                                                           0.0m);

            decimal calculatedLevelFunction = fixedPaymentStream.LevelFunction();
           // decimal calculatedAccruedNPV = fixedPaymentStream.AccruedNPV();
                //fixedPaymentStream.AccruedNPV(4.22095m/100.0m);

            //double expectedAccruedNPV = -397825.73;

            double expectedLevelFunction = -7.60223072;

            Assert.AreEqual(Convert.ToDouble(calculatedLevelFunction), expectedLevelFunction, 5e-3);
           // Assert.AreEqual(Convert.ToDouble(calculatedAccruedNPV), expectedAccruedNPV, 0.2);

        } 

        #endregion

        [TestMethod]
        public void CrossCurrencyIRSwapTest()
        {
            #region Receiver Leg of the swap
            PaymentStream.Direction direction = PaymentStream.Direction.Receiver;
            decimal notional = 1000000.0m;
            List<decimal> notionals = new List<decimal>(40);
            for (int i = 0; i < 40; ++i)
                notionals.Add(notional);

            DateTime referenceDate = new DateTime(2009, 7, 1);


            FloatingPaymentStream floatingPaymentStream = new FloatingPaymentStream(direction,
                                                                                    notionals,
                                                                                    floatingYearFractions,
                                                                                    floatingCashflows,
                                                                                    floatingDiscountFactors,
                                                                                    0.0m);
            #endregion

            #region Payer Leg of the swap
            PaymentStream.Direction dir = PaymentStream.Direction.Payer;
            decimal payerNotional = 1239771.88m;
            List<decimal> payerNotionals = new List<decimal>(40);
            for (int i = 0; i < 40; ++i)
                payerNotionals.Add(payerNotional);

            DateTime payerReferenceDate = new DateTime(2009, 7, 1);


            FixedPaymentStream fixedPaymentStream = new FixedPaymentStream(dir,
                                                                           payerNotionals,
                                                                           fixedYearFractions,
                                                                           fixedCashflows,
                                                                           fixedDiscountFactors,
                                                                           0.0m);

            #endregion


            CrossCurrencyIRSwap swap = new CrossCurrencyIRSwap(fixedPaymentStream, floatingPaymentStream, 0.8066m, PaymentStream.Direction.Payer);
            decimal ber = swap.BreakEvenRate(PaymentStream.Direction.Payer);


        }


        [TestMethod]
        public void CrossCurrencyIRSwapTest1()
        {
            #region Floating Leg input data

            List<decimal> floatingNotionals = new List<decimal>();
            for( int i = 0; i < 40; ++i)
                floatingNotionals.Add(1000000.0m);

            decimal[] floatingCashflowArray = { 1413.758339m,1357.325m,2105.335574m,2643.196134m,
                                        3435.867838m,4208.025m,5167.232823m,6026.930053m,
                                        6967.466788m,7659.166734m,8360.802018m,8885.113689m,
                                        9413.389053m,9677.224613m,10084m,10805.54232m,
                                        10317.46796m,10808.16019m,11046.7m,11637.15594m,
                                        11250.75686m,11903.5855m,11520.39757m,12271.52705m,
                                        11829.69033m,11489.875m,11890.38872m,12168.57066m,
                                        11778.58132m,11876.46066m,11962.925m,12318.87233m,
                                        11912.36465m,12093.83465m,12004.925m,12308.25566m,
                                        11932.40021m,12075.81799m,11950.5m,12219.1515m};

            List<decimal> floatingCashflows = new List<decimal>();
            floatingCashflows.AddRange(floatingCashflowArray);


            decimal[] floatingDFArray = {   0.99996697m, 0.99855634m,0.99721368m,0.99511863m,0.99249526m,
                                            0.98909686m,0.98496355m,0.97990017m,0.97402975m,
                                            0.96729018m,0.959942m,0.95198266m,0.94359868m,
                                            0.93479904m,0.92583948m,0.91659652m,0.90679809m,
                                            0.89753777m,0.88794078m,0.87823913m,0.8681365m,
                                            0.85847797m,0.84837922m,0.83872383m,0.82855617m,
                                            0.81886731m,0.80956909m,0.80005611m,0.79043761m,
                                            0.78123576m,0.77206787m,0.76294086m,0.75365666m,
                                            0.74478451m,0.73588485m,0.72715541m,0.71831421m,
                                            0.70984407m,0.7013744m,0.69309162m,0.68472485m };

            List<decimal> floatingDFs = new List<decimal>();
            floatingDFs.AddRange(floatingDFArray);


            decimal[] floatingAccrualFactorsArray = {0.26388889m,0.25m,0.25277778m,0.25277778m,
                                                    0.25555556m,0.25m,0.25277778m,0.25277778m,
                                                    0.25555556m,0.25277778m,0.25277778m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m,
                                                    0.25555556m,0.26111111m,0.24444444m,0.25277778m,
                                                    0.26388889m,0.25m,0.25277778m,0.25277778m,
                                                    0.25555556m,0.25277778m,0.25m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m,
                                                    0.25555556m,0.25555556m,0.25m,0.25277778m};

            List<decimal> floatingAccrualFactors = new List<decimal>();
            floatingAccrualFactors.AddRange(floatingAccrualFactorsArray);

            #endregion

            #region Fixed Leg input data
            List<decimal> fixedNotionals = new List<decimal>();
            for( int i = 0; i < 40; ++i)
                fixedNotionals.Add(1251486.140m);

            decimal[] fixedCashflowArray = {  12769.54216m,12268.9029m,12169.50037m,12046.9249m,
                                                11920.08957m,11782.57773m,11636.61242m,11483.93456m,
                                                11324.49385m,11279.68822m,10985.172m,10807.51677m,
                                                10755.52554m,10581.4098m,10181.80822m,10119.20911m,
                                                10066.37768m,9901.496597m,9527.773713m,9471.434113m,
                                                9431.287684m,9486.829505m,8751.219051m,8911.522481m,
                                                9061.257678m,8638.242213m,8505.583569m,8374.380285m,
                                                8254.568021m,8224.448529m,7931.557441m,7904.578399m,
                                                7876.463967m,7763.082748m,7487.306473m,7462.646259m,
                                                7441.192267m,7339.380491m,7083.929521m,7066.285688m };

            List<decimal> fixedCashflows = new List<decimal>();
            fixedCashflows.AddRange(fixedCashflowArray);


            decimal[] fixedDFArray = {0.99998157m, 0.99049958m,0.98303994m,0.97507536m,0.96525406m,
                                        0.95509144m,0.94407337m,0.93237797m,0.92014473m,
                                        0.90736962m,0.89395592m,0.88018162m,0.86594708m,
                                        0.85241414m,0.83861484m,0.82487708m,0.81079676m,
                                        0.79779669m,0.78472927m,0.77189061m,0.7588941m,
                                        0.74746352m,0.73586825m,0.72509133m,0.71403145m,
                                        0.70285777m,0.692135m,0.68150579m,0.67099319m,
                                        0.66139329m,0.65181717m,0.64257348m,0.63335054m,
                                        0.62423814m,0.61525227m,0.60658258m,0.59794094m,
                                        0.58974129m,0.58167234m,0.57390308m,0.56618274m };

            List<decimal> fixedDFs = new List<decimal>();
            fixedDFs.AddRange(fixedDFArray);

            decimal[] fixedAccrualFactorsArray = {0.25753425m,0.24931507m,0.24931507m,0.24931507m,
                                                    0.24931507m,0.24931507m,0.24931507m,0.24931507m,
                                                    0.24931507m,0.25205479m,0.24931507m,0.24931507m,
                                                    0.25205479m,0.25205479m,0.24657534m,0.24931507m,
                                                    0.25205479m,0.25205479m,0.24657534m,0.24931507m,
                                                    0.25205479m,0.25753425m,0.24109589m,0.24931507m,
                                                    0.25753425m,0.24931507m,0.24931507m,0.24931507m,
                                                    0.24931507m,0.25205479m,0.24657534m,0.24931507m,
                                                    0.25205479m,0.25205479m,0.24657534m,0.24931507m,
                                                    0.25205479m,0.25205479m,0.24657534m,0.24931507m};

            List<decimal> fixedAccrualFactors = new List<decimal>();
            fixedAccrualFactors.AddRange(fixedAccrualFactorsArray);
            #endregion


            FloatingPaymentStream floatingLeg = new FloatingPaymentStream(PaymentStream.Direction.Receiver,
                                                                           floatingNotionals, floatingAccrualFactors,
                                                                           floatingCashflows, floatingDFs,
                                                                           1000000.0m);

            FixedPaymentStream fixedLeg = new FixedPaymentStream(PaymentStream.Direction.Payer,
                                                                 fixedNotionals, fixedAccrualFactors,
                                                                 fixedCashflows, fixedDFs,
                                                                 1251486.14m );


            CrossCurrencyIRSwap swap = new CrossCurrencyIRSwap(fixedLeg, floatingLeg, 0.799064724m, PaymentStream.Direction.Payer);
            decimal ber = swap.BreakEvenRate(PaymentStream.Direction.Payer);                                                              

        } 


    }
}
