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

namespace Orion.Analytics.Utilities
{
    public static class ToQuarters
    {
        public static int Convert(string s)
        {
            int res = -1;
            if (s.Length==0)
            {
                return -1;
            }
            char c = s[s.Length - 1];
            string s2 = s.Substring(0, s.Length - 1).Trim();
            try
            {
                if (char.ToLower(c) == 'm')
                {
                    res = int.Parse(s2);
                    if (res % 3 == 0)
                    {
                        res = res / 3;
                    }
                    else
                    {
                        res = -1;
                    }
                }
                else if (char.ToLower(c) == 'y')
                {
                    res = 4 * int.Parse(s2);
                }
                return res;
            }
            catch
            {
                return -1;
            }
        }
    }
}
