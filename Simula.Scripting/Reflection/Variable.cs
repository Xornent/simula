using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace Simula.Scripting.Reflection {

    public class Variable : CompiledBase {
        public string Name = "";
        public string FullName = "";
        public List<string> ModuleHirachy = new List<string>();

        public bool Hidden = false;
        public bool Writable = true;

        public Type.Var Object = new Simula.Scripting.Type._Null();
        public Variable? Conflict;

        public dynamic Invoke(List<Base> param) {
            Type.Function? func;
            Variable? va = this;
            while(va?.Conflict != null) {
                if(va.Object is Type.Function) {
                    func = va.Object as Type.Function;
                    if (func == null) { va = va.Conflict; continue; }
                    try {
                        if (func.function == null) { va = va.Conflict;  continue; }
                        if(func.function != null) {
                            System.Reflection.MethodInfo d = func.function;
                            object? obj = d.Invoke(va.Object, param.ToArray());
                            if (obj == null) return Type.Global.Null;
                            else return obj;
                        }
                    } catch {
                        va = va.Conflict; continue;
                    }
                }

                va = va.Conflict;
            }

            return Type.Global.Null;
        }

        public dynamic InvokeMember(string member, List<Base> param) {
            Type.Var? v;
            Variable va = this;
            while(va.Conflict !=null) {
                v = va.Object;
                System.Type t = v.GetType();
                List<System.Type> paramTypes = new List<System.Type>();
                foreach (var item in param) {
                    paramTypes.Add(item.GetType());
                }

                System.Reflection.MethodInfo? mi = t.GetMethod(member, paramTypes.ToArray());
                if (mi == null) { 
                } else {
                    object? obj = mi.Invoke(v, param.ToArray());
                    if (obj == null) return Type.Global.Null;
                    return obj;
                }

                va = va.Conflict;
            }

            return Type.Global.Null;
        }

        public Type.Var GetMember(string member) {
            Type.Var? v;
            Variable va = this;
            while (va.Conflict != null) {
                v = va.Object;
                System.Type t = v.GetType();

                var prop = t.GetProperty(member);
                if (prop == null) {
                } else {
                    var obj = prop.GetValue(v);
                    if (obj == null) return Type.Global.Null;
                    else if (obj is Type.Var) return (Type.Var)obj;
                    else return Type.Global.Null;
                }
                
                va = va.Conflict;
            }

            return Type.Global.Null;
        }
    }
}
