using System;
using System.Linq;
using System.Collections.Generic;
using Simula.Scripting.Contexts;
using System.Dynamic;
using Simula.Scripting.Types;
using Simula.Scripting.Build;

namespace Simula.Scripting.Syntax
{
    public class ParameterOperation : OperatorStatement
    {
        public override Execution Operate(DynamicRuntime ctx) {
            List<Execution> members = new List<Execution>();
            if (EvaluateOperators.Count == 0) return new Execution();
            List<OperatorStatement?> ops = new List<OperatorStatement?>() { null };
            foreach (var item in this.EvaluateOperators) {
                if (item is SelfOperation) {
                    if (((SelfOperation)item).Self == ",") {
                        ops.Add(null);
                        continue;
                    }
                }

                ops[ops.Count - 1] = item;
            }

            if (ops.Count > 0) return ops[0].Operate(ctx);
            else return new Execution();
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            if (EvaluateOperators.Count == 0) return new TypeInference();
            List<OperatorStatement?> ops = new List<OperatorStatement?>() { null };
            foreach (var item in this.EvaluateOperators) {
                if (item is SelfOperation) {
                    if (((SelfOperation)item).Self == ",") {
                        ops.Add(null);
                        continue;
                    }
                }

                ops[ops.Count - 1] = item;
            }

            if (ops.Count > 0) return ops[0].InferType(ctx);
            else return new TypeInference();
        }

        public List<Execution> FullExecution(DynamicRuntime ctx) {
            List<Execution> members = new List<Execution>();
            if (EvaluateOperators.Count == 0) return new List<Execution>();
            List<OperatorStatement?> ops = new List<OperatorStatement?>() { null };
            foreach (var item in this.EvaluateOperators) {
                if (item is SelfOperation) {
                    if (((SelfOperation)item).Self == ",") {
                        ops.Add(null);
                        continue;
                    }
                }

                ops[ops.Count - 1] = item;
            }

            foreach (var item in ops) {
                if (item == null) members.Add(new Execution());
                else members.Add(item.Operate(ctx));
            }

            return members;
        }

        public List<dynamic> DynamicFullExecution(DynamicRuntime ctx)
        {
            List<dynamic> members = new List<dynamic>();
            if (EvaluateOperators.Count == 0) return new List<dynamic>();
            List<OperatorStatement?> ops = new List<OperatorStatement?>() { null };
            foreach (var item in this.EvaluateOperators) {
                if (item is SelfOperation) {
                    if (((SelfOperation)item).Self == ",") {
                        ops.Add(null);
                        continue;
                    }
                }

                ops[ops.Count - 1] = item;
            }

            foreach (var item in ops) {
                if (item == null) {
                    members.Add(Types.Null.NULL);
                    continue;
                }

                dynamic result = item.Operate(ctx).Result;
                while (result is Execution) { result = result.Result; }
                if (item == null) members.Add(Types.Null.NULL);
                
                members.Add(result);
            }

            return members;
        }

        public override string Generate(GenerationContext ctx)
        {
            List<string> str = new List<string>();
            foreach (var item in this.EvaluateOperators) {
                str.Add(item.Generate(ctx));
            }
            return "(" + str.JoinString(", ") + ")";
        }
    }

    public class ParenthesisOperation : ParameterOperation { }

    public class BracketOperation : ParameterOperation 
    {
        // brackets [] are used to indicate a location 1-dimensional matrix.
        // and the operation is called indexing.

        public override Execution Operate(DynamicRuntime ctx)
        {
            BraceOperation brace = new BraceOperation();
            brace.EvaluateOperators = this.EvaluateOperators;
            return brace.Operate(ctx);
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            return new TypeInference(new HashSet<string> { "sys.matrix" }, null);
        }
    }

    public class BraceOperation : ParameterOperation 
    {
        // change log: braces {} are used to declare matrixes. but not arrays.
        //             arrays are excluded from the basic types of the language because all
        //             of its functions can be replaced by a matrix version.

