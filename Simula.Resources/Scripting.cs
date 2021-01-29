using System;
using System.Collections.Generic;

namespace Simula.Scripting
{
    public static class Resources
    {
        public static Dictionary<StringTableIndex, string> StringTable = new Dictionary<StringTableIndex, string>()
        {
            {StringTableIndex.NullContainer, "The referenced container is not defined." },
            {StringTableIndex.AnnonymousTypeNameReferenceFail, "Cannot create a reference by name to an annonymous type." },
            {StringTableIndex.IntegralCastOutOfRange, "The target value is either too large or too small for a integral value type." },
            {StringTableIndex.NovolatileMemberGetAddrFail, "The object you are trying to get address is or contains non-volatile members." },

            {StringTableIndex.Doc_Sys_Alert, "Displays a system message box containing the string form of given information" },
            {StringTableIndex.Doc_Sys_Dir, "Basic information output about a given variable, containing its registered fields or functions.\n\n" +
                                           "You should avoid using the dialog in procedure or turn to the 'variable' interface for variable properties."},
            {StringTableIndex.Doc_Sys_Ref, "Create a reference by name to an object.\n\n" +
                                           "An annonymous object cannot be refered to by name. this will trigger an SS2002"},
            {StringTableIndex.Doc_Sys_Uint8, "Convert a numeric value, or a numeric matrix, to an equivilant form of value or matrix of the type uint8(byte).\n\n" +
                                             "If any of the given values are not within the range [0, 255], this will trigger an SS2003" },
            {StringTableIndex.Doc_Sys_Addr, "Get the integer address of given variable. this is in the form of addr.\n\n" + 
                                            "If the given value is or contains non-volatile members, a container for example, this will trigger an SS2004" },
            {StringTableIndex.UnsupportMatrixExpression, "Unsupport expression for higher-dimensional matrices." },
            {StringTableIndex.MatrixOutOfRange, "The index is out of the boundary in a matrix." }
        };

        public static string Loc(StringTableIndex index)
        {
            return StringTable[index];
        }
    }

    public enum StringTableIndex
    {
        // runtime error messages. [2000]

        // these messages are thrown by the runtime process to track the considered
        // exceptional cases. or thrown out by a strong-type check system.

        NullContainer = 2001,
        AnnonymousTypeNameReferenceFail,
        IntegralCastOutOfRange,
        NovolatileMemberGetAddrFail,
        UnsupportMatrixExpression,
        MatrixOutOfRange,

        // documentations. [10000]

        Doc_Sys_Alert = 10001,
        Doc_Sys_Ref,
        Doc_Sys_Addr,
        Doc_Sys_Dir,
        Doc_Sys_Uint8
    }
}
