using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Mvc;
using Highlander.Core.Interface.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Exception = System.Exception;

namespace Highlander.WebAPI.V5r3.Controllers
{
    /// <summary>
    /// Creates and views trades.
    /// </summary>
    public class PricingCacheController : ApiController
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
        public PricingCacheController()
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
        public IEnumerable<Trade> GetAllTrades()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IHttpActionResult GetTrade(string id)
        {
            var trade = PricingCache.GetTrade(id);
            return Ok(trade);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IHttpActionResult GetCurve(string id)
        {
            var pricingStructure = PricingCache.GetPricingStructure(id);
            return Ok(pricingStructure);
        }

        ///// <summary>
        ///// Creates a trade
        ///// </summary>
        ///// <param name="trade"></param>
        ///// <returns></returns>
        //public ActionResult Create(Trade trade)
        //{
        //    db.Add(person);
        //    db.SaveChanges();
        //    return Accepted();
        //}
    }
}