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

using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.Helpers;

namespace FpML.V5r10.Reporting.Analytics.Stochastics.SABR
{
    /// <summary>
    /// A class to model a simple SABR Volatility key
    /// The key is the option/tenor pair that will identify an engine.
    /// For an ATM engine the tenor will be defaulted to ATM
    /// This version of a SABR key implements the <see cref="IEqualityComparer{T}"/> interface
    /// for use in Generic unsorted dictionary classes.
    /// To allow sorting this key implements the <see cref="IComparer{T}"/> interface
    /// for use in a sorted dictionary/lists.
    /// </summary>
    public class SABRKey : IEqualityComparer<SABRKey>, IComparer<SABRKey>
    {
        /// <summary>
        /// A default constant defining the tenor value to use when operating with ATM Engine keys
        /// </summary>
        private const string DefaultTenor = "ATM";

        #region Properties

        ///<summary>
        ///</summary>
        public string Expiry { get; set; }

        ///<summary>
        ///</summary>
        public string Tenor { get; set; }

        ///<summary>
        ///</summary>
        public decimal ExpiryAsDecimal { get; set; }

        ///<summary>
        ///</summary>
        public decimal TenorAsDecimal { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// This should be used when setting an <see cref="IEqualityComparer{T}"/> for Dictionary Key comparisons
        /// </summary>
        public SABRKey()
        {
            Expiry = "0D";
            Tenor = "0D";
            ExpiryAsDecimal = 0.0m;
            TenorAsDecimal = 0.0m;
        }

        /// <summary>
        /// The key constructor to use with Full Calibration Engines
        /// </summary>
        /// <param name="expiry"></param>
        public SABRKey(string expiry)//TODO problem with ATM.
        {
            Expiry = expiry;
            Tenor = DefaultTenor;
            ExpiryAsDecimal = (decimal)PeriodHelper.Parse(Expiry).ToYearFraction();
            TenorAsDecimal = 0.0m;
        }

        /// <summary>
        /// The constructor to use with ATM Calibration engines.
        /// This version uses a default for the tenor value of the key.
        /// </summary>
        /// <param name="expiry"></param>
        /// <param name="tenor"></param>
        public SABRKey(string expiry, string tenor)
        {
            Expiry = expiry;
            Tenor = tenor;
            ExpiryAsDecimal = ConversionFactor(Expiry);
            TenorAsDecimal = ConversionFactor(Tenor);
        }

        #endregion

        #region IEqualityComparer<SABRKey> Members

        /// <summary>
        /// Implementation of the <see cref="IEqualityComparer{T}"/> interface
        /// to allow Key searches in the dictionary
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool Equals(SABRKey x, SABRKey y)
        {
            try
            {
                var match = y != null && x != null && x.ExpiryAsDecimal == y.ExpiryAsDecimal;
                if (match && (x.Tenor.ToLower() == DefaultTenor.ToLower() || y.Tenor.ToLower() == DefaultTenor.ToLower()))
                    return true;

                match = (match && (x.TenorAsDecimal == y.TenorAsDecimal));
                return match;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        ///<summary>
        ///
        ///                    Returns a hash code for the specified object.
        ///                
        ///</summary>
        ///
        ///<returns>
        ///
        ///                    A hash code for the specified object.
        ///                
        ///</returns>
        ///
        ///<param name="obj">
        ///                    The <see cref="T:System.Object" /> for which a hash code is to be returned.
        ///                </param>
        ///<exception cref="T:System.ArgumentNullException">
        ///                    The type of <paramref name="obj" /> is a reference type and <paramref name="obj" /> is null.
        ///                </exception>
        public int GetHashCode(SABRKey obj)
        {
            return base.GetHashCode();
        }

        #endregion

        #region IComparer<SABRKey> Members

        ///<summary>
        /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
        /// The rules are d->w->m->y for the alpha part of the term
        /// and normal numeric rules for the number part
        ///</summary>
        ///
        ///<returns>
        ///Value Condition Less than zero x is less than y. Zero x equals y. Greater than zero x is greater than y.
        ///</returns>
        ///
        ///<param name="y">The second object to compare.</param>
        ///<param name="x">The first object to compare.</param>
        public int Compare(SABRKey x, SABRKey y)
        {
            // Use null check logic
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            if (x.ExpiryAsDecimal < y.ExpiryAsDecimal)
                return -1;
            if (x.ExpiryAsDecimal > y.ExpiryAsDecimal)
                return 1;
            if (x.TenorAsDecimal == 0 || y.TenorAsDecimal == 0) // ATM catchall if the expiries are equal then it matches
                return 0;
            if (x.TenorAsDecimal < y.TenorAsDecimal)
                return -1;
            return x.TenorAsDecimal > y.TenorAsDecimal ? 1 : 0;
        }

        #endregion

        #region Object Overrides

        ///<summary>
        ///
        ///                    Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
        ///                
        ///</summary>
        ///
        ///<returns>
        ///true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />; otherwise, false.
        ///                
        ///</returns>
        ///
        ///<param name="obj">
        ///                    The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. 
        ///                </param>
        ///<exception cref="T:System.NullReferenceException">
        ///                    The <paramref name="obj" /> parameter is null.
        ///                </exception><filterpriority>2</filterpriority>
        public override bool Equals(object obj)
        {
            try
            {
                var y = (SABRKey)obj;
                var match = y != null && (Expiry == y.Expiry);
                if (match && (Tenor == DefaultTenor || y.Tenor == DefaultTenor))
                    return true;

                match = (match && (Tenor == y.Tenor));
                return match;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        ///<summary>
        ///
        ///                    Serves as a hash function for a particular type. 
        ///                
        ///</summary>
        ///
        ///<returns>
        ///
        ///                    A hash code for the current <see cref="T:System.Object" />.
        ///                
        ///</returns>
        ///<filterpriority>2</filterpriority>
        public override int GetHashCode() => base.GetHashCode();

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculate a conversion factor for the multiplier.
        /// The conversion will only check the initial letter of the period string for matching.
        /// The matches are d(ay), w(eek), m(onth) and y(ear).
        /// </summary>
        /// <param name="period"></param>
        /// <returns></returns>
        private static decimal ConversionFactor(string period)
        {
            if (period == "ATM" || period == "") return 0.0m;
            var mult = (decimal)PeriodHelper.Parse(period).ToYearFraction();
            return mult;
        }

        #endregion
    }
}