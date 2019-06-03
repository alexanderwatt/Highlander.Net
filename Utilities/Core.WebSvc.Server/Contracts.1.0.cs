/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Runtime.Serialization;
using System.ServiceModel;

#endregion

namespace Core.WebSvc.Server
{
    [DataContract]
    public class V101CoreItem
    {
        [DataMember]
        public Guid ItemId;

        [DataMember]
        public string ItemName;

        [DataMember]
        public string DataTypeName;

        [DataMember]
        public string ItemBody;
    }

    [DataContract]
    public class V101ErrorDetail
    {
        [DataMember]
        public string FullName;

        [DataMember]
        public string Message;

        [DataMember]
        public string Source;

        [DataMember]
        public string StackTrace;
        [DataMember]

        public V101ErrorDetail InnerError;

        // constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public V101ErrorDetail(Exception e)
        {
            FullName = e.GetType().FullName;
            Message = e.Message;
            Source = e.Source;
            StackTrace = e.StackTrace;
            if (e.InnerException != null)
                InnerError = new V101ErrorDetail(e.InnerException);
        }
    }

    [DataContract]
    public class V101ResultSet
    {
        [DataMember]
        public V101CoreItem[] Items;

        [DataMember]
        public V101ErrorDetail Error;
    }

    [ServiceContract]
    public interface IWebProxyV101
    {
        [OperationContract]
        V101ResultSet V101LoadObjectByName(string itemName);
    }

    public class WebProxyRecverV101 : IWebProxyV101
    {
        private readonly IWebProxyV101 _channel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="channel"></param>
        public WebProxyRecverV101(IWebProxyV101 channel)
        {
            _channel = channel ?? throw new ArgumentNullException(nameof(channel));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemName"></param>
        /// <returns></returns>
        public V101ResultSet V101LoadObjectByName(string itemName)
        {
            return _channel.V101LoadObjectByName(itemName);
        }
    }

}
