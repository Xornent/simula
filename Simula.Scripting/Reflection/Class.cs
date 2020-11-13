using Simula.Scripting.Compilation;
using Simula.Scripting.Debugging;
using Simula.Scripting.Reflection.Markup;
using Simula.Scripting.Syntax;
using Simula.Scripting.Type;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;

namespace Simula.Scripting.Reflection {

    public enum OperatorType {
        Binary,
        UnaryLeft,
        UnaryRight
    }

    public struct OperatorRegistry {
        public OperatorRegistry(string method, OperatorType type) {
            this.MethodBinding = method;
            this.Type = type;
        }

        public string MethodBinding { get; set; }
        public OperatorType Type { get; set; }
    }

    public class Class : Member {
        public Class(ref RuntimeContext context) {
            this.Type = MemberType.Class;
            this.RuntimeObject = new _Class(this);
            this.Runtime = context;
        }

        public Class? Inheritage;
        public RuntimeContext Runtime; 
        public _Class? RuntimeObject = null;
        public Syntax.DefinitionBlock? Definition;
        public Function? Startup;

        // 这一组运算符字典是可以自行扩充的, 调用 class.register_operator(string, string, int).
        // 可以注册一个成员函数和符号序列的关联. 并且指定一个可用的运算符类型.

        // 值得注意的是, 一些与语法密切相关的运算符是不可重命名的. 列表如下:
        //   1.  [int count]           索引运算符, 被默认注册到 "_index(...)" 函数
        //   2.  (int param1, ...)     函数参数运算符, 被默认注册到 "_call(...)" 函数
        //   3.  func init(...)        类型初始化函数

        public static Dictionary<string, OperatorRegistry> Operators = new Dictionary<string, OperatorRegistry>()
        {
            { "+", new OperatorRegistry("_add", OperatorType.Binary) },
            { "-", new OperatorRegistry("_minus", OperatorType.Binary) },
            { "*", new OperatorRegistry("_multiply", OperatorType.Binary) },
            { "/", new OperatorRegistry("_divide", OperatorType.Binary) },
            { "&", new OperatorRegistry("_and", OperatorType.Binary) },
            { "&&", new OperatorRegistry("_bitand", OperatorType.Binary) },
            { ".", new OperatorRegistry("_member", OperatorType.Binary) },
            { "|", new OperatorRegistry("_bitor", OperatorType.Binary) },
            { "||",new OperatorRegistry("_or", OperatorType.Binary)},
            { "^", new OperatorRegistry("_bitnot", OperatorType.Binary) },
            { "==",new OperatorRegistry("_equal", OperatorType.Binary) },
            { "<<", new OperatorRegistry("_bitlshift", OperatorType.Binary) },
            { ">>", new OperatorRegistry("_bitrshift", OperatorType.Binary) },
            { "%", new OperatorRegistry("_quotient", OperatorType.Binary) },
            { "<=",new OperatorRegistry("_lte", OperatorType.Binary) },
            { "<", new OperatorRegistry("_lt", OperatorType.Binary) },
            { ">", new OperatorRegistry("_gt", OperatorType.Binary) },
            { ">=", new OperatorRegistry("_gte", OperatorType.Binary) },
            { "!=", new OperatorRegistry("_notequal", OperatorType.Binary) },
            { "**", new OperatorRegistry("_pow", OperatorType.Binary) },
            { "++", new OperatorRegistry("_increment", OperatorType.UnaryLeft) },
            { "--", new OperatorRegistry("_decrement", OperatorType.UnaryLeft) },
            { "!", new OperatorRegistry("_not", OperatorType.UnaryLeft) }
        };

        public virtual Instance CreateInstance(List<Member> parameters, ref RuntimeContext ctx) {
            Instance inst = new Instance(ref ctx);
            inst.Parent = this;
            
            if(this.Inheritage != null)
                inst.ParentalInstance = this.Inheritage.CreateInstance(new List<Member>(), ref ctx);
            
            // 在正常情况下这一句恒为假.
            if (Definition?.Children == null) return inst;

            foreach (var item in Definition.Children) {

                // 值得注意的是, 一个注册进模块中的变量只能是声明在最外层的变量, 假如一个变量声明在函数的里层
                // 或者一个类型的里层, 在这里是不会被解析的. 当类型实例化的时候会解析类型中声明的函数和变量,
                // 在函数被执行的时候会解析局部的变量. (事实上, 局部的变量不需要声明, 在赋值语句调用时就会自行添加)

                if (item is DefinitionBlock) {
                    var defs = item as DefinitionBlock;
                    if (defs == null) continue;
                    var locModule = new Locator(true);
                    switch (defs.Type) {
                        case DefinitionType.Constant:
                            ExecutionResult? result = defs.ConstantValue?.Execute(ctx);
                            if (result == null) break;
                            if (result.Result.Name != "") {
                                ExecutionResult renamed = new ExecutionResult(result.Result, ctx.MaximumAllocatedPointer, ctx);
                                ctx.MaximumAllocatedPointer++;
                                inst.SetMember(defs.ConstantName ?? "", result.Result);
                            } else {
                                result.Result.ModuleHierarchy = locModule;
                                result.Result.Name = defs.ConstantName ?? "";
                                inst.SetMember(result.Result.Name, result.Result);
                            }
                            break;
                        case DefinitionType.Function:
                            Reflection.Function func = new Reflection.Function(ref this.Runtime);
                            func.ModuleHierarchy = locModule;
                            func.Name = defs.FunctionName?.Value ?? "";
                            func.Startup = defs.Children;
                            func.Parent = inst;

                            foreach (var par in defs.FunctionParameters) {
                                var typeResult = par.Type?.Execute(ctx).Result;
                                if (typeResult?.Type != MemberType.Class) {
                                    ctx.Errors.Add(new RuntimeError(1, "函数的参数类型不是可用的类型.", defs.FunctionName?.Location));
                                    break;
                                }
                                func.Parameters.Add(new NamedType(par.Name?.Value ?? "", (Class)typeResult));
                            }

                            ExecutionResult funcResult = new ExecutionResult(func, ctx);
                            inst.SetMember(func.Name, func);
                            break;
                        case DefinitionType.Class:
                            ctx.Errors.Add(new RuntimeError(3, "类型中嵌套的类型和模块中嵌套的类型是无差别的, 禁止在类型中进行嵌套", defs.ClassName?.Location));
                            break;
                        default:
                            break;
                    }
                }
            }

            // 在这里我们调用类型的初始化方法.

            var initializer = inst.GetMember("_init");
            if(!initializer.IsNull())
                if(initializer.Result.Type == MemberType.Function) {
                    ((Function)initializer.Result).Invoke(parameters,ref ctx);
                }

            return inst;
        } 

