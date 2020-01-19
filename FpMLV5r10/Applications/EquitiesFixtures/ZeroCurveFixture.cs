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
  public class ZeroCurveFixture
  {

    [Test]
    public void FlatRates()
    {
      ZeroCurve myZero = new ZeroCurve();

      myZero.ratepoints = 2;
      myZero.makeArrays();
      myZero.set_r(0, 0.05, 1.0);
      myZero.set_r(1, 0.05, 2.0);

      //test interpolation priot to point 0
      double tTime = 0.5;
      double val = myZero.linInterp(tTime);
      Assert.Greater(val, 0.0499999999);
      Assert.Less(val, 0.050000001);

      //test interpolation prior to point 1
      tTime = 1.5;
      val = myZero.linInterp(tTime);
      Assert.Greater(val, 0.0499999999);
      Assert.Less(val, 0.050000001);

      //test interpolation post  point 1
      tTime = 2.5;
      val = myZero.linInterp(tTime);
      Assert.Greater(val, 0.0499999999);
      Assert.Less(val, 0.050000001);

      //test forward rates

      //test interpolation priot to point 0
      double tl = 0.25;
      tTime = 0.5;
      val = myZero.forwardRate(tl,tTime);
      Assert.Greater(val, 0.0499999999);
      Assert.Less(val, 0.050000001);

      //test interpolation prior to point 1
      tTime = 1.5;
      val = myZero.forwardRate(tl, tTime);
      Assert.Greater(val, 0.0499999999);
      Assert.Less(val, 0.050000001);

      //test interpolation post  point 1
      tTime = 2.5;
      val = myZero.forwardRate(tl, tTime);
      Assert.Greater(val, 0.0499999999);
      Assert.Less(val, 0.050000001);


    }

    [Test]
    public void NonFlatRates()
    {
      ZeroCurve myZero = new ZeroCurve();

      myZero.ratepoints = 2;
      myZero.makeArrays();
      myZero.set_r(0, 0.05, 1.0);
      myZero.set_r(1, 0.075, 2.0);

      //test interpolation priot to point 0
      double tTime = 0.5;
      double val = myZero.linInterp(tTime);
      Assert.Greater(val, 0.0499999999);
      Assert.Less(val, 0.050000001);

      //test interpolation prior to point 1
      tTime = 1.5;
      val = myZero.linInterp(tTime);
      Assert.Greater(val, 0.062499999999);
      Assert.Less(val, 0.06250000001);

      //test interpolation post  point 1
      tTime = 2.5;
      val = myZero.linInterp(tTime);
      Assert.Greater(val, 0.07499999999);
      Assert.Less(val, 0.0750000001);

      //test forward rates

      //test interpolation priot to point 0
      double tl = 0.25;
      tTime = 0.5;
      val = myZero.forwardRate(tl, tTime);
      Assert.Greater(val, 0.0499999999);  
      Assert.Less(val, 0.050000001);

      //test interpolation prior to point 1
      tTime = 1.5;
      val = myZero.forwardRate(tl, tTime);
      Assert.Greater(val, 0.06499999);   /// 0.065
      Assert.Less(val, 0.065000001);

      //test interpolation post  point 1
      tTime = 2.5;
      val = myZero.forwardRate(tl, tTime); ///0.077778
      Assert.Greater(val, 0.0777777);
      Assert.Less(val, 0.07777778);


    }


  }
}
