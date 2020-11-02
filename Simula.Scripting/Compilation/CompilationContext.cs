using Microsoft.CodeAnalysis;
#if cs_support
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyModel;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simula.Scripting.Compilation {
#if cs_support
    public class CompilationContext {
        public void Compile(string moduleName) {
            MetadataReference[] _ref = DependencyContext.Default.CompileLibraries
                .First(cl => cl.Name == "Microsoft.NETCore.App")
                .ResolveReferencePaths()
                    .Append<string>(Environment.CurrentDirectory + @"/toolsets/simula.scripting.basetype.dll")
                .Select(asm => MetadataReference.CreateFromFile(asm))
                .ToArray();

            var compilation = CSharpCompilation.Create( moduleName.ToLower() + ".dll")
              .WithOptions(new CSharpCompilationOptions(
                  Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary,
                  usings: null,
                  optimizationLevel: OptimizationLevel.Debug,
                  checkOverflow: false,                      
                  allowUnsafe: true,                         
                  platform: Platform.AnyCpu,
                  warningLevel: 4,
                  xmlReferenceResolver: null
                  ))
              .AddReferences(_ref)
              .AddSyntaxTrees(CSharpSyntaxTree.ParseText(""));
            var eResult = compilation.Emit(moduleName.ToLower() + ".dll");
        }
    }
#endif
}
