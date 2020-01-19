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

using System;
using System.Reflection;

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Reflection Helper class
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 
        /// <remarks>
        /// http://www.developmentnow.com/g/21_2004_12_0_0_9987/Excel-Automation-Add-In-Functions-and-C-question.htm
        /// 
        /// Following are the HRESULTs for the VT_ERROR:
        /// 
        /// #NULL! 0x800a07d0
        /// #DIV/0! 0x800a07d7
        /// #VALUE! 0x800a07df
        /// #REF! 0x800a07e7
        /// #NAME? 0x800a07ed
        /// #NUM! 0x800a07f4
        /// #N/A 0x800a07fa -2146826246
        /// </remarks>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="conversionType"></param>
        /// <returns></returns>
        public static object ChangeType(object value, Type conversionType)
        {
            // Check for #N/A first
            //
            if (value is int)
            {
                if (-2146826246 == (int)value)
                {
                    return null;
                }
            }
            // Special treatment for enumerations
            //
            if (conversionType.IsEnum)
            {
                var valueAsString = (string)Convert.ChangeType(value, typeof(string));
                return Enum.Parse(conversionType, valueAsString);
            }
            object result = Convert.ChangeType(value, conversionType);
            return result;
        }

        /// <summary>
        /// Gets the assembly code base location.
        /// </summary>
        /// <param name="assembly">The assembly.</param>
        /// <returns></returns>
        public static string GetAssemblyCodeBaseLocation(Assembly assembly)
        {
            string assemblyBasePath = System.IO.Path.GetDirectoryName(assembly.CodeBase);
            assemblyBasePath = assemblyBasePath.Replace(@"file:\", "");
            return assemblyBasePath;
        }
    }
}
