using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace National.QRSC.QRLib.Tests.Products
{
    /// <summary>
    ///This is a test class for ProductsTest and is intended
    ///to contain all ProductsTest Unit Tests
    ///</summary>
    [TestClass]
    public class ProductsTest
    {
        /// <summary>
        ///A test for CreateIRProduct
        ///</summary>
        [TestMethod]
        public void CreateIRProductTest()
        {
            object[,] productParameters = {
                                              {"ProductType", "FRA"}, 
                                              {"ExpiryTenor", 3}, 
                                              {"TerminationTenor", 12},
                                              {"Currency", "AUD"},
                                              {"Notional", 1000000},
                                              {"FixedRate", 0.02}
                                          };

            string actual = QRLib.Products.CreateIRProduct(productParameters);

            Assert.AreEqual("FRA-3M-12M", actual);
        }

        /// <summary>
        ///A test for FlexEVal
        ///</summary>
        [TestMethod]
        public void FlexEValTest()
        {
            object[] args = GetFlexEValFraArgs();
            object[] expected = GetFlexEValFraExpected();
            object[] actual = QRLib.Products.FlexEVal(args);
            CollectionAssert.AreEqual(expected, actual);
        }

        private object[] GetFlexEValFraExpected()
        {
            object[] result = new object[] {System.Convert.ToDateTime("7/sep/2009"),
                                            System.Convert.ToDateTime("7/oct/2009"),
                                            System.Convert.ToDateTime("7/jan/2010"),
                                            0.252054794520548M,
                                            0.997661200950066M,
                                            0.990522937313789M,
                                            0.0285912461103725439133035018M,
                                            1724.8813315773440473185903785M,
                                            0.2496660554325167337272842364M,
                                            24.966605543251673372728423637M,
                                            0.9888133532467675248270068896M };
            return result;
        }

        private object[] GetFlexEValFraArgs()
        {
            object[,] discountCurve = new object[,] { { "DiscountCurve", null },
                                                      { "Date","Discount"},
                                                      { System.Convert.ToDateTime("7/sep/2009"),1.0m},
                                                      { System.Convert.ToDateTime("6/dec/2009"),0.993m},
                                                      { System.Convert.ToDateTime("6/03/2010"),0.986049m},
                                                      { System.Convert.ToDateTime("4/06/2010"),0.979146657m},
                                                      { System.Convert.ToDateTime("2/09/2010"),0.97229263m},
                                                      { System.Convert.ToDateTime("1/12/2010"),0.965486582m},
                                                      { System.Convert.ToDateTime("1/03/2011"),0.958728176m},
                                                      { System.Convert.ToDateTime("30/05/2011"),0.952017079m},
                                                      { System.Convert.ToDateTime("28/08/2011"),0.945352959m},
                                                      { System.Convert.ToDateTime("26/11/2011"),0.938735488m},
                                                      { System.Convert.ToDateTime("24/02/2012"),0.93216434m},
                                                      { System.Convert.ToDateTime("24/05/2012"),0.92563919m},
                                                      { System.Convert.ToDateTime("22/08/2012"),0.919159715m},
                                                      { System.Convert.ToDateTime("20/11/2012"),0.912725597m},
                                                      { System.Convert.ToDateTime("18/02/2013"),0.906336518m},
                                                      { System.Convert.ToDateTime("19/05/2013"),0.899992162m},
                                                      { System.Convert.ToDateTime("17/08/2013"),0.893692217m},
                                                      { System.Convert.ToDateTime("15/11/2013"),0.887436372m},
                                                      { System.Convert.ToDateTime("13/02/2014"),0.881224317m},
                                                      { System.Convert.ToDateTime("14/05/2014"),0.875055747m},
                                                      { System.Convert.ToDateTime("12/08/2014"),0.868930357m},
                                                      { System.Convert.ToDateTime("10/11/2014"),0.862847844m},
                                                      { System.Convert.ToDateTime("8/02/2015"),0.856807909m} };

            object[,] valuationParameters = new object[,] {{"ValuationParameters",null},
                                                           {"ValuationDate",System.Convert.ToDateTime("7/09/2009")}};

            object[,] productParameters = new object[,] {{"ProductParameters", null},
                                                         {"ProductType","FRA"},
                                                         {"ExpiryTenor",1},
                                                         {"TerminationTenor",4},
                                                         {"Currency","AUD"},
                                                         {"Notional",1000000},
                                                         {"FixedRate",0.0355}};

            object[,] metrics = new object[,] {{"Metrics"},
                                               {"ValuationDate"},
                                               {"ExpiryDate"},
                                               {"TerminationDate"},
                                               {"YearFraction"},
                                               {"StartDiscountFactor"},
                                               {"EndDiscountFactor"},
                                               {"FraRate"},
                                               {"NPV"},
                                               {"AccrualFactor"},
                                               {"DeltaR"},
                                               {"DiscountFactorAtMaturity"}};


            object[] result = new object[] { discountCurve, valuationParameters, productParameters, metrics };
            return result;
        }
    }
}