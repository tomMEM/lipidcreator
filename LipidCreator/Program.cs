﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;

using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using SkylineTool;

namespace LipidCreator
{    
    [Serializable]
    public class FattyAcid : IComparable<FattyAcid>
    {
        public int length;
        public int db;
        public String suffix;
        public DataTable atomsCount;

        public FattyAcid(int l, int _db)
        {
            length = l;
            db = _db;
            suffix = "";
            atomsCount = MS2Fragment.createEmptyElementTable();
            if (l > 0 || _db > 0){
                atomsCount.Rows[0]["Count"] = l;
                atomsCount.Rows[1]["Count"] = 2 * l - 1 - 2 * _db;
                atomsCount.Rows[2]["Count"] = 1;
            }
        }

        public FattyAcid(int l, int _db, String _suffix)
        {
            length = l;
            db = _db;
            suffix = (_suffix.Length > 2) ? _suffix.Substring(2, 1) : "";
            atomsCount = MS2Fragment.createEmptyElementTable();
            if (l > 0 || _db > 0){
                atomsCount.Rows[0]["Count"] = l;
                switch(suffix)
                {
                    case "":
                        atomsCount.Rows[1]["Count"] = 2 * l - 1 - 2 * _db;
                        atomsCount.Rows[2]["Count"] = 1;
                        break;
                    case "p":
                        atomsCount.Rows[1]["Count"] = 2 * l - 1 - 2 * _db;
                        atomsCount.Rows[2]["Count"] = 0;
                        break;
                    case "e":
                        atomsCount.Rows[1]["Count"] = (l + 1) * 2 - 1 - 2 * _db;
                        atomsCount.Rows[2]["Count"] = 0;
                        break;
                    case "h":
                        atomsCount.Rows[1]["Count"] = 2 * l - 1 - 2 * _db;
                        atomsCount.Rows[2]["Count"] = 2;
                        break;
                }
            }
        }
        
        public FattyAcid(FattyAcid copy)
        {
            length = copy.length;
            db = copy.length;
            suffix = copy.suffix;
            atomsCount = MS2Fragment.createEmptyElementTable(copy.atomsCount);
        }

        public int CompareTo(FattyAcid other)
        {
            if (length != other.length)
            {
                return length - other.length;
            }
            else if (db != other.db)
            {
                return db - other.db;
            }
            else if (suffix.Length != other.suffix.Length)
            {
                return suffix.Length - other.suffix.Length;
            }
            else if (suffix.Length > 0 && suffix[0] != other.suffix[0])
            {
                return suffix[0] - other.suffix[0];
            }
            return 0;
        }
    }
    
    
    [Serializable]
    public class FattyAcidComparer : EqualityComparer<FattyAcid>
    {
        public override int GetHashCode(FattyAcid obj)
        {
            return obj.length * 31 + obj.db;
        }
        
        public override bool Equals(FattyAcid obj, FattyAcid obj2)
        { 
            return (obj.length == obj2.length) && (obj.db == obj2.db) && (obj.suffix == obj2.suffix);
        }
    }

    [Serializable]
    public class fattyAcidGroup
    {
        public int chainType; // 0 = no restriction, 1 = odd carbon number, 2 = even carbon number
        public String lengthInfo;
        public String dbInfo;
        public String hydroxylInfo;
        public Dictionary<String, bool> faTypes;
        public HashSet<int> carbonCounts;
        public HashSet<int> doubleBondCounts;
        public HashSet<int> hydroxylCounts;
    
        public fattyAcidGroup()
        {
            chainType = 0;
            lengthInfo = "2-5";
            dbInfo = "0";
            hydroxylInfo = "0";
            faTypes = new Dictionary<String, bool>();
            faTypes.Add("FA", true);
            faTypes.Add("FAp", false);
            faTypes.Add("FAe", false);
            faTypes.Add("FAx", false);  // no fatty acid dummy
            carbonCounts = new HashSet<int>();
            doubleBondCounts = new HashSet<int>();
            hydroxylCounts = new HashSet<int>();
        }
        
        public fattyAcidGroup(fattyAcidGroup copy)
        {
            chainType = copy.chainType;
            lengthInfo = copy.lengthInfo;
            dbInfo = copy.dbInfo;
            hydroxylInfo = copy.hydroxylInfo;
            faTypes = new Dictionary<String, bool>();
            faTypes.Add("FA", copy.faTypes["FA"]);
            faTypes.Add("FAp", copy.faTypes["FAp"]);
            faTypes.Add("FAe", copy.faTypes["FAe"]);
            faTypes.Add("FAx", copy.faTypes["FAx"]);  // no fatty acid dummy
            carbonCounts = new HashSet<int>();
            foreach (int l in copy.carbonCounts)
            {
                carbonCounts.Add(l);
            }
            doubleBondCounts = new HashSet<int>();
            foreach (int d in copy.doubleBondCounts)
            {
                doubleBondCounts.Add(d);
            }
            hydroxylCounts = new HashSet<int>();
            foreach (int d in copy.hydroxylCounts)
            {
                hydroxylCounts.Add(d);
            }
        }
        
        public void import(XElement node)
        {
            chainType = Convert.ToInt32(node.Attribute("chainType").Value);
            lengthInfo = node.Attribute("lengthInfo").Value;
            dbInfo = node.Attribute("dbInfo").Value;
            hydroxylInfo = node.Attribute("hydroxylInfo").Value;
            
            carbonCounts.Clear();
            doubleBondCounts.Clear();
            hydroxylCounts.Clear();
        
            foreach(XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "faType":
                        faTypes[child.Attribute("type").Value.ToString()] = child.Value == "1";
                        break;
                        
                    case "length":
                        carbonCounts.Add(Convert.ToInt32(child.Value.ToString()));
                        break;
                        
                    case "doublebond":
                        doubleBondCounts.Add(Convert.ToInt32(child.Value.ToString()));
                        break;
                        
                    case "hydroxyl":
                        hydroxylCounts.Add(Convert.ToInt32(child.Value.ToString()));
                        break;
                        
                    default:
                        throw new Exception();
                }
            }
        }
        
        public string serialize()
        {
            string xml = "<fattyAcidGroup";
            xml += " chainType=\"" + chainType + "\"";
            xml += " lengthInfo=\"" + lengthInfo + "\"";
            xml += " dbInfo=\"" + dbInfo + "\"";
            xml += " hydroxylInfo=\"" + hydroxylInfo + "\">\n";
            foreach (KeyValuePair<String, bool> item in faTypes)
            {
                xml += "<faType";
                xml += " type=\"" + item.Key + "\">";
                xml += (item.Value ? 1 : 0);
                xml += "</faType>\n";
            }
            foreach (int len in carbonCounts)
            {
                xml += "<length>" + len + "</length>\n";
            }
            foreach (int db in doubleBondCounts)
            {
                xml += "<doublebond>" + db + "</doublebond>\n";
            }
            foreach (int hydroxyl in hydroxylCounts)
            {
                xml += "<hydroxyl>" + hydroxyl + "</hydroxyl>\n";
            }
            xml += "</fattyAcidGroup>\n";
            return xml;
        }
        
        
        
