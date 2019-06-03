using System;
using System.Web.Services.Protocols;

namespace Orion.V5r10.PublisherWebService
{
    /// <summary>
    /// SoapExceptionHandler
    /// </summary>
    public class SoapExceptionHandler : SoapExtension 
    {
        /// <summary>
        /// GetInitializer
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null; 
        }

        /// <summary>
        /// GetInitializer
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public override object GetInitializer(Type serviceType)
        {
            return null;
        }

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="initializer"></param>
        public override void Initialize(object initializer)
        {
        }

        /// <summary>
        /// ProcessMessage
        /// </summary>
        /// <param name="message"></param>
        public override void ProcessMessage(SoapMessage message)
        {
            if (message.Stage == SoapMessageStage.AfterSerialize)
            {
                if (message.Exception != null)
                {
                    Global.LoggerRef.Target.LogError(message.Exception.ToString());
                }
            }
        }
    }
}
