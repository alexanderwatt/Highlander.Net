/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Globalization;

namespace Highlander.Numerics.Pedersen
{
    public class Parameters
    {
        #region declarations

        public int NumberOfFactors { get; set; }

        public int NumberOfExpiries { get; private set; }

        public int NumberOfTenors { get; private set; }

        public int UnderlyingExpiry { get; set; }

        public int UnderlyingTenor { get; set; }

        public int[] Expiry { get; private set; }

        public int[] Tenor { get; private set; }

        public int[] Timeframe { get; set; }

        public int[] SwaptionExpiries { get; }

        public int[] SwaptionTenors { get; }

        public int NumberOfSwaptions { get; set; }

        public int NumberOfCaplets { get; set; }

        public int CapletTenors { get; set; }

        public bool CapletOn { get; set; }

        public bool SwaptionOn { get; set; }

        public double MaxImpliedVolatility { get; set; }

        public double MinImpliedVolatility { get; set; }

        public double AverageSwaptionImpliedVolatility { get; set; }

        public double AverageCapletImpliedVolatility { get; set; }

        #endregion

        public Parameters()
        {
            NumberOfFactors = 3;
            //SetExpiry(UnderlyingExpiry);
            //SetTenor(UnderlyingTenor);
            SwaptionExpiries = new[] { 1, 2, 4, 8, 12, 16, 20, 28, 40, 60, 80, 100, 120 };
            SwaptionTenors = new[] { 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 60, 80, 100, 120 };
            for (int i = 0; i < SwaptionExpiries.Length; i++)
                SwaptionExpiries[i]--;
            for (int i = 0; i < SwaptionTenors.Length; i++)
                SwaptionTenors[i]--;
        }

        public void Initialise()
        {
            SetExpiry(UnderlyingExpiry);
            SetTenor(UnderlyingTenor);
            NumberOfExpiries = Expiry.Length - 1;
            NumberOfTenors = Tenor.Length - 1;
            if (NumberOfFactors > NumberOfTenors)
            {
                throw new Exception("To use " + NumberOfFactors.ToString(CultureInfo.InvariantCulture) + " factors, tenor must be at least " + Timeframe[NumberOfFactors].ToString(CultureInfo.InvariantCulture) + " quarters.");
            }
            if (UnderlyingExpiry > UnderlyingTenor)
            {
                throw new Exception("Expiry cannot be greater than Tenor.");
            }
        }

        public void SetExpiry(int exp)
        {
            int expPos = RelativePositionOf(exp, Timeframe);
            Expiry = new int[expPos + 1];
            for (int i = 0; i < expPos; i++)
            {
                Expiry[i] = Timeframe[i];
            }
            Expiry[expPos] = exp;
        }

        public void SetTenor(int ten)
        {
            int tenPos = RelativePositionOf(ten, Timeframe);
            Tenor = new int[tenPos + 1];
            for (int i = 0; i < tenPos; i++)
            {
                Tenor[i] = Timeframe[i];
            }
            Tenor[tenPos] = ten;
        }

        public string OutputExpiry()
        {
            string str = "";
            for (int i = Expiry.GetLowerBound(0); i < Expiry.GetUpperBound(0); i++)
            {
                str = str + Expiry[i].ToString(CultureInfo.InvariantCulture) + ", ";
            }
            str = str + Expiry[Expiry.GetUpperBound(0)].ToString(CultureInfo.InvariantCulture);
            return str;
        }

        public string OutputTenor()
        {
            string str = "";
            for (int i = Tenor.GetLowerBound(0); i < Tenor.GetUpperBound(0); i++)
            {
                str = str + Tenor[i].ToString(CultureInfo.InvariantCulture) + ", ";
            }
            str = str + Tenor[Tenor.GetUpperBound(0)].ToString(CultureInfo.InvariantCulture);
            return str;
        }

        public static int RelativePositionOf(int x, int[] arr)
        {
            for (int i = arr.GetLowerBound(0); i <= arr.GetUpperBound(0); i++)
            {
                if (arr[i] >= x)
                {
                    return i;
                }
            }
            return arr.GetUpperBound(0) + 1;
        }

        public static int ExactPositionOf(int x, int[] arr)
        {
            for (int i = arr.GetLowerBound(0); i <= arr.GetUpperBound(0); i++)
            {
                if (arr[i] == x)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
