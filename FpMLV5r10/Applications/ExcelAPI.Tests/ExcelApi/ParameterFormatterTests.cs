#region Using directives

using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Orion.CurveEngine.Helpers;


#endregion

namespace Orion.ExcelAPI.Tests.ExcelApi
{
    [TestClass]
    public class ParameterFormatterTests
    {
        [TestMethod]
        public void FormatException()
        {
            try
            {
                throw new NotImplementedException("Test string");
            }
            catch(Exception ex)
            {
                string formatted = ParameterFormatter.FormatObject(ex);
                Debug.WriteLine(formatted);
            }
        }


        [TestMethod]
        public void FormatExceptionWithInnerExceptin()
        {
            try
            {
                try
                {
                    throw new NotImplementedException("Test string");
                }
                catch (Exception ex)
                {
                    throw new Exception("Excetion1", ex);
                }
            }
            catch (Exception ex)
            {
                string formatted = ParameterFormatter.FormatObject(ex);
                Debug.WriteLine(formatted);
            }
        }

        [TestMethod]
        public void FormatExceptionWithInnerExceptinWithData()
        {
            try
            {
                try
                {
                    NotImplementedException ex =  new NotImplementedException("Test string");

                    ex.Data.Add("Key1", "Value1");
                    ex.Data.Add("Key2", null);
                    ex.Data.Add("Key3", 3);

                    throw ex;
                }
                catch (Exception ex)
                {
                    throw new Exception("Excetion1", ex);
                }
            }
            catch (Exception ex)
            {
                string formatted = ParameterFormatter.FormatObject(ex);
                Debug.WriteLine(formatted);
            }
        }

        [TestMethod]
        public void FormatInteger()
        {
            int i = 24;


            string formatted = ParameterFormatter.FormatObject(i);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatNull()
        {
            object n = null;
            
            string formatted = ParameterFormatter.FormatObject(n);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatString()
        {
            string s = "Test string";

            string formatted = ParameterFormatter.FormatObject(s);

            Debug.WriteLine(formatted);
        }
        
        [TestMethod]
        public void FormatDouble()
        {
            double d = 24.5;

            string formatted = ParameterFormatter.FormatObject(d);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatDecimal()
        {
            decimal d = 24.56m;

            string formatted = ParameterFormatter.FormatObject(d);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatBool()
        {
            bool b = true;

            string formatted = ParameterFormatter.FormatObject(b);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatOneDimensionalArrayOfStrings()
        {
            string[] array = new string[]{"One", "Two", "Three"};

            string formatted = ParameterFormatter.FormatObject(array);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatTwoDimensionalArrayOfStrings()
        {
            string[,] array = new string[,]
                                  {
                                      {"One1", "Two1", "Three1"},
                                      {"One2", "Two2", "Three2"},
                                      {"One3", "Two3", "Three3"}
                                  };
                    
            string formatted = ParameterFormatter.FormatObject(array);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatTriDimensionalArrayOfStrings()
        {
            string[,,] array = new string[,,]
                                   {
                                       {
                                           {"One1", "Two1", "Three1"},
                                           {"One2", "Two2", "Three2"},
                                           {"One3", "Two3", "Three3"}
                                       },
                                       {
                                           {"2One1", "2Two1", "2Three1"},
                                           {"2One2", "2Two2", "2Three2"},
                                           {"2One3", "2Two3", "2Three3"}
                                       }
                                   };
                    
            string formatted = ParameterFormatter.FormatObject(array);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatOneDimensionalArrayOfInts()
        {
            int[] array = new int[]{12, 34, 56};

            string formatted = ParameterFormatter.FormatObject(array);

            Debug.WriteLine(formatted);
        }

        [TestMethod]
        public void FormatTwoDimensionalArrayOfInts()
        {
            int[,] array = new int[,]
                               {
                                   {112, 134, 156},
                                   {212, 234, 256},
                                   {312, 334, 356},
                                   {412, 434, 456},
                                   {512, 534, 556}
                               };

            string formatted = ParameterFormatter.FormatObject(array);

            Debug.WriteLine(formatted);
        }    
        
        [TestMethod]
        public void FormatTwoDimensionalArrayOfIntsStringsDecimalsDoublesBools()
        {
            object[,] array = new object[,]
                                  {
                                      {112, "some", 156},
                                      {212.0d, 234, 256},
                                      {null, 334, false},
                                      {312, 34.12m,false},
                                      {412, "another", 456, },
                                      {512, 534, 556}
                                  };

            string formatted = ParameterFormatter.FormatObject(array);

            Debug.WriteLine(formatted);
        }
    }
}