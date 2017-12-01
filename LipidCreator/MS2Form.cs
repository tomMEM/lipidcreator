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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LipidCreator
{
    [Serializable]
    public partial class MS2Form : Form
    {
        
        public Image fragmentComplete = null;
        public Lipid currentLipid;
        public ArrayList positiveIDs;
        public ArrayList negativeIDs;
        public CreatorGUI creatorGUI;
        public bool senderInterupt;
        public bool loading;
        
        public MS2Form(CreatorGUI creatorGUI, Lipid currentLipid)
        {
            this.creatorGUI = creatorGUI;
            positiveIDs = new ArrayList();
            negativeIDs = new ArrayList();
            senderInterupt = false;
            loading = false;
            
            
            if (currentLipid is GLLipid){
                this.currentLipid = new GLLipid((GLLipid)currentLipid);
            }
            else if (currentLipid is PLLipid)
            {
                this.currentLipid = new PLLipid((PLLipid)currentLipid);
            }
            else if (currentLipid is SLLipid)
            {
                this.currentLipid = new SLLipid((SLLipid)currentLipid);
            }
            else if (currentLipid is Cholesterol)
            {
                this.currentLipid = new Cholesterol((Cholesterol)currentLipid);
            }
            
            InitializeComponent();
            
            foreach (KeyValuePair<String, ArrayList> item in this.currentLipid.MS2Fragments)
            {
                if (!creatorGUI.lipidCreator.headgroups[item.Key].heavyLabeled)
                {
                    TabPage tp = new TabPage();
                    tp.Location = new System.Drawing.Point(4, 22);
                    tp.Name = item.Key;
                    tp.Padding = new System.Windows.Forms.Padding(3);
                    tp.Size = new System.Drawing.Size(766, 372);
                    tp.TabIndex = 0;
                    tp.Text = item.Key;
                    tp.UseVisualStyleBackColor = true;
                    this.tabControlFragments.Controls.Add(tp);
                    this.tabPages.Add(tp);
                }
            }
            
            tabChange(0);
        }
        
        public string getHeadgroup()
        {
            if (isotopeList.SelectedIndex == 0) return ((TabPage)tabPages[tabControlFragments.SelectedIndex]).Name;
            return ((string)isotopeList.Items[isotopeList.SelectedIndex]).Replace(Lipid.HEAVY_LABEL_SEPARATOR, "/");
        }

        void checkedListBoxMouseLeave(object sender, EventArgs e)
        {
            pictureBoxFragments.Image = fragmentComplete;
        }
        
        void checkedListBoxPositiveSelectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = getHeadgroup();
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge > 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = true; 
            }
            for (int i = 0; i < checkedListBoxPositiveFragments.Items.Count; ++i)
            {
                checkedListBoxPositiveFragments.SetItemChecked(i, true);
            }
            senderInterupt = false;
        }
        
        void checkedListBoxPositiveDeselectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = getHeadgroup();
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge > 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = false;  
            }
            for (int i = 0; i < checkedListBoxPositiveFragments.Items.Count; ++i)
            {
                checkedListBoxPositiveFragments.SetItemChecked(i, false);
            }
            senderInterupt = false;
        }
        
        void checkedListBoxNegativeSelectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = getHeadgroup();
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge < 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = true; 
            }
            for (int i = 0; i < checkedListBoxNegativeFragments.Items.Count; ++i)
            {
                checkedListBoxNegativeFragments.SetItemChecked(i, true);
            }
            senderInterupt = false;
        }
        
        void checkedListBoxNegativeDeselectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = getHeadgroup();
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge < 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = false; 
            }
            for (int i = 0; i < checkedListBoxNegativeFragments.Items.Count; ++i)
            {
                checkedListBoxNegativeFragments.SetItemChecked(i, false);
            }
            senderInterupt = false;
        }

        private void checkedListBoxPositiveMouseHover(object sender, MouseEventArgs e)
        {

            toolTip1.Hide(this.checkedListBoxPositiveFragments);
            toolTip1.SetToolTip(this.checkedListBoxNegativeFragments, "");
            Point point = checkedListBoxPositiveFragments.PointToClient(Cursor.Position);
            int hoveredIndex = checkedListBoxPositiveFragments.IndexFromPoint(point);

            if (hoveredIndex != -1)
            {
                int fragmentIndex = (int)positiveIDs[hoveredIndex];
                String lipidClass = getHeadgroup();
                String filePath = ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentFile;
                if (filePath != null) pictureBoxFragments.Image = Image.FromFile(filePath);
                
                // create tool tip
                MS2Fragment fragment = (MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex];                
                string chemForm = "";
                string baseName = "";
                string connector = "";
                string lBracket = "";
                string rBracket = "";
                bool chemAdding = true;
                
                if (fragment.fragmentBase.Count > 0)
                {
                    foreach (string bs in fragment.fragmentBase)
                    {
                        if (baseName.Length > 0) baseName += " + ";
                        baseName += bs;
                    }
                }
                
                foreach (DataRow row in fragment.fragmentElements.Rows)
                {
                    if (Convert.ToInt32(row["Count"]) != 0)
                    {
                        chemForm += Convert.ToString(row["Shortcut"]) + Convert.ToString(Math.Abs(Convert.ToInt32(row["Count"])));
                        chemAdding = Convert.ToInt32(row["Count"]) > 0;
                    }
                }
                if (baseName.Length > 0 && chemForm.Length > 0)
                {
                    connector = chemAdding ? " + " : " - ";
                    lBracket = "(";
                    rBracket = ")";
                }
                string toolTipText = lBracket + baseName + connector + chemForm + rBracket + "+";
                toolTip1.SetToolTip(this.checkedListBoxPositiveFragments, toolTipText);
            }
            else
            {
                pictureBoxFragments.Image = fragmentComplete;
            }
        }

        private void checkedListBoxNegativeMouseHover(object sender, MouseEventArgs e)
        {

            toolTip1.Hide(this.checkedListBoxNegativeFragments);
            toolTip1.SetToolTip(this.checkedListBoxPositiveFragments, "");
            Point point = checkedListBoxNegativeFragments.PointToClient(Cursor.Position);
            int hoveredIndex = checkedListBoxNegativeFragments.IndexFromPoint(point);

            if (hoveredIndex != -1)
            {
                int fragmentIndex = (int)negativeIDs[hoveredIndex];
                String lipidClass = getHeadgroup();
                String filePath = ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentFile;
                if (filePath != null) pictureBoxFragments.Image = Image.FromFile(filePath);
                
                // create tool tip
                MS2Fragment fragment = (MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex];                
                string chemForm = "";
                string baseName = "";
                string connector = "";
                string lBracket = "";
                string rBracket = "";
                bool chemAdding = true;
                
                if (fragment.fragmentBase.Count > 0)
                {
                    foreach (string bs in fragment.fragmentBase)
                    {
                        if (baseName.Length > 0) baseName += " + ";
                        baseName += bs;
                    }
                }
                
                foreach (DataRow row in fragment.fragmentElements.Rows)
                {
                    if (Convert.ToInt32(row["Count"]) != 0)
                    {
                        chemForm += Convert.ToString(row["Shortcut"]) + Convert.ToString(Math.Abs(Convert.ToInt32(row["Count"])));
                        chemAdding = Convert.ToInt32(row["Count"]) > 0;
                    }
                }
                if (baseName.Length > 0 && chemForm.Length > 0)
                {
                    connector = chemAdding ? " + " : " - ";
                    lBracket = "(";
                    rBracket = ")";
                }
                string toolTipText = lBracket + baseName + connector + chemForm + rBracket + "-";
                toolTip1.SetToolTip(this.checkedListBoxNegativeFragments, toolTipText);
            }
            else
            {
                pictureBoxFragments.Image = fragmentComplete;
            }
        }

        public void tabIndexChanged(Object sender, EventArgs e)
        {
            tabChange(((TabControl)sender).SelectedIndex);
        }
        
        
        public void isotopeListComboBoxValueChanged(object sender, EventArgs e)
        {
            if (loading) return;
            String lipidClass = getHeadgroup();
            negativeIDs.Clear();
            positiveIDs.Clear();
            
            checkedListBoxPositiveFragments.Items.Clear();
            checkedListBoxNegativeFragments.Items.Clear();
            
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                MS2Fragment currentFragment = (MS2Fragment)currentFragments[i];
                if (currentFragment.fragmentCharge > 0)
                {
                    checkedListBoxPositiveFragments.Items.Add(currentFragment.fragmentName);
                    positiveIDs.Add(i);
                    checkedListBoxPositiveFragments.SetItemChecked(checkedListBoxPositiveFragments.Items.Count - 1, currentFragment.fragmentSelected);
                }
                else 
                {
                    checkedListBoxNegativeFragments.Items.Add(currentFragment.fragmentName);
                    negativeIDs.Add(i);
                    checkedListBoxNegativeFragments.SetItemChecked(checkedListBoxNegativeFragments.Items.Count - 1, currentFragment.fragmentSelected);
                }
            }
            
            if (creatorGUI.lipidCreator.headgroups.ContainsKey(lipidClass) && creatorGUI.lipidCreator.headgroups[lipidClass].pathToImage.Length > 0)
            {
                fragmentComplete = Image.FromFile(creatorGUI.lipidCreator.headgroups[lipidClass].pathToImage);
                pictureBoxFragments.Image = fragmentComplete;
            }
            else
            {
                if (fragmentComplete != null)
                {
                    fragmentComplete = null;
                }
                
                if (pictureBoxFragments.Image != null)
                {
                    pictureBoxFragments.Image.Dispose();
                    pictureBoxFragments.Image = null;
                }
            }
            
        }

        public void tabChange(int index)
        {
            loading = true;
            isotopeList.Items.Clear();
            ((TabPage)tabPages[index]).Controls.Add(checkedListBoxNegativeFragments);
            ((TabPage)tabPages[index]).Controls.Add(labelPositiveFragments);
            ((TabPage)tabPages[index]).Controls.Add(labelNegativeFragments);
            ((TabPage)tabPages[index]).Controls.Add(labelFragmentDescriptionBlack);
            ((TabPage)tabPages[index]).Controls.Add(labelFragmentDescriptionRed);
            ((TabPage)tabPages[index]).Controls.Add(labelFragmentDescriptionBlue);
            ((TabPage)tabPages[index]).Controls.Add(labelPositiveSelectAll);
            ((TabPage)tabPages[index]).Controls.Add(labelPositiveDeselectAll);
            ((TabPage)tabPages[index]).Controls.Add(labelNegativeSelectAll);
            ((TabPage)tabPages[index]).Controls.Add(labelNegativeDeselectAll);
            ((TabPage)tabPages[index]).Controls.Add(labelSlashPositive);
            ((TabPage)tabPages[index]).Controls.Add(labelSlashNegative);
            ((TabPage)tabPages[index]).Controls.Add(checkedListBoxPositiveFragments);
            ((TabPage)tabPages[index]).Controls.Add(pictureBoxFragments);
            ((TabPage)tabPages[index]).Controls.Add(isotopeList);
            
            isotopeList.Items.Add("Monoisotopic");
            String lipidClass = ((TabPage)tabPages[tabControlFragments.SelectedIndex]).Name;
            foreach(Precursor heavyPrecursor in creatorGUI.lipidCreator.headgroups[lipidClass].heavyLabeledPrecursors)
            {
                isotopeList.Items.Add(heavyPrecursor.name.Replace("/", Lipid.HEAVY_LABEL_SEPARATOR));
            }
            
            loading = false;
            isotopeList.SelectedIndex = 0;
        }


        void CheckedListBoxPositiveItemCheck(Object sender, ItemCheckEventArgs e)
        {
            if (senderInterupt) return;
            int fragmentIndex = (int)positiveIDs[e.Index];
            String lipidClass = getHeadgroup();
            ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentSelected = (e.NewValue == CheckState.Checked);
        }
        
        void CheckedListBoxNegativeItemCheck(Object sender, ItemCheckEventArgs e)
        {
            if (senderInterupt) return;
            int fragmentIndex = (int)negativeIDs[e.Index];
            String lipidClass = getHeadgroup();
            ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentSelected = (e.NewValue == CheckState.Checked);
        }
        
        private void cancelClick(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void okClick(object sender, EventArgs e)
        {
            if (currentLipid is GLLipid)
            {
                creatorGUI.lipidTabList[(int)LipidCategory.GlyceroLipid] = new GLLipid((GLLipid)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidTabList[(int)LipidCategory.GlyceroLipid];
            }
            else if (currentLipid is PLLipid)
            {
                creatorGUI.lipidTabList[(int)LipidCategory.PhosphoLipid] = new PLLipid((PLLipid)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidTabList[(int)LipidCategory.PhosphoLipid];
            }
            else if (currentLipid is SLLipid)
            {
                creatorGUI.lipidTabList[(int)LipidCategory.SphingoLipid] = new SLLipid((SLLipid)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidTabList[(int)LipidCategory.SphingoLipid];
            }
            else if (currentLipid is Cholesterol)
            {
                creatorGUI.lipidTabList[(int)LipidCategory.Cholesterol] = new Cholesterol((Cholesterol)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidTabList[(int)LipidCategory.Cholesterol];
            }
            this.Close();
        }
        
        private void addFragmentClick(object sender, EventArgs e)
        {
            NewFragment newPositiveFragment = new NewFragment(this);
            newPositiveFragment.Owner = this;
            newPositiveFragment.ShowInTaskbar = false;
            newPositiveFragment.ShowDialog();
            newPositiveFragment.Dispose();
            tabChange(tabControlFragments.SelectedIndex);
        }
    }
}