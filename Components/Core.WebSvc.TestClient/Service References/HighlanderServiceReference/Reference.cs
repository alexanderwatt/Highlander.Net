﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Highlander.Core.WebSvc.TestClient.HighlanderServiceReference {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="V101ResultSet", Namespace="http://schemas.datacontract.org/2004/07/Highlander.Core.WebService")]
    [System.SerializableAttribute()]
    public partial class V101ResultSet : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ErrorDetail ErrorField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101CoreItem[] ItemsField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ErrorDetail Error {
            get {
                return this.ErrorField;
            }
            set {
                if ((object.ReferenceEquals(this.ErrorField, value) != true)) {
                    this.ErrorField = value;
                    this.RaisePropertyChanged("Error");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101CoreItem[] Items {
            get {
                return this.ItemsField;
            }
            set {
                if ((object.ReferenceEquals(this.ItemsField, value) != true)) {
                    this.ItemsField = value;
                    this.RaisePropertyChanged("Items");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="V101ErrorDetail", Namespace="http://schemas.datacontract.org/2004/07/Highlander.Core.WebService")]
    [System.SerializableAttribute()]
    public partial class V101ErrorDetail : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string FullNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ErrorDetail InnerErrorField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string MessageField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string SourceField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string StackTraceField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string FullName {
            get {
                return this.FullNameField;
            }
            set {
                if ((object.ReferenceEquals(this.FullNameField, value) != true)) {
                    this.FullNameField = value;
                    this.RaisePropertyChanged("FullName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ErrorDetail InnerError {
            get {
                return this.InnerErrorField;
            }
            set {
                if ((object.ReferenceEquals(this.InnerErrorField, value) != true)) {
                    this.InnerErrorField = value;
                    this.RaisePropertyChanged("InnerError");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Message {
            get {
                return this.MessageField;
            }
            set {
                if ((object.ReferenceEquals(this.MessageField, value) != true)) {
                    this.MessageField = value;
                    this.RaisePropertyChanged("Message");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string Source {
            get {
                return this.SourceField;
            }
            set {
                if ((object.ReferenceEquals(this.SourceField, value) != true)) {
                    this.SourceField = value;
                    this.RaisePropertyChanged("Source");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string StackTrace {
            get {
                return this.StackTraceField;
            }
            set {
                if ((object.ReferenceEquals(this.StackTraceField, value) != true)) {
                    this.StackTraceField = value;
                    this.RaisePropertyChanged("StackTrace");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="V101CoreItem", Namespace="http://schemas.datacontract.org/2004/07/Highlander.Core.WebService")]
    [System.SerializableAttribute()]
    public partial class V101CoreItem : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string DataTypeNameField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ItemBodyField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private System.Guid ItemIdField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ItemNameField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string DataTypeName {
            get {
                return this.DataTypeNameField;
            }
            set {
                if ((object.ReferenceEquals(this.DataTypeNameField, value) != true)) {
                    this.DataTypeNameField = value;
                    this.RaisePropertyChanged("DataTypeName");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ItemBody {
            get {
                return this.ItemBodyField;
            }
            set {
                if ((object.ReferenceEquals(this.ItemBodyField, value) != true)) {
                    this.ItemBodyField = value;
                    this.RaisePropertyChanged("ItemBody");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public System.Guid ItemId {
            get {
                return this.ItemIdField;
            }
            set {
                if ((this.ItemIdField.Equals(value) != true)) {
                    this.ItemIdField = value;
                    this.RaisePropertyChanged("ItemId");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ItemName {
            get {
                return this.ItemNameField;
            }
            set {
                if ((object.ReferenceEquals(this.ItemNameField, value) != true)) {
                    this.ItemNameField = value;
                    this.RaisePropertyChanged("ItemName");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="HighlanderServiceReference.IWebProxyV101")]
    public interface IWebProxyV101 {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebProxyV101/V101LoadObjectByName", ReplyAction="http://tempuri.org/IWebProxyV101/V101LoadObjectByNameResponse")]
        Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ResultSet V101LoadObjectByName(string itemName);
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IWebProxyV101/V101LoadObjectByName", ReplyAction="http://tempuri.org/IWebProxyV101/V101LoadObjectByNameResponse")]
        System.Threading.Tasks.Task<Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ResultSet> V101LoadObjectByNameAsync(string itemName);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IWebProxyV101Channel : Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.IWebProxyV101, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class WebProxyV101Client : System.ServiceModel.ClientBase<Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.IWebProxyV101>, Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.IWebProxyV101 {
        
        public WebProxyV101Client() {
        }
        
        public WebProxyV101Client(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public WebProxyV101Client(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebProxyV101Client(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public WebProxyV101Client(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ResultSet V101LoadObjectByName(string itemName) {
            return base.Channel.V101LoadObjectByName(itemName);
        }
        
        public System.Threading.Tasks.Task<Highlander.Core.WebSvc.TestClient.HighlanderServiceReference.V101ResultSet> V101LoadObjectByNameAsync(string itemName) {
            return base.Channel.V101LoadObjectByNameAsync(itemName);
        }
    }
}
