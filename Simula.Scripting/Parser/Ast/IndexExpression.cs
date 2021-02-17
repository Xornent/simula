using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser.Ast
{
    public class IndexExpression : IExpression
    {
        public IndexExpression(IExpression callee)
        {
            this.Callee = callee;
        }

        public List<IExpression> Arguments { get; set; } = new List<IExpression>();
        public IExpression Callee { get; set; }
        public bool CanCache { get; set; } = true;
        public bool IsCached { get; set; } = false;
        public uint CacheId { get; set; } = 0;
        public TokenCollection Tokens { get; set; } = new TokenCollection();
    }
}
