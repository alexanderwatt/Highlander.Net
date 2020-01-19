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
using System.Text;

namespace Highlander.Utilities.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public static string ReplaceDateTimeTokens(string input, DateTimeOffset date)
        {
            // a fairly dumb but quick token replacer
            if (input == null)
                return null;
            bool inToken = false;
            string token = null;
            var sb = new StringBuilder();
            foreach (char ch in input)
            {
                if (inToken)
                {
                    if (ch == '}')
                    {
                        // end token
                        sb.Append(date.ToString(token));
                        token = null;
                        inToken = false;
                    }
                    else
                        token += ch;
                }
                else
                {
                    if (ch == '{')
                    {
                        inToken = true;
                    }
                    else
                        sb.Append(ch);
                }
            }
            return sb.ToString();
        }
    }
}
