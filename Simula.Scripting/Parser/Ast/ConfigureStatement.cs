using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class ConfigureStatement : BlockStatement
    {
        public ConfigureStatement(IExpression type) 
        {
            this.ConfigurationObject = type;
        }

        public IExpression ConfigurationObject { get; set; }
    }
}
