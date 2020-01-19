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
using System.Linq;
using Highlander.Utilities.Threading;

namespace Highlander.Core.Common
{
    internal class DomainItem
    {
        internal static string CheckName(string name)
        {
            // checks name has allowed chars only
            // allowed chars are: A-Za-z0-9_.
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (name.Length == 0)
                throw new ArgumentException("Cannot be empty", nameof(name));
            for (int i = 0; i < name.Length; i++)
            {
                char ch = name[i];
                if (ch >= 'A' && ch <= 'Z'
                    || ch >= 'a' && ch <= 'z'
                    || ch >= '0' && ch <= '9'
                    || ch == '_' || ch == '.')
                {
                    // allowed char - check 1st and last char
                    // 1st char cannot be '.' or digit
                    if (i == 0 && (ch == '.' || ch >= '0' && ch <= '9'))
                        throw new ArgumentException("ItemName has invalid 1st char: '" + ch + "'");
                    // 1st char cannot be '.'
                    if ((i == (name.Length - 1)) && (ch == '.'))
                        throw new ArgumentException("ItemName has invalid last char: '" + ch + "'");
                }
                else
                {
                    throw new ArgumentException("ItemName contains invalid char: '" + ch + "'");
                }
            }
            return name.Trim().ToUpper();
        }
    }

    /// <summary>
    /// The public interface for appScope managers.
    /// </summary>
    public interface IScopeManager
    {
        void AddScope(string name);
        void AddScopes(string[] names);
        void SetScopes(string[] names);
        bool Exists(string name);
        string[] AllScopes { get; }
        string DefaultAppScope { get; set; }
        bool LegacyDisabled { get; set; }
    }

    public class ScopeManager : IScopeManager
    {
        private readonly Guarded<Dictionary<string, DomainItem>> _dict =
            new Guarded<Dictionary<string, DomainItem>>(new Dictionary<string, DomainItem>());
        private string _defaultDomain;
        private bool _legacyDisabled;

        public ScopeManager()
        {
            string key = DomainItem.CheckName(AppScopeNames.Legacy);
            _dict.Locked(dict => dict.Add(key, new DomainItem()));
            _defaultDomain = AppScopeNames.Legacy;
        }

        public string[] AllScopes
        {
            get
            {
                string[] result = null;
                _dict.Locked(dict => result = dict.Keys.ToArray());
                return result;
            }
        }

        public void AddScope(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            string key = DomainItem.CheckName(name);
            _dict.Locked(dict =>
            {
                DomainItem item;
                if (!dict.TryGetValue(key, out item))
                {
                    dict[key] = new DomainItem();
                }
            });
        }

        public void AddScopes(string[] names)
        {
            if (names == null)
                return;
            _dict.Locked(dict =>
            {
                foreach (string name in names)
                {
                    string key = DomainItem.CheckName(name);
                    DomainItem item;
                    if (!dict.TryGetValue(key, out item))
                    {
                        dict[key] = new DomainItem();
                    }
                }
            });
        }

        public void SetScopes(string[] names)
        {
            if (names == null)
                return;
            _dict.Locked(dict =>
            {
                dict.Clear();
                foreach (string name in names)
                {
                    string key = DomainItem.CheckName(name);
                    dict[key] = new DomainItem();
                }
            });
        }

        public bool Exists(string name)
        {
            string key = DomainItem.CheckName(name);
            bool result = false;
            _dict.Locked(dict =>
            {
                result = dict.ContainsKey(key);
            });
            return result;
        }

        public string DefaultAppScope
        {
            get => _defaultDomain;
            set
            {
                string key = DomainItem.CheckName(value);
                _dict.Locked(dict =>
                {
                    dict[key] = new DomainItem();
                    _defaultDomain = value;
                });
            }
        }

        public bool LegacyDisabled
        {
            get => _legacyDisabled;
            set
            {
                string key = DomainItem.CheckName(AppScopeNames.Legacy);
                _dict.Locked(dict =>
                {
                    if (value)
                        dict.Remove(key);
                    else
                        dict[key] = new DomainItem();
                    _legacyDisabled = value;
                });
            }
        }
    }
}
