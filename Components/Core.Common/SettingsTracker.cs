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

namespace Highlander.Core.Common
{
    public class SettingsTracker : IDisposable
    {
        private readonly ICoreClient _client;
        private readonly string _applName;
        private NamedValueSet _currentSettings;
        private NamedValueSet _unsavedSettings;

        public SettingsTracker(ICoreClient client, string applName)
        {
            _client = client;
            _applName = applName;
            // load current settings
            _currentSettings = _client.LoadAppSettings(_applName);
            _unsavedSettings = new NamedValueSet();
        }

        public void Dispose()
        {
            if (_unsavedSettings != null)
            {
                Commit(false);
            }
            _unsavedSettings = null;
            _currentSettings = null;
        }

        public NamedValueSet GetAllValues(bool includeUncommitted)
        {
            var results =  new NamedValueSet(_currentSettings);
            if (includeUncommitted)
                results.Add(_unsavedSettings);
            return results;
        }

        public T GetSetValue<T>(string valueName, T defaultValue)
        {
            NamedValue nv = _currentSettings.Get(valueName, false);
            if ((nv == null) || (nv.ValueType != typeof(T)))
            {
                _currentSettings.Set(valueName, defaultValue);
                _unsavedSettings.Set(valueName, defaultValue);
            }
            return _currentSettings.GetValue(valueName, defaultValue);
        }
        public void SetNewValue<T>(string valueName, T newValue)
        {
            _currentSettings.Set(valueName, newValue);
            _unsavedSettings.Set(valueName, newValue);
        }
        public void Commit(bool reload)
        {
            // save unsaved settings
            if (_unsavedSettings.Count > 0)
                _client.SaveAppSettings(_unsavedSettings, _applName);
            // if requested load current settings 
            // (only needed to resync external changes)
            if (reload)
                _currentSettings = _client.LoadAppSettings(_applName);
            _unsavedSettings = new NamedValueSet();
        }
    }

}
