﻿
using Simula.Editor.Document;
using Simula.Editor.Rendering;
using Simula.Editor.Utils;
using System.Diagnostics;

namespace Simula.Editor.Folding
{
    /// <summary>
    /// A section that can be folded.
    /// </summary>
    public sealed class FoldingSection : TextSegment
    {
        private readonly FoldingManager manager;
        private bool isFolded;
        internal CollapsedLineSection[] collapsedSections;
        private string title;

        /// <summary>
        /// Gets/sets if the section is folded.
        /// </summary>
        public bool IsFolded {
            get { return isFolded; }
            set {
                if (isFolded != value) {
                    isFolded = value;
                    ValidateCollapsedLineSections(); // create/destroy CollapsedLineSection
                    manager.Redraw(this);
                }
            }
        }

        internal void ValidateCollapsedLineSections()
        {
            if (!isFolded) {
                RemoveCollapsedLineSection();
                return;
            }
            // It is possible that StartOffset/EndOffset get set to invalid values via the property setters in TextSegment,
            // so we coerce those values into the valid range.
            DocumentLine startLine = manager.document.GetLineByOffset(StartOffset.CoerceValue(0, manager.document.TextLength));
            DocumentLine endLine = manager.document.GetLineByOffset(EndOffset.CoerceValue(0, manager.document.TextLength));
            if (startLine == endLine) {
                RemoveCollapsedLineSection();
            } else {
                if (collapsedSections == null)
                    collapsedSections = new CollapsedLineSection[manager.textViews.Count];
                // Validate collapsed line sections
                DocumentLine startLinePlusOne = startLine.NextLine;
                for (int i = 0; i < collapsedSections.Length; i++) {
                    var collapsedSection = collapsedSections[i];
                    if (collapsedSection == null || collapsedSection.Start != startLinePlusOne || collapsedSection.End != endLine) {
                        // recreate this collapsed section
                        if (collapsedSection != null) {
                            Debug.WriteLine("CollapsedLineSection validation - recreate collapsed section from " + startLinePlusOne + " to " + endLine);
                            collapsedSection.Uncollapse();
                        }
                        collapsedSections[i] = manager.textViews[i].CollapseLines(startLinePlusOne, endLine);
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnSegmentChanged()
        {
            ValidateCollapsedLineSections();
            base.OnSegmentChanged();
            // don't redraw if the FoldingSection wasn't added to the FoldingManager's collection yet
            if (IsConnectedToCollection)
                manager.Redraw(this);
        }

        /// <summary>
        /// Gets/Sets the text used to display the collapsed version of the folding section.
        /// </summary>
        public string Title {
            get {
                return title;
            }
            set {
                if (title != value) {
                    title = value;
                    if (IsFolded)
                        manager.Redraw(this);
                }
            }
        }

        /// <summary>
        /// Gets the content of the collapsed lines as text.
        /// </summary>
        public string TextContent {
            get {
                return manager.document.GetText(StartOffset, EndOffset - StartOffset);
            }
        }

        /// <summary>
        /// Gets/Sets an additional object associated with this folding section.
        /// </summary>
        public object Tag { get; set; }

        internal FoldingSection(FoldingManager manager, int startOffset, int endOffset)
        {
            Debug.Assert(manager != null);
            this.manager = manager;
            StartOffset = startOffset;
            Length = endOffset - startOffset;
        }

        private void RemoveCollapsedLineSection()
        {
            if (collapsedSections != null) {
                foreach (var collapsedSection in collapsedSections) {
                    if (collapsedSection != null && collapsedSection.Start != null)
                        collapsedSection.Uncollapse();
                }
                collapsedSections = null;
            }
        }
    }
}
