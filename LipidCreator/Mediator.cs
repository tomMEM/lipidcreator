/*
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
using System.Data.SQLite;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;


namespace LipidCreator
{   
    [Serializable]
    public class Mediator : Lipid
    {
        public Mediator(Dictionary<String, String> allPaths, Dictionary<String, Dictionary<String, ArrayList>> allFragments)
        {
            if (allFragments.ContainsKey("Mediator"))
            {
                foreach (KeyValuePair<String, ArrayList> PLFragments in allFragments["Mediator"])
                {
                    if (allPaths.ContainsKey(PLFragments.Key)) pathsToFullImage.Add(PLFragments.Key, allPaths[PLFragments.Key]);
                    MS2Fragments.Add(PLFragments.Key, new ArrayList());
                    foreach (MS2Fragment fragment in PLFragments.Value)
                    {
                        MS2Fragments[PLFragments.Key].Add(new MS2Fragment(fragment));
                    }
                }
            }
        }
    
        public Mediator(Mediator copy) : base((Lipid)copy) 
        {
            
        }
        
        
        public override string serialize()
        {
            string xml = "<lipid type=\"Mediator\">\n";
            foreach (string headgroup in headGroupNames)
            {
                xml += "<headGroup>" + headgroup + "</headGroup>\n";
            }
            xml += "</lipid>\n";
            return xml;
        }
        
        
        public override void import(XElement node)
        {
            foreach (XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                        
                    case "headGroup":
                        headGroupNames.Add(child.Value.ToString());
                        break;                     
                        
                    default:
                        base.import(child);
                        break;
                }
            }
        }
        
        
        public override void computePrecursorData(Dictionary<String, DataTable> headGroupsTable, Dictionary<String, Dictionary<String, bool>> headgroupAdductRestrictions, HashSet<String> usedKeys, ArrayList precursorDataList)
        {
            
            foreach(string headgroupIter in headGroupNames)
            {   
                string headgroup = headgroupIter;                
                String key = headgroup;
                
                if (!usedKeys.Contains(key))
                {
                    foreach (KeyValuePair<string, bool> adduct in adducts)
                    {
                        if (adduct.Value && headgroupAdductRestrictions[headgroup][adduct.Key])
                        {
                            usedKeys.Add(key);
                            
                            DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                            MS2Fragment.addCounts(atomsCount, headGroupsTable[headgroup]);
                            String chemForm = LipidCreatorForm.computeChemicalFormula(atomsCount);
                            int charge = getChargeAndAddAdduct(atomsCount, adduct.Key);
                            String chemFormComplete = LipidCreatorForm.computeChemicalFormula(atomsCount);
                            double mass = LipidCreatorForm.computeMass(atomsCount, charge);
                                                                

                            PrecursorData precursorData = new PrecursorData();
                            precursorData.lipidCategory = LipidCategory.Mediator;
                            precursorData.moleculeListName = headgroup;
                            precursorData.precursorName = key;
                            precursorData.precursorIonFormula = chemForm;
                            precursorData.precursorAdduct = "[M" + adduct.Key + "]";
                            precursorData.precursorM_Z = mass / (double)(Math.Abs(charge));
                            precursorData.precursorCharge = charge;
                            precursorData.adduct = adduct.Key;
                            precursorData.atomsCount = atomsCount;
                            precursorData.fa1 = null;
                            precursorData.fa2 = null;
                            precursorData.fa3 = null;
                            precursorData.fa4 = null;
                            precursorData.lcb = null;
                            precursorData.chemFormComplete = chemFormComplete;
                            precursorData.MS2Fragments = MS2Fragments[headgroup];
                            
                            precursorDataList.Add(precursorData);
                        }
                    }
                }
            }
        }
    }
}