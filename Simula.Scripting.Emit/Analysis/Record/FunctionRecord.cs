using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Parser.Ast;

namespace Simula.Scripting.Analysis.Record
{
    public class FunctionRecord : IRecord
    {
#pragma warning disable CS8618
        public FunctionRecord(string symbol)
#pragma warning restore CS8618
        {
            this.Symbol = symbol;
        }

        // this method is called later, when all the objects' names are registered as references

        public void Define(FunctionDeclaration decl)
        {
            this.declaration = decl;
            
            // evaluate the type used for current function. including the return types and parameter types.
            // a function is complete only when:

            // (1) its return type can be described with available types.
            // (2) all of its parameter types can be described with available types.
            // (3) all of the locals can be described with available types.
            // (4) all the references to outside is within the availble definitions.
        }

        public string Symbol { get; set; } = "_";
        public bool IsUnion { get; } = false;
        public RecordType RecordType { get { return RecordType.Function; } }
        public bool IsClr { get; } = false;

        private FunctionDeclaration declaration;

        public TypeRecord ReturnType;
        public List<TypeRecord> ParameterType = new List<TypeRecord>();
        public List<string> ParameterSymbol = new List<string>();
        public List<ParameterModifer> ParameterModifer = new List<ParameterModifer>();

        public List<IRecord> Locals = new List<IRecord>();
        public List<IRecord> References = new List<IRecord>();
    }

    public enum ParameterModifer
    {
        Expose,
        Hidden
    }
}
