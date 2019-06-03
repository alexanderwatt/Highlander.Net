using System.Configuration;

namespace National.QRSC.Engine.Tests.Helpers
{
    public class TestHelper
    {
        static private string defaultNabCapConfigLocation =  @"c:\Program Files\nabCapital\QR\Configuration";
        static string[] configFileList = { "Assets.config" };

        /// <summary>
        /// Initializes the <see cref="TestHelper"/> class.
        /// </summary>
        static TestHelper()
        {
            string overrideConfigLocation = ConfigurationManager.AppSettings["DefaultNabCapConfigLocation"];
            if (overrideConfigLocation.Length > 0)
            {
                defaultNabCapConfigLocation = overrideConfigLocation;
            }
        }
    }
}