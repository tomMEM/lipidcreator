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
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace LipidCreator
{
    public partial class TranslatorDialog : Form
    {
        public CreatorGUI creatorGUI;
        public DataTable lipidNamesList;
        public ArrayList parsedLipids;
        public Image whiteImage;
    
        public const string GRAMMER_FILENAME = "data/lipidmaps.grammer";
        public const char QUOTE = '"';
        public bool tableInitialized = false;
            
        public LipidMapsParserEventHandler lipidMapsParserEventHandler;
        public Parser parser;
        public const string FIRST_HEADER = "Old lipid name";
        public const string SECOND_HEADER = "Current lipid name";
        public const string DELETE_HEADER = "Delete";

    
        public TranslatorDialog(CreatorGUI _creatorGUI)
        {
            parsedLipids = new ArrayList();
            creatorGUI = _creatorGUI;
            lipidMapsParserEventHandler = new LipidMapsParserEventHandler(creatorGUI.lipidCreator);
            parser = new Parser(lipidMapsParserEventHandler, GRAMMER_FILENAME, QUOTE);
            
            whiteImage = Image.FromFile(creatorGUI.lipidCreator.prefixPath + "images/white.png");

            lipidNamesList = new DataTable("lipidNamesList");
            lipidNamesList.Columns.Add(new DataColumn(FIRST_HEADER));
            lipidNamesList.Columns[0].DataType = typeof(string);
            lipidNamesList.Columns.Add(new DataColumn(SECOND_HEADER));
            lipidNamesList.Columns[1].DataType = typeof(string);
            lipidNamesList.Columns[1].ReadOnly = true;
            
            InitializeComponent();
            disableImport();
        }
        
        
        private void lipidNamesGridViewDataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (tableInitialized) return;
            tableInitialized = true;
            DataGridViewImageColumn deleteColumn = new DataGridViewImageColumn();  
            deleteColumn.Name = "Delete";  
            deleteColumn.HeaderText = "Delete";  
            deleteColumn.ValuesAreIcons = false;
            deleteColumn.Width = 40;
            lipidNamesGridView.Columns.Add(deleteColumn);
            lipidNamesGridView.Columns[0].Width = lipidNamesGridView.Width >> 1;
            lipidNamesGridView.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            lipidNamesGridView.AllowUserToResizeColumns = false;
            
            
            DataRow row = lipidNamesList.NewRow();
            row[FIRST_HEADER] = "";
            row[SECOND_HEADER] = "";
            lipidNamesList.Rows.Add(row);
            
            lipidNamesGridView.Rows[0].Cells[DELETE_HEADER].Value = whiteImage;
            
            foreach (DataGridViewColumn dgvc in lipidNamesGridView.Columns) {
                dgvc.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            
            lipidNamesGridView.Update();
            lipidNamesGridView.Refresh();
            lipidNamesGridView.AllowUserToAddRows = false;
        }

        
        
        
        private void lipidNamesGridViewCellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == lipidNamesList.Rows.Count - 1 && (string)lipidNamesList.Rows[e.RowIndex][FIRST_HEADER] != "")
            {
                DataRow row = lipidNamesList.NewRow();
                row[FIRST_HEADER] = "";
                row[SECOND_HEADER] = "";
                lipidNamesList.Rows.Add(row);
                for (int i = 0; i < lipidNamesList.Rows.Count - 1; ++i)
                {
                    lipidNamesGridView.Rows[i].Cells[DELETE_HEADER].Value = creatorGUI.deleteImage;
                }
                lipidNamesGridView.Rows[lipidNamesList.Rows.Count - 1].Cells[DELETE_HEADER].Value = whiteImage;
                lipidNamesGridView.Update();
                lipidNamesGridView.Refresh();
            }
            disableImport();
        }
        
        
        
        public void disableImport()
        {
            button3.Enabled = false;
        }
        
        
        
        public void lipidNamesGridViewEditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            e.Control.KeyDown += new KeyEventHandler(lipidNamesGridViewKeyDown);
        }

        
        
        
        public void lipidNamesGridViewKeyDown(object sender, KeyEventArgs e)
        {
            lipidNamesGridView.AllowUserToAddRows = true;
            if (e.Modifiers == Keys.Control && e.KeyCode == Keys.V)
            {
                int currentCell = lipidNamesGridView.CurrentCell.RowIndex; 
                string[] insertText = Clipboard.GetText().Split('\n');
                foreach (string ins in insertText)
                {
                    string insert = Parser.strip(ins, (char)13);
                    insert = Parser.strip(insert, (char)10);
                    if (insert.Length == 0) continue;                    
                    if (currentCell < lipidNamesList.Rows.Count)
                    {
                        lipidNamesList.Rows[currentCell][FIRST_HEADER] = insert;
                    }
                    else 
                    {
                        lipidNamesList.Rows.Add(new string[]{insert, ""});
                    }
                    ++currentCell;
                }
                if (currentCell == lipidNamesList.Rows.Count)
                {
                    lipidNamesList.Rows.Add(new string[]{"", ""});
                }
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Delete && lipidNamesGridView.CurrentCell.RowIndex != lipidNamesList.Rows.Count - 1)
            {
                lipidNamesList.Rows.RemoveAt(lipidNamesGridView.CurrentCell.RowIndex);
            }
            for (int i = 0; i < lipidNamesList.Rows.Count - 1; ++i)
            {
                lipidNamesGridView.Rows[i].Cells[DELETE_HEADER].Value = creatorGUI.deleteImage;
            }
            lipidNamesGridView.Rows[lipidNamesList.Rows.Count - 1].Cells[DELETE_HEADER].Value = whiteImage;
            lipidNamesGridView.Update();
            lipidNamesGridView.Refresh();
            lipidNamesGridView.AllowUserToAddRows = false;
            
            disableImport();
        }
        
        
        
        
        public void lipidsGridviewDoubleClick(Object sender, EventArgs e)
        {
            int rowIndex = ((DataGridView)sender).CurrentCell.RowIndex;
            int colIndex = ((DataGridView)sender).CurrentCell.ColumnIndex;
            if (((DataGridView)sender).Columns[colIndex].Name == "Delete")
            {
                lipidNamesList.Rows.RemoveAt(rowIndex);
                for (int i = 0; i < lipidNamesList.Rows.Count - 1; ++i)
                {
                    lipidNamesGridView.Rows[i].Cells[DELETE_HEADER].Value = creatorGUI.deleteImage;
                }
                lipidNamesGridView.Rows[lipidNamesList.Rows.Count - 1].Cells[DELETE_HEADER].Value = whiteImage;
                lipidNamesGridView.Update();
                lipidNamesGridView.Refresh();
            }
        }
        

        
        
        // cancel
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }
        
        
        
        
        // translate
        private void button2_Click(object sender, EventArgs e)
        {
            lipidNamesList.Columns[1].ReadOnly = false;
            parsedLipids.Clear();
            int i = 0;
            foreach (DataRow row in lipidNamesList.Rows)
            {
                if (i == lipidNamesList.Rows.Count - 1) break;
                lipidNamesGridView.Rows[i].DefaultCellStyle.BackColor = Color.Empty;
                Lipid lipid = null;
                if (row[FIRST_HEADER] is string)
                {
                    string oldLipidName = (string)row[FIRST_HEADER];
                    if (oldLipidName.Length > 0)
                    {
                        parser.parse(oldLipidName);
                        if (parser.wordInGrammer)
                        {
                            parser.raiseEvents();
                            if (lipidMapsParserEventHandler.lipid != null)
                            {
                                lipid = lipidMapsParserEventHandler.lipid;
                            }
                        }
                    }
                }
                parsedLipids.Add(lipid);
                ++i;
            }
            
            
            HashSet<String> usedKeys = new HashSet<String>();
            ArrayList precursorDataList = new ArrayList();
            i = 0;
            int correctlyParsed = 0;
            foreach (Lipid currentLipid in parsedLipids)
            {
                if (currentLipid != null)
                {
                    currentLipid.computePrecursorData(creatorGUI.lipidCreator.headgroups, usedKeys, precursorDataList);
                    lipidNamesList.Rows[i][SECOND_HEADER] = ((PrecursorData)precursorDataList[precursorDataList.Count - 1]).precursorName;
                    usedKeys.Clear();
                    if (!(currentLipid is UnsupportedLipid)) ++correctlyParsed;
                    else
                    {
                        lipidNamesGridView.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                    }
                    
                }
                else
                {
                    lipidNamesList.Rows[i][SECOND_HEADER] = "Unrecognized molecule";
                    lipidNamesGridView.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                }
                ++i;
            }
            lipidNamesList.Columns[1].ReadOnly = true;
            lipidNamesGridView.Refresh();
            
            if (correctlyParsed == lipidNamesList.Rows.Count - 1) button3.Enabled = true;
        }

        
        
        
        
        // import
        private void button3_Click(object sender, EventArgs e)
        {
            foreach(Lipid currentLipid in parsedLipids)
            {
                creatorGUI.lipidCreator.registeredLipids.Add(currentLipid);
            }
            creatorGUI.refreshRegisteredLipidsTable();
            Close();
        }
    }
}
