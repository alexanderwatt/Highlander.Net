using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Common;
using Core.V34;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using nab.QDS.FpML.Codes;
using Orion.WebViewer.Properties;

namespace Orion.WebViewer.Trade.Business
{
    public class TradeProvider
    {
        private readonly ICoreCache _store;
        private readonly bool _storeIsMine;
        private readonly Reference<ILogger> _logRef;

        #region Constructors and Destructors

        public TradeProvider()
        {
            // create loggers
            _logRef = Reference<ILogger>.Create(new TraceLogger(true));
            // create client factory and client
            CoreClientFactory factory = new CoreClientFactory(_logRef)
            .SetEnv("DEV")
            .SetApplication(Assembly.GetExecutingAssembly())
            .SetProtocols(WcfConst.AllProtocolsStr)
            .SetServers("localhost");
            _store = factory.Create();
            _storeIsMine = true;
        }

        public TradeProvider(ICoreCache providedStore)
        {
            _store = providedStore;
        }

        public TradeProvider(Reference<ILogger> logger, ICoreCache providedStore)
        {
            _logRef = logger;
            _store = providedStore;
        }

        ~TradeProvider()
        {
            if (_storeIsMine)
                _store.Dispose();
        }

        #endregion

        public static IList<string> SupportedProductTypes
        {
            get 
            { 
                return new List<string>((new Settings()).SupportedProductTypes.Split(',')); 
            }
        }

        public Trade GetTrade(string tradeId)
        {
            if (string.IsNullOrEmpty(tradeId))
            {
                return null;
            }
            IExpression whereExpr = Expr.IsEQU(TradeProp.TradeId, tradeId);
            //ICoreItem result = _store.LoadItem<nab.QDS.FpML.V47.Trade>(tradeId);
            var result = _store.LoadItems<nab.QDS.FpML.V47.Trade>(whereExpr);
            if (result != null && result.Count < 2)
            {
                return new Trade(result.First());
            }
            return null;
        }

        public IEnumerable<Trade> GetTrades(DateTime? startDate, DateTime? endDate,
            string productType, string tradeId, int maximumRows)
        {
            if (!string.IsNullOrEmpty(tradeId))
            {
                return new List<Trade> {GetTrade(tradeId)};
            }
            IExpression whereExpr = CreateConditions(startDate, endDate, productType);
            //IExpression orderExpr = Expr.Prop("Id");
            //IExpression filter = Expr.BoolAND(whereExpr, orderExpr);
            var items = _store.LoadItems<nab.QDS.FpML.V47.Trade>(whereExpr);//filter
            var result = new List<Trade>();
            var totalLength = Math.Min(maximumRows, items.Count);
            for (var i = 0; i < totalLength; i++)
            {
                var trade = items[i];
                if (trade != null)
                {
                    result.Add(new Trade(trade));
                }
            }
            return result;
        }

        private static IExpression CreateConditions(DateTime? startDate, DateTime? endDate, string productType)
        {
            IExpression conditions = null;
            if (string.IsNullOrEmpty(productType))
            {
                conditions = Expr.ALL;
            }
            if (startDate != DateTime.MinValue && startDate != null)
            {
                conditions = Expr.IsGEQ(TradeProp.TradeDate, startDate);
            }
            if (endDate != DateTime.MaxValue && endDate != null)
            {
                conditions = Expr.BoolAND(Expr.IsLEQ(TradeProp.TradeDate, endDate), conditions);
            }
            if (!string.IsNullOrEmpty(productType))
            {
                conditions = Expr.BoolAND(productType == Resources.UserInteface.AllItems ? Expr.ALL : Expr.IsEQU(TradeProp.ProductType, productType), conditions);
            }
            return conditions;
        }

        public int GetTradesCount(DateTime? startDate, DateTime? endDate, string productType, 
            string tradeId, int maximumRows)
        {
            IExpression whereExpr = CreateConditions(startDate, endDate, productType);
            var items = _store.LoadItems<nab.QDS.FpML.V47.Trade>(whereExpr);
            return items.Count;
        }
    }
}
