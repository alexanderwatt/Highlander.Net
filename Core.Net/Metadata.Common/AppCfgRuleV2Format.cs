﻿//
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 

using System.Xml.Serialization;

namespace Highlander.Metadata.Common {
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true, Namespace="http://tempuri.org/AppCfgRuleV2Format.xsd")]
    [XmlRoot(Namespace="http://tempuri.org/AppCfgRuleV2Format.xsd", IsNullable=false)]
    public partial class AppCfgRuleSet {
        
        private string setNameField;
        
        private AppCfgRuleV2[] v2RulesField;
        
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
        [XmlElement("v2Rules")]
        public AppCfgRuleV2[] v2Rules {
            get => v2RulesField;
            set => v2RulesField = value;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(Namespace="http://tempuri.org/AppCfgRuleV2Format.xsd")]
    public partial class AppCfgRuleV2 {
        
        private int priorityField;
        
        private bool disabledField;
        
        private string envField;
        
        private string applNameField;
        
        private string hostNameField;
        
        private string userNameField;
        
        private string settingsField;
        
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
        public string Env {
            get {
                return this.envField;
            }
            set {
                this.envField = value;
            }
        }
        
        /// <remarks/>
        public string ApplName {
            get {
                return this.applNameField;
            }
            set {
                this.applNameField = value;
            }
        }
        
        /// <remarks/>
        public string HostName {
            get {
                return this.hostNameField;
            }
            set {
                this.hostNameField = value;
            }
        }
        
        /// <remarks/>
        public string UserName {
            get {
                return this.userNameField;
            }
            set {
                this.userNameField = value;
            }
        }
        
        /// <remarks/>
        public string Settings {
            get => settingsField;
            set => settingsField = value;
        }
    }
}
