
using System;
using System.Runtime.Serialization;
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json
{
    public enum MemberSerialization
    {
#pragma warning disable 1584,1711,1572,1581,1580,1574
        OptOut = 0,
        OptIn = 1,
        Fields = 2
#pragma warning restore 1584,1711,1572,1581,1580,1574
    }
}
