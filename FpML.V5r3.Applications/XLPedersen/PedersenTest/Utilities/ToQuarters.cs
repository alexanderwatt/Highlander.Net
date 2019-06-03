namespace PedersenHost.Utilities
{
    class ToQuarters
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
