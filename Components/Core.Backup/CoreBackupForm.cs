using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Highlander.Core.Backup.Properties;
using Highlander.Core.Common;
using Highlander.Core.V34;
using Highlander.Utilities.Expressions;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Utilities.RefCounting;

namespace Highlander.Core.Backup
{
    public partial class CoreBackupForm : Form
    {
        private Reference<ILogger> _loggerRef;
        private IExpression _filter;
        private string _dataTypeName;
        private long _cancelFlag;

        public CoreBackupForm()
        {
            InitializeComponent();
        }

        private void Form1Load(object sender, EventArgs e)
        {
            _loggerRef = Reference<ILogger>.Create(new TextBoxLogger(txtLog));
            ResetSettings();
        }

        private void ResetSettings()
        {
            rbSrcServer.Checked = false;
            txtSrcServer.Text = "";
            rbSrcFilename.Checked = false;
            txtSrcFilename.Text = "";
            rbTgtServer.Checked = false;
            txtTgtServer.Text = "";
            rbTgtFilename.Checked = false;
            txtTgtFilename.Text = "";
        }

        private void RbPreset1CheckedChanged(object sender, EventArgs e)
        {
            // copy
            ResetSettings();
            // from main server
            rbSrcServer.Checked = true;
            txtSrcServer.Text = Resources.Form1_RbPreset4CheckedChanged_DEV_SYDWADQDS01;
            // to local server
            rbTgtServer.Checked = true;
            txtTgtServer.Text = Resources.Form1_RbPreset3CheckedChanged_DEV_LOCALHOST;
        }

        private void RbPreset2CheckedChanged(object sender, EventArgs e)
        {
            // backup
            ResetSettings();
            // from local server
            rbSrcServer.Checked = true;
            txtSrcServer.Text = Resources.Form1_RbPreset3CheckedChanged_DEV_LOCALHOST;
            // to file
            rbTgtFilename.Checked = true;
            txtTgtFilename.Text = Resources.Form1_RbPreset4CheckedChanged_C___bak_QRStore_bak;
        }

        private void RbPreset3CheckedChanged(object sender, EventArgs e)
        {
            // restore
            ResetSettings();
            // from file
            rbSrcFilename.Checked = true;
            txtSrcFilename.Text = Resources.Form1_RbPreset4CheckedChanged_C___bak_QRStore_bak;
            // to local server
            rbTgtServer.Checked = true;
            txtTgtServer.Text = Resources.Form1_RbPreset3CheckedChanged_DEV_LOCALHOST;
        }

        private void RbPreset4CheckedChanged(object sender, EventArgs e)
        {
            // backup
            ResetSettings();
            // from main server
            rbSrcServer.Checked = true;
            txtSrcServer.Text = Resources.Form1_RbPreset4CheckedChanged_DEV_SYDWADQDS01;
            // to file
            rbTgtFilename.Checked = true;
            txtTgtFilename.Text = Resources.Form1_RbPreset4CheckedChanged_C___bak_QRStore_bak;
        }

        private void BuildFilter()
        {
            var tempFilterArgs = new List<IExpression>();
            _dataTypeName = null;
            if (chkProp1.Checked)
                _dataTypeName = cbDataTypeValues.Text;
            if (chkPropItemName.Checked)
                tempFilterArgs.Add(Expr.StartsWith(Expr.SysPropItemName, txtPropItemNameValue.Text));
            IExpression[] filterArgs = tempFilterArgs.ToArray();
            if (filterArgs.Length > 1)
                _filter = Expr.BoolAND(filterArgs);
            else if (filterArgs.Length == 1)
                _filter = filterArgs[0];
            else
                _filter = Expr.ALL;
            _loggerRef.Target.LogDebug("Filter: {0} and (DataType is {1})", _filter.DisplayString(), _dataTypeName ?? "(any)");
        }

