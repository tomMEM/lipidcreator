﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LipidCreator
{
    public partial class NewFragment : Form
    {

        DataTable elements;
        MS2Form ms2form;

        public NewFragment(MS2Form ms2form)
        {
            this.ms2form = ms2form;
            elements = new DataTable();
            elements.Clear();
            String count = "Count";
            String shortcut = "Shortcut";
            String element = "Element";
            String monoMass = "mass";

            DataColumn columnCount = elements.Columns.Add(count);
            DataColumn columnShortcut = elements.Columns.Add(shortcut);
            DataColumn columnElement = elements.Columns.Add(element);
            elements.Columns.Add(monoMass);

            columnCount.DataType = System.Type.GetType("System.Int32");
            columnShortcut.ReadOnly = true;
            columnElement.ReadOnly = true;

            DataRow carbon = elements.NewRow();
            carbon[count] = "0";
            carbon[shortcut] = "C";
            carbon[element] = "carbon";
            carbon[monoMass] = 12;
            elements.Rows.Add(carbon);

            DataRow hydrogen = elements.NewRow();
            hydrogen[count] = "0";
            hydrogen[shortcut] = "H";
            hydrogen[element] = "hydrogen";
            hydrogen[monoMass] = 1.007276;
            elements.Rows.Add(hydrogen);

            DataRow oxygen = elements.NewRow();
            oxygen[count] = "0";
            oxygen[shortcut] = "O";
            oxygen[element] = "oxygen";
            oxygen[monoMass] = 15.994915;
            elements.Rows.Add(oxygen);

            DataRow nitrogen = elements.NewRow();
            nitrogen[count] = "0";
            nitrogen[shortcut] = "N";
            nitrogen[element] = "nitrogen";
            nitrogen[monoMass] = 14.003074;
            elements.Rows.Add(nitrogen);

            DataRow phosphor = elements.NewRow();
            phosphor[count] = "0";
            phosphor[shortcut] = "P";
            phosphor[element] = "phosphor";
            phosphor[monoMass] = 30.973763;
            elements.Rows.Add(phosphor);

            DataRow sulfur = elements.NewRow();
            sulfur[count] = "0";
            sulfur[shortcut] = "S";
            sulfur[element] = "sulfur";
            sulfur[monoMass] = 31.972072;
            elements.Rows.Add(sulfur);

            DataRow sodium = elements.NewRow();
            sodium[count] = "0";
            sodium[shortcut] = "Na";
            sodium[element] = "sodium";
            sodium[monoMass] = 22.989770;
            elements.Rows.Add(sodium);


            InitializeComponent();

            dataGridView1.DataSource = elements;
            dataGridView1.Columns[3].Visible = false;
            dataGridView1.Columns[0].Width = 125;
            dataGridView1.Columns[1].Width = 125;
            dataGridView1.Columns[2].Width = 247;
            dataGridView1.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridView1.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void tableView_KeyPress(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            // Check for the flag being set in the KeyDown event.
            if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
            {
                // Stop the character from being entered into the control since it is non-numerical.
                e.Handled = true;
            }
        }

        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        

        private void add_Click(object sender, EventArgs e)
        {
            int elementsSelected = 0;
            foreach (DataRow row in elements.Rows)
            {
                int cnt = Convert.ToInt32(row["Count"]);
                if (cnt < 0)
                {
                    MessageBox.Show("Invalid count for element " + row["Element"]);
                    return;
                }
                if (cnt > 0) elementsSelected += 1;
            }
            if (elementsSelected == 0)
            {
                MessageBox.Show("No element selected");
                return;
            }
            if (textBox1.Text == "")
            {
                MessageBox.Show("No name defined");
                return;
            }
            
            String lipidClass = ((TabPage)ms2form.tabPages[ms2form.tabControl1.SelectedIndex]).Text;
            ((ArrayList)ms2form.currentLipid.MS2Fragments[lipidClass]).Add(new MS2Fragment(textBox1.Text, Convert.ToInt32(numericUpDown1.Value), null, true, elements, "", ""));
            this.Close();
        }
        private void dataGridView1_CellValueChanged(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (string.IsNullOrEmpty(Convert.ToString(elements.Rows[e.RowIndex][0]))) elements.Rows[e.RowIndex][0] = 0;
            updateInfo();
        }

        private void updateInfo()
        {
            double mass = 0;
            String chemForm = "";
            foreach (DataRow row in elements.Rows)
            {
                mass += Convert.ToDouble(row["Count"]) * Convert.ToDouble(row["mass"]);
                if (Convert.ToInt32(row["Count"]) > 0)
                {
                    chemForm += Convert.ToString(row["Shortcut"]) + Convert.ToString(row["Count"]);
                }
            }
            if (chemForm != "" && numericUpDown1.Value > 0)
            {
                chemForm += "+";
            }
            else if (chemForm != "" && numericUpDown1.Value < 0)
            {
                chemForm += "-";
            }
            label1.Text = string.Format("{0:0.0000} Da", mass) + ", " + chemForm;
        }

        private void numericUpDown1_TextChanged(object sender, EventArgs e)
        {
            updateInfo();
        }
    }
}
