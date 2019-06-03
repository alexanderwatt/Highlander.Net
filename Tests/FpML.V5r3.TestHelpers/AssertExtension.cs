using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.TestHelpers
{
    public static class AssertExtension
    {
        public static void LessOrEqual(int item1, int item2)
        {
            Assert.IsTrue(item1 <= item2);
        }

        public static void LessOrEqual(double item1, double item2)
        {
            Assert.IsTrue(item1 <= item2);
        }

        public static void Less(int item1, int item2)
        {
            Assert.IsTrue(item1 < item2);
        }

        public static void Less(double item1, double item2)
        {
            Assert.IsTrue(item1 < item2);
        }

        public static void Less(decimal item1, decimal item2)
        {
            Assert.IsTrue(item1 < item2);
        }

        public static void Greater(decimal item1, decimal item2)
        {
            Assert.IsTrue(item1 > item2);
        }
    }
}
