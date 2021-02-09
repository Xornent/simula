using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public enum LexicalError
    {
        RuleNotMatch = -1,
        Ok = 0,
        UnexpectedToken = 1,
        UnexpectedNumeral
    }
}
