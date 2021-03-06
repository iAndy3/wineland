﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BIProject
{
    public partial class periods_sale : Form
    {

        public OleDbConnection conn;
        public OleDbCommand cmd;
        public OleDbDataAdapter adapter;
        public DataTable result;
        public String lastQuery;

        public periods_sale()
        {
            InitializeComponent();
        }

        private void periods_sale_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'winesDatabaseDataSet1.sales' table. You can move, or remove it, as needed.
            this.salesTableAdapter.Fill(this.winesDatabaseDataSet1.sales);
            ConnectToAccess();
        }

        public void ConnectToAccess()
        {
            conn = new OleDbConnection();
            conn.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0; Data source= C:\Users\Andy\Desktop\BI\BIProject\WinesDatabase.accdb";

            try
            {
                conn.Open();
                populateWines();
                populateCounties();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect to data source" + ex);
            }
        }

        private void displayData(String county, String wine)
        {
            lastQuery = $"select * from sales s inner join wines w on s.id_wine=w.id_wine where s.id_county='{county}' and w.wine_name='{wine}'";
            dataGridView1.DataSource = getData(lastQuery);
            drawChart(getData($"SELECT year, sum(quantity)from sales s inner join wines w on s.id_wine = w.id_wine where w.wine_name='{wine}' and id_county = '{county}' group by year"));
        }

        private void drawChart(DataTable res)
        {
            foreach (var series in chart.Series)
            {
                series.Points.Clear();
            }

            string[] N = new string[res.Rows.Count];
            int[] M = new int[res.Rows.Count];
            for (int i = 0; i < res.Rows.Count; i++)
            {
                N[i] = res.Rows[i][0].ToString();
                M[i] = Convert.ToInt32(res.Rows[i][1]);
            }
            chart.Series[0].Points.DataBindXY(N, M);

        }

        private DataTable getData(String query)
        {
            cmd = new OleDbCommand(query, conn);
            adapter = new OleDbDataAdapter(cmd);
            result = new DataTable();

            adapter.Fill(result);
            return result;
        }

        private void populateWines()
        {
            foreach (DataRow row in getData("select * from wines order by wine_name").Rows)
            {
                comboBox1.Items.Add(row["wine_name"]);
            }
        }

        private void populateCounties()
        {
            foreach (DataRow row in getData("select distinct id_county from sales order by id_county").Rows)
            {
                comboBox2.Items.Add(row["id_county"]);
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            handleDropDownChange();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            handleDropDownChange();
        }

        private void handleDropDownChange()
        {
            String selectedCounty = comboBox2.Text;
            String selectedWine = comboBox1.Text;
            if (!string.IsNullOrEmpty(selectedCounty) && !string.IsNullOrEmpty(selectedWine)) { 
                displayData(selectedCounty, selectedWine);
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Visible)
            {
                dataGridView1.Hide();
                generateCsv.Hide();
                chart.Show();
                chartBtn.Text = "Table";
            }
            else
            {
                dataGridView1.Show();
                generateCsv.Show();
                chart.Hide();
                chartBtn.Text = "Chart";
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if(string.IsNullOrEmpty(lastQuery)) {
                return;
            }

            DataTable dt = getData(lastQuery);
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in dt.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText("sale-periods.csv", sb.ToString());

            DialogResult dialogResult = MessageBox.Show("The CSV file has been successfully generated. \nDo you want to open it?", "CSV generated successfuly", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                System.Diagnostics.Process.Start("C:/Users/Andy/Desktop/BI/BIProject/BIProject/bin/Debug/sale-periods.csv");
            }

        }
    }
}
