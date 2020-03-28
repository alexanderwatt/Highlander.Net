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
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Highlander.Codes.V5r3;
using Highlander.Configuration.Data.V5r3;
using Highlander.Constants;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Core.Viewer.V5r3.Properties;
using Highlander.Reporting.Identifiers.V5r3;
using Highlander.Reporting.UI.V5r3;
using Highlander.Reporting.V5r3;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using Highlander.Utilities.Serialisation;
using Highlander.Utilities.Threading;
using Highlander.ValuationEngine.V5r3;
using Highlander.WinTools;
using Highlander.Build;
using Highlander.Metadata.Common;
using AppCfgRuleV2 = Highlander.Core.Common.AppCfgRuleV2;
using Exception = System.Exception;

#endregion

namespace Highlander.Core.Viewer.V5r3
{
    public partial class CoreViewerForm : Form
    {
        private static readonly EnvId BuildEnv = EnvHelper.ParseEnvName(BuildConst.BuildEnv);
        private Reference<ILogger> _loggerRef;
        private ICoreClient _client;
        private readonly SynchronizationContext _syncContext;
        private int _queuedCalls;
        private readonly Guarded<Queue<ICoreItem>> _queuedItems = new Guarded<Queue<ICoreItem>>(new Queue<ICoreItem>());
        private string _navDetailSubject;
        private string _nameSpace = EnvironmentProp.DefaultNameSpace;

        public CoreViewerForm()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;
        }

        private void Form1Load(object sender, EventArgs e)
        {
            _loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
            // init controls
            pnlRuntimeState.BackColor = CoreHelper.CoreStateColor(CoreStateEnum.Initial);
            pnlRuntimeState.Text = Resources.CoreViewerForm_Form1Load__unknown_;
            foreach (EnvId env in Enum.GetValues(typeof(EnvId)))
            {
                if (env != EnvId.Undefined && env <= BuildEnv)
                    cbEnvId.Items.Add(EnvHelper.EnvName(env));
            }
            cbEnvId.SelectedIndex = (int)BuildEnv - 1;
            // - form title
            WinFormHelper.SetAppFormTitle(this, BuildConst.BuildEnv);
            // known data types
            cbDataTypeValues.Items.Clear();
            cbDataTypeValues.Sorted = true;
            cbDataTypeValues.Items.Add(typeof(Instrument).FullName);
            cbDataTypeValues.Items.Add(typeof(Market).FullName);
            cbDataTypeValues.Items.Add(typeof(QuotedAssetSet).FullName);
            cbDataTypeValues.Items.Add(typeof(Trade).FullName);
            cbDataTypeValues.Items.Add(typeof(ValuationReport).FullName);
            cbDataTypeValues.Items.Add(typeof(AppCfgRuleV2).FullName);
            cbDataTypeValues.Items.Add(typeof(ConfigRule).FullName);
            cbDataTypeValues.SelectedIndex = 0;
            // connect
            StartUp();
        }

        private void ClearTree()
        {
            treeNavigation.Nodes.Clear();
            treeNavigation.Nodes.Add("Objects", "Objects");
            treeNavigation.Nodes.Add("Debug", "Debug");
            //treeNavigation.Nodes.Add("Signals", "Signals");
            treeNavigation.Sort();
        }
        private void StartUp()
        {
            _loggerRef.Target.Clear();
            ClearTree();
            try
            {
                var factory = new CoreClientFactory(_loggerRef)
                    .SetEnv(cbEnvId.Text)
                    .SetApplication(Assembly.GetExecutingAssembly())
                    .SetProtocols(WcfConst.AllProtocolsStr);
                if (rbDefaultServers.Checked)
                    _client = factory.Create();
                else if (rbLocalhost.Checked)
                    _client = factory.SetServers("localhost").Create();
                else
                    _client = factory.SetServers(txtSpecificServers.Text).Create();
                _syncContext.Post(OnClientStateChange, new CoreStateChange(CoreStateEnum.Initial, _client.CoreState));
                _client.OnStateChange += _Client_OnStateChange;
            }
            catch (Exception excp)
            {
                _loggerRef.Target.Log(excp);
            }
        }

        void _Client_OnStateChange(CoreStateChange update)
        {
            _syncContext.Post(OnClientStateChange, update);
        }

        private void OnClientStateChange(object state)
        {
            var update = (CoreStateChange)state;
            pnlRuntimeState.Text = update.NewState.ToString();
            pnlRuntimeState.BackColor = CoreHelper.CoreStateColor(update.NewState);
        }

