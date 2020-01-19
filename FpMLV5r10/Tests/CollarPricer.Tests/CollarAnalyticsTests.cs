using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.Analytics.Equities;
using Orion.Analytics.Options;
using Orion.Analytics.Rates;

namespace Orion.EquityCollarPricer.Tests
{
  [TestClass]
  public class CollarAnalyticsTests
  {
      //public CollarAnalytics()
      //  {
      //      //
      //      // TODO: Add constructor logic here
      //      //
      //  }

      //  private TestContext testContextInstance;

      //  /// <summary>
      //  ///Gets or sets the test context which provides
      //  ///information about and functionality for the current test run.
      //  ///</summary>
      //  public TestContext TestContext
      //  {
      //      get
      //      {
      //          return testContextInstance;
      //      }
      //      set
      //      {
      //          testContextInstance = value;
      //      }
      //  }

    [TestMethod]
    public void FlatRates()
    {
        ZeroCurve myZero = new ZeroCurve();

        myZero.Ratepoints = 2;
        myZero.MakeArrays();
        myZero.SetR(0, 0.05, 1.0);
        myZero.SetR(1, 0.05, 2.0);

        //test interpolation priot to point 0
        double tTime = 0.5;
        double val = myZero.LinInterp(tTime);
        Assert.IsTrue(val > 0.0499999999);
        Assert.IsTrue(val < 0.050000001);

        //test interpolation prior to point 1
        tTime = 1.5;
        val = myZero.LinInterp(tTime);
        Assert.IsTrue(val > 0.0499999999);
        Assert.IsTrue(val < 0.050000001);

        //test interpolation post  point 1
        tTime = 2.5;
        val = myZero.LinInterp(tTime);
        Assert.IsTrue(val > 0.0499999999);
        Assert.IsTrue(val < 0.050000001);

        //test forward rates

        //test interpolation priot to point 0
        double tl = 0.25;
        tTime = 0.5;
        val = myZero.ForwardRate(tl, tTime);
        Assert.IsTrue(val > 0.0499999999);
        Assert.IsTrue(val < 0.050000001);

        //test interpolation prior to point 1
        tTime = 1.5;
        val = myZero.ForwardRate(tl, tTime);
        Assert.IsTrue(val > 0.0499999999);
        Assert.IsTrue(val < 0.050000001);

        //test interpolation post  point 1
        tTime = 2.5;
        val = myZero.ForwardRate(tl, tTime);
        Assert.IsTrue(val > 0.0499999999);
        Assert.IsTrue(val < 0.050000001);
    }

    [TestMethod]
    public void NonFlatRates()
    {
      ZeroCurve myZero = new ZeroCurve();

      myZero.Ratepoints = 2;
      myZero.MakeArrays();
      myZero.SetR(0, 0.05, 1.0);
      myZero.SetR(1, 0.075, 2.0);

      //test interpolation priot to point 0
      double tTime = 0.5;
      double val = myZero.LinInterp(tTime);
      Assert.IsTrue(val > 0.0499999999);
      Assert.IsTrue(val < 0.050000001);

      //test interpolation prior to point 1
      tTime = 1.5;
      val = myZero.LinInterp(tTime);
      Assert.IsTrue(val > 0.062499999999);
      Assert.IsTrue(val < 0.06250000001);

      //test interpolation post  point 1
      tTime = 2.5;
      val = myZero.LinInterp(tTime);
      Assert.IsTrue(val > 0.07499999999);
      Assert.IsTrue(val < 0.0750000001);

      //test forward rates

      //test interpolation priot to point 0
      double tl = 0.25;
      tTime = 0.5;
      val = myZero.ForwardRate(tl, tTime);
      Assert.IsTrue(val > 0.0499999999);
      Assert.IsTrue(val < 0.050000001);

      //test interpolation prior to point 1
      tTime = 1.5;
      val = myZero.ForwardRate(tl, tTime);
      Assert.IsTrue(val > 0.06499999);   /// 0.065
      Assert.IsTrue(val < 0.065000001);

      //test interpolation post  point 1
      tTime = 2.5;
      val = myZero.ForwardRate(tl, tTime); ///0.077778
      Assert.IsTrue(val > 0.0777777);
      Assert.IsTrue(val < 0.07777778);


    }


