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
        public Mediator(Dictionary<String, Precursor> headgroups, Dictionary<int, Dictionary<String, ArrayList>> allFragments)
        {
            if (allFragments.ContainsKey((int)LipidCategory.Mediator))
            {
                foreach (KeyValuePair<String, ArrayList> PLFragments in allFragments[(int)LipidCategory.Mediator])
                {
                    if (headgroups.ContainsKey(PLFragments.Key)) pathsToFullImage.Add(PLFragments.Key, headgroups[PLFragments.Key].pathToImage);
                    MS2Fragments.Add(PLFragments.Key, new ArrayList());
                    bool containsDeuterium = PLFragments.Key.IndexOf("/") > -1;
                    foreach (MS2Fragment fragment in PLFragments.Value)
                    {
                        MS2Fragment tmp = new MS2Fragment(fragment);
                        MS2Fragments[PLFragments.Key].Add(tmp);
                        if (containsDeuterium) tmp.fragmentSelected = false;
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
                
                if (!usedKeys.Contains(key))
                {
                    foreach (KeyValuePair<string, bool> adduct in adducts)
                    {
                        if (adduct.Value && headgroups[headgroup].adductRestrictions[adduct.Key])
                        {
                            usedKeys.Add(key);
                            
                            DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                            MS2Fragment.addCounts(atomsCount, headgroups[headgroup].elements);
                            String chemForm = LipidCreator.computeChemicalFormula(atomsCount);
                            int charge = getChargeAndAddAdduct(atomsCount, adduct.Key);
                            double mass = LipidCreator.computeMass(atomsCount, charge);
                                                                

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
                            precursorData.MS2Fragments = MS2Fragments[headgroup];
                            
                            precursorDataList.Add(precursorData);
                            /*
                            foreach (Precursor heavyHeadgroup  in headgroups[headgroup].heavyLabeledPrecursors)
                            {
                                string derivativeHeadgroup = heavyHeadgroup.name;
                                if (headgroups[derivativeHeadgroup].adductRestrictions[adduct.Key])
                                {
                                    usedKeys.Add(key);
                        
                                    DataTable atomsCountDeuterium = MS2Fragment.createEmptyElementTable();
                                    MS2Fragment.addCounts(atomsCountDeuterium, headgroups[derivativeHeadgroup].elements);
                                    String chemFormDeuterium = LipidCreator.computeChemicalFormula(atomsCountDeuterium);
                                    int chargeDeuterium = getChargeAndAddAdduct(atomsCountDeuterium, adduct.Key);
                                    double massDeuterium = LipidCreator.computeMass(atomsCountDeuterium, chargeDeuterium);
                                                                        

                                    PrecursorData precursorDataDeuterium = new PrecursorData();
                                    precursorDataDeuterium.lipidCategory = LipidCategory.Mediator;
                                    precursorDataDeuterium.moleculeListName = derivativeHeadgroup;
                                    precursorDataDeuterium.precursorName = derivativeHeadgroup;
                                    precursorDataDeuterium.precursorIonFormula = chemFormDeuterium;
                                    precursorDataDeuterium.precursorAdduct = "[M" + adduct.Key + "]";
                                    precursorDataDeuterium.precursorM_Z = massDeuterium / (double)(Math.Abs(chargeDeuterium));
                                    precursorDataDeuterium.precursorCharge = chargeDeuterium;
                                    precursorDataDeuterium.adduct = adduct.Key;
                                    precursorDataDeuterium.atomsCount = atomsCountDeuterium;
                                    precursorDataDeuterium.fa1 = null;
                                    precursorDataDeuterium.fa2 = null;
                                    precursorDataDeuterium.fa3 = null;
                                    precursorDataDeuterium.fa4 = null;
                                    precursorDataDeuterium.lcb = null;
                                    precursorDataDeuterium.MS2Fragments = MS2Fragments[derivativeHeadgroup];
                                    
                                    precursorDataList.Add(precursorDataDeuterium);
                                }
                            }
                            */
                        }
                    }
                }
            }
        }
    }
}