        private void CleanUp()
        {
            _loggerRef.Target.LogInfo("Stopped.");
            txtNavDetail.Clear();
            DisposeHelper.SafeDispose(ref _client);
        }

        public void ProcessItems()
        {
            int count = Interlocked.Decrement(ref _queuedCalls);
            // exit if there are more callbacks following us
            if (count % 10000 == 0)
                _loggerRef.Target.LogDebug("ProcessItems: Queued calls remaining: {0}", count);
            if (count != 0)
                return;
            // start updating the display
            treeNavigation.BeginUpdate();
            ICoreItem item = null;
            _queuedItems.Locked(queue =>
                {
                    if (queue.Count > 0)
                        item = queue.Dequeue();
                });
            while (item != null)
            {
                // add item to navigation tree
                TreeNode[] rootNodes = null;
                switch (item.ItemKind)
                {
                    case ItemKind.Object:
                        rootNodes = treeNavigation.Nodes.Find("Objects", false);
                        break;
                    case ItemKind.Debug:
                        rootNodes = treeNavigation.Nodes.Find("Debug", false);
                        break;
                    //case ItemKind.Signal:
                    //    rootNodes = treeNavigation.Nodes.Find("Signals", false);
                    //    break;
                }
                if (rootNodes != null)
                {
                    TreeNode parentNode = rootNodes[0];
                    TreeNode thisNode = null;
                    string[] itemNameParts = item.Name.Split('.');
                    foreach (string itemNamePart in itemNameParts)
                    {
                        TreeNode[] nodes = parentNode.Nodes.Find(itemNamePart, false);
                        if (nodes.Length == 0)
                        {
                            // not found - add node at this level
                            thisNode = parentNode.Nodes.Insert(FindIndex(parentNode.Nodes, itemNamePart), itemNamePart);
                            thisNode.Name = itemNamePart;
                        }
                        else
                        {
                            // found
                            thisNode = nodes[0];
                        }
                        // next
                        parentNode = thisNode;
                    }
                    if (thisNode != null) thisNode.Tag = item;
                }
                if (item.Name == _navDetailSubject)
                {
                    DisplayItemInNavDetail(item);
                }
                item = null;
                _queuedItems.Locked(queue =>
                {
                    if (queue.Count > 0)
                        item = queue.Dequeue();
                });
            }
            treeNavigation.EndUpdate();
        }

        public void LogNavDetail(string msg)
        {
            txtNavDetail.AppendText(msg + Environment.NewLine);
        }

        private void DisplayItemInNavDetail(ICoreItem item)
        {
            _navDetailSubject = item.Name;
            txtNavDetail.Clear();
            LogNavDetail($"Item        : {item.ItemKind}");
            LogNavDetail($"  Id        : {item.Id}");
            LogNavDetail($"  Type      : {item.DataTypeName}");
            LogNavDetail($"  Name      : {item.Name}");
            LogNavDetail($"  Created   : {item.Created.ToLocalTime()}");
            LogNavDetail($"  Expires   : {item.Expires.ToLocalTime()}");
            //  dump application properties
            LogNavDetail("  AppProps  :");
            item.AppProps.LogValues(text => LogNavDetail("    " + text));
            //  dump system properties
            LogNavDetail(string.Format("  SysProps  :"));
            item.SysProps.LogValues(text => LogNavDetail("    " + text));
            // dump item data (xml text)
            LogNavDetail(item.Text);
//            txtNavDetail.Focus();
            txtNavDetail.SelectionStart = 0;
            txtNavDetail.SelectionLength = 0;
            txtNavDetail.ScrollToCaret();
        }

        private static int FindIndex(TreeNodeCollection nodes, string nodeName)
        {
          int top = 0;
          int bottom = nodes.Count - 1;
          const int sign = 1; // ascending
          int index = -1;
          while (top <= bottom)
          {
            int middle = (top + bottom) >> 1;
            int comp = string.CompareOrdinal(nodes[middle].Name, nodeName) * sign;
            if (0 == comp)
            {
              index = middle;
              break;
            }
            if (comp > 0)
            {
                bottom = middle - 1;
            }
            else
            {
                top = middle + 1;
            }
          }
          return index == -1 ? top : index;
        }

/*
        private void PurgeExpiredItems(TreeNodeCollection nodes)
        {
            PurgeExpiredItems(nodes, 0);
        }
*/

