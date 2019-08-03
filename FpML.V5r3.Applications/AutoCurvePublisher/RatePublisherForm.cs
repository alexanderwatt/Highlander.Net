/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Core.Common;
using Core.V34;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.CurveEngine.Assets.Helpers;
using Orion.Identifiers;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using Orion.Util.RefCounting;
using Orion.Util.WinTools;
using Exception = System.Exception;

#endregion

namespace FpML.V5r3.AutoRatePublisher
{
    public partial class RatePublisherForm : Form
    {
        private Reference<ILogger> _loggerRef;
        private Reference<ICoreClient> _clientRef;
        private ICoreCache _cache;
        private System.Timers.Timer _timer;
        private Random _rand;

        // CurveDef grid
        private IListViewHelper<RatesObj> _qasDefView;
        private IList<RatesObj> _qasBaseView;
        private IViewHelper _qasDefViewHelper;
        private IDataHelper<RatesObj> _qasDefDataHelper;
        private ISelecter<RatesObj> _qasDefSelecter;
        private IFilterGroup _qasDefFilters;
        private NamedValueSet _curveProperties;

        public RatePublisherForm()
        {
            InitializeComponent();
        }

        private void Form1Load(object sender, EventArgs e)
        {
            // create loggers
            _loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
            StartUp();
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeHelper.SafeDispose(ref _cache);
            DisposeHelper.SafeDispose(ref _clientRef);
        }

        private void ProcessItem(Market market)
        {
            if (market?.Items1?[0] is YieldCurveValuation psv && psv.inputs != null)
            {
                int index = 0;
                foreach (var asset in psv.inputs.instrumentSet.Items)
                {
                    var item = new RatesObj(asset, psv.inputs.assetQuote[index]);
                    _qasBaseView.Add(new RatesObj(item, 0.0m));
                    _qasDefView.UpdateData(new ViewChangeNotification<RatesObj>
                    {
                        OldData = item,
                        NewData = item
                    });
                    index++;
                }
            }
        }

        private void StartUp()
        {
            try
            {
                var factory = new CoreClientFactory(_loggerRef)
                .SetEnv("Dev")
                .SetApplication(Assembly.GetExecutingAssembly())
                .SetProtocols(WcfConst.AllProtocolsStr)
                .SetServers("localhost");
                var client = factory.Create();
                _clientRef = Reference<ICoreClient>.Create(client);
                _cache = _clientRef.Target.CreateCache();
                // init controls
                // - form title
                EnvId env = _clientRef.Target.ClientInfo.ConfigEnv;
                Text += String.Format(" ({0})", EnvHelper.EnvName(env));
                // - server port
                int defaultPort = EnvHelper.SvcPort(env, SvcId.GridSwitch);
                _qasDefViewHelper = new QASDefViewHelper();
                _qasDefDataHelper = new QASDefDataHelper();
                _qasDefFilters = new ComboxBoxFilterGroup(panelQASDef, _qasDefViewHelper, QASDefSelectionChanged);
                _qasDefSelecter = new QASDefSelecter(
                                _qasDefFilters, _qasDefViewHelper, _qasDefDataHelper);
                _qasDefView = new ListViewManager<RatesObj>(
                _loggerRef.Target, lvQASDef, _qasDefViewHelper,
                _qasDefSelecter, _qasDefFilters, new QASDefSorter(), _qasDefDataHelper);
                //Get the starting configuration
                //
                chkChangePort.Text = String.Format("Change server port from default ({0}) to:", defaultPort);
                //Set up the timer and the random number generator.
                //
                _timer = new System.Timers.Timer(10000);
                _rand = new Random();
            }
            catch (Exception excp)
            {
                _loggerRef.Target.Log(excp);
            }
        }

        private void UpdateListView(object curveObject)
        {
            var curveName = curveObject as string;
            lvQASDef.SuspendLayout();
            lvQASDef.Items.Clear();
            var identifier = EnvironmentProp.DefaultNameSpace + ".Configuration.PricingStructures." + curveName;
            var items = _cache.LoadItem<Market>(identifier);
            _qasBaseView = new List<RatesObj>();
            if (items != null)
            {
                _curveProperties = items.AppProps;
                _loggerRef.Target.LogInfo(identifier + " has been retrieved from the cache");
                var ps = items.Data as Market;
                _loggerRef.Target.LogDebug("Processing Items");
                ProcessItem(ps);
                _loggerRef.Target.LogInfo(identifier + " QuotedAssetSet has been retrieved from the cache");
            }
            else
            {
                _loggerRef.Target.LogInfo(identifier + " is not in the cache");
            }
            lvQASDef.ResumeLayout();
        }

        private void BtnSelectCurveClick(object sender, EventArgs e)
        {
            var curveName = lstBoxCurveName.SelectedItem.ToString();
            // run locally on background worker thread
            ThreadPool.QueueUserWorkItem(UpdateListView, curveName);
            //UpdateListView(curveName);
        }

