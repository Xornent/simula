using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    // any type is an abstraction of all types that are currently available.
    // the use of any type should be limited for this breaks all the strong-type advantages.

    // note: the 'any' keyword can only be applied in the input parameter type limits in a function
    //       declaration or type judgement condition, meaning there are no limit to the parameter's type. 
    //       however, it can not be placed at function return types, varaible declaration, type casting
    //       result, and data members.
    // 
    //       def foo = func (any input) ...                                                     [v]
    //       if ( obj ~> any ! numeral ) ...                                                    [v]
    //
    //       def foo = func (any input) => any                                                  [x]
    //       cast = obj -> any                                                                  [x]
    //       data ( any item ) ...                                                              [x]

    public class AnyType : TypeRecord
    {
        public new string Symbol { get; set; } = "any";
    }
}
