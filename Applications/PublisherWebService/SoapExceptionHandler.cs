using System;
using System.Web.Services.Protocols;

namespace Orion.PublisherWebService
{
    public class SoapExceptionHandler : SoapExtension 
    {
        public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
        {
            return null; 
        }

        public override object GetInitializer(Type serviceType)
        {
            return null;
        }

        public override void Initialize(object initializer)
        {
        }

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
