/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net.git

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.IO;
using System.Reflection;

#endregion

namespace HLV5r3
{
    ///<summary>
    /// Information about the CurveGenerator assembly.
    ///</summary>
    public static class Information
    {
        ///<summary>
        /// Gets the version of the assembly
        ///</summary>
        ///<returns></returns>
        public static string GetVersionInfo()
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().GetName().CodeBase);
            var fileInfo = new FileInfo(uri.LocalPath);
            DateTime buildTime = fileInfo.LastWriteTime;
            string formattedInfo =
                $"Version: Highlander.V5r3 v1.0 Built on : {buildTime.ToShortDateString()}, {buildTime.ToShortTimeString()}";
            return formattedInfo;
        }
    }
}