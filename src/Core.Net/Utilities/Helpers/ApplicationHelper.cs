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

#region Usings

using System;
using System.Reflection;

#endregion

namespace Highlander.Utilities.Helpers
{
    ///<summary>
    ///</summary>
    public static class ApplicationHelper
    {
        #region Diagnostics

        ///<summary>
        ///</summary>
        ///<returns></returns>
        public static object[,] Diagnostics()
        {
            var info = new AssemblyInfo(Assembly.GetExecutingAssembly());
            return info.ToArray();
        }

        ///<summary>
        ///</summary>
        ///<param name="itemName"></param>
        ///<returns></returns>
        public static string Diagnostics(string itemName)
        {
            var info = new AssemblyInfo(Assembly.GetExecutingAssembly());
            return info[itemName];
        }

        #endregion

        #region Com Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="subKeyName"></param>
        /// <returns></returns>
        public static string GetSubKeyName(Type type, string subKeyName)
        {
            var s = new System.Text.StringBuilder();
            s.Append(@"CLSID\{");
            s.Append(type.GUID.ToString().ToUpper());
            s.Append(@"}\");
            s.Append(subKeyName);
            return s.ToString();
        }

        #endregion
    }
}
