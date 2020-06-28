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
using System.Collections.Generic;
using System.Text;
using Highlander.Constants;
using Highlander.Utilities.NamedValues;

namespace Highlander.CurveEngine.Helpers.V5r3
{
    ///<summary>
    ///</summary>
    public interface IPricingStructureTerms
    {
        ///<summary>
        ///</summary>
        string ReferenceKey { get; set; }
        ///<summary>
        ///</summary>
        string Market { get; set; }
        ///<summary>
        ///</summary>
        IDictionary<string, object> Terms { get; set; }

        ///<summary>
        ///</summary>
        ///<returns></returns>
        string BuildKey();
    }

    ///<summary>
    ///</summary>
    public interface IVolatilitySurfaceTerms : IPricingStructureTerms
    {
        ///<summary>
        ///</summary>
        string Currency { get; set; }
        ///<summary>
        ///</summary>
        DateTime BaseDate { get; set; }
    }

    ///<summary>
    ///</summary>
    public interface IRateCurveTerms : IPricingStructureTerms
    {
        ///<summary>
        ///</summary>
        string Currency { get; set; }
        ///<summary>
        ///</summary>
        string Tenor { get; set; }
        ///<summary>
        ///</summary>
        int Version { get; set; }
        ///<summary>
        ///</summary>
        DateTime BaseDate { get; set; }
    }

    ///<summary>
    ///</summary>
    public interface IFXCurveTerms : IPricingStructureTerms
    {
        ///<summary>
        ///</summary>
        string Currency1 { get; set; }
        ///<summary>
        ///</summary>
        string Currency2 { get; set; }
    }

