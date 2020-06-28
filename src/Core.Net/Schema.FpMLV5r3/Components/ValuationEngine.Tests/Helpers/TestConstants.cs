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

using System.Collections.Generic;
using System.Diagnostics;

namespace Highlander.ValuationEngine.Tests.V5r3.Helpers
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