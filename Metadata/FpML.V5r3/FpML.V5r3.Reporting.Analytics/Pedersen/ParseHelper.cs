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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#endregion

namespace Orion.Analytics.Pedersen
{
    public class ParseHelper
    {
        private static readonly char[] Ops = new[] { '+', '-', '*', '/', '^' };

        public static string[] SpecialSplit(string s, char c)
        {
            var resultList = new List<string>();
            int i = 0;
            while (i < s.Length)
            {
                if (Bracket.IsOpen(s[i]))
                {
                    i = MatchBracket(s, i);
                }
                if (i > 0)
                {
                    if (c == s[i] && (!Ops.Contains(s[i - 1])))
                    {
                        resultList.Add(s.Substring(0, i));
                        s = s.Substring(i + 1);
                        i = -1;
                    }
                }
                i++;
            }
            resultList.Add(s);
            string[] result = resultList.ToArray();
            return result;
        }
        public static string RemoveOuterBracket(string s)
        {
            if (Bracket.IsOpen(s[0]))
            {
                if (MatchBracket(s, 0) == s.Length - 1)
                {
                    return RemoveOuterBracket(s.Substring(1, s.Length - 2));
                }
            }
            return s;
        }
        public static int MatchBracket(string s, int open)
        {
            string queue = s[open].ToString(CultureInfo.InvariantCulture);
            if (Bracket.IsOpen(s[open]))
            {
                for (int i = open + 1; i < s.Length; i++)
                {
                    if (Bracket.IsOpen(s[i]))
                    {
                        queue = s[i] + queue;
                    }
                    else if (s[i] == Bracket.Match(queue[0]))
                    {
                        queue = queue.Substring(1);
                    }
                    if (queue.Length == 0)
                    {
                        return i;
                    }
                }
            }
            else if (Bracket.IsClose(s[open]))
            {
                for (int i = open - 1; i >= 0; i--)
                {
                    if (Bracket.IsClose(s[i]))
                    {
                        queue = s[i] + queue;
                    }
                    else if (s[i] == Bracket.Match(queue[0]))
                    {
                        queue = queue.Substring(1);
                    }
                    if (queue.Length == 0)
                    {
                        return i;
                    }
                }
            }
            else
            {
                return -1;
            }
            throw new Exception("!! Brackets not matched");
        }
    }

    class Bracket
    {
        static readonly char[] Open = { '(', '{', '[' };
        static readonly char[] Close = { ')', '}', ']' };

        public static bool IsOpen(char c)
        {
            return Open.Contains(c);
        }
        public static bool IsClose(char c)
        {
            return Close.Contains(c);
        }
        public static char Match(char c)
        {
            for (int i = 0; i < Open.Length; i++)
            {
                if (Open[i] == c)
                {
                    return Close[i];
                }
            }
            for (int i = 0; i < Close.Length; i++)
            {
                if (Close[i] == c)
                {
                    return Open[i];
                }
            }
            return new char();
        }
    }
}
