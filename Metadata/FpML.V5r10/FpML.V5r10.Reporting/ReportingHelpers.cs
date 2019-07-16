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

#region Usings

using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Reflection;
using Metadata.Common;

#endregion

namespace FpML.V5r10.Reporting
{
    public static class FpMLViewHelpers
    {
        private static readonly List<Type> _AllFpMLTypes = new List<Type>();
        private static IEnumerable<Type> AllFpMLTypes
        {
            get
            {
                lock (_AllFpMLTypes)
                {
                    if (_AllFpMLTypes.Count == 0)
                    {
                        _AllFpMLTypes.Add(typeof(Document));
                        _AllFpMLTypes.Add(typeof(Message));
                        _AllFpMLTypes.Add(typeof(ResponseMessage));
                        _AllFpMLTypes.Add(typeof(TradeCashflowsMatchResult));
                        _AllFpMLTypes.Add(typeof(PositionsMatchResults));
                        _AllFpMLTypes.Add(typeof(PositionsAcknowledged));
                        _AllFpMLTypes.Add(typeof(NettedTradeCashflowsMatchResult));
                        _AllFpMLTypes.Add(typeof(EventStatusResponse));
                        _AllFpMLTypes.Add(typeof(Acknowledgement));
                        _AllFpMLTypes.Add(typeof(InterestStatus));
                        _AllFpMLTypes.Add(typeof(SubstituteReturnConfirmationStatus));
                        _AllFpMLTypes.Add(typeof(SubstitutionStatus));
                        _AllFpMLTypes.Add(typeof(CollateralProposalStatus));
                        _AllFpMLTypes.Add(typeof(RequestCollateralAcceptance));
                        _AllFpMLTypes.Add(typeof(MarginCallStatus));
                        _AllFpMLTypes.Add(typeof(RequestMessage));
                        _AllFpMLTypes.Add(typeof(RequestPortfolio));
                        _AllFpMLTypes.Add(typeof(PositionsAsserted));
                        _AllFpMLTypes.Add(typeof(NonCorrectableRequestMessage));
                        _AllFpMLTypes.Add(typeof(NettedTradeCashflowsRetracted));
                        _AllFpMLTypes.Add(typeof(RequestRetransmission));
                        _AllFpMLTypes.Add(typeof(RequestEventStatus));
                        _AllFpMLTypes.Add(typeof(InterestStatusRetracted));
                        _AllFpMLTypes.Add(typeof(RequestInterestRetracted));
                        _AllFpMLTypes.Add(typeof(SubstitutionStatusRetracted));
                        _AllFpMLTypes.Add(typeof(RequestSubstitutionRetracted));
                        _AllFpMLTypes.Add(typeof(DisputeRetracted));
                        _AllFpMLTypes.Add(typeof(MarginCallStatusRetracted));
                        _AllFpMLTypes.Add(typeof(RequestMarginRetracted));
                        _AllFpMLTypes.Add(typeof(CorrectableRequestMessage));
                        _AllFpMLTypes.Add(typeof(RequestValuationReport));
                        _AllFpMLTypes.Add(typeof(RequestPositionReport));
                        _AllFpMLTypes.Add(typeof(NettedTradeCashflowsAsserted));
                        _AllFpMLTypes.Add(typeof(CreditEventNotification));
                        _AllFpMLTypes.Add(typeof(RequestInterest));
                        _AllFpMLTypes.Add(typeof(RequestSubstitution));
                        _AllFpMLTypes.Add(typeof(RequestMargin));
                        _AllFpMLTypes.Add(typeof(NotificationMessage));
                        _AllFpMLTypes.Add(typeof(ValuationReportRetracted));
                        _AllFpMLTypes.Add(typeof(ValuationReport));
                        _AllFpMLTypes.Add(typeof(ExposureReport));
                        _AllFpMLTypes.Add(typeof(TerminatingEventsReport));
                        _AllFpMLTypes.Add(typeof(ResetReport));
                        _AllFpMLTypes.Add(typeof(PositionReport));
                        _AllFpMLTypes.Add(typeof(PositionActivityReport));
                        _AllFpMLTypes.Add(typeof(PartyReport));
                        _AllFpMLTypes.Add(typeof(EventActivityReport));
                        _AllFpMLTypes.Add(typeof(TradeCashflowsAsserted));
                        _AllFpMLTypes.Add(typeof(CancelTradeCashflows));
                        _AllFpMLTypes.Add(typeof(ServiceNotification));
                        _AllFpMLTypes.Add(typeof(InterestStatement));
                        _AllFpMLTypes.Add(typeof(DisputeNotification));
                        _AllFpMLTypes.Add(typeof(Exception));
                        _AllFpMLTypes.Add(typeof(DataDocument));
                        _AllFpMLTypes.Add(typeof(ValuationDocument));
                    }
                    return _AllFpMLTypes.AsReadOnly();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static XmlSchemaSet GetSchemaSet()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = new XmlSchemaSet();
            foreach (string resourceName in assembly.GetManifestResourceNames())
            {
                if (resourceName.EndsWith(".xsd"))
                {
                    using (Stream s = assembly.GetManifestResourceStream(resourceName))
                        if (s != null)
                            using (var xr = new XmlTextReader(s))
                            {
                                result.Add(null, xr);
                            }
                }
            }
            result.Compile();
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="concreteOnly"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetFpMLTypes(bool concreteOnly)
        {
            if (concreteOnly)
                return AllFpMLTypes.Where(t => !t.IsAbstract);
            return AllFpMLTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementName"></param>
        /// <returns></returns>
        public static Type MapElementToType(string elementName)
        {
            foreach (Type type in GetFpMLTypes(false))
            {
                if (elementName.Equals(type.Name))
                    return type;
                if (type.GetCustomAttributes(false).OfType<XmlRootAttribute>().Select(attr => attr).Any(xmlRootAttr => xmlRootAttr.ElementName.Equals(elementName)))
                {
                    return type;
                }
            }
            throw new NotSupportedException("Unknown element name: " + elementName);
        }

        private static Type MapFirstElement(Stream stream)
        {
            using (XmlReader xr = new XmlTextReader(stream))
            {
                while (!xr.EOF)
                {
                    xr.Read();
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        return MapElementToType(xr.Name);
                    }
                }
                throw new FormatException("XML document has no elements!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="streamBytes"></param>
        /// <returns></returns>
        public static Type AutoDetectType(byte[] streamBytes)
        {
            using (var stream = new MemoryStream(streamBytes, false))
                return MapFirstElement(stream);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static Type AutoDetectType(string filename)
        {
            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return MapFirstElement(stream);
        }

        //public static XmlConversionMap GetIncomingConversionMap()
        //{
        //    XmlConversionMap result = new XmlConversionMap();
        //    // global text conversions
        //    result.AddGlobalTagConversion("bond", "underlyingAssert", typeof(Bond));

        //    // node-specific mappings
        //    result.AddChildMappingOld(".../bermudaExerciseDates", "date", "dateTime", null);
        //    //result.AddChildMapping(".../bermudaExerciseDates", "date", "dateTime", GetXsiTypeAttributeAsList(typeof(GWML_AbsoluteDate)));

        //    return result;
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<CustomXmlTransformRule> GetIncomingConversionMap()
        {
            var results = new List<CustomXmlTransformRule>
                {
                    new CustomXmlTransformRule(null, "bermudaExerciseDates", "date", null, "dateTime", null),
                    new CustomXmlTransformRule(null, "assets", "bond", null, "underlyingAsset", typeof (Bond)),
                    new CustomXmlTransformRule(null, "assets", "loan", null, "underlyingAsset", typeof (Loan)),
                    new CustomXmlTransformRule(null, "assets", "cash", null, "underlyingAsset", typeof (Cash)),
                    new CustomXmlTransformRule(null, "assets", "equity", null, "underlyingAsset", typeof (EquityAsset))
                };
            //results.Add(new CustomXmlTransformRule(null, "paymentDetail", "paymentRule", null, "paymentRule", typeof(PercentageRule)));
            return results;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<CustomXmlTransformRule> GetOutgoingConversionMap()
        {
            var results = new List<CustomXmlTransformRule>
                {
                    new CustomXmlTransformRule(null, "bermudaExerciseDates", "dateTime", null, "date", null),
                    new CustomXmlTransformRule(null, "assets", "underlyingAsset", typeof (Bond), "bond", null),
                    new CustomXmlTransformRule(null, "assets", "underlyingAsset", typeof (Loan), "loan", null),
                    new CustomXmlTransformRule(null, "assets", "underlyingAsset", typeof (Cash), "cash", null),
                    new CustomXmlTransformRule(null, "assets", "underlyingAsset", typeof (EquityAsset), "equity", null)
                };
            //results.Add(new CustomXmlTransformRule(null, "paymentDetail", "paymentRule", typeof(PercentageRule), "paymentRule", null));
            return results;
        }
    }
}
