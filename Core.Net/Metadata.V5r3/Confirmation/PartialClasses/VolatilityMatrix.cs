namespace FpML.V5r3.Confirmation
{
    public partial class VolatilityMatrix
    {
        private QuotedAssetSet inputsField;

        /// <summary>
        /// Extended property to hold any input assets
        /// </summary>
        public QuotedAssetSet inputs
        {
            get
            {
                return this.inputsField;
            }
            set
            {
                this.inputsField = value;
            }
        }
    }
}
