using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class DeclarationBlock : BlockStatement
    {
        public Literal Identifer { get; set; }
        public DefinitionType Type { get; set; }
        public bool IsReadonly { get; set; }
        public bool IsExposed { get; set; }
    }

    public enum DefinitionType
    {
        Function,
        Data,
        Variable
    }
}
