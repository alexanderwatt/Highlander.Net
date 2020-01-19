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

using System;
using System.Windows.Forms;
using Highlander.Core.Common;

namespace Highlander.Reporting.UI.V5r3
{
    public partial class PropertiesForm : Form
    {
        public PropertiesForm(ICoreItem coreItem)
        {
            InitializeComponent();
            StartUp(coreItem);
        }

        private void StartUp(ICoreItem coreItem)
        {
            //Set the trade identifier
            //
            textBox1.Text = coreItem.Name;
            //Instantiate the datagrid
            //
            dataGridView1.Columns.Add("PropertyName", "Name");
            dataGridView1.Columns.Add("PropertyValue", "Value");
            var properties = coreItem.AppProps.ToDictionary();
            foreach (var property in properties)
            {
                String value = null;
                if (property.Value is Array temp)
                {
                    foreach (var element in temp)
                    {
                        value = element + ";" + value;
                    }
                }
                else
                {
                    value = property.Value.ToString();
                }
                object[] row = { property.Key, value };
                dataGridView1.Rows.Add(row);
            }
        }
    }
}
