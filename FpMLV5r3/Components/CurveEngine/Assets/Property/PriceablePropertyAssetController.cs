using System;
using Highlander.Reporting.ModelFramework.V5r3.Assets;

namespace Highlander.CurveEngine.V5r3.Assets.Property
{
    ///<summary>
    ///</summary>
    public abstract class PriceablePropertyAssetController : AssetControllerBase, IPriceablePropertyAssetController
    {
        public decimal Price { get; set; }

        public DateTime SettlementDate { get; set; }

        public string PropertyCurveName { get; set; }

        public decimal Multiplier { get; set; }
    }
}