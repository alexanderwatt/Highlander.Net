﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.0.30319.1.
// 
namespace Orion.V5r3.Configuration {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/GwmlEnumMaps.xsd")]
    [System.Xml.Serialization.XmlRootAttribute("enumMaps", Namespace="http://tempuri.org/GwmlEnumMaps.xsd", IsNullable=false)]
    public partial class EnumMaps {
        
        private EnumMap[] enumMapField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("enumMap")]
        public EnumMap[] enumMap {
            get {
                return this.enumMapField;
            }
            set {
                this.enumMapField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/GwmlEnumMaps.xsd")]
    public partial class EnumMap {
        
        private EnumMapValue[] enumMapValueField;
        
        private string nameField;
        
        private string gwmlDomainField;
        
        private string fpmlSchemeField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("enumMapValue")]
        public EnumMapValue[] enumMapValue {
            get {
                return this.enumMapValueField;
            }
            set {
                this.enumMapValueField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string gwmlDomain {
            get {
                return this.gwmlDomainField;
            }
            set {
                this.gwmlDomainField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string fpmlScheme {
            get {
                return this.fpmlSchemeField;
            }
            set {
                this.fpmlSchemeField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://tempuri.org/GwmlEnumMaps.xsd")]
    public partial class EnumMapValue {
        
        private string gwmlValueField;
        
        private string fpmlValueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string gwmlValue {
            get {
                return this.gwmlValueField;
            }
            set {
                this.gwmlValueField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string fpmlValue {
            get {
                return this.fpmlValueField;
            }
            set {
                this.fpmlValueField = value;
            }
        }
    }
}
