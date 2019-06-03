using System;

namespace PedersenHost.PayoffParser
{
    class Node
    {
        #region declarations

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
                    throw new Exception(String.Format("!! Cannot parse \"{0}\".", s));
                }
                throw new Exception(e.Message);
            }
        }
        public static Node[] CreateNodes(string[] s, NodeOperation[] nodeOps)
        {
            var result = new Node[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != "")
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
