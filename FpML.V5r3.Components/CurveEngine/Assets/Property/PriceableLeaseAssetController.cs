using System;
using Orion.ModelFramework.Assets;

namespace Orion.CurveEngine.Assets.Property
{
    ///<summary>
    ///</summary>
    public abstract class PriceableLeaseAssetController : AssetControllerBase, IPriceableLeaseAssetController
    {
        public decimal Price { get; private set; }
        public DateTime SettlementDate { get; set; }
        public DateTime MaturityDate { get; set; }
        public decimal StartAmount { get; set; }
        public decimal Multiplier { get; set; }
        public decimal ChangeToBase { get; set; }
    }
}