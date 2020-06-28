using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.ExcelAPI.Tests.Products
{
    
    [TestClass]
    public class FraSolverWrapperTests
    {

         [TestMethod()]
         public void GetObjectsTest()
         {
             object[,] data = new object[3,3];
             data[0, 0] = "Instrument";
             data[0, 1] = "Rate";
             data[0, 2] = "Guess";
             data[1, 0] = "USD-Deposit-1D";
             data[1, 1] = 0.0529;
             data[1, 2] = null;
             data[2, 0] = "USD-Deposit-TD";
             data[2, 1] = 0.0529;
             data[2, 2] = 3;
             List<double> res = FraSolverWrapper.GetObjects<double>(data, 1);
             List<object> guess = FraSolverWrapper.GetObjects<object>(data, 2);
         }

         [TestMethod()]
         public void GetFraGuessesTest()
         {
             List<object> fraGuesses = new List<object>();
             fraGuesses.Add("");
             fraGuesses.Add(3.2);
             fraGuesses.Add(4.5);
             fraGuesses.Add("");
             List<decimal> res = new List<decimal>();
             List<int> indecies = new List<int>();
             FraSolverWrapper.GetFraGuesses(fraGuesses, ref res, ref indecies);
         }


        [TestMethod()]
         public void GetFraEquivalentRatesTest()
        {
            object[,] properties = {
                                           {"MarketName", "Barra"},
                                           {"PricingStructureType", "RateCurve"},
                                           {"Currency", "USD"},
                                           {"IndexName", "LIBOR-ISDA"},
                                           {"IndexTenor", "3M"},
                                           {"Algorithm", "FastLinearZero"},
                                           {"Identifier", "RateCurve.USD-LIBOR-ISDA-3M"},
                                           {"CurveName", "USD-LIBOR-ISDA-3M"},
                                           {"IndexName", "LIBOR-ISDA-3M"},
                                           {"BaseDate", new DateTime(2009, 10, 7)}
                                       };

            object[] instruments = {   "Instrument",
                                           "USD-Deposit-1D",
                                           "USD-Deposit-TN",
                                           "USD-Deposit-1M",
                                           "USD-Deposit-2M",
                                           "USD-Deposit-3M",
                                           "USD-IRFuture-ED-U9",
                                           "USD-IRFuture-ED-Z9",
                                           "USD-IRFuture-ED-H0",
                                           "USD-IRFuture-ED-M0",
                                           "USD-IRFuture-ED-U0"
                                       };

            object[] rates = {
                                      "rate",
                                       0.0023500,
                                       0.0023500,
                                       0.002725000,
                                       0.003068800,
                                       0.0042500,
                                       0.0044745,
                                       0.0061450,
                                       0.0088236,
                                       0.0127793,
                                       0.0170081
                                  };

            object[] guesses = {   "Guess",
                                   "", "", 0.00458, 0.00536, "", "",
                                   "", "", "", "", ""
                               };

            object[,] dataTable = new object[11,3];

            for(int i= 0; i < 11; ++i)
            {
                for( int j = 0; j < 3; ++j)
                {
                    if (j % 3 == 0)
                        dataTable[i, j] = instruments[i];
                    if (j % 3 == 1)
                        dataTable[i, j] = rates[i];
                    if( j % 3 == 2)
                        dataTable[i, j] = guesses[i];
                }
            }
            double[] fraRates = FraSolverWrapper.CalculateFraEquivalents(properties, dataTable);
        }
    }
}
