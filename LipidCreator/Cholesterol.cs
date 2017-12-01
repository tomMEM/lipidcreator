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
    public class Cholesterol : Lipid
    {
        public bool containsEster;
        public FattyAcidGroup fag;
    
    
        public Cholesterol(LipidCreator lipidCreator)
        {
            fag = new FattyAcidGroup();
            containsEster = false;
            
            Dictionary<String, ArrayList> allFragments = lipidCreator.allFragments;
            Dictionary<int, ArrayList> categoryToClass = lipidCreator.categoryToClass;
            
            if (categoryToClass.ContainsKey((int)LipidCategory.Cholesterol))
            {
                foreach (String lipidClass in categoryToClass[(int)LipidCategory.Cholesterol])
                {
                    MS2Fragments.Add(lipidClass, new ArrayList());
                    foreach (MS2Fragment fragment in allFragments[lipidClass])
                    {
                        MS2Fragments[lipidClass].Add(new MS2Fragment(fragment));
                    }
                }
            }
            
            adducts["+NH4"] = true;
            adducts["-H"] = false;
        }
    
        public Cholesterol(Cholesterol copy) : base((Lipid)copy) 
        {
            fag = new FattyAcidGroup(copy.fag);
            containsEster = copy.containsEster;
        }
        
        
        public override string serialize()
        {
            string xml = "<lipid type=\"Cholesterol\" containsEster=\"" + containsEster + "\">\n";
            xml += fag.serialize();
            xml += base.serialize();
            xml += "</lipid>\n";
            return xml;
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
                            Console.WriteLine("Error, fatty acid");
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
        
        
        public override void computePrecursorData(Dictionary<String, Precursor> headgroups, HashSet<String> usedKeys, ArrayList precursorDataList)
        {
            if (containsEster)
            {   
                foreach (FattyAcid fa in fag.getFattyAcids())
                {
                    
                    String headgroup = "ChE";
                    String key = headgroup + " ";
                    key += Convert.ToString(fa.length) + ":" + Convert.ToString(fa.db);
                    if (fa.hydroxyl > 0) key += ";" + Convert.ToString(fa.hydroxyl);
                    key += fa.suffix;
                    if (!usedKeys.Contains(key))
                    {
                        foreach (KeyValuePair<string, bool> adduct in adducts)
                        {
                            if (adduct.Value && headgroups[headgroup].adductRestrictions[adduct.Key])
                            {
                                usedKeys.Add(key);
                                
                                DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                                MS2Fragment.addCounts(atomsCount, fa.atomsCount);
                                MS2Fragment.addCounts(atomsCount, headgroups[headgroup].elements);
                                String chemForm = LipidCreator.computeChemicalFormula(atomsCount);
                                int charge = getChargeAndAddAdduct(atomsCount, adduct.Key);
                                double mass = LipidCreator.computeMass(atomsCount, charge);
                            
                                PrecursorData precursorData = new PrecursorData();
                                precursorData.lipidCategory = LipidCategory.Cholesterol;
                                precursorData.moleculeListName = headgroup;
                                precursorData.precursorName = key;
                                precursorData.precursorIonFormula = chemForm;
                                precursorData.precursorAdduct = "[M" + adduct.Key + "]";
                                precursorData.precursorM_Z = mass / (double)(Math.Abs(charge));
                                precursorData.precursorCharge = charge;
                                precursorData.adduct = adduct.Key;
                                precursorData.atomsCount = atomsCount;
                                precursorData.fa1 = fa;
                                precursorData.fa2 = null;
                                precursorData.fa3 = null;
                                precursorData.fa4 = null;
                                precursorData.lcb = null;
                                precursorData.MS2Fragments = MS2Fragments[headgroup];
                                
                                precursorDataList.Add(precursorData);
                                        
                                foreach (Precursor heavyPrecursor  in headgroups[headgroup].heavyLabeledPrecursors)
                                {
                                    string heavyHeadgroup = heavyPrecursor.name;
                                    
                                    if (headgroups[heavyHeadgroup].adductRestrictions[adduct.Key])
                                    {
                                        string suffix = heavyHeadgroup.Split(new Char[]{'/'})[1];
                                        string heavyKey = key + "/" + suffix;
                                        
                                        FattyAcid heavyFA1 = new FattyAcid(fa);
                                        heavyFA1.updateForHeavyLabeled((DataTable)heavyPrecursor.userDefinedFattyAcids[0]);
                            
                                        DataTable heavyAtomsCount = MS2Fragment.createEmptyElementTable();
                                        MS2Fragment.addCounts(heavyAtomsCount, heavyFA1.atomsCount);
                                        MS2Fragment.addCounts(heavyAtomsCount, headgroups[heavyHeadgroup].elements);
                                        String heavyChemForm = LipidCreator.computeChemicalFormula(heavyAtomsCount);
                                        int heavyCharge = getChargeAndAddAdduct(heavyAtomsCount, adduct.Key);
                                        double heavyMass = LipidCreator.computeMass(heavyAtomsCount, heavyCharge);
                                                                            

                                        PrecursorData heavyPrecursorData = new PrecursorData();
                                        heavyPrecursorData.lipidCategory = LipidCategory.Cholesterol;
                                        heavyPrecursorData.moleculeListName = headgroup;
                                        heavyPrecursorData.precursorName = heavyKey;
                                        heavyPrecursorData.precursorIonFormula = heavyChemForm;
                                        heavyPrecursorData.precursorAdduct = "[M" + adduct.Key + "]";
                                        heavyPrecursorData.precursorM_Z = heavyMass / (double)(Math.Abs(heavyCharge));
                                        heavyPrecursorData.precursorCharge = heavyCharge;
                                        heavyPrecursorData.adduct = adduct.Key;
                                        heavyPrecursorData.atomsCount = heavyAtomsCount;
                                        heavyPrecursorData.fa1 = heavyFA1;
                                        heavyPrecursorData.fa2 = null;
                                        heavyPrecursorData.fa3 = null;
                                        heavyPrecursorData.fa4 = null;
                                        heavyPrecursorData.lcb = null;
                                        heavyPrecursorData.MS2Fragments = MS2Fragments[heavyHeadgroup];
                                        
                                        precursorDataList.Add(heavyPrecursorData);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                String headgroup = "Ch";
                String key = headgroup + " ";
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
                            precursorData.lipidCategory = LipidCategory.Cholesterol;
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
                                        
                            foreach (Precursor heavyPrecursor  in headgroups[headgroup].heavyLabeledPrecursors)
                            {
                                string heavyHeadgroup = heavyPrecursor.name;
                                
                                if (headgroups[heavyHeadgroup].adductRestrictions[adduct.Key])
                                {
                                    string suffix = heavyHeadgroup.Split(new Char[]{'/'})[1];
                                    string heavyKey = key + "/" + suffix;
                        
                                    DataTable heavyAtomsCount = MS2Fragment.createEmptyElementTable();
                                    MS2Fragment.addCounts(heavyAtomsCount, headgroups[heavyHeadgroup].elements);
                                    String heavyChemForm = LipidCreator.computeChemicalFormula(heavyAtomsCount);
                                    int heavyCharge = getChargeAndAddAdduct(heavyAtomsCount, adduct.Key);
                                    double heavyMass = LipidCreator.computeMass(heavyAtomsCount, heavyCharge);
                                                                        

                                    PrecursorData heavyPrecursorData = new PrecursorData();
                                    heavyPrecursorData.lipidCategory = LipidCategory.Cholesterol;
                                    heavyPrecursorData.moleculeListName = headgroup;
                                    heavyPrecursorData.precursorName = heavyKey;
                                    heavyPrecursorData.precursorIonFormula = heavyChemForm;
                                    heavyPrecursorData.precursorAdduct = "[M" + adduct.Key + "]";
                                    heavyPrecursorData.precursorM_Z = heavyMass / (double)(Math.Abs(heavyCharge));
                                    heavyPrecursorData.precursorCharge = heavyCharge;
                                    heavyPrecursorData.adduct = adduct.Key;
                                    heavyPrecursorData.atomsCount = heavyAtomsCount;
                                    heavyPrecursorData.fa1 = null;
                                    heavyPrecursorData.fa2 = null;
                                    heavyPrecursorData.fa3 = null;
                                    heavyPrecursorData.fa4 = null;
                                    heavyPrecursorData.lcb = null;
                                    heavyPrecursorData.MS2Fragments = MS2Fragments[heavyHeadgroup];
                                    
                                    precursorDataList.Add(heavyPrecursorData);
                                }
                            }
                            
                        }
                    }
                }
            }
        }
    }
}