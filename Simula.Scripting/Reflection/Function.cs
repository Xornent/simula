using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Compilation;

namespace Simula.Scripting.Reflection {
    
    public class Function : SourceBase {
        public List<(Base type, string name)> Parameters = new List<(Base, string)>();

        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Documentation DocumentationSource = new Documentation();

        public bool Compiled = false;
        public Function? Conflict;
        public Instance? Parent = null;
        public bool IsStatic { get { return Parent == null; } }

        public dynamic? Invoke(List<Base> pars, Compilation.RuntimeContext ctx) {

            // 执行在 DOM 语句树定义的函数.

            var selection = this;
            bool notMatch = true;

            while (notMatch) {
                notMatch = false;

                if (pars.Count != selection.Parameters.Count) { notMatch = true; continue; }
                int count = 0;
                foreach (var item in pars) {
                    if (item.GetType() != Parameters[count].type.GetType()) notMatch = true;
                }

                if (!notMatch) break;
                if (selection.Conflict == null) break;
                selection = selection.Conflict;
            }

            // 注册函数可以使用的变量

            // 函数的一个 CallStack 中储存了自己的参数作为变量, 以及定义自己的类中的变量作为变量.
            // 它们分别在 pars 和 Parent.Functions 与 Parent.Variables 中

            TemperaryContext tctx = new TemperaryContext();
            foreach (var item in pars) {
                if(item is AbstractClass) {
                    var i = item as AbstractClass;
                    if (i == null) continue;
                    tctx.Classes.OverflowAddAbstractClass(i.Name, i);
                } else if (item is IdentityClass) {
                    var i = item as IdentityClass;
                    if (i == null) continue;
                    tctx.IdentityClasses.OverflowAddIdentityClass(i.Name, i);
                } else if (item is Instance) {
                    var i = item as Instance;
                    if (i == null) continue;
                    tctx.Instances.OverflowAddInstance(i.Name, i);
                } else if (item is Function) {
                    var i = item as Function;
                    if (i == null) continue;
                    tctx.Functions.OverflowAddFunction(i.Name, i);
                } else if (item is Module) {
                    var i = item as Module;
                    if (i == null) continue;
                    tctx.SubModules.OverflowAddModule(i.Name, i);
                } else if (item is Variable) {
                    var i = item as Variable;
                    if (i == null) continue;
                    tctx.Variables.OverflowAddVariable(i.Name, i);
                }
            }

            if (!IsStatic) {

                // 其实通过 IsStatic, 我们已经断言 Parent 不为 null 了, 但是 Visual Studio 会持续警告

                foreach (var item in Parent.Variables) {
                    if (item is AbstractClass) {
                        var i = item as AbstractClass;
                        if (i == null) continue;
                        tctx.Classes.OverflowAddAbstractClass(i.Name, i);
                    } else if (item is IdentityClass) {
                        var i = item as IdentityClass;
                        if (i == null) continue;
                        tctx.IdentityClasses.OverflowAddIdentityClass(i.Name, i);
                    } else if (item is Instance) {
                        var i = item as Instance;
                        if (i == null) continue;
                        tctx.Instances.OverflowAddInstance(i.Name, i);
                    } else if (item is Function) {
                        var i = item as Function;
                        if (i == null) continue;
                        tctx.Functions.OverflowAddFunction(i.Name, i);
                    } else if (item is Module) {
                        var i = item as Module;
                        if (i == null) continue;
                        tctx.SubModules.OverflowAddModule(i.Name, i);
                    } else if (item is Variable) {
                        var i = item as Variable;
                        if (i == null) continue;
                        tctx.Variables.OverflowAddVariable(i.Name, i);
                    }
                }

                foreach (var item in Parent.Functions) {
                    tctx.Functions.OverflowAddFunction(item.Name, item);
                }
            }

            ctx.CallStack.Push(tctx);

            dynamic? result = null;
            if(!notMatch) result = new Syntax.BlockStatement() { Children = selection.Startup }.Operate(ctx);

            ctx.CallStack.Pop();
            return result;
        }
    }
}
