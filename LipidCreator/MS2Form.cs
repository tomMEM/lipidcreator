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
        
        public Image fragment_complete = null;
        public Lipid currentLipid;
        public ArrayList positiveIDs;
        public ArrayList negativeIDs;
        public CreatorGUI creatorGUI;
        public bool senderInterupt;
        
        public MS2Form(CreatorGUI creatorGUI, Lipid currentLipid)
        {
            this.creatorGUI = creatorGUI;
            positiveIDs = new ArrayList();
            negativeIDs = new ArrayList();
            senderInterupt = false;
            
            
            if (currentLipid is CLLipid ) this.currentLipid = new CLLipid((CLLipid)currentLipid);
            else if (currentLipid is GLLipid ) this.currentLipid = new GLLipid((GLLipid)currentLipid);
            else if (currentLipid is PLLipid ) this.currentLipid = new PLLipid((PLLipid)currentLipid);
            else if (currentLipid is SLLipid ) this.currentLipid = new SLLipid((SLLipid)currentLipid);
            
            InitializeComponent(currentLipid.MS2Fragments);            
            tabChange(0);
        }

        void checkedListBox_MouseLeave(object sender, EventArgs e)
        {
            pictureBox1.Image = fragment_complete;
        }
        
        void checkedListBoxPositiveSelectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge > 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = true; 
            }
            for (int i = 0; i < checkedListBox1.Items.Count; ++i)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
            senderInterupt = false;
        }
        
        void checkedListBoxPositiveDeselectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge > 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = false;  
            }
            for (int i = 0; i < checkedListBox1.Items.Count; ++i)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
            senderInterupt = false;
        }
        
        void checkedListBoxNegativeSelectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge < 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = true; 
            }
            for (int i = 0; i < checkedListBox2.Items.Count; ++i)
            {
                checkedListBox2.SetItemChecked(i, true);
            }
            senderInterupt = false;
        }
        
        void checkedListBoxNegativeDeselectAll(object sender, EventArgs e)
        {
            senderInterupt = true;
            String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                if (((MS2Fragment)currentFragments[i]).fragmentCharge < 0) ((MS2Fragment)currentFragments[i]).fragmentSelected = false; 
            }
            for (int i = 0; i < checkedListBox2.Items.Count; ++i)
            {
                checkedListBox2.SetItemChecked(i, false);
            }
            senderInterupt = false;
        }

        private void checkedListBox1_MouseHover(object sender, MouseEventArgs e)
        {

            toolTip1.Hide(this.checkedListBox1);
            toolTip1.SetToolTip(this.checkedListBox2, "");
            Point point = checkedListBox1.PointToClient(Cursor.Position);
            int hoveredIndex = checkedListBox1.IndexFromPoint(point);

            if (hoveredIndex != -1)
            {
                int fragmentIndex = (int)positiveIDs[hoveredIndex];
                String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
                String filePath = ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentFile;
                if (filePath != null) pictureBox1.Image = Image.FromFile(filePath);
                
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
                toolTip1.SetToolTip(this.checkedListBox1, toolTipText);
            }
            else
            {
                pictureBox1.Image = fragment_complete;
            }
        }

        private void checkedListBox2_MouseHover(object sender, MouseEventArgs e)
        {

            toolTip1.Hide(this.checkedListBox2);
            toolTip1.SetToolTip(this.checkedListBox1, "");
            Point point = checkedListBox2.PointToClient(Cursor.Position);
            int hoveredIndex = checkedListBox2.IndexFromPoint(point);

            if (hoveredIndex != -1)
            {
                int fragmentIndex = (int)negativeIDs[hoveredIndex];
                String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
                String filePath = ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentFile;
                if (filePath != null) pictureBox1.Image = Image.FromFile(filePath);
                
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
                toolTip1.SetToolTip(this.checkedListBox2, toolTipText);
            }
            else
            {
                pictureBox1.Image = fragment_complete;
            }
        }

        public void tabIndexChanged(Object sender, EventArgs e)
        {
            tabChange(((TabControl)sender).SelectedIndex);
        }

        public void tabChange(int index)
        {
            ((TabPage)tabPages[index]).Controls.Add(checkedListBox2);
            ((TabPage)tabPages[index]).Controls.Add(label1);
            ((TabPage)tabPages[index]).Controls.Add(label2);
            ((TabPage)tabPages[index]).Controls.Add(label3);
            ((TabPage)tabPages[index]).Controls.Add(label4);
            ((TabPage)tabPages[index]).Controls.Add(label5);
            ((TabPage)tabPages[index]).Controls.Add(label6);
            ((TabPage)tabPages[index]).Controls.Add(label7);
            ((TabPage)tabPages[index]).Controls.Add(label8);
            ((TabPage)tabPages[index]).Controls.Add(label9);
            ((TabPage)tabPages[index]).Controls.Add(label10);
            ((TabPage)tabPages[index]).Controls.Add(label11);
            ((TabPage)tabPages[index]).Controls.Add(checkedListBox1);
            ((TabPage)tabPages[index]).Controls.Add(pictureBox1);
            negativeIDs.Clear();
            positiveIDs.Clear();
            checkedListBox1.Items.Clear();
            checkedListBox2.Items.Clear();
            
            String lipidClass = ((TabPage)tabPages[index]).Text;
            ArrayList currentFragments = currentLipid.MS2Fragments[lipidClass];
            for (int i = 0; i < currentFragments.Count; ++i)
            {
                MS2Fragment currentFragment = (MS2Fragment)currentFragments[i];
                if (currentFragment.fragmentCharge > 0)
                {
                    checkedListBox1.Items.Add(currentFragment.fragmentName);
                    positiveIDs.Add(i);
                    checkedListBox1.SetItemChecked(checkedListBox1.Items.Count - 1, currentFragment.fragmentSelected);
                }
                else 
                {
                    checkedListBox2.Items.Add(currentFragment.fragmentName);
                    negativeIDs.Add(i);
                    checkedListBox2.SetItemChecked(checkedListBox2.Items.Count - 1, currentFragment.fragmentSelected);
                }
            }
            
            if (currentLipid.pathsToFullImage.ContainsKey(lipidClass))
            {
                fragment_complete = Image.FromFile((String)currentLipid.pathsToFullImage[lipidClass]);
                pictureBox1.Image = fragment_complete;
            }
            else
            {
                if (fragment_complete != null)
                {
                    fragment_complete = null;
                }
                
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                    pictureBox1.Image = null;
                }
            }
        }


        void CheckedListBox1_ItemCheck(Object sender, ItemCheckEventArgs e)
        {
            if (senderInterupt) return;
            int fragmentIndex = (int)positiveIDs[e.Index];
            String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
            ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentSelected = (e.NewValue == CheckState.Checked);
        }
        
        void CheckedListBox2_ItemCheck(Object sender, ItemCheckEventArgs e)
        {
            if (senderInterupt) return;
            int fragmentIndex = (int)negativeIDs[e.Index];
            String lipidClass = ((TabPage)tabPages[tabControl1.SelectedIndex]).Text;
            ((MS2Fragment)currentLipid.MS2Fragments[lipidClass][fragmentIndex]).fragmentSelected = (e.NewValue == CheckState.Checked);
        }
        
        private void cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        private void ok_Click(object sender, EventArgs e)
        {
            if (currentLipid is CLLipid)
            {
                creatorGUI.lipidCreatorForm.lipidTabList[0] = new CLLipid((CLLipid)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidCreatorForm.lipidTabList[0];
                
            }
            else if (currentLipid is GLLipid)
            {
                creatorGUI.lipidCreatorForm.lipidTabList[1] = new GLLipid((GLLipid)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidCreatorForm.lipidTabList[1];
            }
            else if (currentLipid is PLLipid)
            {
                creatorGUI.lipidCreatorForm.lipidTabList[2] = new PLLipid((PLLipid)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidCreatorForm.lipidTabList[2];
            }
            else if (currentLipid is SLLipid)
            {
                creatorGUI.lipidCreatorForm.lipidTabList[3] = new SLLipid((SLLipid)currentLipid);
                creatorGUI.currentLipid = (Lipid)creatorGUI.lipidCreatorForm.lipidTabList[3];
            }
            
            this.Close();
        }
        
        private void add_fragment_Click(object sender, EventArgs e)
        {
            NewFragment newPositiveFragment = new NewFragment(this);
            newPositiveFragment.Owner = this;
            newPositiveFragment.ShowInTaskbar = false;
            newPositiveFragment.ShowDialog();
            newPositiveFragment.Dispose();
            tabChange(tabControl1.SelectedIndex);
        }
    }
}