using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Highlander.WebAPI.V5r3.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 
    /// </summary>
    public class ParameterDescription
    {
        /// <summary>
        /// 
        /// </summary>
        public ParameterDescription()
        {
            Annotations = new Collection<ParameterAnnotation>();
        }

        /// <summary>
        /// 
        /// </summary>
        public Collection<ParameterAnnotation> Annotations { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Documentation { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ModelDescription TypeDescription { get; set; }
    }
}