        public bool any_fa_checked()
        {
            return faTypes["FA"] || faTypes["FAp"] || faTypes["FAe"];
        }
    }

    [Serializable]
    public class MS2Fragment
    {
        public String fragmentName;
        public int fragmentCharge;
        public String fragmentFile;
        public bool fragmentSelected;
        public DataTable fragmentElements;
        public ArrayList fragmentBase;
        public HashSet<string> restrictions;
    
        public string serialize()
        {
            string xml = "<MS2Fragment";
            xml += " fragmentName=\"" + fragmentName + "\"";
            xml += " fragmentCharge=\"" + fragmentCharge + "\"";
            xml += " fragmentFile=\"" + fragmentFile + "\"";
            xml += " fragmentSelected=\"" + (fragmentSelected ? 1 : 0) + "\">\n";
            foreach (string restriction in restrictions)
            {
                xml += "<restriction>" + restriction + "</restriction>\n";
            }
            foreach (string fbase in fragmentBase)
            {
                xml += "<fragmentBase>" + fbase + "</fragmentBase>\n";
            }
            foreach (DataRow dr in fragmentElements.Rows)
            {
                xml += "<Element type=\"" + dr["Shortcut"] + "\">" + dr["Count"] + "</Element>\n";
            }
            xml += "</MS2Fragment>\n";
            return xml;
        }
        
        public void import(XElement node)
        {
            Dictionary<String, int> ElementPositions = new Dictionary<String, int>(){
                {"C", 0},
                {"H", 1},
                {"O", 2},
                {"N", 3},
                {"P", 4},
                {"S", 5},
                {"Na", 6}
            };
        
            restrictions.Clear();
            fragmentBase.Clear();
        
            fragmentName = node.Attribute("fragmentName").Value.ToString();
            fragmentCharge = Convert.ToInt32(node.Attribute("fragmentCharge").Value.ToString());
            fragmentFile = node.Attribute("fragmentFile").Value.ToString();
            fragmentSelected = node.Attribute("fragmentSelected").Value.ToString() == "1";
            
            
            foreach(XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "restriction":
                        restrictions.Add(child.Value.ToString());
                        break;
                        
                    case "fragmentBase":
                        fragmentBase.Add(child.Value.ToString());
                        break;
                        
                    case "Element":
                        fragmentElements.Rows[ElementPositions[child.Attribute("type").Value.ToString()]]["Count"] = child.Value.ToString();
                        break;
                        
                    default:
                        throw new Exception();
                }
            }
        }
    
        public static void addCounts(DataTable dt1, DataTable dt2)
        {
            String count = "Count";
            dt1.Rows[0][count] = (int)dt1.Rows[0][count] + (int)dt2.Rows[0][count];  // carbon
            dt1.Rows[1][count] = (int)dt1.Rows[1][count] + (int)dt2.Rows[1][count];  // hydrogen
            dt1.Rows[2][count] = (int)dt1.Rows[2][count] + (int)dt2.Rows[2][count];  // oxygen
            dt1.Rows[3][count] = (int)dt1.Rows[3][count] + (int)dt2.Rows[3][count];  // nitrogen
            dt1.Rows[4][count] = (int)dt1.Rows[4][count] + (int)dt2.Rows[4][count];  // phosphor
            dt1.Rows[5][count] = (int)dt1.Rows[5][count] + (int)dt2.Rows[5][count];  // sulfur
            dt1.Rows[6][count] = (int)dt1.Rows[6][count] + (int)dt2.Rows[6][count];  // sodium
        }
        
        public static DataTable createEmptyElementTable()
        {
            DataTable elements = new DataTable();
            elements.Clear();
            String count = "Count";
            String shortcut = "Shortcut";
            String element = "Element";
            String monoMass = "mass";

            DataColumn columnCount = elements.Columns.Add(count);
            DataColumn columnShortcut = elements.Columns.Add(shortcut);
            DataColumn columnElement = elements.Columns.Add(element);
            elements.Columns.Add(monoMass);

            columnCount.DataType = System.Type.GetType("System.Int32");
            columnShortcut.ReadOnly = true;
            columnElement.ReadOnly = true;

            DataRow carbon = elements.NewRow();
            carbon[count] = "0";
            carbon[shortcut] = "C";
            carbon[element] = "carbon";
            carbon[monoMass] = 12;
            elements.Rows.Add(carbon);

            DataRow hydrogen = elements.NewRow();
            hydrogen[count] = "0";
            hydrogen[shortcut] = "H";
            hydrogen[element] = "hydrogen";
            hydrogen[monoMass] = 1.007825035;
            elements.Rows.Add(hydrogen);

            DataRow oxygen = elements.NewRow();
            oxygen[count] = "0";
            oxygen[shortcut] = "O";
            oxygen[element] = "oxygen";
            oxygen[monoMass] = 15.99491463;
            elements.Rows.Add(oxygen);

            DataRow nitrogen = elements.NewRow();
            nitrogen[count] = "0";
            nitrogen[shortcut] = "N";
            nitrogen[element] = "nitrogen";
            nitrogen[monoMass] = 14.003074;
            elements.Rows.Add(nitrogen);

            DataRow phosphor = elements.NewRow();
            phosphor[count] = "0";
            phosphor[shortcut] = "P";
            phosphor[element] = "phosphor";
            phosphor[monoMass] = 30.973762;
            elements.Rows.Add(phosphor);

            DataRow sulfur = elements.NewRow();
            sulfur[count] = "0";
            sulfur[shortcut] = "S";
            sulfur[element] = "sulfur";
            sulfur[monoMass] = 31.9720707;
            elements.Rows.Add(sulfur);

            DataRow sodium = elements.NewRow();
            sodium[count] = "0";
            sodium[shortcut] = "Na";
            sodium[element] = "sodium";
            sodium[monoMass] = 22.9897677;
            elements.Rows.Add(sodium);
            return elements;
        }
        
        public static DataTable createEmptyElementTable(DataTable copy)
        {
            DataTable elements = new DataTable();
            elements.Clear();
            String count = "Count";
            String shortcut = "Shortcut";
            String element = "Element";
            String monoMass = "mass";

            DataColumn columnCount = elements.Columns.Add(count);
            DataColumn columnShortcut = elements.Columns.Add(shortcut);
            DataColumn columnElement = elements.Columns.Add(element);
            elements.Columns.Add(monoMass);

            columnCount.DataType = System.Type.GetType("System.Int32");
            columnShortcut.ReadOnly = true;
            columnElement.ReadOnly = true;

            DataRow carbon = elements.NewRow();
            carbon[count] = copy.Rows[0][count];
            carbon[shortcut] = "C";
            carbon[element] = "carbon";
            carbon[monoMass] = 12;
            elements.Rows.Add(carbon);

            DataRow hydrogen = elements.NewRow();
            hydrogen[count] = copy.Rows[1][count];
            hydrogen[shortcut] = "H";
            hydrogen[element] = "hydrogen";
            hydrogen[monoMass] = 1.007825035;
            elements.Rows.Add(hydrogen);

            DataRow oxygen = elements.NewRow();
            oxygen[count] = copy.Rows[2][count];
            oxygen[shortcut] = "O";
            oxygen[element] = "oxygen";
            oxygen[monoMass] = 15.99491463;
            elements.Rows.Add(oxygen);

            DataRow nitrogen = elements.NewRow();
            nitrogen[count] = copy.Rows[3][count];
            nitrogen[shortcut] = "N";
            nitrogen[element] = "nitrogen";
            nitrogen[monoMass] = 14.003074;
            elements.Rows.Add(nitrogen);

            DataRow phosphor = elements.NewRow();
            phosphor[count] = copy.Rows[4][count];
            phosphor[shortcut] = "P";
            phosphor[element] = "phosphor";
            phosphor[monoMass] = 30.973762;
            elements.Rows.Add(phosphor);

            DataRow sulfur = elements.NewRow();
            sulfur[count] = copy.Rows[5][count];
            sulfur[shortcut] = "S";
            sulfur[element] = "sulfur";
            sulfur[monoMass] =  31.9720707;
            elements.Rows.Add(sulfur);

            DataRow sodium = elements.NewRow();
            sodium[count] = copy.Rows[6][count];
            sodium[shortcut] = "Na";
            sodium[element] = "sodium";
            sodium[monoMass] = 22.9897677;
            elements.Rows.Add(sodium);
            return elements;
        }
    
        public MS2Fragment()
        {
            fragmentName = "-";
            fragmentCharge = -1;
            fragmentFile = "-";
            fragmentSelected = true;
            fragmentElements = createEmptyElementTable();
            fragmentBase = new ArrayList();
            restrictions = new HashSet<string>();
        }

        public MS2Fragment(String name, String fileName)
        {
            fragmentName = name;
            fragmentCharge = -1;
            fragmentFile = fileName;
            fragmentSelected = true;
            fragmentElements = createEmptyElementTable();
            fragmentBase = new ArrayList();
            restrictions = new HashSet<string>();
        }


        public MS2Fragment(String name, String fileName, int charge)
        {
            fragmentName = name;
            fragmentCharge = charge;
            fragmentFile = fileName;
            fragmentSelected = true;
            fragmentElements = createEmptyElementTable();
            fragmentBase = new ArrayList();
            restrictions = new HashSet<string>();
        }
        
        public MS2Fragment(String name, int charge, String fileName, bool selected, DataTable dataElements, String baseForms, String restrictions)
        {
            fragmentName = name;
            fragmentCharge = charge;
            fragmentFile = fileName;
            fragmentSelected = selected;
            fragmentElements = dataElements;
            this.restrictions = new HashSet<string>();
            fragmentBase = new ArrayList(baseForms.Split(new char[] {';'}));
            if (restrictions.Length > 0) foreach (string restriction in restrictions.Split(new char[] {';'})) this.restrictions.Add(restriction);
        }

        public MS2Fragment(MS2Fragment copy)
        {
            fragmentName = copy.fragmentName;
            fragmentCharge = copy.fragmentCharge;
            fragmentFile = copy.fragmentFile;
            fragmentSelected = copy.fragmentSelected;
            fragmentElements = createEmptyElementTable(copy.fragmentElements);
            fragmentBase = new ArrayList();
            restrictions = new HashSet<string>();
            foreach (string fbase in copy.fragmentBase) fragmentBase.Add(fbase);
            foreach (string restriction in copy.restrictions) restrictions.Add(restriction);
        }
    }


    [Serializable]
    public class lipid
    {
        public string className;
        public Dictionary<String, ArrayList> MS2Fragments;
        public Dictionary<String, String> pathsToFullImage;
        public Dictionary<String, bool> adducts;
        public bool representativeFA;
    
        public lipid(){
            adducts = new Dictionary<String, bool>();
            adducts.Add("+H", false);
            adducts.Add("+2H", false);
            adducts.Add("+NH4", false);
            adducts.Add("+Na", false);
            adducts.Add("-H", true);
            adducts.Add("-2H", false);
            adducts.Add("+HCOO", false);
            adducts.Add("+CH3COO", false);
            MS2Fragments = new Dictionary<String, ArrayList>();
            pathsToFullImage = new Dictionary<String, String>();
            representativeFA = false;
        }
        
        public virtual void add_lipids(DataTable dt, Dictionary<String, DataTable> ddt)
        {
        }
        
        public virtual string serialize()
        {
            string xml = "<className>" + className + "</className>\n";
            xml += "<representativeFA>" + (representativeFA ? 1 : 0) + "</representativeFA>\n";
            foreach (KeyValuePair<String, bool> item in adducts)
            {
                xml += "<adduct type=\"" + item.Key + "\">" + (item.Value ? 1 : 0) + "</adduct>\n";
            }
            
            foreach (KeyValuePair<String, ArrayList> item in MS2Fragments)
            {
                xml += "<MS2FragmentGroup name=\"" + item.Key + "\">\n";
                foreach (MS2Fragment fragment in item.Value)
                {
                    xml += fragment.serialize();
                }
                xml += "</MS2FragmentGroup>\n";
            }
            return xml;
        }
        
        public lipid(lipid copy)
        {
            adducts = new Dictionary<String, bool>();
            adducts.Add("+H", copy.adducts["+H"]);
            adducts.Add("+2H", copy.adducts["+2H"]);
            adducts.Add("+NH4", copy.adducts["+NH4"]);
            adducts.Add("+Na", copy.adducts["+Na"]);
            adducts.Add("-H", copy.adducts["-H"]);
            adducts.Add("-2H", copy.adducts["-2H"]);
            adducts.Add("+HCOO", copy.adducts["+HCOO"]);
            adducts.Add("+CH3COO", copy.adducts["+CH3COO"]);
            className = copy.className;
            representativeFA = copy.representativeFA;
            MS2Fragments = new Dictionary<String, ArrayList>();
            pathsToFullImage = new Dictionary<String, String>();
            foreach (KeyValuePair<String, String> item in copy.pathsToFullImage)
            {
                pathsToFullImage.Add(item.Key, item.Value);
            }
            
            foreach (KeyValuePair<String, ArrayList> item in copy.MS2Fragments)
            {
                MS2Fragments.Add(item.Key, new ArrayList());
                foreach (MS2Fragment fragment in item.Value)
                {
                    MS2Fragments[item.Key].Add(new MS2Fragment(fragment));
                }
            }
        }
        
        public int get_charge_and_add_adduct(DataTable atomsCount, String adduct)
        {
            int charge = 0;
            switch (adduct)
            {
                                                                                                
                case "+H":
                    atomsCount.Rows[1]["Count"] = (int)atomsCount.Rows[1]["Count"] + 1;
                    charge = 1;
                    break;
                case "+2H":
                    atomsCount.Rows[1]["Count"] = (int)atomsCount.Rows[1]["Count"] + 2;
                    charge = 2;
                    break;
                case "+NH4":
                    atomsCount.Rows[1]["Count"] = (int)atomsCount.Rows[1]["Count"] + 4;
                    atomsCount.Rows[3]["Count"] = (int)atomsCount.Rows[3]["Count"] + 1;
                    charge = 1;
                    break;
                case "+Na":
                    atomsCount.Rows[6]["Count"] = (int)atomsCount.Rows[6]["Count"] + 1;
                    charge = 1;
                    break;
                case "-H":
                    atomsCount.Rows[1]["Count"] = (int)atomsCount.Rows[1]["Count"] - 1;
                    charge = -1;
                    break;
                case "-2H":
                    atomsCount.Rows[1]["Count"] = (int)atomsCount.Rows[1]["Count"] - 2;
                    charge = -2;
                    break;
                case "+HCOO":
                    atomsCount.Rows[1]["Count"] = (int)atomsCount.Rows[1]["Count"] + 1;
                    atomsCount.Rows[0]["Count"] = (int)atomsCount.Rows[0]["Count"] + 1;
                    atomsCount.Rows[2]["Count"] = (int)atomsCount.Rows[2]["Count"] + 2;
                    charge = -1;
                    break;
                case "+CH3COO":
                    atomsCount.Rows[0]["Count"] = (int)atomsCount.Rows[0]["Count"] + 2;
                    atomsCount.Rows[1]["Count"] = (int)atomsCount.Rows[1]["Count"] + 3;
                    atomsCount.Rows[2]["Count"] = (int)atomsCount.Rows[2]["Count"] + 2;
                    charge = -1;
                    break;
            }
            return charge;
        }
        
        
        public virtual void import(XElement node)
        {
            switch (node.Name.ToString())
            {
                case "className":
                    className = node.Value.ToString();
                    break;
                    
                case "representativeFA":
                    representativeFA = node.Value == "1";
                    break;
                    
                case "adduct":
                    string adductKey = node.Attribute("type").Value.ToString();
                    adducts[adductKey] = node.Value == "1";
                    break;
                    
                case "MS2FragmentGroup":
                    string fragmentKey = node.Attribute("name").Value.ToString();
                    foreach (XElement fragment in node.Elements()){
                        MS2Fragment ms2fragment = new MS2Fragment();
                        ms2fragment.import(fragment);
                        MS2Fragments[fragmentKey].Add(ms2fragment);
                    }
                    break;
                    
                default:
                    Console.WriteLine("Error: " + node.Name.ToString());
                    throw new Exception();
            }
        }
    }
    
    

    [Serializable]
    public class cl_lipid : lipid
    {
        public fattyAcidGroup fag1;
        public fattyAcidGroup fag2;
        public fattyAcidGroup fag3;
        public fattyAcidGroup fag4;
    
        public cl_lipid(Dictionary<String, String> all_paths, Dictionary<String, ArrayList> all_fragments)
        {
            fag1 = new fattyAcidGroup();
            fag2 = new fattyAcidGroup();
            fag3 = new fattyAcidGroup();
            fag4 = new fattyAcidGroup();
            MS2Fragments.Add("CL", new ArrayList());
            MS2Fragments.Add("MLCL", new ArrayList());
            
            foreach(KeyValuePair<String, ArrayList> kvp in MS2Fragments)
            {
                if (all_paths.ContainsKey(kvp.Key)) pathsToFullImage.Add(kvp.Key, all_paths[kvp.Key]);
                if (all_fragments != null && all_fragments.ContainsKey(kvp.Key))
                {
                    foreach (MS2Fragment fragment in all_fragments[kvp.Key])
                    {
                        MS2Fragments[kvp.Key].Add(new MS2Fragment(fragment));
                    }
                }
            }
        }
        
        public cl_lipid(cl_lipid copy) : base((lipid)copy)
        {
            fag1 = new fattyAcidGroup(copy.fag1);
            fag2 = new fattyAcidGroup(copy.fag2);
            fag3 = new fattyAcidGroup(copy.fag3);
            fag4 = new fattyAcidGroup(copy.fag4);
        }
        
        public override string serialize()
        {
            string xml = "<lipid type=\"CL\">\n";
            xml += fag1.serialize();
            xml += fag2.serialize();
            xml += fag3.serialize();
            xml += fag4.serialize();
            xml += base.serialize();
            xml += "</lipid>\n";
            return xml;
        }
        
        public override void import(XElement node)
        {
            int fa_counter = 0;
            foreach (XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "fattyAcidGroup":
                        if (fa_counter == 0)
                        {
                            fag1.import(child);
                        }
                        else if (fa_counter == 1)
                        {
                            fag2.import(child);
                        }
                        else if (fa_counter == 2)
                        {
                            fag3.import(child);
                        }
                        else if (fa_counter == 3)
                        {
                            fag4.import(child);
                        }
                        else
                        {   
                            Console.WriteLine("Error, fatty acid");
                            throw new Exception();
                        }
                        ++fa_counter;
                        break;
                        
                    default:
                        base.import(child);
                        break;
                }
            }
        }
        
        public override void add_lipids(DataTable all_lipids, Dictionary<String, DataTable> ddt)
        {
            // check if more than one fatty acids are 0:0
            int check_FattyAcids = 0;
            check_FattyAcids += fag1.faTypes["FAx"] ? 1 : 0;
            check_FattyAcids += fag2.faTypes["FAx"] ? 1 : 0;
            check_FattyAcids += fag3.faTypes["FAx"] ? 1 : 0;
            check_FattyAcids += fag4.faTypes["FAx"] ? 1 : 0;
            if (check_FattyAcids > 1) return;
            
            
            HashSet<String> used_keys = new HashSet<String>();
            int contains_mono_lyso = 0;
            foreach (int fa_l_1 in fag1.carbonCounts)
            {
                int max_db_1 = (fa_l_1 - 1) >> 1;
                foreach (int fa_db_1 in fag1.doubleBondCounts)
                {
                    foreach (KeyValuePair<string, bool> fa_kvp_1 in fag1.faTypes)
                    {
                        if (fa_kvp_1.Value && max_db_1 >= fa_db_1)
                        {
                            FattyAcid fa1 = new FattyAcid(fa_l_1, fa_db_1, fa_kvp_1.Key);
                            contains_mono_lyso &= ~1;
                            if (fa_kvp_1.Key == "FAx")
                            {
                                fa1 = new FattyAcid(0, 0, "FA");
                                contains_mono_lyso |= 1;
                            }
                            foreach (int fa_l_2 in fag2.carbonCounts)
                            {
                                int max_db_2 = (fa_l_2 - 1) >> 1;
                                foreach (int fa_db_2 in fag2.doubleBondCounts)
                                {
                                    foreach (KeyValuePair<string, bool> fa_kvp_2 in fag2.faTypes)
                                    {
                                        if (fa_kvp_2.Value && max_db_2 >= fa_db_2)
                                        {
                                            FattyAcid fa2 = new FattyAcid(fa_l_2, fa_db_2, fa_kvp_2.Key);
                                            contains_mono_lyso &= ~2;
                                            if (fa_kvp_2.Key == "FAx")
                                            {
                                                fa2 = new FattyAcid(0, 0, "FA");
                                                contains_mono_lyso |= 2;
                                            }
                                            foreach (int fa_l_3 in fag3.carbonCounts)
                                            {
                                                int max_db_3 = (fa_l_3 - 1) >> 1;
                                                foreach (int fa_db_3 in fag3.doubleBondCounts)
                                                {
                                                    foreach (KeyValuePair<string, bool> fa_kvp_3 in fag3.faTypes)
                                                    {
                                                        if (fa_kvp_3.Value && max_db_3 >= fa_db_3)
                                                        {
                                                            FattyAcid fa3 = new FattyAcid(fa_l_3, fa_db_3, fa_kvp_3.Key);
                                                            contains_mono_lyso &= ~4;
                                                            if (fa_kvp_3.Key == "FAx")
                                                            {
                                                                fa3 = new FattyAcid(0, 0, "FA");
                                                                contains_mono_lyso |= 4;
                                                            }
                                                            foreach (int fa_l_4 in fag4.carbonCounts)
                                                            {
                                                                int max_db_4 = (fa_l_4 - 1) >> 1;
                                                                foreach (int fa_db_4 in fag4.doubleBondCounts)
                                                                {
                                                                    foreach (KeyValuePair<string, bool> fa_kvp_4 in fag4.faTypes)
                                                                    {
                                                                        if (fa_kvp_4.Value && max_db_4 >= fa_db_4)
                                                                        {
                                                                            FattyAcid fa4 = new FattyAcid(fa_l_4, fa_db_4, fa_kvp_4.Key);
                                                                            contains_mono_lyso &= ~8;
                                                                            if (fa_kvp_4.Key == "FAx")
                                                                            {
                                                                                fa4 = new FattyAcid(0, 0, "FA");
                                                                                contains_mono_lyso |= 8;
                                                                            }
                                                                            
                                                                            
                                                                            
                                                                            
                                                                            List<FattyAcid> sorted_acids = new List<FattyAcid>();
                                                                            sorted_acids.Add(fa1);
                                                                            sorted_acids.Add(fa2);
                                                                            sorted_acids.Add(fa3);
                                                                            sorted_acids.Add(fa4);
                                                                            sorted_acids.Sort();
                                                                            String headgroup = (contains_mono_lyso == 0) ? "CL" : "MLCL";
                                                                            String key = headgroup + " ";
                                                                            int i = 0;
                                                                            foreach (FattyAcid fa in sorted_acids)
                                                                            {
                                                                                if (fa.length > 0){
                                                                                    if (i++ > 0) key += "_";
                                                                                    key += Convert.ToString(fa.length) + ":" + Convert.ToString(fa.db) + fa.suffix;
                                                                                }
                                                                            }
                                                                            if (!used_keys.Contains(key))
                                                                            {
                                                                            
                                                                            
                                                                                foreach (KeyValuePair<string, bool> adduct in adducts)
                                                                                {
                                                                                    if (adduct.Value)
                                                                                    {
                                                                                        used_keys.Add(key);
                                                                                        
                                                                                        DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                                                                                        MS2Fragment.addCounts(atomsCount, fa1.atomsCount);
                                                                                        MS2Fragment.addCounts(atomsCount, fa2.atomsCount);
                                                                                        MS2Fragment.addCounts(atomsCount, fa3.atomsCount);
                                                                                        MS2Fragment.addCounts(atomsCount, fa4.atomsCount);
                                                                                        MS2Fragment.addCounts(atomsCount, ddt[headgroup]);
                                                                                        String chemForm = LipidCreatorForm.compute_chemical_formula(atomsCount);
                                                                                        int charge = get_charge_and_add_adduct(atomsCount, adduct.Key);
                                                                                        double mass = LipidCreatorForm.compute_mass(atomsCount);
                                                                                        
                                                                                        
                                                                                        
                                                                                        
                                                                                        foreach (MS2Fragment fragment in MS2Fragments[headgroup])
                                                                                        {
                                                                                            if (fragment.fragmentSelected && ((charge < 0 && fragment.fragmentCharge < 0) || (charge > 0 && fragment.fragmentCharge > 0)))
                                                                                            {
                                                                                                DataTable atomsCountFragment = MS2Fragment.createEmptyElementTable(fragment.fragmentElements);
                                                                                                foreach (string fbase in fragment.fragmentBase)
                                                                                                {
                                                                                                    switch(fbase)
                                                                                                    {
                                                                                                        case "FA1":
                                                                                                            MS2Fragment.addCounts(atomsCountFragment, fa1.atomsCount);
                                                                                                            break;
                                                                                                        case "FA2":
                                                                                                            MS2Fragment.addCounts(atomsCountFragment, fa2.atomsCount);
                                                                                                            break;
                                                                                                        case "FA3":
                                                                                                            MS2Fragment.addCounts(atomsCountFragment, fa3.atomsCount);
                                                                                                            break;
                                                                                                        case "FA4":
                                                                                                            MS2Fragment.addCounts(atomsCountFragment, fa4.atomsCount);
                                                                                                            break;
                                                                                                        case "PRE":
                                                                                                            MS2Fragment.addCounts(atomsCountFragment, atomsCount);
                                                                                                            break;
                                                                                                        default:
                                                                                                            break;
                                                                                                    }
                                                                                                }
                                                                                                String chemFormFragment = LipidCreatorForm.compute_chemical_formula(atomsCountFragment);
                                                                                                double massFragment = LipidCreatorForm.compute_mass(atomsCountFragment);
                                                                                                
                                                                                            
                                                                                                DataRow lipid_row = all_lipids.NewRow();
                                                                                                lipid_row["Molecule List Name"] = headgroup;
                                                                                                lipid_row["Precursor Name"] = key;
                                                                                                lipid_row["Precursor Ion Formula"] = chemForm;
                                                                                                lipid_row["Precursor Adduct"] = "[M" + adduct.Key + "]";
                                                                                                lipid_row["Precursor m/z"] = mass / (double)(Math.Abs(charge));
                                                                                                lipid_row["Precursor Charge"] = ((charge > 0) ? "+" : "") + Convert.ToString(charge);
                                                                                                lipid_row["Pruduct Name"] = fragment.fragmentName;
                                                                                                lipid_row["Pruduct Ion Formula"] = chemFormFragment;
                                                                                                lipid_row["Pruduct m/z"] = massFragment / (double)(Math.Abs(fragment.fragmentCharge));
                                                                                                lipid_row["Pruduct Charge"] = ((fragment.fragmentCharge > 0) ? "+" : "") + Convert.ToString(fragment.fragmentCharge);
                                                                                                all_lipids.Rows.Add(lipid_row);
                                                                                            }
                                                                                        }
                                                                                    }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class gl_lipid : lipid
    {
        public fattyAcidGroup fag1;
        public fattyAcidGroup fag2;
        public fattyAcidGroup fag3;
        public bool contains_sugar;
        public List<int> hgValues;
        public List<String> headGroupNames = new List<String>{"MGDG", "DGDG", "SQDG"};
    
    
        public gl_lipid(Dictionary<String, String> all_paths, Dictionary<String, ArrayList> all_fragments)
        {
            fag1 = new fattyAcidGroup();
            fag2 = new fattyAcidGroup();
            fag3 = new fattyAcidGroup();
            contains_sugar = false;
            hgValues = new List<int>();
            MS2Fragments.Add("MG", new ArrayList());
            MS2Fragments.Add("DG", new ArrayList());
            MS2Fragments.Add("MGDG", new ArrayList());
            MS2Fragments.Add("DGDG", new ArrayList());
            MS2Fragments.Add("SQDG", new ArrayList());
            MS2Fragments.Add("TG", new ArrayList());
            
            foreach(KeyValuePair<String, ArrayList> kvp in MS2Fragments)
            {
                if (all_paths.ContainsKey(kvp.Key)) pathsToFullImage.Add(kvp.Key, all_paths[kvp.Key]);
                if (all_fragments != null && all_fragments.ContainsKey(kvp.Key))
                {
                    foreach (MS2Fragment fragment in all_fragments[kvp.Key])
                    {
                        MS2Fragments[kvp.Key].Add(new MS2Fragment(fragment));
                    }
                }
            }
        }
    
        public gl_lipid(gl_lipid copy) : base((lipid)copy) 
        {
            fag1 = new fattyAcidGroup(copy.fag1);
            fag2 = new fattyAcidGroup(copy.fag2);
            fag3 = new fattyAcidGroup(copy.fag3);
            contains_sugar = copy.contains_sugar;
            hgValues = new List<int>();
            foreach (int hgValue in copy.hgValues)
            {
                hgValues.Add(hgValue);
            }
            
        }
        
        
        public override string serialize()
        {
            string xml = "<lipid type=\"GL\">\n";
            xml += fag1.serialize();
            xml += fag2.serialize();
            xml += fag3.serialize();
            xml += "<containsSugar>" + (contains_sugar ? 1 : 0) + "</containsSugar>\n";
            foreach (int hgValue in hgValues)
            {
                xml += "<headGroup>" + hgValue + "</headGroup>\n";
            }
            xml += base.serialize();
            xml += "</lipid>\n";
            return xml;
        }
        
        
        public override void import(XElement node)
        {
            int fa_counter = 0;
            hgValues.Clear();
            foreach (XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "fattyAcidGroup":
                        if (fa_counter == 0)
                        {
                            fag1.import(child);
                        }
                        else if (fa_counter == 1)
                        {
                            fag2.import(child);
                        }
                        else if (fa_counter == 2)
                        {
                            fag3.import(child);
                        }
                        else
                        {   
                            Console.WriteLine("Error, fatty acid");
                            throw new Exception();
                        }
                        ++fa_counter;
                        break;
                        
                    case "headGroup":
                        hgValues.Add(Convert.ToInt32(child.Value.ToString()));
                        break;
                        
                    case "containsSugar":
                        contains_sugar = child.Value == "1";
                        break;
                        
                        
                    default:
                        base.import(child);
                        break;
                }
            }
        }
        
        public override void add_lipids(DataTable all_lipids, Dictionary<String, DataTable> ddt)
        {
            // check if more than one fatty acids are 0:0
            int check_FattyAcids = 0;
            check_FattyAcids += fag1.faTypes["FAx"] ? 1 : 0;
            check_FattyAcids += fag2.faTypes["FAx"] ? 1 : 0;
            check_FattyAcids += fag3.faTypes["FAx"] ? 1 : 0;
            if (check_FattyAcids > 2) return;
            
            HashSet<String> used_keys = new HashSet<String>();
            int contains_mono_lyso = 0;
            foreach (int fa_l_1 in fag1.carbonCounts)
            {
                int max_db_1 = (fa_l_1 - 1) >> 1;
                foreach (int fa_db_1 in fag1.doubleBondCounts)
                {
                    foreach (KeyValuePair<string, bool> fa_kvp_1 in fag1.faTypes)
                    {
                        if (fa_kvp_1.Value && max_db_1 >= fa_db_1)
                        {
                            FattyAcid fa1 = new FattyAcid(fa_l_1, fa_db_1, fa_kvp_1.Key);
                            contains_mono_lyso &= ~1;
                            if (fa_kvp_1.Key == "FAx")
                            {
                                fa1 = new FattyAcid(0, 0, "FA");
                                contains_mono_lyso |= 1;
                            }
                            foreach (int fa_l_2 in fag2.carbonCounts)
                            {
                                int max_db_2 = (fa_l_2 - 1) >> 1;
                                foreach (int fa_db_2 in fag2.doubleBondCounts)
                                {
                                    foreach (KeyValuePair<string, bool> fa_kvp_2 in fag2.faTypes)
                                    {
                                        if (fa_kvp_2.Value && max_db_2 >= fa_db_2)
                                        {
                                            FattyAcid fa2 = new FattyAcid(fa_l_2, fa_db_2, fa_kvp_2.Key);
                                            contains_mono_lyso &= ~2;
                                            if (fa_kvp_2.Key == "FAx")
                                            {
                                                fa2 = new FattyAcid(0, 0, "FA");
                                                contains_mono_lyso |= 2;
                                            }
                                            foreach (int fa_l_3 in fag3.carbonCounts)
                                            {
                                                int max_db_3 = (fa_l_3 - 1) >> 1;
                                                foreach (int fa_db_3 in fag3.doubleBondCounts)
                                                {
                                                    foreach (KeyValuePair<string, bool> fa_kvp_3 in fag3.faTypes)
                                                    {
                                                        if (fa_kvp_3.Value && max_db_3 >= fa_db_3)
                                                        {
                                                            FattyAcid fa3 = new FattyAcid(fa_l_3, fa_db_3, fa_kvp_3.Key);
                                                            contains_mono_lyso &= ~4;
                                                            if (fa_kvp_3.Key == "FAx")
                                                            {
                                                                fa3 = new FattyAcid(0, 0, "FA");
                                                                contains_mono_lyso |= 4;
                                                            }
                                                                    
                                                                            
                                                            List<FattyAcid> sorted_acids = new List<FattyAcid>();
                                                            sorted_acids.Add(fa1);
                                                            sorted_acids.Add(fa2);
                                                            sorted_acids.Add(fa3);
                                                            sorted_acids.Sort();
                                                            
                                                            // popcount
                                                            int pc_contains_mono_lyso = contains_mono_lyso - ((contains_mono_lyso >> 1) & 0x55555555);
                                                            pc_contains_mono_lyso = (pc_contains_mono_lyso & 0x33333333) + ((pc_contains_mono_lyso >> 2) & 0x33333333);
                                                            pc_contains_mono_lyso = ((pc_contains_mono_lyso + (pc_contains_mono_lyso >> 4) & 0xF0F0F0F) * 0x1010101) >> 24;
                                                            
                                                            String headgroup = "";
                                                            switch(pc_contains_mono_lyso)
                                                            {
                                                                case 0:
                                                                headgroup = "TG";
                                                                    break;
                                                                case 1:
                                                                headgroup = "DG";
                                                                    break;
                                                                case 2:
                                                                headgroup = "MG";
                                                                    break;
                                                            }
                                                            String key = headgroup + " ";
                                                            int i = 0;
                                                            foreach (FattyAcid fa in sorted_acids)
                                                            {
                                                                if (fa.length > 0){
                                                                    if (i++ > 0) key += "_";
                                                                    key += Convert.ToString(fa.length) + ":" + Convert.ToString(fa.db) + fa.suffix;
                                                                }
                                                            }
                                                            if (!used_keys.Contains(key))
                                                            {
                                                                foreach (KeyValuePair<string, bool> adduct in adducts)
                                                                {
                                                                    if (adduct.Value)
                                                                    {
                                                                        used_keys.Add(key);
                                                                        
                                                                        DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                                                                        MS2Fragment.addCounts(atomsCount, fa1.atomsCount);
                                                                        MS2Fragment.addCounts(atomsCount, fa2.atomsCount);
                                                                        MS2Fragment.addCounts(atomsCount, fa3.atomsCount);
                                                                        MS2Fragment.addCounts(atomsCount, ddt[headgroup]);
                                                                        int charge = get_charge_and_add_adduct(atomsCount, adduct.Key);
                                                                        String chemForm = LipidCreatorForm.compute_chemical_formula(atomsCount);
                                                                        double mass = LipidCreatorForm.compute_mass(atomsCount);
                                                                        
                                                                        DataRow lipid_row = all_lipids.NewRow();
                                                                        lipid_row["Molecule List Name"] = headgroup;
                                                                        lipid_row["Precursor Name"] = key;
                                                                        lipid_row["Precursor Ion Formula"] = chemForm;
                                                                        lipid_row["Precursor Adduct"] = chemForm + "[M" + adduct.Key + "]";
                                                                        lipid_row["Precursor m/z"] = mass;
                                                                        lipid_row["Precursor Charge"] = ((charge > 0) ? "+" : "") + Convert.ToString(charge);
                                                                        all_lipids.Rows.Add(lipid_row);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class pl_lipid : lipid
    {
        public fattyAcidGroup fag1;
        public fattyAcidGroup fag2;
        public List<int> hgValues;
        
        public List<String> headGroupNames = new List<String>{"PA", "PC", "PE", "DMPE", "MMPE", "PG", "PI", "PIP", "PIP2", "PIP3", "PS", "LPA", "LPC", "LPE", "LPG", "LPI", "LPS"};
    
        public pl_lipid(Dictionary<String, String> all_paths, Dictionary<String, ArrayList> all_fragments)
        {
            fag1 = new fattyAcidGroup();
            fag2 = new fattyAcidGroup();
            hgValues = new List<int>();
            MS2Fragments.Add("PA", new ArrayList());
            MS2Fragments.Add("PC", new ArrayList());
            MS2Fragments.Add("PE", new ArrayList());
            MS2Fragments.Add("DMPE", new ArrayList());
            MS2Fragments.Add("MMPE", new ArrayList());
            MS2Fragments.Add("PG", new ArrayList());
            MS2Fragments.Add("PI", new ArrayList());
            MS2Fragments.Add("PIP", new ArrayList());
            MS2Fragments.Add("PIP2", new ArrayList());
            MS2Fragments.Add("PIP3", new ArrayList());
            MS2Fragments.Add("PS", new ArrayList());
            MS2Fragments.Add("LPA", new ArrayList());
            MS2Fragments.Add("LPC", new ArrayList());
            MS2Fragments.Add("LPE", new ArrayList());
            MS2Fragments.Add("LPG", new ArrayList());
            MS2Fragments.Add("LPI", new ArrayList());
            MS2Fragments.Add("LPS", new ArrayList());
            
            foreach(KeyValuePair<String, ArrayList> kvp in MS2Fragments)
            {
                if (all_paths.ContainsKey(kvp.Key)) pathsToFullImage.Add(kvp.Key, all_paths[kvp.Key]);
                if (all_fragments != null && all_fragments.ContainsKey(kvp.Key))
                {
                    foreach (MS2Fragment fragment in all_fragments[kvp.Key])
                    {
                        MS2Fragments[kvp.Key].Add(new MS2Fragment(fragment));
                    }
                }
            }
        }
    
        public pl_lipid(pl_lipid copy) : base((lipid)copy)
        {
            fag1 = new fattyAcidGroup(copy.fag1);
            fag2 = new fattyAcidGroup(copy.fag2);
            hgValues = new List<int>();
            foreach (int hgValue in copy.hgValues)
            {
                hgValues.Add(hgValue);
            }
        }
        
        public override string serialize()
        {
            string xml = "<lipid type=\"PL\">\n";
            xml += fag1.serialize();
            xml += fag2.serialize();
            foreach (int hgValue in hgValues)
            {
                xml += "<headGroup>" + hgValue + "</headGroup>\n";
            }
            xml += base.serialize();
            xml += "</lipid>\n";
            return xml;
        }
        
        public override void import(XElement node)
        {
            int fa_counter = 0;
            hgValues.Clear();
            foreach (XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "fattyAcidGroup":
                        if (fa_counter == 0)
                        {
                            fag1.import(child);
                        }
                        else if (fa_counter == 1)
                        {
                            fag2.import(child);
                        }
                        else
                        {   
                            Console.WriteLine("Error, fatty acid");
                            throw new Exception();
                        }
                        ++fa_counter;
                        break;
                        
                    case "headGroup":
                        hgValues.Add(Convert.ToInt32(child.Value.ToString()));
                        break;
                        
                        
                    default:
                        base.import(child);
                        break;
                }
            }
        }
        
        
        public override void add_lipids(DataTable all_lipids, Dictionary<String, DataTable> ddt)
        {
            // check if more than one fatty acids are 0:0
            int check_FattyAcids = 0;
            check_FattyAcids += fag1.faTypes["FAx"] ? 1 : 0;
            check_FattyAcids += fag2.faTypes["FAx"] ? 1 : 0;
            if (check_FattyAcids > 0) return;
            if (hgValues.Count == 0) return;
            
            HashSet<String> used_keys = new HashSet<String>();
            foreach (int fa_l_1 in fag1.carbonCounts)
            {
                int max_db_1 = (fa_l_1 - 1) >> 1;
                foreach (int fa_db_1 in fag1.doubleBondCounts)
                {
                    foreach (KeyValuePair<string, bool> fa_kvp_1 in fag1.faTypes)
                    {
                        if (fa_kvp_1.Value && max_db_1 >= fa_db_1)
                        {
                            FattyAcid fa1 = new FattyAcid(fa_l_1, fa_db_1, fa_kvp_1.Key);
                            if (fa_kvp_1.Key == "FAx")
                            {
                                fa1 = new FattyAcid(0, 0, "FA");
                            }
                            foreach (int fa_l_2 in fag2.carbonCounts)
                            {
                                int max_db_2 = (fa_l_2 - 1) >> 1;
                                foreach (int fa_db_2 in fag2.doubleBondCounts)
                                {
                                    foreach (KeyValuePair<string, bool> fa_kvp_2 in fag2.faTypes)
                                    {
                                        if (fa_kvp_2.Value && max_db_2 >= fa_db_2)
                                        {
                                            FattyAcid fa2 = new FattyAcid(fa_l_2, fa_db_2, fa_kvp_2.Key);
                                            if (fa_kvp_2.Key == "FAx")
                                            {
                                                fa2 = new FattyAcid(0, 0, "FA");
                                            }
                                                                  
                                            List<FattyAcid> sorted_acids = new List<FattyAcid>();
                                            sorted_acids.Add(fa1);
                                            sorted_acids.Add(fa2);
                                            sorted_acids.Sort();
                                            
                                            foreach(int hgValue in hgValues)
                                            {
                                            
                                                String headgroup = headGroupNames[hgValue];
                                                String key = headgroup + " ";
                                                int i = 0;
                                                foreach (FattyAcid fa in sorted_acids)
                                                {
                                                    if (fa.length > 0){
                                                        if (i++ > 0) key += "_";
                                                        key += Convert.ToString(fa.length) + ":" + Convert.ToString(fa.db) + fa.suffix;
                                                    }
                                                }
                                                if (!used_keys.Contains(key))
                                                {
                                                    foreach (KeyValuePair<string, bool> adduct in adducts)
                                                    {
                                                        if (adduct.Value)
                                                        {
                                                            used_keys.Add(key);
                                                            
                                                            DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                                                            MS2Fragment.addCounts(atomsCount, fa1.atomsCount);
                                                            MS2Fragment.addCounts(atomsCount, fa2.atomsCount);
                                                            MS2Fragment.addCounts(atomsCount, ddt[headgroup]);
                                                            String chemForm = LipidCreatorForm.compute_chemical_formula(atomsCount);
                                                            int charge = get_charge_and_add_adduct(atomsCount, adduct.Key);
                                                            double mass = LipidCreatorForm.compute_mass(atomsCount);
                                                            
                                                            foreach (MS2Fragment fragment in MS2Fragments[headgroup])
                                                            {
                                                                if (fragment.fragmentSelected && ((charge < 0 && fragment.fragmentCharge < 0) || (charge > 0 && fragment.fragmentCharge > 0)) && (fragment.restrictions.Count == 0 || fragment.restrictions.Contains(adduct.Key)))
                                                                {
                                                                    DataTable atomsCountFragment = MS2Fragment.createEmptyElementTable(fragment.fragmentElements);
                                                                    foreach (string fbase in fragment.fragmentBase)
                                                                    {
                                                                        switch(fbase)
                                                                        {
                                                                            case "FA1":
                                                                                MS2Fragment.addCounts(atomsCountFragment, fa1.atomsCount);
                                                                                break;
                                                                            case "FA2":
                                                                                MS2Fragment.addCounts(atomsCountFragment, fa2.atomsCount);
                                                                                break;
                                                                            case "PRE":
                                                                                MS2Fragment.addCounts(atomsCountFragment, atomsCount);
                                                                                break;
                                                                            default:
                                                                                break;
                                                                        }
                                                                    }
                                                                    String chemFormFragment = LipidCreatorForm.compute_chemical_formula(atomsCountFragment);
                                                                    double massFragment = LipidCreatorForm.compute_mass(atomsCountFragment);
                                                                    
                                                                
                                                                    DataRow lipid_row = all_lipids.NewRow();
                                                                    lipid_row["Molecule List Name"] = headgroup;
                                                                    lipid_row["Precursor Name"] = key;
                                                                    lipid_row["Precursor Ion Formula"] = chemForm;
                                                                    lipid_row["Precursor Adduct"] = "[M" + adduct.Key + "]";
                                                                    lipid_row["Precursor m/z"] = mass / (double)(Math.Abs(charge));
                                                                    lipid_row["Precursor Charge"] = ((charge > 0) ? "+" : "") + Convert.ToString(charge);
                                                                    lipid_row["Pruduct Name"] = fragment.fragmentName;
                                                                    lipid_row["Pruduct Ion Formula"] = chemFormFragment;
                                                                    lipid_row["Pruduct m/z"] = massFragment / (double)(Math.Abs(fragment.fragmentCharge));
                                                                    lipid_row["Pruduct Charge"] = ((fragment.fragmentCharge > 0) ? "+" : "") + Convert.ToString(fragment.fragmentCharge);
                                                                    all_lipids.Rows.Add(lipid_row);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    public class sl_lipid : lipid
    {
        public List<string> headGroupNames = new List<string>{"Cer", "CerP", "GB3Cer", "GB4Cer", "GD3Cer", "GM3Cer", "GM4Cer", "HexCer", "LacCer", "MIPCer", "MIP2Cer", "PECer", "PICer", "SM", "SPH", "SPH-P", "SPC"};
        public List<int> hgValues;
        public fattyAcidGroup fag;
        public fattyAcidGroup lcb;       
        public int lcb_hydroxyValue;        
        public int fa_hydroxyValue;
    
        public sl_lipid(Dictionary<String, String> all_paths, Dictionary<String, ArrayList> all_fragments)
        {
            lcb = new fattyAcidGroup();
            fag = new fattyAcidGroup();
            hgValues = new List<int>();
            lcb_hydroxyValue = 2;
            fa_hydroxyValue = 0;
            MS2Fragments.Add("Cer", new ArrayList());
            MS2Fragments.Add("CerP", new ArrayList());
            MS2Fragments.Add("GB3Cer", new ArrayList());
            MS2Fragments.Add("GB4Cer", new ArrayList());
            MS2Fragments.Add("GD3Cer", new ArrayList());
            MS2Fragments.Add("GM3Cer", new ArrayList());
            MS2Fragments.Add("GM4Cer", new ArrayList());
            MS2Fragments.Add("HexCer", new ArrayList());
            MS2Fragments.Add("HexCerS", new ArrayList());
            MS2Fragments.Add("LacCer", new ArrayList());
            MS2Fragments.Add("MIPCer", new ArrayList());
            MS2Fragments.Add("MIP2Cer", new ArrayList());
            MS2Fragments.Add("PECer", new ArrayList());
            MS2Fragments.Add("PICer", new ArrayList());
            MS2Fragments.Add("SM", new ArrayList());
            MS2Fragments.Add("SPH", new ArrayList());
            MS2Fragments.Add("SPH-P", new ArrayList());
            MS2Fragments.Add("SPC", new ArrayList());
            
            
            foreach(KeyValuePair<String, ArrayList> kvp in MS2Fragments)
            {
                if (all_paths.ContainsKey(kvp.Key)) pathsToFullImage.Add(kvp.Key, all_paths[kvp.Key]);
                if (all_fragments != null && all_fragments.ContainsKey(kvp.Key))
                {
                    foreach (MS2Fragment fragment in all_fragments[kvp.Key])
                    {
                        MS2Fragments[kvp.Key].Add(new MS2Fragment(fragment));
                    }
                }
            }
        }
    
        public sl_lipid(sl_lipid copy) : base((lipid)copy)
        {
            lcb = new fattyAcidGroup(copy.lcb);
            fag = new fattyAcidGroup(copy.fag);
            lcb_hydroxyValue = copy.lcb_hydroxyValue;
            fa_hydroxyValue = copy.fa_hydroxyValue;
            hgValues = new List<int>();
            foreach (int hgValue in copy.hgValues)
            {
                hgValues.Add(hgValue);
            }
        }
        
        
        public override string serialize()
        {
            string xml = "<lipid type=\"SL\">\n";
            xml += lcb.serialize();
            xml += fag.serialize();
            xml += "<lcbHydroxyValue>" + lcb_hydroxyValue + "</lcbHydroxyValue>\n";
            xml += "<faHydroxyValue>" + fa_hydroxyValue + "</faHydroxyValue>\n";
            foreach (int hgValue in hgValues)
            {
                xml += "<headGroup>" + hgValue + "</headGroup>\n";
            }
            xml += base.serialize();
            xml += "</lipid>\n";
            return xml;
        }
        
        public override void import(XElement node)
        {
            int fa_counter = 0;
            hgValues.Clear();
            foreach (XElement child in node.Elements())
            {
                switch (child.Name.ToString())
                {
                    case "fattyAcidGroup":
                        if (fa_counter == 0)
                        {
                            lcb.import(child);
                        }
                        else if (fa_counter == 1)
                        {
                            fag.import(child);
                        }
                        else
                        {   
                            Console.WriteLine("Error, fatty acid");
                            throw new Exception();
                        }
                        ++fa_counter;
                        break;
                        
                    case "lcbHydroxyValue":
                        lcb_hydroxyValue = Convert.ToInt32(child.Value.ToString());
                        break;
                        
                    case "faHydroxyValue":
                        fa_hydroxyValue = Convert.ToInt32(child.Value.ToString());
                        break;
                        
                    case "headGroup":
                        hgValues.Add(Convert.ToInt32(child.Value.ToString()));
                        break;
                        
                        
                    default:
                        base.import(child);
                        break;
                }
            }
        }
        
        
        public override void add_lipids(DataTable all_lipids, Dictionary<String, DataTable> ddt)
        {
            
            
            HashSet<String> used_keys = new HashSet<String>();
            foreach (int lcb_l in lcb.carbonCounts)
            {
                int max_db_1 = (lcb_l - 1) >> 1;
                foreach (int lcb_db_1 in lcb.doubleBondCounts)
                {
                    if (max_db_1 < lcb_db_1) continue;
                    if (true) // TODO: sphingolipids without fatty acid
                    {
                        foreach (int fa_l in fag.carbonCounts)
                        {
                            if (fa_l < fa_hydroxyValue + 2) continue;
                            int max_db_2 = (fa_l - 1) >> 1;
                            foreach (int fa_db_2 in fag.doubleBondCounts)
                            {
                                if (max_db_2 < fa_db_2) continue;
                                foreach (int hgValue in hgValues)
                                {
                                    String headgroup = headGroupNames[hgValue];
                                    String key = headgroup + " ";
                                    
                                    key += Convert.ToString(lcb_l) + ":" + Convert.ToString(lcb_db_1) + ";" + Convert.ToString(lcb_hydroxyValue);
                                    key += "/";
                                    key += Convert.ToString(fa_l) + ":" + Convert.ToString(fa_db_2) + ";" + Convert.ToString(fa_hydroxyValue);

                                    if (!used_keys.Contains(key))
                                    {
                                        foreach (KeyValuePair<string, bool> adduct in adducts)
                                        {
                                            if (adduct.Value)
                                            {
                                                used_keys.Add(key);
                                                
                                                DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                                                MS2Fragment.addCounts(atomsCount, ddt[headgroup]);
                                                
                                                // long chain base
                                                atomsCount.Rows[0]["Count"] = (int)(atomsCount.Rows[0]["Count"]) + lcb_l;
                                                atomsCount.Rows[1]["Count"] = (int)(atomsCount.Rows[1]["Count"]) + (2 * (lcb_l + lcb_db_1) + 1);
                                                atomsCount.Rows[2]["Count"] = (int)(atomsCount.Rows[2]["Count"]) + lcb_hydroxyValue;
                                                atomsCount.Rows[3]["Count"] = (int)(atomsCount.Rows[3]["Count"]) + 1;
                                                
                                                // add fatty acid
                                                atomsCount.Rows[0]["Count"] = (int)(atomsCount.Rows[0]["Count"]) + fa_l;
                                                atomsCount.Rows[1]["Count"] = (int)(atomsCount.Rows[1]["Count"]) + (2 * fa_l - 1 - 2 * fa_db_2);
                                                atomsCount.Rows[2]["Count"] = (int)(atomsCount.Rows[2]["Count"]) + (1 + fa_hydroxyValue);
                                                
                                                
                                                int charge = get_charge_and_add_adduct(atomsCount, adduct.Key);
                                                String chemForm = LipidCreatorForm.compute_chemical_formula(atomsCount);
                                                double mass = LipidCreatorForm.compute_mass(atomsCount);
                                                
                                                DataRow lipid_row = all_lipids.NewRow();
                                                lipid_row["Molecule List Name"] = headgroup;
                                                lipid_row["Precursor Name"] = key;
                                                lipid_row["Precursor Ion Formula"] = chemForm;
                                                lipid_row["Precursor Adduct"] = chemForm + "[M" + adduct.Key + "]";
                                                lipid_row["Precursor m/z"] = mass;
                                                lipid_row["Precursor Charge"] = ((charge > 0) ? "+" : "") + Convert.ToString(charge);
                                                all_lipids.Rows.Add(lipid_row);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (int hgValue in hgValues)
                        {
                            String headgroup = headGroupNames[hgValue];
                            String key = headgroup + " ";
                            
                            key += Convert.ToString(lcb_l) + ":" + Convert.ToString(lcb_db_1) + ";" + Convert.ToString(lcb_hydroxyValue);

                            if (!used_keys.Contains(key))
                            {
                                foreach (KeyValuePair<string, bool> adduct in adducts)
                                {
                                    if (adduct.Value)
                                    {
                                        used_keys.Add(key);
                                        
                                        DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                                        MS2Fragment.addCounts(atomsCount, ddt[headgroup]);
                                        
                                        // long chain base
                                        atomsCount.Rows[0]["Count"] = (int)(atomsCount.Rows[0]["Count"]) + lcb_l;
                                        atomsCount.Rows[1]["Count"] = (int)(atomsCount.Rows[1]["Count"]) + (2 * (lcb_l + lcb_db_1) + 1);
                                        atomsCount.Rows[2]["Count"] = (int)(atomsCount.Rows[2]["Count"]) + lcb_hydroxyValue;
                                        atomsCount.Rows[3]["Count"] = (int)(atomsCount.Rows[3]["Count"]) + 1;
                                        
                                        
                                        int charge = get_charge_and_add_adduct(atomsCount, adduct.Key);
                                        String chemForm = LipidCreatorForm.compute_chemical_formula(atomsCount);
                                        double mass = LipidCreatorForm.compute_mass(atomsCount);
                                        
                                        DataRow lipid_row = all_lipids.NewRow();
                                        lipid_row["Molecule List Name"] = headgroup;
                                        lipid_row["Precursor Name"] = key;
                                        lipid_row["Precursor Ion Formula"] = chemForm;
                                            lipid_row["Precursor Adduct"] = chemForm + "[M" + adduct.Key + "]";
                                        lipid_row["Precursor m/z"] = mass;
                                        lipid_row["Precursor Charge"] = ((charge > 0) ? "+" : "") + Convert.ToString(charge);
                                        all_lipids.Rows.Add(lipid_row);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }



    [Serializable]
    public class LipidCreatorForm
    {

        public ArrayList lipidTabList;
        public ArrayList registered_lipids;
        public CreatorGUI creatorGUI;
        public Dictionary<String, ArrayList> all_fragments;
        public Dictionary<String, String> all_paths_to_precursor_images;
        public Dictionary<String, DataTable> headgroups;
        public DataTable all_lipids;
        public SkylineToolClient skylineToolClient;
        public bool opened_as_external;
        public string prefix_path = "Tools/LipidCreator/";
        
        public LipidCreatorForm(String pipe)
        {
            opened_as_external = (pipe != null);
            skylineToolClient = opened_as_external ? new SkylineToolClient(pipe, "LipidCreator") : null;
            registered_lipids = new ArrayList();
            all_paths_to_precursor_images = new Dictionary<String, String>();
            all_fragments = new Dictionary<String, ArrayList>();
            headgroups = new Dictionary<String, DataTable>();
            all_lipids = new DataTable();
            all_lipids.Columns.Add("Molecule List Name");
            all_lipids.Columns.Add("Precursor Name");
            all_lipids.Columns.Add("Precursor Ion Formula");
            all_lipids.Columns.Add("Precursor Adduct");
            all_lipids.Columns.Add("Precursor m/z");
            all_lipids.Columns.Add("Precursor Charge");
            all_lipids.Columns.Add("Pruduct Name");
            all_lipids.Columns.Add("Pruduct Ion Formula");
            all_lipids.Columns.Add("Pruduct m/z");
            all_lipids.Columns.Add("Pruduct Charge");
            
            
            int line_counter = 1;
            string ms2fragments_file = (opened_as_external ? prefix_path : "") + "data/ms2fragments.csv";
            if (File.Exists(ms2fragments_file))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(ms2fragments_file))
                    {
                        String line = sr.ReadLine(); // omit titles
                        while((line = sr.ReadLine()) != null)
                        {
                            line_counter++;
                            if (line.Length < 2) continue;
                            if (line[0] == '#') continue;
                            String[] tokens = line.Split(new char[] {','});
                            if (!all_fragments.ContainsKey(tokens[0]))
                            {
                                all_fragments.Add(tokens[0], new ArrayList());
                            }
                            DataTable atomsCount = MS2Fragment.createEmptyElementTable();
                            atomsCount.Rows[0]["Count"] = Convert.ToInt32(tokens[5]);
                            atomsCount.Rows[1]["Count"] = Convert.ToInt32(tokens[6]);
                            atomsCount.Rows[2]["Count"] = Convert.ToInt32(tokens[7]);
                            atomsCount.Rows[3]["Count"] = Convert.ToInt32(tokens[8]);
                            atomsCount.Rows[4]["Count"] = Convert.ToInt32(tokens[9]);
                            atomsCount.Rows[5]["Count"] = Convert.ToInt32(tokens[10]);
                            atomsCount.Rows[6]["Count"] = Convert.ToInt32(tokens[11]);
                            string fragment_file = (opened_as_external ? prefix_path : "") + tokens[2];
                            if (!File.Exists(fragment_file))
                            {
                                Console.WriteLine("Error (" + line_counter + "): MS2 fragment file " + fragment_file + " does not exist or can not be opened.");
                            }
                            
                            all_fragments[tokens[0]].Add(new MS2Fragment(tokens[1], Convert.ToInt32(tokens[3]), fragment_file, true, atomsCount, tokens[4], tokens[12]));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file '" + ms2fragments_file + "' in line '" + line_counter + "' could not be read:");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Error: file " + ms2fragments_file + " does not exist or can not be opened.");
            }
            
            
            
            
            string headgroups_file = (opened_as_external ? prefix_path : "") + "data/headgroups.csv";
            if (File.Exists(headgroups_file))
            {
                line_counter = 1;
                try
                {
                    using (StreamReader sr = new StreamReader(headgroups_file))
                    {
                        String line = sr.ReadLine(); // omit titles
                        while((line = sr.ReadLine()) != null)
                        {
                            line_counter++;
                            if (line.Length < 2) continue;
                            if (line[0] == '#') continue;
                            String[] tokens = line.Split(new char[] {','}); // StringSplitOptions.RemoveEmptyEntries
                            if (tokens.Length != 9) throw new Exception("invalid line in file");
                            headgroups.Add(tokens[0], MS2Fragment.createEmptyElementTable());
                            headgroups[tokens[0]].Rows[0]["Count"] = Convert.ToInt32(tokens[1]);
                            headgroups[tokens[0]].Rows[1]["Count"] = Convert.ToInt32(tokens[2]);
                            headgroups[tokens[0]].Rows[2]["Count"] = Convert.ToInt32(tokens[3]);
                            headgroups[tokens[0]].Rows[3]["Count"] = Convert.ToInt32(tokens[4]);
                            headgroups[tokens[0]].Rows[4]["Count"] = Convert.ToInt32(tokens[5]);
                            headgroups[tokens[0]].Rows[5]["Count"] = Convert.ToInt32(tokens[6]);
                            headgroups[tokens[0]].Rows[6]["Count"] = Convert.ToInt32(tokens[7]);
                            string precursor_file = (opened_as_external ? prefix_path : "") + tokens[8];
                            if (!File.Exists(precursor_file))
                            {
                                Console.WriteLine("Error (" + line_counter + "): precursor file " + precursor_file + " does not exist or can not be opened.");
                            }
                            all_paths_to_precursor_images.Add(tokens[0], precursor_file);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("The file '" + headgroups_file + "' in line '" + line_counter + "' could not be read:");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Error: file " + headgroups_file + " does not exist or can not be opened.");
            }
            
            lipidTabList = new ArrayList(new lipid[] {new cl_lipid(all_paths_to_precursor_images, all_fragments),
                                                      new gl_lipid(all_paths_to_precursor_images, all_fragments),
                                                      new pl_lipid(all_paths_to_precursor_images, all_fragments),
                                                      new sl_lipid(all_paths_to_precursor_images, all_fragments) } );
                                                      
                                                      
            creatorGUI = new CreatorGUI(this);
            creatorGUI.changeTab(1);
        }


        public HashSet<int> parseRange(String text, int lower, int upper)
        {
            return parseRange(text, lower, upper, 0);
        }
        
        // ob_type (Object type): 0 = carbon length, 1 = carbon length odd, 2 = carbon length even, 3 = db length, 4 = hydroxyl length
        public HashSet<int> parseRange(String text, int lower, int upper, int ob_type)
        {
            int odd_even = (ob_type <= 2) ? ob_type : 0;
            if (text.Length == 0) return null;
            foreach (char c in text)
            {
                int ic = (int)c;
                if (!((ic == (int)',') || (ic == (int)'-') || (ic == (int)' ') || (48 <= ic && ic < 58)))
                {
                    return null;
                }
            }
        
            string[] delimitors = new string[] { "," };
            string[] delimitors_range = new string[] { "-" };
            string[] tokens = text.Split(delimitors, StringSplitOptions.None);
            
            HashSet<int> carbonCounts = new HashSet<int>();
            
            for (int i = 0; i < tokens.Length; ++i)
            {
                if (tokens[i].Length == 0) return null;
                string[] range_boundaries = tokens[i].Split(delimitors_range, StringSplitOptions.None);
                if (range_boundaries.Length == 1)
                {
                    int range_start = 0;
                    try 
                    {
                        range_start = Convert.ToInt32(range_boundaries[0]);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                    if (range_start < lower || upper < range_start) return null;
                    if (odd_even == 0 || (odd_even == 1 && (range_start % 2 == 1)) || (odd_even == 2 && (range_start % 2 == 0)))
                    {
                        carbonCounts.Add(range_start);
                    }
                }
                else if (range_boundaries.Length == 2)
                {
                    int range_start = 0;
                    int range_end = 0;
                    try 
                    {
                        range_start = Convert.ToInt32(range_boundaries[0]);
                        range_end = Convert.ToInt32(range_boundaries[1]);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                    if (range_end < range_start || range_start < lower || upper < range_end) return null;
                    for (int l = range_start; l <= range_end; ++l)
                    {
                        if (odd_even == 0 || (odd_even == 1 && (l % 2 == 1)) || (odd_even == 2 && (l % 2 == 0)))
                        {
                            carbonCounts.Add(l);
                        }
                    }
                }
                else return null;
                
            }
            return carbonCounts;
        }

        
        public void assemble_lipids()
        {
            all_lipids.Clear();
            
            foreach (lipid curr_lipid in registered_lipids)
            {
                curr_lipid.add_lipids(all_lipids, headgroups);
            }
        }

        public static String compute_chemical_formula(DataTable elements)
        {
            String chemForm = "";
            foreach (DataRow row in elements.Rows)
            {
                if (Convert.ToInt32(row["Count"]) > 0)
                {
                    chemForm += Convert.ToString(row["Shortcut"]) + (((int)row["Count"] > 1) ? Convert.ToString(row["Count"]) : "");
                }
            }
            return chemForm;
        }

        public static double compute_mass(DataTable elements)
        {
            double mass = 0;
            foreach (DataRow row in elements.Rows)
            {
                mass += Convert.ToDouble(row["Count"]) * Convert.ToDouble(row["mass"]);
            }
            return mass;
        }
        
        public void send_to_Skyline(DataTable dt)
        {
            if (skylineToolClient == null) return;
            
            var header = string.Join(",", new string[]
            {
                "MoleculeGroup",
                "PrecursorName",
                "PrecursorFormula",
                "PrecursorAdduct",
                "PrecursorMz",
                "PrecursorCharge",
                "ProductName",
                "ProductFormula",
                "ProductMz",
                "ProductCharge"
            });
            string pipe_string = header + "\n";
            double max_mass = 0;
            
            foreach (DataRow entry in dt.Rows)
            {
                // Default col order is listname, preName, PreFormula, preAdduct, preMz, preCharge, prodName, ProdFormula, prodAdduct, prodMz, prodCharge
                pipe_string += entry["Molecule List Name"] + ","; // listname
                pipe_string += entry["Precursor Name"] + ","; // preName
                pipe_string += entry["Precursor Ion Formula"] + ","; // PreFormula
                pipe_string += entry["Precursor Adduct"] + ","; // preAdduct
                pipe_string += entry["Precursor m/z"] + ","; // preMz
                max_mass = Math.Max(max_mass, Convert.ToDouble((string)entry["Precursor m/z"]));
                pipe_string += entry["Precursor Charge"] + ","; // preCharge
                pipe_string += entry["Pruduct Name"] + ","; // prodName
                pipe_string += entry["Pruduct Ion Formula"] + ","; // ProdFormula, no prodAdduct
                pipe_string += entry["Pruduct m/z"] + ","; // prodMz
                pipe_string += entry["Pruduct Charge"]; // prodCharge
                pipe_string += "\n";
            }
            try
            {
                //Console.WriteLine(pipe_string);
                
                skylineToolClient.InsertSmallMoleculeTransitionList(pipe_string);
                skylineToolClient.Dispose();
            }
            catch (Exception e)
            {
                //Console.WriteLine(e.ToString());
                MessageBox.Show("An error occured, data could not be send to Skyline, please check if your Skyline parameters allow precursor masses up to " + max_mass + "Da.");
            }
        }
        
        
        public string serialize()
        {
            string xml = "<LipidCreator>\n";
            foreach (lipid curr_lipid in registered_lipids)
            {
                xml += curr_lipid.serialize();
            }
            xml += "</LipidCreator>\n";
            return xml;
        }
        
        public void import(XDocument doc)
        {
        
            var lipids = doc.Descendants("lipid");
            foreach ( var lipid in lipids )
            {
                string lipid_type = lipid.Attribute("type").Value;
                switch (lipid_type)
                {
                    case "CL":
                        cl_lipid cll = new cl_lipid(all_paths_to_precursor_images, null);
                        cll.import(lipid);
                        registered_lipids.Add(cll);
                        break;
                        
                    case "GL":
                        gl_lipid gll = new gl_lipid(all_paths_to_precursor_images, null);
                        gll.import(lipid);
                        registered_lipids.Add(gll);
                        break;
                        
                    case "PL":
                        pl_lipid pll = new pl_lipid(all_paths_to_precursor_images, null);
                        pll.import(lipid);
                        registered_lipids.Add(pll);
                        break;
                        
                    case "SL":
                        sl_lipid sll = new sl_lipid(all_paths_to_precursor_images, null);
                        sll.import(lipid);
                        registered_lipids.Add(sll);
                        break;
                        
                    default:
                        Console.WriteLine("Error global import");
                        throw new Exception();
                }
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            LipidCreatorForm lcf = new LipidCreatorForm((args.Length > 0) ? args[0] : null);
            Application.Run(lcf.creatorGUI);
        }
    }

}


