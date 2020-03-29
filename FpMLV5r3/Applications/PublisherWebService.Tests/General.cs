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

using Highlander.PublisherWebService.V5r3;
using Highlander.UnitTestEnv.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.PublisherWebService.Tests.V5r3
{
    [TestClass]
    public class General
    {
        private static UnitTestEnvironment UTE { get; set; }
        public static PricingStructures PricingStructures { get; set; }
        //public static LpmPublisher LpmPublisher { get; set; }
        public static CurveEngine.V5r3.CurveEngine CurveEngine { get; set; }

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContext)
        {
            UTE = new UnitTestEnvironment();
            var logger = Reference<ILogger>.Create(UTE.Logger);
            PricingStructures = new PricingStructures(logger, UTE.Cache, UTE.NameSpace);
            //LpmPublisher = new LpmPublisher(logger, UTE.Cache);
            CurveEngine = new CurveEngine.V5r3.CurveEngine(logger.Target, UTE.Cache);
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            UTE.Dispose();
        }
    }
}
