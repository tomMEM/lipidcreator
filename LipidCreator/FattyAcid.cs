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
using System.Collections.Generic;

namespace LipidCreator
{    
    [Serializable]
    public class FattyAcid : IComparable<FattyAcid>
    {
        public int length;
        public int db;
        public int hydroxyl;
        public string suffix;
        public ElementDictionary atomsCount;
        public bool isLCB;
        
        public FattyAcid(int l, int db, int hydro)
        {
        
        }
        
        public FattyAcid(int l, int db, int hydro, string suffix, bool _isLCB = false)
        {
            length = l;
            this.db = db;
            hydroxyl = hydro;
            isLCB = _isLCB;
            atomsCount = MS2Fragment.createEmptyElementDict();
            if (!isLCB)
            {
                this.suffix = (suffix.Length > 2) ? suffix.Substring(2, 1) : "";
                if (length > 0 || db > 0)
                {
                    atomsCount[Molecule.C] = length; // C
                    switch(this.suffix)
                    {
                        case "":
                            atomsCount[Molecule.H] = 2 * length - 1 - 2 * db; // H
                            atomsCount[Molecule.O] = 1 + hydroxyl; // O
                            break;
                        case "p":
                            atomsCount[Molecule.H] = 2 * length - 1 - 2 * db + 2; // H
                            atomsCount[Molecule.O] = hydroxyl; // O
                            break;
                        case "a":
                            atomsCount[Molecule.H] = (length + 1) * 2 - 1 - 2 * db; // H
                            atomsCount[Molecule.O] = hydroxyl; // O
                            break;
                    }
                }
            }
            else 
            {
                // long chain base
                this.suffix = "";
                atomsCount[Molecule.C] = length; // C
                atomsCount[Molecule.H] = (2 * (length - db) + 2); // H
                atomsCount[Molecule.O] = hydroxyl; // O
                atomsCount[Molecule.N] = 1; // N
            }
        }
        
        public FattyAcid(FattyAcid copy)
        {
            length = copy.length;
            db = copy.db;
            isLCB = copy.isLCB;
            hydroxyl = copy.hydroxyl;
            suffix = copy.suffix;
            atomsCount = MS2Fragment.createEmptyElementDict();
            foreach (KeyValuePair<Molecule, int> row in copy.atomsCount) atomsCount[row.Key] = row.Value;
        }
        
        
        
        
        public void merge(FattyAcid copy)
        {
            if (copy.suffix == "x") return;
            
            length += copy.length;
            db += copy.db;
            isLCB |= copy.isLCB;
            hydroxyl += copy.hydroxyl;
            suffix += copy.suffix;
            foreach (KeyValuePair<Molecule, int> row in copy.atomsCount) atomsCount[row.Key] += row.Value;
        }
        
        
        public string ToString(bool fullFormat = true)
        {
            string key = Convert.ToString(length) + ":" + Convert.ToString(db);
            if (isLCB)
            {
                key += ";" + Convert.ToString(hydroxyl);
            }
            else
            {
                if (fullFormat)
                {
                    if (hydroxyl > 0) key += ";" + Convert.ToString(hydroxyl);
                    key += suffix;
                }
            }
            key += LipidCreator.computeHeavyIsotopeLabel(atomsCount);
            return key;
        }
        
        
        
        
        public void updateForHeavyLabeled(ElementDictionary heavyAtomsCount)
        {
            MS2Fragment.updateForHeavyLabeled(atomsCount, heavyAtomsCount);
        }
        

        public int CompareTo(FattyAcid other)
        {
            if (other.suffix.Length > 0 && other.suffix[0] == 'x') return -1;
            if (suffix.Length > 0 && suffix[0] == 'x') return 1;
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
            return obj.length * (LipidCreator.MAX_CARBON_LENGTH + 1) * (LipidCreator.MAX_CARBON_LENGTH + 1) * (LipidCreator.MAX_DB_LENGTH + 1) + obj.db * (LipidCreator.MAX_CARBON_LENGTH + 1) + obj.hydroxyl;
        }
        
        public override bool Equals(FattyAcid obj, FattyAcid obj2)
        { 
            return (obj.length == obj2.length) && (obj.db == obj2.db) && (obj.suffix == obj2.suffix) && (obj.hydroxyl == obj2.hydroxyl);
        }
    }
}
