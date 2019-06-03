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
  public class OrcWingVolFixture
  {
    [Test]
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

      double res1 = volSurface.orcvol(3065.0, 1508.64 );
      double res2 = volSurface.orcvol(3065.0, 2222.65);
      double res3 = volSurface.orcvol(3065.0, 2591.17);
      double res4 = volSurface.orcvol(3065.0, 3224.57);
      double res5 = volSurface.orcvol(3065.0, 3454.89);
      double res6 = volSurface.orcvol(3065.0, 4502.88);

      Assert.Less(res1,0.3147);
      Assert.Greater(res1, 0.3143);

      Assert.Less(res2, 0.3040);
      Assert.Greater(res2, 0.3035);

      Assert.Less(res3, 0.2692);
      Assert.Greater(res3, 0.2688);

      Assert.Less(res4, 0.2325);
      Assert.Greater(res4, 0.2321);

      Assert.Less(res5, 0.2105);
      Assert.Greater(res5, 0.2101);

      //Test floor;
      Assert.Less(res6, 0.1909);
      Assert.Greater(res6, 0.1905);








    }


  }
}
