using System;
using Orion.Util.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Util.Tests.Helpers
{
    /// <summary>
    ///This is a test class for ArrayHelperTest and is intended
    ///to contain all ArrayHelperTest Unit Tests
    ///</summary>
    [TestClass]
    public class ArrayHelperTest
    {
        /// <summary>
        ///A test for RangeToMatrix
        ///</summary>
        [TestMethod]
        public void RangeToMatrixTestZeroBased()
        {
            object[,] range0
                = new object[,]
                      {
                          {"Item0", "Value0"},
                          {"Item1", "Value1"},
                          {"Item2", "Value2"},
                          {"Item3", "Value3"}
                      };

            object[,] matrix = ArrayHelper.RangeToMatrix(range0);
            Assert.AreEqual(range0[1, 1], matrix[1, 1]);
            Assert.AreEqual(3, matrix.GetUpperBound(0));
            Assert.AreEqual(1, matrix.GetUpperBound(1));
        }

        /// <summary>
        ///A test for RangeToMatrix
        ///</summary>
        [TestMethod]
        public void RangeToMatrixTestOneBased()
        {
            object[,] range0
                = new object[,]
                      {
                          {"Item0", "Value0"},
                          {"Item1", "Value1"},
                          {"Item2", "Value2"},
                          {"Item3", "Value3"}
                      };

            object[,] range1 = (object[,])Array.CreateInstance(typeof(object), new int[] { 4, 2 }, new int[] { 1, 1 });
            for (int i = 0; i <= range0.GetUpperBound(0); i++)
            {
                range1[i + 1, 1] = range0[i, 0];
                range1[i + 1, 2] = range0[i, 1];
            }

            object[,] matrix = ArrayHelper.RangeToMatrix(range1);

            Assert.AreEqual(range1[2, 2], matrix[1, 1]);
            Assert.AreEqual(3, matrix.GetUpperBound(0));
            Assert.AreEqual(1, matrix.GetUpperBound(1));
        }

        /// <summary>
        ///A test for MatrixToArray
        ///</summary>
        [TestMethod]
        public void MatrixToArrayTestColumn()
        {
            object[,] range0
                = new object[,]
                      {
                          {"Item0"},
                          {"Item1"},
                          {"Item2"},
                          {"Item3"}
                      };

            object[] matrix = ArrayHelper.MatrixToArray(range0);
            Assert.AreEqual(range0[1, 0], matrix[1]);
            Assert.AreEqual(4, matrix.Length);
        }

        /// <summary>
        ///A test for MatrixToArray
        ///</summary>
        [TestMethod]
        public void MatrixToArrayTestRow()
        {
            object[,] range0
                = new object[,]
                      {
                          {"Item0", "Item1", "Item2", "Item3"}
                      };

            object[] matrix = ArrayHelper.MatrixToArray(range0);
            Assert.AreEqual(range0[0, 1], matrix[1]);
            Assert.AreEqual(4, matrix.Length);
        }

        /// <summary>
        ///A test for ArrayToVerticalMatrix
        ///</summary>
        [TestMethod]
        public void ArrayToVerticalMatrixTest()
        {
            double[] array = new double[]{1,2,3};

            double[,] matrix = ArrayHelper.ArrayToVerticalMatrix(array);
            Assert.AreEqual(array[1], matrix[1, 0]);
            Assert.AreEqual(matrix.GetUpperBound(0), 2);
            Assert.AreEqual(matrix.GetUpperBound(1), 0);
        }

        /// <summary>
        ///A test for ArrayToVerticalMatrix
        ///</summary>
        [TestMethod]
        public void ArrayToHorizontalMatrixTest()
        {
            double[] array = new double[] { 1, 2, 3 };

            double[,] matrix = ArrayHelper.ArrayToHorizontalMatrix(array);
            Assert.AreEqual(array[1], matrix[0, 1]);
            Assert.AreEqual(matrix.GetUpperBound(0), 0);
            Assert.AreEqual(matrix.GetUpperBound(1), 2);
        }

        /// <summary>
        ///A test for ConvertDictionaryTo2DArray
        ///</summary>
        [TestMethod]
        public void ConvertDictionaryTo2DArrayTest()
        {
            IDictionary<string, object> dictionary
                = new Dictionary<string, object>
                      {
                          {"Name", "Test"},
                          {"Number", 1},
                          {"Date", DateTime.Now}
                      };
            object[,] actual = ArrayHelper.ConvertDictionaryTo2DArray(dictionary);

            Assert.AreEqual("Name", actual[0, 0]);
            Assert.AreEqual(1, actual[1, 1]);
            Assert.AreEqual("Date", actual[2, 0]);
            Assert.AreEqual(2, actual.GetUpperBound(0));
            Assert.AreEqual(1, actual.GetUpperBound(1));
        }
    }
}