        private void TransferData(bool readOnly)
        {
            Interlocked.Exchange(ref _cancelFlag, 0);
            txtLog.Clear();

            BuildFilter();

            if (rbSrcServer.Checked && rbTgtServer.Checked)
            {
                // server-to-server transfer
                // resolve source address
                string[] sourceAddrParts = txtSrcServer.Text.Split(';');
                if (sourceAddrParts.Length != 2)
                    throw new ApplicationException("Source address not in format 'env;host[:port]'");
                string sourceEnv = sourceAddrParts[0];
                string sourceAddress = sourceAddrParts[1];
                // resolve target address
                string[] targetAddrParts = txtTgtServer.Text.Split(';');
                if (targetAddrParts.Length != 2)
                    throw new ApplicationException("Target address not in format 'env;host[:port]'");
                string targetEnv = targetAddrParts[0];
                string targetAddress = targetAddrParts[1];
                // start background data transfer
                var job = new DataTransferJob
                              {
                                  ReadOnly = readOnly,
                                  Filter = _filter,
                                  DataTypeName = _dataTypeName,
                                  SourceClient = new CoreClientFactory(_loggerRef)
                                      .SetEnv(sourceEnv)
                                      .SetServers(sourceAddress)
                                      .SetApplication(Assembly.GetExecutingAssembly())
                                      .Create()
                              };
                //job.SourceClient.MaxDataRows = Int32.Parse(txtMaxRows.Text);
                if (!readOnly)
                    job.TargetClient = new CoreClientFactory(_loggerRef)
                        .SetEnv(targetEnv)
                        .SetServers(targetAddress)
                        .SetApplication(Assembly.GetExecutingAssembly())
                        .Create();
                ThreadPool.QueueUserWorkItem(BackgroundTransfer, job);
                _loggerRef.Target.LogDebug("Job running in background...");
            }
            else if (rbSrcServer.Checked && rbTgtFilename.Checked)
            {
                // server-to-archive transfer
                // resolve source address
                string[] sourceAddrParts = txtSrcServer.Text.Split(';');
                if (sourceAddrParts.Length != 2)
                    throw new ApplicationException("Source address not in format 'env;host[:port]'");
                string sourceEnv = sourceAddrParts[0];
                string sourceAddress = sourceAddrParts[1];
                // resolve target filename
                // start background data transfer
                var job = new DataTransferJob
                              {
                                  ReadOnly = readOnly,
                                  Filter = _filter,
                                  DataTypeName = _dataTypeName,
                                  SourceClient = new CoreClientFactory(_loggerRef)
                                      .SetEnv(sourceEnv)
                                      .SetServers(sourceAddress)
                                      .SetApplication(Assembly.GetExecutingAssembly())
                                      .Create()
                              };
                //job.SourceClient.MaxDataRows = Int32.Parse(txtMaxRows.Text);
                if (!readOnly)
                    job.TargetStream = new StreamWriter(txtTgtFilename.Text);
                ThreadPool.QueueUserWorkItem(BackgroundTransfer, job);
                _loggerRef.Target.LogDebug("Job running in background...");
            }
            else if (rbSrcFilename.Checked && rbTgtServer.Checked)
            {
                // archive-to-server transfer
                // resolve source filename
                // resolve target address
                string[] targetAddrParts = txtTgtServer.Text.Split(';');
                if (targetAddrParts.Length != 2)
                    throw new ApplicationException("target address not in format 'env;host[:port]'");
                string targetEnv = targetAddrParts[0];
                string targetAddress = targetAddrParts[1];
                // start background data transfer
                var job = new DataTransferJob
                              {
                                  ReadOnly = readOnly,
                                  Filter = _filter,
                                  DataTypeName = _dataTypeName,
                                  SourceStream = new StreamReader(txtSrcFilename.Text)
                              };
                if (!readOnly)
                    job.TargetClient = new CoreClientFactory(_loggerRef)
                        .SetEnv(targetEnv)
                        .SetServers(targetAddress)
                        .SetApplication(Assembly.GetExecutingAssembly())
                        .Create();
                ThreadPool.QueueUserWorkItem(BackgroundTransfer, job);
                _loggerRef.Target.LogDebug("Job running in background...");
            }
            else
            {
                _loggerRef.Target.LogDebug("Not implemented!");
            }
        }

        private void BackgroundTransfer(object state)
        {
            var job = (DataTransferJob)state;
            _loggerRef.Target.LogDebug("Job commencing...");
            try
            {
                BackupTools.TransferObjects(job, ref _cancelFlag);
            }
            finally
            {
                DisposeHelper.SafeDispose(ref job.SourceClient);
                DisposeHelper.SafeDispose(ref job.SourceStream);
                DisposeHelper.SafeDispose(ref job.TargetClient);
                DisposeHelper.SafeDispose(ref job.TargetStream);
                _loggerRef.Target.LogDebug("Job completed.");
            }
        }

        private void BtnReadOnlyClick(object sender, EventArgs e)
        {
            TransferData(true);
        }

        private void ChkProp1CheckedChanged(object sender, EventArgs e)
        {
            BuildFilter();
        }

        private void ChkPropItemNameCheckedChanged(object sender, EventArgs e)
        {
            BuildFilter();
        }

        private void BtnReadWriteClick(object sender, EventArgs e)
        {
            TransferData(false);
        }

        private void BtnCancelClick(object sender, EventArgs e)
        {
            Interlocked.Increment(ref _cancelFlag);
        }
    }
}