        //private void PurgeExpiredItems(TreeNodeCollection nodes, int level)
        //{
        //    // removes items that have expired
        //    // then removes nodes that have no item or child nodes
        //    // but does not remove root (level=0) nodes
        //    foreach (TreeNode node in nodes)
        //    {
        //        // check children first
        //        PurgeExpiredItems(node.Nodes, level + 1);
        //        // now check this node
        //        if (node.Tag != null)
        //        {
        //            if (node.Tag is ICoreItem)
        //            {
        //                var item = (ICoreItem)node.Tag;
        //                if (!item.IsCurrent())
        //                    node.Tag = null;
        //            }
        //        }
        //        if ((level != 0) && (node.Tag == null) && (node.Nodes.Count == 0))
        //            node.Remove();
        //    }
        //}

        /// <summary>
        /// Shows message box confirming deleting 
        /// and 
        /// deletes selected object in case of positive answer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolStripMenuItem1Click(object sender, EventArgs e)
        {
            if (!(treeNavigation.SelectedNode?.Tag is ICoreItem itemToDelete))
            {
                return;//nothing to delete...
            }
            if (null == _client)
            {
                return;//there's no way we can delete anything without storage object
            }
            var message = "Do you want to delete the selected object?" + Environment.NewLine + Environment.NewLine +
                          "This object may contain important information and/or may have a sentimental value to its creator." +
                          Environment.NewLine + Environment.NewLine +
                          "Unauthorised or inappropriate deletion of the objects is against Group policy and can lead to disciplinary action and or legal actions." +
                          Environment.NewLine + Environment.NewLine +
                          @"The use of the ""Delete object ..."" menu item is monitored and recorded." +
                          Environment.NewLine + Environment.NewLine +
                          "Where considered appropriate, such deletion of events may be reviewed for compliance with law and the Group's policies." +
                          Environment.NewLine + Environment.NewLine +
                          @"Clicking the ""Yes"" but confirm that you accept the conditions detailed in the Information Systems Security Policies and Codes of Conduct that apply to you in your work withing the group." +
                          Environment.NewLine + Environment.NewLine + Environment.NewLine + Environment.NewLine +
                          "Amen." +
                          Environment.NewLine + Environment.NewLine +
                          "*(hold CTRL button to suppress this warning the next time)";
            if (_suppressWarningMessageBox || DialogResult.Yes == MessageBox.Show(message, Resources.CoreViewerForm_ToolStripMenuItem1Click_Important_message, MessageBoxButtons.YesNo, MessageBoxIcon.Stop))
            {
                _suppressWarningMessageBox = ModifierKeys == Keys.Control;
                _client.DeleteItem(itemToDelete);                
                treeNavigation.SelectedNode.Remove();
            }
        }

