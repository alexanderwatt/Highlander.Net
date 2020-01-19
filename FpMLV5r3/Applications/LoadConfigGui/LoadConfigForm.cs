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
using System.Reflection;
using System.Windows.Forms;
using Highlander.Configuration.Data.V5r3;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;
using AppCfgRuleV2 = Highlander.Core.Common.AppCfgRuleV2;

#endregion

namespace Highlander.Configuration.Loader.V5r3
{
    public partial class LoadConfigForm : Form
    {
        public LoadConfigForm()
        {
            InitializeComponent();
        }

        private Reference<ILogger> _logRef;
        private CoreClientFactory _factory;

        private void Form1Load(object sender, EventArgs e)
        {
            _logRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
            _factory = new CoreClientFactory(_logRef)
                .SetEnv("DEV")
                .SetApplication(Assembly.GetExecutingAssembly())
                .SetProtocols(WcfConst.AllProtocolsStr)
                .SetServers("localhost");
        }

        private void Form1FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposeHelper.SafeDispose(ref _factory);
            DisposeHelper.SafeDispose(ref _logRef);
        }

        private void BtnLoadSelectedClick(object sender, EventArgs e)
        {
            _logRef.Target.Clear();
            _logRef.Target.LogDebug("Loading commenced...");
            var nameSpace = cBoxNameSpaces.Text;
            using(ICoreClient client = _factory.Create())
            {
                if (chkDeleteAllConfig.Checked)
                {
                    chkDeleteAllConfig.Checked = false;
                    client.DeleteTypedObjects(null, Expr.StartsWith(Expr.SysPropItemName, nameSpace + ".Configuration."));
                }
                if (chkDeleteAllStatus.Checked)
                {
                    chkDeleteAllStatus.Checked = false;
                    client.DeleteTypedObjects(null, Expr.StartsWith(Expr.SysPropItemName, nameSpace + ".Status."));
                }
                if (chkDeleteAllAppSettings.Checked)
                {
                    chkDeleteAllAppSettings.Checked = false;
                    client.DeleteObjects<AppCfgRuleV2>(Expr.ALL);
                }
                if (chkMDSProviderMaps.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.MarketDataConfigHelper.LoadProviderRules(_logRef.Target, client);
                    }
                    else
                    {
                        MarketDataConfigHelper.LoadProviderRules(_logRef.Target, client, nameSpace);
                    }                  
                    chkMDSProviderMaps.Checked = false;
                }
                if (chkPricingStructureDefs.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.PricingStructureLoader.LoadPricingStructures(_logRef.Target, client);
                    }
                    else
                    {
                        PricingStructureLoader.LoadPricingStructures(_logRef.Target, client, nameSpace);
                    }
                    chkPricingStructureDefs.Checked = false;
                }
                if (chkAlertMonitorRules.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.AlertRulesLoader.Load(_logRef.Target, client);
                    }
                    else
                    {
                        AlertRulesLoader.Load(_logRef.Target, client, nameSpace);
                    }
                    chkAlertMonitorRules.Checked = false;
                }
                if (chkFileImportRules.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.FileImportRuleLoader.Load(_logRef.Target, client);
                    }
                    else
                    {
                        FileImportRuleLoader.Load(_logRef.Target, client, nameSpace);
                    }
                    chkFileImportRules.Checked = false;
                }
                if (chkTradeImportRules.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.TradeImportRuleLoader.Load(_logRef.Target, client);
                    }
                    else
                    {
                        TradeImportRuleLoader.Load(_logRef.Target, client, nameSpace);
                    }
                    chkTradeImportRules.Checked = false;
                }
                if (chkStressRules.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.StressDefinitionLoader.LoadStressDefinitions(_logRef.Target, client);
                        //Highlander.Configuration.StressDefinitionLoader.LoadScenarioDefinitions(_logRef.Target, client);
                    }
                    else
                    {
                        StressDefinitionLoader.LoadStressDefinitions(_logRef.Target, client, nameSpace);
                        StressDefinitionLoader.LoadScenarioDefinitions(_logRef.Target, client, nameSpace);
                    }
                    chkStressRules.Checked = false;
                }
                if (chkAppSettings.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.AppSettingsLoader.Load(_logRef.Target, client);
                    }
                    else
                    {
                        AppSettingsLoader.Load(_logRef.Target, client, nameSpace);
                    }
                    chkAppSettings.Checked = false;
                }
                if (chkInstrumentsConfig.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.ConfigDataLoader.LoadInstrumentsConfig(_logRef.Target, client);
                    }
                    else
                    {
                        ConfigDataLoader.LoadInstrumentsConfig(_logRef.Target, client, nameSpace);
                    }
                    chkInstrumentsConfig.Checked = false;
                }
                if (chkAlgorithmConfig.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.ConfigDataLoader.LoadPricingStructureAlgorithm(_logRef.Target, client);
                    }
                    else
                    {

                        ConfigDataLoader.LoadPricingStructureAlgorithm(_logRef.Target, client, nameSpace);
                    }
                    chkAlgorithmConfig.Checked = false;
                }
                if (chkDateRules.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.ConfigDataLoader.LoadDateRules(_logRef.Target, client);
                    }
                    else
                    {
                        ConfigDataLoader.LoadDateRules(_logRef.Target, client, nameSpace);
                    }
                    chkDateRules.Checked = false;
                }
                if (bondDataCheckBox.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.MarketLoader.LoadFixedIncomeData(_logRef.Target, client);
                    }
                    else
                    {
                        MarketLoader.LoadFixedIncomeData(_logRef.Target, client, nameSpace);
                    }
                    bondDataCheckBox.Checked = false;
                }
                if (checkBoxMarkets.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.MarketLoader.Load(_logRef.Target, client);
                    }
                    else
                    {
                        MarketLoader.Load(_logRef.Target, client, nameSpace);
                    }
                    checkBoxMarkets.Checked = false;
                }
                if (checkBoxFpML.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.ConfigDataLoader.LoadFpml(_logRef.Target, client);
                        //Highlander.Configuration.ConfigDataLoader.LoadGwml(_logRef.Target, client);
                    }
                    else
                    {
                        ConfigDataLoader.LoadFpMLCodes(_logRef.Target, client, nameSpace);
                    }
                    checkBoxFpML.Checked = false;
                }
                if (checkBoxHolidayDates.Checked)
                {
                    if (nameSpace == "FpML.V4r7")
                    {
                        //Highlander.Configuration.ConfigDataLoader.LoadNewHolidayDates(_logRef.Target, client);
                    }
                    else
                    {
                        ConfigDataLoader.LoadNewHolidayDates(_logRef.Target, client, nameSpace);
                    }
                    checkBoxHolidayDates.Checked = false;
                }
            }
            _logRef.Target.LogDebug("Load completed.");
        }

        private void BtnSelectAllClick(object sender, EventArgs e)
        {
            chkAlertMonitorRules.Checked = true;
            chkAlgorithmConfig.Checked = true;
            chkAppSettings.Checked = true;
            chkFileImportRules.Checked = true;
            chkInstrumentsConfig.Checked = true;
            chkMDSProviderMaps.Checked = true;
            chkPricingStructureDefs.Checked = true;
            chkStressRules.Checked = true;
            chkTradeImportRules.Checked = true;
            chkDateRules.Checked = true;
            bondDataCheckBox.Checked = true;
            checkBoxMarkets.Checked = true;
            checkBoxFpML.Checked = true;
            checkBoxHolidayDates.Checked = true;
        }

        private void BtnUnselectAllClick(object sender, EventArgs e)
        {
            chkAlertMonitorRules.Checked = false;
            chkAlgorithmConfig.Checked = false;
            chkAppSettings.Checked = false;
            chkFileImportRules.Checked = false;
            chkInstrumentsConfig.Checked = false;
            chkMDSProviderMaps.Checked = false;
            chkPricingStructureDefs.Checked = false;
            chkStressRules.Checked = false;
            chkTradeImportRules.Checked = false;
            chkDateRules.Checked = false;
            bondDataCheckBox.Checked = false;
            checkBoxMarkets.Checked = false;
            checkBoxFpML.Checked = false;
            checkBoxHolidayDates.Checked = false;
        }

        private void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ChkDeleteAllConfigCheckedChanged(object sender, EventArgs e)
        {

        }

        private void CheckBoxMarketsCheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