    [TestMethod]
    public void SimpleOrcWingModel()
    {
      const double vc = 0.2417;
      const double sr = -0.1243;
      const double pc = 0.2283;
      const double cc = -1.1478;
      const double dc = -0.2871;
      const double uc = 0.1327;
      const double refFwd = 3065.0;
      const double refVol = 0.2417;
      const double vcr = 0.0;
      const double scr = 0.0;
      const double ssr = 100.0;
      const double dsm = 0.5;
      const double usm = 0.5;
      const double time = 0.2931507;

      OrcWingVol volSurface = new OrcWingVol();
      volSurface.currentVol = vc;
      volSurface.dnCutoff = dc;
      volSurface.dsr = dsm;
      volSurface.putCurve = pc;
      volSurface.callCurve = cc;
      volSurface.refFwd = refFwd;
      volSurface.refVol = refVol;
      volSurface.scr = scr;
      volSurface.slopeRef = sr;
      volSurface.ssr = ssr;
      volSurface.timeToMaturity = time;
      volSurface.upCutoff = uc;
      volSurface.usr = usm;
      volSurface.vcr = vcr;

      double res1 = volSurface.orcvol(3065.0, 1508.64);
      double res2 = volSurface.orcvol(3065.0, 2222.65);
      double res3 = volSurface.orcvol(3065.0, 2591.17);
      double res4 = volSurface.orcvol(3065.0, 3224.57);
      double res5 = volSurface.orcvol(3065.0, 3454.89);
      double res6 = volSurface.orcvol(3065.0, 4502.88);

      Assert.IsTrue(res1 < 0.3147);
      Assert.IsTrue(res1 > 0.3143);

      Assert.IsTrue(res2 < 0.3040);
      Assert.IsTrue(res2 > 0.3035);

      Assert.IsTrue(res3 < 0.2692);
      Assert.IsTrue(res3 > 0.2688);

      Assert.IsTrue(res4 < 0.2325);
      Assert.IsTrue(res4 > 0.2321);

      Assert.IsTrue(res5 < 0.2105);
      Assert.IsTrue(res5 > 0.2101);

      //Test floor;
      Assert.IsTrue(res6 < 0.1909);
      Assert.IsTrue(res6 > 0.1905);
    }



    [TestMethod]
    public void DivTest()
    {
      DivList myDiv = new DivList();
      myDiv.Divpoints = 3;
      myDiv.MakeArrays();
      myDiv.SetD(0, 1.0, 1.0);
      myDiv.SetD(1, 1.0, 2.0);
      myDiv.SetD(2, 1.0, 3.0);


      //test 1.5 year sum

      double sum = 0.0;
      double testT = 1.5;
      for (int idx = 0; idx < myDiv.Divpoints; idx++)
      {
        if (myDiv.GetT(idx) < testT)
          sum += myDiv.GetD(idx);
      }
      Assert.IsTrue(sum > 0.9999999);
      Assert.IsTrue(sum < 1.0000001);


      //test 2.5 year sum

      sum = 0.0;
      testT = 2.5;
      for (int idx = 0; idx < myDiv.Divpoints; idx++)
      {
        if (myDiv.GetT(idx) < testT)
          sum += myDiv.GetD(idx);
      }
      Assert.IsTrue(sum > 1.9999999);
      Assert.IsTrue(sum < 2.0000001);

      //test 3.5 year sum

      sum = 0.0;
      testT = 3.5;
      for (int idx = 0; idx < myDiv.Divpoints; idx++)
      {
        if (myDiv.GetT(idx) < testT)
          sum += myDiv.GetD(idx);
      }
      Assert.IsTrue(sum > 2.9999999);
      Assert.IsTrue(sum < 3.0000001);


    }



