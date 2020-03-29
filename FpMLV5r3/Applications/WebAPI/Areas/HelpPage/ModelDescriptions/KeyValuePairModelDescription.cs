namespace Highlander.WebAPI.V5r3.Areas.HelpPage.ModelDescriptions
{
    /// <summary>
    /// 
    /// </summary>
    public class KeyValuePairModelDescription : ModelDescription
    {
        /// <summary>
        /// 
        /// </summary>
        public ModelDescription KeyModelDescription { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ModelDescription ValueModelDescription { get; set; }
    }
}