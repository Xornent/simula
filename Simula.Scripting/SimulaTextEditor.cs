using Simula.Editor;
using Simula.Editor.CodeCompletion;
using Simula.Editor.Document;
using Simula.Editor.Folding;
using Simula.Editor.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Xml;

namespace Simula.Scripting
{
    public class SimulaTextEditor : TextEditor
    {
        private CompletionWindow? completionWindow;
        private readonly TextMarkerService textMarkerService;
        private Popup? toolTip;
        private readonly System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker();
        private bool usingValidation = false;
        private readonly FoldingManager manager;
        private readonly SimulaFoldingStrategy folding = new SimulaFoldingStrategy();

        public static readonly string fontFamily = "consolas, simsun";
        public Contexts.DynamicRuntime runtime = new Contexts.DynamicRuntime();
        public Contexts.CompletionContext completionCtx;

        public SimulaTextEditor() : base()
        {
            textMarkerService = new TextMarkerService(this);
            TextView textView = TextArea.TextView;
            SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
            FontFamily = new System.Windows.Media.FontFamily(fontFamily);
            FontSize = 13.5;
            TextView.LineHeightScale = 1.1;
            
            textView.BackgroundRenderers.Add(textMarkerService);
            textView.LineTransformers.Add(textMarkerService);
            textView.Services.AddService(typeof(TextMarkerService), textMarkerService);

            TextArea.TextEntering += TextArea_TextEntering;
            TextArea.TextEntered += TextArea_TextEntered;
            textView.MouseHover += HandleMouseHover;
            textView.MouseHoverStopped += TextEditorMouseHoverStopped;
            textView.VisualLinesChanged += HandleVisualLinesChanged;
            TextChanged += HandleTextChanged;
            worker.DoWork += Worker_DoWork;
            worker.RunWorkerCompleted += Worker_RunWorkerCompleted;

            manager = FoldingManager.Install(TextArea);
            folding = new SimulaFoldingStrategy();
            folding.UpdateFoldings(manager, Document);
            this.completionCtx = new Contexts.CompletionContext(runtime);
        }

        private void HandleTextChanged(object? sender, EventArgs e)
        {
            usingValidation = true;
            // worker.RunWorkerAsync();
            Validate(null, null);
            folding.UpdateFoldings(manager, Document);
            usingValidation = false;
        }

        private void Worker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            usingValidation = false;
        }

