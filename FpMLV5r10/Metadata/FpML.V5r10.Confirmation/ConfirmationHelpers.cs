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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Metadata.Common;

namespace FpML.V5r10.Confirmation
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
                        _AllFpMLTypes.Add(typeof(EventStatusResponse));
                        _AllFpMLTypes.Add(typeof(Acknowledgement));
                        _AllFpMLTypes.Add(typeof(EventRequestAcknowledgement));
                        _AllFpMLTypes.Add(typeof(ConsentRefused));
                        _AllFpMLTypes.Add(typeof(ConsentGranted));
                        _AllFpMLTypes.Add(typeof(ConfirmationStatus));
                        _AllFpMLTypes.Add(typeof(ConfirmationDisputed));
                        _AllFpMLTypes.Add(typeof(ConfirmationAgreed));
                        _AllFpMLTypes.Add(typeof(AllocationRefused));
                        _AllFpMLTypes.Add(typeof(AllocationApproved));
                        _AllFpMLTypes.Add(typeof(RequestMessage));
                        _AllFpMLTypes.Add(typeof(NonCorrectableRequestMessage));
                        _AllFpMLTypes.Add(typeof(RequestRetransmission));
                        _AllFpMLTypes.Add(typeof(RequestEventStatus));
                        _AllFpMLTypes.Add(typeof(TradeChangeAdviceRetracted));
                        _AllFpMLTypes.Add(typeof(RequestTradeReferenceInformationUpdateRetracted));
                        _AllFpMLTypes.Add(typeof(RequestExecutionRetracted));
                        _AllFpMLTypes.Add(typeof(RequestConsentRetracted));
                        _AllFpMLTypes.Add(typeof(RequestClearingRetracted));
                        _AllFpMLTypes.Add(typeof(RequestAllocationRetracted));
                        _AllFpMLTypes.Add(typeof(ExecutionRetracted));
                        _AllFpMLTypes.Add(typeof(ExecutionAdviceRetracted));
                        _AllFpMLTypes.Add(typeof(ConfirmationRetracted));
                        _AllFpMLTypes.Add(typeof(CorrectableRequestMessage));
                        _AllFpMLTypes.Add(typeof(TradeChangeAdvice));
                        _AllFpMLTypes.Add(typeof(RequestTradeReferenceInformationUpdate));
                        _AllFpMLTypes.Add(typeof(RequestExecution));
                        _AllFpMLTypes.Add(typeof(RequestConsent));
                        _AllFpMLTypes.Add(typeof(RequestConfirmation));
                        _AllFpMLTypes.Add(typeof(RequestClearing));
                        _AllFpMLTypes.Add(typeof(RequestAllocation));
                        _AllFpMLTypes.Add(typeof(MaturityNotification));
                        _AllFpMLTypes.Add(typeof(ExecutionNotification));
                        _AllFpMLTypes.Add(typeof(ExecutionAdvice));
                        _AllFpMLTypes.Add(typeof(NotificationMessage));
                        _AllFpMLTypes.Add(typeof(ServiceNotification));
                        _AllFpMLTypes.Add(typeof(ClearingStatus));
                        _AllFpMLTypes.Add(typeof(ClearingRefused));
                        _AllFpMLTypes.Add(typeof(ClearingConfirmed));
                        _AllFpMLTypes.Add(typeof(Exception));
                        _AllFpMLTypes.Add(typeof(DataDocument));
                        _AllFpMLTypes.Add(typeof(ValuationDocument));
                    }
                    return _AllFpMLTypes.AsReadOnly();
                }
            }
        }

        public static XmlSchemaSet GetSchemaSet()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var result = new XmlSchemaSet();
            var resources = assembly.GetManifestResourceNames();
            foreach (string resourceName in resources)
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

        public static IEnumerable<Type> GetFpMLTypes(bool concreteOnly)
        {
            if (concreteOnly)
                return AllFpMLTypes.Where(t => !t.IsAbstract);
            return AllFpMLTypes;
        }

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
            using(var tr = new StreamReader(stream, Encoding.UTF8))
            using (XmlReader xr = new XmlTextReader(tr))
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

        public static Type AutoDetectType(byte[] streamBytes)
        {
            using (var stream = new MemoryStream(streamBytes, false))
                return MapFirstElement(stream);
        }

        public static Type AutoDetectType(string filename)
        {
            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                return MapFirstElement(stream);
        }

        public static List<CustomXmlTransformRule> GetIncomingConversionMap()
        {
            var results = new List<CustomXmlTransformRule>
                {
                    new CustomXmlTransformRule(null, "bermudaExerciseDates", "date", null, "dateTime", null)
                };
            //results.Add(new CustomXmlTransformRule(null, "paymentDetail", "paymentRule", null, "paymentRule", typeof(PercentageRule)));
            //results.Add(new CustomXmlTransformRule(null, null, "bond", null, "underlyingAsset", typeof(Bond)));
            return results;
        }

        public static List<CustomXmlTransformRule> GetOutgoingConversionMap()
        {
            var results = new List<CustomXmlTransformRule>
                {
                    new CustomXmlTransformRule(null, "bermudaExerciseDates", "dateTime", null, "date", null),
                    new CustomXmlTransformRule(null, "hourMinuteTime", "#text", CustomXmlTransformTextFormat.XsdTimeOnly),
                    new CustomXmlTransformRule(null, "effectiveFrom", "#text", CustomXmlTransformTextFormat.XsdDateTime),
                    new CustomXmlTransformRule(null, "effectiveTo", "#text", CustomXmlTransformTextFormat.XsdDateTime),
                    new CustomXmlTransformRule(null, null, "isCancellation", default(bool).ToString()),
                    new CustomXmlTransformRule(null, null, "componentSecurityIndexAnnexFallback", default(bool).ToString()),
                    new CustomXmlTransformRule(null, null, "masterAgreementPaymentDates", default(bool).ToString()),
                    new CustomXmlTransformRule(null, null, "optionalEarlyTermination", default(bool).ToString()),
                    new CustomXmlTransformRule(null, null, "spotRate",
                                               default(decimal).ToString(CultureInfo.InvariantCulture)),
                    new CustomXmlTransformRule(null, null, "forwardPoints",
                                               default(decimal).ToString(CultureInfo.InvariantCulture)),
                    new CustomXmlTransformRule(null, null, "optionEntitlement",
                                               default(decimal).ToString(CultureInfo.InvariantCulture)),
                    new CustomXmlTransformRule(null, null, "multiplier",
                                               default(decimal).ToString(CultureInfo.InvariantCulture)),
                    new CustomXmlTransformRule(null, null, "gross", default(decimal).ToString(CultureInfo.InvariantCulture))
                };
            //results.Add(new CustomXmlTransformRule(null, "paymentDetail", "paymentRule", typeof(PercentageRule), "paymentRule", null));
            //results.Add(new CustomXmlTransformRule(null, null, "underlyingAsset", typeof(Bond), "bond", null));

            // content formatting rules

            // default/empty node removal rules
            // 
            // 
            //results.Add(new CustomXmlTransformRule(null, null, "breakFeeElection", default(FeeElectionEnum).ToString()));
            //results.Add(new CustomXmlTransformRule(null, null, "breakFeeRate", default(decimal).ToString()));
            //
            return results;
        }
    }
}
