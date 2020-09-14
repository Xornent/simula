using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Syntax {
   
    public class ClassTypeOperation : OperatorStatement {
        public override dynamic Operate(Compilation.RuntimeContext ctx) {
            if (this.Left == null) return Type.Global.Null;
            if (this.Right == null) return Type.Global.Null;
            dynamic evalLeft = Left.Operate(ctx);
            AngleBracketOperation angle = (AngleBracketOperation)this.Right;
            List<Reflection.Base> bases = angle.Operate(ctx);
            
            if(evalLeft is Type.Var) {
                if (((Type.Var)evalLeft) is Type.Class) {
                    List<Type.Var> lst = new List<Type.Var>();
                    foreach (var item in bases) {
                        if (item == null) lst.Add(Type.Global.Null);
                        else if (item is Reflection.Variable) lst.Add(((Reflection.Variable)item).Object);
                        else lst.Add((Type.Var)item);
                    }

                    return ((Type.Class)evalLeft).create_identity(lst.ToArray());
                }
                return Type.Global.Null;
            } else if(evalLeft is Reflection.Variable) {
                if(((Reflection.Variable)evalLeft).Object is Type.Class) {
                    List<Type.Var> lst = new List<Type.Var>();
                    foreach (var item in bases) {
                        if (item == null) lst.Add(Type.Global.Null);
                        else if (item is Reflection.Variable) lst.Add(((Reflection.Variable)item).Object);
                        else lst.Add((Type.Var)item);
                    }

                    return ((Type.Class)(((Reflection.Variable)evalLeft).Object)).create_identity(lst.ToArray());
                }
                return Type.Global.Null;
            } else if(evalLeft is Reflection.AbstractClass) {
                var v = ((Reflection.AbstractClass)evalLeft).Identify(bases, ctx);
                if (v == null) return Type.Global.Null;
                else return v;
            }

            return Type.Global.Null;
        }
    }
}
