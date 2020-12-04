
using Simula.Editor.Utils;
using System;
using System.Collections.Generic;

namespace Simula.Editor.Highlighting.Xshd
{
    /// <summary>
    /// A list of keywords.
    /// </summary>
    [Serializable]
    public class XshdKeywords : XshdElement
    {
        /// <summary>
        /// The color.
        /// </summary>
        public XshdReference<XshdColor> ColorReference { get; set; }

        private readonly NullSafeCollection<string> words = new NullSafeCollection<string>();

        /// <summary>
        /// Gets the list of key words.
        /// </summary>
        public IList<string> Words {
            get { return words; }
        }

        /// <inheritdoc/>
        public override object AcceptVisitor(IXshdVisitor visitor)
        {
            return visitor.VisitKeywords(this);
        }
    }
}
