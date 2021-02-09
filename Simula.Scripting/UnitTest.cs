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
            var result = tokenizer.Tokenize(@"
module sys.io
use sys.io ' this is a line comment

' a comment
' followed by another line of comment
data ast ( string | null   nodeName ,
           astType         type     ) : tree assert executable
    func execute () => any
        string | null str = " + "\"" + @"some test of \" + "\"" + @" <~ that is an escape ~> string" + "\"" + @"
        float value = 0.344 + 0. + .344
        int32 valueIntegral = 1223
        return this ~> (executable)
    end
end
");
            Console.WriteLine(result.ToString(Parser.TokenFormatterOption.Table));
        }
    }
}
