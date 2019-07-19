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
using System.Collections.Generic;
using FpML.V5r10.Codes;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Identifiers;
using Orion.Constants;
using Orion.Util.NamedValues;

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class ValuationReportIdentifier : Identifier, IValuationReportIdentifier
    {
        /// <summary>
        /// Domain
        /// </summary>
        public String Domain { get; private set; }

        /// <summary>
        /// DataType
        /// </summary>
        public String DataType { get; private set; }

        /// <summary>
        /// CalculationDateTime
        /// </summary>
        public DateTime CalculationDateTime { get; set; }

        ///<summary>
        /// The Source System.
        ///</summary>
        public string SourceSystem { get; set; }

        ///<summary>
        /// The base party.
        ///</summary>
        public string BaseParty { get; set; }

        ///<summary>
        /// The parties.
        ///</summary>
        public List<Party> Parties { get; set; }

        ///<summary>
        /// The trades.
        ///</summary>
        public List<IIdentifier> TradeList { get; set; }

        /// <summary>
        /// Market
        /// </summary>
        public String MarketName { get; set; }

        /// <summary>
        /// TradeId
        /// </summary>
        public String TradeId { get; set; }

        ///<summary>
        /// An id for a trade.
        ///</summary>
        ///<param name="properties">The properties. These need to include:
        /// SourceSystem, Id and Trade date.</param>
        public ValuationReportIdentifier(NamedValueSet properties)
            : base(properties)
        {
            try
            {
                SetProperties(properties);
            }
            catch (System.Exception)
            {
                throw new System.Exception("Invalid reportid.");
            }
        }

        ///// <summary>
        /////  An id for a ratecurve.
        ///// </summary>
        ///// <param name="sourceSystem">The source system.</param>
        ///// <param name="baseParty">The base Party.</param>
        ///// <param name="reportId">The report Id.</param>
        ///// <param name="calculationDateTime">The calculationDateTime</param>
        //public ValuationReportIdentifier(String sourceSystem, string baseParty, string reportId, DateTime calculationDateTime)
        //{
        //    SetProperties(sourceSystem, baseParty, reportId, calculationDateTime);
        //}

        private void SetProperties(NamedValueSet properties)
        {
            try
            {
                DataType = "ValuationReport";
                SourceSystem = PropertyHelper.ExtractSourceSystem(properties);
                Id = properties.GetValue<string>(ValueProp.PortfolioId, false);
                TradeId = properties.GetValue<string>(TradeProp.TradeId, true);
                MarketName = properties.GetValue<string>(ValueProp.MarketName, false);
                Domain = SourceSystem + '.' + DataType;
                CalculationDateTime = properties.GetValue<DateTime>("CalculationDateTime");
                BaseParty = properties.GetValue<string>(ValueProp.BaseParty);
                UniqueIdentifier = BuildUniqueId();
                if (properties.GetValue<string>(CurveProp.UniqueIdentifier)!=null)
                {
                    UniqueIdentifier = properties.GetValue<string>(CurveProp.UniqueIdentifier);
                }
                PropertyHelper.Update(Properties, CurveProp.UniqueIdentifier, UniqueIdentifier);
                PropertyHelper.Update(Properties, "Domain", Domain);
            }
            catch
            {
                throw new System.Exception("Invalid tradeid.");
            }
        }

        //private void SetProperties(String sourceSystem, string baseParty, string reportId, DateTime calculationDateTime)
        //{
        //    try
        //    {
        //        DataType = "ValuationReport";
        //        SourceSystem = sourceSystem;
        //        Domain = SourceSystem + '.' + DataType;
        //        BaseParty = baseParty;
        //        CalculationDateTime = calculationDateTime;
        //        Id = reportId;
        //        UniqueIdentifier = BuildUniqueId();
        //        Properties = new NamedValueSet();
        //        Properties.Set("DataType", "ValuationReport");
        //        Properties.Set("SourceSystem", sourceSystem);
        //        Properties.Set("Domain", SourceSystem + '.' + DataType);
        //        Properties.Set("CalculationDateTime", CalculationDateTime);
        //        Properties.Set("Identifier", Id);
        //        Properties.Set(CurveProp.UniqueIdentifier, UniqueIdentifier);
        //    }
        //    catch
        //    {
        //        throw new System.Exception("Invalid reportid.");
        //    }
        //}

        /// <summary>
        /// Gets the build date string.
        /// </summary>
        /// <value>The build date string.</value>
        public string BuildDateString => CalculationDateTime.ToLongTimeString();

        /// <summary>
        /// Builds the id.
        /// </summary>
        /// <returns></returns>
        private string BuildUniqueId()
        {
            if (Id == null)
            {
                return $"{DataType}.{SourceSystem}.{TradeId}";
            }
            if (MarketName == null)
            {
                return $"{DataType}.{SourceSystem}.{Id}";//This is the old naming convention for backward compatibility.
            }
            return $"{DataType}.{Id}.{TradeId}.{MarketName}";
        }
    }
}