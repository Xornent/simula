using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Simula.Scripting.Analysis
{
    // the analysis environment accomplishes the following tasks with an input of syntax trees.
    // (1) type registration  : all the runtime types will be added to a virtual array of record
    //                          in preperation for the code generation and other services.
    // (2) completeness check : the analysis enviroment check whether the available types is complete
    //                          that all objects are defined using the given set of types. it will 
    //                          announce the system for type incompleteness, typing errors and warnings
    //                          of inadequate types (such as dangling references)
    // (3) type inference     : the environment will infer the result type of executions

    public class AnalysisEnvironment
    {
        public AnalysisEnvironment()
        {
            this.Scopes.Add(this.BaselineScope);
        }

        private List<AnalysisScope> Scopes = new List<AnalysisScope>();
        private AnalysisScope BaselineScope = new AnalysisScope();
        private SyntaxTree Current;
        public IDictionary<Guid, Record.IRecord> Objects = new Dictionary<Guid, Record.IRecord>();

        // load method import the analysis environment declarations using a syntax tree. syntax trees
        // represents all of the uncompiled definition blocks that are available in the environment.

        public void Load(SyntaxTree syntaxTree)
        {
            this.Current = syntaxTree;

            RegisterSystem();
            Register(syntaxTree);

            VisitDefinition();
        }

        // the first step of syntax analysis is to register all the named items in the analysis scope.
        // by the end of this stage, naming information will be fully loaded to the scope and is ready
        // for the completion check and type inference.                                                   (I)

        private void RegisterSystem()
        {
            var uint8 = new Interop.ClrTypeRecord(typeof(byte), "uint8");
            var uint8Add = new Interop.PlaceholderRecord("add:o2");
            var uint8Sub = new Interop.PlaceholderRecord("substract:o2");
            var uint8Mul = new Interop.PlaceholderRecord("multiply:o2");
            var uint8Div = new Interop.PlaceholderRecord("divide:o2");
            var uint8Mod = new Interop.PlaceholderRecord("mod:o2");
            var uint8Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var uint8IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var uint8Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var uint8DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(uint8, uint8Add);
            RegisterMemberFunction(uint8, uint8Sub);
            RegisterMemberFunction(uint8, uint8Mul);
            RegisterMemberFunction(uint8, uint8Div);
            RegisterMemberFunction(uint8, uint8Mod);
            RegisterMemberFunction(uint8, uint8Inc);
            RegisterMemberFunction(uint8, uint8IncR);
            RegisterMemberFunction(uint8, uint8Dec);
            RegisterMemberFunction(uint8, uint8DecR);
            SetObject(uint8.Oid, uint8);
            this.BaselineScope.Registry.Add(uint8.Symbol, uint8);

            var uint16 = new Interop.ClrTypeRecord(typeof(ushort), "uint16");
            var uint16Add = new Interop.PlaceholderRecord("add:o2");
            var uint16Sub = new Interop.PlaceholderRecord("substract:o2");
            var uint16Mul = new Interop.PlaceholderRecord("multiply:o2");
            var uint16Div = new Interop.PlaceholderRecord("divide:o2");
            var uint16Mod = new Interop.PlaceholderRecord("mod:o2");
            var uint16Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var uint16IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var uint16Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var uint16DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(uint16, uint16Add);
            RegisterMemberFunction(uint16, uint16Sub);
            RegisterMemberFunction(uint16, uint16Mul);
            RegisterMemberFunction(uint16, uint16Div);
            RegisterMemberFunction(uint16, uint16Mod);
            RegisterMemberFunction(uint16, uint16Inc);
            RegisterMemberFunction(uint16, uint16IncR);
            RegisterMemberFunction(uint16, uint16Dec);
            RegisterMemberFunction(uint16, uint16DecR);
            SetObject(uint16.Oid, uint16);
            this.BaselineScope.Registry.Add(uint16.Symbol, uint16);

            var uint32 = new Interop.ClrTypeRecord(typeof(uint), "uint32");
            var uint32Add = new Interop.PlaceholderRecord("add:o2");
            var uint32Sub = new Interop.PlaceholderRecord("substract:o2");
            var uint32Mul = new Interop.PlaceholderRecord("multiply:o2");
            var uint32Div = new Interop.PlaceholderRecord("divide:o2");
            var uint32Mod = new Interop.PlaceholderRecord("mod:o2");
            var uint32Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var uint32IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var uint32Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var uint32DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(uint32, uint32Add);
            RegisterMemberFunction(uint32, uint32Sub);
            RegisterMemberFunction(uint32, uint32Mul);
            RegisterMemberFunction(uint32, uint32Div);
            RegisterMemberFunction(uint32, uint32Mod);
            RegisterMemberFunction(uint32, uint32Inc);
            RegisterMemberFunction(uint32, uint32IncR);
            RegisterMemberFunction(uint32, uint32Dec);
            RegisterMemberFunction(uint32, uint32DecR);
            SetObject(uint32.Oid, uint32);
            this.BaselineScope.Registry.Add(uint32.Symbol, uint32);

            var uint64 = new Interop.ClrTypeRecord(typeof(ulong), "uint64");
            var uint64Add = new Interop.PlaceholderRecord("add:o2");
            var uint64Sub = new Interop.PlaceholderRecord("substract:o2");
            var uint64Mul = new Interop.PlaceholderRecord("multiply:o2");
            var uint64Div = new Interop.PlaceholderRecord("divide:o2");
            var uint64Mod = new Interop.PlaceholderRecord("mod:o2");
            var uint64Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var uint64IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var uint64Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var uint64DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(uint64, uint64Add);
            RegisterMemberFunction(uint64, uint64Sub);
            RegisterMemberFunction(uint64, uint64Mul);
            RegisterMemberFunction(uint64, uint64Div);
            RegisterMemberFunction(uint64, uint64Mod);
            RegisterMemberFunction(uint64, uint64Inc);
            RegisterMemberFunction(uint64, uint64IncR);
            RegisterMemberFunction(uint64, uint64Dec);
            RegisterMemberFunction(uint64, uint64DecR);
            SetObject(uint64.Oid, uint64);
            this.BaselineScope.Registry.Add(uint64.Symbol, uint64);

            var int8 = new Interop.ClrTypeRecord(typeof(sbyte), "int8");
            var int8Add = new Interop.PlaceholderRecord("add:o2");
            var int8Sub = new Interop.PlaceholderRecord("substract:o2");
            var int8Mul = new Interop.PlaceholderRecord("multiply:o2");
            var int8Div = new Interop.PlaceholderRecord("divide:o2");
            var int8Mod = new Interop.PlaceholderRecord("mod:o2");
            var int8Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var int8IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var int8Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var int8DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(int8, int8Add);
            RegisterMemberFunction(int8, int8Sub);
            RegisterMemberFunction(int8, int8Mul);
            RegisterMemberFunction(int8, int8Div);
            RegisterMemberFunction(int8, int8Mod);
            RegisterMemberFunction(int8, int8Inc);
            RegisterMemberFunction(int8, int8IncR);
            RegisterMemberFunction(int8, int8Dec);
            RegisterMemberFunction(int8, int8DecR);
            SetObject(int8.Oid, int8);
            this.BaselineScope.Registry.Add(int8.Symbol, int8);

            var int16 = new Interop.ClrTypeRecord(typeof(short), "int16");
            var int16Add = new Interop.PlaceholderRecord("add:o2");
            var int16Sub = new Interop.PlaceholderRecord("substract:o2");
            var int16Mul = new Interop.PlaceholderRecord("multiply:o2");
            var int16Div = new Interop.PlaceholderRecord("divide:o2");
            var int16Mod = new Interop.PlaceholderRecord("mod:o2");
            var int16Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var int16IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var int16Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var int16DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(int16, int16Add);
            RegisterMemberFunction(int16, int16Sub);
            RegisterMemberFunction(int16, int16Mul);
            RegisterMemberFunction(int16, int16Div);
            RegisterMemberFunction(int16, int16Mod);
            RegisterMemberFunction(int16, int16Inc);
            RegisterMemberFunction(int16, int16IncR);
            RegisterMemberFunction(int16, int16Dec);
            RegisterMemberFunction(int16, int16DecR);
            SetObject(int16.Oid, int16);
            this.BaselineScope.Registry.Add(int16.Symbol, int16);

            var int32 = new Interop.ClrTypeRecord(typeof(int), "int32");
            var int32Add = new Interop.PlaceholderRecord("add:o2");
            var int32Sub = new Interop.PlaceholderRecord("substract:o2");
            var int32Mul = new Interop.PlaceholderRecord("multiply:o2");
            var int32Div = new Interop.PlaceholderRecord("divide:o2");
            var int32Mod = new Interop.PlaceholderRecord("mod:o2");
            var int32Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var int32IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var int32Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var int32DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(int32, int32Add);
            RegisterMemberFunction(int32, int32Sub);
            RegisterMemberFunction(int32, int32Mul);
            RegisterMemberFunction(int32, int32Div);
            RegisterMemberFunction(int32, int32Mod);
            RegisterMemberFunction(int32, int32Inc);
            RegisterMemberFunction(int32, int32IncR);
            RegisterMemberFunction(int32, int32Dec);
            RegisterMemberFunction(int32, int32DecR);
            SetObject(int32.Oid, int32);
            this.BaselineScope.Registry.Add(int32.Symbol, int32);

            var int64 = new Interop.ClrTypeRecord(typeof(long), "int64");
            var int64Add = new Interop.PlaceholderRecord("add:o2");
            var int64Sub = new Interop.PlaceholderRecord("substract:o2");
            var int64Mul = new Interop.PlaceholderRecord("multiply:o2");
            var int64Div = new Interop.PlaceholderRecord("divide:o2");
            var int64Mod = new Interop.PlaceholderRecord("mod:o2");
            var int64Inc = new Interop.PlaceholderRecord("lincrement:o1l");
            var int64IncR = new Interop.PlaceholderRecord("rincrement:o1r");
            var int64Dec = new Interop.PlaceholderRecord("ldecrement:o1l");
            var int64DecR = new Interop.PlaceholderRecord("rdecrement:o1r");
            RegisterMemberFunction(int64, int64Add);
            RegisterMemberFunction(int64, int64Sub);
            RegisterMemberFunction(int64, int64Mul);
            RegisterMemberFunction(int64, int64Div);
            RegisterMemberFunction(int64, int64Mod);
            RegisterMemberFunction(int64, int64Inc);
            RegisterMemberFunction(int64, int64IncR);
            RegisterMemberFunction(int64, int64Dec);
            RegisterMemberFunction(int64, int64DecR);
            SetObject(int64.Oid, int64);
            this.BaselineScope.Registry.Add(int64.Symbol, int64);

            var logical = new Interop.ClrTypeRecord(typeof(bool), "logical");
            var logicalAnd = new Interop.PlaceholderRecord("and:o2");
            var logicalOr = new Interop.PlaceholderRecord("or:o2");
            var logicalNot = new Interop.PlaceholderRecord("not:o2");
            RegisterMemberFunction(logical, logicalAnd);
            RegisterMemberFunction(logical, logicalOr);
            RegisterMemberFunction(logical, logicalNot);
            SetObject(logical.Oid, logical);
            this.BaselineScope.Registry.Add(logical.Symbol, logical);

            var float32 = new Interop.ClrTypeRecord(typeof(float), "float32");
            var float32Add = new Interop.PlaceholderRecord("add:o2");
            var float32Sub = new Interop.PlaceholderRecord("substract:o2");
            var float32Mul = new Interop.PlaceholderRecord("multiply:o2");
            var float32Div = new Interop.PlaceholderRecord("divide:o2");
            RegisterMemberFunction(float32, float32Add);
            RegisterMemberFunction(float32, float32Sub);
            RegisterMemberFunction(float32, float32Mul);
            RegisterMemberFunction(float32, float32Div);
            SetObject(float32.Oid, float32);
            this.BaselineScope.Registry.Add(float32.Symbol, float32);

            var float64 = new Interop.ClrTypeRecord(typeof(double), "float64");
            var float64Add = new Interop.PlaceholderRecord("add:o2");
            var float64Sub = new Interop.PlaceholderRecord("substract:o2");
            var float64Mul = new Interop.PlaceholderRecord("multiply:o2");
            var float64Div = new Interop.PlaceholderRecord("divide:o2");
            RegisterMemberFunction(float64, float64Add);
            RegisterMemberFunction(float64, float64Sub);
            RegisterMemberFunction(float64, float64Mul);
            RegisterMemberFunction(float64, float64Div);
            SetObject(float64.Oid, float64);
            this.BaselineScope.Registry.Add(float64.Symbol, float64);

            var data = new Interop.ClrTypeRecord(typeof(Native.Data.DataObject), "data");
            this.BaselineScope.Registry.Add(data.Symbol, data);
            var func = new Interop.ClrTypeRecord(typeof(Native.Function.FunctionObject), "func");
            this.BaselineScope.Registry.Add(func.Symbol, func);
            var expr = new Interop.ClrTypeRecord(typeof(Native.Expr.ExprObject), "expr");
            this.BaselineScope.Registry.Add(expr.Symbol, expr);
            var matrix = new Interop.ClrTypeRecord(typeof(Native.Matrix.MatrixObject), "matrix");
            this.BaselineScope.Registry.Add(matrix.Symbol, matrix);
            var tuple = new Interop.ClrTypeRecord(typeof(Native.Tuple.TupleObject), "tuple");
            this.BaselineScope.Registry.Add(tuple.Symbol, tuple);
        }

        private void RegisterClrType(System.Type clrType, string name = "")
        {
            var type = new Interop.ClrTypeRecord(clrType, string.IsNullOrEmpty(name) ? clrType.Name : name);

            // note that only types should register its member function if defined explicitly
            // inside a datatype block. functions defined inside functions are treated as locals
            // with a function block as default value.

            foreach (var memberFunctions in clrType.GetMembers()) {
                if (memberFunctions is System.Reflection.MethodInfo methodInfo) {
                    var function = new Record.FunctionRecord(methodInfo.Name);
                    RegisterMemberFunction(type, function);
                    if (methodInfo.IsStatic) function.IsStatic = true;
                    
                } else if(memberFunctions.MemberType == System.Reflection.MemberTypes.Constructor) {
                    var function = new Record.FunctionRecord("init");
                    RegisterMemberFunction(type, function);
                }
            }
            
            this.SetObject(type.Oid, type);
        }

        private void RegisterMemberFunction(Record.TypeRecord type, Record.FunctionRecord function)
        {
            this.SetObject(function.Oid, function);

            if (!type.FieldSymbols.ContainsKey(function.Symbol))
                type.FieldSymbols.Add(function.Symbol, function.Oid);
            else this.Current.Diagnostics.AddFatal
                    (Parser.SyntaxError.MembersWithTheSameIdentifer, function.Declaration.Tokens[0]);
        }

        private T Lookup<T>(string symbol) where T : Record.IRecord
        {
            for(int i = this.Scopes.Count - 1; i >= 0; i++) {
                foreach (var item in this.Scopes[i].Registry) {
                    if (item.Key == symbol) return (T)item.Value;
                }
            }

            return default(T);
        }

        private void Register(SyntaxTree syntaxTree)
        {
            // register the syntax tree (with only names). 

            foreach (var item in syntaxTree.Statements) {
                if(item is Parser.Ast.DataDeclaration data) {
                    var datarec = RegisterTypeRecursive(data);
                    this.BaselineScope.Registry.Add(datarec.Symbol, datarec);
                    this.SetObject(datarec.Oid, datarec);

                } else if(item is Parser.Ast.FunctionDeclaration function) {
                    var obj = RegisterFunctionRecursive(function);
                    this.BaselineScope.Registry.Add(obj.Symbol, obj);
                    this.SetObject(obj.Oid, obj);
                }
            }
        }

        private Record.TypeRecord RegisterTypeRecursive(Parser.Ast.DataDeclaration data)
        {
            Record.TypeRecord datarec = new Record.TypeRecord(data.Name);
            datarec.Declaration = data;
            if (data.Identifer != null) datarec.Symbol = data.Identifer.Value;

            // add the fields defined in the data-type

            foreach (var field in data.Variables.VariableDeclarations) {
                Record.DataRecord fieldrec = new Record.DataRecord();
                fieldrec.Symbol = field.Identifer.Value;
                fieldrec.Expression = field.Expression;
                this.Objects.Add(fieldrec.Oid, fieldrec);

                // the identifers of the members cannot be set to the same, this will cause the conflict
                // error when two or more members, variables and functions are set to the same name.
                //
                //     def type = data ( int32 local )      ' the data member named 'local'
                //         def local = 1                    ' the data member named 'local'
                //         def local = func ( int32 i )     ' the function named 'local'
                //             ...
                //         end
                //     end

                if (!datarec.FieldSymbols.ContainsKey(fieldrec.Symbol))
                    datarec.FieldSymbols.Add(fieldrec.Symbol, fieldrec.Oid);
                else this.Current.Diagnostics.AddFatal
                        (Parser.SyntaxError.MembersWithTheSameIdentifer, data.Tokens[0]);
            }

            // add the functions defined in data-type

            foreach (var funcdecl in data.Variables.FunctionDeclarations) {
                var obj = RegisterFunctionRecursive(funcdecl);
                RegisterMemberFunction(datarec, obj);
            }

            // add the fields of the type

            foreach (var field in data.Fields) {
                Record.DataRecord fieldrec = new Record.DataRecord();
                fieldrec.Symbol = field.Identifer.Value;
                fieldrec.Expression = field.Default;
                fieldrec.ExplicitType = field.Type;
                this.Objects.Add(fieldrec.Oid, fieldrec);

                if (!datarec.FieldSymbols.ContainsKey(fieldrec.Symbol))
                    datarec.FieldSymbols.Add(fieldrec.Symbol, fieldrec.Oid);
                else this.Current.Diagnostics.AddFatal
                        (Parser.SyntaxError.MembersWithTheSameIdentifer, data.Tokens[0]);
            }

            return datarec;
        }

        // this method returns a new modified function record without adding it to the baseline scope.

        private Record.FunctionRecord RegisterFunctionRecursive(Parser.Ast.FunctionDeclaration decl)
        {
            // a function have its own fields inside the body and other local functions. these local
            // variables are defined using assignment operation implicitly. the parser will ignore
            // explicitly-defined functions nested in functions.

            // 1.  def combine = func (func a, func b) => func
            //         return a <*> b
            //         def local = func (int32 a) => int32
            //             ...
            //         end
            //     end

            // is equals sematically as

            // 1'. def combine = func (func a, func b) => func
            //         return ...
            //         local = func (int32 a) => int32
            //         end
            //     end

            Record.FunctionRecord funcrec = new Record.FunctionRecord(decl.Name);
            funcrec.Declaration = decl;
            if (decl.Identifer != null) funcrec.Symbol = decl.Identifer.Value;

            // register function parameters and their modifers. the modifer of function parameter have
            // fewer choices than in the class identifer. current version supports a flagged enumeration
            // of modifers (enum ParameterModifer)

            foreach (var item in decl.Statements) {
                if (item is Parser.Ast.BinaryExpression binary) {
                    if (binary.Operator.Method == "assign") {
                        if (binary.Left is Parser.Ast.Literal literal &&
                            literal.Type == Parser.Ast.LiteralType.Named) {
                            if (binary.Right is Parser.Ast.FunctionDeclaration funcDecl) {
                                Record.FunctionRecord func = RegisterFunctionRecursive(funcDecl);
                                func.Symbol = literal.Value;
                                if (!funcrec.Locals.ContainsKey(func.Symbol))
                                    funcrec.Locals.Add(func.Symbol, func);
                                this.Objects.Add(func.Oid, func);
                            } else if (binary.Right is Parser.Ast.DataDeclaration dataDecl) {
                                Record.TypeRecord typerec = RegisterTypeRecursive(dataDecl);
                                typerec.Symbol = literal.Value;
                                if (!funcrec.Locals.ContainsKey(typerec.Symbol))
                                    funcrec.Locals.Add(typerec.Symbol, typerec);
                                this.Objects.Add(typerec.Oid, typerec);
                            } else {
                                Record.DataRecord data = new Record.DataRecord();
                                data.Symbol = literal.Value;
                                data.Expression = binary.Right;
                                if (!funcrec.Locals.ContainsKey(data.Symbol))
                                    funcrec.Locals.Add(data.Symbol, data);
                                this.Objects.Add(data.Oid, data);
                            }

                        } else if (binary.Left is Parser.Ast.SequenceExpression seq) {
                            int indexCounter = 0;
                            foreach (var seqelement in seq.Statements) {
                                indexCounter++;
                                if( seqelement is Parser.Ast.Literal elem &&
                                    elem.Type == Parser.Ast.LiteralType.Named) {
                                    Record.DataRecord data = new Record.DataRecord();
                                    data.Symbol = elem.Value;
                                    data.Expression = new Parser.Ast.IndexExpression(binary.Right)
                                    {
                                        Arguments = new List<Parser.Ast.IExpression>() {
                                            new Parser.Ast.Literal(indexCounter.ToString())
                                        }
                                    };

                                    if (!funcrec.Locals.ContainsKey(data.Symbol))
                                        funcrec.Locals.Add(data.Symbol, data);
                                    this.Objects.Add(data.Oid, data);
                                }
                            }
                        }
                    }
                }
            }

            return funcrec;
        }

        private void SetObject(Guid oid, Record.IRecord record)
        {
            if (this.Objects.ContainsKey(oid)) this.Objects[oid] = record;
            else this.Objects.Add(oid, record);
        }

        // the second step is to infer the type of all functions. the function's parameters and the
        // return types. as well as the types for fields of a data type.                                 (II)
        
        // direct inference section infer the type indicating the literal types and basic type operation
        // it involves no expression evaluation to obtain type properties. the types of them is written
        // out explicitly.                                                                               (II.1)

        private void VisitDefinition()
        {
            foreach (var scope in this.Scopes) {
                foreach (var registry in scope.Registry) {
                    if(registry.Value is Record.TypeRecord type) {
                        InferDefinition(type);
                    } else if(registry.Value is Record.FunctionRecord func) {
                        if (func is Interop.PlaceholderRecord) continue;
                        InferDefinition(func);
                    }
                }
            }
        }

        private void InferDefinition(Record.TypeRecord type)
        {
            if (type.Declaration == null) return;

            foreach (var assert in type.Declaration.Assertions) {
                type.Assertions.Add(GetDirectTypeRecord(assert));
            }

            foreach (var symbol in type.FieldSymbols) {

            }

            foreach (var inherit in type.Declaration.Inheritage) {

            }
        }

        private void InferDefinition(Record.FunctionRecord func)
        {
            
        }

        private Record.TypeRecord GetDirectTypeRecord(Parser.Ast.IExpression expr)
        {
            if(expr is Parser.Ast.BinaryExpression binary) {
                var left = GetDirectTypeRecord(binary.Left);
                var right = GetDirectTypeRecord(binary.Right);

                switch (binary.Operator.Method) {
                    case "exclude":
                        return new Record.ExcludeType(left, right);

                    case "bitand":
                        if( left is Record.CombinationType ||
                            right is Record.CombinationType ) {
                            var combLeft = (Record.CombinationType)left;
                            combLeft.Combination.AddRange(((Record.CombinationType)right).Combination);
                            return combLeft;
                        }

                        if(left is Record.CombinationType) {
                            var combLeft = (Record.CombinationType)left;
                            combLeft.Combination.Add(right);
                            return combLeft;
                        }

                        if (right is Record.CombinationType) {
                            var combRight = (Record.CombinationType)right;
                            combRight.Combination.Insert(0, left);
                            return combRight;
                        }

                        return new Record.CombinationType(new List<Record.TypeRecord>()
                        {
                            left,
                            right
                        });

                    case "bitor":
                        if (left is Record.EitherType ||
                            right is Record.EitherType) {
                            var combLeft = (Record.EitherType)left;
                            combLeft.Choices.AddRange(((Record.EitherType)right).Choices);
                            return combLeft;
                        }

                        if (left is Record.EitherType) {
                            var combLeft = (Record.EitherType)left;
                            combLeft.Choices.Add(right);
                            return combLeft;
                        }

                        if (right is Record.EitherType) {
                            var combRight = (Record.EitherType)right;
                            combRight.Choices.Insert(0, left);
                            return combRight;
                        }

                        return new Record.EitherType(new List<Record.TypeRecord>()
                        {
                            left,
                            right
                        });

                    default:
                        this.Current.Diagnostics.AddFatal(Parser.SyntaxError.InvalidTypeCalc, binary.Tokens[0]);
                        return new Record.NullType();
                }
            }

            if(expr is Parser.Ast.Literal literal) {
                var oid = GetRegistryOid(literal.Value, RegistryKind.NoLimit);
                if(oid.Equals(Guid.Empty)) {
                    this.Current.Diagnostics.AddFatal(Parser.SyntaxError.InvalidType, literal.Tokens[0]);
                    return new Record.NullType();
                }

                if(this.Objects[oid] is Record.TypeRecord t) {
                    return t;
                } else {
                    this.Current.Diagnostics.AddFatal(Parser.SyntaxError.InvalidType, literal.Tokens[0]);
                    return new Record.NullType();
                }
            }

            this.Current.Diagnostics.AddFatal(Parser.SyntaxError.InvalidType, expr.Tokens[0]);
            return new Record.NullType();
        }

        private Guid GetRegistryOid(string name, RegistryKind limits = RegistryKind.NoLimit)
        {
            for(int counter = this.Scopes.Count -1; counter>=0; counter--) {
                var currentScope = this.Scopes[counter];
                foreach (var scopeElement in currentScope.Registry) {
                    if (scopeElement.Key.Trim() != name.Trim()) continue;

                    if (limits == RegistryKind.NoLimit) return scopeElement.Value.Oid;
                    if (scopeElement.Value is Record.TypeRecord type && limits == RegistryKind.Datatype)
                        return type.Oid;
                    if (scopeElement.Value is Record.FunctionRecord func && limits == RegistryKind.Function)
                        return func.Oid;
                    if (limits == RegistryKind.Locals) return scopeElement.Value.Oid;
                }
            }

            return Guid.Empty;
        }

        private T GetRegistry<T>(string name, RegistryKind limits = RegistryKind.NoLimit)
        {
            for (int counter = this.Scopes.Count - 1; counter >= 0; counter--) {
                var currentScope = this.Scopes[counter];
                foreach (var scopeElement in currentScope.Registry) {
                    if (scopeElement.Key.Trim() != name.Trim()) continue;

                    if (limits == RegistryKind.NoLimit) return (T)scopeElement.Value;
                    if (scopeElement.Value is Record.TypeRecord type && limits == RegistryKind.Datatype)
                        return (T)scopeElement.Value;
                    if (scopeElement.Value is Record.FunctionRecord func && limits == RegistryKind.Function)
                        return (T)scopeElement.Value;
                    if (limits == RegistryKind.Locals) return (T)scopeElement.Value;
                }
            }

            return default(T);
        }

        public enum RegistryKind
        {
            Datatype = 0b001 ,
            Function = 0b010 ,
            Locals   = 0b100 ,
            NoLimit  = 0b111
        }
    }
}
