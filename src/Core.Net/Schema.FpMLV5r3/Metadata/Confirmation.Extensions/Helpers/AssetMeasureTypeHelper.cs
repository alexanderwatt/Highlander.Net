using nab.QDS.FpML.Codes;

namespace nab.QDS.FpML.V47
{
    public class AssetMeasureTypeHelper
    {
        public static AssetMeasureType Parse(string measureTypeAsString)
        {
            // ensures value is valid enum string
            AssetMeasureEnum assetMeasureEnum = AssetMeasureEnum.Undefined;
            if (measureTypeAsString != null)
                assetMeasureEnum = AssetMeasureScheme.ParseEnumString(measureTypeAsString);
            AssetMeasureType assetMeasureType = Create(assetMeasureEnum);
            return assetMeasureType;
        }
        
        public static AssetMeasureType Create(AssetMeasureEnum assetMeasure)
        {
            var assetMeasureType = new AssetMeasureType {Value = assetMeasure.ToString()};
            return assetMeasureType;
        }

        public static AssetMeasureType Copy(AssetMeasureType assetMeasure)
        {
            var assetMeasureType = new AssetMeasureType { Value = assetMeasure.Value};
            return assetMeasureType;
        }
    }
}