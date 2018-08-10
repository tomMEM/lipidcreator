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
using System.Data.SQLite;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace LipidCreator
{   
    [Serializable]
    public class Mediator : Lipid
    { 
        public Mediator(LipidCreator lipidCreator) : base(lipidCreator, LipidCategory.Mediator)
        {
        }
    
        public Mediator(Mediator copy) : base((Lipid)copy) 
        {
            
        }
        
        
        
        
        public override ArrayList getFattyAcidGroupList()
        {
            return new ArrayList();
        }
        
        
        public override string serialize()
        {
            string xml = "<lipid type=\"Mediator\">\n";
            foreach (string headgroup in headGroupNames)
            {
                xml += "<headGroup>" + headgroup + "</headGroup>\n";
            }
            xml += base.serialize();
            xml += "</lipid>\n";
            return xml;
        }
        
        
        public override void import(XElement node, string importVersion)
        {
            foreach (XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                        
                    case "headGroup":
                        headGroupNames.Add(child.Value.ToString());
                        break;                     
                        
                    default:
                        base.import(child, importVersion);
                        break;
                }
            }
        }
        
        // synchronize the fragment list with list from LipidCreator root
        public override void Update(object sender, EventArgs e)
        {
            Updating((int)LipidCategory.Mediator);
        }
        
        
        public override void computePrecursorData(Dictionary<String, Precursor> headgroups, HashSet<String> usedKeys, ArrayList precursorDataList)
        {
            ArrayList allHeadgroups = new ArrayList();
            foreach(string headgroup in headGroupNames)
            {
                allHeadgroups.Add(headgroup);
                foreach(Precursor precursor in headgroups[headgroup].heavyLabeledPrecursors)
                {
                    allHeadgroups.Add(precursor.name);
                }
            }
            
            foreach(string headgroupIter in allHeadgroups)
            {   
                string headgroup = headgroupIter;                
                String key = headgroup;
                
                if (headgroup.IndexOf("/") > -1 && onlyHeavyLabeled == 0) continue;
                else if (headgroup.IndexOf("/") == -1 && onlyHeavyLabeled == 1) continue;
                
                
                foreach (KeyValuePair<string, bool> adduct in adducts)
                {
                
                    if (!adduct.Value || !headgroups[headgroup].adductRestrictions[adduct.Key]) continue;
                    if (usedKeys.Contains(key + adduct.Key)) continue;
                    
                    usedKeys.Add(key + adduct.Key);
                    
                    Dictionary<int, int> atomsCount = MS2Fragment.createEmptyElementDict();
                    MS2Fragment.addCounts(atomsCount, headgroups[headgroup].elements);
                    string chemForm = LipidCreator.computeChemicalFormula(atomsCount);
                    string adductForm = LipidCreator.computeAdductFormula(atomsCount, adduct.Key);
                    int charge = getChargeAndAddAdduct(atomsCount, adduct.Key);
                    double mass = LipidCreator.computeMass(atomsCount, charge);
                                                        

                    PrecursorData precursorData = new PrecursorData();
                    precursorData.lipidCategory = LipidCategory.Mediator;
                    precursorData.fullMoleculeListName = headgroup;
                    precursorData.moleculeListName = headgroup.Split(new Char[]{'/'})[0];
                    precursorData.lipidClass = headgroup;
                    precursorData.precursorName = headgroup.Split(new Char[]{'/'})[0];
                    precursorData.precursorIonFormula = chemForm;
                    precursorData.precursorAdduct = adduct.Key;
                    precursorData.precursorAdductFormula = adductForm;
                    precursorData.precursorM_Z = mass / (double)(Math.Abs(charge));
                    precursorData.precursorCharge = charge;
                    precursorData.atomsCount = headgroups[headgroup].elements;
                    precursorData.fa1 = null;
                    precursorData.fa2 = null;
                    precursorData.fa3 = null;
                    precursorData.fa4 = null;
                    precursorData.lcb = null;
                    precursorData.addPrecursor = (onlyPrecursors != 0);
                    precursorData.fragmentNames = (onlyPrecursors != 1) ? ((charge > 0) ? positiveFragments[headgroup] : negativeFragments[headgroup]) : new HashSet<string>();
                    
                    precursorDataList.Add(precursorData);
                }
            }
        }
    }
}