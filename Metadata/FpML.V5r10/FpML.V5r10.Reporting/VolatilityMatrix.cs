namespace FpML.V5r10.Reporting
{
    public partial class VolatilityMatrix
    {
        private QuotedAssetSet inputsField;

        /// <summary>
        /// Extended property to hold any input assets
        /// </summary>
        public QuotedAssetSet inputs
        {
            get => inputsField;
            set => inputsField = value;
        }
    }
}
