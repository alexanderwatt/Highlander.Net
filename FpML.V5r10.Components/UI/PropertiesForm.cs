using System;
using System.Windows.Forms;
using Core.Common;

namespace Orion.UI
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
                var temp = property.Value as Array;
                if (temp != null)
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
