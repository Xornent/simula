
using Simula.Editor.Document;
using Simula.Editor.Editing;
using System;
using System.Collections.Generic;
using System.Windows;

namespace Simula.Editor.Snippets
{
    /// <summary>
    /// Represents the context of a snippet insertion.
    /// </summary>
    public class InsertionContext : IWeakEventListener
    {
        private enum Status
        {
            Insertion,
            RaisingInsertionCompleted,
            Interactive,
            RaisingDeactivated,
            Deactivated
        }

        private Status currentStatus = Status.Insertion;

        /// <summary>
        /// Creates a new InsertionContext instance.
        /// </summary>
        public InsertionContext(TextArea textArea, int insertionPosition)
        {
            if (textArea == null)
                throw new ArgumentNullException("textArea");
            TextArea = textArea;
            Document = textArea.Document;
            SelectedText = textArea.Selection.GetText();
            InsertionPosition = insertionPosition;
            startPosition = insertionPosition;

            DocumentLine startLine = Document.GetLineByOffset(insertionPosition);
            ISegment indentation = TextUtilities.GetWhitespaceAfter(Document, startLine.Offset);
            Indentation = Document.GetText(indentation.Offset, Math.Min(indentation.EndOffset, insertionPosition) - indentation.Offset);
            Tab = textArea.Options.IndentationString;

            LineTerminator = TextUtilities.GetNewLineFromDocument(Document, startLine.LineNumber);
        }

        /// <summary>
        /// Gets the text area.
        /// </summary>
        public TextArea TextArea { get; private set; }

        /// <summary>
        /// Gets the text document.
        /// </summary>
        public Simula.Editor.Document.TextDocument Document { get; private set; }

        /// <summary>
        /// Gets the text that was selected before the insertion of the snippet.
        /// </summary>
        public string SelectedText { get; private set; }

        /// <summary>
        /// Gets the indentation at the insertion position.
        /// </summary>
        public string Indentation { get; private set; }

        /// <summary>
        /// Gets the indentation string for a single indentation level.
        /// </summary>
        public string Tab { get; private set; }

        /// <summary>
        /// Gets the line terminator at the insertion position.
        /// </summary>
        public string LineTerminator { get; private set; }

        /// <summary>
        /// Gets/Sets the insertion position.
        /// </summary>
        public int InsertionPosition { get; set; }

        private readonly int startPosition;
        private Simula.Editor.Document.AnchorSegment wholeSnippetAnchor;
        private bool deactivateIfSnippetEmpty;

        /// <summary>
        /// Gets the start position of the snippet insertion.
        /// </summary>
        public int StartPosition {
            get {
                if (wholeSnippetAnchor != null)
                    return wholeSnippetAnchor.Offset;
                else
                    return startPosition;
            }
        }

        /// <summary>
        /// Inserts text at the insertion position and advances the insertion position.
        /// This method will add the current indentation to every line in <paramref name="text"/> and will
        /// replace newlines with the expected newline for the document.
        /// </summary>
        public void InsertText(string text)
        {
            if (text == null)
                throw new ArgumentNullException("text");
            if (currentStatus != Status.Insertion)
                throw new InvalidOperationException();

            text = text.Replace("\t", Tab);

            using (Document.RunUpdate()) {
                int textOffset = 0;
                SimpleSegment segment;
                while ((segment = NewLineFinder.NextNewLine(text, textOffset)) != SimpleSegment.Invalid) {
                    string insertString = text.Substring(textOffset, segment.Offset - textOffset)
                        + LineTerminator + Indentation;
                    Document.Insert(InsertionPosition, insertString);
                    InsertionPosition += insertString.Length;
                    textOffset = segment.EndOffset;
                }
                string remainingInsertString = text.Substring(textOffset);
                Document.Insert(InsertionPosition, remainingInsertString);
                InsertionPosition += remainingInsertString.Length;
            }
        }

        private readonly Dictionary<SnippetElement, IActiveElement> elementMap = new Dictionary<SnippetElement, IActiveElement>();
        private readonly List<IActiveElement> registeredElements = new List<IActiveElement>();

        /// <summary>
        /// Registers an active element. Elements should be registered during insertion and will be called back
        /// when insertion has completed.
        /// </summary>
        /// <param name="owner">The snippet element that created the active element.</param>
        /// <param name="element">The active element.</param>
        public void RegisterActiveElement(SnippetElement owner, IActiveElement element)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (element == null)
                throw new ArgumentNullException("element");
            if (currentStatus != Status.Insertion)
                throw new InvalidOperationException();
            elementMap.Add(owner, element);
            registeredElements.Add(element);
        }

        /// <summary>
        /// Returns the active element belonging to the specified snippet element, or null if no such active element is found.
        /// </summary>
        public IActiveElement GetActiveElement(SnippetElement owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");
            IActiveElement element;
            if (elementMap.TryGetValue(owner, out element))
                return element;
            else
                return null;
        }

