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

using System.Collections.Generic;
using System.Reflection;
using Highlander.Utilities.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.Diagnostics
{
    /// <summary>
    /// Summary description for AssemblyInfoTest
    /// </summary>
    [TestClass]
    public class AssemblyInfoTest
    {
        [TestMethod]
        public void TestAssemblyInfo()
        {
            Assembly ass = Assembly.GetAssembly(typeof(AssemblyInfo));
            AssemblyInfo info = new AssemblyInfo(ass);
            List<string> fields = info.DictionaryFields;
            IDictionary<string, string> dict = info.ToDict();
            string fv = dict["FileVersion"].ToString();
            object[,] array = info.ToArray();
            string pkt = info["PuBLicKeyToken"];
            string field = info["BOB"];
        }
    }
}