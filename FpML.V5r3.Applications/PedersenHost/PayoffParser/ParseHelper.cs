using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace PedersenHost.PayoffParser
{
    class ParseHelper
    {
        private static readonly char[] Ops = new[] { '+', '-', '*', '/', '^' };

        public static string[] SpecialSplit(string s, char c)
        {
            var resultlist = new List<string>();
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
                        resultlist.Add(s.Substring(0, i));
                        s = s.Substring(i + 1);
                        i = -1;
                    }
                }
                i++;
            }
            resultlist.Add(s);
            string[] result = resultlist.ToArray();
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
        static readonly char[] Open = new[] { '(', '{', '[' };
        static readonly char[] Close = new[] { ')', '}', ']' };

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
