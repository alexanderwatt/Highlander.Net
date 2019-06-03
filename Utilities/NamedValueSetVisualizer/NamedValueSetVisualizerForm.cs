#region Using directives

using System;
using System.Data;
using System.Windows.Forms;
using Orion.Util.NamedValues;

#endregion

namespace Orion.NVS.DebuggerVisualizer
{
    public partial class NamedValueSetVisualizerForm : Form
    {
        public NamedValueSetVisualizerForm(NamedValueSet objectToVisualize)
        {
            InitializeComponent();

            PopulateUI(objectToVisualize);
        }

        private void PopulateUI(NamedValueSet objectToVisualize)
        {
            //  populate text box
            //
            textBox1.Text = objectToVisualize.Serialise();

            //  populate datagrid
            //

            //  create datatable from NVS props
            //
            var dataTable = new DataTable();
            dataTable.Columns.Add("NameColumn", typeof(string));
            dataTable.Columns.Add("Value", typeof (object));
            dataTable.Columns.Add("Type", typeof (Type));



            foreach (var nv in objectToVisualize.ToArray())
            {
                
                if (nv.IsArray())
                {
                    int itemNumber = 0;

                    foreach (var itemInArray in (Array)nv.Value)
                    {
                        var dataRow = dataTable.NewRow();

                        dataRow["Value"] = itemInArray;
                        dataRow["NameColumn"] = String.Format("{0}[{1}]", nv.Name, itemNumber) ;
                        dataRow["Type"] = itemInArray.GetType();

                        dataTable.Rows.Add(dataRow);

                        ++itemNumber;
                    }
                }
                else
                {
                    var dataRow = dataTable.NewRow();

                    dataRow["Value"] = nv.Value;
                    dataRow["NameColumn"] = nv.Name;
                    dataRow["Type"] = nv.Value.GetType();

                    dataTable.Rows.Add(dataRow);
                }
                

            }

            dataTable.AcceptChanges();


            dataGridView1.DataSource = dataTable;
            dataGridView1.Columns[0].Name = "Name";
            dataGridView1.Columns[0].Width = 300;
            dataGridView1.Columns[2].Width = 200;
            dataGridView1.Columns[1].Width = dataGridView1.ClientSize.Width - dataGridView1.Columns[0].Width - dataGridView1.Columns[2].Width - 5;


        }

        private void DataGridView1Resize(object sender, EventArgs e)
        {
            //dataGridView1.Columns["NameColumn"].Width = 300;
            dataGridView1.Columns[1].Width = dataGridView1.ClientSize.Width - dataGridView1.Columns[0].Width - dataGridView1.Columns[2].Width - 5;

        }
    }
}