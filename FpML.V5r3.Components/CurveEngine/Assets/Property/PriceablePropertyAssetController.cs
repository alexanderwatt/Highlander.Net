using System;
using Orion.ModelFramework.Assets;

namespace Orion.CurveEngine.Assets.Property
{
    ///<summary>
    ///</summary>
    public abstract class PriceablePropertyAssetController : AssetControllerBase, IPriceablePropertyAssetController
    {
        public decimal Price { get; private set; }
        public DateTime SettlementDate { get; set; }
        public string PropertyCurveName { get; set; }
        public decimal Multiplier { get; set; }
    }
}