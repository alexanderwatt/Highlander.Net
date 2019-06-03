using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.TestHelpers
{
    public static class CollectionAssertExtension
    {
        public static void IsEmpty(Array item1)
        {
            Assert.IsTrue(item1 == null || item1.GetLength(0) == 0);
        }
    }
}
