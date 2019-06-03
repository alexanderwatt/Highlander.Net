namespace FpML.V5r10.Reporting.Helpers
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