﻿
using Simula.Editor.Document;
using Simula.Editor.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Simula.Editor.Highlighting
{
    /// <summary>
    /// A RichTextWriter that writes into a document and RichTextModel.
    /// </summary>
    internal class RichTextModelWriter : PlainRichTextWriter
    {
        private readonly RichTextModel richTextModel;
        private readonly DocumentTextWriter documentTextWriter;
        private readonly Stack<HighlightingColor> colorStack = new Stack<HighlightingColor>();
        private HighlightingColor currentColor;
        private int currentColorBegin = -1;

        /// <summary>
        /// Creates a new RichTextModelWriter that inserts into document, starting at insertionOffset.
        /// </summary>
        public RichTextModelWriter(RichTextModel richTextModel, IDocument document, int insertionOffset)
            : base(new DocumentTextWriter(document, insertionOffset))
        {
            if (richTextModel == null)
                throw new ArgumentNullException("richTextModel");
            this.richTextModel = richTextModel;
            documentTextWriter = (DocumentTextWriter)base.textWriter;
            currentColor = richTextModel.GetHighlightingAt(Math.Max(0, insertionOffset - 1));
        }

        /// <summary>
        /// Gets/Sets the current insertion offset.
        /// </summary>
        public int InsertionOffset {
            get { return documentTextWriter.InsertionOffset; }
            set { documentTextWriter.InsertionOffset = value; }
        }


        /// <inheritdoc/>
        protected override void BeginUnhandledSpan()
        {
            colorStack.Push(currentColor);
        }

        private void BeginColorSpan()
        {
            WriteIndentationIfNecessary();
            colorStack.Push(currentColor);
            currentColor = currentColor.Clone();
            currentColorBegin = documentTextWriter.InsertionOffset;
        }

        /// <inheritdoc/>
        public override void EndSpan()
        {
            currentColor = colorStack.Pop();
            currentColorBegin = documentTextWriter.InsertionOffset;
        }

        /// <inheritdoc/>
        protected override void AfterWrite()
        {
            base.AfterWrite();
            richTextModel.SetHighlighting(currentColorBegin, documentTextWriter.InsertionOffset - currentColorBegin, currentColor);
        }

        /// <inheritdoc/>
        public override void BeginSpan(Color foregroundColor)
        {
            BeginColorSpan();
            currentColor.Foreground = new SimpleHighlightingBrush(foregroundColor);
            currentColor.Freeze();
        }

        /// <inheritdoc/>
        public override void BeginSpan(FontFamily fontFamily)
        {
            BeginUnhandledSpan(); // TODO
        }

        /// <inheritdoc/>
        public override void BeginSpan(FontStyle fontStyle)
        {
            BeginColorSpan();
            currentColor.FontStyle = fontStyle;
            currentColor.Freeze();
        }

        /// <inheritdoc/>
        public override void BeginSpan(FontWeight fontWeight)
        {
            BeginColorSpan();
            currentColor.FontWeight = fontWeight;
            currentColor.Freeze();
        }

        /// <inheritdoc/>
        public override void BeginSpan(HighlightingColor highlightingColor)
        {
            BeginColorSpan();
            currentColor.MergeWith(highlightingColor);
            currentColor.Freeze();
        }
    }
}
