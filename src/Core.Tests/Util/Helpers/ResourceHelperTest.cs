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

using System.Diagnostics;
using System.Reflection;
using Highlander.Utilities.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Highlander.Utilities.Tests.Helpers
{
    /// <summary>
    ///This is a test class for ResourceHelperTest and is intended
    ///to contain all ResourceHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class ResourceHelperTest
    {
        /// <summary>
        ///A test for ReadResourceValue
        ///</summary>
        [TestMethod]
        public void ReadResourcesTest()
        {
            var actual = ResourceHelper.GetResources(Assembly.GetExecutingAssembly());
            foreach (var file in actual)
            {
                Debug.Print(file);
            }
        }


        /// <summary>
        ///A test for ReadResourceValue
        ///</summary>
        [TestMethod]
        public void ReadResourceValueTest()
        {
            string actual = ResourceHelper.ReadResourceValue("Highlander.Utilities.Tests.Helpers.ResourceTest.resources", "Name1");
            Assert.AreEqual("value1", actual);
        }

        /// <summary>
        ///A test for GetResourceWithPartialName
        ///</summary>
        [TestMethod]
        public void GetResourceWithPartialNameTest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string partialResourceName = "ResourceTest.resources";
            string actual = ResourceHelper.GetResourceWithPartialName(assembly, partialResourceName);
            Assert.AreEqual("Resource Test", actual);
            actual = ResourceHelper.GetResourceWithPartialName(assembly, "invalid");
            Assert.AreEqual(null, actual);
        }

        /// <summary>
        ///A test for GetResource
        ///</summary>
        [TestMethod]
        public void GetResourceTest()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            const string resourceName = "Highlander.Utilities.Tests.Helpers.ResourceTest.resources";
            string actual = ResourceHelper.GetResource(assembly, resourceName);
            Assert.AreEqual("Resource Test", actual);
        }
    }
}