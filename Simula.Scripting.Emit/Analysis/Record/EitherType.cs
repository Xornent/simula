using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    // indicate an object's type is within a range of types. with function either(string, int32, null)
    // can indicates an object is whether string or int32 or null but not other types. using this
    // in function declaration will provide a parameter multiple choices of types. operator '|' between
    // types can also act like either function.

    // ( any | b )         = any
    // (( a | b ) | b )    = a | b
    // (( a | b ) | c )    = a | b | c
    // (( a | b ) & b )    = ( a & b ) | b
    // (( a | b ) & c )    = ( a & c ) | ( b & c )
    // (( a | b ) ! b )    = a
    // (( a | b ) ! c )    = a | b

    // in the static analysis process, all type inference containing either(|), and(&), and exclude(!)
    // should be simplify into one of the following form:

    // either ( a1, a2, ... )
    // any ! either ( a1, a2, ... )

    public class EitherType : TypeRecord
    {
        public EitherType() : base() { }
        public EitherType(List<TypeRecord> choices) : base()
        {
            this.Choices = choices;
        }

        public EitherType(params TypeRecord[] choices) : base()
        {
            this.Choices = choices.ToList();
        }

        public List<TypeRecord> Choices { get; set; } = new List<TypeRecord>();
    }
}
