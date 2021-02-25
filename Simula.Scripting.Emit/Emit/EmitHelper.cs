using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;

namespace Simula.Scripting.Emit
{
    public static class EmitHelper
    {
        public static void BuildStructure()
        {
            AssemblyBuilder asm = AssemblyBuilder.DefineDynamicAssembly(new System.Reflection.AssemblyName("name.dll"),
                AssemblyBuilderAccess.RunAndCollect);
        }
    }
}
