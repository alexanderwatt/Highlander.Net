﻿/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Runtime.Serialization;

#endregion

namespace Highlander.Metadata.Common
{
    // V221 contracts

    // V221 enums
    // Warning: Values are transmitted/persisted externally.
    //          Do not change enumeration order. Ever.
    /// <summary>
    /// 
    /// </summary>
    public enum V221ProviderId
    {
        Undefined,
        GlobalIB,
        Bloomberg,
        //ReutersIDN,
        Simulator
        //ReutersDTS,
    }

    /// <summary>
    /// 
    /// </summary>
    public class V221Helpers
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static MDSProviderId ToProviderId(V221ProviderId provider)
        {
            switch (provider)
            {
                case V221ProviderId.GlobalIB: return MDSProviderId.GlobalIB;
                case V221ProviderId.Bloomberg: return MDSProviderId.Bloomberg;
                //case V221ProviderId.ReutersIDN: return MDSProviderId.ReutersIDN;
                //case V221ProviderId.ReutersDTS: return MDSProviderId.ReutersDTS;
                case V221ProviderId.Simulator: return MDSProviderId.Simulator;
                default:
                    throw new NotSupportedException("V221ProviderId: " + provider);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static V221ProviderId ToV221ProviderId(MDSProviderId provider)
        {
            switch (provider)
            {
                case MDSProviderId.GlobalIB: return V221ProviderId.GlobalIB;
                case MDSProviderId.Bloomberg: return V221ProviderId.Bloomberg;
                //case MDSProviderId.ReutersIDN: return V221ProviderId.ReutersIDN;
                //case MDSProviderId.ReutersDTS: return V221ProviderId.ReutersDTS;
                case MDSProviderId.Simulator: return V221ProviderId.Simulator;
                default:
                    throw new NotSupportedException("MDSProviderId: " + provider);
            }
        }
    }

    [DataContract]
    public class V221Header
    {
        [DataMember]
        public readonly Guid SessionId;

        [DataMember]
        public readonly Guid RequestId;

        [DataMember]
        public readonly bool DebugRequest;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public V221Header() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        public V221Header(Guid sessionId)
        {
            SessionId = sessionId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="requestId"></param>
        /// <param name="debugRequest"></param>
        public V221Header(Guid sessionId, Guid requestId, bool debugRequest)
        {
            SessionId = sessionId;
            RequestId = requestId;
            DebugRequest = debugRequest;
        }
    }

    [DataContract]
    public class V221ErrorDetail
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
        public V221ErrorDetail InnerError;

        // constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public V221ErrorDetail(Exception e)
        {
            FullName = e.GetType().FullName;
            Message = e.Message;
            Source = e.Source;
            StackTrace = e.StackTrace;
            if (e.InnerException != null)
                InnerError = new V221ErrorDetail(e.InnerException);
        }
    }

    [DataContract]
    public class V221BeginSession
    {
        [DataMember]
        public readonly Guid Token;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public V221BeginSession() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        public V221BeginSession(Guid token) { Token = token; }
    }

    [DataContract]
    public class V221CloseSession
    {
        [DataMember]
        public readonly int Reserved;
    }

    [DataContract]
    public class V221OutputQuotedAssetSet
    {
        [DataMember]
        public readonly byte[] QuotedAssetSet; // serialised/compressed

        [DataMember]
        public readonly V221ErrorDetail Error;

        // constructors
        /// <summary>
        /// 
        /// </summary>
        public V221OutputQuotedAssetSet() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="error"></param>
        public V221OutputQuotedAssetSet(byte[] result, V221ErrorDetail error)
        {
            QuotedAssetSet = result;
            Error = error;
        }
    }
}
