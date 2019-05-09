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

namespace LipidCreator
{
    public enum Molecule {C = 0, C13 = 1, H = 2, H2 = 3, N = 4, N15 = 5, O = 6, O17 = 7, O18 = 8, P = 9, P32 = 10, S = 11, S34 = 12, S33 = 13};

    [Serializable]
    public partial class Element
    {
        public string shortcut;
        public string shortcutNumber;
        public string shortcutIUPAC;
        public string shortcutNomenclature;
        public int position;
        public double mass;
        public bool isHeavy;
        public ArrayList derivatives;
        public Molecule lightOrigin;
        
        public Element(string _shortcut, string _shortcutNumber, string _shortcutIUPAC, string _shortcutNomenclature, int _position, double _mass, bool _isHeavy, ArrayList _derivatives, Molecule _lightOrigin)
        {
            shortcut = _shortcut;
            shortcutNumber = _shortcutNumber;
            shortcutIUPAC = _shortcutIUPAC;
            shortcutNomenclature = _shortcutNomenclature;
            position = _position;
            mass = _mass;
            isHeavy = _isHeavy;
            derivatives = _derivatives;
            lightOrigin = _lightOrigin;
        }
    }
}
