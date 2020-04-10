using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Common;
using Orion.Util.Expressions;
using Orion.Util.NamedValues;

namespace Orion.Configuration
{
    // rule interface
    public interface IRuleObject : ICoreObject, IComparable<IRuleObject>
    {
        string EnvName {get;}
        string HostName { get; }
        string Instance { get; }
        string UserName { get; }
        int RulePriority { get; }
        bool RuleDisabled { get; }
    }

    public static class RuleHelper
    {
        // constants
        public const string HostEnvName = "HostEnvName";
        public const string HostComputer = "HostComputer";
        public const string HostUserName = "HostUserName";
        public const string HostInstance = "HostInstance";

        /// <summary>
        /// Makes a rule filter.
        /// </summary>
        /// <param name="applName">The (optional) name of the application processing the rules.</param>
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
                clauses.Add(Expr.BoolOR(Expr.IsNull(RuleHelper.HostEnvName), Expr.IsEQU(RuleHelper.HostEnvName, envAbbr)));
            if (hostName != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(RuleHelper.HostComputer), Expr.IsEQU(RuleHelper.HostComputer, hostName)));
            if (instance != null)
                clauses.Add(Expr.IsEQU(RuleHelper.HostInstance, instance));
            else
                clauses.Add(Expr.IsNull(RuleHelper.HostInstance));
            if (userName != null)
                clauses.Add(Expr.BoolOR(Expr.IsNull(RuleHelper.HostUserName), Expr.IsEQU(RuleHelper.HostUserName, userName)));
            return Expr.BoolAND(clauses.ToArray());
        }

        public static NamedValueSet MakeRuleProps(IRuleObject rule)
        {
            NamedValueSet result = new NamedValueSet();
            result.Set(RuleHelper.HostEnvName, rule.EnvName);
            result.Set(RuleHelper.HostComputer, rule.HostName);
            result.Set(RuleHelper.HostUserName, rule.UserName);
            result.Set(RuleHelper.HostInstance, rule.Instance);
            return result;
        }

        public static string FormatRuleScope(IRuleObject rule)
        {
            return String.Format("{0}.{1}.{2}.{3}",
                rule.EnvName ?? "(all-envs)",
                rule.HostName ?? "(all-host)",
                rule.Instance ?? "(all-inst)",
                rule.UserName ?? "(all-user)");
        }
    }
}