    [TestMethod]
    public void ENoDivNoZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 0.0, 1.0);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 1;
      myZero.MakeArrays();
      myZero.SetR(0, 0.0, 1.0);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 9.945);  //BS pr = 9.947653
      Assert.IsTrue(prc < 9.958);

      Assert.IsTrue(prp > 9.945);  //BS pr = 9.947653
      Assert.IsTrue(prp < 9.958);

    }

    [TestMethod]
    public void ENoDivFlatZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 0.0, 1.0);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 1;
      myZero.MakeArrays();
      myZero.SetR(0, 0.05, 1.0);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 12.330);  //BS pr = 12.336
      Assert.IsTrue(prc < 12.350);

      Assert.IsTrue(prp > 7.435);  //BS pr = 7.45894
      Assert.IsTrue(prp < 7.485);

    }

    [TestMethod]
    public void EDivFlatZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 5, 0.5);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 1;
      myZero.MakeArrays();
      myZero.SetR(0, 0.05, 1.0);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 9.455);  //BS pr = 9.46277
      Assert.IsTrue(prc < 9.475);

      Assert.IsTrue(prp > 9.462);  //BS pr = 9.46226
      Assert.IsTrue(prp < 9.475);

    }

    [TestMethod]
    public void EDivSlopingZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 5, 0.5);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 2;
      myZero.MakeArrays();
      myZero.SetR(0, 0.075, 0.5);
      myZero.SetR(1, 0.05, 2);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 10.22);  //Orc BS pr = 10.23
      Assert.IsTrue(prc < 10.24);

      Assert.IsTrue(prp > 8.58);  //Orc BS pr = 8.59
      Assert.IsTrue(prp < 8.61);
    }

    [TestMethod]
    public void ANoDivNoZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 0.0, 1.0);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 1;
      myZero.MakeArrays();
      myZero.SetR(0, 0.0, 1.0);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 9.94);  //Orc pr = 9.95
      Assert.IsTrue(prc < 9.96);

      Assert.IsTrue(prp > 9.95);  //Orc pr = 9.96
      Assert.IsTrue(prp < 9.97);

    }

    [TestMethod]
    public void ANoDivFlatZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 0.0, 1.0);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 1;
      myZero.MakeArrays();
      myZero.SetR(0, 0.05, 1.0);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 12.33);  //Orc pr = 12.34
      Assert.IsTrue(prc < 12.35);

      Assert.IsTrue(prp > 7.98);  //Orc pr = 7.99
      Assert.IsTrue(prp < 8.00);

    }

   [TestMethod]
    public void ADivFlatZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 5, 0.5);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 1;
      myZero.MakeArrays();
      myZero.SetR(0, 0.05, 1.0);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 9.73);   //Orc pr = 9.74
      Assert.IsTrue(prc < 9.75);

      Assert.IsTrue(prp > 10.08);  //Orc pr = 10.09
      Assert.IsTrue(prp < 10.10);

    }

    [TestMethod]
    public void ADivSlopingZero()
    {
      DivList mydiv = new DivList();
      mydiv.Divpoints = 1;
      mydiv.MakeArrays();
      mydiv.SetD(0, 5, 0.5);

      ZeroCurve myZero = new ZeroCurve();
      myZero.Ratepoints = 2;
      myZero.MakeArrays();
      myZero.SetR(0, 0.075, 0.5);
      myZero.SetR(1, 0.05, 2);

      //create the tree
      Tree myTree = new Tree();
      myTree.Gridsteps = 100;
      myTree.Tau = 1.0;
      myTree.Sig = 0.25;
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

      Assert.IsTrue(prc > 10.37);  //Orc pr = 10.38
      Assert.IsTrue(prc < 10.39);

      Assert.IsTrue(prp > 9.42);  //Orc pr = 9.43
      Assert.IsTrue(prp < 9.44);
    }
  }
}
