using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace nab.QDS.FpML.V47
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