        public bool IsCompatible(Member target) {
            switch (target.Type) {
                case MemberType.Class:
                    if (this.Name == "class") return true;
                    else return false;
                case MemberType.Instance:

                    // TODO: 这里还没有考虑类的继承.

                    if (this.ModuleHierarchy.Connect(".") == ((Instance)target).Parent?.ModuleHierarchy.Connect(".") &&
                        this.Name == target.Name) return true;
                    return false;
                case MemberType.Function:
                    if (this.Name == "func") return true;
                    else return false;
                case MemberType.Module:
                    return false;
                case MemberType.Unknown:

                    // 事实上, 约定 MemberType.Unknown 的对象只是 Null; 而每个对象均可以为 Null

                    return true;
                default:
                    break;
            }

            return false;
        }
    }

    public class ClrClass : Class, ClrMember {
        public ClrClass(ref RuntimeContext ctx): base(ref ctx){
            this.Reflection = null;
            this.RuntimeObject = null;
            this.RuntimeObject = new _Class(this);
        }

        public static ClrClass Create(System.Type type, ref RuntimeContext ctx) {
            var attribute = type.GetCustomAttribute<ExposeAttribute>();
            if (attribute == null) return new ClrClass(ref ctx);

            if (attribute.Alias == "<global>") {
                return new ClrClass(ref ctx);
            } else {
                ClrClass cls = new ClrClass(ref ctx);
                ExecutionResult result = new ExecutionResult(cls, ctx);
                cls.Name = attribute.Alias;
                if (Registry.ContainsKey(type))
                    return Registry[type];
                cls.Reflection = type;
                Registry.Add(type, cls);
                if (type.BaseType == null) return cls;
                if (type.BaseType == typeof(Reflect)) {
                    cls.Inheritage = null;
                } else if (type.IsSubclassOf(typeof(Reflect))) {
                    if (Registry.ContainsKey(type.BaseType))
                        cls.Inheritage = Registry[type.BaseType];
                    cls.Inheritage = ClrClass.Create(type.BaseType, ref ctx);
                }

                return cls;
            }
        }

        
        public new ClrClass? Inheritage { get; internal set; }
        public System.Type? Reflection { get; internal set; }
        public override Instance CreateInstance(List<Member> parameters, ref RuntimeContext ctx) {
            var type = this.Reflection;
            ClrInstance instance = new ClrInstance(ref this.Runtime);
            instance.Parent = this;
            if(this.Inheritage != null)
                instance.ParentalInstance = this.Inheritage.CreateInstance(new List<Member>(), ref ctx);
            instance.Reflection = Activator.CreateInstance(this.Reflection ?? typeof(Var));

            // 解析这个类型中的所有字段和函数(非静态的), 并把它转换到包装抽象的
            // ClrInstance 和 ClrFunction.

            foreach (var item in (type ?? typeof(Var)).GetMethods()) {
                if (item.IsStatic) continue;
                var attribute = item.GetCustomAttribute<ExposeAttribute>();
                if (attribute == null) continue;

                var funcs = new ClrFunction(item, ref this.Runtime);
                funcs.Parent = instance;
                funcs.Name = attribute.Alias;
                
                var result = new ExecutionResult(funcs, this.Runtime);
                instance.SetMember(attribute.Alias, result.Result);
            }

            foreach (var item in (type ?? typeof(Var)).GetFields()) {
                if (item.IsStatic) continue;
                var attribute = item.GetCustomAttribute<ExposeAttribute>();
                if (attribute == null) continue;

                var field = new ClrInstance(item, ref ctx);
                field.Name = attribute.Alias;
                
                var result = new ExecutionResult(field, this.Runtime);
                instance.SetMember(attribute.Alias, result.Result);
            }

            var initializer = instance.GetMember("_init");
            if(!initializer.IsNull())
                if(initializer.Result.Type == MemberType.Function) {
                    ((Function)initializer.Result).Invoke(parameters, ref ctx);
                } 
            return instance;
        }

        public static Dictionary<System.Type, ClrClass> Registry = new Dictionary<System.Type, ClrClass>();
        public object? GetNative() {
            return this.Reflection;
        }
    }
}