using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Array : Var
    {
        private dynamic[] raw = new dynamic[] { };
        public Array() { }
        public Array(dynamic[] arr)
        {
            this.raw = arr;
        }

        public static Function last = new Function((self, args) => {
            return self.raw[self.raw.Length - 1];
        });

        public static Function first = new Function((self, args) => {
            return self.raw[0];
        });

        public static Function get = new Function((self, args) => {
            return self.raw[args[0]];
        });

        public static Function set = new Function((self, args) => {
            self.raw[args[0]] = args[1];
            return args[1];
        });

        public static Function insert = new Function((self, args) => {
            var list = self.raw.ToList();
            list.Insert(args[0], args[1]);
            self.raw = list.ToArray();
            return self;
        });

        public static Function insertRange = new Function((self, args) => {
            var list = self.raw.ToList();
            list.InsertRange(args[0], args[1]);
            self.raw = list.ToArray();
            return self;
        });

        public static Function add = new Function((self, args) => {
            var list = self.raw.ToList();
            list.Add(args[0]);
            self.raw = list.ToArray();
            return self;
        });

        public static Function addRange = new Function((self, args) => {
            var list = self.raw.ToList();
            list.AddRange(args[0]);
            self.raw = list.ToArray();
            return self;
        });

        public static Function remove = new Function((self, args) => {
            var list = self.raw.ToList();
            list.RemoveAt(args[0]);
            self.raw = list.ToArray();
            return self;
        });

        public static Function removeRange = new Function((self, args) => {
            var list = self.raw.ToList();
            list.RemoveRange(args[0], args[1]);
            self.raw = list.ToArray();
            return self;
        });

        public static Function total = new Function((self, args) => {
            int counter = 0;
            static int count(dynamic obj)
            {
                if (obj is Array) return obj.total.call(new dynamic[] { });
                else return 1;
            }

            foreach (var item in self.raw) {
                counter += count(item);
            }

            return (Integer)counter;
        });

        public static Function length = new Function((self, args) => {
            return (Integer)self.raw.Length;
        });

        internal new string type = "array";

        public override string ToString()
        {
            string s = "{ ";
            if (this.raw.Length == 0) return "{ }";

            int counter = 1;
            s += this.raw[0].ToString();
            while (counter < this.raw.Length) {
                s = s + ", " + this.raw[counter].ToString();
                counter++;
            }

            return s + " }";
        }
    }
}
