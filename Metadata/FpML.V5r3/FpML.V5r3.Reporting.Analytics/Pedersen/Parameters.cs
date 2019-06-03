/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Globalization;

namespace Orion.Analytics.Pedersen
{
    public class Parameters
    {
        #region declarations

        public int NFAC { get; set; }

        public int NEXPIRY { get; private set; }

        public int Ntenor { get; private set; }

        public int Uexpiry { get; set; }

        public int Utenor { get; set; }

        public int[] Expiry { get; private set; }

        public int[] Tenor { get; private set; }

        public int[] Timeframe { get; set; }

        public int[] SwpnExp { get; }

        public int[] SwpnTen { get; }

        public int Nswpn { get; set; }

        public int Ncplt { get; set; }

        public int Tcplt { get; set; }

        public bool CpltOn { get; set; }

        public bool SwpnOn { get; set; }

        public double MaxIvol { get; set; }

        public double MinIvol { get; set; }

        public double AvgSwpnIvol { get; set; }

        public double AvgCpltIvol { get; set; }

        #endregion

        public Parameters()
        {
            NFAC = 3;
            //SetExpiry(UEXPIRY);
            //SetTenor(UTENOR);
            SwpnExp = new[] { 1, 2, 4, 8, 12, 16, 20, 28, 40, 60, 80, 100, 120 };
            SwpnTen = new[] { 4, 8, 12, 16, 20, 24, 28, 32, 36, 40, 60, 80, 100, 120 };
            for (int i = 0; i < SwpnExp.Length; i++)
                SwpnExp[i]--;
            for (int i = 0; i < SwpnTen.Length; i++)
                SwpnTen[i]--;
        }

        public void Initialise()
        {
            SetExpiry(Uexpiry);
            SetTenor(Utenor);
            NEXPIRY = Expiry.Length - 1;
            Ntenor = Tenor.Length - 1;
            if (NFAC > Ntenor)
            {
                throw new Exception("To use " + NFAC.ToString(CultureInfo.InvariantCulture) + " factors, tenor must be at least " + Timeframe[NFAC].ToString(CultureInfo.InvariantCulture) + " quarters.");
            }
            if (Uexpiry > Utenor)
            {
                throw new Exception("Expiry cannot be greater than Tenor.");
            }
        }

        public void SetExpiry(int exp)
        {
            int exppos = RelativePositionOf(exp, Timeframe);
            Expiry = new int[exppos + 1];
            for (int i = 0; i < exppos; i++)
            {
                Expiry[i] = Timeframe[i];
            }
            Expiry[exppos] = exp;
        }

        public void SetTenor(int ten)
        {
            int tenpos = RelativePositionOf(ten, Timeframe);
            Tenor = new int[tenpos + 1];
            for (int i = 0; i < tenpos; i++)
            {
                Tenor[i] = Timeframe[i];
            }
            Tenor[tenpos] = ten;
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
