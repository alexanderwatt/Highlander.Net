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

#region Using directives

using System.IO;

#endregion

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Assembly File Info class
    /// </summary>
    public static class AssemblyFileInfo
    {
        /// <summary>
        /// Gets the executable UTC date time.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static System.DateTime GetExecutableUTCDateTime(string location)
        {
            var time = new System.DateTime(1970, 1, 1); // create base date

            using (var stream = new FileStream(location, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var reader = new BinaryReader(stream))
                {
                    stream.Position = 0x3c;
                    stream.Position = reader.ReadInt32() + 8; // goto TimeDateStamp
                    int stamp = reader.ReadInt32(); // read stamp
                    time = time.AddSeconds(stamp); // adjust date
                }
            }
            return time;
        }

        /// <summary>
        /// Gets the executable local date time.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns></returns>
        public static System.DateTime GetExecutableLocalDateTime(string location)
        {
            return GetExecutableUTCDateTime(location).ToLocalTime();
        }
    }
}