        public override Execution Operate(DynamicRuntime ctx)
        {
            Types.Matrix arr = new NumericalMatrix<double>(new double[0]);
            List<dynamic> members = new List<dynamic>();
            if (EvaluateOperators.Count == 0) return new Execution(ctx, arr);
            if(this.EvaluateOperators.Last() is SelfOperation so) {
                if (so.Self == ";") this.EvaluateOperators.RemoveAt(this.EvaluateOperators.Count - 1);
            }

            // in the parsing procedures, newline charactor will be parsed back to the charactor
            // ';' and all explicit ';' will be preserved. ';' is the separator of matrix's rows.
            // one can declare a 2-dimensional matrix (the most common ones) using ',' and ';'

            //     mat = [ 0, 1, 2; 3, 4, 5; 6, 7, 8 ]

            //     mat = [ 
            //     0, 1, 2
            //     3, 4, 5
            //     6, 7, 8 ]

            // the ',' can be ommited. when the expression contains at least 1 comma, the parser
            // will assume that ','s are the separate values, otherwise, it percieve the list of
            // evaluating operators as elements.

            //    mat = [ 0 1 2 3 4 5 6 7 8 ]
            //    mat = [
            //    0 1 2
            //    3 4 5
            //    6 7 8 + 10 ]

            // note that the '+' operation is parsed first, and the '8 + 10' together is an operator.

            // checking if row lengths are uniform.

            bool uniform = true;
            int row = 0;
            int rowCount = 1;
            int counter = 0;
            foreach (var item in this.EvaluateOperators) {
                if(item is SelfOperation self) {
                    if (self.Self == ";") {
                        if (counter != 0) {
                            if (row == 0) {
                                row = counter;
                                rowCount++;
                            } else if (row != counter) {
                                uniform = false;
                                break;
                            } else {
                                rowCount++;
                            }
                        } 

                        counter = 0;
                        continue;
                    } else if (self.Self == ",") { continue; }
                }

                counter++;
            }

            if (uniform && row >= 0) {
                foreach (var item in this.EvaluateOperators) {
                    if (item is SelfOperation self) {
                        if (self.Self == ";") { continue; }
                        else if (self.Self == ",") { continue; }
                    }

                    if (item == null) members.Add(Types.Null.NULL);
                    else members.Add(item.Operate(ctx).Result);
                }
            } else ctx.PostRuntimeError("ss0000", "matrix are not uniform or with 0 column count.");

            bool uniformType = true;
            Type t = members[0].GetType();
            foreach (var item in members) {
                if (item.GetType() != t) uniformType = false;
            }

            if (uniformType) {
                var obj = members[0];
                if (obj is Types.Boolean) arr = new Types.NumericalMatrix<bool>(members.ToArray());

                else if (obj is Types.Double) arr = new Types.NumericalMatrix<double>(members.ToArray());

                else if (obj is Types.Byte) arr = new Types.NumericalMatrix<byte>(members.ToArray());
                else if (obj is Types.Char) arr = new Types.NumericalMatrix<ushort>(members.ToArray());
                else if (obj is Types.UInt32) arr = new Types.NumericalMatrix<uint>(members.ToArray());
                else if (obj is Types.UInt64) arr = new Types.NumericalMatrix<ulong>(members.ToArray());

                else if (obj is Int8) arr = new Types.NumericalMatrix<sbyte>(members.ToArray());
                else if (obj is Types.Int16) arr = new Types.NumericalMatrix<short>(members.ToArray());
                else if (obj is Types.Int32) arr = new Types.NumericalMatrix<int>(members.ToArray());
                else if (obj is Types.Int64) arr = new Types.NumericalMatrix<long>(members.ToArray());

                else arr = new Types.ObjectMatrix(members.ToArray());
            } else arr = new Types.ObjectMatrix(members.ToArray());

            
            if (rowCount > 1) {
                arr.Reshape(new NumericalMatrix<int>(new int[2]{ rowCount, row }));
            }

            return new Execution(ctx, arr);
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            return new TypeInference(new HashSet<string> { "sys.matrix" }, null);
        }
    }

    public class IndexOperation : OperatorStatement
    {
        public override Execution Operate(DynamicRuntime ctx)
        {
            if (this.Left == null) return new Execution();
            if (this.Right == null) return new Execution();
            dynamic left = this.Left.Operate(ctx).Result;
            dynamic right = this.Right.Operate(ctx).Result;

            if(left is Matrix mleft) {
                if(right is INumericalMatrix mright) {
                    return new Execution(ctx, mleft.Get( mright.ToIntegerMatrix() ));
                }
            }

            return new Execution();
        }
    }

    public class FunctionCallOperation : OperatorStatement
    {
        public override Execution Operate(DynamicRuntime ctx)
        {
            if (this.Left == null) return new Execution();
            dynamic[] args = ((ParameterOperation)(this.Right)).DynamicFullExecution(ctx).ToArray();
            dynamic obj = this.Left.Operate(ctx).Result;

            if(obj.type == "sys.class") {
                var list = args.ToList();
                list.Insert(0, ctx);
                return new Execution(ctx, Types.Class._create._call(obj, list.ToArray()));
            } else if(obj.type == "sys.func") {
                if(this.Left is MemberOperation member) {
                    var caller = member.Left.Operate(ctx).Result;
                    return new Execution(ctx, obj._call(caller, args));
                }

                // note that a member function (when called without 'this.' prefix will be 
                // considered as a static function and passes 'null' to 'self'. however, this
                // seems not to trigger any bugs in execution.

                // a possible solution is to check if the current scope contains a 'this' 
                // variable, which will always be added to a function scope if it is called 
                // as a class member. then send that as 'self'.

                return new Execution(ctx, obj._call(null, args));
            }

            return new Execution();
        }

        public override TypeInference InferType(CompletionContext ctx)
        {
            if (this.Left == null) return new TypeInference();
            var left = this.Left.InferType(ctx);
            HashSet<string> ret = new HashSet<string>();

            if(left.Types.Contains("sys.func") || left.Types.Contains("sys.class") || left.Types.Contains("any") || left.Types.Contains("ref")) {
                if(left.Object !=null) {
                    if (left.Types.Contains("sys.func")) ret.AddRange(left.Object.ReturnTypes);
                    else if (left.Types.Contains("sys.class")) ret.Add(left.Object.FullName);
                    else ret.Add("any");
                }
            } else {
                this.Left.RawEvaluateToken.Last().Error = new Token.TokenizerException("ss1001");
            }

            return new TypeInference(ret, null);
        }

        public override string Generate(GenerationContext ctx)
        {
            string str = this.Right?.Generate(ctx);
            str = str.Remove(0, 1);
            str = str.Remove(str.Length - 1, 1);
            return this.Left?.Generate(ctx) + "(null, new dynamic[] {" + str + "})";
        }
    }
}