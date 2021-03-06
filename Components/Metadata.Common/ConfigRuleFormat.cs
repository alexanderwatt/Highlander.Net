﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.17929.
// 
namespace Highlander.Metadata.Common {


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd", IsNullable=false)]
    public partial class BridgeConfigRule : ConfigRule {
        
        private string ruleNameField;
        
        private string sourceAddressField;
        
        private string targetAddressField;
        
        /// <remarks/>
        public string ruleName {
            get {
                return this.ruleNameField;
            }
            set {
                this.ruleNameField = value;
            }
        }
        
        /// <remarks/>
        public string sourceAddress {
            get {
                return this.sourceAddressField;
            }
            set {
                this.sourceAddressField = value;
            }
        }
        
        /// <remarks/>
        public string targetAddress {
            get {
                return this.targetAddressField;
            }
            set {
                this.targetAddressField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Highlander.Metadata.Common.FileImportRule))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Highlander.Metadata.Common.HostConfigRule))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Highlander.Metadata.Common.BridgeConfigRule))]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd")]
    public partial class ConfigRule {
        
        private int priorityField;
        
        private bool disabledField;
        
        private string hostEnvNameField;
        
        private string hostComputerField;
        
        private string hostInstanceField;
        
        private string hostUserNameField;
        
        /// <remarks/>
        public int Priority {
            get {
                return this.priorityField;
            }
            set {
                this.priorityField = value;
            }
        }
        
        /// <remarks/>
        public bool Disabled {
            get {
                return this.disabledField;
            }
            set {
                this.disabledField = value;
            }
        }
        
        /// <remarks/>
        public string hostEnvName {
            get {
                return this.hostEnvNameField;
            }
            set {
                this.hostEnvNameField = value;
            }
        }
        
        /// <remarks/>
        public string hostComputer {
            get {
                return this.hostComputerField;
            }
            set {
                this.hostComputerField = value;
            }
        }
        
        /// <remarks/>
        public string hostInstance {
            get {
                return this.hostInstanceField;
            }
            set {
                this.hostInstanceField = value;
            }
        }
        
        /// <remarks/>
        public string hostUserName {
            get {
                return this.hostUserNameField;
            }
            set {
                this.hostUserNameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd")]
    public partial class FileImportRule : ConfigRule {
        
        private bool debugEnabledField;
        
        private string debugPropertiesField;
        
        private string ruleNameField;
        
        private string monitorPeriodField;
        
        private int asAtDateOffsetField;
        
        private string otherPropertiesField;
        
        private string checkConstraintExprField;
        
        private string effectiveDateExprField;
        
        private string importConditionExprField;
        
        private string importDelayExprField;
        
        private string dateUpdateExprField;
        
        private string sourceSystemField;
        
        private string sourceLocationField;
        
        private string targetLocationField;
        
        private bool removeOldTargetFilesField;
        
        private bool onlyCopyUpdatedFilesField;
        
        private string copyFilePatternsField;
        
        private string fileContentTypeField;
        
        /// <remarks/>
        public bool DebugEnabled {
            get {
                return this.debugEnabledField;
            }
            set {
                this.debugEnabledField = value;
            }
        }
        
        /// <remarks/>
        public string DebugProperties {
            get {
                return this.debugPropertiesField;
            }
            set {
                this.debugPropertiesField = value;
            }
        }
        
        /// <remarks/>
        public string RuleName {
            get {
                return this.ruleNameField;
            }
            set {
                this.ruleNameField = value;
            }
        }
        
        /// <remarks/>
        public string MonitorPeriod {
            get {
                return this.monitorPeriodField;
            }
            set {
                this.monitorPeriodField = value;
            }
        }
        
        /// <remarks/>
        public int AsAtDateOffset {
            get {
                return this.asAtDateOffsetField;
            }
            set {
                this.asAtDateOffsetField = value;
            }
        }
        
        /// <remarks/>
        public string OtherProperties {
            get {
                return this.otherPropertiesField;
            }
            set {
                this.otherPropertiesField = value;
            }
        }
        
        /// <remarks/>
        public string CheckConstraintExpr {
            get {
                return this.checkConstraintExprField;
            }
            set {
                this.checkConstraintExprField = value;
            }
        }
        
        /// <remarks/>
        public string EffectiveDateExpr {
            get {
                return this.effectiveDateExprField;
            }
            set {
                this.effectiveDateExprField = value;
            }
        }
        
        /// <remarks/>
        public string ImportConditionExpr {
            get {
                return this.importConditionExprField;
            }
            set {
                this.importConditionExprField = value;
            }
        }
        
        /// <remarks/>
        public string ImportDelayExpr {
            get {
                return this.importDelayExprField;
            }
            set {
                this.importDelayExprField = value;
            }
        }
        
        /// <remarks/>
        public string DateUpdateExpr {
            get {
                return this.dateUpdateExprField;
            }
            set {
                this.dateUpdateExprField = value;
            }
        }
        
        /// <remarks/>
        public string SourceSystem {
            get {
                return this.sourceSystemField;
            }
            set {
                this.sourceSystemField = value;
            }
        }
        
        /// <remarks/>
        public string SourceLocation {
            get {
                return this.sourceLocationField;
            }
            set {
                this.sourceLocationField = value;
            }
        }
        
        /// <remarks/>
        public string TargetLocation {
            get {
                return this.targetLocationField;
            }
            set {
                this.targetLocationField = value;
            }
        }
        
        /// <remarks/>
        public bool RemoveOldTargetFiles {
            get {
                return this.removeOldTargetFilesField;
            }
            set {
                this.removeOldTargetFilesField = value;
            }
        }
        
        /// <remarks/>
        public bool OnlyCopyUpdatedFiles {
            get {
                return this.onlyCopyUpdatedFilesField;
            }
            set {
                this.onlyCopyUpdatedFilesField = value;
            }
        }
        
        /// <remarks/>
        public string CopyFilePatterns {
            get {
                return this.copyFilePatternsField;
            }
            set {
                this.copyFilePatternsField = value;
            }
        }
        
        /// <remarks/>
        public string FileContentType {
            get {
                return this.fileContentTypeField;
            }
            set {
                this.fileContentTypeField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd", IsNullable=false)]
    public partial class HostConfigRule : ConfigRule {
        
        private string buildConfigField;
        
        private string serverApplNameField;
        
        private string serverImplTypeField;
        
        private string serverAssmNameField;
        
        private string[] serverAssmPathField;
        
        private bool serverEnabledField;
        
        private System.Nullable<int> serverInstanceLocalStartField;
        
        private System.Nullable<int> serverInstanceLocalCountField;
        
        private System.Nullable<int> serverInstanceTotalCountField;
        
        /// <remarks/>
        public string buildConfig {
            get {
                return this.buildConfigField;
            }
            set {
                this.buildConfigField = value;
            }
        }
        
        /// <remarks/>
        public string serverApplName {
            get {
                return this.serverApplNameField;
            }
            set {
                this.serverApplNameField = value;
            }
        }
        
        /// <remarks/>
        public string serverImplType {
            get {
                return this.serverImplTypeField;
            }
            set {
                this.serverImplTypeField = value;
            }
        }
        
        /// <remarks/>
        public string serverAssmName {
            get {
                return this.serverAssmNameField;
            }
            set {
                this.serverAssmNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("serverAssmPath")]
        public string[] serverAssmPath {
            get {
                return this.serverAssmPathField;
            }
            set {
                this.serverAssmPathField = value;
            }
        }
        
        /// <remarks/>
        public bool serverEnabled {
            get {
                return this.serverEnabledField;
            }
            set {
                this.serverEnabledField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<int> serverInstanceLocalStart {
            get {
                return this.serverInstanceLocalStartField;
            }
            set {
                this.serverInstanceLocalStartField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<int> serverInstanceLocalCount {
            get {
                return this.serverInstanceLocalCountField;
            }
            set {
                this.serverInstanceLocalCountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(IsNullable=true)]
        public System.Nullable<int> serverInstanceTotalCount {
            get {
                return this.serverInstanceTotalCountField;
            }
            set {
                this.serverInstanceTotalCountField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/ConfigRuleFormat.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd", IsNullable=false)]
    public partial class HostConfigRuleSet {
        
        private string setNameField;
        
        private string defaultHostEnvNameField;
        
        private string defaultHostComputerField;
        
        private HostConfigRule[] hostConfigRuleField;
        
        /// <remarks/>
        public string setName {
            get {
                return this.setNameField;
            }
            set {
                this.setNameField = value;
            }
        }
        
        /// <remarks/>
        public string defaultHostEnvName {
            get {
                return this.defaultHostEnvNameField;
            }
            set {
                this.defaultHostEnvNameField = value;
            }
        }
        
        /// <remarks/>
        public string defaultHostComputer {
            get {
                return this.defaultHostComputerField;
            }
            set {
                this.defaultHostComputerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("hostConfigRule")]
        public HostConfigRule[] hostConfigRule {
            get {
                return this.hostConfigRuleField;
            }
            set {
                this.hostConfigRuleField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd", IsNullable=false)]
    public partial class ImportRuleSet {
        
        private FileImportRule[] importRulesField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ImportRules")]
        public Highlander.Metadata.Common.FileImportRule[] ImportRules {
            get {
                return this.importRulesField;
            }
            set {
                this.importRulesField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.17929")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/ConfigRuleFormat.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/ConfigRuleFormat.xsd", IsNullable=false)]
    public partial class HostConfigResult {
        
        private string hostEnvNameField;
        
        private string hostComputerField;
        
        private string hostInstanceField;
        
        private string hostUserNameField;
        
        private string serverApplNameField;
        
        private int serverInstanceField;
        
        private string serverImplTypeField;
        
        private bool serverRunningField;
        
        private string serverCommentField;
        
        /// <remarks/>
        public string hostEnvName {
            get {
                return this.hostEnvNameField;
            }
            set {
                this.hostEnvNameField = value;
            }
        }
        
        /// <remarks/>
        public string hostComputer {
            get {
                return this.hostComputerField;
            }
            set {
                this.hostComputerField = value;
            }
        }
        
        /// <remarks/>
        public string hostInstance {
            get {
                return this.hostInstanceField;
            }
            set {
                this.hostInstanceField = value;
            }
        }
        
        /// <remarks/>
        public string hostUserName {
            get {
                return this.hostUserNameField;
            }
            set {
                this.hostUserNameField = value;
            }
        }
        
        /// <remarks/>
        public string serverApplName {
            get {
                return this.serverApplNameField;
            }
            set {
                this.serverApplNameField = value;
            }
        }
        
        /// <remarks/>
        public int serverInstance {
            get {
                return this.serverInstanceField;
            }
            set {
                this.serverInstanceField = value;
            }
        }
        
        /// <remarks/>
        public string serverImplType {
            get {
                return this.serverImplTypeField;
            }
            set {
                this.serverImplTypeField = value;
            }
        }
        
        /// <remarks/>
        public bool serverRunning {
            get {
                return this.serverRunningField;
            }
            set {
                this.serverRunningField = value;
            }
        }
        
        /// <remarks/>
        public string serverComment {
            get {
                return this.serverCommentField;
            }
            set {
                this.serverCommentField = value;
            }
        }
    }
}
