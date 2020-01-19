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
  public class DivListFixture
  {
    [Test]
    public void DivTest()
    {
      DivList myDiv = new DivList();
      myDiv.divpoints = 3;
      myDiv.makeArrays();
      myDiv.set_d(0, 1.0, 1.0);
      myDiv.set_d(1, 1.0, 2.0);
      myDiv.set_d(2, 1.0, 3.0);


      //test 1.5 year sum

      double sum = 0.0;
      double testT = 1.5;
      for (int idx = 0; idx < myDiv.divpoints; idx++)
      {
        if (myDiv.get_t(idx) < testT)
          sum += myDiv.get_d(idx);
      }
      Assert.Greater(sum, 0.9999999);
      Assert.Less(sum, 1.0000001);


      //test 2.5 year sum

      sum = 0.0;
      testT = 2.5;
      for (int idx = 0; idx < myDiv.divpoints; idx++)
      {
        if (myDiv.get_t(idx) < testT)
          sum += myDiv.get_d(idx);
      }
      Assert.Greater(sum, 1.9999999);
      Assert.Less(sum, 2.0000001);

      //test 3.5 year sum

      sum = 0.0;
      testT = 3.5;
      for (int idx = 0; idx < myDiv.divpoints; idx++)
      {
        if (myDiv.get_t(idx) < testT)
          sum += myDiv.get_d(idx);
      }
      Assert.Greater(sum, 2.9999999);
      Assert.Less(sum, 3.0000001);


    }



  }
}
