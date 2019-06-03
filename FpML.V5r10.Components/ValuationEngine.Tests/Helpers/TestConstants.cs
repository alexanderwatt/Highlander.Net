using System.Collections.Generic;
using System.Diagnostics;

namespace Orion.ValuationEngine.Tests.Helpers
{
    public class TestConstants
    {
        public const string CyMLCounterpartyName = "AUSTIN STR LDG 7122";
        public const string CyMLFixedRateLoanBookName = "3111-RETAIL AUSTIN";
        public const string CyMLBookName = "1978-RET-AUSTIN LOANS";
    }

    public static class StringMatrix
    {
        public static void PrintToDebug(List<List<string>> matrix)
        {
            foreach (List<string> list in matrix)
            {
                foreach (string s in list)
                {
                    Debug.Write(s + "\t");
                }
                Debug.WriteLine("");
            }
        }

    }
}