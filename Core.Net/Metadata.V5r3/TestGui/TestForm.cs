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
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Highlander.Confirmation.V5r3;
using Highlander.Metadata.Common;
using Highlander.TestHelpers.V5r3;

namespace Highlander.TestGui.V5r3
{
    public partial class TestForm : Form, ITestLog
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private void BtnSelectSourcePathClick(object sender, EventArgs e)
        {
            ofdSelectSourcePath.ShowDialog();
            //DialogResult dialogResult = openFileDialog1.ShowDialog();
            //if (dialogResult == System.Windows.Forms.DialogResult.OK)
            //{
            //    txtSourcePath.Text = openFileDialog1.FileName;
            //}
        }

        private void BtnSelectWorkPathClick(object sender, EventArgs e)
        {
            var dr = fbdSelectWorkPath.ShowDialog();
            if (dr == DialogResult.OK)
            {
                txtWorkPath.Text = fbdSelectWorkPath.SelectedPath;
            }
        }

        private void OfdSelectSourcePathFileOk(object sender, CancelEventArgs e)
        {
            txtSourcePath.Text = ofdSelectSourcePath.FileName;
        }

        private void BtnPresetInternalTestsClick(object sender, EventArgs e)
        {
        }

        private bool _abort;
        private object _fpMLObject;

        private void Log(string text)
        {
            DateTime dt = DateTime.Now;
            txtLog.AppendText($"{dt:hh:mm},{text}{Environment.NewLine}");
        }

        private void LogInfo(string text) { Log($"Info.: {text}"); }
        public void LogInfo(string text, string testId) { Log($"Info.: [{testId}] {text}"); }

        public void LogError(string text, string testId, bool fatal)
        {
            string severity = fatal ? "Fatal" : "Error";
            Log(testId != null
                    ? $"{severity}: [{testId}] {text}"
                    : $"{severity}: {text}");
            if (fatal)
                _abort = true;
        }
        public void LogException(System.Exception excp, string testId, bool fatal) { LogError(excp.ToString(), testId, fatal); }

        private void BtnSetupTestsClick(object sender, EventArgs e)
        {
            clbTests.Items.Clear();
            if (rbExternal.Checked)
            {
                clbTests.Items.Add("Load/format external source [FMTEXT]", true);
                clbTests.Items.Add("Validate external source [VALEXT]", true);
                clbTests.Items.Add("Import (transform to internal) [IMPORT]", true);
                clbTests.Items.Add("Deserialise imported [DESIMP]", true);
                clbTests.Items.Add("Re-serialise [SERIAL]", true);
                clbTests.Items.Add("Compare serialised with imported [CMPINT]", true);
                clbTests.Items.Add("Export (transform to external) [EXPORT]", true);
                clbTests.Items.Add("Validate exported [VALEXP]", true);
                clbTests.Items.Add("Compare external documents [CMPEXT]", true);
            }
            else
            {
                clbTests.Items.Add("Load/format internal source [FMTINT]", true);
                clbTests.Items.Add("Deserialise internal source [DESIMP]", true);
                clbTests.Items.Add("Serialise [SERIAL]", true);
                clbTests.Items.Add("Compare serialised with internal source [CMPINT]", true);
                clbTests.Items.Add("Export (transform to external) [EXPORT]", true);
                clbTests.Items.Add("Validate exported [VALEXP]", true);
                clbTests.Items.Add("Import (transform to internal) [IMPORT]", true);
                clbTests.Items.Add("Compare imported with internal source [CMPIIS]", true);
            }
        }

