using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.ModelFramework.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Exception = System.Exception;

namespace Highlander.WebAPI.V5r3.Controllers
{
    /// <summary>
    /// Creates and views trades.
    /// </summary>
    public class PricingController : ApiController
    {
        /// <summary>
        /// The logger
        /// </summary>
        public static readonly Reference<ILogger> LoggerRef = Reference<ILogger>.Create(new TraceLogger(true));

        /// <summary>
        /// 
        /// </summary>
        public PricingCache PricingCache;

        /// <summary>
        /// 
        /// </summary>
        public PricingController()
        {
            const string fullAppName = "Highlander.WebAPI.V5r3";
            LoggerRef.Target.LogInfo("Starting up...");
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                PricingCache = new PricingCache();
                stopwatch.Stop();
                Debug.Print("Initialized environment, in {0} seconds", stopwatch.Elapsed.TotalSeconds);
                LoggerRef.Target.LogInfo("Loaded..." + fullAppName);
            }
            catch (Exception excp)
            {
                LoggerRef.Target.Log(excp);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetAllPropertyTradeIdentifiers()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Trade GetPropertyTrade(string id)
        {
            //var identifier = "Trade.Reporting.Murex.swap.123456";
            var trade = PricingCache.GetTrade(id);
            return trade;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public PricingStructureData GetCurve(string id)
        {
            var pricingStructure = PricingCache.GetPricingStructure(id);
            return pricingStructure;
            //return Ok(pricingStructure);
        }

        ///// <summary>
        ///// Creates a trade
        ///// </summary>
        ///// <param name="trade"></param>
        ///// <returns></returns>
        //public ActionResult CreatePropertyTrade(Trade trade)
        //{
        //    PricingCache.Create(person);
        //    return Accepted();
        //}
    }
}