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
    public class EuropeanPricerFixture
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
            Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "e"};
            myPrice.MakeGrid(myTree);
              double prc= myPrice.Price();
              myPrice.Payoff = "p";
              myPrice.MakeGrid(myTree);
              double prp = myPrice.Price();
              Assert.IsTrue(prc > 9.945);  //BS pr = 9.947653
              Assert.IsTrue(prc < 9.958);
              Assert.IsTrue(prp > 9.945);  //BS pr = 9.947653
              Assert.IsTrue(prp < 9.958);
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
            Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "e"};
            myPrice.MakeGrid(myTree);
            double prc = myPrice.Price();
            myPrice.Payoff = "p";
            myPrice.MakeGrid(myTree);
            double prp = myPrice.Price();
            Assert.IsTrue(prc > 12.330);  //BS pr = 12.336
            Assert.IsTrue(prc < 12.350);
            Assert.IsTrue(prp > 7.435);  //BS pr = 7.45894
            Assert.IsTrue(prp < 7.485);
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
            Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "e"};
            myPrice.MakeGrid(myTree);
            double prc = myPrice.Price();
            myPrice.Payoff = "p";
            myPrice.MakeGrid(myTree);
            double prp = myPrice.Price();
            Assert.IsTrue(prc > 9.455);  //BS pr = 9.46277
            Assert.IsTrue(prc < 9.475);
            Assert.IsTrue(prp > 9.462);  //BS pr = 9.46226
            Assert.IsTrue(prp < 9.475);
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
            Pricer myPrice = new Pricer {Strike = 100, Payoff = "c", Smoothing = "y", Style = "e"};
            myPrice.MakeGrid(myTree);
            double prc = myPrice.Price();
            myPrice.Payoff = "p";
            myPrice.MakeGrid(myTree);
            double prp = myPrice.Price();
            Assert.IsTrue(prc > 10.22);  //Orc BS pr = 10.23
            Assert.IsTrue(prc < 10.24);
            Assert.IsTrue(prp > 8.58);  //Orc BS pr = 8.59
            Assert.IsTrue(prp < 8.61);
        }
    }
}
