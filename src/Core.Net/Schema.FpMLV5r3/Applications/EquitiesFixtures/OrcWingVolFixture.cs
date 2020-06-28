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
    public class OrcWingVolFixture
    {
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
            OrcWingVol volSurface = new OrcWingVol
            {
                CurrentVol = vc,
                DnCutoff = dc,
                Dsr = dsm,
                PutCurve = pc,
                CallCurve = cc,
                RefFwd = refFwd,
                RefVol = refVol,
                Scr = scr,
                SlopeRef = sr,
                Ssr = ssr,
                TimeToMaturity = time,
                UpCutoff = uc,
                Usr = usm,
                Vcr = vcr
            };
            double res1 = volSurface.Orcvol(3065.0, 1508.64);
            double res2 = volSurface.Orcvol(3065.0, 2222.65);
            double res3 = volSurface.Orcvol(3065.0, 2591.17);
            double res4 = volSurface.Orcvol(3065.0, 3224.57);
            double res5 = volSurface.Orcvol(3065.0, 3454.89);
            double res6 = volSurface.Orcvol(3065.0, 4502.88);
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
    }
}