        /// <summary>
        /// Gets the list of active elements.
        /// </summary>
        public IEnumerable<IActiveElement> ActiveElements {
            get { return registeredElements; }
        }

        /// <summary>
        /// Calls the <see cref="IActiveElement.OnInsertionCompleted"/> method on all registered active elements
        /// and raises the <see cref="InsertionCompleted"/> event.
        /// </summary>
        /// <param name="e">The EventArgs to use</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
                                                         Justification = "There is an event and this method is raising it.")]
        public void RaiseInsertionCompleted(EventArgs e)
        {
            if (currentStatus != Status.Insertion)
                throw new InvalidOperationException();
            if (e == null)
                e = EventArgs.Empty;

            currentStatus = Status.RaisingInsertionCompleted;
            int endPosition = InsertionPosition;
            wholeSnippetAnchor = new AnchorSegment(Document, startPosition, endPosition - startPosition);
            TextDocumentWeakEventManager.UpdateFinished.AddListener(Document, this);
            deactivateIfSnippetEmpty = (endPosition != startPosition);

            foreach (IActiveElement element in registeredElements) {
                element.OnInsertionCompleted();
            }
            if (InsertionCompleted != null)
                InsertionCompleted(this, e);
            currentStatus = Status.Interactive;
            if (registeredElements.Count == 0) {
                // deactivate immediately if there are no interactive elements
                Deactivate(new SnippetEventArgs(DeactivateReason.NoActiveElements));
            } else {
                myInputHandler = new SnippetInputHandler(this);
                // disable existing snippet input handlers - there can be only 1 active snippet
                foreach (TextAreaStackedInputHandler h in TextArea.StackedInputHandlers) {
                    if (h is SnippetInputHandler)
                        TextArea.PopStackedInputHandler(h);
                }
                TextArea.PushStackedInputHandler(myInputHandler);
            }
        }

        private SnippetInputHandler myInputHandler;

        /// <summary>
        /// Occurs when the all snippet elements have been inserted.
        /// </summary>
        public event EventHandler InsertionCompleted;

        /// <summary>
        /// Calls the <see cref="IActiveElement.Deactivate"/> method on all registered active elements.
        /// </summary>
        /// <param name="e">The EventArgs to use</param>
        public void Deactivate(SnippetEventArgs e)
        {
            if (currentStatus == Status.Deactivated || currentStatus == Status.RaisingDeactivated)
                return;
            if (currentStatus != Status.Interactive)
                throw new InvalidOperationException("Cannot call Deactivate() until RaiseInsertionCompleted() has finished.");
            if (e == null)
                e = new SnippetEventArgs(DeactivateReason.Unknown);

            TextDocumentWeakEventManager.UpdateFinished.RemoveListener(Document, this);
            currentStatus = Status.RaisingDeactivated;
            TextArea.PopStackedInputHandler(myInputHandler);
            foreach (IActiveElement element in registeredElements) {
                element.Deactivate(e);
            }
            if (Deactivated != null)
                Deactivated(this, e);
            currentStatus = Status.Deactivated;
        }

        /// <summary>
        /// Occurs when the interactive mode is deactivated.
        /// </summary>
        public event EventHandler<SnippetEventArgs> Deactivated;

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return ReceiveWeakEvent(managerType, sender, e);
        }

        /// <inheritdoc cref="IWeakEventListener.ReceiveWeakEvent"/>
        protected virtual bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(TextDocumentWeakEventManager.UpdateFinished)) {
                // Deactivate if snippet is deleted. This is necessary for correctly leaving interactive
                // mode if Undo is pressed after a snippet insertion.
                if (wholeSnippetAnchor.Length == 0 && deactivateIfSnippetEmpty)
                    Deactivate(new SnippetEventArgs(DeactivateReason.Deleted));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds existing segments as snippet elements.
        /// </summary>
        public void Link(ISegment mainElement, ISegment[] boundElements)
        {
            var main = new SnippetReplaceableTextElement { Text = Document.GetText(mainElement) };
            RegisterActiveElement(main, new ReplaceableActiveElement(this, mainElement.Offset, mainElement.EndOffset));
            foreach (var boundElement in boundElements) {
                var bound = new SnippetBoundElement { TargetElement = main };
                var start = Document.CreateAnchor(boundElement.Offset);
                start.MovementType = AnchorMovementType.BeforeInsertion;
                start.SurviveDeletion = true;
                var end = Document.CreateAnchor(boundElement.EndOffset);
                end.MovementType = AnchorMovementType.BeforeInsertion;
                end.SurviveDeletion = true;

                RegisterActiveElement(bound, new BoundActiveElement(this, main, bound, new AnchorSegment(start, end)));
            }
        }
    }
}
