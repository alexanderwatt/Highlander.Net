using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Core.Common;
using Core.V34;
using Orion.Util.Expressions;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using nab.QDS.FpML.V47;
using Orion.WebViewer.Properties;
using Orion.Constants;

namespace Orion.WebViewer.Curve.Business
{
    public class CurveProvider
    {
        private readonly ICoreClient _store;
        private readonly bool _storeIsMine;
        private readonly Reference<ILogger> _logRef;

        #region Constructors and destructors

        public CurveProvider()
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

        public CurveProvider(Reference<ILogger> logger, ICoreClient providedStore)
        {
            _logRef = logger;
            _store = providedStore;
        }

        public CurveProvider(ICoreClient providedStore)
        {
            _store = providedStore;
        }

        ~CurveProvider()
        {
            if(_storeIsMine)
                _store.Dispose();
        }

        #endregion

        public static IList<string> SupportedPricingStructures
        {
            get
            {
                return new List<string>((new Settings()).SupportedPricingStructures.Split(','));
            }
        }

        public Curve GetCurve(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }

            ICoreItem result = _store.LoadItem<Market>(id);
            var curve = new Curve(result);
            return curve;
        }

        public Surface GetCurveSurface(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            Curve curve = GetCurve(id);
            return curve.Surface;
        }

        /// <summary>
        /// Gets the curves.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        /// <param name="marketName">Name of the market.</param>
        /// <param name="pricingStructureType">Type of the pricing structure.</param>
        /// <param name="id">The id.</param>
        /// <param name="maximumRows">How many rows on the page.</param>
        /// <param name="startRowIndex">Where to start from.</param>
        /// <returns></returns>
        public IEnumerable<Curve> GetCurves(bool enabled, string marketName, string pricingStructureType, 
            string id, int maximumRows, int startRowIndex)
        {
            if (!enabled)
            {
                return new List<Curve>();
            }
            if (!string.IsNullOrEmpty(id))
            {
                return new List<Curve>{ GetCurve(id) };
            }
            IExpression whereExpr = CreateConditions(marketName, pricingStructureType);
            IExpression orderExpr = Expr.Prop(CurveProp.UniqueIdentifier);
            List<ICoreItem> query = _store.LoadItems(typeof(Market),whereExpr, orderExpr, startRowIndex, maximumRows);
            IEnumerable<Curve> curves = query.Select(a => new Curve(a));
            return curves;
        }

        private static IExpression CreateConditions(string marketName, string pricingStructureType)
        {
            IExpression conditions;
            if (!string.IsNullOrEmpty(marketName))
            {
                conditions = Expr.IsEQU(CurveProp.MarketAndDate, marketName);
            }
            else
            {
                conditions = Expr.IsNEQ(CurveProp.MarketAndDate, "");
                conditions = Expr.BoolAND(Expr.IsNotNull(CurveProp.MarketAndDate), conditions);
            }
            if (!string.IsNullOrEmpty(pricingStructureType) && pricingStructureType != Resources.UserInteface.AllItems)
            {
                conditions = Expr.BoolAND(Expr.IsEQU(CurveProp.PricingStructureType, pricingStructureType), conditions);
            }
            else
            {
                // If not specified, then at least make sure it has a pricingStructureType
                conditions = Expr.BoolAND(Expr.IsNEQ(CurveProp.PricingStructureType, ""), conditions);
                conditions = Expr.BoolAND(Expr.IsNotNull(CurveProp.PricingStructureType), conditions);
            }
            return conditions;
        }

        public int GetCurvesCount(bool enabled, string marketName, string pricingStructureType, 
            string id, int maximumRows, int startRowIndex)
        {
            if (!enabled)
            {
                return 0;
            }
            IExpression whereExpr = CreateConditions(marketName, pricingStructureType);
            int count = _store.CountObjects<Market>(whereExpr);
            return count;
        }
    }
}
