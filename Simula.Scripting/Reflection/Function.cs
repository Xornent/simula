using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;

namespace Simula.Scripting.Reflection {

    public struct NamedValue {
        public NamedValue(string name, Member value) {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }
        public Member Value { get; set; }
    }

    public struct NamedType {
        public NamedType(string name, Class value) {
            this.Name = name;
            this.Type = value;
        }

        public string Name { get; set; }
        public Class Type { get; set; }
    }

    public class Function : Member {
        public Function() : base() {
            this.Type = MemberType.Function;
            this.RuntimeObject = new Type._Function(this);
        }

        /// <summary>
        /// 函数的参数声明表, 是一个特化类和名称的有序对的序列. Class 是一个链接
        /// C# 类型和脚本中定义的类型的结构, 有子类 ClrClass(Type).
        /// </summary>
        public List<NamedType> Parameters { get; internal set; } = new List<NamedType>();

        public List<Syntax.Statement> Startup = new List<Syntax.Statement>();
        public Type._Function? RuntimeObject = null;
        public Instance? Parent = null;
        public bool IsStatic { get { return Parent == null; } }

        public virtual ExecutionResult Invoke(List<Member> parameters, Compilation.RuntimeContext ctx) {

            // 每个函数变量的 Conflict 链中写着与其同名的函数组. 我们要遍历整个函数组, 来选择其
            // 变量类型表与调用传入参数的变量类型表完全相同的函数, 如果没有找到, 返回 Null 常量,
            // 并抛出一个运行时错误: "对于传入的参数 (..., ...) 没有找到一个 ... 函数与它匹配."

            Function? selection = this;
            bool notMatch = true;

            while(notMatch) {
                notMatch = false;
                for(int parameterCounter = 0; parameterCounter<parameters.Count; parameterCounter ++) {
                    if(this.Parameters.Count < parameterCounter+1 ) { notMatch = false; break; }
                    if (!this.Parameters[parameterCounter].Type.IsCompatible(parameters[parameterCounter])) { notMatch = true; break; }
                }

                if (selection?.Conflict == null) break;
                if(notMatch) selection = (Function?)selection.Conflict;
            }

            if (selection == null)
                return new ExecutionResult();

            if (parameters.Count != selection.Parameters.Count) {
                Function func = new Function();
                func.Name = this.Name;
                func.Parent = this.Parent;
                func.ModuleHierarchy = this.ModuleHierarchy;
                func.Documentation = this.Documentation;
                func.Startup = this.Startup;

                int count = 0;
                foreach (var item in selection.Parameters) {
                    count++;
                    if (count > parameters.Count)
                        func.Parameters.Add(item);
                }

                ExecutionResult reconstruct = new ExecutionResult(func, ctx);
                return reconstruct;
            }

            // 注册函数可以使用的变量:
            // 函数的一个 CallStack 中储存了自己的参数作为变量, 以及定义自己的类中的变量作为变量.

            TemperaryContext scope = new TemperaryContext(ctx);
            scope.Name = this.Name;
            int counter = 0;
            foreach (var item in parameters) {
                scope.SetMember(this.Parameters[counter].Name, item);
                counter++;
            }

            if (this.Parent != null)
                foreach (var item in this.Parent.Members) {
                    scope.SetMember(item.Key, ctx.GetMemberByMetadata(item.Value));
                }

            scope.BaseTemperaryContent = ctx.CallStack.Peek();
            ctx.CallStack.Push(scope);

            var result = new Syntax.BlockStatement() { Children = selection.Startup }.Execute(ctx);

            ctx.CallStack.Pop();
            return result;
        }

