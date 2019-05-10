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
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using log4net;


namespace LipidCreator
{   
    [Serializable]
    public class Cholesterol : Lipid
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Cholesterol));
        public bool containsEster;
        public FattyAcidGroup fag;
    
    
        public Cholesterol(LipidCreator lipidCreator) : base(lipidCreator, LipidCategory.Sterollipid)
        {
            fag = new FattyAcidGroup();
            containsEster = false;
            adducts["+NH4"] = true;
            adducts["-H"] = false;
        }
    
        public Cholesterol(Cholesterol copy) : base((Lipid)copy) 
        {
            fag = new FattyAcidGroup(copy.fag);
            containsEster = copy.containsEster;
        }
        
        
        
        public override ArrayList getFattyAcidGroupList()
        {
            return new ArrayList{fag};
        }
        
        
        public override string serialize()
        {
            string xml = "<lipid type=\"Cholesterol\" containsEster=\"" + containsEster + "\">\n";
            xml += fag.serialize();
            xml += base.serialize();
            xml += "</lipid>\n";
            return xml;
        }
        
        // synchronize the fragment list with list from LipidCreator root
        public override void Update(object sender, EventArgs e)
        {
            Updating((int)LipidCategory.Sterollipid);
        }
        
        
        public override void import(XElement node, string importVersion)
        {
            int fattyAcidCounter = 0;
            containsEster = node.Attribute("containsEster").Value == "True";
            foreach (XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "FattyAcidGroup":
                        if (fattyAcidCounter == 0)
                        {
                            fag.import(child, importVersion);
                        }
                        else
                        {   
                            log.Error("A Cholesterol can have at most 1 fatty acid chains. Found: " + (fattyAcidCounter + 1) + "");
                            throw new Exception();
                        }
                        ++fattyAcidCounter;
                        break;                        
                        
                    default:
                        base.import(child, importVersion);
                        break;
                }
            }
        }
        
        
        public override void computePrecursorData(IDictionary<String, Precursor> headgroups, HashSet<String> usedKeys, ArrayList precursorDataList)
        {
            if (containsEster)
            {   
                foreach (FattyAcid fa in fag.getFattyAcids())
                {
                    
                    String headgroup = "ChE";
                    String key = " ";
                    key += Convert.ToString(fa.length) + ":" + Convert.ToString(fa.db);
                    if (fa.hydroxyl > 0) key += ";" + Convert.ToString(fa.hydroxyl);
                    key += fa.suffix;
                    
                    foreach (KeyValuePair<string, bool> adduct in adducts)
                    {
                        if (!adduct.Value || !headgroups[headgroup].adductRestrictions[adduct.Key]) continue;
                        if (usedKeys.Contains(headgroup + key + adduct.Key)) continue;
                        
                        usedKeys.Add(headgroup + key + adduct.Key);
                        
                        ElementDictionary atomsCount = MS2Fragment.createEmptyElementDict();
                        MS2Fragment.addCounts(atomsCount, fa.atomsCount);
                        MS2Fragment.addCounts(atomsCount, headgroups[headgroup].elements);
                        string chemForm = LipidCreator.computeChemicalFormula(atomsCount);
                        string adductForm = LipidCreator.computeAdductFormula(atomsCount, adduct.Key);
                        int charge = getChargeAndAddAdduct(atomsCount, adduct.Key);
                        double mass = LipidCreator.computeMass(atomsCount, charge);
                    
                        PrecursorData precursorData = new PrecursorData();
                        precursorData.lipidCategory = LipidCategory.Sterollipid;
                        precursorData.moleculeListName = headgroup;
                        precursorData.fullMoleculeListName = headgroup;
                        precursorData.precursorExportName = headgroup + key;
                        precursorData.precursorName = headgroup + key;
                        precursorData.precursorIonFormula = chemForm;
                        precursorData.precursorAdduct = adduct.Key;
                        precursorData.precursorAdductFormula = adductForm;
                        precursorData.precursorM_Z = mass / (double)(Math.Abs(charge));
                        precursorData.precursorCharge = charge;
                        precursorData.atomsCount = headgroups[headgroup].elements;
                        precursorData.fa1 = fa;
                        precursorData.fa2 = null;
                        precursorData.fa3 = null;
                        precursorData.fa4 = null;
                        precursorData.lcb = null;
                        precursorData.addPrecursor = (onlyPrecursors != 0);
                        precursorData.fragmentNames = (onlyPrecursors != 1) ? ((charge > 0) ? positiveFragments[headgroup] : negativeFragments[headgroup]) : new HashSet<string>();
                        
                        if (onlyHeavyLabeled != 1) precursorDataList.Add(precursorData);
                                
                        if (onlyHeavyLabeled == 0) continue;
                        foreach (Precursor heavyPrecursor  in headgroups[headgroup].heavyLabeledPrecursors)
                        {
                            string heavyHeadgroup = heavyPrecursor.name;
                            
                            if (!headgroups[heavyHeadgroup].adductRestrictions[adduct.Key]) continue;
                            
                            
                            FattyAcid heavyFA1 = new FattyAcid(fa);
                            heavyFA1.updateForHeavyLabeled((ElementDictionary)heavyPrecursor.userDefinedFattyAcids[0]);
                
                            ElementDictionary heavyAtomsCount = MS2Fragment.createEmptyElementDict();
                            MS2Fragment.addCounts(heavyAtomsCount, heavyFA1.atomsCount);
                            MS2Fragment.addCounts(heavyAtomsCount, headgroups[heavyHeadgroup].elements);
                            string heavyChemForm = LipidCreator.computeChemicalFormula(heavyAtomsCount);
                            string heavyAdductForm = LipidCreator.computeAdductFormula(heavyAtomsCount, adduct.Key);
                            int heavyCharge = getChargeAndAddAdduct(heavyAtomsCount, adduct.Key);
                            double heavyMass = LipidCreator.computeMass(heavyAtomsCount, heavyCharge);
                            
                            
                            string heavyKey = LipidCreator.precursorNameSplit(heavyHeadgroup)[0] + LipidCreator.computeHeavyIsotopeLabel(heavyAtomsCount);
                                                                

                            PrecursorData heavyPrecursorData = new PrecursorData();
                            heavyPrecursorData.lipidCategory = LipidCategory.Sterollipid;
                            heavyPrecursorData.moleculeListName = headgroup;
                            heavyPrecursorData.fullMoleculeListName = heavyHeadgroup;
                            heavyPrecursorData.precursorExportName = headgroup + key;
                            heavyPrecursorData.precursorName = heavyKey + key;
                            heavyPrecursorData.precursorIonFormula = heavyChemForm;
                            heavyPrecursorData.precursorAdduct = adduct.Key;
                            heavyPrecursorData.precursorAdductFormula = heavyAdductForm;
                            heavyPrecursorData.precursorM_Z = heavyMass / (double)(Math.Abs(heavyCharge));
                            heavyPrecursorData.precursorCharge = heavyCharge;
                            heavyPrecursorData.atomsCount = headgroups[heavyHeadgroup].elements;
                            heavyPrecursorData.fa1 = heavyFA1;
                            heavyPrecursorData.fa2 = null;
                            heavyPrecursorData.fa3 = null;
                            heavyPrecursorData.fa4 = null;
                            heavyPrecursorData.lcb = null;
                            heavyPrecursorData.addPrecursor = (onlyPrecursors != 0);
                            heavyPrecursorData.fragmentNames = (onlyPrecursors != 1) ? ((heavyCharge > 0) ? positiveFragments[heavyHeadgroup] : negativeFragments[heavyHeadgroup]) : new HashSet<string>();
                            
                            precursorDataList.Add(heavyPrecursorData);
                        }
                    }
                }
            }
            else
            {
                String headgroup = "Ch";
                String key = headgroup;
                
                foreach (KeyValuePair<string, bool> adduct in adducts)
                {
                    if (!adduct.Value || !headgroups[headgroup].adductRestrictions[adduct.Key]) continue;
                    if (usedKeys.Contains(key + adduct.Key)) continue;
                    
                    usedKeys.Add(key + adduct.Key);
                    
                    ElementDictionary atomsCount = MS2Fragment.createEmptyElementDict();
                    MS2Fragment.addCounts(atomsCount, headgroups[headgroup].elements);
                    string chemForm = LipidCreator.computeChemicalFormula(atomsCount);
                    string adductForm = LipidCreator.computeAdductFormula(atomsCount, adduct.Key);
                    int charge = getChargeAndAddAdduct(atomsCount, adduct.Key);
                    double mass = LipidCreator.computeMass(atomsCount, charge);
                                    
                    PrecursorData precursorData = new PrecursorData();
                    precursorData.lipidCategory = LipidCategory.Sterollipid;
                    precursorData.moleculeListName = headgroup;
                    precursorData.fullMoleculeListName = headgroup;
                    precursorData.precursorExportName = headgroup;
                    precursorData.precursorName = key;
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
                    
                    if (onlyHeavyLabeled != 1) precursorDataList.Add(precursorData);
                                
                    if (onlyHeavyLabeled == 0) continue;
                    foreach (Precursor heavyPrecursor  in headgroups[headgroup].heavyLabeledPrecursors)
                    {
                        string heavyHeadgroup = heavyPrecursor.name;
                        
                        if (!headgroups[heavyHeadgroup].adductRestrictions[adduct.Key]) continue;
            
                        ElementDictionary heavyAtomsCount = MS2Fragment.createEmptyElementDict();
                        MS2Fragment.addCounts(heavyAtomsCount, headgroups[heavyHeadgroup].elements);
                        string heavyChemForm = LipidCreator.computeChemicalFormula(heavyAtomsCount);
                        string heavyAdductForm = LipidCreator.computeAdductFormula(heavyAtomsCount, adduct.Key);
                        int heavyCharge = getChargeAndAddAdduct(heavyAtomsCount, adduct.Key);
                        double heavyMass = LipidCreator.computeMass(heavyAtomsCount, heavyCharge);
                                    
                        
                        string heavyKey = LipidCreator.precursorNameSplit(heavyHeadgroup)[0] + LipidCreator.computeHeavyIsotopeLabel(heavyAtomsCount);
                                                            

                        PrecursorData heavyPrecursorData = new PrecursorData();
                        heavyPrecursorData.lipidCategory = LipidCategory.Sterollipid;
                        heavyPrecursorData.moleculeListName = headgroup;
                        heavyPrecursorData.fullMoleculeListName = heavyHeadgroup;
                        heavyPrecursorData.precursorExportName = headgroup;
                        heavyPrecursorData.precursorName = heavyKey;
                        heavyPrecursorData.precursorIonFormula = heavyChemForm;
                        heavyPrecursorData.precursorAdduct = adduct.Key;
                        heavyPrecursorData.precursorAdductFormula = heavyAdductForm;
                        heavyPrecursorData.precursorM_Z = heavyMass / (double)(Math.Abs(heavyCharge));
                        heavyPrecursorData.precursorCharge = heavyCharge;
                        heavyPrecursorData.atomsCount = headgroups[heavyHeadgroup].elements;
                        heavyPrecursorData.fa1 = null;
                        heavyPrecursorData.fa2 = null;
                        heavyPrecursorData.fa3 = null;
                        heavyPrecursorData.fa4 = null;
                        heavyPrecursorData.lcb = null;
                        heavyPrecursorData.addPrecursor = (onlyPrecursors != 0);
                        heavyPrecursorData.fragmentNames = (onlyPrecursors != 1) ? ((heavyCharge > 0) ? positiveFragments[heavyHeadgroup] : negativeFragments[heavyHeadgroup]) : new HashSet<string>();
                        
                        precursorDataList.Add(heavyPrecursorData);
                    }
                }
            }
        }
    }
}