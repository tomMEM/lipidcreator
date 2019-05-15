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
SOFTWARE. IF YOU VIOLATE THE COPYRIGHT, SMALL YELLOW CAPSULE-SHAPED CREATURES
WITH BLUE BIBS WILL COME AND SLAP YOU WITH BANANAS.
*/


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LipidCreator
{    
    
    public enum Context {NoContext, InLineComment, InLongComment, InQuote};
    public enum MatchWords {NoMatch, LineCommentStart, LineCommentEnd, LongCommentStart, LongCommentEnd, Quote};
    
    [Serializable]
    public class Parser
    {
        [Serializable]
        public class ExtendedLinkedList<T> : LinkedList<T>
        {
            public T PopFirst()
            {
                if (First == null) return default(T);
                T current = First.Value;
                RemoveFirst();
                return current;
            }
            
            public T PopLast()
            {
                if (Last == null) return default(T);
                T current = Last.Value;
                RemoveLast();
                return current;
            }
        }
        
        
        
        // DP stands for dynamic programming
        public class DPNode
        {
            public long ruleIndex1;
            public long ruleIndex2;
            public DPNode left;
            public DPNode right;
            
            public DPNode(long _rule1, long _rule2, DPNode _left, DPNode _right)
            {
                ruleIndex1 = _rule1;
                ruleIndex2 = _rule2;
                left = _left;
                right = _right;
            }
        }
        
        
        
        
        // this class is dedicated to have an efficient sorted set class storing
        // values within 0..n-1 and fast sequencial iterator
        public class Bitfield
        {
            public ulong[] field;
            public ulong[] superfield;
            static readonly ulong multiplicator = 0x022fdd63cc95386dUL;
            static readonly public int[] positions = new int[64] // from http://chessprogramming.wikispaces.com/De+Bruijn+Sequence+Generator
            { 
                0, 1,  2, 53,  3,  7, 54, 27, 4, 38, 41,  8, 34, 55, 48, 28,
                62,  5, 39, 46, 44, 42, 22,  9, 24, 35, 59, 56, 49, 18, 29, 11,
                63, 52,  6, 26, 37, 40, 33, 47, 61, 45, 43, 21, 23, 58, 17, 10,
                51, 25, 36, 32, 60, 20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12
            };
            
            public Bitfield(int length)
            {
                int l = 1 + ((length + 1) >> 6);
                int s = 1 + ((l + 1) >> 6);
                field = new ulong[l];
                superfield = new ulong[s];
                for (int i = 0; i < l; ++i) field[i] = 0;
                for (int i = 0; i < s; ++i) superfield[i] = 0;
            }
            
            public void set(int pos)
            {
                field[pos >> 6] |= (ulong)(1UL << (pos & 63));
                superfield[pos >> 12] |= (ulong)(1UL << ((pos >> 6) & 63));
            }
            
            
            public bool isSet(int pos)
            {
                return ((field[pos >> 6] >> (pos & 63)) & 1UL) == 1UL;
            }
            
            
            public bool isNotSet(int pos)
            {
                return ((field[pos >> 6] >> (pos & 63)) & 1UL) == 0UL;
            }
            
            
            public System.Collections.Generic.IEnumerable<int> getBitPositions()
            {
                int spre = 0;
                foreach (ulong cell in superfield)
                {
                    ulong sv = cell;
                    while (sv != 0)
                    {
                        // algorithm for getting least significant bit position
                        ulong sv1 = (ulong)((long)sv & -(long)sv);
                        int pos = spre + positions[(ulong)(sv1 * multiplicator) >> 58];
                        
                        ulong v = field[pos];
                        while (v != 0)
                        {
                            // algorithm for getting least significant bit position
                            ulong v1 = (ulong)((long)v & -(long)v);
                            yield return unchecked(pos << 6) + positions[unchecked((v1 * multiplicator) >> 58)];
                            v &= v - 1;
                        }
                        
                        sv &= sv - 1;
                    }
                    spre += 64;
                }
            }
        }
        
        
        
        
    
        public class TreeNode
        {
            public long ruleIndex;
            public TreeNode left;
            public TreeNode right;
            public char terminal;
            public bool fireEvent;
            
            public TreeNode(long _rule, bool _fireEvent)
            {
                ruleIndex = _rule;
                left = null;
                right = null;
                terminal = '\0';
                fireEvent = _fireEvent;
            }
            
            
            
            public string getText(TreeNode node = null)
            {
                if (node == null) node = this;
                if (node.terminal == '\0') return getText(node.left) + ((node.right != null) ? getText(node.right) : "");
                    
                return Convert.ToString(node.terminal);
            }
        }
        
        
        
        public uint nextFreeRuleIndex;
        public Dictionary<char, HashSet<long>> TtoNT;
        public Dictionary<long, HashSet<long>> NTtoNT;
        public char quote;
        [NonSerialized]
        public TreeNode parseTree;
        public bool wordInGrammar;
        public Dictionary<long, string> NTtoRule;
        public BaseParserEventHandler parserEventHandler;
        public const int SHIFT = 32;
        public const long MASK = (1L << SHIFT) - 1;
    
    
        public Parser(BaseParserEventHandler _parserEventHandler, string grammarFilename, char _quote = '"')
        {
            nextFreeRuleIndex = 1;
            TtoNT = new Dictionary<char, HashSet<long>>();
            NTtoNT = new Dictionary<long, HashSet<long>>();
            NTtoRule = new Dictionary<long, string>();
            quote = _quote;
            parserEventHandler = _parserEventHandler;
            parseTree = null;
            wordInGrammar = false;
            
            
            if (File.Exists(grammarFilename))
            {
                Dictionary<string, long> ruleToNT = new Dictionary<string, long>();
                
                string grammar = File.ReadAllText(grammarFilename) + "\n";
                int grammarLength = grammar.Length;
                
                
                // deleting comments to prepare for splitting in rules
                StringBuilder sb = new StringBuilder();
                Context currentContext = Context.NoContext;
                int currentPosition = 0;
                for (int i = 0; i < grammarLength - 1; ++i)
                {
                    MatchWords match = MatchWords.NoMatch;
                
                    if (grammar[i] == '/' && grammar[i + 1] == '/') match = MatchWords.LineCommentStart;
                    else if (grammar[i] == '\n') match = MatchWords.LineCommentEnd;
                    else if (grammar[i] == '/' && grammar[i + 1] == '*') match = MatchWords.LongCommentStart;
                    else if (grammar[i] == '*' && grammar[i + 1] == '/') match = MatchWords.LongCommentEnd;
                    else if (grammar[i] == quote) match = MatchWords.Quote;
                    
                    if (match != MatchWords.NoMatch)
                    {
                        switch (currentContext)
                        {
                            case Context.NoContext:
                                switch (match)
                                {
                                    case MatchWords.LongCommentStart:
                                        sb.Append(grammar.Substring(currentPosition, i - currentPosition));
                                        currentContext = Context.InLongComment;
                                        break;
                                        
                                    case MatchWords.LineCommentStart:
                                        sb.Append(grammar.Substring(currentPosition, i - currentPosition));
                                        currentContext = Context.InLineComment;
                                        break;
                                        
                                    case MatchWords.Quote:
                                        currentContext = Context.InQuote;
                                        break;
                                        
                                    default:
                                        break;
                                } 
                                break;
                                
                                
                            case Context.InQuote:
                                if (match == MatchWords.Quote)
                                {
                                    currentContext = Context.NoContext;
                                }
                                break;
                                
                                
                            case Context.InLineComment:
                                if (match == MatchWords.LineCommentEnd)
                                {
                                    currentContext = Context.NoContext;
                                    currentPosition = i + 1;
                                }
                                break;
                                
                            case Context.InLongComment:
                                if (match == MatchWords.LongCommentEnd)
                                {
                                    currentContext = Context.NoContext;
                                    currentPosition = i + 2;
                                }
                                break;
                        }
                    }
                }
                if (currentContext == Context.NoContext)
                {
                    sb.Append(grammar.Substring(currentPosition, grammarLength - currentPosition));
                }
                else
                {
                    throw new Exception("Error: corrupted grammar, ends either in comment or quote");
                }
                grammar = strip(sb.ToString().Replace("\n", ""), ' ');
                if (grammar[grammar.Length - 1] != ';')
                {
                    Console.WriteLine(grammar);
                    throw new Exception("Error: corrupted grammar, last rule has no termininating sign");
                }
                ArrayList rules = splitString(grammar, ';', quote);
                
                if (rules.Count < 1) return;
                
                ArrayList grammarNameRule = splitString((string)rules[0], ' ', quote);
                if (!grammarNameRule[0].Equals("grammar"))
                {
                    throw new Exception("Error: first rule must start with the keyword 'grammar'");
                }
                else if (grammarNameRule.Count != 2)
                {
                    throw new Exception("Error: incorrect first rule");
                }
                   
                   
                // interpret the rules and create the structure for parsing
                int lineCount = 0;
                foreach (string ruleLine in rules)
                {
                    if (lineCount++ == 0) continue;
                
                    ArrayList tokens_level_1 = new ArrayList();
                    foreach (string t in splitString(ruleLine, ':', quote)) tokens_level_1.Add(strip(t, ' '));
                    if (tokens_level_1.Count != 2) throw new Exception("Error: corrupted token in grammar rule: '" + ruleLine + "'");
                    
                    if (splitString((string)tokens_level_1[0], ' ', quote).Count > 1)
                    {
                        throw new Exception("Error: several rule names on left hand side in grammar rule: '" + ruleLine + "'");
                    }

                    string rule = (string)tokens_level_1[0];
                    
                    ArrayList products = new ArrayList();
                    foreach (string p in splitString((string)tokens_level_1[1], '|', quote)) products.Add(strip(p, ' '));
                    
                    if (!ruleToNT.ContainsKey(rule)) ruleToNT.Add(rule, getNextFreeRuleIndex());
                    long newRuleIndex = ruleToNT[rule];
                    
                    if (!NTtoRule.ContainsKey(newRuleIndex)) NTtoRule.Add(newRuleIndex, rule);
                    
                    
                    foreach (string product in products)
                    {
                        LinkedList<string> nonTerminals = new LinkedList<string>();
                        ExtendedLinkedList<long> nonTerminalRules = new ExtendedLinkedList<long>();
                        foreach (string NT in splitString(product, ' ', quote))
                        {
                            string stripedNT = strip(NT, ' ');
                            nonTerminals.AddLast(stripedNT);
                        }
                        
                        string NTFirst = nonTerminals.First.Value;
                        if (nonTerminals.Count > 1 || !isTerminal(NTFirst, quote) || NTFirst.Length != 3)
                        {
                        
                            foreach (string nonTerminal in nonTerminals)
                            {
                                if (isTerminal(nonTerminal, quote))
                                {
                                    nonTerminalRules.AddLast(addTerminal(nonTerminal));
                                }
                                else
                                {
                                    if (!ruleToNT.ContainsKey(nonTerminal))
                                    {
                                        ruleToNT[nonTerminal] = getNextFreeRuleIndex();
                                    }
                                    nonTerminalRules.AddLast(ruleToNT[nonTerminal]);
                                }
                            }
                        }
                        else
                        {
                            char c = NTFirst[1];
                            if (!TtoNT.ContainsKey(c)) TtoNT[c] = new HashSet<long>();
                            TtoNT[c].Add(newRuleIndex);
                        }
                        
                        
                        // more than two rules, insert intermediate rule indexes
                        while (nonTerminalRules.Count > 2)
                        {
                            long ruleIndex2 = nonTerminalRules.PopLast();
                            long ruleIndex1 = nonTerminalRules.PopLast();
                            
                            long key = computeRuleKey(ruleIndex1, ruleIndex2);
                            long nextIndex = getNextFreeRuleIndex();
                            if (!NTtoNT.ContainsKey(key)) NTtoNT.Add(key, new HashSet<long>());
                            NTtoNT[key].Add(nextIndex);
                            nonTerminalRules.AddLast(nextIndex);
                        }    
                        
                            
                        // two product rules
                        if (nonTerminalRules.Count == 2)
                        {
                            long ruleIndex1 = nonTerminalRules.PopFirst();
                            long ruleIndex2 = nonTerminalRules.PopFirst();
                            long key = computeRuleKey(ruleIndex1, ruleIndex2);
                            if (!NTtoNT.ContainsKey(key)) NTtoNT.Add(key, new HashSet<long>());
                            NTtoNT[key].Add(newRuleIndex);
                        }
                        // only one product rule
                        else if (nonTerminalRules.Count == 1)
                        {
                            long ruleIndex1 = nonTerminalRules.First.Value;
                            if (ruleIndex1 == newRuleIndex) throw new Exception("Error: corrupted token in grammar: rule '" + rule + "' is not allowed to refer soleley to itself.");
                            
                            if (!NTtoNT.ContainsKey(ruleIndex1)) NTtoNT.Add(ruleIndex1, new HashSet<long>());
                            NTtoNT[ruleIndex1].Add(newRuleIndex);
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Error: file '" + grammarFilename + "' does not exist or can not be opened.");
            }
            
            
            
            HashSet<char> keys = new HashSet<char>(TtoNT.Keys);
            foreach(char c in keys)
            {
                HashSet<long> rules = new HashSet<long>(TtoNT[c]);
                TtoNT[c].Clear();
                foreach(long rule in rules)
                {
                    foreach (long p in collectBackwards(rule))
                    {
                        long key = computeRuleKey(p, rule);
                        TtoNT[c].Add(key);
                    }
                }
            }
            
            
            HashSet<long> keysNT = new HashSet<long>(NTtoNT.Keys);
            foreach(long r in keysNT)
            {
                HashSet<long> rules = new HashSet<long>(NTtoNT[r]);
                foreach(long rule in rules)
                {
                    foreach (long p in collectBackwards(rule)) NTtoNT[r].Add(p);
                }
            }
        }
        
        
        
        
        public long getNextFreeRuleIndex()
        {
            if (nextFreeRuleIndex <= MASK) return nextFreeRuleIndex++;
            throw new Exception("Error: grammar is too big.");
        }
        
        
        
        
        public long computeRuleKey(long ruleIndex1, long ruleIndex2)
        {
            return (ruleIndex1 << SHIFT) | ruleIndex2;
        }
        
        
        
        
        public static ArrayList splitString(string text, char separator, char quote)
        {
            bool inQuote = false;
            ArrayList tokens = new ArrayList();
            string token = "";
            
            foreach (char c in text)
            {
                if (!inQuote)
                {
                    if (c == separator)
                    {
                        if (token.Length > 0) tokens.Add(token);
                        token = "";
                    }
                    else
                    {
                        if (c == quote) inQuote = !inQuote;
                        token += c;
                    }
                }
                else
                {
                    if (c == quote) inQuote = !inQuote;
                    token += c;
                }
            }
                    
            if (token.Length > 0) tokens.Add(token);
            if (inQuote) throw new Exception("Error: corrupted token in grammar");
            
            return tokens;
        }
        
        
        
        
        public static string strip(string text, char stripChar)
        {
            if (text.Length > 0)
            {
                int st = 0;
                while (st < text.Length - 1 && text[st] == stripChar) ++st;
                text = text.Substring(st, text.Length - st);
            }
            
            if (text.Length > 0)
            {
                int en = 0;
                while (en < text.Length - 1 && text[text.Length - 1 - en] == stripChar) ++en;
                text = text.Substring(0, text.Length - en);
            }
            return text;
        }
        
        
        
        
        // checking if string is terminal
        public static bool isTerminal(string productToken, char quote)
        {
            int cnt = 0;
            foreach(char c in productToken) cnt += (c == quote) ? 1 : 0;
            if (cnt != 0 && cnt != 2) throw new Exception("Error: corrupted token in grammar");
            
            if (cnt == 0) return false;
        
            if (productToken[0] == quote && productToken[productToken.Length - 1] == quote && productToken.Length > 2) return true;

            throw new Exception("Error: corrupted token in grammar");
        }
        
        
        
        
        // splitting the whole terminal in a tree structure where characters of terminal are the leafs and the inner nodes are added non terminal rules
        public long addTerminal(string text)
        {
            text = strip(text, quote);
            ExtendedLinkedList<long> terminalRules = new ExtendedLinkedList<long>();
            foreach (char c in text)
            {
                if (!TtoNT.ContainsKey(c)) TtoNT.Add(c, new HashSet<long>());
                long nextIndex = getNextFreeRuleIndex();
                TtoNT[c].Add(nextIndex);
                terminalRules.AddLast(nextIndex);
            }
            while (terminalRules.Count > 1)
            {
                long ruleIndex2 = terminalRules.PopLast();
                long ruleIndex1 = terminalRules.PopLast();
                
                long nextIndex = getNextFreeRuleIndex();
                
                long key = computeRuleKey(ruleIndex1, ruleIndex2);
                if (!NTtoNT.ContainsKey(key)) NTtoNT.Add(key, new HashSet<long>());
                NTtoNT[key].Add(nextIndex);
                terminalRules.AddLast(nextIndex);
            }
            return terminalRules.First.Value;
        }
        
        
        
        // expanding singleton rules, e.g. S -> A, A -> B, B -> C
        public LinkedList<long> collectBackwards(long ruleIndex)
        {
            LinkedList<long> collection = new LinkedList<long>();
            collection.AddLast(ruleIndex);
            LinkedListNode<long> current = collection.First;
            while (current != null)
            {
                long currentIndex = current.Value;
                if (NTtoNT.ContainsKey(currentIndex))
                {
                    foreach (long previousIndex in NTtoNT[currentIndex]) collection.AddLast(previousIndex);
                }
                current = current.Next;
            }
            return collection;
        }
        
        
        
        
        
        public LinkedList<long> collectBackwards(long childRuleIndex, long parentRuleIndex)
        {
            if (!NTtoNT.ContainsKey(childRuleIndex)) return null;
            LinkedList<long> collection;
            
            foreach (long previousIndex in NTtoNT[childRuleIndex])
            {
                if (previousIndex == parentRuleIndex)
                {
                    collection = new LinkedList<long>();
                    return collection;
                }
                else if (NTtoNT.ContainsKey(previousIndex))
                {
                    collection = collectBackwards(previousIndex, parentRuleIndex);
                    if (collection != null)
                    {
                        collection.AddLast(previousIndex);
                        return collection;
                    }
                }
            }
            return null;
        }
            
            
            
            
        
        public void raiseEvents(TreeNode node = null)
        {
            if (node != null)
            {
                string nodeRuleName = node.fireEvent ? NTtoRule[node.ruleIndex] : "";
                if (node.fireEvent) parserEventHandler.handleEvent(nodeRuleName + "_pre_event", node);
                
                if (node.terminal == '\0') // node.terminal is != null when node is leaf
                {
                    raiseEvents(node.left);
                    if (node.right != null) raiseEvents(node.right);
                }
                    
                if (node.fireEvent) parserEventHandler.handleEvent(nodeRuleName + "_post_event", node);
            }
            else
            {
                if (parseTree != null) raiseEvents(parseTree);
            }
        }
        
        
    
        // filling the syntax tree including events
        public void fillTree(TreeNode node, DPNode dpNode)
        {
           
            // checking and extending nodes for single rule chains
            long key = (dpNode.left != null) ? computeRuleKey(dpNode.ruleIndex1, dpNode.ruleIndex2) : dpNode.ruleIndex2;
            LinkedList<long> mergedRules = collectBackwards(key, node.ruleIndex);
            if (mergedRules != null)
            {
                foreach (long ruleIndex in mergedRules)
                {
                    node.left = new TreeNode(ruleIndex, NTtoRule.ContainsKey(ruleIndex));
                    node = node.left;
                }
            }
            
            
            if (dpNode.left != null) // null => leaf
            {
                node.left = new TreeNode(dpNode.ruleIndex1, NTtoRule.ContainsKey(dpNode.ruleIndex1));
                node.right = new TreeNode(dpNode.ruleIndex2, NTtoRule.ContainsKey(dpNode.ruleIndex2));
                fillTree(node.left, dpNode.left);
                fillTree(node.right, dpNode.right);
            }
            else
            {
                // I know, it is not 100% clean to store the character in an integer
                // especially when it is not the dedicated attribute for, but the heck with it!
                node.terminal = (char)dpNode.ruleIndex1;
            }
        }
        
        
        
        
        // re-implementation of Cocke-Younger-Kasami algorithm
        public void parse(string textToParse)
        {
            wordInGrammar = false;
            parseTree = null;
            int n = textToParse.Length;
            // dp stands for dynamic programming, nothing else
            Dictionary<long, DPNode>[][] dpTable = new Dictionary<long, DPNode>[n][];
            // Ks is a lookup, which fields in the dpTable are filled
            Bitfield[] Ks = new Bitfield[n];
            
            
            
            for (int i = 0; i < n; ++i)
            {
                dpTable[i] = new Dictionary<long, DPNode>[n - i];
                Ks[i] = new Bitfield(n - 1);
                for (int j = 0; j < n - i; ++j) dpTable[i][j] = new Dictionary<long, DPNode>();
            }
            
            for (int i = 0; i < n; ++i)
            {
                char c = textToParse[i];
                if (!TtoNT.ContainsKey(c)) return;
                
                foreach (long ruleIndex in TtoNT[c])
                {
                    long newKey = ruleIndex >> SHIFT;
                    long oldKey = ruleIndex & MASK;
                    DPNode dpNode = new DPNode((long)c, oldKey, null, null);
                    dpTable[i][0][newKey] =  dpNode;
                    Ks[i].set(0);
                }
            }
            
            for (int i = 1; i < n; ++i)
            {
                int im1 = i - 1;
                for (int j = 0; j < n - i; ++j)
                {
                    Dictionary<long, DPNode>[] D = dpTable[j];
                    Dictionary<long, DPNode> Di = D[i];
                    int jp1 = j + 1;
                    
                    foreach(int k in Ks[j].getBitPositions())
                    {
                        if (k >= i) break;
                        if (Ks[jp1 + k].isNotSet(im1 - k)) continue;
                        
                        foreach (KeyValuePair<long, DPNode> indexPair1 in D[k])
                        {
                            foreach (KeyValuePair<long, DPNode> indexPair2 in dpTable[jp1 + k][im1 - k])
                            {
                                long key = computeRuleKey(indexPair1.Key, indexPair2.Key);
                                if (!NTtoNT.ContainsKey(key)) continue;
                                
                                DPNode content = new DPNode(indexPair1.Key, indexPair2.Key, indexPair1.Value, indexPair2.Value);
                                Ks[j].set(i);
                                foreach (long ruleIndex in NTtoNT[key])
                                {
                                    Di[ruleIndex] = content;
                                }
                            }
                        }
                    }
                }
            }
            
            
            
            if (dpTable[0][n - 1].ContainsKey(1)) // 1 => start rule
            {
                wordInGrammar = true;
                parseTree = new TreeNode(1, NTtoRule.ContainsKey(1));
                fillTree(parseTree, dpTable[0][n - 1][1]);
            }
            
        }
    }    
}
