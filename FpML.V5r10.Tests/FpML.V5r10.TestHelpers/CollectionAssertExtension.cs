using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FpML.V5r10.TestHelpers
{
    public static class CollectionAssertExtension
    {
        public static void IsEmpty(Array item1)
        {
            Assert.IsTrue(item1 == null || item1.GetLength(0) == 0);
        }
    }
}