    ///<summary>
    ///</summary>
    public class RateCurveTerms : IRateCurveTerms
    {
        ///<summary>
        ///</summary>
        public string CurveReferenceIdentifier { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ReferenceKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Tenor { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int Version { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object> Terms { get; set; }

        ///<summary>
        ///</summary>
        ///<param name="terms"></param>
        public RateCurveTerms(IDictionary<string, object> terms)
        {
            Terms = terms;
            var nvs = new NamedValueSet();
            nvs.Add(terms);

            if (nvs.Get("Version", false) != null)
                Version = Convert.ToInt32(nvs.GetValue("Version", 0.0));

            if (nvs.Get(CurveProp.Market, false) != null)
                Market = nvs.GetValue<string>(CurveProp.Market, null);
            else if (nvs.Get("Market", false) != null)
                Market = nvs.GetValue<string>("Market", null);

            if (nvs.Get(CurveProp.IndexTenor, false) != null)
                Tenor = nvs.GetValue<string>(CurveProp.IndexTenor, null);
            else if (nvs.Get("Tenor", false) != null)
                Tenor = nvs.GetValue<string>("Tenor", null);

            Currency = nvs.GetValue<string>(CurveProp.Currency1, null);


            BaseDate = nvs.GetValue(CurveProp.BaseDate, DateTime.MinValue);
            ReferenceKey = BuildKey();
            CurveReferenceIdentifier = null;
        }

        /// <summary>
        /// Builds the key.
        /// </summary>
        /// <returns></returns>
        public string BuildKey()
        {
            var sb = new StringBuilder();
            foreach (object term in Terms.Values)
            {
                string value = term.ToString();
                if (term is int i && i == 0)
                {
                    value = string.Empty;
                }

                sb.Append(value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurveTerms"/> class.
        /// </summary>
        /// <param name="curveReferenceIdentifier">The curve reference identifier.</param>
        public RateCurveTerms(string curveReferenceIdentifier)
        {
            CurveReferenceIdentifier = curveReferenceIdentifier;
            string[] curveParts = curveReferenceIdentifier.Split('-');
            if (curveParts.Length < 3)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid curve reference {0} specified. Curve Reference must be of format <MARKET>-<CURRENCY>-<TENOR>", curveReferenceIdentifier);
            }
            Market = curveParts[0];
            Currency = curveParts[1];
            Tenor = curveParts[2];
            if (curveParts.Length == 4)
            {
                BaseDate = DateTime.Parse(curveParts[curveParts.Length - 1]);
            }
            CurveReferenceIdentifier = null;
        }
    }

    ///<summary>
    ///</summary>
    public class FXCurveTerms : IFXCurveTerms
    {
        ///<summary>
        ///</summary>
        public string CurveReferenceIdentifier { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ReferenceKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency1 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency2 { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object> Terms { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FXCurveTerms"/> class.
        /// </summary>
        /// <param name="terms">The terms.</param>
        public FXCurveTerms(IDictionary<string, object> terms)
        {
            Terms = terms;
            var nvs = new NamedValueSet();
            nvs.Add(terms);
            Market = nvs.GetValue<string>("Market", null);
            Currency1 = nvs.GetValue<string>("Currency1", null);
            Currency2 = nvs.GetValue<string>("Currency2", null);
            ReferenceKey = BuildKey();
            CurveReferenceIdentifier = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FXCurveTerms"/> class.
        /// </summary>
        /// <param name="curveReferenceIdentifier">The curve reference identifier.</param>
        public FXCurveTerms(string curveReferenceIdentifier)
        {
            CurveReferenceIdentifier = curveReferenceIdentifier;
            string[] curveParts = curveReferenceIdentifier.Split('-');
            if (curveParts.Length < 2)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid curve reference {0} specified. Curve Reference must be of format <MARKET>-<CURRENCY1>-<CURRENCY2>", curveReferenceIdentifier);
            }
            Market = curveParts[0];
            Currency1 = curveParts[1];
            if (curveParts.Length == 3)
            {
                Currency2 = curveParts[2];
            }
            CurveReferenceIdentifier = null;
        }

        /// <summary>
        /// Builds the key.
        /// </summary>
        /// <returns></returns>
        public string BuildKey()
        {
            var sb = new StringBuilder();
            foreach (object term in Terms.Values)
            {
                sb.Append(term);
            }
            return sb.ToString();
        }
    }

    ///<summary>
    ///</summary>
    public class VolatilitySurfaceTerms : IVolatilitySurfaceTerms
    {
        ///<summary>
        ///</summary>
        public string SurfaceReferenceIdentifier { get; }

        /// <summary>
        /// 
        /// </summary>
        public string ReferenceKey { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Market { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public DateTime BaseDate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IDictionary<string, object> Terms { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilitySurfaceTerms"/> class.
        /// </summary>
        /// <param name="terms">The terms.</param>
        public VolatilitySurfaceTerms(IDictionary<string, object> terms)
        {
            Terms = terms;
            var nvs = new NamedValueSet();
            nvs.Add(terms);
            if (nvs.Get(CurveProp.Market, false) != null)
                Market = nvs.GetValue<string>(CurveProp.Market, null);
            else if (nvs.Get("Market", false) != null)
                Market = nvs.GetValue<string>("Market", null);
            Currency = nvs.GetValue<string>(CurveProp.Currency1, null);
            BaseDate = nvs.GetValue(CurveProp.BaseDate, DateTime.MinValue);
            ReferenceKey = BuildKey();
            SurfaceReferenceIdentifier = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VolatilitySurfaceTerms"/> class.
        /// </summary>
        /// <param name="surfaceReferenceIdentifier">The surface reference identifier.</param>
        public VolatilitySurfaceTerms(string surfaceReferenceIdentifier)
        {
            SurfaceReferenceIdentifier = surfaceReferenceIdentifier;
            string[] surfaceParts = surfaceReferenceIdentifier.Split('-');
            if (surfaceParts.Length < 2)
            {
                throw new ArgumentOutOfRangeException(
                    $"Invalid surface reference {0} specified. Surface Reference must be of format <MARKET>-<CURRENCY>", surfaceReferenceIdentifier);
            }
            Market = surfaceParts[0];
            Currency = surfaceParts[1];
            if (surfaceParts.Length == 3)
            {
                BaseDate = DateTime.Parse(surfaceParts[surfaceParts.Length - 1]);
            }
            SurfaceReferenceIdentifier = null;
        }

        /// <summary>
        /// Builds the key.
        /// </summary>
        /// <returns></returns>
        public string BuildKey()
        {
            var sb = new StringBuilder();
            foreach (object term in Terms.Values)
            {
                string value = term.ToString();
                if (term is int i && i == 0)
                {
                    value = string.Empty;
                }
                sb.Append(value);
            }
            return sb.ToString();
        }
    }
}