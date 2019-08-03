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
using System.Threading;

namespace Core.Common
{
    /// <summary>
    /// A public interface for date/time managers. Note: This interface is for test purposes only.
    /// Supports:
    /// - timezones
    /// - time compression/expansion ratio
    /// - clock drift
    /// - system timeouts, sleeps and others
    /// </summary>
    public interface IDateTimeProvider
    {
        TimeZoneInfo TimeZoneInfo { get; }
        string TimeZoneName { get; }
        TimeSpan TimeZoneOffset { get; }
        TimeSpan ClockError { get; set; }
        DateTimeOffset CorrectedUtc { get; }
        DateTimeOffset MachineLocal { get; }
        DateTimeOffset ToMachineLocal(DateTimeOffset correctedUtcTime);
        DateTimeOffset ToCorrectedUtc(DateTimeOffset machineLocalTime);
        void Sleep(int millisecondsTimeout);
        void Sleep(TimeSpan timeout);
    }

    public class StandardDateTimeProvider : IDateTimeProvider
    {
        private readonly TimeZoneInfo _timeZoneInfo;
        public TimeZoneInfo TimeZoneInfo => _timeZoneInfo;

        public string TimeZoneName
        {
            get
            {
                if (_timeZoneInfo.IsDaylightSavingTime(DateTimeOffset.Now))
                    return _timeZoneInfo.DaylightName;
                return _timeZoneInfo.StandardName;
            }
        }

        public TimeSpan TimeZoneOffset => _timeZoneInfo.GetUtcOffset(DateTimeOffset.Now);

        private TimeSpan _clockError = TimeSpan.Zero;
        public TimeSpan ClockError { get => _clockError;
            set => _clockError = value;
        }

        public DateTimeOffset CorrectedUtc => (DateTimeOffset.UtcNow - _clockError);

        public DateTimeOffset MachineLocal => ToMachineLocal(DateTimeOffset.UtcNow - _clockError);

        public DateTimeOffset ToMachineLocal(DateTimeOffset correctedUtcTime)
        {
            TimeSpan offset = _timeZoneInfo.BaseUtcOffset;
            return (new DateTimeOffset(correctedUtcTime.UtcTicks + offset.Ticks, offset) + _clockError);
        }

        public DateTimeOffset ToCorrectedUtc(DateTimeOffset machineLocalTime)
        {
            return new DateTimeOffset(machineLocalTime.UtcDateTime - _clockError, TimeSpan.Zero);
        }

        public void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }

        public void Sleep(TimeSpan timeout)
        {
            Thread.Sleep(timeout);
        }

        // constructor
        public StandardDateTimeProvider(TimeZoneInfo timeZoneInfo)
        {
            _timeZoneInfo = timeZoneInfo;
        }

        public StandardDateTimeProvider(string zoneName, TimeSpan utcOffset, string displayName)
        {
            _timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(zoneName, utcOffset, displayName, zoneName);
        }
    }
    public class UnitTestDateTimeProvider : IDateTimeProvider
    {
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly double _timeFlowRatio;
        private readonly DateTimeOffset _referenceUtc;

        public TimeZoneInfo TimeZoneInfo => _timeZoneInfo;

        public string TimeZoneName
        {
            get
            {
                if (_timeZoneInfo.IsDaylightSavingTime(DateTimeOffset.Now))
                    return _timeZoneInfo.DaylightName;
                return _timeZoneInfo.StandardName;
            }
        }

        public TimeSpan TimeZoneOffset => _timeZoneInfo.GetUtcOffset(DateTimeOffset.Now);
        private TimeSpan _clockError = TimeSpan.Zero;

        public TimeSpan ClockError { get => _clockError;
            set => _clockError = value;
        }

        private DateTimeOffset AdjustedUtcNow()
        {
            return _referenceUtc +
                TimeSpan.FromSeconds((DateTimeOffset.UtcNow - _referenceUtc).TotalSeconds * _timeFlowRatio);
        }

        public DateTimeOffset CorrectedUtc => (AdjustedUtcNow() - _clockError);

        public DateTimeOffset MachineLocal => ToMachineLocal(AdjustedUtcNow() - _clockError);

        public DateTimeOffset ToMachineLocal(DateTimeOffset correctedUtcTime)
        {
            TimeSpan offset = _timeZoneInfo.BaseUtcOffset;
            return (new DateTimeOffset(correctedUtcTime.UtcTicks + offset.Ticks, offset) + _clockError);
        }

        public DateTimeOffset ToCorrectedUtc(DateTimeOffset machineLocalTime)
        {
            return new DateTimeOffset(machineLocalTime.UtcDateTime - _clockError, TimeSpan.Zero);
        }

        public void Sleep(int millisecondsTimeout)
        {
            TimeSpan adjustedTimeout = TimeSpan.FromMilliseconds(millisecondsTimeout / _timeFlowRatio);
            Thread.Sleep(adjustedTimeout);
        }

        public void Sleep(TimeSpan timeout)
        {
            TimeSpan adjustedTimeout = TimeSpan.FromSeconds(timeout.TotalSeconds / _timeFlowRatio);
            Thread.Sleep(adjustedTimeout);
        }

        // constructor
        public UnitTestDateTimeProvider(TimeZoneInfo timeZoneInfo, double timeFlowRatio, DateTimeOffset referenceUtc)
        {
            if ((timeFlowRatio < 0.0001) || (timeFlowRatio > 10000.0))
                throw new ArgumentOutOfRangeException("timeRateRatio");
            _timeZoneInfo = timeZoneInfo;
            _timeFlowRatio = timeFlowRatio;
            _referenceUtc = referenceUtc;
        }

        public UnitTestDateTimeProvider(string zoneName, TimeSpan utcOffset, string displayName, double timeFlowRatio, DateTimeOffset referenceUtc)
        {
            if ((timeFlowRatio < 0.0001) || (timeFlowRatio > 10000.0))
                throw new ArgumentOutOfRangeException("timeRateRatio");
            _timeZoneInfo = TimeZoneInfo.CreateCustomTimeZone(zoneName, utcOffset, displayName, zoneName);
            _timeFlowRatio = timeFlowRatio;
            _referenceUtc = referenceUtc;
        }
    }
}