        private void Worker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            Validate(null, null);
        }

        private void HandleMouseHover(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var pos = TextArea.TextView.GetPositionFloor(e.GetPosition(TextArea.TextView) + TextArea.TextView.ScrollOffset);
            bool inDocument = pos.HasValue;
            if (inDocument && (!usingValidation)) {
                TextLocation logicalPosition = pos.Value.Location;
                int offset = Document.GetOffset(logicalPosition);

                var markersAtOffset = textMarkerService.GetMarkersAtOffset(offset);
                TextMarkerService.TextMarker markerWithToolTip = markersAtOffset.FirstOrDefault(marker => marker.ToolTip != null);

                if (markerWithToolTip != null) {
                    if (markerWithToolTip.ToolTip != null) {
                        if (toolTip == null) {
                            toolTip = new Popup();
                            toolTip.Closed += ToolTipClosed;
                            toolTip.PlacementTarget = this;

                            Border border = new Border();
                            Grid contain = new Grid();
                            border.SnapsToDevicePixels = true;
                            border.BorderThickness = new System.Windows.Thickness(1, 1, 1, 1);
                            border.Background = Brushes.White;
                            border.CornerRadius = new System.Windows.CornerRadius(2);
                            border.Margin = new System.Windows.Thickness(2, 2, 6, 6);
                            border.BorderBrush = new SolidColorBrush(Color.FromArgb(230, 230, 230, 230));
                            border.Effect = new DropShadowEffect()
                            {
                                ShadowDepth = 1,
                                BlurRadius = 4,
                                Opacity = 0.15
                            };
                            contain.Children.Add(border);

                            ScrollViewer scroll = new ScrollViewer();
                            scroll.SnapsToDevicePixels = true;
                            scroll.MaxWidth = 400;
                            scroll.MaxHeight = 400;

                            scroll.Content = markerWithToolTip.ToolTip;
                            border.Child = scroll;

                            toolTip.Child = contain;
                            TextOptions.SetTextFormattingMode(toolTip, TextFormattingMode.Display);
                            toolTip.AllowsTransparency = true;
                            toolTip.Placement = PlacementMode.Mouse;
                            toolTip.PlacementTarget = this;
                            toolTip.IsOpen = true;

                            e.Handled = true;
                        }
                    }
                }
            }
        }

        private void ToolTipClosed(object sender, EventArgs e)
        {
            toolTip = null;
        }

        private void TextEditorMouseHoverStopped(object sender, MouseEventArgs e)
        {
            if (toolTip != null) {
                toolTip.IsOpen = false;
                e.Handled = true;
            }
        }

        private void HandleVisualLinesChanged(object sender, EventArgs e)
        {
            if (toolTip != null) {
                toolTip.IsOpen = false;
            }
        }

        private void Validate(object sender, ExecutedRoutedEventArgs e)
        {
            IServiceProvider sp = this;
            var markerService = (TextMarkerService)sp.GetService(typeof(TextMarkerService));
            markerService.Clear();

            Token.TokenDocument doc = new Token.TokenDocument();
            
            try {
                doc.Tokenize(this.Text);
                Syntax.BlockStatement block = new Syntax.BlockStatement();
                block.Parse(doc.Tokens);
            } catch { } finally {
                foreach (var item in doc.Tokens ?? new Token.TokenCollection()) {
                    if (item.HasError) {
                        StackPanel stack = new StackPanel();
                        stack.Margin = new Thickness(4);
                        stack.MaxWidth = 500;
                        stack.Orientation = Orientation.Vertical;
                        TextBlock textMessage = new TextBlock();
                        textMessage.Text = "[" + item.Error?.Id.ToUpper() + "] " + item.Error?.Message;
                        textMessage.FontFamily = new FontFamily(fontFamily);
                        textMessage.FontWeight = FontWeights.Bold;
                        textMessage.TextWrapping = TextWrapping.Wrap;
                        stack.Children.Add(textMessage);

                        if (!string.IsNullOrEmpty(item.Error?.Help ?? "")) {
                            TextEditor editor = new TextEditor();
                            editor.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
                            editor.FontFamily = new System.Windows.Media.FontFamily(fontFamily);
                            editor.FontSize = 13;
                            editor.Text = "";

                            TextBlock textHelp = new TextBlock();
                            textHelp.FontFamily = new FontFamily(fontFamily);
                            textHelp.TextWrapping = TextWrapping.Wrap;

                            string[] lines = (item.Error?.Help ?? "").Split('\n');
                            bool isincode = false;
                            foreach (var ln in lines) {
                                if (ln.Trim() == "```") {
                                    if (isincode) {
                                        isincode = false;
                                        stack.Children.Add(new Grid() { Height = 10 });
                                        stack.Children.Add(editor);
                                        editor = new TextEditor();
                                        editor.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
                                        editor.FontFamily = new System.Windows.Media.FontFamily(fontFamily);
                                        editor.FontSize = 13;
                                        editor.Text = "";
                                    } else {
                                        isincode = true;
                                        stack.Children.Add(new Grid() { Height = 10 });
                                        stack.Children.Add(textHelp);
                                        textHelp = new TextBlock();
                                        textHelp.FontFamily = new FontFamily(fontFamily);
                                        textHelp.TextWrapping = TextWrapping.Wrap;
                                    }
                                    continue;
                                }

                                if (isincode) {
                                    if (editor.Text == "") editor.Text += ln;
                                    else editor.Text += ("\n" + ln);
                                } else {
                                    if (textHelp.Text == "") textHelp.Text += ln;
                                    else textHelp.Text += ("\n" + ln);
                                }
                            }

                            stack.Children.Add(new Grid() { Height = 10 });
                            stack.Children.Add(textHelp);
                        }

                        DisplayValidationError(stack, item.Location.End.Column + 1, item.Location.End.Line);
                    }
                }
            }
        }

        private void DisplayValidationError(UIElement message, int linePosition, int lineNumber)
        {
            if (lineNumber >= 1 && lineNumber <= Document.LineCount) {
                int endOffset = Document.GetOffset(new TextLocation(lineNumber, linePosition));
                int offset = TextUtilities.GetNextCaretPosition(Document, endOffset, System.Windows.Documents.LogicalDirection.Backward, CaretPositioningMode.WordBorderOrSymbol);
                if (endOffset < 0) {
                    endOffset = Document.TextLength;
                }
                int length = endOffset - offset;

                if (length < 2) {
                    length = Math.Min(2, Document.TextLength - offset);
                }

                textMarkerService.Create(offset, length, message);
            }
        }

        private void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {

        }

        bool closeCompletion = true;
        private async void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            ScrollToLine(TextArea.Caret.Line);
            
            if (e.Text.Length > 0 && (completionWindow != null)) {
                if (!char.IsLetterOrDigit(e.Text[0])) {
                    completionWindow.Close();

                    // the commented command requested that every time user press whitespace,
                    // the selected item is completed by autocompleter. this may cause great 
                    // inconvinience since the completion engine is not so intelligent.

                    // completionWindow.CompletionList.RequestInsertion(e);
                }
            } else if (char.IsLetter(e.Text[0]) || e.Text == "_") {

                // call completion provider with TextArea.Caret locations, full source
                // file and current dynamic runtime.

                completionCtx.SetSource(this.Text);
                Contexts.CompletionCaret caret = completionCtx.GetCaret(TextArea.Caret.Line, TextArea.Caret.Column);
                Contexts.CompletionProvider provider = new Contexts.CompletionProvider(completionCtx, caret);
                var availableKeys = provider.GetCompletionData();

                if (availableKeys.Count > 0) {
                    if (completionWindow == null)
                        completionWindow = new CompletionWindow(TextArea);
                    IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                    data.Clear();

                    availableKeys.Sort((a, b) => { return string.Compare(a.Text, b.Text); });
                    availableKeys.Where((x, i) => availableKeys.FindIndex(z => z.Text == x.Text) == i);

                    foreach (var item in availableKeys) {
                        data.Add(item);
                    }

                    completionWindow.Closed += (obj, e) => { completionWindow = null; };
                    completionWindow.Show();
                }
            }
        }
    }

    public class TextMarkerService : IBackgroundRenderer, IVisualLineTransformer
    {
        private readonly TextEditor textEditor;
        private readonly TextSegmentCollection<TextMarker> markers;

        public sealed class TextMarker : TextSegment
        {
            public TextMarker(int startOffset, int length)
            {
                StartOffset = startOffset;
                Length = length;
            }

            public Color? BackgroundColor { get; set; }
            public Color MarkerColor { get; set; }
            public UIElement? ToolTip { get; set; }
        }

        public TextMarkerService(TextEditor textEditor)
        {
            this.textEditor = textEditor;
            markers = new TextSegmentCollection<TextMarker>(textEditor.Document);
        }

        public void Draw(TextView textView, DrawingContext drawingContext)
        {
            if (markers == null || !textView.VisualLinesValid) {
                return;
            }
            var visualLines = textView.VisualLines;
            if (visualLines.Count == 0) {
                return;
            }
            int viewStart = visualLines.First().FirstDocumentLine.Offset;
            int viewEnd = visualLines.Last().LastDocumentLine.EndOffset;
            foreach (TextMarker marker in markers.FindOverlappingSegments(viewStart, viewEnd - viewStart)) {
                if (marker.BackgroundColor != null) {
                    var geoBuilder = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 3 };
                    geoBuilder.AddSegment(textView, marker);
                    Geometry geometry = geoBuilder.CreateGeometry();
                    if (geometry != null) {
                        Color color = marker.BackgroundColor.Value;
                        var brush = new SolidColorBrush(color);
                        brush.Freeze();
                        drawingContext.DrawGeometry(brush, null, geometry);
                    }
                }

                foreach (Rect r in BackgroundGeometryBuilder.GetRectsForSegment(textView, marker)) {
                    Point startPoint = r.BottomLeft;
                    Point endPoint = r.BottomRight;

                    var usedPen = new Pen(new SolidColorBrush(marker.MarkerColor), 1);
                    usedPen.Freeze();
                    const double offset = 2.5;

                    int count = Math.Max((int)((endPoint.X - startPoint.X) / offset) + 1, 4);

                    var geometry = new StreamGeometry();

                    using (StreamGeometryContext ctx = geometry.Open()) {
                        ctx.BeginFigure(startPoint, false, false);
                        ctx.PolyLineTo(CreatePoints(startPoint, endPoint, offset, count).ToArray(), true, false);
                    }

                    geometry.Freeze();

                    drawingContext.DrawGeometry(Brushes.Transparent, usedPen, geometry);
                    break;
                }
            }
        }

        public KnownLayer Layer {
            get { return KnownLayer.Selection; }
        }

        public void Transform(ITextRunConstructionContext context, IList<VisualLineElement> elements) { }

        private IEnumerable<Point> CreatePoints(Point start, Point end, double offset, int count)
        {
            for (int i = 0; i < count; i++) {
                yield return new Point(start.X + (i * offset), start.Y - ((i + 1) % 2 == 0 ? offset : 0));
            }
        }

        public void Clear()
        {
            foreach (TextMarker m in markers) {
                Remove(m);
            }
        }

        private void Remove(TextMarker marker)
        {
            if (markers.Remove(marker)) {
                Redraw(marker);
            }
        }

        private void Redraw(ISegment segment)
        {
            textEditor.TextArea.TextView.Redraw(segment);
        }

        public void Create(int offset, int length, UIElement message)
        {
            var m = new TextMarker(offset, length);
            markers.Add(m);
            m.MarkerColor = Colors.Red;
            m.ToolTip = message;
            Redraw(m);
        }

        public IEnumerable<TextMarker> GetMarkersAtOffset(int offset)
        {
            return markers == null ? Enumerable.Empty<TextMarker>() : markers.FindSegmentsContaining(offset);
        }
    }

    public class SimulaFoldingStrategy
    {
        public bool ShowAttributesWhenFolded { get; set; }

        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            int firstErrorOffset;
            IEnumerable<NewFolding> foldings = CreateNewFoldings(document, out firstErrorOffset);
            manager.UpdateFoldings(foldings, firstErrorOffset);
        }

        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            try {
                List<NewFolding> foldings = new List<NewFolding>();
                Stack<NewFolding> context = new Stack<NewFolding>();

                int position = 0;
                foreach (var ln in document.Text.Split('\n')) {
                    string s = ln.Trim();
                    position += (ln.Length - ln.TrimStart().Length);
                    if (s.StartsWith("while")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() });
                    if (s.StartsWith("expose")) {
                        string str = s.Remove(0, 6).Trim();
                        if (str.StartsWith("def")) s = s.Remove(0, 6).Trim();
                        if (str.StartsWith("func")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() }); ;
                        if (str.StartsWith("class")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() }); ;
                    }
                    if (s.StartsWith("hidden")) {
                        string str = s.Remove(0, 6).Trim();
                        if (str.StartsWith("def")) s = s.Remove(0, 6).Trim();
                        if (str.StartsWith("func")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() }); ;
                        if (str.StartsWith("class")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() }); ;
                    }
                    if (s.StartsWith("def")) {
                        string str = s.Remove(0, 3).Trim();
                        if (str.StartsWith("func")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() }); ;
                        if (str.StartsWith("class")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() }); ;
                    }

                    if (s.StartsWith("if")) context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() }); ;
                    if (s.StartsWith("eif")) {
                        if (context.Count > 0) {
                            var fold = context.Pop();
                            fold.EndOffset = position - (ln.Length - ln.TrimStart().Length) - 2;
                            foldings.Add(fold);
                        }

                        context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() });
                    }
                    if (s.StartsWith("else")) {
                        if (context.Count > 0) {
                            var fold = context.Pop();
                            fold.EndOffset = position - (ln.Length - ln.TrimStart().Length) - 2;
                            foldings.Add(fold);
                        }

                        context.Push(new NewFolding() { StartOffset = position, Name = ln.Trim() });
                    }
                    if (s.StartsWith("end")) {
                        if (context.Count > 0) {
                            var fold = context.Pop();
                            fold.EndOffset = position - (ln.Length - ln.TrimStart().Length) - 2;
                            foldings.Add(fold);
                        }
                    }

                    position += ln.TrimStart().Length + 1;
                }

                firstErrorOffset = 0;
                foldings.Sort((X, Y) => { return X.StartOffset.CompareTo(Y.StartOffset); });
                return foldings;
            } catch (XmlException) {
                firstErrorOffset = 0;
                return Enumerable.Empty<NewFolding>();
            }
        }
    }
}
