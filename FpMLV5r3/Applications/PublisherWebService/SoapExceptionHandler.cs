/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using System.Web.Services.Protocols;

namespace Highlander.PublisherWebService.V5r3
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
