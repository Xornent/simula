﻿
using Simula.Editor.Document;
using Simula.Editor.Rendering;
using System;

namespace Simula.Editor.Highlighting
{
    /// <summary>
    /// A colorizer that applies the highlighting from a <see cref="RichTextModel"/> to the editor.
    /// </summary>
    public class RichTextColorizer : DocumentColorizingTransformer
    {
        private readonly RichTextModel richTextModel;

        /// <summary>
        /// Creates a new RichTextColorizer instance.
        /// </summary>
        public RichTextColorizer(RichTextModel richTextModel)
        {
            if (richTextModel == null)
                throw new ArgumentNullException("richTextModel");
            this.richTextModel = richTextModel;
        }

        /// <inheritdoc/>
        protected override void ColorizeLine(DocumentLine line)
        {
            var sections = richTextModel.GetHighlightedSections(line.Offset, line.Length);
            foreach (HighlightedSection section in sections) {
                if (HighlightingColorizer.IsEmptyColor(section.Color))
                    continue;
                ChangeLinePart(section.Offset, section.Offset + section.Length,
                               visualLineElement => HighlightingColorizer.ApplyColorToElement(visualLineElement, section.Color, CurrentContext));
            }
        }
    }
}
