﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Simula.Scripting.Token {

    // expose def class string<> : dimension<1> 
    //
    // [1]   : expose
    // [2]   : def
    // [3]   : class
    // [4]   : string
    // [5]   : <
    // [6]   : >
    // [7]   : :
    // [8]   : dimension
    // [9]   : <
    // [10]  : 1
    // [11]  : >

    // module simula.interop
    // def func has_member(string name, bool exposed)
    //     return get_member(name, exposed) != null
    // end
    //
    // [1]   : module
    // [2]   : simula
    // [3]   : .
    // [4]   : interop
    // [5]   : <newline>
    // [6]   : def
    // [7]   : func
    // [8]   : has_member
    // [9]   : (
    // [10]  : string
    // [11]  : name
    // [12]  : ,
    // [13]  : bool
    // [14]  : exposed
    // [15]  : )
    // [16]  : <newline>
    // [17]  : return
    // [18]  : get_member
    // [19]  : (
    // [20]  : name
    // [21]  : ,
    // [22]  : exposed
    // [23]  : )
    // [24]  : !=
    // [25]  : null
    // [26]  : <newline>
    // [27]  : end

    public class Token {
        public Token(string val) {
            this.Value = val;
        }

        public Token(string val, Span loc) {
            this.Value = val;
            this.Location = loc;
        }

        public string Value { get; set; }
        public Span Location { get; set; }
        public bool HasError {
            get {
                return Error != null;
            }
        }
        public TokenizerException? Error { get; set; } 

        public bool ContentEquals(Token t) {
            return this.Value == t.Value;
        }

        public bool IsValidNumberBeginning() {
            string lower = this.Value.ToLower();
            Regex reg = new Regex(@"^[0-9]+\.?[0-9]*$");
            return reg.IsMatch(lower);
        }

        public bool IsValidNameBeginning() {
            string lower = this.Value.ToLower();
            Regex reg = new Regex("^[a-z_]+[a-z0-9_]*$");
            return reg.IsMatch(lower);
        }

        public bool IsValidSymbolBeginning() {
            string lower = this.Value.ToLower();
            bool flag = true;
            foreach (var item in this.Value) {
                if (item != '.' &&
                    item != '[' &&
                    item != ']' &&
                    item != '{' &&
                    item != '}' &&
                    item != '<' &&
                    item != '>' &&
                    item != '=' &&
                    item != '(' &&
                    item != ')' &&
                    item != '*' &&
                    item != '&' &&
                    item != '%' &&
                    item != '!' &&
                    item != '\'' &&
                    item != ':' &&
                    item != ';' &&
                    item != ',' &&
                    item != '+' &&
                    item != '-' &&
                    item != '/' &&
                    item != '|' &&
                    item != '^' &&
                    item != '?') flag = false;
            }
            return flag;
        }

        public static implicit operator string(Token t) {
            return t.Value;
        }

        public static Token LineBreak = new Token("<newline>", new Span());
        public static Token Continue = new Token("...", new Span());

        public override string ToString() {
            if (HasError)
                return this.Value + "  ' " + this.Error?.Message;
            else return this.Value;
        }
    }

    public static class CharExtension {
        public static bool IsAlphabet(this char c) {
            string lower = new string(new char[1] { c }).ToLower();
            Regex reg = new Regex("[a-z_]");
            return reg.IsMatch(lower);
        }
    }
}
