﻿/*
MIT License

Copyright (c) 2017 Dominik Kopczynski   -   dominik.kopczynski {at} isas.de
                   Bing Peng   -   bing.peng {at} isas.de

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LipidCreator
{
    public partial class AddHeavyPrecursor : Form
    {
        public CreatorGUI creatorGUI;
        ArrayList buildingBlockDataTables;
        public bool updating;
        public Dictionary<string, object[]> currentDict = null;
        public bool editing;
        public bool inGridSet = false;
        public bool userDefined = true;
        
        
        public AddHeavyPrecursor(CreatorGUI creatorGUI, LipidCategory category)
        {
            this.creatorGUI = creatorGUI;
            buildingBlockDataTables = new ArrayList();
            editing = false;
            
        
            InitializeComponent();
            
            updating = true;
            dataGridView1.ColumnCount = 3;
            dataGridView1.Columns[0].Name = "Element";
            dataGridView1.Columns[0].DefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.Columns[1].Name = "Count";
            dataGridView1.Columns[1].DefaultCellStyle.BackColor = Color.LightGray;
            dataGridView1.Columns[2].Name = "Isotope count";
            DataGridViewComboBoxColumn combo1 = new DataGridViewComboBoxColumn();
            dataGridView1.Columns.Add(combo1);
            dataGridView1.Columns[0].Width = (dataGridView1.Width - 2) / 4;
            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].Width = (dataGridView1.Width - 2) / 4;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.Columns[2].Width = (dataGridView1.Width - 2) / 4;
            dataGridView1.Columns[3].Width = (dataGridView1.Width - 2) / 4;
            dataGridView1.AllowUserToAddRows = false;
            combo1.Name = "Isotope type";
            
            updating = false;
            
            foreach (string lipidClass in creatorGUI.lipidCreator.categoryToClass[(int)category])
            {
                if (!creatorGUI.lipidCreator.headgroups[lipidClass].heavyLabeled) comboBox1.Items.Add(lipidClass);
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
            
        }
        
        
        
        public void fillGridContent()
        {
            updating = true;
            for (int k = 0; k < MS2Fragment.HEAVY_DERIVATIVE.Count; ++k) dataGridView1.Rows.Add(new object[] {"-", 0, 0, new DataGridViewComboBoxCell()});
            foreach (KeyValuePair<int, ArrayList> row in MS2Fragment.HEAVY_DERIVATIVE)
            {
                int l = MS2Fragment.MONOISOTOPE_POSITIONS[row.Key];
                dataGridView1.Rows[l].Cells[0].Value = MS2Fragment.ELEMENT_SHORTCUTS[row.Key];
                dataGridView1.Rows[l].Cells[1].Value = 0;
                dataGridView1.Rows[l].Cells[2].Value = 0;
                
                DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)dataGridView1.Rows[l].Cells[3];
                int j = 0;
                foreach (int element in row.Value)
                {
                    if (j++ == 0) cell.Value = MS2Fragment.HEAVY_SHORTCUTS[element];
                    cell.Items.Add(MS2Fragment.HEAVY_SHORTCUTS[element]);
                }
            }
            updating = false;
        }
        
        
        
        public void changeDataGridContent(Dictionary<string, object[]> data)
        {
            updating = true;
            foreach (KeyValuePair<string, object[]> row in data)
            {
                int l = MS2Fragment.MONOISOTOPE_POSITIONS[(int)MS2Fragment.ELEMENT_POSITIONS[row.Key]];
                
                dataGridView1.Rows[l].Cells[1].Value = row.Value[0];
                dataGridView1.Rows[l].Cells[2].Value = row.Value[1];
                dataGridView1.Rows[l].Cells[3].Value = row.Value[2];
            }
            currentDict = data;
            updating = false;
        }
        
        
        
        public Dictionary<string, object[]> createGridData(Dictionary<int, int> input)
        {
            Dictionary<string, object[]> data = new Dictionary<string, object[]>();
            
            foreach (KeyValuePair<int, int> row in input)
            {
                if (MS2Fragment.MONOISOTOPE_POSITIONS.ContainsKey(row.Key))
                {
                    // check for heavy isotopes
                    int heavyElementIndex = MS2Fragment.HEAVY_DERIVATIVE[row.Key].Count - 1;
                    int heavyElementCount = 0;
                    string heavyShortcut = "";
                    for (; heavyElementIndex >= 0; --heavyElementIndex)
                    {
                        heavyElementCount = input[(int)MS2Fragment.HEAVY_DERIVATIVE[row.Key][heavyElementIndex]];
                        heavyShortcut = MS2Fragment.HEAVY_SHORTCUTS[(int)MS2Fragment.HEAVY_DERIVATIVE[(int)row.Key][heavyElementIndex]];
                        if (input[(int)MS2Fragment.HEAVY_DERIVATIVE[row.Key][heavyElementIndex]] > 0)
                        {
                            break;
                        }
                    }
            
                    data.Add(MS2Fragment.ELEMENT_SHORTCUTS[row.Key], new object[]{row.Value, heavyElementCount, heavyShortcut});
                }
            }
            return data;
        }
        
        
        public Dictionary<int, int> createElementData(Dictionary<string, object[]> input)
        {
            Dictionary<int, int> elements = MS2Fragment.createEmptyElementDict();
            foreach (KeyValuePair<string, object[]> row in input)
            {
                int elementIndex = MS2Fragment.ELEMENT_POSITIONS[row.Key];
                int heavyIndex = MS2Fragment.HEAVY_POSITIONS[(string)row.Value[2]];
                
                elements[elementIndex] = (int)row.Value[0];
                elements[heavyIndex] = (int)row.Value[1];
                
            }
            return elements;
        }
        
        
        public void clearData()
        {
            currentDict = null;
            buildingBlockDataTables.Clear();
            dataGridView1.Rows.Clear();
        
        }
        
        
        public void updateAvailableIsotopes()
        {
            clearData();
            string headgroup = (string)comboBox1.Items[comboBox1.SelectedIndex];
            Precursor precursor = creatorGUI.lipidCreator.headgroups[headgroup];
            comboBox3.Items.Clear();
            
            foreach(Precursor heavyPrecursor in precursor.heavyLabeledPrecursors)
            {
                comboBox3.Items.Add(heavyPrecursor.name.Split(new Char[]{'/'})[1]);
            }
            if (comboBox3.Items.Count > 0)
            {
                comboBox3.SelectedIndex = 0;
                button3.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
                button4.Enabled = false;
            }
            
        }
        
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            setGrid();
        }
        
        

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (editing) updateAvailableIsotopes();
            else setGrid();
        }
        
        
        public void setGrid()
        {
            // base types:
            // 0 -> fixed, FA1, FA2, FA3, FA4, FA1 + FA2, FA1 + FA3, FA1 + FA4, FA2 + FA3, FA2 + FA4, FA3 + FA4, FA1 + FA2 + FA3, FA1 + FA2 + FA4, FA1 + FA3 + FA4, PRE
            // 1 -> fixed, FA1, FA2, FA3, FA1 + FA2, FA1 + FA3, FA2 + FA3, PRE
            // 2 -> fixed, FA1, FA2, PRE
            // 3 -> fixed, FA, PRE
            // 4 -> fixed, LCB, FA, HG, LCB + FA, LCB + HG, FA + HG, PRE
            // 5 -> fixed, LCB, HG, PRE
            // 6 -> fixed, FA1, FA2, HG, FA1 + FA2, FA1 + HG, FA2 + HG, PRE
            // 7 -> fixed, FA1, HG, PRE
            // 8 -> fixed, PRE
            clearData();
            comboBox2.Items.Clear();
            inGridSet = true;
            
            if (!editing)
            {
                fillGridContent();
                comboBox2.Items.Add("Head group");
                string headgroup = (string)comboBox1.Items[comboBox1.SelectedIndex];
                Precursor precursor = creatorGUI.lipidCreator.headgroups[headgroup];
                buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict(precursor.elements)));
                        
                switch(precursor.buildingBlockType)
                {
                    case 0:
                        comboBox2.Items.Add("Fatty acid 1");
                        comboBox2.Items.Add("Fatty acid 2");
                        comboBox2.Items.Add("Fatty acid 3");
                        comboBox2.Items.Add("Fatty acid 4");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        break;
                        
                    case 1:
                        comboBox2.Items.Add("Fatty acid 1");
                        comboBox2.Items.Add("Fatty acid 2");
                        comboBox2.Items.Add("Fatty acid 3");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        break;
                        
                    case 2:
                    case 6:
                        comboBox2.Items.Add("Fatty acid 1");
                        comboBox2.Items.Add("Fatty acid 2");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        break;
                        
                    case 3:
                    case 7:
                        comboBox2.Items.Add("Fatty acid");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        break;
                        
                    case 4:
                        comboBox2.Items.Add("Long chain base");
                        comboBox2.Items.Add("Fatty acid");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        break;
                        
                    case 5:
                        comboBox2.Items.Add("Long chain base");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createEmptyElementDict()));
                        break;
                        
                    case 8:
                        break;
                        
                    default:
                        break;
                }
                if (comboBox2.Items.Count > 0)
                {
                    comboBox2.SelectedIndex = 0;
                }
            }
            else if(comboBox3.SelectedIndex >= 0)
            {
                fillGridContent();
                string headgroup = (string)comboBox1.Items[comboBox1.SelectedIndex] + "/" + (string)comboBox3.Items[comboBox3.SelectedIndex];
                Precursor precursor = creatorGUI.lipidCreator.headgroups[headgroup];
                userDefined = precursor.userDefined;
                comboBox2.Items.Add("Head group");
                buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict(precursor.elements)));
                switch(precursor.buildingBlockType)
                {
                    case 0:
                        comboBox2.Items.Add("Fatty acid 1");
                        comboBox2.Items.Add("Fatty acid 2");
                        comboBox2.Items.Add("Fatty acid 3");
                        comboBox2.Items.Add("Fatty acid 4");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[0])));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[1])));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[2])));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[3])));
                        break;
                        
                    case 1:
                        comboBox2.Items.Add("Fatty acid 1");
                        comboBox2.Items.Add("Fatty acid 2");
                        comboBox2.Items.Add("Fatty acid 3");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[0])));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[1])));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[2])));
                        break;
                        
                    case 2:
                    case 6:
                        comboBox2.Items.Add("Fatty acid 1");
                        comboBox2.Items.Add("Fatty acid 2");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[0])));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[1])));
                        break;
                        
                    case 3:
                    case 7:
                        comboBox2.Items.Add("Fatty acid");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[0])));
                        break;
                        
                    case 4:
                        comboBox2.Items.Add("Long chain base");
                        comboBox2.Items.Add("Fatty acid");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[0])));
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[1])));
                        break;
                        
                    case 5:
                        comboBox2.Items.Add("Long chain base");
                        buildingBlockDataTables.Add(createGridData(MS2Fragment.createFilledElementDict((Dictionary<int, int>)precursor.userDefinedFattyAcids[0])));
                        break;
                        
                    case 8:
                        break;
                        
                    default:
                        break;
                }
                if (comboBox2.Items.Count > 0)
                {
                    comboBox2.SelectedIndex = 0;
                    changeDataGridContent((Dictionary<string, object[]>)buildingBlockDataTables[0]);
                }
            }
            if (userDefined)
            {
                dataGridView1.Columns[2].DefaultCellStyle.BackColor = Color.Empty;
                dataGridView1.Columns[2].ReadOnly = false;
                dataGridView1.Columns[3].DefaultCellStyle.BackColor = Color.Empty;
                dataGridView1.Columns[3].ReadOnly = false;
                button3.Enabled = true;
                button4.Enabled = true;
            }
            else
            {
                dataGridView1.Columns[2].DefaultCellStyle.BackColor = Color.LightGray;
                dataGridView1.Columns[2].ReadOnly = true;
                dataGridView1.Columns[3].DefaultCellStyle.BackColor = Color.LightGray;
                dataGridView1.Columns[3].ReadOnly = true;
                button3.Enabled = false;
                button4.Enabled = false;
                
            }
            dataGridView1.Update();
            dataGridView1.Refresh();
            inGridSet = false;
        }
        

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox2.Items.Count > 0)
            {
                changeDataGridContent((Dictionary<string, object[]>)buildingBlockDataTables[comboBox2.SelectedIndex]);
            }
        }
        
        private void radioButtons_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                this.textBox1.Visible = true;
                this.textBox1.Text = "";
                this.comboBox3.Visible = false;
                this.button2.Visible = true;
                this.button3.Visible = false;
                this.button4.Visible = false;
                editing = false;
                userDefined = true;
                setGrid();
            }
            else
            {
                this.textBox1.Visible = false;
                this.comboBox3.Visible = true;
                this.button2.Visible = false;
                this.button3.Visible = true;
                this.button4.Visible = true;
                editing = true;
                updateAvailableIsotopes();
            }
        }
        
        
        private void dataGridView1CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (currentDict == null) return;
            string key = (string)dataGridView1.Rows[e.RowIndex].Cells[0].Value;
            currentDict[key][e.ColumnIndex - 1] = dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            
            if(updating) return;
            
            if (e.ColumnIndex != 2) return;
            updating = true;
            if (comboBox2.SelectedIndex == 0)
            {
                string headgroup = (string)comboBox1.Items[comboBox1.SelectedIndex];
                Precursor precursor = creatorGUI.lipidCreator.headgroups[headgroup];
                
                int orig = precursor.elements[MS2Fragment.LIGHT_ORIGIN[MS2Fragment.HEAVY_POSITIONS[(string)dataGridView1.Rows[e.RowIndex].Cells[3].Value]]];
                int n = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[2].Value);
                if (n < 0) n = 0;
                if (n > orig) n = orig;
                dataGridView1.Rows[e.RowIndex].Cells[2].Value = n;
                dataGridView1.Rows[e.RowIndex].Cells[1].Value = orig - n;
            }
            else
            {
                int n = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[2].Value);
                if (n < 0) n = 0;
                dataGridView1.Rows[e.RowIndex].Cells[2].Value = n;
                dataGridView1.Rows[e.RowIndex].Cells[1].Value = -n;
            }
            updating = false;
        }
        

        
        // closing the window
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        
        
        // editing isotope
        private void button3_Click(object sender, EventArgs e)
        {
            int numHeavyElements = 0;
            
            ArrayList tmp = new ArrayList();
            foreach (Dictionary<string, object[]> bbdt in buildingBlockDataTables)
            {
                Dictionary<int, int> elements = createElementData(bbdt);
                foreach(KeyValuePair<int, int> row in elements) if (MS2Fragment.HEAVY_SHORTCUTS.ContainsKey(row.Key) && row.Value > 0) numHeavyElements += row.Value;
                tmp.Add(elements);
                
            }
            
            if (numHeavyElements == 0)
                {
                    MessageBox.Show("No building block contains a heavy isotope!", "Not editable");
                }
                else
                {
                    string heavyHeadgroup = (string)comboBox1.Items[comboBox1.SelectedIndex] + "/" + (string)comboBox3.Items[comboBox3.SelectedIndex];
                    Precursor heavyPrecursor = creatorGUI.lipidCreator.headgroups[heavyHeadgroup];
                    heavyPrecursor.elements = (Dictionary<int, int>)tmp[0];
                    heavyPrecursor.userDefinedFattyAcids.Clear();
                    for (int i = 1; i < tmp.Count; ++i)
                    {
                        heavyPrecursor.userDefinedFattyAcids.Add(tmp[i]);
                    }
                    MessageBox.Show ("Changes have been stored.", "Editing complete");
            }
        }
        
        
        
        // deleting isotope
        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult mbr = MessageBox.Show ("Are you sure to delete this heavy isotope?", "Deleting isotope", MessageBoxButtons.YesNo);
            
            if (mbr == DialogResult.Yes) {
                string headgroup = (string)comboBox1.Items[comboBox1.SelectedIndex];
                string heavyHeadgroup = (string)comboBox1.Items[comboBox1.SelectedIndex] + "/" + (string)comboBox3.Items[comboBox3.SelectedIndex];
                Console.WriteLine(headgroup + " " + heavyHeadgroup);
                Precursor heavyPrecursor = creatorGUI.lipidCreator.headgroups[heavyHeadgroup];
                creatorGUI.lipidCreator.categoryToClass[(int)heavyPrecursor.category].Remove(heavyHeadgroup);
                creatorGUI.lipidCreator.allFragments.Remove(heavyHeadgroup);
                creatorGUI.lipidCreator.headgroups.Remove(heavyHeadgroup);
                
                
                Precursor precursor = creatorGUI.lipidCreator.headgroups[headgroup];
                for (int i = 0; i < precursor.heavyLabeledPrecursors.Count; ++i)
                {
                    if (((Precursor)precursor.heavyLabeledPrecursors[i]).name.Equals(heavyHeadgroup))
                    {
                        precursor.heavyLabeledPrecursors.RemoveAt(i);
                        break;
                    }
                }                
                updateAvailableIsotopes();
            }
        }

        
        
        // adding isotope
        private void button2_Click(object sender, EventArgs e)
        {
            string headgroup = (string)comboBox1.Items[comboBox1.SelectedIndex];
            string name = headgroup + "/" + textBox1.Text;
            
            if (textBox1.Text.Length == 0)
            {
                MessageBox.Show("Please set a suffix!", "Not registrable");
            }
            else if(creatorGUI.lipidCreator.headgroups.ContainsKey(name))
            {
                MessageBox.Show("The lipid class '" + headgroup + "' with heavy label suffix '" + textBox1.Text + "' is already registered!", "Not registrable");
            }   
            else
            {
                int numHeavyElements = 0;
            
                // create and set precursor properties
                Precursor precursor = creatorGUI.lipidCreator.headgroups[headgroup];
                Precursor heavyPrecursor = new Precursor();
                heavyPrecursor.elements = createElementData((Dictionary<string, object[]>)buildingBlockDataTables[0]);
                foreach(KeyValuePair<int, int> row in heavyPrecursor.elements) if (MS2Fragment.HEAVY_SHORTCUTS.ContainsKey(row.Key) && row.Value > 0) numHeavyElements += row.Value;
                heavyPrecursor.name = name;
                heavyPrecursor.category = precursor.category;
                heavyPrecursor.pathToImage = precursor.pathToImage;
                heavyPrecursor.buildingBlockType = precursor.buildingBlockType;
                foreach (KeyValuePair<string, bool> kvp in precursor.adductRestrictions) heavyPrecursor.adductRestrictions.Add(kvp.Key, kvp.Value);
                heavyPrecursor.derivative = precursor.derivative;
                heavyPrecursor.heavyLabeled = true;
                heavyPrecursor.userDefined = true;
                heavyPrecursor.userDefinedFattyAcids = new ArrayList();
                for (int i = 1; i < buildingBlockDataTables.Count; ++i)
                {
                    Dictionary<int, int> newElements = createElementData((Dictionary<string, object[]>)buildingBlockDataTables[i]);
                    foreach(KeyValuePair<int, int> row in newElements) if (MS2Fragment.HEAVY_SHORTCUTS.ContainsKey(row.Key) && row.Value > 0) numHeavyElements += row.Value;
                    heavyPrecursor.userDefinedFattyAcids.Add(newElements);
                }
                
                if (numHeavyElements == 0)
                {
                    MessageBox.Show("No building block contains a heavy isotope!", "Not registrable");
                }
                else
                {
                    creatorGUI.lipidCreator.headgroups.Add(name, heavyPrecursor);
                    precursor.heavyLabeledPrecursors.Add(heavyPrecursor);
                    
                    creatorGUI.lipidCreator.categoryToClass[(int)heavyPrecursor.category].Add(name);
                    
                    // copy all MS2Fragments
                    creatorGUI.lipidCreator.allFragments.Add(name, new Dictionary<bool, Dictionary<string, MS2Fragment>>());
                    creatorGUI.lipidCreator.allFragments[name].Add(true, new Dictionary<string, MS2Fragment>());
                    creatorGUI.lipidCreator.allFragments[name].Add(false, new Dictionary<string, MS2Fragment>());
                    foreach (KeyValuePair<string, MS2Fragment> ms2Fragment in creatorGUI.lipidCreator.allFragments[precursor.name][true])
                    {
                        creatorGUI.lipidCreator.allFragments[name][true].Add(ms2Fragment.Key, new MS2Fragment(ms2Fragment.Value));
                    }
                    foreach (KeyValuePair<string, MS2Fragment> ms2Fragment in creatorGUI.lipidCreator.allFragments[precursor.name][false])
                    {
                        creatorGUI.lipidCreator.allFragments[name][false].Add(ms2Fragment.Key, new MS2Fragment(ms2Fragment.Value));
                    }
                    MessageBox.Show("Heavy isotope was successfully added!", "Isotope added");
                }
            }
        }
    }
}
