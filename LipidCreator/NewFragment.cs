﻿/*
MIT License

Copyright (c) 2018 Dominik Kopczynski   -   dominik.kopczynski {at} isas.de
                   Bing Peng   -   bing.peng {at} isas.de
                   Nils Hoffmann  -  nils.hoffmann {at} isas.de

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
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using log4net;

namespace LipidCreator
{
    [Serializable]
    public partial class NewFragment : Form
    {
        
        private static readonly ILog log = LogManager.GetLogger(typeof(NewFragment));
        public Dictionary<string, object[]> elements;
        public MS2Form ms2form = null;
        public string[] buildingBlocks;
        public bool updating = false;
        public bool edit = false;
        public MS2Fragment ms2Fragment;
        public string fragmentOutputName = "";
        public LipidException lipidException;
        

        public NewFragment(MS2Form ms2form, bool _edit = false, LipidException _lipidException = null)
        {
            lipidException = _lipidException;
            this.ms2form = ms2form;
            edit = _edit;
            elements = createGridData(MS2Fragment.createEmptyElementDict());
            InitializeComponent();
            InitializeCustom();

            updating = true;
            dataGridViewElements.ColumnCount = 3;
            dataGridViewElements.Columns[0].Name = "Element";
            dataGridViewElements.Columns[0].DefaultCellStyle.BackColor = Color.LightGray;
            dataGridViewElements.Columns[1].Name = "Count (Monoisotopic)";
            dataGridViewElements.Columns[2].Name = "Count (Isotopic)";
            DataGridViewComboBoxColumn combo1 = new DataGridViewComboBoxColumn();
            dataGridViewElements.Columns.Add(combo1);
            dataGridViewElements.Columns[0].Width = (dataGridViewElements.Width - 2) / 4;
            dataGridViewElements.Columns[0].ReadOnly = true;
            dataGridViewElements.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewElements.Columns[1].Width = (dataGridViewElements.Width - 2) / 4;
            dataGridViewElements.Columns[1].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewElements.Columns[2].Width = (dataGridViewElements.Width - 2) / 4;
            dataGridViewElements.Columns[2].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewElements.Columns[3].Width = (dataGridViewElements.Width - 2) / 4;
            dataGridViewElements.Columns[3].SortMode = DataGridViewColumnSortMode.NotSortable;
            dataGridViewElements.AllowUserToAddRows = false;
            combo1.Name = "Isotope type";
            
            
            foreach (Molecule m in MS2Fragment.ALL_ELEMENTS.Keys.Where(x => !MS2Fragment.ALL_ELEMENTS[x].isHeavy))
            {
                dataGridViewElements.Rows.Add(new object[] {"-", 0, 0, new DataGridViewComboBoxCell()});
            }
            foreach (Molecule molecule in MS2Fragment.ALL_ELEMENTS.Keys.Where(x => !MS2Fragment.ALL_ELEMENTS[x].isHeavy))
            {
                int l = MS2Fragment.ALL_ELEMENTS[molecule].position;
                dataGridViewElements.Rows[l].Cells[0].Value = MS2Fragment.ALL_ELEMENTS[molecule].shortcut;
                dataGridViewElements.Rows[l].Cells[1].Value = 0;
                dataGridViewElements.Rows[l].Cells[2].Value = 0;
                
                DataGridViewComboBoxCell cell = (DataGridViewComboBoxCell)dataGridViewElements.Rows[l].Cells[3];
                int j = 0;
                foreach (Molecule heavyMolecule in MS2Fragment.ALL_ELEMENTS[molecule].derivatives)
                {
                    if (j++ == 0) cell.Value = MS2Fragment.ALL_ELEMENTS[heavyMolecule].shortcutNumber;
                    cell.Items.Add(MS2Fragment.ALL_ELEMENTS[heavyMolecule].shortcutNumber);
                }
            }
            updating = false;
            
            // base types:
            // 0 -> fixed, FA1, FA2, FA3, FA4, HG
            // 1 -> fixed, FA1, FA2, FA3, HG
            // 2 -> fixed, FA1, FA2, HG
            // 3 -> fixed, FA, HG
            // 4 -> fixed, LCB, FA, HG
            // 5 -> fixed, LCB, HG
            // 6 -> fixed, HG
            string lipidClass = ms2form.getHeadgroup();
            int bbType = ms2form.creatorGUI.lipidCreator.headgroups[lipidClass].buildingBlockType;
            
            
            String[][] buildingBlockList = new String[7][];
            buildingBlockList[0] = new String[]{"FA1", "FA2", "FA3", "FA4", "HG"};
            buildingBlockList[1] = new String[]{"FA1", "FA2", "FA3", "HG"};
            buildingBlockList[2] = new String[]{"FA1", "FA2", "HG"};
            buildingBlockList[3] = new String[]{"FA", "HG"};
            buildingBlockList[4] = new String[]{"LCB", "FA", "HG"};
            buildingBlockList[5] = new String[]{"LCB", "HG"};
            buildingBlockList[6] = new String[]{"HG"};
            
            
            selectBaseCombobox.Items.Add("fixed");
            ArrayList buildingBlocksArray = new ArrayList();
            buildingBlocksArray.Add("");
            createCombinations(buildingBlockList[bbType], -1, "", selectBaseCombobox, buildingBlocksArray);
            buildingBlocks = (string[])buildingBlocksArray.ToArray(typeof(string));
            selectBaseCombobox.SelectedIndex = 0;
            
            if (edit)
            {
                bool isPositive = ms2form.editDeletePositive.Name.Equals("checkedListBoxPositive");
                string fragmentName = (string)ms2form.editDeletePositive.Items[ms2form.editDeleteIndex];
                ms2Fragment = new MS2Fragment(ms2form.creatorGUI.lipidCreator.allFragments[lipidClass][isPositive][fragmentName]);
                textBoxFragmentName.Text = fragmentName;
                textBoxFragmentName.Enabled = false;
                numericUpDownCharge.Enabled = false;
                addButton.Text = "OK";
                fragmentOutputName = ms2Fragment.fragmentOutputName;
                
                HashSet<string> fragmentBB = new HashSet<string>(  (String[]) ms2Fragment.fragmentBase.ToArray( typeof( string ) ));
                for (int i = 1; i < buildingBlocksArray.Count; ++i)
                {
                    HashSet<string> currentBB = new HashSet<string>(((string)buildingBlocksArray[i]).Split(new char[]{';'}));
                    int intersect = fragmentBB.Intersect(currentBB).Count();
                    if (intersect == fragmentBB.Count() && intersect == currentBB.Count())
                    {
                        selectBaseCombobox.SelectedIndex = i;
                        break;
                    }
                }
                
                numericUpDownCharge.Value = ms2Fragment.fragmentAdduct.charge;
                
                ElementDictionary input = ms2Fragment.fragmentElements;
                Dictionary<string, object[]> data = new Dictionary<string, object[]>();
                
            
                foreach (KeyValuePair<Molecule, int> row in input)
                {
                    if (!MS2Fragment.ALL_ELEMENTS[row.Key].isHeavy)
                    {
                        // check for heavy isotopes
                        int heavyElementIndex = MS2Fragment.ALL_ELEMENTS[row.Key].derivatives.Count() - 1;
                        int heavyElementCount = 0;
                        string heavyShortcut = "";
                        for (; heavyElementIndex >= 0; --heavyElementIndex)
                        {
                            heavyElementCount = input[MS2Fragment.ALL_ELEMENTS[row.Key].derivatives[heavyElementIndex]];
                            heavyShortcut = MS2Fragment.ALL_ELEMENTS[MS2Fragment.ALL_ELEMENTS[row.Key].derivatives[heavyElementIndex]].shortcutNumber;
                            if (input[MS2Fragment.ALL_ELEMENTS[row.Key].derivatives[heavyElementIndex]] > 0)
                            {
                                break;
                            }
                        }
                
                        data.Add(MS2Fragment.ALL_ELEMENTS[row.Key].shortcut, new object[]{row.Value, heavyElementCount, heavyShortcut});
                    }
                }
                
                foreach (KeyValuePair<string, object[]> row in data)
                {
                    int l = MS2Fragment.ALL_ELEMENTS[MS2Fragment.ELEMENT_POSITIONS[row.Key]].position;
                    
                    dataGridViewElements.Rows[l].Cells[1].Value = row.Value[0];
                    dataGridViewElements.Rows[l].Cells[2].Value = row.Value[1];
                    dataGridViewElements.Rows[l].Cells[3].Value = row.Value[2];
                }
            }
            
            if (ms2form.isotopeList.SelectedIndex == 0)
            {
                dataGridViewElements.Columns[2].DefaultCellStyle.BackColor = Color.LightGray;
                dataGridViewElements.Columns[2].ReadOnly = true;
                dataGridViewElements.Columns[3].DefaultCellStyle.BackColor = Color.LightGray;
                dataGridViewElements.Columns[3].ReadOnly = true;
            }
        }
        
        
        public void createCombinations(string[] tokens, int pos, string baseCombination, ComboBox combobox, ArrayList buildingBlocksArray)
        {
            if (pos == tokens.Length) return;
            for (int i = pos + 1; i < tokens.Length; ++i)
            {
                string newBaseCombination = baseCombination + (baseCombination.Length > 0 ? ";" : "") +tokens[i];
                buildingBlocksArray.Add(newBaseCombination);
                combobox.Items.Add(newBaseCombination.Replace(";", " + "));
                createCombinations(tokens, i, newBaseCombination, combobox, buildingBlocksArray);
            }
        }
        

        private void cancelClick(object sender, EventArgs e)
        {
            this.Close();
        }
        
        
        
        private void Form_Shown(Object sender, EventArgs e)
        {
            if (lipidException == null) return;
            
            for (int i = 0; i < dataGridViewElements.Rows.Count; ++i)
            {
                if (((string)dataGridViewElements.Rows[i].Cells[0].Value).Equals(MS2Fragment.ALL_ELEMENTS[lipidException.molecule].shortcut))
                {
                    dataGridViewElements.CurrentCell = dataGridViewElements.Rows[i].Cells[2];
                    break;
                }
            }
        }
        
        
        
        public Dictionary<string, object[]> createGridData(ElementDictionary input)
        {
            Dictionary<string, object[]> data = new Dictionary<string, object[]>();
            
            foreach (KeyValuePair<Molecule, int> row in input)
            {
                if (!MS2Fragment.ALL_ELEMENTS[row.Key].isHeavy)
                {
                    // check for heavy isotopes
                    int heavyElementIndex = MS2Fragment.ALL_ELEMENTS[row.Key].derivatives.Count() - 1;
                    int heavyElementCount = 0;
                    string heavyShortcut = "";
                    for (; heavyElementIndex >= 0; --heavyElementIndex)
                    {
                        heavyElementCount = input[MS2Fragment.ALL_ELEMENTS[row.Key].derivatives[heavyElementIndex]];
                        heavyShortcut = MS2Fragment.ALL_ELEMENTS[MS2Fragment.ALL_ELEMENTS[row.Key].derivatives[heavyElementIndex]].shortcutNumber;
                        if (input[MS2Fragment.ALL_ELEMENTS[row.Key].derivatives[heavyElementIndex]] > 0)
                        {
                            break;
                        }
                    }
            
                    data.Add(MS2Fragment.ALL_ELEMENTS[row.Key].shortcut, new object[]{row.Value, heavyElementCount, heavyShortcut});
                }
            }
            return data;
        }
        
        
        
        
        public ElementDictionary createElementData(Dictionary<string, object[]> input)
        {
            ElementDictionary elements = MS2Fragment.createEmptyElementDict();
            foreach (KeyValuePair<string, object[]> row in input)
            {
                Molecule elementIndex = MS2Fragment.ELEMENT_POSITIONS[row.Key];
                Molecule heavyIndex = MS2Fragment.ELEMENT_POSITIONS[(string)row.Value[2]];
                
                elements[elementIndex] = (int)row.Value[0];
                elements[heavyIndex] = (int)row.Value[1];
                
            }
            return elements;
        }
        

        private void addClick(object sender, EventArgs e)
        {
            int elementsSelected = 0;
            foreach (KeyValuePair<string, object[]> row in elements)
            {
                elementsSelected += (int)row.Value[0];
                elementsSelected += (int)row.Value[1];
            }
            if (elementsSelected == 0)
            {
                MessageBox.Show("No element selected");
                return;
            }
            if (textBoxFragmentName.Text == "")
            {
                MessageBox.Show("No name defined");
                return;
            }
            if (numericUpDownCharge.Value == 0)
            {
                MessageBox.Show("Fragment must have an either positive or negative charge");
                return;
            }
            
            string lipidClass = ms2form.getHeadgroup();
            int charge = Convert.ToInt32(numericUpDownCharge.Value);
            if (!edit && ms2form.creatorGUI.lipidCreator.allFragments[lipidClass][charge >= 0].ContainsKey(textBoxFragmentName.Text))
            {
                MessageBox.Show((charge >= 0 ? "Positive" : "Negative") + " fragment '" + textBoxFragmentName.Text + "' already registered for lipid class '" + lipidClass + "'");
                return;
            }
        
            
            ElementDictionary newElements = createElementData(elements);
            if (fragmentOutputName == "") fragmentOutputName = textBoxFragmentName.Text;
            MS2Fragment newFragment = new MS2Fragment(textBoxFragmentName.Text, fragmentOutputName, Lipid.chargeToAdduct[charge], "", newElements, buildingBlocks[selectBaseCombobox.SelectedIndex]);
            newFragment.userDefined = true;
                
            if (!edit)
            {
                ms2form.creatorGUI.lipidCreator.allFragments[lipidClass][charge >= 0].Add(textBoxFragmentName.Text, newFragment);
                if (Convert.ToInt32(numericUpDownCharge.Value) > 0)
                {
                    ms2form.checkedListBoxPositiveFragments.Items.Add(textBoxFragmentName.Text);
                }
                else
                {
                    ms2form.checkedListBoxNegativeFragments.Items.Add(textBoxFragmentName.Text);
                }
            }
            else {
                newFragment.fragmentFile = ms2Fragment.fragmentFile;
                ms2form.creatorGUI.lipidCreator.allFragments[lipidClass][charge >= 0][textBoxFragmentName.Text] = newFragment;
            }
            this.Close();
        }
        
        
        
        private void dataGridView1CellValueChanged(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if(updating) return;
            updating = true;
            string key = dataGridViewElements.Rows[e.RowIndex].Cells[0].Value.ToString();
            string val;
            
            try 
            {
                val = dataGridViewElements.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            }
            catch(Exception ee)
            {
                log.Error("Conversion error while updating cell value, setting value to 0.", ee);
                dataGridViewElements.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "0";
                val = "0";
            }
            if (e.ColumnIndex != 3)
            {
                int n;
                try {
                    n = Convert.ToInt32(val);
                }
                catch (Exception ee){
                    log.Error("Conversion error while updating cell value to int32: " + val, ee);
                    n = 0;
                }
                if (selectBaseCombobox.SelectedIndex == 0) n = Math.Max(n, 0);
                elements[key][e.ColumnIndex - 1] = n;
                dataGridViewElements.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = n;
            }
            else
            {
                elements[key][e.ColumnIndex - 1] = val;
            }
            updateInfo();
            updating = false;
        }
        
        
        public void selectBaseComboboxValueChanged(Object sender, EventArgs e)
        {
            if (selectBaseCombobox.SelectedIndex == 0)
            {
                for (int i = 0; i < dataGridViewElements.Rows.Count; ++i){
                    if ((int)dataGridViewElements.Rows[i].Cells[1].Value < 0) dataGridViewElements.Rows[i].Cells[1].Value = 0;
                    if ((int)dataGridViewElements.Rows[i].Cells[2].Value < 0) dataGridViewElements.Rows[i].Cells[2].Value = 0;
                }
            }
            updateInfo();
        }

        private void updateInfo()
        {
            //double mass = 0;
            string chemFormP = "";
            string chemFormN = "";
            string baseName = "";
            string chrg = "";
            
            if (selectBaseCombobox.SelectedIndex > 0)
            {
                baseName = (string)selectBaseCombobox.SelectedItem;
            }
            
            foreach (KeyValuePair<Molecule, Element> row in MS2Fragment.ALL_ELEMENTS)
            {
                if (row.Value.isHeavy) continue;
                
                string element = MS2Fragment.ALL_ELEMENTS[row.Key].shortcut;
                int elementCount = (int)elements[element][0];
                int heavyElementCount = (int)elements[element][1];
                string heavyElement = MS2Fragment.ALL_ELEMENTS[MS2Fragment.ELEMENT_POSITIONS[(string)elements[element][2]]].shortcut;
                
                if (elementCount > 0) chemFormP += element + Convert.ToString(elementCount);
                if (heavyElementCount > 0) chemFormP += heavyElement + Convert.ToString(heavyElementCount);
                
                if (elementCount < 0) chemFormN += element + Convert.ToString(-elementCount);
                if (heavyElementCount < 0) chemFormN += heavyElement + Convert.ToString(-heavyElementCount);
            }
            
            if (numericUpDownCharge.Value > 0) chrg = Convert.ToString(numericUpDownCharge.Value) + "+";
            else if (numericUpDownCharge.Value < 0) chrg = Convert.ToString(-numericUpDownCharge.Value) + "-";
            
            string combinedChemForm = baseName;
            if (chemFormP.Length > 0 || chemFormN.Length > 0)
            {
                if (chemFormP.Length > 0) combinedChemForm += ((combinedChemForm != "") ? " + " : "") + chemFormP;
                if (chemFormN.Length > 0) combinedChemForm += " - " + chemFormN;
            }
            if (combinedChemForm != "") labelMass.Text = "(" + combinedChemForm + ")" + chrg;
            else labelMass.Text = "";
        }

        private void numericUpDown1TextChanged(object sender, EventArgs e)
        {
            updateInfo();
        }
    }
}
