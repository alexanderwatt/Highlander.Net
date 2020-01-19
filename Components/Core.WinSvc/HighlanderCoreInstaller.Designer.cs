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

namespace Highlander.Core.Service
{
    partial class HighlanderCoreInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.highlanderServiceInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.CoreInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // serviceProcessInstaller1
            // 
            this.highlanderServiceInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.highlanderServiceInstaller.Password = null;
            this.highlanderServiceInstaller.Username = null;
            // 
            // CoreInstaller
            // 
            this.CoreInstaller.Description = "This is the core Highlander service.";
            this.CoreInstaller.ServiceName = "HIGHLANDER_CoreWinSvc";
            this.CoreInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.highlanderServiceInstaller,
            this.CoreInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller highlanderServiceInstaller;
        private System.ServiceProcess.ServiceInstaller CoreInstaller;
    }
}