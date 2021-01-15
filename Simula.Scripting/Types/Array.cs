using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Array : Var
    {
        public dynamic[] raw = new dynamic[] { };
        public Array() : base() { }
        public Array(dynamic[] arr) : base()
        {
            this.raw = arr;
        }

        public static Function last = new Function((self, args) => {
            return self.raw[self.raw.Length - 1];
        }, new List<Pair>() { });

        public static Function first = new Function((self, args) => {
            return self.raw[0];
        }, new List<Pair>() { });

        public static Function get = new Function((self, args) => {
            return self.raw[(int)args[0]];
        }, new List<Pair>() {
            new Pair(new Types.String("index"), new Types.String("sys.int"))
        });

        public static Function set = new Function((self, args) => {
            self.raw[args[0]] = args[1];
            return args[1];
        }, new List<Pair>() { 
            new Pair(new String("index"), new String("sys.int")),
            new Pair(new String("value"), new String("any"))
        });

        public static Function insert = new Function((self, args) => {
            var list = CreateList(self.raw);
            list.Insert(args[0], args[1]);
            self.raw = list.ToArray();
            return self;
        }, new List<Pair>() {
            new Pair(new String("index"), new String("sys.int")),
            new Pair(new String("value"), new String("any"))
        });

        public static Function insertRange = new Function((self, args) => {
            var list = CreateList(self.raw);
            list.InsertRange(args[0], args[1].raw);
            self.raw = list.ToArray();
            return self;
        }, new List<Pair>() {
            new Pair(new String("index"), new String("sys.int")),
            new Pair(new String("range"), new String("sys.array"))
        });

        public static Function add = new Function((self, args) => {
            var list = CreateList(self.raw);
            list.Add(args[0]);
            self.raw = list.ToArray();
            return self;
        }, new List<Pair>() {
            new Pair(new String("index"), new String("sys.int")),
            new Pair(new String("value"), new String("any"))
        });

        public static Function addRange = new Function((self, args) => {
            var list = CreateList(self.raw);
            list.AddRange(args[0].raw);
            self.raw = list.ToArray();
            return self;
        }, new List<Pair>() {
            new Pair(new String("index"), new String("sys.int")),
            new Pair(new String("range"), new String("sys.array"))
        });

        public static Function remove = new Function((self, args) => {
            var list = CreateList(self.raw);
            list.RemoveAt(args[0]);
            self.raw = list.ToArray();
            return self;
        }, new List<Pair>() { 
            new Pair(new String("index"), new String("sys.int"))
        });

        public static Function removeRange = new Function((self, args) => {
            var list = CreateList(self.raw);
            list.RemoveRange(args[0], args[1]);
            self.raw = list.ToArray();
            return self;
        }, new List<Pair>() {
            new Pair(new String("start"), new String("sys.int")),
            new Pair(new String("length"), new String("sys.int"))
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
        }, new List<Pair>() { });

        public static Function length = new Function((self, args) => {
            return (Integer)self.raw.Length;
        }, new List<Pair>() { });

        internal new string type = "sys.array";

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

        public static List<dynamic> CreateList(dynamic[] obj)
        { return obj.ToList(); }
    }
}
