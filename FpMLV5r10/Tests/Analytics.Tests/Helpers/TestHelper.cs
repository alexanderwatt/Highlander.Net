
using System.Diagnostics;

using System;


namespace National.QRSC.Numerics.Tests.Helpers
{
    static public class TestHelper
    {
        static readonly string _resourceFile = "National.QRSC.Numerics.Tests.Data.TenorConversionResource";

        #region get roll dates

        /// <summary>
        /// Get the roll dates
        /// </summary>
        /// <returns>an array of roll dates</returns>
        static internal System.DateTime[] GetQuarterlyRollDates()
        {
            //Get the method name
            string methodInstance = TestHelper.GetExecutingMethod();
            return getDates(methodInstance);  
        }


        /// <summary>
        /// Get the roll dates
        /// </summary>
        /// <returns>an array of roll dates</returns>
        static internal System.DateTime[] GetMonthlyRollDates()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getDates(methodInstance);       
        }



        /// <summary>
        /// Get the roll dates
        /// </summary>
        /// <returns>an array of roll dates</returns>
        static internal System.DateTime[] GetSemiRollDates()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getDates(methodInstance);  
        }



        /// <summary>
        /// Get the roll dates
        /// </summary>
        /// <returns>an array of roll dates</returns>
        static internal System.DateTime[] Get5yqRollDates()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getDates(methodInstance);
        }


        /// <summary>
        /// Get the roll dates
        /// </summary>
        /// <returns>an array of roll dates</returns>
        static internal System.DateTime[] Get5ysRollDates()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getDates(methodInstance);
        }

        #endregion

        #region get discount factors

        /// <summary>
        /// get discount factors
        /// </summary>
        /// <returns>An array of discount factors</returns>
        static internal double[] GetQuarterlyDiscountFactors()
        {

            string methodInstance = TestHelper.GetExecutingMethod();
            return getRates(methodInstance);

        }


        /// <summary>
        /// get discount factors
        /// </summary>
        /// <returns>An array of discount factors</returns>
        static internal double[] Get5yqDiscountFactors()
        {

            string methodInstance = TestHelper.GetExecutingMethod();
            return getRates(methodInstance);

        }

        #endregion

        #region get vols methods

        /// <summary>
        /// Get Quarterly vols
        /// </summary>
        /// <returns>an array of querterly vols</returns>
        static internal double[] GetQuarterlyVols()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getRates(methodInstance);
            
        }



        /// <summary>
        /// Get Semi vols
        /// </summary>
        /// <returns>an array of querterly vols</returns>
        static internal double[] GetSemiVols()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getRates(methodInstance);
            
        }


        /// <summary>
        /// get monthly vols
        /// </summary>
        /// <returns>an array of monthly vols</returns>
        static internal double[] GetMonthlyVols()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getRates(methodInstance);
        }


        /// <summary>
        /// get monthly vols
        /// </summary>
        /// <returns>an array of monthly vols</returns>
        static internal double[] Get5yqVols()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getRates(methodInstance);
        }



        /// <summary>
        /// get monthly vols
        /// </summary>
        /// <returns>an array of monthly vols</returns>
        static internal double[] Get5ysVols()
        {
            string methodInstance = TestHelper.GetExecutingMethod();
            return getRates(methodInstance);
        }

        #endregion

        #region helper methods
        /// <summary>
        /// Get the rates or vols out of the resource file
        /// </summary>
        /// <param name="methodInstance">The name of executing method</param>
        /// <returns></returns>
        static internal double[] getRates(string methodInstance)
        {
            string volsList = ResourceHelper.ReadResourceValue(TestHelper._resourceFile, methodInstance);
            string[] vols = volsList.Split(';');

            double[] theVols = new double[vols.Length];
            GetInputs<double>(vols, theVols);

            return theVols;
        }

        
        /// <summary>
        /// Get the dates out of resource file and return that
        /// in the format of DateTime
        /// </summary>
        /// <param name="methodInstance">The name of executing method</param>
        /// <returns></returns>
        static internal DateTime[] getDates(string methodInstance)
        {
            string rollDatesList = ResourceHelper.ReadResourceValue(TestHelper._resourceFile, methodInstance);
            string[] rollDates = rollDatesList.Split(';');

            System.DateTime[] dates = new System.DateTime[rollDates.Length];
            GetInputs<DateTime>(rollDates, dates);

            return dates;
        }

        /// <summary>
        /// Convert from string to different types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inputData"></param>
        /// <param name="outputData"></param>
        static internal void GetInputs<T>(string[] inputData, T[] outputData)
        {
            int  i = 0;

            foreach( string data in inputData)
            { 
                outputData[i] = (T) System.Convert.ChangeType( data,typeof(T));
                ++i;
            } 
        }


        /// <summary>
        /// Gets the executing class.
        /// </summary>
        /// <returns></returns>
        static internal string GetExecutingClass()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            return sf.GetMethod().DeclaringType.Name;
        }

        /// <summary>
        /// Gets the executing method.
        /// </summary>
        /// <returns></returns>
        static internal string GetExecutingMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            return sf.GetMethod().Name;
        }
        #endregion
    }
}