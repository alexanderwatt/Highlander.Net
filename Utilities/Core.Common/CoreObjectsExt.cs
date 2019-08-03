/*
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

using System;
using System.Collections.Generic;
using System.Security.Principal;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;

namespace Core.Common
{
    public partial class CoreObject : ICoreObject
    {
        // virtual methods
        protected virtual void OnValidateKeyProperties() { }
        protected virtual void OnValidateOtherProperties() { }
        protected virtual string OnBuildPrivateKey() { throw new NotImplementedException(); }
        protected virtual void OnSetProperties(NamedValueSet props) { }
        protected virtual bool OnIsTransient() { return false; }
        protected virtual TimeSpan OnGetLifetime()
        {
            if (OnIsTransient())
                return TimeSpan.FromDays(1);
            return TimeSpan.MaxValue;
        }
        // ICoreObject methods
        private void ValidateKeyProperties() { OnValidateKeyProperties(); }
        private void ValidateAllProperties() { ValidateKeyProperties(); OnValidateOtherProperties(); }
        public string NetworkKey => $"System.{PrivateKey}";
        public string PrivateKey { get { ValidateKeyProperties(); return OnBuildPrivateKey(); } }
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

    public partial class RuleObject
    {
        public static class Prop
        {
            public const string HostEnvAbbr = "HostEnvAbbr";
            public const string HostComputer = "HostComputer";
            public const string HostUserName = "HostUserName";
            public const string HostInstance = "HostInstance";
            public const string RuleUniqueId = "RuleUniqueId";
        }

        /// <summary>
        /// Makes a rule filter.
        /// </summary>
        /// <param name="envAbbr">The (optional) name of the application processing the rules.</param>
        /// <param name="hostName">The (optional) name of the computer running the rules processor.</param>
        /// <param name="userName">The (optional) name of the user account hosting the rules processor.</param>
        /// <param name="instance">The (optional) instance name for the rule processor.</param>
        /// <returns></returns>
        public static IExpression MakeRuleFilter(
            string envAbbr,     // optional environment short name
            string hostName,    // optional name of computer
            string instance,    // optional instance name
            string userName)    // optional name of the user
        {
            List<IExpression> clauses = new List<IExpression>();
            if (envAbbr != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(Prop.HostEnvAbbr), Expr.IsEQU(Prop.HostEnvAbbr, envAbbr)));
            if (hostName != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(Prop.HostComputer), Expr.IsEQU(Prop.HostComputer, hostName)));
            clauses.Add(instance != null ? Expr.IsEQU(Prop.HostInstance, instance) : Expr.IsNull(Prop.HostInstance));
            if (userName != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(Prop.HostUserName), Expr.IsEQU(Prop.HostUserName, userName)));
            return Expr.BoolAND(clauses.ToArray());
        }

        private string FormatRuleScope()
        {
            return
                $"{hostEnvAbbrField ?? "(all-envs)"}.{hostComputerField ?? "(all-host)"}.{hostInstanceField ?? "(all-inst)"}.{hostUserNameField ?? "(all-user)"}";
        }
        protected override TimeSpan OnGetLifetime() { return TimeSpan.FromDays(365.2425 * 1000.0); } // 1000 years
        protected override bool OnIsTransient() { return false; }
        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (ruleUniqueIdField == null)
                throw new ArgumentNullException("RuleUniqueId");
        }
        protected override string OnBuildPrivateKey()
        {
            return $"{typeof(AlertRule).Name}.{FormatRuleScope()}.{ruleUniqueIdField}";
        }
        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(Prop.HostEnvAbbr, hostEnvAbbrField);
            props.Set(Prop.HostComputer, hostComputerField);
            props.Set(Prop.HostUserName, hostUserNameField);
            props.Set(Prop.HostInstance, hostInstanceField);
            props.Set(Prop.RuleUniqueId, ruleUniqueIdField);
        }
        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
        }
    }

    public enum AlertStatus
    {
        Undefined,
        Disabled,
        Inactive,
        AllClear,
        Alerted
    }

    public partial class AlertRule
    {
        // property names
        public const string MailHost = "MailHost";
        public const string MailFrom = "MailFrom";
        public const string MailTo = "MailTo";
        // default addresses
        public const string GroupDefaultMailTo = "Default_MailTo";
        public const string UserDefaultReplyTo = "Default_ReplyTo";
    }

    public partial class AlertSignal : IComparable<AlertSignal>
    {
        public int CompareTo(AlertSignal signal)
        {
            // ascending RuleName
            var result = String.Compare(ruleUniqueIdField, signal.RuleUniqueId, StringComparison.OrdinalIgnoreCase);
            if (result != 0)
                return result;

            // ascending AlertServer
            result = String.Compare(AlertServer, signal.AlertServer, StringComparison.OrdinalIgnoreCase); // A -> Z
            return result;
        }
        public string ItemName => $"System.AlertSignal.{(ruleUniqueIdField ?? "(unknown)").Replace('.', '_')}.{(alertServerField ?? "(unknown)").Replace('.', '_')}";

        public NamedValueSet ItemProps
        {
            get
            {
                NamedValueSet result = new NamedValueSet();
                result.Set("RuleName", (ruleUniqueIdField ?? "(unknown)"));
                result.Set("AlertServer", (alertServerField ?? "(unknown)"));
                result.Set("Status", (statusField ?? "Undefined"));
                return result;
            }
        }
    }

    public partial class UserDetail : IIdentity
    {
        public static class Prop
        {
            public const string Disabled = "Disabled";
            public const string UserName = "UserName";
            public const string WindowsDomain = "WindowsDomain";
            public const string WindowsLoginId = "WindowsLoginId";
        }

        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (userNameField == null)
                throw new ArgumentNullException("UserName");
        }
        protected override string OnBuildPrivateKey()
        {
            return $"{typeof(UserDetail).Name}.{userNameField}";
        }
        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(Prop.Disabled, disabledField);
            props.Set(Prop.UserName, userNameField);
            props.Set(Prop.WindowsDomain, windowsDomainField);
            props.Set(Prop.WindowsLoginId, windowsLoginIdField);
        }
        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
        }

        #region IIdentity Members

        public string AuthenticationType => throw new NotImplementedException();

        public bool IsAuthenticated => throw new NotImplementedException();

        public string Name => windowsDomainField + @"\" + windowsLoginIdField;

        #endregion
    }

    public partial class UserGroup
    {
        public static class Prop
        {
            public const string Disabled = "Disabled";
            public const string GroupName = "GroupName";
        }

        protected override void OnValidateKeyProperties()
        {
            base.OnValidateKeyProperties();
            if (groupNameField == null)
                throw new ArgumentNullException("GroupName");
        }
        protected override string OnBuildPrivateKey()
        {
            return $"{typeof(UserGroup).Name}.{groupNameField}";
        }
        protected override void OnSetProperties(NamedValueSet props)
        {
            base.OnSetProperties(props);
            props.Set(Prop.Disabled, disabledField);
            props.Set(Prop.GroupName, groupNameField);
        }
        protected override void OnValidateOtherProperties()
        {
            base.OnValidateOtherProperties();
        }

    }

}
