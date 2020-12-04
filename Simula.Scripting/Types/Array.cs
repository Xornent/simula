using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace Simula.Scripting.Types
{
    public class Array : DynamicObject
    {
        private dynamic[] raw = new dynamic[] { };
        public Array() 
        {
            initialize();
        }

        public Array(dynamic[] arr)
        {
            this.raw = arr;
            initialize();
        }

        private void initialize()
        {/*
            this.last = new Function((args) => {
                return raw[raw.Length - 1];
            });

            this.first = new Function((args) => {
                return raw[0];
            });

            this.get = new Function((args) => {
                return raw[args[0]];
            });

            this.set = new Function((args) => {
                raw[args[0]] = args[1];
                return args[1];
            });

            this.insert = new Function((args) => {
                var list = raw.ToList();
                list.Insert(args[0], args[1]);
                raw = list.ToArray();
                return this;
            });

            this.insertRange = new Function((args) => {
                var list = raw.ToList();
                list.InsertRange(args[0], args[1]);
                raw = list.ToArray();
                return this;
            });

            this.add = new Function((args) => {
                var list = raw.ToList();
                list.Add(args[0]);
                raw = list.ToArray();
                return this;
            });

            this.addRange = new Function((args) => {
                var list = raw.ToList();
                list.AddRange(args[0]);
                raw = list.ToArray();
                return this;
            });

            this.remove = new Function((args) => {
                var list = raw.ToList();
                list.RemoveAt(args[0]);
                raw = list.ToArray();
                return this;
            });

            this.removeRange = new Function((args) => {
                var list = raw.ToList();
                list.RemoveRange(args[0], args[1]);
                raw = list.ToArray();
                return this;
            });

            this.total = new Function((args) => {
                int counter = 0;
                static int count(dynamic obj)
                {
                    if (obj is Array) return obj.total.call(new dynamic[] { });
                    else return 1;
                }

                foreach(var item in this.raw) {
                    counter += count(item);
                }

                return (Integer)counter;
            });

            this.length = new Function((args) => {
                return (Integer)this.raw.Length;
            });
            */
        }

        public Function last;
        public Function first;
        public Function get;
        public Function set;
        public Function insert;
        public Function insertRange;
        public Function add;
        public Function addRange;
        public Function remove;
        public Function removeRange;
        public Function total;
        public Function length;

        public Class type = Class.typeArr;
    }
}
