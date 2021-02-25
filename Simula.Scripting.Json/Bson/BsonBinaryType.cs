using System;

namespace Simula.Scripting.Json.Bson
{
    internal enum BsonBinaryType : byte
    {
        Binary = 0x00,
        Function = 0x01,

        [Obsolete("This type has been deprecated in the BSON specification. Use Binary instead.")]
        BinaryOld = 0x02,

        [Obsolete("This type has been deprecated in the BSON specification. Use Uuid instead.")]
        UuidOld = 0x03,
        Uuid = 0x04,
        Md5 = 0x05,
        UserDefined = 0x80
    }
}