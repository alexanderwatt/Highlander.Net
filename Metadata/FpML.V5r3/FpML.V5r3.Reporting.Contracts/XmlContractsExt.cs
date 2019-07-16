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
using System.Security.Principal;
using Core.Common;
using FpML.V5r3.Codes;
using Orion.Constants;
using Orion.Util.NamedValues;

#endregion

namespace Orion.Contracts
{
    public partial class ExceptionDetail
    {
        public ExceptionDetail() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        public ExceptionDetail(Exception e)
        {
            fullNameField = e.GetType().FullName;
            messageField = e.Message;
            sourceField = e.Source;
            stackTraceField = e.StackTrace;
            if (e.InnerException != null)
                innerErrorField = new ExceptionDetail(e.InnerException);
        }

        /// <summary>
        /// 
        /// </summary>
        public string ShortName
        {
            get
            {
                string[] parts = fullNameField?.Split('.');
                return parts?[parts.Length - 1];
            }
        }
    }

    public partial class UserIdentity : IIdentity
    {
        public string AuthenticationType => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();

        public override string ToString()
        {
            return Name;
        }
    }

    public partial class PortfolioSpecification : ICoreObject
    {
        private void ValidateKeyProperties()
        {
            if (portfolioIdField == null)
                throw new ArgumentNullException(ValueProp.PortfolioId);
            new Guid(portfolioIdField);
        }
        private void ValidateAllProperties()
        {
            ValidateKeyProperties();
            if (ownerIdField != null)
            {
                if (ownerIdField.Name == null)
                    throw new ArgumentNullException($"Owner Id Name");
            }
        }

        public string NetworkKey => string.Format(NameSpace + ".Workflow.{0}", PrivateKey);

        protected virtual string OnBuildPrivateKey()
        {
            // base private key
            // - can be extended by derived types
            return $"PortfolioSpecification.{portfolioIdField}";
        }

        public string PrivateKey
        {
            get
            {
                ValidateKeyProperties();
                return OnBuildPrivateKey();
            }
        }

        public NamedValueSet AppProperties
        {
            get
            {
                ValidateAllProperties();
                var result = new NamedValueSet();
                result.Set(ValueProp.PortfolioId, new Guid(portfolioIdField));
                result.Set(ValueProp.OwnerId, ownerIdField);
                if (ownerIdField != null)
                    result.Set(RequestBase.Prop.Requester, ownerIdField.Name);
                return result;
            }
        }

        public bool IsTransient => false;

        public TimeSpan Lifetime => TimeSpan.MaxValue;

        // additional constructors
        public PortfolioSpecification() { }

        public PortfolioSpecification(string portfolioId, string nameSpace)
        {
            nameSpaceField = nameSpace;
            portfolioIdField = portfolioId;
        }
    }

