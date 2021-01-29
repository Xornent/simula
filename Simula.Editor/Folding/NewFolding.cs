﻿
using Simula.Editor.Document;
using System;

namespace Simula.Editor.Folding
{
    /// <summary>
    /// Helper class used for <see cref="FoldingManager.UpdateFoldings"/>.
    /// </summary>
    public class NewFolding : ISegment
    {
        /// <summary>
        /// Gets/Sets the start offset.
        /// </summary>
        public int StartOffset { get; set; }

        /// <summary>
        /// Gets/Sets the end offset.
        /// </summary>
        public int EndOffset { get; set; }

        /// <summary>
        /// Gets/Sets the name displayed for the folding.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets/Sets whether the folding is closed by default.
        /// </summary>
        public bool DefaultClosed { get; set; }

        /// <summary>
        /// Gets/Sets whether the folding is considered to be a definition.
        /// This has an effect on the 'Show Definitions only' command.
        /// </summary>
        public bool IsDefinition { get; set; }

        /// <summary>
        /// Creates a new NewFolding instance.
        /// </summary>
        public NewFolding()
        {
        }

        /// <summary>
        /// Creates a new NewFolding instance.
        /// </summary>
        public NewFolding(int start, int end)
        {
            if (!(start <= end))
                throw new ArgumentException("'start' must be less than 'end'");
            StartOffset = start;
            EndOffset = end;
            Name = null;
            DefaultClosed = false;
        }

        int ISegment.Offset {
            get { return StartOffset; }
        }

        int ISegment.Length {
            get { return EndOffset - StartOffset; }
        }
    }
}