        private void BtnRunTestsClick(object sender, EventArgs e)
        {
            txtLog.Clear();
            _abort = false;
            LogInfo("Tests starting...");
            var schemaSet = FpMLViewHelpers.GetSchemaSet();
            // init paths
            string externalPath = txtWorkPath.Text + @"\External.xml";
            string importedPath = txtWorkPath.Text + @"\Imported.xml";
            string internalPath = txtWorkPath.Text + @"\Internal.xml";
            string exportedPath = txtWorkPath.Text + @"\Exported.xml";
            IXmlTransformer importTransformer = new CustomXmlTransformer(FpMLViewHelpers.GetIncomingConversionMap());
            IXmlTransformer exportTransformer = new CustomXmlTransformer(FpMLViewHelpers.GetOutgoingConversionMap());
            // test loop
            foreach (var item in clbTests.CheckedItems)
            {
                if (_abort)
                    break;
                string checkedTest = item.ToString();
                int p1 = checkedTest.IndexOf('[');
                int p2 = checkedTest.IndexOf(']');
                string testId = checkedTest.Substring(p1 + 1, (p2 - p1 - 1));
                string testTitle = checkedTest.Substring(0, p1 - 1);
                LogInfo(testTitle + " ...", testId);
                var errorCounter = new Counter();
                var warningCounter = new Counter();
                try
                {
                    switch (testId)
                    {
                    case "FMTEXT":
                        TestHelper.FormatXml(this, testId, txtSourcePath.Text, externalPath, errorCounter);
                        ShowXml(externalPath, txtExternalXml);
                        break;
                    case "VALEXT":
                        TestHelper.ValidateXml(this, testId, externalPath, schemaSet, errorCounter, warningCounter);
                        break;
                    case "IMPORT":
                        TestHelper.TransformXml(this, testId, importTransformer, externalPath, importedPath, errorCounter);
                        ShowXml(importedPath, txtImportedXml);
                        break;
                    case "DESIMP":
                        _fpMLObject = TestHelper.DeserialiseXml(this, testId, FpMLViewHelpers.AutoDetectType, importedPath, errorCounter);
                        break;
                    case "SERIAL":
                        TestHelper.SerialiseXml(this, testId, _fpMLObject, internalPath, errorCounter);
                        ShowXml(internalPath, txtInternalXml);
                        break;
                    case "CMPINT":
                        TestHelper.CompareXml(this, testId, importedPath, internalPath, errorCounter);
                        break;
                    case "EXPORT":
                        TestHelper.TransformXml(this, testId, exportTransformer, internalPath, exportedPath, errorCounter);
                        ShowXml(exportedPath, txtExportedXml);
                        break;
                    case "VALEXP":
                        TestHelper.ValidateXml(this, testId, exportedPath, schemaSet, errorCounter, warningCounter);
                        break;
                    case "CMPEXT":
                        TestHelper.CompareXml(this, testId, txtSourcePath.Text, exportedPath, errorCounter);
                        break;
                    // internal
                    case "FMTINT":
                        TestHelper.FormatXml(this, testId, txtSourcePath.Text, importedPath, errorCounter);
                        ShowXml(importedPath, txtImportedXml);
                        break;
                    case "CMPIIS":
                        TestHelper.CompareXml(this, testId, txtSourcePath.Text, importedPath, errorCounter);
                        break;

                    default:
                        throw new NotImplementedException("This test has not not implemented!");
                    }
                }
                catch (System.Exception excp)
                {
                    errorCounter.Inc();
                    LogException(excp, testId, true);
                }
                if (errorCounter.Count > 0)
                    LogError($"    Test FAILED with {errorCounter.Count} errors, {warningCounter.Count} warnings.", testId, true);
                else
                    LogInfo($"    Test PASSED with {warningCounter.Count} warnings.", testId);
            }
            Log(_abort ? "Tests aborted!" : "Tests completed.");
        }

        private void ShowXml(string importedPath, TextBox textBox)
        {
            textBox.Clear();
            try
            {
                using (var sr = new StreamReader(importedPath))
                {
                    for (string line = sr.ReadLine(); line != null; line = sr.ReadLine())
                    {
                        textBox.AppendText(line);
                        textBox.AppendText(Environment.NewLine);
                    }
                }
            }
            catch (System.Exception excp)
            {
                textBox.AppendText(Environment.NewLine);
                textBox.AppendText(excp.ToString());
            }
        }
    }
}
