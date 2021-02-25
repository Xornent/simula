using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting
{
    public class UnitTest
    {
        public static void Alert(string message)
        {
            System.Windows.MessageBox.Show(message);
        }
    }

    public class Program
    {
        [STAThread]
        static void Main()
        {
            Parser.Tokenizer tokenizer = new Parser.Tokenizer();
            var str1 = @"
module sys.io
use sys.io 
' this is a line comment
debugger on
def d1 = data(private int32 i = 0, ...
          public static readonly int32 j)
end
def d2 = data(params int32 k) : d1
    def a = func(int32 i)
        alert(hello)
    end
end
def a1 = data()
end
d3 = data(int32 l) : d1, d2 assert a1
end

def a = func (int32 i, int32 j) => int32
   another = 0
   variable = [block return i + j end, 2
               2, 3
               4, 5 ]
   (another, variable) = (0, func (uint16 c1, uint16 c2) => uint16
      return c1 + c2
   end )
   [ a1
     a2
     a3 ] = [ 0; 1; 2 ]

   [1, b] = [1, 2]
end

b = 5.886 + 867
v = u.prop1.getid[4].tostring(show())
c = f1 <*> b

try
if (b == 0)
    return 1
eif b == 1
    return 2
else return 3 end

config conf
    util = uint16
    id = config t
        obj = null
    end
end
catch ex
touch sbls
end

iter 19293
    iter a in collection
        a = 1
    end
    iter a in collection at pos
        alert(pos)
    end
end
";
            
            var result = tokenizer.Tokenize(str1);
            Console.WriteLine("Lexical Analysis");
            Console.WriteLine("");
            Console.WriteLine(result.ToString(Parser.TokenFormatterOption.Table));
            Console.WriteLine("");
            Console.WriteLine("Grammar Analysis");
            Console.WriteLine("");
            Parser.Parser parser = new Scripting.Parser.Parser();
            var program = parser.Parse(result);
            if (program == null) Console.WriteLine("Null Program");
            else {
                PrintAst(program.Body, 0);
                Console.WriteLine("");
                
                Console.WriteLine("Diagnostics: {0} Errors, {1} Warnings", program.Result.Fatals, program.Result.Warnings);
                foreach (var item in program.Result.Diagnostics) {
                    switch (item.Severity) {
                        case Parser.Severity.Information:
                            Console.Write("(i) ");
                            break;
                        case Parser.Severity.Warning:
                            Console.Write("(!) ");
                            break;
                        case Parser.Severity.Fatal:
                            Console.Write("(x) ");
                            break;
                        default:
                            break;
                    }

                    Console.Write("({0},{1})-({2},{3})", item.Location.Start.Line, item.Location.Start.Column, item.Location.End.Line, item.Location.End.Column);
                    Console.WriteLine(" " + item.Error.ToString());
                }

                Analysis.SyntaxTree syntax = Analysis.SyntaxTree.Analyse(program);
                Console.WriteLine("Syntax Static Analysis");
                Console.WriteLine("Diagnostics: {0} Errors, {1} Warnings", syntax.Diagnostics.Fatals, syntax.Diagnostics.Warnings);
                foreach (var item in syntax.Diagnostics.Diagnostics) {
                    switch (item.Severity) {
                        case Parser.Severity.Information:
                            Console.Write("(i) ");
                            break;
                        case Parser.Severity.Warning:
                            Console.Write("(!) ");
                            break;
                        case Parser.Severity.Fatal:
                            Console.Write("(x) ");
                            break;
                        default:
                            break;
                    }

                    Console.Write(item.File + " ");
                    Console.Write("({0},{1})-({2},{3})", item.Location.Start.Line, item.Location.Start.Column, item.Location.End.Line, item.Location.End.Column);
                    Console.WriteLine(" " + item.Error.ToString());
                }
            }
        }

        static void PrintAst(List<Parser.Ast.IExpression> program, int indent)
        {
            foreach(var stmt in program) {
                for (int i = 0; i < indent; i++) Console.Write("| ");
                Console.WriteLine(stmt.GetType().Name+"    :: " + stmt.Tokens.ToString(Parser.TokenFormatterOption.SpaceSeparatedList));
                if (stmt is Parser.Ast.BlockStatement block) {
                    PrintAst(block.Statements, indent + 1);
                } else if(stmt is Parser.Ast.BinaryExpression bin) {
                    PrintAst(new List<Parser.Ast.IExpression>() { bin.Left, bin.Right }, indent + 1);
                } else if(stmt is Parser.Ast.UnaryExpression unary) {
                    PrintAst(new List<Parser.Ast.IExpression>() { unary.Operant }, indent + 1);
                }
            }
        }
    }
}