        private void BtnAutoCurvePublisherClick(object sender, EventArgs e)
        {
            //Set the timer defaults for publishing based on the slider control.
            var interval = trackBarFrequency.Value;
            _timer.Interval = (interval + 1)*1000;
            _timer.Enabled = true;
            _timer.Elapsed += OnTimedEvent;
            //_timer.Start();         
        }

        private void OnTimedEvent(object source, EventArgs e)
        {
            var newValue = _rand.Next(-100, 101);
            //1. Update the rates with a random adjustment
            //
            var value = Convert.ToDecimal(newValue)/100000.0m;
            var rateChangeList = new List<RatesObj>();
            foreach (var asset in _qasBaseView)
            {
                var item = new RatesObj(asset, value);
                rateChangeList.Add(item);
                _qasDefView.UpdateData(new ViewChangeNotification<RatesObj>
                {
                    OldData = item,
                    NewData = item
                });
            }
            _loggerRef.Target.LogDebug("Update market data shifted by: {0} %", value * 100.0m);
            //2. Publish the QuotedAssetSet to the cache.
            //
            var qasProperties = _curveProperties.Clone();
            var curveName = qasProperties.GetValue<String>(CurveProp.CurveName);
            var curveType = qasProperties.GetValue<String>(CurveProp.PricingStructureType);
            var market = qasProperties.GetValue<String>(CurveProp.Market);//Use Market for real time.
            var nameSpace = qasProperties.GetValue<String>(EnvironmentProp.NameSpace);
            var itemName = FunctionProp.QuotedAssetSet + "." + market + "." + curveType + "." + curveName;
            qasProperties.Set(EnvironmentProp.DataGroup, nameSpace + "." + FunctionProp.QuotedAssetSet);
            qasProperties.Set(EnvironmentProp.Function, FunctionProp.QuotedAssetSet.ToString());
            qasProperties.Set(CurveProp.BuildDateTime, DateTime.Now);
            qasProperties.Set(CurveProp.UniqueIdentifier, itemName);
            var assetIdentifiers = new List<String>();
            foreach (var asset in rateChangeList)
            {
                assetIdentifiers.AddRange(asset.GetAssetIds());
            }
            var values = new List<Decimal>();
            foreach (var asset in rateChangeList)
            {
                values.AddRange(asset.GetValues());
            }
            var measureTypes = new List<String>();
            foreach (var asset in rateChangeList)
            {
                measureTypes.AddRange(asset.GetMeasureTypes());
            }
            var priceQuoteUnits = new List<String>();
            foreach (var asset in rateChangeList)
            {
                priceQuoteUnits.AddRange(asset.GetPriceQuoteUnits());
            }
            var liveQAS = AssetHelper.Parse(assetIdentifiers.ToArray(), values.ToArray(), measureTypes.ToArray(), priceQuoteUnits.ToArray());
            //TODO Populate the market data set.
            _cache.SaveObject(liveQAS, nameSpace + "." + itemName, qasProperties, false, TimeSpan.FromDays(7));
            _loggerRef.Target.LogDebug("Publish new market data at:{0}", DateTime.Now);
        }

        private void ButtonStopPublishClick(object sender, EventArgs e)
        {
            _clientRef.Target.UnsubscribeAll();
            _timer.Stop();
        }

        // ------------------------------------------------------------------------
        // Rates

        public class RatesObj
        {
            public DateTimeOffset Created { get; set; }
            public string Publisher { get; set; }
            public string InstrumentId { get; set; }
            public string Instrument { get; set; }
            public string Currency { get; set; }
            public string Term { get; set; }
            public decimal Rate1 { get; set; }
            public string Rate1Measure { get; set; }
            public string Rate1Quotes { get; set; }
            public decimal? Rate2 { get; set; }
            public string Rate2Measure { get; set; }
            public string Rate2Quotes { get; set; }

            public RatesObj(RatesObj ratesObj, Decimal newValue)
            {
                Created = DateTime.Now;
                Publisher = ratesObj.Publisher;
                InstrumentId = ratesObj.InstrumentId;
                Instrument = ratesObj.Instrument;
                Currency = ratesObj.Currency;
                Term = ratesObj.Term;
                Rate1 = ratesObj.Rate1 + newValue;
                Rate1Measure = ratesObj.Rate1Measure;
                Rate1Quotes = ratesObj.Rate1Quotes;
                if (ratesObj.Rate2 != null)
                {
                    Rate2 = ratesObj.Rate2;
                    Rate2Measure = ratesObj.Rate2Measure;
                    Rate2Quotes = ratesObj.Rate2Quotes;
                }
            }

