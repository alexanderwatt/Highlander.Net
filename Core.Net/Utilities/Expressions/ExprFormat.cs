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

using System.Xml.Serialization;

namespace Highlander.Utilities.Expressions {
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true, Namespace="http://tempuri.org/ExprFormat.xsd")]
    [XmlRoot(Namespace="http://tempuri.org/ExprFormat.xsd", IsNullable=false)]
    public partial class QuerySpec {
        
        private int versionField;
        
        private V1QueryExpr v1QueryExprField;
        
        /// <remarks/>
        public int version {
            get {
                return this.versionField;
            }
            set {
                this.versionField = value;
            }
        }
        
        /// <remarks/>
        public V1QueryExpr v1QueryExpr {
            get {
                return this.v1QueryExprField;
            }
            set {
                this.v1QueryExprField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.0.30319.1")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(Namespace="http://tempuri.org/ExprFormat.xsd")]
    public partial class V1QueryExpr {
        
        private V1QueryExpr[] argsField;
        
        private string nodeField;
        
        private string nameField;
        
        private string valueField;
        
        /// <remarks/>
        [XmlElement("args")]
        public V1QueryExpr[] args {
            get {
                return this.argsField;
            }
            set {
                this.argsField = value;
            }
        }
        
        /// <remarks/>
        [XmlAttribute()]
        public string node {
            get {
                return this.nodeField;
            }
            set {
                this.nodeField = value;
            }
        }
        
        /// <remarks/>
        [XmlAttribute()]
        public string name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [XmlAttribute()]
        public string value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
    }
}
