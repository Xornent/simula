﻿
using System;
using System.IO;
using System.Text;

namespace Simula.Editor.Utils
{
    /// <summary>
    /// RichTextWriter implementation that writes plain text only
    /// and ignores all formatted spans.
    /// </summary>
    internal class PlainRichTextWriter : RichTextWriter
    {
        /// <summary>
        /// The text writer that was passed to the PlainRichTextWriter constructor.
        /// </summary>
        protected readonly TextWriter textWriter;
        private string indentationString = "\t";
        private int indentationLevel;
        private char prevChar;

        /// <summary>
        /// Creates a new PlainRichTextWriter instance that writes the text to the specified text writer.
        /// </summary>
        public PlainRichTextWriter(TextWriter textWriter)
        {
            if (textWriter == null)
                throw new ArgumentNullException("textWriter");
            this.textWriter = textWriter;
        }

        /// <summary>
        /// Gets/Sets the string used to indent by one level.
        /// </summary>
        public string IndentationString {
            get {
                return indentationString;
            }
            set {
                indentationString = value;
            }
        }

        /// <inheritdoc/>
        protected override void BeginUnhandledSpan()
        {
        }

        /// <inheritdoc/>
        public override void EndSpan()
        {
        }

        private void WriteIndentation()
        {
            for (int i = 0; i < indentationLevel; i++) {
                textWriter.Write(indentationString);
            }
        }

        /// <summary>
        /// Writes the indentation, if necessary.
        /// </summary>
        protected void WriteIndentationIfNecessary()
        {
            if (prevChar == '\n') {
                WriteIndentation();
                prevChar = '\0';
            }
        }

        /// <summary>
        /// Is called after a write operation.
        /// </summary>
        protected virtual void AfterWrite()
        {
        }

        /// <inheritdoc/>
        public override void Write(char value)
        {
            if (prevChar == '\n')
                WriteIndentation();
            textWriter.Write(value);
            prevChar = value;
            AfterWrite();
        }

        /// <inheritdoc/>
        public override void Indent()
        {
            indentationLevel++;
        }

        /// <inheritdoc/>
        public override void Unindent()
        {
            if (indentationLevel == 0)
                throw new NotSupportedException();
            indentationLevel--;
        }

        /// <inheritdoc/>
        public override Encoding Encoding {
            get { return textWriter.Encoding; }
        }

        /// <inheritdoc/>
        public override IFormatProvider FormatProvider {
            get { return textWriter.FormatProvider; }
        }

        /// <inheritdoc/>
        public override string NewLine {
            get {
                return textWriter.NewLine;
            }
            set {
                textWriter.NewLine = value;
            }
        }
    }
}
