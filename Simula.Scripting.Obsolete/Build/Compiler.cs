using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Simula.Scripting.Build
{
    public class Compiler
    {
        public static void Run(string source)
        {
            ScriptOptions options = ScriptOptions.Default;
            options = options.AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
                   .AddReferences(MetadataReference.CreateFromFile(typeof(System.Dynamic.ExpandoObject).Assembly.Location))
                   .AddReferences(MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).Assembly.Location))
                   .AddReferences(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.Binder).Assembly.Location))
                   .AddReferences(MetadataReference.CreateFromFile(typeof(Compiler).Assembly.Location))
                   .WithImports("Simula.Scripting.Build");

            var script = CSharpScript.Create("public static bool _lt(this object a, dynamic b) { return a < b; }", options, typeof(Global));
            
            var result = script.Compile();
            foreach (var item in result) {
                System.Windows.MessageBox.Show(item.ToString());
            }

            try {
                script.RunAsync(new Global(), (ex) => { System.Windows.MessageBox.Show(ex.Message); return true; }).Result
                      .ContinueWithAsync(source, options, (ex) => { System.Windows.MessageBox.Show(ex.Message); return true; });
            } catch {
                System.Windows.MessageBox.Show("error");
            }
        }
    }
}