    public partial class ResponseBase
    {
        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (requestIdField == null)
                throw new ArgumentNullException(RequestBase.Prop.RequestId);
            new Guid(requestIdField);
        }

        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
            if (requesterIdField == null)
                throw new ArgumentNullException($"UserId");
            if (requesterIdField.Name == null)
                throw new ArgumentNullException($"UserId.Name");
            if (retentionField != null)
                TimeSpan.Parse(retentionField);
            if (statusField == RequestStatusEnum.Undefined)
                throw new ArgumentNullException($"status");
        }

        protected override string OnBuildPrivateKey()
        {
            // base private key
            // - can be extended by derived types
            return $"Response.{requestIdField}";
        }

        protected override void OnSetProperties(NamedValueSet props)
        {
            props.Set(RequestBase.Prop.RequestId, new Guid(requestIdField));
            props.Set(RequestBase.Prop.Requester, requesterIdField.Name);
            props.Set(RequestBase.Prop.Status, statusField.ToString());
        }

        protected override TimeSpan OnGetLifetime()
        {
            if (retentionField == null)
                return TimeSpan.FromDays(1);
            return TimeSpan.Parse(retentionField);
        }
    }

    public partial class HandlerResponse
    {
        protected override string OnBuildPrivateKey()
        {
            return $"{base.OnBuildPrivateKey()}.{typeof(HandlerResponse).Name}";
        }
        public void IncrementItemsFailed()
        {
            if (!itemsFailedField.HasValue)
                itemsFailedField = 0;
            itemsFailedField++;
        }

        public void IncrementItemsPassed()
        {
            if (!itemsPassedField.HasValue)
                itemsPassedField = 0;
            itemsPassedField++;
        }

        public void IncrementTotalItems()
        {
            if (!itemCountField.HasValue)
                itemCountField = 0;
            itemCountField++;
        }
    }

    public partial class WorkerResponse
    {
        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (workerHostComputerField == null)
                throw new ArgumentNullException(RequestBase.Prop.WorkerHostComputer);
        }
        protected override string OnBuildPrivateKey()
        {
            return
                $"{base.OnBuildPrivateKey()}.{typeof(WorkerResponse).Name}.{workerHostComputerField}.{workerHostInstanceField ?? "Default"}";
        }
        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(RequestBase.Prop.WorkerHostComputer, workerHostComputerField);
            props.Set(RequestBase.Prop.WorkerHostInstance, workerHostInstanceField);
        }
    }

    public partial class WorkerAvailability
    {
        public class Const
        {
            public static TimeSpan LifeTime => TimeSpan.FromSeconds(30);
        }
        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (workerHostComputerField == null)
                throw new ArgumentNullException(RequestBase.Prop.WorkerHostComputer);
        }
        protected override string OnBuildPrivateKey()
        {
            return
                $"{typeof(WorkerAvailability).Name}.{workerHostComputerField}.{workerHostInstanceField ?? "Default"}";
        }
        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(RequestBase.Prop.WorkerHostComputer, workerHostComputerField);
            props.Set(RequestBase.Prop.WorkerHostInstance, workerHostInstanceField);
        }
        protected override bool OnIsTransient()
        {
            return true;
        }
        protected override TimeSpan OnGetLifetime()
        {
            return Const.LifeTime;
        }
    }

    public partial class ManagerResponse
    {
        protected override string OnBuildPrivateKey()
        {
            return $"{base.OnBuildPrivateKey()}.{typeof(ManagerResponse).Name}";
        }
    }

    public partial class UserIdentity
    {
        public UserIdentity() { }

        public UserIdentity(UserIdentity source)
        {
            nameField = source.Name;
            displayNameField = source.DisplayName;
        }
    }

    public partial class AssignedWorkflowRequest
    {
        public AssignedWorkflowRequest() { }

        public AssignedWorkflowRequest(RequestBase source) : base(source) { }

        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (workerHostComputerField == null)
                throw new ArgumentNullException(Prop.WorkerHostComputer);
        }

        protected override string OnBuildPrivateKey()
        {
            return
                $"{base.OnBuildPrivateKey()}.{typeof(AssignedWorkflowRequest).Name}.{workerHostComputerField}.{workerHostInstanceField ?? "Default"}";
        }

        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(Prop.WorkerHostComputer, workerHostComputerField);
            props.Set(Prop.WorkerHostInstance, workerHostInstanceField);
        }

        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
        }
    }

    public partial class UnassignedWorkflowRequest
    {
        public UnassignedWorkflowRequest() { }

        public UnassignedWorkflowRequest(RequestBase source) : base(source) { }

        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
        }

        protected override string OnBuildPrivateKey()
        {
            return $"{base.OnBuildPrivateKey()}.{typeof(UnassignedWorkflowRequest).Name}";
        }

        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(Prop.RequestDataType, requestDataTypeField);
            props.Set(Prop.RequestItemName, requestItemNameField);
        }

        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
            if (requestDataTypeField == null)
                throw new ArgumentNullException(Prop.RequestDataType);
            if (requestItemNameField == null)
                throw new ArgumentNullException(Prop.RequestItemName);
        }
    }

    public partial class TradeValuationRequest
    {
        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (tradeSourceField == null)
                throw new ArgumentNullException(TradeProp.TradeSource);
            if (tradeIdField == null)
                throw new ArgumentNullException(TradeProp.TradeId);
            if (tradeItemNameField == null)
                throw new ArgumentNullException("TradeItemName");
        }

        protected override string OnBuildPrivateKey()
        {
            return $"{base.OnBuildPrivateKey()}.{typeof(TradeValuationRequest).Name}.{tradeSourceField}.{tradeIdField}";
        }
        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
            if (tradeItemNameField == null)
                throw new ArgumentNullException("TradeItemName");
        }

        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(TradeProp.TradeId, tradeIdField);
            props.Set(TradeProp.TradeSource, tradeSourceField);
            props.Set(Prop.NameSpace, NameSpace);
        }
    }

    public partial class PortfolioValuationRequest
    {
        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (portfolioIdField == null)
                throw new ArgumentNullException(ValueProp.PortfolioId);
            // check string is valid guid
            new Guid(portfolioIdField);
        }

        protected override string OnBuildPrivateKey()
        {
            return $"{base.OnBuildPrivateKey()}.{typeof(PortfolioValuationRequest).Name}.{portfolioIdField}";
        }

        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(ValueProp.PortfolioId, new Guid(portfolioIdField));
            props.Set(Prop.NameSpace, NameSpace);
        }
    }

    public partial class PingHandlerRequest
    {
        public PingHandlerRequest() { }

        public PingHandlerRequest(RequestBase source) : base(source) { }

        protected override string OnBuildPrivateKey()
        {
            return $"{base.OnBuildPrivateKey()}.{typeof(PingHandlerRequest).Name}";
        }
    }

    public partial class PingWorkerRequest
    {
        public PingWorkerRequest() { }

        public PingWorkerRequest(RequestBase source) : base(source) { }

        protected override string OnBuildPrivateKey()
        {
            return $"{base.OnBuildPrivateKey()}.{typeof(PingWorkerRequest).Name}";
        }
    }

    public partial class WorkflowObject : ICoreObject
    {
        // virtual methods
        protected virtual void OnValidateKeyProperties() { }

        protected virtual void OnValidateOtherProperties() { }

        protected virtual string OnBuildPrivateKey() { throw new NotImplementedException(); }

        protected virtual void OnSetProperties(NamedValueSet props) { }

        protected virtual TimeSpan OnGetLifetime() { return TimeSpan.FromDays(1); }

        protected virtual bool OnIsTransient() { return false; }

        // ICoreObject methods
        private void ValidateKeyProperties() { OnValidateKeyProperties(); }

        private void ValidateAllProperties() { ValidateKeyProperties(); OnValidateOtherProperties(); }

        public string NetworkKey => String.Format(NameSpace + ".Workflow.{0}", PrivateKey);

        public string PrivateKey { get { ValidateKeyProperties(); return OnBuildPrivateKey(); } }

        public string NameSpace { get; set;}

        public NamedValueSet AppProperties
        {
            get
            {
                ValidateAllProperties();
                var result = new NamedValueSet();
                OnSetProperties(result);
                return result;
            }
        }

        public bool IsTransient => OnIsTransient();

        public TimeSpan Lifetime => OnGetLifetime();
    }

    public partial class RequestBase
    {
        public static class Prop
        {
            public const string NameSpace = "NameSpace";
            public const string RequestId = "RequestId";
            public const string Requester = "Requester";
            public const string WorkerHostComputer = "WorkerHostComputer";
            public const string WorkerHostInstance = "WorkerHostInstance";
            public const string Status = "Status";
            public const string RequestDataType = "RequestDataType";
            public const string RequestItemName = "RequestItemName";
        }

        public RequestBase() { }

        public RequestBase(RequestBase source)
        {
            NameSpace = source.NameSpace;
            requestIdField = source.RequestId;
            requesterIdField = new UserIdentity(source.RequesterId);
            retentionField = source.Retention;
            submitTimeField = source.SubmitTime;
            requestDescriptionField = source.RequestDescription;
            debugEnabledField = source.DebugEnabled;
            runAsUsernameField = source.RunAsUsername;
            runAsPasswordField = source.RunAsPassword;
        }

        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (requestIdField == null)
                throw new ArgumentNullException(Prop.RequestId);
            new Guid(requestIdField);
        }

        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
            if (requesterIdField == null)
                throw new ArgumentNullException("UserId");
            if (requesterIdField.Name == null)
                throw new ArgumentNullException("UserId.Name");
            if (retentionField != null)
                TimeSpan.Parse(retentionField);
        }
        protected override string OnBuildPrivateKey()
        {
            // base private key
            // - can be extended by derived types
            return $"Request.{requestIdField}";
        }
        protected override void OnSetProperties(NamedValueSet props)
        {
            props.Set(Prop.RequestId, new Guid(requestIdField));
            props.Set(Prop.Requester, requesterIdField.Name);
            props.Set(Prop.NameSpace, NameSpace);
        }

        protected override TimeSpan OnGetLifetime()
        {
            return retentionField == null ? TimeSpan.FromDays(1) : TimeSpan.Parse(retentionField);
        }

        public static void DispatchToWorker(ICoreCache cache, RequestBase request, string workerHostComputer, string workerHostInstance)
        {
            cache.SaveObject(new AssignedWorkflowRequest(request)
            {
                RequestDataType = request.GetType().FullName,
                RequestItemName = request.NetworkKey,
                WorkerHostComputer = workerHostComputer,
                WorkerHostInstance = workerHostInstance
            });
        }

        public static void TransferToWorker(ICoreCache cache, UnassignedWorkflowRequest request, string workerHostComputer, string workerHostInstance)
        {
            cache.SaveObject(new AssignedWorkflowRequest(request)
            {
                RequestDataType = request.RequestDataType,
                RequestItemName = request.RequestItemName,
                WorkerHostComputer = workerHostComputer,
                WorkerHostInstance = workerHostInstance
            });
        }

        public static void DispatchToManager(ICoreCache cache, RequestBase request)
        {
            string requestItemName = request.NetworkKey;
            if (cache.Proxy.LoadItem(requestItemName) == null)
                throw new ArgumentException($"The '{request.GetType().Name}' named '{requestItemName}' does not exist");
            cache.SaveObject(new UnassignedWorkflowRequest(request)
            {
                RequestDataType = request.GetType().FullName,
                RequestItemName = requestItemName
            });
        }
    }
}

