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
  public class AmericanPricerFixture
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
      myPrice.Style = "a";

      myPrice.MakeGrid(myTree);
      double prc = myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 9.94);  //Orc pr = 9.95
      Assert.Less(prc, 9.96);

      Assert.Greater(prp, 9.95);  //Orc pr = 9.96
      Assert.Less(prp, 9.97);

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
      myPrice.Style = "a";

      myPrice.MakeGrid(myTree);
      double prc = myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 12.33);  //Orc pr = 12.34
      Assert.Less(prc, 12.35);

      Assert.Greater(prp, 7.98);  //Orc pr = 7.99
      Assert.Less(prp, 8.00);

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
      myPrice.Style = "a";

      myPrice.MakeGrid(myTree);
      double prc = myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 9.73);   //Orc pr = 9.74
      Assert.Less(prc, 9.75);

      Assert.Greater(prp, 10.08);  //Orc pr = 10.09
      Assert.Less(prp, 10.10);

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
      myPrice.Style = "a";

      myPrice.MakeGrid(myTree);
      double prc = myPrice.Price();

      myPrice.Payoff = "p";
      myPrice.MakeGrid(myTree);
      double prp = myPrice.Price();

      Assert.Greater(prc, 10.37);  //Orc pr = 10.38
      Assert.Less(prc, 10.39);

      Assert.Greater(prp, 9.42);  //Orc pr = 9.43
      Assert.Less(prp, 9.44);
    }

  }
}
