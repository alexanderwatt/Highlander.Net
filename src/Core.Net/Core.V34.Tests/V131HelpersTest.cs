/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using Highlander.Core.Common;
using Highlander.Utilities.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Core.V34.Tests
{
    [TestClass]
    public class V131HelpersTest
    {
        [TestMethod]
        public void CheckRequiredFileVersionTest()
        {
            ILogger logger = new TraceLogger(false);
            const string requiredVersion = "3.4.1222.0";

            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.3.1011.6602"));
            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.3.1208.6602"));
            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.3.1222.6602"));

            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.4.1011.6602"));
            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.4.1208.6602"));
            Assert.IsTrue(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.4.1222.6602"));

            Assert.IsFalse(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.5.1208.6602"));
            Assert.IsTrue(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.5.1222.6602"));
            Assert.IsTrue(V131Helpers.CheckRequiredFileVersion(logger, requiredVersion, "3.5.1407.6602"));
        }
    }
}
