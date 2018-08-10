/*
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
using System.Data;
using System.Collections;
using System.Collections.Generic;

namespace LipidCreator
{    
    
    [Serializable]
    public class LipidMapsParserEventHandler : BaseParserEventHandler
    {
        public LipidCreator lipidCreator;
        public Lipid lipid;
        public FattyAcidGroupEnumerator fagEnum;
        public FattyAcidGroup fag;
        public string mediatorName;
    
    
        public LipidMapsParserEventHandler(LipidCreator _lipidCreator) : base()
        {
            lipidCreator = _lipidCreator;
            resetLipidBuilder(null);
            
            registeredEvents.Add("lipid_pre_event", resetLipidBuilder);
            registeredEvents.Add("lipid_post_event", lipidPostEvent);
            
            registeredEvents.Add("Pure_FA_pre_event", PureFAPreEvent);
            
            registeredEvents.Add("FA_pre_event", FAPreEvent);
            registeredEvents.Add("FA_post_event", FAPostEvent);
            
            registeredEvents.Add("LCB_pre_event", LCBPreEvent);
            registeredEvents.Add("LCB_post_event", LCBPostEvent);
            
            registeredEvents.Add("Carbon_pre_event", CarbonPreEvent);
            registeredEvents.Add("DB_count_pre_event", DB_countPreEvent);
            registeredEvents.Add("Hydroxyl_pre_event", HydroxylPreEvent);
            registeredEvents.Add("Hydroxyl_LCB_pre_event", HydroxylLCBPreEvent);
            registeredEvents.Add("Ether_pre_event", EtherPreEvent);
            registeredEvents.Add("mod_text_pre_event", mod_textPreEvent);
            
            registeredEvents.Add("GL_pre_event", GLPreEvent);
            registeredEvents.Add("PL_pre_event", PLPreEvent);
            registeredEvents.Add("PL_post_event", PLPostEvent);
            registeredEvents.Add("SL_pre_event", SLPreEvent);
            registeredEvents.Add("Cholesterol_pre_event", CholesterolPreEvent);
            registeredEvents.Add("Mediator_pre_event", MediatorPreEvent);
            
            registeredEvents.Add("HG_SGL_pre_event", HG_SGLPreEvent);
            registeredEvents.Add("HG_GL_pre_event", HG_GLPreEvent);
            
            registeredEvents.Add("HG_CL_pre_event", HG_CLPreEvent);
            registeredEvents.Add("HG_DPL_pre_event", HG_DPLPreEvent);
            registeredEvents.Add("HG_LPL_pre_event", HG_LPLPreEvent);
            registeredEvents.Add("HG_4PL_pre_event", HG_4PLPreEvent);
            
            registeredEvents.Add("HG_DSL_pre_event", HG_DSLPreEvent);
            registeredEvents.Add("SL_post_event", SLPostEvent);
            registeredEvents.Add("SphingoXine_pre_event", SphingoXinePreEvent);
            registeredEvents.Add("SphingoXine_post_event", SphingoXinePostEvent);
            registeredEvents.Add("SphingoXine_pure_pre_event", SphingoXine_purePreEvent);
            registeredEvents.Add("Sphingosine_name_pre_event", Sphingosine_namePreEvent);
            registeredEvents.Add("Sphinganine_name_pre_event", Sphinganine_namePreEvent);
            registeredEvents.Add("CType_pre_event", CTypePreEvent);
            
            
            
            registeredEvents.Add("Ch_pre_event", ChPreEvent);
            registeredEvents.Add("HG_ChE_pre_event", HG_ChEPreEvent);
            
            
            
            
            registeredEvents.Add("Mediator_post_event", MediatorPostEvent);
            registeredEvents.Add("Mediator_Number_pure_pre_event", MediatorAssemble);
            registeredEvents.Add("Mediator_Oxo_pre_event", Mediator_OxoPreEvent);
            registeredEvents.Add("Mediator_Name_separator_pre_event", MediatorAssemble);
            registeredEvents.Add("Mediator_separator_pre_event", MediatorAssemble);
            registeredEvents.Add("Mediator_Var_Name_pre_event", MediatorAssemble);
            registeredEvents.Add("Mediator_Const_pre_event", MediatorAssemble);
            
        }
        
        
        public void resetLipidBuilder(Parser.TreeNode node)
        {
            lipid = null;
            fagEnum = null;
            fag = null;
            mediatorName = "";
        }
        
        
        
        
        public void PureFAPreEvent(Parser.TreeNode node)
        {
            lipid = new UnsupportedLipid(lipidCreator);
        }
        
        
        
        
        public void MediatorAssemble(Parser.TreeNode node)
        {
            mediatorName += node.getText();
        }
        
        
        
        // handling all events
        public void lipidPostEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid) && lipid.headGroupNames.Count > 0 && lipidCreator.headgroups.ContainsKey(lipid.headGroupNames[0]))
            {
                lipid.adducts["+H"] = false;
                lipid.adducts["+2H"] = false;
                lipid.adducts["+NH4"] = false;
                lipid.adducts["-H"] = false;
                lipid.adducts["-2H"] = false;
                lipid.adducts["+HCOO"] = false;
                lipid.adducts["+CH3COO"] = false;
                
                lipid.adducts[lipidCreator.headgroups[lipid.headGroupNames[0]].defaultAdduct] = true;
            }
        }
        
        
        
        public void GLPreEvent(Parser.TreeNode node)
        {
            lipid = new GLLipid(lipidCreator);
            fagEnum = new FattyAcidGroupEnumerator((GLLipid)lipid);
        }
        
        
        
        public void PLPreEvent(Parser.TreeNode node)
        {
            lipid = new PLLipid(lipidCreator);
            fagEnum = new FattyAcidGroupEnumerator((PLLipid)lipid);
        }
        
        
        
        public void PLPostEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                if (lipid.headGroupNames.Count != 0)
                {
                    if (((PLLipid)lipid).fag1.faTypes["FAx"])
                    {
                        FattyAcidGroup fag = ((PLLipid)lipid).fag1;
                        ((PLLipid)lipid).fag1 = ((PLLipid)lipid).fag2;
                        ((PLLipid)lipid).fag2 = fag;
                    }
                
                    string hg = lipid.headGroupNames[0];
                    if (((PLLipid)lipid).fag2.faTypes["FAx"] && (new HashSet<string>{"PA", "PC", "PE", "PI", "PS"}).Contains(hg))
                    {
                        lipid.headGroupNames[0] = "L" + lipid.headGroupNames[0];
                    }
                    
                    if ((new HashSet<string>{"LPC", "PC", "LPE", "PE"}).Contains(hg))
                    {
                        if (((PLLipid)lipid).fag1.faTypes["FAa"] || ((PLLipid)lipid).fag2.faTypes["FAa"]) lipid.headGroupNames[0] += " O-a";
                        else if (((PLLipid)lipid).fag1.faTypes["FAp"] || ((PLLipid)lipid).fag2.faTypes["FAp"]) lipid.headGroupNames[0] += " O-p";
                    }
                 }
                else
                {
                    lipid = null;
                }
            }
        }
        
        
        public void SLPreEvent(Parser.TreeNode node)
        {
            lipid = new SLLipid(lipidCreator);
            ((SLLipid)lipid).lcb.hydroxylCounts.Clear();
            ((SLLipid)lipid).fag.hydroxylCounts.Clear();
            fagEnum = new FattyAcidGroupEnumerator((SLLipid)lipid);
        }
        
        
        
        public void CholesterolPreEvent(Parser.TreeNode node)
        {
            lipid = new Cholesterol(lipidCreator);
            fagEnum = new FattyAcidGroupEnumerator((Cholesterol)lipid);
        }
        
        
        
        public void MediatorPreEvent(Parser.TreeNode node)
        {
            lipid = new Mediator(lipidCreator);
        }
        
        
        
        
        public void LCBPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                fag = ((SLLipid)lipid).lcb;
            }
        }
        
        
        
        
        public void mod_textPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid) && fag != null)
            {
                if (node.getText().Equals("OH"))
                {
                    if (fag.hydroxylCounts.Count == 0)
                    {
                        fag.hydroxylCounts.Add(1);
                    }
                    else
                    {
                        int hydCnt = (new List<int>(fag.hydroxylCounts))[0];
                        fag.hydroxylCounts.Clear();
                        fag.hydroxylCounts.Add(hydCnt + 1);
                    }
                }
            }
        }
        
        
        
        public void LCBPostEvent(Parser.TreeNode node)
        {
            FALCBvalidationCheck();
        }
        
        
        
        
        public void FAPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                fag = (fagEnum != null && fagEnum.MoveNext()) ? fagEnum.Current : null;
            }
        }
        
        
        
        
        public void FAPostEvent(Parser.TreeNode node)
        {
            if ((fag != null) && fag.faTypes["FAp"])
            {
                int dbCnt = (new List<int>(fag.doubleBondCounts))[0];
                fag.doubleBondCounts.Clear();
                fag.doubleBondCounts.Add(dbCnt + 1);
            }
            FALCBvalidationCheck();
        }
        
        
        
        
        public void FALCBvalidationCheck()
        {
            // check if created fatty acid is valid
            if (lipid != null && !(lipid is UnsupportedLipid) && fag != null)
            {
                if (fag != null)
                {
                    if (fag.hydroxylCounts.Count == 0)
                    {
                        fag.hydroxylCounts.Add(0);
                    }
                
                    if (fag.carbonCounts.Count == 0 || fag.doubleBondCounts.Count == 0)
                    {
                        lipid = null;
                    }
                    else if (fag.carbonCounts.Count == 1 && fag.doubleBondCounts.Count == 1)
                    {
                        int carbonLength = (new List<int>(fag.carbonCounts))[0];
                        int doubleBondCount = (new List<int>(fag.doubleBondCounts))[0];
                        
                        int maxDoubleBond = Math.Max((carbonLength - 1) >> 1, 0);
                        if (doubleBondCount > maxDoubleBond)
                        {
                            lipid = null;
                        }
                        else if (fag.hydroxylCounts.Count == 1)
                        {
                            int hydroxylCount = (new List<int>(fag.hydroxylCounts))[0];
                            if (carbonLength < hydroxylCount) lipid = null;
                        }
                        
                        if (carbonLength == 0)
                        {
                            fag.faTypes["FA"] = false;
                            fag.faTypes["FAp"] = false;
                            fag.faTypes["FAa"] = false;
                            fag.faTypes["FAx"] = true;
                        }
                    }
                    else 
                    {
                        lipid = null;
                    }
                    
                    // check if at least one fatty acid type is enabled
                    int enablesFATypes = 0;
                    foreach(KeyValuePair<string, bool> kvp in fag.faTypes) enablesFATypes += kvp.Value ? 1 : 0;                
                    if (enablesFATypes == 0)
                    {
                        lipid = null;
                    }
                }
                else 
                {
                    lipid = null;
                }
            }
            
            if (lipid != null && !(lipid is UnsupportedLipid) && fag != null)
            {
                foreach(int l in fag.carbonCounts) fag.lengthInfo = Convert.ToString(l);
                foreach(int db in fag.doubleBondCounts) fag.dbInfo = Convert.ToString(db);
                foreach(int h in fag.hydroxylCounts) fag.hydroxylInfo = Convert.ToString(h);
            }
        }
        
        
        
        
        public void CarbonPreEvent(Parser.TreeNode node)
        {
            if (fag != null)
            {
                string carbonCount = node.getText();
                int carbonCountInt = Convert.ToInt32(carbonCount);
                if (0 <= carbonCountInt && carbonCountInt <= 30) fag.carbonCounts.Add(carbonCountInt);
                else fag = null;
            }
        }
        
        
        
        
        public void DB_countPreEvent(Parser.TreeNode node)
        {
            if (fag != null)
            {
                string doubleBondCount = node.getText();
                int doubleBondCountInt = Convert.ToInt32(doubleBondCount);
                if (0 <= doubleBondCountInt && doubleBondCountInt <= 6) fag.doubleBondCounts.Add(doubleBondCountInt);
                else fag = null;
            }
        }
        
        
        
        
        public void HydroxylPreEvent(Parser.TreeNode node)
        {
            if (fag != null)
            {
                string hydroxylCount = node.getText();
                int hydroxylCountInt = Convert.ToInt32(hydroxylCount);
                if (fag.isLCB && 2 <= hydroxylCountInt && hydroxylCountInt <= 3) fag.hydroxylCounts.Add(hydroxylCountInt);
                else if ((lipid is SLLipid) && !fag.isLCB && 0 <= hydroxylCountInt && hydroxylCountInt <= 3) fag.hydroxylCounts.Add(hydroxylCountInt);
                else if (!(lipid is SLLipid) && 0 <= hydroxylCountInt && hydroxylCountInt <= 6) fag.hydroxylCounts.Add(hydroxylCountInt);
                else fag = null;
            }
        }
        
        
        
        
        
        public void HydroxylLCBPreEvent(Parser.TreeNode node)
        {
            if (fag != null)
            {
                string hydroxylCount = node.getText();
                int hydroxylCountInt = 0; 
                
                if (hydroxylCount == "m"){
                    lipid = new UnsupportedLipid(lipidCreator);
                }
                else {
                    if (hydroxylCount == "d") hydroxylCountInt = 2;
                    else if (hydroxylCount == "t") hydroxylCountInt = 3;
                    if (fag.isLCB && 2 <= hydroxylCountInt && hydroxylCountInt <= 3) fag.hydroxylCounts.Add(hydroxylCountInt);
                    else if ((lipid is SLLipid) && !fag.isLCB && 0 <= hydroxylCountInt && hydroxylCountInt <= 3) fag.hydroxylCounts.Add(hydroxylCountInt);
                    else if (!(lipid is SLLipid) && 0 <= hydroxylCountInt && hydroxylCountInt <= 6) fag.hydroxylCounts.Add(hydroxylCountInt);
                    else fag = null;
                }
            }
        }
        
        
        
        
        
        
        public void EtherPreEvent(Parser.TreeNode node)
        {
            if (fag != null)
            {
                List<string> keys = new List<string>(fag.faTypes.Keys);
                foreach(string faTypeKey in keys) fag.faTypes[faTypeKey] = false;
            
                string faType = node.getText();
                if (faType == "O-") faType = "a";
                else if (faType == "P-") faType = "p";
                fag.faTypes["FA" + faType] = true;
            }
        }
        
        
        
        
        public void HG_SGLPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                string headgroup = node.getText();
                if (headgroup != "SQMG"){
                    lipid.headGroupNames.Add(headgroup);
                    List<string> keys = new List<string>(((GLLipid)lipid).fag3.faTypes.Keys);
                    foreach(string faTypeKey in keys) ((GLLipid)lipid).fag3.faTypes[faTypeKey] = false;
                    ((GLLipid)lipid).fag3.faTypes["FAx"] = true;
                    if (headgroup != "DG") ((GLLipid)lipid).containsSugar = true;
                }
                else
                {
                    lipid = new UnsupportedLipid(lipidCreator);
                }
            }
        }
        
        
        
        
        public void SphingoXinePostEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                FALCBvalidationCheck();
            }
        }
        
        
        
        
        public void SphingoXinePreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                ((SLLipid)lipid).isLyso = true;
                fag = ((SLLipid)lipid).lcb;
                fag.hydroxylCounts.Add(2);
            }
        }
        
        
        
        
        public void SphingoXine_purePreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid) && fag != null)
            {
                fag.carbonCounts.Add(18);
            }
        }
        
        
        
        public void Sphingosine_namePreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid) && fag != null)
            {
                fag.doubleBondCounts.Add(1);
                string headgroup = node.getText();
                if (headgroup.Equals("Sphingosine")) headgroup = "LCB";
                else if (headgroup.Equals("Sphingosine-1-phosphate")) headgroup = "LCBP";
                lipid.headGroupNames.Add(headgroup);
            }
        }
        
        
        
        
        public void Sphinganine_namePreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid) && fag != null)
            {
                fag.doubleBondCounts.Add(0);
                string headgroup = node.getText();
                if (headgroup.Equals("Sphinganine")) headgroup = "LCB";
                else if (headgroup.Equals("Sphinganine-1-phosphate")) headgroup = "LCBP";
                lipid.headGroupNames.Add(headgroup);
            }
        }
        
        
        
        
        
        public void CTypePreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid) && fag != null)
            {
                string carbonCount = node.right.getText(); // omit the C e.g. for C16
                int carbonCountInt = Convert.ToInt32(carbonCount);
                if (0 <= carbonCountInt && carbonCountInt <= 30) fag.carbonCounts.Add(carbonCountInt);
                else fag = null;
            }
        }
        
        
        
        
        
        public void HG_GLPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                string headgroup = node.getText();
                lipid.headGroupNames.Add(headgroup);
            }
        }
        
        
        
        
        public void HG_CLPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                string headgroup = node.getText();
                lipid.headGroupNames.Add(headgroup);
                ((PLLipid)lipid).isCL = true;
            }
        }
        
        
        
        
        
        public void HG_DPLPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                string headgroup = node.getText();
                if ((new HashSet<string>{"PIM1", "PIM2", "PIM3", "PIM4", "PIM5", "PIM6", "Glc-DG", "PGP", "PE-NMe2", "AC2SGL", "DAT", "PE-NMe", "PT", "Glc-GP", "NAPE"}).Contains(headgroup))
                {
                    lipid = new UnsupportedLipid(lipidCreator);
                }
                else {
                    if ("CDP-DG".Equals(headgroup)) headgroup = "CDPDAG";
                    else if ("LBPA".Equals(headgroup)) headgroup = "BMP";
                    lipid.headGroupNames.Add(headgroup);
                }
            }
        }
        
        
        
        
        
        public void HG_LPLPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                string headgroup = node.getText();
                if ((new HashSet<string>{"LPIM1", "LPIM2", "LPIM3", "LPIM4", "LPIM5", "LPIM6", "CPA"}).Contains(headgroup))
                {
                    lipid = new UnsupportedLipid(lipidCreator);
                }
                else
                {
                    if ("LysoPC".Equals(headgroup)) headgroup = "LPC";
                    else if ("LysoPE".Equals(headgroup)) headgroup = "LPE";
                    lipid.headGroupNames.Add(headgroup);
                    ((PLLipid)lipid).isLyso = true;
                }
            }
        }
        
        
        
        
        public void HG_4PLPreEvent(Parser.TreeNode node)
        {
            lipid = new UnsupportedLipid(lipidCreator);
        }
        
        
        
        
        
        public void HG_DSLPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                string headgroup = node.getText();
                if ((new HashSet<string>{"FMC-5", "FMC-6"}).Contains(headgroup))
                {
                    lipid = new UnsupportedLipid(lipidCreator);
                }
                else
                {
                    if (headgroup.Equals("PE-Cer")) headgroup = "EPC";
                    else if (headgroup.Equals("PI-Cer")) headgroup = "IPC";
                    else if (headgroup.Equals("LacCer")) headgroup = "Hex2Cer";
                    else if (headgroup.Equals("GalCer")) headgroup = "HexCer";
                    else if (headgroup.Equals("GlcCer")) headgroup = "HexCer";
                    else if (headgroup.Equals("(3'-sulfo)Galbeta-Cer")) headgroup = "SHexCer";
                    lipid.headGroupNames.Add(headgroup);
                }
            }
        }
        
        
        
        
        public void ChPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                lipid.headGroupNames.Add("Ch");
                List<string> keys = new List<string>(((Cholesterol)lipid).fag.faTypes.Keys);
                foreach(string faTypeKey in keys) ((Cholesterol)lipid).fag.faTypes[faTypeKey] = false;
                ((Cholesterol)lipid).fag.faTypes["FAx"] = true;
            }
        }
        
        
        
        
        public void HG_ChEPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                lipid.headGroupNames.Add("ChE");
                ((Cholesterol)lipid).containsEster = true;
            }
        }
        
        
        
        
        
        public void SLPostEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                if (lipid.headGroupNames.Count == 0)
                {
                    lipid = null;
                }
            }
        }
            
            
        public void Mediator_OxoPreEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                mediatorName += "Oxo";
            }
        }
            
            
        public void MediatorPostEvent(Parser.TreeNode node)
        {
            if (lipid != null && !(lipid is UnsupportedLipid))
            {
                if (mediatorName.Equals("Arachidonic acid")) mediatorName = "AA";
                else if (mediatorName.Equals("Arachidonic Acid")) mediatorName = "AA";
            
                if ((new HashSet<string>{"10-HDoHE", "11-HDoHE", "11-HETE", "11,12-DHET", "11(12)-EET", "12-HEPE", "12-HETE", "12-HHTrE", "12-OxoETE", "12(13)-EpOME", "13-HODE", "13-HOTrE", "14,15-DHET", "14(15)-EET", "14(15)-EpETE", "15-HEPE", "15-HETE", "15d-PGJ2", "16-HDoHE", "16-HETE", "18-HEPE", "5-HEPE", "5-HETE", "5-HpETE", "5-OxoETE", "5,12-DiHETE", "5,6-DiHETE", "5,6,15-LXA4", "5(6)-EET", "8-HDoHE", "8-HETE", "8,9-DHET", "8(9)-EET", "9-HEPE", "9-HETE", "9-HODE", "9-HOTrE", "9(10)-EpOME", "AA", "alpha-LA", "DHA", "EPA", "Linoleic acid", "LTB4", "LTC4", "LTD4", "Maresin 1", "Palmitic acid", "PGB2", "PGD2", "PGE2", "PGF2alpha", "PGI2", "Resolvin D1", "Resolvin D2", "Resolvin D3", "Resolvin D5", "tetranor-12-HETE", "TXB1", "TXB2", "TXB3"}).Contains(mediatorName))
                {
                    lipid.headGroupNames.Add(mediatorName);
                }
                else
                {
                    lipid = new UnsupportedLipid(lipidCreator);
                }
            }
        }
    }    
}