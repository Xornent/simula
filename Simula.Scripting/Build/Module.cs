using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Simula.Scripting.Syntax;
using System.Reflection.Emit;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Runtime.Loader;

namespace Simula.Scripting.Build
{
    public class Module
    {
        private BlockStatement program;
        public Module(BlockStatement block)
        {
            this.program = block;
        }

        
    }
}
