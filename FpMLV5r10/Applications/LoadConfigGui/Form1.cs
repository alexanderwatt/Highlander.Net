﻿using System;
using System.Reflection;
using System.Windows.Forms;
using Core.Common;
using Core.V34;
using Orion.Util.Expressions;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.RefCounting;
using Orion.V5r3.Configuration;

namespace LoadConfigGui
{
    public partial class Form1 : Form
    {
        public Form1()
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
                        //Orion.Configuration.MarketDataConfigHelper.LoadProviderRules(_logRef.Target, client);
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
                        //Orion.Configuration.PricingStructureLoader.LoadPricingStructures(_logRef.Target, client);
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
                        //Orion.Configuration.AlertRulesLoader.Load(_logRef.Target, client);
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
                        //Orion.Configuration.FileImportRuleLoader.Load(_logRef.Target, client);
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
                        //Orion.Configuration.TradeImportRuleLoader.Load(_logRef.Target, client);
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
                        //Orion.Configuration.StressDefinitionLoader.LoadStressDefinitions(_logRef.Target, client);
                        //Orion.Configuration.StressDefinitionLoader.LoadScenarioDefinitions(_logRef.Target, client);
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
                        //Orion.Configuration.AppSettingsLoader.Load(_logRef.Target, client);
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
                        //Orion.Configuration.ConfigDataLoader.LoadInstrumentsConfig(_logRef.Target, client);
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
                        //Orion.Configuration.ConfigDataLoader.LoadPricingStructureAlgorithm(_logRef.Target, client);
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
                        //Orion.Configuration.ConfigDataLoader.LoadDateRules(_logRef.Target, client);
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
                        //Orion.Configuration.MarketLoader.LoadFixedIncomeData(_logRef.Target, client);
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
                        //Orion.Configuration.MarketLoader.Load(_logRef.Target, client);
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
                        //Orion.Configuration.ConfigDataLoader.LoadFpml(_logRef.Target, client);
                        //Orion.Configuration.ConfigDataLoader.LoadGwml(_logRef.Target, client);
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
                        //Orion.Configuration.ConfigDataLoader.LoadNewHolidayDates(_logRef.Target, client);
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