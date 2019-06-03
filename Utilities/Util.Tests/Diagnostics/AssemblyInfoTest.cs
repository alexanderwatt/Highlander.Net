using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Orion.Util.Diagnostics;

namespace Util.Tests.Diagnostics
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