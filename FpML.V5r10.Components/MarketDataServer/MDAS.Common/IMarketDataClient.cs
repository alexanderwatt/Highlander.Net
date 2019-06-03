using System;
using Core.Common;
using FpML.V5r10.Reporting;
using Metadata.Common;
using Orion.Util.NamedValues;
using Orion.Util.Threading;
using Exception = System.Exception;

namespace Orion.V5r10.Reporting.Common
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
        MDSResult<QuotedAssetSet> GetPricingStructure(
            MDSProviderId provider,
            IModuleInfo clientInfo,
            Guid requestId,
            bool throwOnError,
            NamedValueSet requestParams,
            NamedValueSet structureParams);
        void StartRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            NamedValueSet requestParams,
            QuotedAssetSet standardQuotedAssetSet,
            TimeSpan subsLifetime,
            AsyncQueueCallback<QuotedAssetSet> callback);
        void CancelRealtimeFeed(
            IModuleInfo clientInfo,
            Guid subscriptionId,
            AsyncQueueCallback<QuotedAssetSet> callback);
    }
}
