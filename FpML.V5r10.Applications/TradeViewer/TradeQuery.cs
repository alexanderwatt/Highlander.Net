#region Usings

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Core.Common;
using Core.V34;
using FpML.V5r3.Codes;
using FpML.V5r3.Reporting;
using Orion.Build;
using Orion.Constants;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Exception = System.Exception;

#endregion

namespace TradeViewer
{
    /// <summary>
    /// Provides read-only access to trades in the store.
    /// </summary>
    public class TradeQuery
    {
        #region Fields

        private static readonly EnvId BuildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv);

        private readonly ICoreCache _cache;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeQuery"/> class.
        /// </summary>
        public TradeQuery(ILogger logger)
        {
            var logRef = Reference<ILogger>.Create(logger);
            try
            {
                CoreClientFactory factory = new CoreClientFactory(logRef)
                    .SetEnv(BuildEnv.ToString())
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                //if (rbDefaultServers.Checked)
                //    _Client = factory.Create();
                //else if (rbLocalhost.Checked)
                var client = factory.SetServers("localhost").Create();
                _cache = client.CreateCache();
                //else
                //    _Client = factory.SetServers(txtSpecificServers.Text).Create();
                //_SyncContext.Post(OnClientStateChange, new CoreStateChange(CoreStateEnum.Initial, _Client.CoreState));
                //_Client.OnStateChange += new CoreStateHandler(_Client_OnStateChange);
            }
            catch (Exception excp)
            {
                logRef.Target.Log(excp);
            }
        }

        #endregion

