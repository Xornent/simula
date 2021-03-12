using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Parser.Ast;

namespace Simula.Scripting.Analysis.Record
{
    public class FunctionRecord : IRecord
    {
        public FunctionRecord()
        {
            this.Oid = Guid.NewGuid();
        }

#pragma warning disable CS8618
        public FunctionRecord(string symbol) : this()
#pragma warning restore CS8618
        {
            this.Symbol = symbol;
        }

        // this method is called later, when all the objects' names are registered as references

        public void Define(FunctionDeclaration decl)
        {
            this.Declaration = decl;
            
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

        internal FunctionDeclaration Declaration;

        public TypeRecord ReturnType;
        public List<TypeRecord> ParameterType = new List<TypeRecord>();
        public IDictionary<string, Guid> ParameterSymbol = new Dictionary<string, Guid>();
        public List<ParameterModifer> ParameterModifer = new List<ParameterModifer>();

        public Dictionary<string, IRecord> Locals = new Dictionary<string, IRecord>();
        public Dictionary<string, IRecord> References = new Dictionary<string, IRecord>();

        public bool IsStatic { get; set; } = false;
        public Guid Oid { get; set; }
    }

    public enum ParameterModifer
    {
        Expose,
        Hidden
    }
}
