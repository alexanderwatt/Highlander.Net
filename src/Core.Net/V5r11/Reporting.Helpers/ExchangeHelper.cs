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

using Highlander.Reporting.V5r3;

namespace Highlander.Reporting.Helpers.V5r3
{
    public static class ExchangeHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="country"></param>
        /// <param name="isoCountryCode"></param>
        /// <param name="mic"></param>
        /// <param name="operatingMIC"></param>
        /// <param name="os"></param>
        /// <param name="name"></param>
        /// <param name="acronym"></param>
        /// <param name="city"></param>
        /// <param name="website"></param>
        /// <param name="statusDate"></param>
        /// <param name="status"></param>
        /// <param name="creationDate"></param>
        /// <returns></returns>
        public static Exchange Create(string id, string country, string isoCountryCode, string mic, string operatingMIC, string os,
            string name, string acronym, string city, string website, string statusDate, string status, string creationDate)
        {
            var exchange = new Exchange
                {
                    Id = id,
                    Country = country,
                    ISOCountryCode = isoCountryCode,
                    MIC = mic,
                    OperatingMIC = operatingMIC,
                    OS = os,
                    Name = name,
                    Acronym = acronym,
                    City = city,
                    Website = website,
                    StatusDate = statusDate,
                    Status = status,
                    CreationDate = creationDate
                };
            return exchange;
        }
    }
}