        public virtual ExecutionResult Invoke(List<NamedValue> parameters, Compilation.RuntimeContext ctx) {
            Function? selection = this;
            bool notMatch = true;

            while (notMatch) {
                notMatch = false;
                foreach (var item in parameters) {
                    int found = this.Parameters.FindIndex((named) =>
                    {
                        if (named.Name == item.Name) return true;
                        else return false;
                    });

                    if (found == -1) { notMatch = true; break; }
                    var compared = this.Parameters[found];
                    if(!compared.Type.IsCompatible(item.Value)) {
                        notMatch = true;
                        break;
                    }
                }

                if (selection?.Conflict == null) break;
                if (notMatch) selection = (Function?)selection.Conflict;
            }

            if (selection == null)
                return new ExecutionResult();

            if(parameters.Count != selection.Parameters.Count) {
                Function func = new Function();
                func.Name = this.Name;
                func.Parent = this.Parent;
                func.ModuleHierarchy = this.ModuleHierarchy;
                func.Documentation = this.Documentation;
                func.Startup = this.Startup;

                foreach (var item in selection.Parameters) {
                    int given = parameters.FindIndex((named) =>
                    {
                        if (named.Name == item.Name) return true;
                        else return false;
                    });

                    if(given == -1) func.Parameters.Add(item);
                }

                ExecutionResult reconstruct = new ExecutionResult(func, ctx);
                return reconstruct;
            }

            TemperaryContext scope = new TemperaryContext(ctx);
            scope.Name = this.Name;
            foreach (var item in parameters) {
                scope.SetMember(item.Name, item.Value);
            }

            if (this.Parent != null)
                foreach (var item in this.Parent.Members) {
                    scope.SetMember(item.Key, ctx.GetMemberByMetadata(item.Value));
                }

            scope.BaseTemperaryContent = ctx.CallStack.Peek();
            ctx.CallStack.Push(scope);

            var result = new Syntax.BlockStatement() { Children = selection.Startup }.Execute(ctx);

            ctx.CallStack.Pop();
            return result;
        }
    }

    public class ClrFunction : Function, ClrMember {
        public ClrFunction(MethodInfo function) : base() {
            Markup.ExposeAttribute? expose = function.GetCustomAttribute<Markup.ExposeAttribute>();
            this.Reflection = function;
            this.Name = expose != null ? expose.Alias : function.Name;
            this.RuntimeObject = null;
            this.RuntimeObject = new Type._Function(this);

            this.Parameters.Clear();
            foreach (var item in function.GetParameters()) {
                NamedType type = new NamedType();
                type.Name = item.Name ?? "<annoymous>";
                type.Type = ClrClass.Create(item.ParameterType);
                this.Parameters.Add(type);
            }
        }

        public new ClrInstance? Parent = null;
        public new bool IsStatic { get { return Parent == null; } }
        MethodInfo? Reflection = null; 
        public override ExecutionResult Invoke(List<Member> parameters, RuntimeContext ctx) {
            try {
                List<object?> objectParameters = new List<object?>();
                foreach (var item in parameters) {
                    switch (item.Type) {
                        case MemberType.Class:
                            objectParameters.Add(((Class)item).RuntimeObject);
                            break;
                        case MemberType.Instance:
                            if(item is ClrInstance) {
                                objectParameters.Add(((ClrInstance)item).Reflection);
                            } else {
                                objectParameters.Add(item);
                            }
                            break;
                        case MemberType.Function:
                            objectParameters.Add(((Function)item).RuntimeObject);
                            break;
                        case MemberType.Unknown:
                            objectParameters.Add(Simula.Scripting.Type.Global.Null);
                            break;
                    }
                }
                var raw = Reflection?.Invoke(Parent?.Reflection, objectParameters.ToArray());
                
                object? result;
                if(raw is Type.Var variable)
                    result = new ClrInstance(variable, ctx);
                else if(raw is Member)
                    result = raw;
                else if(raw is ExecutionResult exec)
                    return exec;
                else result = null;

                if (result == null) return new ExecutionResult();
                else return new ExecutionResult((Member)result, ctx);
            } catch (Exception ex) {
#if DEBUG
                MessageBox.Show(ex.Message + "\n\n" + ex.StackTrace);
#endif
            }

            return new ExecutionResult();
        }

        public override ExecutionResult Invoke(List<NamedValue> parameters, RuntimeContext ctx) {
            return new ExecutionResult();
        }

        public object? GetNative() {
            return this.Reflection;
        }
    }
}
