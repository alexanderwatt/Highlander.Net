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

#region Usings

using System;
using Highlander.Core.Common;
using Highlander.Metadata.Common;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.NamedValues;
using Highlander.Utilities.Threading;
using Exception = System.Exception;

#endregion

namespace Highlander.MDS.Client.V5r3
{
    public class MdpConfigName
    {
        public const string ProviderId = "providerid";
        // Bloomberg specific
        public const string BloombergCustName = "custname";
        public const string BloombergUUID = "uuid";
        public const string BloombergSID = "sid";
        public const string BloombergSidN = "sidn";
        public const string BloombergTsid = "tsid";
        public const string BloombergTSidN = "tsidn";
        // Reuters specific
        public const string ReutersIdleTimeoutSecs = "idletimeout";
        // provider prefix
        public const string ProviderPrefix = "Provider.";
    }

    public class MdsPropName
    {
        public const string EnvId = "EnvId";
        public const string Port = "Port";
        public const string Endpoints = "Endpoints";
        public const string EnabledProviders = "EnabledProviders";
    }

    public sealed class MDSException : Exception
    {
        public override string StackTrace { get; }

        //public MDSException(V212ErrorDetail error)
        //    : base(error.Message, error.InnerError != null ? new MDSException(error.InnerError) : null)
        //{
        //    Source = error.Source;
        //    _StackTrace = error.StackTrace;
        //}
        public MDSException(V221ErrorDetail error)
            : base(error.Message, error.InnerError != null ? new MDSException(error.InnerError) : null)
        {
            Source = error.Source;
            StackTrace = error.StackTrace;
        }

        //public override string Message
        //{
        //    get
        //    {
        //        return base.Message;
        //    }
        //}
    }

    public class MDSResult<T> where T : class
    {
        public T Result;
        public MDSException Error;
    }

    /// <summary>
    /// Interface for realtime market data clients.
    /// </summary>
    public interface IMarketDataClient : IDisposable
    {
        /// <summary>
        /// Gets the current data.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="clientInfo">The client info.</param>
        /// <param name="requestId">The request id.</param>
        /// <param name="throwOnError"> </param>
        /// <param name="requestParams">The request params.</param>
        /// <param name="quoteAssetSet"> </param>
        /// <returns></returns>
        MDSResult<QuotedAssetSet> GetMarketQuotes(
            MDSProviderId provider,
            IModuleInfo clientInfo,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            QuotedAssetSet quoteAssetSet);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="clientInfo"></param>
        /// <param name="requestId"></param>
        /// <param name="throwOnError"></param>
        /// <param name="requestParams"></param>
        /// <param name="structureParams"></param>
        /// <returns></returns>
        MDSResult<QuotedAssetSet> GetPricingStructure(
            MDSProviderId provider,
            IModuleInfo clientInfo,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            NamedValueSet structureParams);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="requestParams"></param>
        /// <param name="standardQuotedAssetSet"></param>
        /// <param name="subsLifetime"></param>
        /// <param name="callback"></param>
        void StartRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            NamedValueSet requestParams,
            QuotedAssetSet standardQuotedAssetSet,
            TimeSpan subsLifetime,
            AsyncQueueCallback<QuotedAssetSet> callback);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="callback"></param>
        void CancelRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            AsyncQueueCallback<QuotedAssetSet> callback);
    }
}
