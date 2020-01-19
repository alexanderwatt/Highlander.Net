/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Equities.Tests
{
    [TestClass]
    public class AmericanPricerFixture
    {

        [TestMethod]
        public void NoDivNoZero()
        {
            DivList myDiv = new DivList {DivPoints = 1};
            myDiv.MakeArrays();
              myDiv.SetD(0, 0.0, 1.0);
              ZeroCurve myZero = new ZeroCurve {RatePoints = 1};
              myZero.MakeArrays();
              myZero.SetR(0, 0.0, 1.0);
              //create the tree
              DiscreteTree myTree = new DiscreteTree {GridSteps = 100, Tau = 1.0, Sig = 0.25, Spot = 100.0};
              //myTree.flatFlag = false;
              myTree.MakeGrid(myZero, myDiv);
              //create pricer
              Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "a"};
              myPrice.MakeGrid(myTree);
              double prc = myPrice.Price();
              myPrice.Payoff = "p";
              myPrice.MakeGrid(myTree);
              double prp = myPrice.Price();
              Assert.IsTrue(prc > 9.94);  //Orc pr = 9.95
              Assert.IsTrue(prc < 9.96);
              Assert.IsTrue(prp > 9.95);  //Orc pr = 9.96
              Assert.IsTrue(prp < 9.97);
        }

        [TestMethod]
        public void NoDivFlatZero()
        {
            DivList myDiv = new DivList {DivPoints = 1};
            myDiv.MakeArrays();
              myDiv.SetD(0, 0.0, 1.0);
              ZeroCurve myZero = new ZeroCurve {RatePoints = 1};
              myZero.MakeArrays();
              myZero.SetR(0, 0.05, 1.0);
              //create the tree
              DiscreteTree myTree = new DiscreteTree {GridSteps = 100, Tau = 1.0, Sig = 0.25, Spot = 100.0};
              //myTree.flatFlag = false;
              myTree.MakeGrid(myZero, myDiv);
              //create pricer
              Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "a"};
              myPrice.MakeGrid(myTree);
              double prc = myPrice.Price();
              myPrice.Payoff = "p";
              myPrice.MakeGrid(myTree);
              double prp = myPrice.Price();
              Assert.IsTrue(prc > 12.33);  //Orc pr = 12.34
              Assert.IsTrue(prc < 12.35);
              Assert.IsTrue(prp > 7.98);  //Orc pr = 7.99
              Assert.IsTrue(prp < 8.00);
        }

        [TestMethod]
        public void DivFlatZero()
        {
            DivList myDiv = new DivList {DivPoints = 1};
            myDiv.MakeArrays();
              myDiv.SetD(0, 5, 0.5);
              ZeroCurve myZero = new ZeroCurve {RatePoints = 1};
              myZero.MakeArrays();
              myZero.SetR(0, 0.05, 1.0);
              //create the tree
              DiscreteTree myTree = new DiscreteTree {GridSteps = 100, Tau = 1.0, Sig = 0.25, Spot = 100.0};
              //myTree.flatFlag = false;
              myTree.MakeGrid(myZero, myDiv);
              //create pricer
              Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "a"};
              myPrice.MakeGrid(myTree);
              double prc = myPrice.Price();
              myPrice.Payoff = "p";
              myPrice.MakeGrid(myTree);
              double prp = myPrice.Price();
              Assert.IsTrue(prc > 9.73);   //Orc pr = 9.74
              Assert.IsTrue(prc < 9.75);
              Assert.IsTrue(prp > 10.08);  //Orc pr = 10.09
              Assert.IsTrue(prp < 10.10);
        }

        [TestMethod]
        public void DivSlopingZero()
        {
            DivList myDiv = new DivList {DivPoints = 1};
            myDiv.MakeArrays();
              myDiv.SetD(0, 5, 0.5);
              ZeroCurve myZero = new ZeroCurve {RatePoints = 2};
              myZero.MakeArrays();
              myZero.SetR(0, 0.075, 0.5);
              myZero.SetR(1, 0.05, 2);
              //create the tree
              DiscreteTree myTree = new DiscreteTree {GridSteps = 100, Tau = 1.0, Sig = 0.25, Spot = 100.0};
              //myTree.flatFlag = false;
              myTree.MakeGrid(myZero, myDiv);
              //create pricer
              Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "a"};
              myPrice.MakeGrid(myTree);
              double prc = myPrice.Price();
              myPrice.Payoff = "p";
              myPrice.MakeGrid(myTree);
              double prp = myPrice.Price();
              Assert.IsTrue(prc > 10.37);  //Orc pr = 10.38
              Assert.IsTrue(prc < 10.39);
              Assert.IsTrue(prp > 9.42);  //Orc pr = 9.43
              Assert.IsTrue(prp < 9.44);
        }
    }
}
