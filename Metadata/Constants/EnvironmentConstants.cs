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

namespace Highlander.Constants
{
    public static class EnvironmentProp
    {
        public const string Function = "Function";
        public const string DataGroup = "DataGroup";
        public const string DataType = "DataType";
        public const string SourceSystem = "SourceSystem";
        public const string Type = "Type";
        public const string Schema = "Schema";
        public const string NameSpace = "NameSpace";
        public const string Domain = "Domain";
        public const string DefaultNameSpace = "Highlander.V5r3";
        public const string OldNameSpace = "Highlander.V5r3";
        public const string LatestNameSpace = "Highlander.V5r10";
        public const string ClientName = "ClientName";
    }

    ///<summary>
    /// The function.
    ///</summary>
    public static class FpML5R10NameSpaces
    {
        public const string Confirmation = "Confirmation";
        public const string ConfirmationSchema = "V5r10.Confirmation";
        public const string Reporting = "Reporting";
        public const string ReportingSchema = "V5r10.Reporting";
    }

    ///<summary>
    /// The function.
    ///</summary>
    public static class FpML5R3NameSpaces
    {
        public const string Confirmation = "Confirmation";
        public const string ConfirmationSchema = "V5r3.Confirmation";
        public const string Reporting = "Reporting";
        public const string ReportingSchema = "V5r3.Reporting";
    }


    ///<summary>
    /// The function.
    ///</summary>
    public enum FunctionProp
    {
        Configuration,
        ReferenceData,
        Trade,
        Market,
        QuotedAssetSet,
        ValuationReport
    }

    ///<summary>
    /// The types of reference data.
    ///</summary>
    public enum ReferenceDataProp
    {
        HolidayDates,
        FixedIncome,
        Equity,
        Property
    }

    ///<summary>
    /// The curve building engine.
    ///</summary>
    public enum CurveCalculationProp
    {
        Murex,
        WallStreet,
        Spreadsheet,
        Highlander
    }
}
