/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Orion.Util.Logging;
using Orion.Util.Threading;

#endregion

namespace Core.Server
{
    /// <summary>
    /// The server interface definition
    /// </summary>
    public interface IServerPart : IDisposable
    {
        void Attach(string name, IServerPart part);
        void Detach(string name);
        void Start();
        void Stop();
    }

    /// <summary>
    /// Ther server part class
    /// </summary>
    public class ServerPart : IServerPart
    {
        protected readonly string PartName;
        protected readonly ILogger Logger;

        /// <summary>
        /// The server part constructor
        /// </summary>
        /// <param name="partName"></param>
        /// <param name="logger"></param>
        public ServerPart(string partName, ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            PartName = partName ?? throw new ArgumentNullException(nameof(partName));
            Logger = new FilterLogger(logger, PartName + ": ");
        }

        private readonly Guarded<Dictionary<string, IServerPart>> _attachedParts =
            new Guarded<Dictionary<string, IServerPart>>(new Dictionary<string, IServerPart>());
        protected virtual void OnAttached(string name, IServerPart part) { }

        /// <summary>
        /// The attach method.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="part"></param>
        public void Attach(string name, IServerPart part)
        {
            if (part != null)
            {
                _attachedParts.Locked((attachedParts) => attachedParts[name] = part );
                //Logger.LogDebug("ServerPart '{0}' attached.", name);
                OnAttached(name, part);
            }
            //else
            //    Logger.LogDebug("ServerPart '{0}' not attached.", name);
        }

        protected virtual void OnDetached(string name) { }

        /// <summary>
        /// The detach method
        /// </summary>
        /// <param name="name"></param>
        public void Detach(string name)
        {
            bool existed = false;
            _attachedParts.Locked((attachedParts) => existed = attachedParts.Remove(name));
            if (existed)
                OnDetached(name);
        }

        // stats reporting
        private Timer _statsReportTimerDeltas;
        private Timer _statsReportTimerTotals;
        protected readonly StatsCounterSet StatsCountersDelta = new StatsCounterSet();
        protected readonly StatsCounterSet StatsCountersTotal = new StatsCounterSet();
        protected readonly DateTimeOffset StatsCountingBegin = DateTimeOffset.Now;
        // default deltas report
        protected virtual void OnStatsTimeoutReportDeltas()
        {
            List<StatsCounter> deltaCounters = StatsCountersDelta.GetCounters(true);
            if (deltaCounters.Count > 0)
            {
                deltaCounters.Sort();
                StatsCountersTotal.AddToMultiple(deltaCounters);
                // build report and log it
                StringBuilder report = new StringBuilder();
                report.AppendLine("--------- deltas ----------------------");
                foreach (StatsCounter counter in deltaCounters)
                    report.AppendLine($"{counter.Name} ({counter.Count})");
                report.Append(String.Format("---------------------------------------"));
                Logger.LogDebug(report.ToString());
            }
        }
        // default totals report
        protected virtual void OnStatsTimeoutReportTotals()
        {
            List<StatsCounter> totalCounters = StatsCountersTotal.GetCounters(false);
            totalCounters.Sort();
            // build report and log it
            StringBuilder report = new StringBuilder();
            report.AppendLine("--------- totals ----------------------");
            report.AppendLine($"Up time: {DateTimeOffset.Now - StatsCountingBegin}");
            foreach (StatsCounter counter in totalCounters)
                report.AppendLine($"{counter.Name} ({counter.Count})");
            report.Append("---------------------------------------");
            Logger.LogDebug(report.ToString());
        }

        private void ReportTimeoutDeltas(object notUsed) { OnStatsTimeoutReportDeltas(); }
        private void ReportTimeoutTotals(object notUsed) { OnStatsTimeoutReportTotals(); }

        protected virtual void OnStart() { }

        /// <summary>
        /// THe start method.
        /// </summary>
        public void Start()
        {
            OnStart();
            _statsReportTimerDeltas = new Timer(ReportTimeoutDeltas, null, ServerCfg.StatsReportIntervalDeltas, ServerCfg.StatsReportIntervalDeltas);
            _statsReportTimerTotals = new Timer(ReportTimeoutTotals, null, ServerCfg.StatsReportIntervalTotals, ServerCfg.StatsReportIntervalTotals);
        }

        protected virtual void OnStop() { }

        /// <summary>
        /// The stop method
        /// </summary>
        public void Stop()
        {
            OnStop();
        }

        protected virtual void Dispose(bool disposing)
        {
            // no managed or unmanaged resources to clean up
        }

        /// <summary>
        /// The dispose method
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            //GC.SuppressFinalize(this);
        }
    }
}
