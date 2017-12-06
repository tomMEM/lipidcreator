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
    public class TestTransitionList
    {
        public static void Assert(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new Exception("Assert failed: " + message);
            }
        }
        
        public static void Assert(int i1, int i2, string message = "")
        {
            if (i1 != i2)
            {
                throw new Exception("Assert failed: " + message);
            }
        }
        
        public static void Assert(double d1, double d2, string message = "")
        {
            if (Math.Abs(d1 - d2) > 1e-4)
            {
                throw new Exception("Assert failed: " + message);
            }
        }
    
        [STAThread]
        public static void Main(string[] args)
        {
            LipidCreator lcf = new LipidCreator(null);
                        
            try {
                //LPC	LPC 18:0	C26H54O7NP	[M+H]	        524.3710668	1	HG(PC)	C5H15O4NP	184.0733215	1
                Console.WriteLine("checking LPC	LPC 18:0	C26H54O7NP	[M+H]	        524.3710668	1	HG(PC)	C5H14O4NP	184.0733215	1");
                PLLipid lpc = new PLLipid(lcf);
                lpc.headGroupNames.Add("LPC"); // set PC
                lpc.adducts["+H"] = true;   // set adduct
                
                
                lpc.fag1.carbonCounts.Add(18); // set first fatty acid parameters
                lpc.fag1.doubleBondCounts.Add(0);
                lpc.fag1.hydroxylCounts.Add(0);
                lpc.fag2.faTypes["FA"] = false; // unset second fatty acid
                lpc.fag2.faTypes["FAx"] = true;
                
                // unset all fragments except HG(PC)
                foreach (KeyValuePair<string, HashSet<string>> fragments in lpc.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in lpc.negativeFragments) fragments.Value.Clear();
                lpc.positiveFragments["LPC"].Add("HG(PC)");
            
                
                lcf.registeredLipids.Add(lpc);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "1st LPC transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("HG(PC)"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("LPC"), "1st LPC category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("LPC 18:0"), "1st LPC precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C26H54O7NP"), "1st LPC precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+H]"), "1st LPC precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 524.3710668, "1st LPC precursor mass: " + row[LipidCreator.PRECURSOR_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), 1, "1st LPC precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("HG(PC)"), "1st LPC product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C5H14O4NP"), "1st LPC product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 184.0733215, "1st LPC product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), 1, "1st LPC product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }
                
                
                
                
                
                //LPC	LPC 18:0	C26H54O7NP	[M+HCOO]	568.3619932	-1	FA	C18H35O2	283.2642541	-1
                Console.WriteLine("checking LPC	LPC 18:0	C26H54O7NP	[M+HCOO]	568.3619932	-1	FA	C18H35O2	283.2642541	-1");
                // unset all fragments except FA
                foreach (KeyValuePair<string, HashSet<string>> fragments in lpc.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in lpc.negativeFragments) fragments.Value.Clear();
                lpc.negativeFragments["LPC"].Add("FA");
                lpc.adducts["+H"] = false;   // unset adduct
                lpc.adducts["+HCOO"] = true;   // set adduct
                
                lcf.registeredLipids.Clear();
                lcf.registeredLipids.Add(lpc);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "2nd LPC transition length");
                
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("FA"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("LPC"), "2nd LPC category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("LPC 18:0"), "2nd LPC precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C26H54O7NP"), "2nd LPC precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+HCOO]"), "2nd LPC precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 568.3619932, "2nd LPC precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "2nd LPC precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("FA"), "2nd LPC product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C18H36O2"), "2nd LPC product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 283.2642541, "2nd LPC product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "2nd LPC product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }
                
                
                //LPC	LPC 18:0	C26H54O7NP	[M+CH3COO]	582.3776432	-1	FA	C18H35O2	283.2642541	-1
                Console.WriteLine("checking LPC	LPC 18:0	C26H54O7NP	[M+CH3COO]	582.3776432	-1	FA	C18H35O2	283.2642541	-1");
                // unset all fragments except HG(PC)
                lpc.adducts["+HCOO"] = false;   // unset adduct
                lpc.adducts["+CH3COO"] = true;   // set adduct
                
                lcf.registeredLipids.Clear();
                lcf.registeredLipids.Add(lpc);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "3nd LPC transition length");
                
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("FA"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("LPC"), "3nd LPC category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("LPC 18:0"), "3nd LPC precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C26H54O7NP"), "3nd LPC precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+CH3COO]"), "3nd LPC precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 582.3776432, "3nd LPC precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "3nd LPC precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("FA"), "3nd LPC product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C18H36O2"), "3nd LPC product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 283.2642541, "3nd LPC product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "3nd LPC product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }
                
                
                
                //PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	P              O3P         78.95905447	-1
                //PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	FA1	       C16H31O2    255.2329539	-1
                //PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	FA2	       C18H35O2    283.2642541	-1
                //PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	FA2' + O       C19H38O7P   409.2360643	-1
                Console.WriteLine("checking PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	P              HO3P         78.95905447	-1");
                Console.WriteLine("checking PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	FA1	       C16H31O2    255.2329539	-1");
                Console.WriteLine("checking PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	FA2	       C18H35O2    283.2642541	-1");
                Console.WriteLine("checking PA	PA 16:0_18:0	C37H73O8P	[M-H]	675.4970301	-1	FA2' + O       C19H38O7P   409.2360643	-1");
                lcf.registeredLipids.Clear();
                PLLipid pa = new PLLipid(lcf);
                pa.headGroupNames.Add("PA"); // set class
                pa.adducts["-H"] = true;   // set adduct
                
                pa.fag1.carbonCounts.Add(16); // set first fatty acid parameters
                pa.fag1.doubleBondCounts.Add(0);
                pa.fag1.hydroxylCounts.Add(0);
                pa.fag1.faTypes["FA"] = true;
                pa.fag2.carbonCounts.Add(18); // set second fatty acid parameters
                pa.fag2.doubleBondCounts.Add(0);
                pa.fag2.hydroxylCounts.Add(0);
                pa.fag2.faTypes["FA"] = true;
                
                // unset all fragments except
                foreach (KeyValuePair<string, HashSet<string>> fragments in pa.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in pa.negativeFragments) fragments.Value.Clear();
                pa.negativeFragments["PA"].Add("P");
                pa.negativeFragments["PA"].Add("FA1");
                pa.negativeFragments["PA"].Add("FA2");
                pa.negativeFragments["PA"].Add("FA2' + O");
                
                lcf.registeredLipids.Add(pa);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 5, "PA transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("P"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("PA"), "1st PA category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("PA 16:0_18:0"), "1st PA precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C37H73O8P"), "1st PA precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M-H]"), "1st PA precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 675.4970301, "1st PA precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "1st PA precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("P"), "1st PA product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("HO3P"), "1st PA product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 78.95905447, "1st PA product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "1st PA product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                    
                    else if (row[LipidCreator.PRODUCT_NAME].Equals("FA1"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("PA"), "2nd PA category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("PA 16:0_18:0"), "2nd PA precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C37H73O8P"), "2nd PA precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M-H]"), "2nd PA precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 675.4970301, "2nd PA precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "2nd PA precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("FA1"), "2nd PA product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C16H32O2"), "2nd PA product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 255.2329539, "2nd PA product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "2nd PA product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                    
                    else if (row[LipidCreator.PRODUCT_NAME].Equals("FA2"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("PA"), "3rd PA category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("PA 16:0_18:0"), "3rd PA precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C37H73O8P"), "3rd PA precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M-H]"), "3rd PA precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 675.4970301, "3rd PA precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "3rd PA precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("FA2"), "3rd PA product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C18H36O2"), "3rd PA product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 283.2642541, "3rd PA product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "3rd PA product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                    
                    else if (row[LipidCreator.PRODUCT_NAME].Equals("FA2' + O"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("PA"), "4th PA category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("PA 16:0_18:0"), "4th PA precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C37H73O8P"), "4th PA precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M-H]"), "4th PA precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 675.4970301, "4th PA precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "4th PA precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("FA2' + O"), "4th PA product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C19H39O7P"), "4th PA product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 409.2360643, "4th PA product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "4th PA product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }    
                
                
                
                
                
                
                //TG	TG 14:0_16:0_18:0	C51H98O6	[M+NH4]	824.7701668	1	FA2	C16H31O      239.2369421	1
                //TG	TG 14:0_16:0_18:0	C51H98O6	[M+NH4]	824.7701668	1	FA2'	C35H67O4     551.5033873	1c
                
                
                
                
                
                
                //CL	CL 14:0_16:0_18:0_20:0	C77H150O17P2	[M-H]	1408.027552	-1	FA2	C16H31O2	255.2329539	-1
                Console.WriteLine("checking CL	CL 14:0_16:0_18:0_20:0	C77H150O17P2	[M-H]	1408.027552	-1	FA2	C16H31O2	255.2329539	-1");
                lcf.registeredLipids.Clear();
                PLLipid cl = new PLLipid(lcf);
                cl.isCL = true;
                cl.headGroupNames.Add("CL"); // set class
                cl.adducts["-H"] = true;   // set adduct
                
                cl.fag1.carbonCounts.Add(14); // set first fatty acid parameters
                cl.fag1.doubleBondCounts.Add(0);
                cl.fag1.hydroxylCounts.Add(0);
                cl.fag1.faTypes["FA"] = true;
                cl.fag2.carbonCounts.Add(16); // set second fatty acid parameters
                cl.fag2.doubleBondCounts.Add(0);
                cl.fag2.hydroxylCounts.Add(0);
                cl.fag2.faTypes["FA"] = true;
                cl.fag3.carbonCounts.Add(18); // set third fatty acid parameters
                cl.fag3.doubleBondCounts.Add(0);
                cl.fag3.hydroxylCounts.Add(0);
                cl.fag3.faTypes["FA"] = true;
                cl.fag4.carbonCounts.Add(20); // set fourth fatty acid parameters
                cl.fag4.doubleBondCounts.Add(0);
                cl.fag4.hydroxylCounts.Add(0);
                cl.fag4.faTypes["FA"] = true;
                
                // unset all fragments except
                foreach (KeyValuePair<string, HashSet<string>> fragments in cl.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in cl.negativeFragments) fragments.Value.Clear();
                cl.negativeFragments["CL"].Add("FA2");
                
                lcf.registeredLipids.Add(cl);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "1st CL transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("FA2"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("CL"), "1st CL category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("CL 14:0_16:0_18:0_20:0"), "1st CL precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C77H150O17P2"), "1st CL precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M-H]"), "1st CL precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 1408.027552, "1st CL precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "1st CL precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("FA2"), "1st CL product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C16H32O2"), "1st CL product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 255.2329539, "1st CL product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "1st CL product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }
                
                
                
                
                
                //CL	CL 14:0_16:0_18:0_20:0	C77H150O17P2	[M-2H]	703.5101375	-2	FA2	C16H31O2	255.2329539	-1
                Console.WriteLine("checking CL	CL 14:0_16:0_18:0_20:0	C77H150O17P2	[M-2H]	703.5101375	-2	FA2	C16H31O2	255.2329539	-1");
                lcf.registeredLipids.Clear();
                cl.adducts["-H"] = false;   // unset adduct
                cl.adducts["-2H"] = true;   // set adduct
                
                lcf.registeredLipids.Add(cl);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "2nd CL transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("FA2"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("CL"), "2nd CL category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("CL 14:0_16:0_18:0_20:0"), "2nd CL precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C77H150O17P2"), "2nd CL precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M-2H]"), "2nd CL precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 703.5101375, "2nd CL precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -2, "2nd CL precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("FA2"), "2nd CL product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C16H32O2"), "2nd CL product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 255.2329539, "2nd CL product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "2nd CL product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }    
                
                
                
                
                //Cer	Cer 18:1;2/12:0	C30H59NO3	[M+H]	482.4568	1	W''	C18H34N	        264.2486	1
                Console.WriteLine("checking Cer	Cer 18:1;2/12:0	C30H59NO3	[M+H]	482.4568	1	W''	C18H34N	        264.2486	1");
                lcf.registeredLipids.Clear();
                SLLipid sl = new SLLipid(lcf);
                sl.headGroupNames.Add("Cer"); // set slass
                sl.adducts["+H"] = true;   // set adduct
                
                sl.lcb.carbonCounts.Add(18); // set first fatty acid parameters
                sl.lcb.doubleBondCounts.Add(1);
                sl.lcb.hydroxylCounts.Add(2);
                sl.lcb.faTypes["FA"] = true;
                sl.fag.carbonCounts.Add(12); // set second fatty acid parameters
                sl.fag.doubleBondCounts.Add(0);
                sl.fag.hydroxylCounts.Add(0);
                sl.fag.faTypes["FA"] = true;
                
                // unset all fragments except
                foreach (KeyValuePair<string, HashSet<string>> fragments in sl.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in sl.negativeFragments) fragments.Value.Clear();
                sl.positiveFragments["Cer"].Add("W''");
                
                lcf.registeredLipids.Add(sl);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "1st Cer transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("W''"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("Cer"), "1st Cer category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("Cer 18:1;2/12:0"), "1st Cer precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C30H59O3N"), "1st Cer precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+H]"), "1st Cer precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 482.4568, "1st Cer precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), 1, "1st Cer precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("W''"), "1st Cer product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C18H33N"), "1st Cer product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 264.2686, "1st Cer product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), 1, "1st Cer product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }
                
                
                
                
                
                //Cer	Cer 18:1;2/12:0	C30H59NO3	[M-H]	480.4422	-1	S	C14H27ON	224.2019881	-1
                Console.WriteLine("checking Cer	Cer 18:1;2/12:0	C30H59NO3	[M-H]	480.4422	-1	S	C14H27ON	224.2019881	-1");
                lcf.registeredLipids.Clear();
                sl.adducts["+H"] = true;   // unset adduct
                sl.adducts["-H"] = true;   // set adduct
                
                foreach (KeyValuePair<string, HashSet<string>> fragments in sl.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in sl.negativeFragments) fragments.Value.Clear();
                sl.negativeFragments["Cer"].Add("S");
                
                lcf.registeredLipids.Add(sl);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "2nd Cer transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("S"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("Cer"), "2nd Cer category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("Cer 18:1;2/12:0"), "2nd Cer precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C30H59O3N"), "2nd Cer precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M-H]"), "2nd Cer precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 480.4422, "2nd Cer precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), -1, "2nd Cer precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("S"), "2nd Cer product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C14H27ON"), "2nd Cer product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 224.2019881, "2nd Cer product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), -1, "2nd Cer product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }
                
                
                
                
                //SM	SM 18:1;2/12:0	C35H71O6N2P	[M+H]	647.4884423	1	HG(PC)	C5H14O4NP	184.0733215	1
                //SM	SM 18:1;2/12:0	C35H71O6N2P	[M+H]	647.4884423	1	W''	C18H33N	        264.2686	1
                Console.WriteLine("checking SM	SM 18:1;2/12:0	C35H71O6N2P	[M+H]	647.4884423	1	HG(PC)	C5H14O4NP	184.0733215	1");
                Console.WriteLine("checking SM	SM 18:1;2/12:0	C35H71O6N2P	[M+H]	647.4884423	1	W''	C18H33N	        264.2686	1");
                lcf.registeredLipids.Clear();
                SLLipid sm = new SLLipid(lcf);
                sm.headGroupNames.Add("SM"); // set class
                sm.adducts["+H"] = true;   // set adduct
                
                sm.lcb.carbonCounts.Add(18); // set long chain base parameters
                sm.lcb.doubleBondCounts.Add(1);
                sm.lcb.hydroxylCounts.Add(2);
                sm.lcb.faTypes["FA"] = true;
                sm.fag.carbonCounts.Add(12); // set fatty acid parameters
                sm.fag.doubleBondCounts.Add(0);
                sm.fag.hydroxylCounts.Add(0);
                sm.fag.faTypes["FA"] = true;
                
                foreach (KeyValuePair<string, HashSet<string>> fragments in sm.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in sm.negativeFragments) fragments.Value.Clear();
                sm.positiveFragments["SM"].Add("HG(PC)");
                sm.positiveFragments["SM"].Add("W''");
                
                lcf.registeredLipids.Add(sm);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 3, "SM transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("HG(PC)"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("SM"), "1st SM category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("SM 18:1;2/12:0"), "1st SM precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C35H71O6N2P"), "1st SM precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+H]"), "1st SM precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 647.51225172, "1st SM precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), 1, "1st SM precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("HG(PC)"), "1st SM product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C5H14O4NP"), "1st SM product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 184.0733215, "1st SM product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), 1, "1st SM product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                    
                    else if (row[LipidCreator.PRODUCT_NAME].Equals("W''"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("SM"), "2nd SM category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("SM 18:1;2/12:0"), "2nd SM precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C35H71O6N2P"), "2nd SM precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+H]"), "2nd SM precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 647.51225172, "2nd SM precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), 1, "2nd SM precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("W''"), "2nd SM product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C18H33N"), "2nd SM product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 264.2686, "2nd SM product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), 1, "2nd SM product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }    

                
                
                
                
                //SPC	SPC 17:1;2	C22H47O5N2P	[M+H]	451.3295	1	HG(PC)	C5H14O4NP	184.0733215	1
                //SPC	SPC 17:1;2	C22H47O5N2P	[M+H]	451.3295	1	W''	C17H31N	        250.2529	1
                Console.WriteLine("checking SPC	SPC 17:1;2	C22H47O5N2P	[M+H]	451.3295	1	HG(PC)	C5H14O4NP	184.0733215	1");
                Console.WriteLine("checking SPC	SPC 17:1;2	C22H47O5N2P	[M+H]	451.3295	1	W''	C17H31N	        250.2529	1");
                lcf.registeredLipids.Clear();
                SLLipid spc = new SLLipid(lcf);
                spc.headGroupNames.Add("SPC"); // set class
                spc.adducts["+H"] = true;   // set adduct
                
                spc.lcb.carbonCounts.Add(17); // set long chain base parameters
                spc.lcb.doubleBondCounts.Add(1);
                spc.lcb.hydroxylCounts.Add(2);
                spc.lcb.faTypes["FA"] = true;
                spc.fag.carbonCounts.Add(12); // set fatty acid parameters
                spc.fag.doubleBondCounts.Add(0);
                spc.fag.hydroxylCounts.Add(0);
                spc.fag.faTypes["FA"] = false;
                spc.fag.faTypes["FAx"] = true;
                
                // unset all fragments except
                foreach (KeyValuePair<string, HashSet<string>> fragments in spc.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in spc.negativeFragments) fragments.Value.Clear();
                spc.positiveFragments["SPC"].Add("HG(PC)");
                spc.positiveFragments["SPC"].Add("W''");
                
                lcf.registeredLipids.Add(spc);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 3, "SPC transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("HG(PC)"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("SPC"), "1st SPC category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("SPC 17:1;2"), "1st SPC precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C22H47O5N2P"), "1st SPC precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+H]"), "1st SPC precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 451.3295, "1st SPC precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), 1, "1st SPC precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("HG(PC)"), "1st SPC product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C5H14O4NP"), "1st SPC product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 184.0733215, "1st SPC product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), 1, "1st SPC product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                    
                    else if (row[LipidCreator.PRODUCT_NAME].Equals("W''"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("SPC"), "2nd SPC category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("SPC 17:1;2"), "2nd SPC precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C22H47O5N2P"), "2nd SPC precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+H]"), "2nd SPC precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 451.3295, "2nd SPC precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), 1, "2nd SPC precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("W''"), "2nd SPC product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C17H31N"), "2nd SPC product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 250.2529, "2nd SPC product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), 1, "2nd SPC product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }

                
                
                
                
                //SPH	SPH 17:1;2	C17H35O2N	[M+H]	286.2741	1	W''	C17H31N	250.2529	1
                Console.WriteLine("checking SPH	SPH 17:1;2	C17H35O2N	[M+H]	286.2741	1	W''	C17H31N	250.2529	1");
                lcf.registeredLipids.Clear();
                SLLipid sph = new SLLipid(lcf);
                sph.headGroupNames.Add("SPH"); // set class
                sph.adducts["+H"] = true;   // set adduct
                
                sph.lcb.carbonCounts.Add(17); // set long chain base parameters
                sph.lcb.doubleBondCounts.Add(1);
                sph.lcb.hydroxylCounts.Add(2);
                sph.lcb.faTypes["FA"] = true;
                sph.fag.faTypes["FA"] = false;
                sph.fag.faTypes["FAx"] = true;
                
                // unset all fragments except
                foreach (KeyValuePair<string, HashSet<string>> fragments in sph.positiveFragments) fragments.Value.Clear();
                foreach (KeyValuePair<string, HashSet<string>> fragments in sph.negativeFragments) fragments.Value.Clear();
                sph.positiveFragments["SPH"].Add("W''");
                
                lcf.registeredLipids.Add(sph);
                lcf.assembleLipids();
                Assert(lcf.transitionList.Rows.Count == 2, "SPH transition length");
                
                foreach (DataRow row in lcf.transitionList.Rows)
                {
                    if (row[LipidCreator.PRODUCT_NAME].Equals("W''"))
                    {
                        // precursor
                        Assert(row[LipidCreator.MOLECULE_LIST_NAME].Equals("SPH"), "SPH category: " + row[LipidCreator.MOLECULE_LIST_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NAME].Equals("SPH 17:1;2"), "SPH precursor name: " + row[LipidCreator.PRECURSOR_NAME]);
                        Assert(row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA].Equals("C17H35O2N"), "SPH precursor formula: " + row[LipidCreator.PRECURSOR_NEUTRAL_FORMULA]);
                        Assert(row[LipidCreator.PRECURSOR_ADDUCT].Equals("[M+H]"), "SPH precursor adduct: " + row[LipidCreator.PRECURSOR_ADDUCT]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRECURSOR_MZ]), 286.2741, "SPH precursor mass");
                        Assert(Convert.ToInt32(row[LipidCreator.PRECURSOR_CHARGE]), 1, "SPH precursor charge: " + row[LipidCreator.PRECURSOR_CHARGE]);
                        // product
                        Assert(row[LipidCreator.PRODUCT_NAME].Equals("W''"), "SPH product name: " + row[LipidCreator.PRODUCT_NAME]);
                        Assert(row[LipidCreator.PRODUCT_NEUTRAL_FORMULA].Equals("C17H31N"), "SPH product formula: " + row[LipidCreator.PRODUCT_NEUTRAL_FORMULA]);
                        Assert(Convert.ToDouble(row[LipidCreator.PRODUCT_MZ]), 250.2529, "SPH product mass: " + row[LipidCreator.PRODUCT_MZ]);
                        Assert(Convert.ToInt32(row[LipidCreator.PRODUCT_CHARGE]), 1, "SPH product charge: " + row[LipidCreator.PRODUCT_CHARGE]);
                    }
                }
                
                Console.WriteLine("Test passed, no errors found");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }
}