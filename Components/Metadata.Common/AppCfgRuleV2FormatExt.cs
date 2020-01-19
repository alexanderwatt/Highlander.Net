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
using Highlander.Utilities.NamedValues;

namespace Highlander.Metadata.Common
{
    public partial class AppCfgRuleV2 : IComparable<AppCfgRuleV2>
    {
        public int CompareTo(AppCfgRuleV2 rule)
        {
            return Priority - rule.Priority; // lowest to highest
        }
        private string ItemNamePart(string input, string fieldName)
        {
            string result;
            if (input == null)
            {
                result = "(null-" + fieldName + ")";
            }
            else if (input == "*")
            {
                result = "(all-" + fieldName + "s)";
            }
            else
            {
                result = input.Replace('.', '_');
            }
            return result;
        }

        public string ItemName =>
            $"System.AppCfgRuleV2.{ItemNamePart(envField, "Env")}.{ItemNamePart(applNameField, "App")}.{ItemNamePart(userNameField, "User")}.{ItemNamePart(hostNameField, "Host")}";

        public NamedValueSet ItemProps
        {
            get
            {
                var result = new NamedValueSet();
                result.Set(AppPropName.ApplName, applNameField);
                result.Set(AppPropName.HostName, hostNameField);
                result.Set(AppPropName.UserName, userNameField);
                result.Set("Env", envField);
                return result;
            }
        }
    }
}