            public RatesObj(Asset asset, BasicAssetValuation bav)
            {
                Created = DateTime.Now;
                Publisher = "Alex";
                Instrument = asset.GetType().Name;
                var identifier = new PriceableAssetProperties(asset.id);
                if (asset is Deposit asset1)
                {
                    var deposit = asset1;
                    InstrumentId = deposit.id;
                    Currency = identifier.Currency;
                    Term = deposit.term.ToString();
                }
                else
                {
                    if (asset is SimpleIRSwap irSwap)
                    {
                        var swap = irSwap;
                        InstrumentId = swap.id;
                        Currency = identifier.Currency;
                        Term = swap.term.ToString();
                    }
                    else
                    {
                        if (asset is Future future1)
                        {
                            var future = future1;
                            InstrumentId = future.id;
                            Currency = identifier.Currency;
                            Term = identifier.Term;
                        }
                    }
                }
                if (bav.quote.Length > 0)
                {
                    Rate1 = bav.quote[0].value;
                    if (bav.quote[0].quoteUnits != null)
                    {
                        Rate1Quotes = bav.quote[0].quoteUnits.Value;
                    }
                    if (bav.quote[0].measureType != null)
                    {
                        Rate1Measure = bav.quote[0].measureType.Value;
                    }
                }
                if (bav.quote.Length > 1)
                {
                    Rate2 = bav.quote[1].value;
                    if (bav.quote[1].quoteUnits != null)
                    {
                        Rate2Quotes = bav.quote[1].quoteUnits.Value;
                    }
                    if (bav.quote[1].measureType != null)
                    {
                        Rate2Measure = bav.quote[1].measureType.Value;
                    }
                }
            }

            public List<String> GetAssetIds()
            {
                var ids = new List<string> {InstrumentId};
                if (Rate2 != null)
                {
                    ids.Add(InstrumentId);
                }
                return ids;
            }

            public List<String> GetPriceQuoteUnits()
            {
                var pqu = new List<string> { Rate1Quotes };
                if (Rate2 != null)
                {
                    pqu.Add(Rate2Quotes);
                }
                return pqu;
            }

            public List<String> GetMeasureTypes()
            {
                var measures = new List<string> { Rate1Measure };
                if (Rate2 != null)
                {
                    measures.Add(Rate2Measure);
                }
                return measures;
            }

            public List<Decimal> GetValues()
            {
                var values = new List<decimal> { Rate1 };
                if (Rate2 != null)
                {
                    values.Add((decimal)Rate2);
                }
                return values;
            }
        }

        public enum QASDefColEnum
        {
            Created,
            InstrumentId,
            Instrument,
            Currency,
            Term,
            Rate1,
            Rate1Measure,
            Rate1Quotes,
            Publisher
        }

        internal class QASDefViewHelper : IViewHelper
        {
            public int ColumnCount { get; } = Enum.GetValues(typeof(QASDefColEnum)).Length;

            public string GetColumnTitle(int column)
            {
                return ((QASDefColEnum)column).ToString();
            }

            public bool IsFilterColumn(int column)
            {
                switch ((QASDefColEnum)column)
                {
                    case QASDefColEnum.Created: return true;
                    case QASDefColEnum.InstrumentId: return true;
                    case QASDefColEnum.Instrument: return true;
                    case QASDefColEnum.Currency: return true;
                    case QASDefColEnum.Term: return true;
                    case QASDefColEnum.Rate1: return true;
                    case QASDefColEnum.Rate1Measure: return true;
                    case QASDefColEnum.Rate1Quotes: return true;
                    case QASDefColEnum.Publisher: return true;
                    default:
                        return false;
                }
            }

            public HorizontalAlignment GetColumnAlignment(int column)
            {
                return HorizontalAlignment.Left;
            }
        }

        internal class QASDefDataHelper : IDataHelper<RatesObj>
        {
            public string GetUniqueKey(RatesObj data)
            {
                return data.InstrumentId;
            }

            public string GetDisplayValue(RatesObj data, int column)
            {
                switch ((QASDefColEnum)column)
                {
                    case QASDefColEnum.Created: return data.Created.ToString();
                    case QASDefColEnum.Publisher: return data.Publisher;
                    case QASDefColEnum.InstrumentId: return data.InstrumentId;
                    case QASDefColEnum.Instrument: return data.Instrument;
                    case QASDefColEnum.Currency: return data.Currency;
                    case QASDefColEnum.Term: return data.Term;
                    case QASDefColEnum.Rate1: return data.Rate1.ToString("P");
                    case QASDefColEnum.Rate1Measure: return data.Rate1Measure;
                    case QASDefColEnum.Rate1Quotes: return data.Rate1Quotes;
                    default: return null;
                }
            }
        }

        void QASDefSelectionChanged(object sender, EventArgs e)
        {
            _qasDefView.RebuildView();
        }

        internal class QASDefSorter : IComparer<RatesObj>
        {
            //private static PeriodComparer _periodComparer = new PeriodComparer();

            public int Compare(RatesObj a, RatesObj b)
            {
                // sort order column priority

                // descending create time
                int result = DateTimeOffset.Compare(a.Created, b.Created);
                if (result != 0)
                    return -1 * result;
                return result;
            }
        }

        public class QASDefSelecter : BaseSelecter<RatesObj>
        {
            // this class is currently is a placeholder for future selection rules
            public QASDefSelecter(IFilterGroup filterValues, IViewHelper viewHelper, IDataHelper<RatesObj> dataHelper)
                : base(filterValues, viewHelper, dataHelper) { }
        }
    }
}
