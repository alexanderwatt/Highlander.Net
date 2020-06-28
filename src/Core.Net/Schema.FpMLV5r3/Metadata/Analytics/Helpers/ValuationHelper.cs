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
using System.Xml;
using Highlander.Utilities.Exception;
using Highlander.Utilities.Helpers;

namespace Highlander.Reporting.Analytics.V5r3.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public class ValuationHelper
    {
        const int CMinValidations = 3;

        private static Valuation CreateValuationFromXmlNode(XmlNode valuationNode)
        {
            DateTime date = XmlHelper.GetNodeValueDate("Date", valuationNode);
            var price = new decimal(XmlHelper.GetNodeValueDouble("Price", valuationNode));
            var valuation = new Valuation(date, price);
            return valuation;
        }

        /// <summary>
        /// Creates the valuations from XML node list.
        /// </summary>
        /// <param name="valuationNodeList">The valuation node list.</param>
        /// <returns></returns>
        public static List<Valuation> CreateValuationsFromXmlNodeList(XmlNodeList valuationNodeList)
        {
            var valuations = new List<Valuation>();
            foreach (XmlNode valuationNode in valuationNodeList)
            {
                Valuation valuation = CreateValuationFromXmlNode(valuationNode);
                AddValuation(valuation, valuations);                
            }
            if (valuations.Count >= CMinValidations)
                return valuations;
            throw new IncompleteInputDataException(String.Format("Insufficient valuations"));
        }

        /// <summary>
        /// Finds the valuation.
        /// </summary>
        /// <param name="valuation">The valuation.</param>
        /// <param name="valuationList">The valuation list.</param>
        /// <returns></returns>
        internal static Valuation FindValuation(Valuation valuation, List<Valuation> valuationList)
        {
            return valuationList.Find(valuationItem => DateTime.Compare(valuation.Date, valuationItem.Date) == 0
                );
        }

        /// <summary>
        /// Adds the valuation.
        /// </summary>
        /// <param name="valuation">The valuation.</param>
        /// <param name="valuationList">The valuation list.</param>
        internal static void AddValuation(Valuation valuation, List<Valuation> valuationList)
        {
            Valuation match = FindValuation(valuation, valuationList);
            if (match == null)
            {
                if (valuationList.Count == 0)
                    valuationList.Add(valuation);
                else
                {
                    List<Valuation> valuationsFound = valuationList.FindAll(
                        valuationItem => (valuationItem.Date < valuation.Date)
                        );               
                    valuationList.Insert(valuationsFound.Count, valuation);                   
                }
            }
            else
            {
                throw new DuplicateNotAllowedException(
                    $"A valuation date {valuation.Date} already exists on this surface");
            }                
        }
    }
}