        /// <summary>
        /// Returns the header information for all trades matching the query properties.
        /// </summary>
        /// <param name="query">The query properties. A 2-column array of names and values.</param>
        /// <returns></returns>
        public object[,] QueryTradeIds(object[,] query)
        {
            //try
            //{
                int rowMin = query.GetLowerBound(0);
                int rowMax = query.GetUpperBound(0);
                int colMin = query.GetLowerBound(1);
                int colMax = query.GetUpperBound(1);
                if (colMax - colMin + 1 != 3)
                    throw new ApplicationException("Input parameters must be 3 columns (name/op/value)!");
                IExpression whereExpr = null;
                for (int row = rowMin; row <= rowMax; row++)
                {
                    int colName = colMin + 0;
                    int colOp = colMin + 1;
                    int colValue = colMin + 2;
                    var name = query[row, colName] == null ? null : query[row, colName].ToString();
                    var op = query[row, colOp] == null ? null : query[row, colOp].ToString();
                    object value = query[row, colValue];
                    if (name != null && (op != null) && (value != null))
                    {
                        op = op.ToLower().Trim();
                        if (op == "equ" || op == "==")
                            whereExpr = Expr.BoolAND(Expr.IsEQU(name, value), whereExpr);
                        else if (op == "neq" || op == "!=")
                            whereExpr = Expr.BoolAND(Expr.IsNEQ(name, value), whereExpr);
                        else if (op == "geq" || op == ">=")
                            whereExpr = Expr.BoolAND(Expr.IsGEQ(name, value), whereExpr);
                        else if (op == "leq" || op == "<=")
                            whereExpr = Expr.BoolAND(Expr.IsLEQ(name, value), whereExpr);
                        else if (op == "gtr" || op == ">")
                            whereExpr = Expr.BoolAND(Expr.IsGTR(name, value), whereExpr);
                        else if (op == "lss" || op == "<")
                            whereExpr = Expr.BoolAND(Expr.IsLSS(name, value), whereExpr);
                        else if (op == "starts")
                            whereExpr = Expr.BoolAND(Expr.StartsWith(name, value.ToString()), whereExpr);
                        else if (op == "ends")
                            whereExpr = Expr.BoolAND(Expr.EndsWith(name, value.ToString()), whereExpr);
                        else if (op == "contains")
                            whereExpr = Expr.BoolAND(Expr.Contains(name, value.ToString()), whereExpr);
                        else
                            throw new ApplicationException("Unknown Operator: '" + op + "'");
                    }
                }
                List<ICoreItem> items = _cache.LoadItems(typeof(Trade), whereExpr);//TODO what about confirmation?
                var result = new object[items.Count + 1, 17];
                // add heading row
                result[0, 0] = TradeProp.ProductType;
                result[0, 1] = TradeProp.TradeId;
                result[0, 2] = TradeProp.TradeDate;
                result[0, 3] = TradeProp.MaturityDate;
                result[0, 4] = TradeProp.EffectiveDate;
                result[0, 5] = TradeProp.TradeState;
                result[0, 6] = TradeProp.RequiredCurrencies;
                result[0, 7] = TradeProp.RequiredPricingStructures;
                result[0, 8] = TradeProp.ProductTaxonomy;
                result[0, 9] = TradeProp.AsAtDate;
                result[0, 10] = EnvironmentProp.SourceSystem;
                result[0, 11] = TradeProp.TradingBookId;
                result[0, 12] = TradeProp.TradingBookName;
                result[0, 13] = TradeProp.BaseParty;
                result[0, 14] = TradeProp.Party1;
                result[0, 15] = TradeProp.CounterPartyName;
                result[0, 16] = TradeProp.Party2;
                // now add data rows
                int tradeNum = 1;
                foreach (ICoreItem item in items)
                {
                    try
                    {
                        //var fpmlTrade = (Trade)item.Data;
                        result[tradeNum, 0] = item.AppProps.GetValue<string>(TradeProp.ProductType);
                        result[tradeNum, 1] = item.AppProps.GetValue<string>(TradeProp.TradeId);
                        result[tradeNum, 2] = item.AppProps.GetValue<DateTime>(TradeProp.TradeDate).ToShortDateString();
                        result[tradeNum, 3] = item.AppProps.GetValue<DateTime>(TradeProp.MaturityDate).ToShortDateString();
                        result[tradeNum, 4] = item.AppProps.GetValue<DateTime>(TradeProp.EffectiveDate).ToShortDateString();
                        result[tradeNum, 5] = item.AppProps.GetValue<string>(TradeProp.TradeState);
                        result[tradeNum, 6] = String.Join(";", item.AppProps.GetArray<string>(TradeProp.RequiredCurrencies));
                        result[tradeNum, 7] = String.Join(";", item.AppProps.GetArray<string>(TradeProp.RequiredPricingStructures));
                        result[tradeNum, 8] = item.AppProps.GetValue<string>(TradeProp.ProductTaxonomy, null);
                        result[tradeNum, 9] = item.AppProps.GetValue<DateTime>(TradeProp.AsAtDate).ToShortDateString();
                        result[tradeNum, 10] = item.AppProps.GetValue<string>(EnvironmentProp.SourceSystem);
                        result[tradeNum, 11] = item.AppProps.GetValue<string>(TradeProp.TradingBookId);
                        result[tradeNum, 12] = item.AppProps.GetValue<string>(TradeProp.TradingBookName);
                        result[tradeNum, 13] = item.AppProps.GetValue<string>(TradeProp.BaseParty);
                        result[tradeNum, 14] = item.AppProps.GetValue<string>(TradeProp.Party1);
                        result[tradeNum, 15] = item.AppProps.GetValue<string>(TradeProp.CounterPartyName);
                        result[tradeNum, 16] = item.AppProps.GetValue<string>(TradeProp.OriginatingPartyName);
                        tradeNum++;
                    }
                    catch (Exception e)
                    {
                        Debug.Print("TradeStoreExcelApi.QueryTrades: Exception: {0}", e);
                    }
                }
            //}
            //catch (Exception e)
            //{
            //    Debug.Print("TradeStoreExcelApi.QueryTrades: Exception: {0}", e);
            //}
            //finally
            //{
            //    Debug.Print("TradeStoreExcelApi.QueryTrades: Leave");
            //}
            return result;
        }
    }
}
