/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using Directives

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

#endregion

namespace Highlander.Utilities.Helpers
{
    public class Utilities
    {

        #region Information

        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public string GetAddInVersionInfo() => Information.GetVersionInfo();

        #endregion

        #region Helper Methods

        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public string GetCurveEngineVersionInfo()
        {
            return Information.GetVersionInfo();
        }

        /// <summary>
        /// Gets the Resource Version
        /// </summary>
        /// <returns></returns>
        public string GetResourcesVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private static void VectorAppend(ICollection<object> vector, [Optional] object arg)
        {
            if (!(arg is System.Reflection.Missing))
            {
                vector.Add(arg);
            }
        }

        #endregion

        #region Array Methods

        public object[] Vector(
            object arg1,
            object arg2 = null,
            object arg3 = null,
            object arg4 = null,
            object arg5 = null,
            object arg6 = null,
            object arg7 = null,
            object arg8 = null,
            object arg9 = null,
            object arg10 = null,
            object arg11 = null,
            object arg12 = null)
        {
            IList<object> vector = new List<object> { arg1 };
            VectorAppend(vector, arg2);
            VectorAppend(vector, arg3);
            VectorAppend(vector, arg4);
            VectorAppend(vector, arg5);
            VectorAppend(vector, arg6);
            VectorAppend(vector, arg7);
            VectorAppend(vector, arg8);
            VectorAppend(vector, arg9);
            VectorAppend(vector, arg10);
            VectorAppend(vector, arg11);
            VectorAppend(vector, arg12);
            return vector.ToArray();
        }

        /// <summary>
        /// Converts a string to upper case.
        /// </summary>
        /// <param name="input">The string to convert.</param>
        /// <returns>The upper case of the string.</returns>
        public string ToUpperCase(string input)
        {
            return input.ToUpper();
        }

        /// list of possible N/A strings in Excel
        /// 
        /// #NULL! 0x800a07d0
        /// #DIV/0! 0x800a07d7
        /// #VALUE! 0x800a07df
        /// #REF! 0x800a07e7
        /// #NAME? 0x800a07ed
        /// #NUM! 0x800a07f4
        /// #N/A 0x800a07fa
        private static readonly List<string> NAValues = new List<string>(new[] { "#NULL!", "#DIV/0!", "#VALUE!", "#REF!", "#NAME?", "#NUM!", "#N/A" });

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsNAString(string value)
        {
            return -1 != NAValues.IndexOf(value);
        }

        #endregion
    }
}