using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public class DataRecord : IRecord
    {
        public DataRecord()
        {
            this.Oid = Guid.NewGuid();
        }

        public string Symbol { get; set; } = "_";
        public bool IsUnion { get; } = false;
        public RecordType RecordType { get { return RecordType.Data; } }
        public bool IsClr { get; } = false;

        // the definition type of the data, in static analysis 2.2, this record is inferred from either
        // expression evaluation or explicit type, if specified.

        public TypeRecord Definition { get; set; }

        // the expression is the first-assigned value to the variable. it decides what the type the data
        // is. in runtime, the data type cannot be changed. for example,
        //
        //     a = 1
        //     a = "string"

        // the second statement will throw an error in type check, because we assume that a is 1, and 
        // a is defined as a integral, or numeral values, it cannot be changed into string. explicitly 
        // casting the default value can set a specified type, using cast operator.
        //
        //     a = 1 -> float64
        //     a = 1.0

        // if the expression is set to '_', it means no default value specified. this can only be written
        // when defining function parameters and data members, using explicitly specified type. if expression
        // is '_'(literal), the 'ExplicitType' must not be 'null'.
        //
        //     a = func (int32 i = _) ...

        internal Parser.Ast.IExpression Expression { get; set; }
        internal Parser.Ast.IExpression ExplicitType { get; set; } = null;
        public Guid Oid { get; set; }
    }
}
