using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EquitiesAddin;
using NUnit.Framework;
//using NUnit.Framework.SyntaxHelpers;


namespace EquitiesFixtures
{
  [TestFixture]
  public class EuropeanPricerFixture
  {
    [Test]
    public void NoDivNoZero()
    {
      DivList mydiv = new DivList();
      mydiv.divpoints = 1;
      mydiv.makeArrays();
      mydiv.set_d(0, 0.0, 1.0);

      ZeroCurve myZero = new ZeroCurve();
      myZero.ratepoints = 1;
      myZero.makeArrays();
      myZero.set_r(0, 0.0, 1.0);

      //create the tree
      DiscreteTree myTree = new DiscreteTree();
      myTree.Gridsteps = 100;
      myTree.tau = 1.0;
      myTree.sig = 0.25;
      myTree.Spot = 100.0;
      //myTree.flatFlag = false;
      myTree.MakeGrid(myZero, mydiv);

      //create pricer
      Pricer myPrice = new Pricer();
      myPrice.Strike = 100;
      myPrice.Payoff = "c";
      myPrice.Smoothing = "y";
      myPrice.Style = "e";

      myPrice.MakeGrid(myTree);
      double prc= myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 9.945);  //BS pr = 9.947653
      Assert.Less(prc,9.958);

      Assert.Greater(prp, 9.945);  //BS pr = 9.947653
      Assert.Less(prp, 9.958);

    }

    [Test]
    public void NoDivFlatZero()
    {
      DivList mydiv = new DivList();
      mydiv.divpoints = 1;
      mydiv.makeArrays();
      mydiv.set_d(0, 0.0, 1.0);

      ZeroCurve myZero = new ZeroCurve();
      myZero.ratepoints = 1;
      myZero.makeArrays();
      myZero.set_r(0, 0.05, 1.0);

      //create the tree
      DiscreteTree myTree = new DiscreteTree();
      myTree.Gridsteps = 100;
      myTree.tau = 1.0;
      myTree.sig = 0.25;
      myTree.Spot = 100.0;
      //myTree.flatFlag = false;
      myTree.MakeGrid(myZero, mydiv);

      //create pricer
      Pricer myPrice = new Pricer();
      myPrice.Strike = 100;
      myPrice.Payoff = "c";
      myPrice.Smoothing = "y";
      myPrice.Style = "e";

      myPrice.MakeGrid(myTree);
      double prc = myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 12.330);  //BS pr = 12.336
      Assert.Less(prc, 12.350);

      Assert.Greater(prp, 7.435);  //BS pr = 7.45894
      Assert.Less(prp, 7.485);

    }

    [Test]
    public void DivFlatZero()
    {
      DivList mydiv = new DivList();
      mydiv.divpoints = 1;
      mydiv.makeArrays();
      mydiv.set_d(0, 5, 0.5);

      ZeroCurve myZero = new ZeroCurve();
      myZero.ratepoints = 1;
      myZero.makeArrays();
      myZero.set_r(0, 0.05, 1.0);

      //create the tree
      DiscreteTree myTree = new DiscreteTree();
      myTree.Gridsteps = 100;
      myTree.tau = 1.0;
      myTree.sig = 0.25;
      myTree.Spot = 100.0;
      //myTree.flatFlag = false;
      myTree.MakeGrid(myZero, mydiv);

      //create pricer
      Pricer myPrice = new Pricer();
      myPrice.Strike = 100;
      myPrice.Payoff = "c";
      myPrice.Smoothing = "y";
      myPrice.Style = "e";

      myPrice.MakeGrid(myTree);
      double prc = myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 9.455);  //BS pr = 9.46277
      Assert.Less(prc, 9.475);

      Assert.Greater(prp, 9.462);  //BS pr = 9.46226
      Assert.Less(prp, 9.475);

    }

    [Test]
    public void DivSlopingZero()
    {
      DivList mydiv = new DivList();
      mydiv.divpoints = 1;
      mydiv.makeArrays();
      mydiv.set_d(0, 5, 0.5);

      ZeroCurve myZero = new ZeroCurve();
      myZero.ratepoints = 2;
      myZero.makeArrays();
      myZero.set_r(0, 0.075, 0.5);
      myZero.set_r(1, 0.05, 2);

      //create the tree
      DiscreteTree myTree = new DiscreteTree();
      myTree.Gridsteps = 100;
      myTree.tau = 1.0;
      myTree.sig = 0.25;
      myTree.Spot = 100.0;
      //myTree.flatFlag = false;
      myTree.MakeGrid(myZero, mydiv);

      //create pricer
      Pricer myPrice = new Pricer();
      myPrice.Strike = 100;
      myPrice.Payoff = "c";
      myPrice.Smoothing = "y";
      myPrice.Style = "e";

      myPrice.MakeGrid(myTree);
      double prc = myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 10.22);  //Orc BS pr = 10.23
      Assert.Less(prc, 10.24);

      Assert.Greater(prp, 8.58);  //Orc BS pr = 8.59
      Assert.Less(prp, 8.61);
    }

  }
}