        private void ContextMenuStrip1Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = !(treeNavigation.SelectedNode?.Tag is ICoreItem);
        }

        private IExpression BuildExpression(out string dataTypeName)
        {
            dataTypeName = null;
            if (chkProp1.Checked)
                dataTypeName = cbDataTypeValues.Text;
            var tempQueryArgs = new List<IExpression>();
            if (chkProp2.Checked)
                tempQueryArgs.Add(Expr.IsEQU(txtProp2Name.Text, txtProp2Value.Text));
            if (chkProp3.Checked)
                tempQueryArgs.Add(Expr.IsEQU(txtProp3Name.Text, txtProp3Value.Text));
            if (chkProp4.Checked)
                tempQueryArgs.Add(Expr.IsEQU(txtProp4Name.Text, txtProp4Value.Text));
            if (chkPropItemName.Checked)
                tempQueryArgs.Add(Expr.StartsWith(Expr.SysPropItemName, txtPropItemNameValue.Text));
            IExpression[] queryArgs = tempQueryArgs.ToArray();
            IExpression query = null;
            if (queryArgs.Length > 0)
                query = Expr.BoolAND(queryArgs);
            if (chkDebugRequests.Checked)
            {
                _loggerRef.Target.LogDebug("DataType: {0}", dataTypeName ?? "(any)");
                _loggerRef.Target.LogDebug("Where   : {0}", (query != null) ? query.DisplayString() : "(all)");
            }
            return query ?? Expr.ALL;
        }

        private void BtnLoadObjectsClick(object sender, EventArgs e)
        {
            IExpression query = BuildExpression(out var dataTypeName);
            _client.DebugRequests = chkDebugRequests.Checked;
            List<ICoreItem> results = _client.LoadUntypedItems(dataTypeName, ItemKind.Object, query, true);
            foreach (ICoreItem item in results)
            {
                // add item to queue
                _queuedItems.Locked(queue => queue.Enqueue(item));
            }
            // simulate posting a callback
            Interlocked.Increment(ref _queuedCalls);
            ProcessItems();
            if (chkDebugRequests.Checked)
            {
                _loggerRef.Target.LogDebug("LOADed {0} objects", results.Count);
            }
            chkDebugRequests.Checked = false;
        }

        private void BtnDeleteObjectsClick1(object sender, EventArgs e)
        {
            IExpression query = BuildExpression(out var dataTypeName);
            treeNavigation.BeginUpdate();
            if (chkProp1.Checked && (chkProp2.Checked || chkProp3.Checked || chkProp4.Checked || chkPropItemName.Checked))
            {
                // at least 1 query clause selected - continue)
                _loggerRef.Target.LogDebug("Deleting all [{0}] objects where: {1}", dataTypeName, query.DisplayString());
                _client.DebugRequests = chkDebugRequests.Checked;
                _client.DeleteUntypedObjects(dataTypeName, query);
                ClearTree();
            }
            else
            {
                _loggerRef.Target.LogDebug("DELETE ignored - DataTypeName and 1 other query clause must be selected");
            }
            treeNavigation.EndUpdate();
            chkDebugRequests.Checked = false;
        }

        private void BtnClearAllClick(object sender, EventArgs e)
        {
            ClearTree();
        }

        private void BtnEventLogClearClick1(object sender, EventArgs e)
        {
            _loggerRef.Target.Clear();
        }

        private void TreeNavigationAfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is ICoreItem tag)
            {
                var item = tag;
                DisplayItemInNavDetail(item);
            }
            else
            {
                txtNavDetail.Text = e.Node.Name + Resources.CoreViewerForm_TreeNavigationAfterSelect_ + e.Node.Text;
            }
        }

        private void TreeNavigationKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (treeNavigation.SelectedNode?.Tag is ICoreItem)
                {
                    ToolStripMenuItem1Click(sender, e);
                }
            }
        }

        private void BtnRestartClick1(object sender, EventArgs e)
        {
            CleanUp();
            StartUp();
        }

        private void BtnSubscribeClick(object sender, EventArgs e)
        {
            IExpression query = BuildExpression(out var dataTypeName);
            _client.DebugRequests = chkDebugRequests.Checked;
            ISubscription newSubscription = _client.CreateUntypedSubscription(null, null);
            newSubscription.DataTypeName = dataTypeName;
            newSubscription.WhereExpr = query;
            newSubscription.UserCallback = delegate(ISubscription subscription, ICoreItem item)
            {
                // note: this is running on a thread pool thread
                // add the item to the queue and post a callback
                _queuedItems.Locked(queue => queue.Enqueue(item));
                int count = Interlocked.Increment(ref _queuedCalls);
                if (count % 10000 == 0)
                    _loggerRef.Target.LogDebug("SubscribeCallback: Queued calls posted: {0}", count);
                _syncContext.Post(ReceiveNewItem, null);
            };
            newSubscription.Start();
            _loggerRef.Target.LogDebug("Subscription started.");
            chkDebugRequests.Checked = false;
        }

        public void ReceiveNewItem(object notUsed)
        {
            // note: this runs on the foreground thread
            ProcessItems();
        }

        private void BtnUnsubscribeClick(object sender, EventArgs e)
        {
            _client.DebugRequests = chkDebugRequests.Checked;
            _client.UnsubscribeAll();
            _loggerRef.Target.LogDebug("Stopped all subscriptions.");
            chkDebugRequests.Checked = false;
        }

        private void CoreViewerFormFormClosing(object sender, FormClosingEventArgs e)
        {
            CleanUp();
        }

        private void SaveToFileToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(treeNavigation.SelectedNode?.Tag is ICoreItem itemToSave))
                return;
            saveFileDialog1.InitialDirectory = Environment.CurrentDirectory;
            saveFileDialog1.FileName = itemToSave.Name + ".xml";
            saveFileDialog1.Filter = Resources.CoreViewerForm_SaveToFileToolStripMenuItemClick_XML_files____xml____xml;
            DialogResult result = saveFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                // save item
                string baseFilename = Path.GetFullPath(saveFileDialog1.FileName);
                string xmlFilename = Path.ChangeExtension(baseFilename, ".xml");
                string nvsFilename = Path.ChangeExtension(xmlFilename, ".nvs");
                try
                {
                    using (var sr = new StreamWriter(xmlFilename))
                    {
                        sr.Write(itemToSave.Text);
                    }
                    using (var sr = new StreamWriter(nvsFilename))
                    {
                        sr.Write(itemToSave.AppProps.Serialise());
                    }
                }
                catch (Exception excp)
                {
                    MessageBox.Show(excp.ToString(), Resources.CoreViewerForm_SaveToFileToolStripMenuItemClick_Save_failed, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ViewPropertiesStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(treeNavigation.SelectedNode?.Tag is ICoreItem itemToValue))
                return;
            //Load the form
            //
            var frm = new PropertiesForm(itemToValue);
            frm.ShowDialog();
        }

        private void ViewXmlStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(treeNavigation.SelectedNode?.Tag is ICoreItem itemToValue))
                return;
            //Load the form
            //
            var xmlTreeDisplay = new XmlTreeDisplay(_client, itemToValue);
            xmlTreeDisplay.ShowDialog();
        }

        private void ViewMarketStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(treeNavigation.SelectedNode?.Tag is ICoreItem itemToValue))
                return;
            //Load the form
            //
            var psv = new PricingStructureVisualizerForm(itemToValue);
            psv.ShowDialog();
        }

        private void ValueStripMenuItemClick(object sender, EventArgs e)
        {
            if (!(treeNavigation.SelectedNode?.Tag is ICoreItem itemToValue))
                return;
            var schema = itemToValue.AppProps.GetValue<string>(TradeProp.Schema, true);
            var user = itemToValue.SysProps.GetValue<string>("UN", true);
            Trade trade;
            if (schema == FpML5R3NameSpaces.ConfirmationSchema)
            {
                var xml = itemToValue.Text; //XmlSerializerHelper.SerializeToString(itemToValue.Data);
                var newXml = xml.Replace("FpML-5/confirmation", "FpML-5/reporting");
                trade = XmlSerializerHelper.DeserializeFromString<Trade>(newXml);
            }
            else
            {
                trade = XmlSerializerHelper.DeserializeFromString<Trade>(itemToValue.Text);
            }
            if (trade == null) return;
            // the item
            var properties = itemToValue.AppProps;
            var party1 = properties.GetValue<string>(TradeProp.Party1, true);
            var baseParty = comboBoxParty.Text == party1 ? "Party1" : "Party2";
            var nameSpace = properties.GetValue<string>(EnvironmentProp.NameSpace, true);               
            var valuationDate = dateTimePickerValuation.Value;
            var market = comboBoxMarket.Items[comboBoxMarket.SelectedIndex].ToString();
            var reportingCurrency = comboBoxCurrency.Items[comboBoxCurrency.SelectedIndex].ToString();
            //Predefined metrics
            var metrics = new List<string> {"NPV", "Delta0", "Delta1", "LocalCurrencyNPV", "NFV"};
            var requestedMetrics = listBoxMetrics.SelectedItems;
            foreach (var metric in requestedMetrics)
            {
                if (!metrics.Contains(metric.ToString()))
                {
                    metrics.Add(metric.ToString());
                }
            }
            var uniqueTradeId = itemToValue.Name;
            var product = trade.Item;
            try
            {
                _loggerRef.Target.LogDebug("Valuing the trade: ." + uniqueTradeId);
                var pricer = new TradePricer(_loggerRef.Target, _client, nameSpace, null, trade, itemToValue.AppProps);
                //Get the market
                var marketEnvironment = CurveEngine.V5r3.CurveEngine.GetMarket(_loggerRef.Target, _client, nameSpace, product, market, reportingCurrency, false);
                var controller = TradePricer.CreateInstrumentModelData(metrics, valuationDate, marketEnvironment, reportingCurrency, baseParty);
                var assetValuationReport = pricer.Price(controller, ValuationReportType.Full);
                _loggerRef.Target.LogDebug("Valued the trade: ." + uniqueTradeId);
                //var id = uniqueTradeId.Split('.')[uniqueTradeId.Split('.').Length - 1];
                //Build the val report properties
                var valProperties = properties.Clone();
                valProperties.Set(ValueProp.PortfolioId, user);
                valProperties.Set(ValueProp.BaseParty, baseParty);
                valProperties.Set(ValueProp.MarketName, market);
                valProperties.Set(ValueProp.CalculationDateTime, valuationDate);
                valProperties.Set(TradeProp.UniqueIdentifier, null);
                //The unique identifier for the valuation report
                var valuationIdentifier = new ValuationReportIdentifier(valProperties);
                _client.SaveObject(assetValuationReport, nameSpace + "." + valuationIdentifier.UniqueIdentifier, valProperties);
                _loggerRef.Target.LogDebug("Valued and saved results for the trade: {0}", uniqueTradeId);
            }
            catch (Exception excp)
            {
                MessageBox.Show(excp.ToString(), Resources.CoreViewerForm_ValueStripMenuItemClick_Value_failed, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLocalTradeData(ILogger logRef, ICoreCache targetClient, string nameSpace)
        {
            //Go to the local directory.
            //Load the FpML trade and create the properties from the trade.
            logRef.LogInfo("Loading trade...");
            var document = XmlSerializerHelper.DeserializeFromFile<Document>(txtBoxTradeDirectory.Text);
            if (document is DataDocument data)
            {
                var tradeData = data;
                var tradeVersion = tradeData.trade[0];
                Party[] party = tradeData.party;
                FpMLTradeLoader.BuildTrade(logRef, targetClient, nameSpace, tradeVersion, party);
                //var properties = MakeTradeProps(trade);
                //var uniqueId = properties.GetValue<string>("UniqueIdentifier");
                //targetClient.SaveObject(trade, uniqueId, properties, TimeSpan.MaxValue);
                logRef.LogInfo("Success");
            }
            else
            {
                logRef.LogInfo("Failed");
            }
        }

        private void BtnLoadTradeClick(object sender, EventArgs e)
        {
            using (Reference<ILogger> logRef = Reference<ILogger>.Create(new ConsoleLogger("LoadLocalTradeData: ")))
            {
                logRef.Target.LogInfo("Running...");
                int exitCode = 0;
                try
                {
                    //using (ICoreClient client = new CoreClientFactory(logRef).SetEnv("DEV").Create())
                    //{
                    //    // delete 'old' configuration data in all environments greater than DEV
                    //    if (client.ClientInfo.ConfigEnv >= EnvId.SIT_SystemTest)
                    //    {
                    //        client.DeleteUntypedObjects(null, Expr.StartsWith(Expr.SysPropItemName, "Highlander.Configuration."));
                    //    }
                        var fileOpen = new OpenFileDialog
                        {
                            InitialDirectory = Environment.CurrentDirectory,
                            Filter = Resources.CoreViewerForm_BtnLoadTradeClick_XML_files____xml____xml_All_files__________,
                            FilterIndex = 1,
                            RestoreDirectory = true
                        };
                        if (fileOpen.ShowDialog() == DialogResult.Cancel)
                        {
                            return;
                        }
                        txtBoxTradeDirectory.Text = fileOpen.FileName;
                        // load configuration data
                        //AppSettingsLoader.Load(logRef.Target, client);
                        LoadLocalTradeData(logRef.Target, _client, _nameSpace);
                        // done
                        logRef.Target.LogInfo("Success");
                    //}
                }
                catch (Exception exception)
                {
                    logRef.Target.Log(exception);
                    logRef.Target.LogInfo("FAILED");
                    exitCode = 2;
                }
                finally
                {
                    Environment.ExitCode = exitCode;
                }
            }
        }

        private void BtnLoadDataClick(object sender, EventArgs e)
        {
            using (Reference<ILogger> logRef = Reference<ILogger>.Create(new ConsoleLogger("LoadReferenceData: ")))
            {
                logRef.Target.LogInfo("Running...");
                int exitCode = 0;
                try
                {
                    // load configuration data
                    //AppSettingsLoader.Load(logRef.Target, _client, EnvironmentProp.DefaultNameSpace);
                    LoadConfigDataHelper.LoadConfigurationData(logRef.Target, _client.Proxy, _nameSpace);
                    // done
                    logRef.Target.LogInfo("Success");
                    //}
                }
                catch (Exception exception)
                {
                    logRef.Target.Log(exception);
                    logRef.Target.LogInfo("FAILED");
                    exitCode = 2;
                }
                finally
                {
                    Environment.ExitCode = exitCode;
                }
            }
        }
    }
}
