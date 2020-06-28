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

using System;

namespace Highlander.Reporting.Analytics.V5r3.Pedersen
{
    public class Node
    {
        #region Declarations

        internal NodeOperation MyOp { get; set; }

        internal Node[] ChildNodes { get; set; }

        internal double Value { get; set; }

        #endregion

        public Node(string s, NodeOperation[] nodeOps)
        {
            try
            {
                s = s.Replace(" ", "");
                while (s.Contains("+-") || s.Contains("--") || s.Contains("++") || s.Contains("-+"))
                {
                    s = s.Replace("--", "+");
                    s = s.Replace("++", "+");
                    s = s.Replace("+-", "-");
                    s = s.Replace("-+", "-");
                }
                s = ParseHelper.RemoveOuterBracket(s);
                if (s[0] == '-')
                {
                    s = "0" + s;
                }
                foreach (NodeOperation t in nodeOps)
                {
                    Node[] tempNodes = t.Parse(s, nodeOps);
                    if (tempNodes != null)
                    {
                        MyOp = t;
                        ChildNodes = tempNodes;
                        return;
                    }
                }
                Value = double.Parse(s.Trim());
            }
            catch (Exception e)
            {
                if (e.Message.IndexOf("!!", StringComparison.Ordinal) != 0)
                {
                    throw new Exception($"!! Cannot parse \"{s}\".");
                }
                throw new Exception(e.Message);
            }
        }

        public static Node[] CreateNodes(string[] s, NodeOperation[] nodeOps)
        {
            var result = new Node[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                if (!string.IsNullOrEmpty(s[i]))
                {
                    result[i] = new Node(s[i], nodeOps);
                }
            }
            return result;
        }
        public double Evaluate()
        {
            if (MyOp == null)
            {
                return Value;
            }
            return MyOp.Evaluate(ChildNodes);
        }
    }
}
