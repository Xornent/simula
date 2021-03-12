using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Analysis.Record
{
    public interface IRecord
    {
        string Symbol { get; set; }

        // a record is distinguished as data-type record, type record, function record and union record
        
        // data-type records represent the built-in basic types that are directly recognizable in 
        // computers, including objects of type int8(sbyte), int16(short, char), int32, int64, uint8(byte), 
        // uint16, uint32, uint64. in the virtual machine, stacks of these types are stored separately.

        // union types represent objects with a specified type. it has a target type which points to a 
        // type record, showing the component of the fields and functions.

        // type types represent a defined combination of data-type. some kind of type record points to 
        // clr types defined in c#, and it has a copy of fields(properties) and functions defined in c#

        // function types indicates that the object is a function. both type types and function types are
        // a derivative to union types, because a function and type are objects to type 'func' or 'data'

        bool IsUnion { get; }
        bool IsClr { get; }
        RecordType RecordType { get; }

        Guid Oid { get; set; }
    }

    public enum RecordType
    {
        Data,
        Type,
        Function
    }
}
