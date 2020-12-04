
using Simula.Editor.Document;
using Simula.Editor.Editing;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Simula.Editor.CodeCompletion
{
    /// <summary>
    /// The code completion window.
    /// </summary>
    public class CompletionWindow : CompletionWindowBase
    {
        private readonly CompletionList completionList = new CompletionList();
        private Popup toolTip = new Popup();

        /// <summary>
        /// Gets the completion list used in this completion window.
        /// </summary>
        public CompletionList CompletionList {
            get { return completionList; }
        }

        /// <summary>
        /// Creates a new code completion window.
        /// </summary>
        public CompletionWindow(TextArea textArea) : base(textArea)
        {
            toolTip.AllowsTransparency = true;
            toolTip.UseLayoutRounding = true;

            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            BorderThickness = new Thickness(4, 1, 4, 4);

            DropShadowEffect effect = new DropShadowEffect();
            effect.Opacity = 0.25;
            effect.Direction = 90;
            effect.ShadowDepth = 0;
            effect.BlurRadius = 6;
            effect.Color = Colors.Black;
            Effect = effect;

            Border border = new Border();
            border.CornerRadius = new CornerRadius(4);
            border.Background = Brushes.White;
            border.Child = completionList;
            border.BorderThickness = new Thickness(1);
            border.BorderBrush = new SolidColorBrush(Color.FromArgb(230, 230, 230, 230));

            CloseAutomatically = true;
            SizeToContent = SizeToContent.Height;
            MaxHeight = 300;
            Width = 175;
            Content = border;
            MinHeight = 15;
            MinWidth = 30;
            toolTip.PlacementTarget = this;
            toolTip.Placement = PlacementMode.Right;
            toolTip.Closed += toolTip_Closed;

            AttachEvents();
        }

        #region ToolTip handling
        private void toolTip_Closed(object sender, EventArgs e)
        {
            // Clear content after tooltip is closed.
            // We cannot clear is immediately when setting IsOpen=false
            // because the tooltip uses an animation for closing.
            if (toolTip != null)
                toolTip.Child = null;
        }

        private void completionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = completionList.SelectedItem;
            if (item == null)
                return;
            object description = item.Description;
            if (description != null) {
                string descriptionText = description as string;
                if (descriptionText != null) {
                    toolTip.Child = new TextBlock
                    {
                        Text = descriptionText,
                        TextWrapping = TextWrapping.Wrap
                    };
                } else {
                    toolTip.Child = new ContentPresenter() { Content = description };
                }
                toolTip.IsOpen = true;
            } else {
                toolTip.IsOpen = false;
            }
        }
        #endregion

        private void completionList_InsertionRequested(object sender, EventArgs e)
        {
            Close();
            // The window must close before Complete() is called.
            // If the Complete callback pushes stacked input handlers, we don't want to pop those when the CC window closes.
            var item = completionList.SelectedItem;
            if (item != null)
                item.Complete(TextArea, new AnchorSegment(TextArea.Document, StartOffset, EndOffset - StartOffset), e);
        }

        private void AttachEvents()
        {
            completionList.InsertionRequested += completionList_InsertionRequested;
            completionList.SelectionChanged += completionList_SelectionChanged;
            TextArea.Caret.PositionChanged += CaretPositionChanged;
            TextArea.MouseWheel += textArea_MouseWheel;
            TextArea.PreviewTextInput += textArea_PreviewTextInput;
        }

        /// <inheritdoc/>
        protected override void DetachEvents()
        {
            completionList.InsertionRequested -= completionList_InsertionRequested;
            completionList.SelectionChanged -= completionList_SelectionChanged;
            TextArea.Caret.PositionChanged -= CaretPositionChanged;
            TextArea.MouseWheel -= textArea_MouseWheel;
            TextArea.PreviewTextInput -= textArea_PreviewTextInput;
            base.DetachEvents();
        }

        /// <inheritdoc/>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (toolTip != null) {
                toolTip.IsOpen = false;
                toolTip = null;
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled) {
                completionList.HandleKey(e);
            }
        }

        private void textArea_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = RaiseEventPair(this, PreviewTextInputEvent, TextInputEvent,
                                       new TextCompositionEventArgs(e.Device, e.TextComposition));
        }

        private void textArea_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = RaiseEventPair(GetScrollEventTarget(),
                                       PreviewMouseWheelEvent, MouseWheelEvent,
                                       new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta));
        }

        private UIElement GetScrollEventTarget()
        {
            if (completionList == null)
                return this;
            return completionList.ScrollViewer ?? completionList.ListBox ?? (UIElement)completionList;
        }

        /// <summary>
        /// Gets/Sets whether the completion window should close automatically.
        /// The default value is true.
        /// </summary>
        public bool CloseAutomatically { get; set; }

        /// <inheritdoc/>
        protected override bool CloseOnFocusLost {
            get { return CloseAutomatically; }
        }

        /// <summary>
        /// When this flag is set, code completion closes if the caret moves to the
        /// beginning of the allowed range. This is useful in Ctrl+Space and "complete when typing",
        /// but not in dot-completion.
        /// Has no effect if CloseAutomatically is false.
        /// </summary>
        public bool CloseWhenCaretAtBeginning { get; set; }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            int offset = TextArea.Caret.Offset;
            if (offset == StartOffset) {
                if (CloseAutomatically && CloseWhenCaretAtBeginning) {
                    Close();
                } else {
                    completionList.SelectItem(string.Empty);
                }
                return;
            }
            if (offset < StartOffset || offset > EndOffset) {
                if (CloseAutomatically) {
                    Close();
                }
            } else {
                TextDocument document = TextArea.Document;
                if (document != null) {
                    completionList.SelectItem(document.GetText(StartOffset, offset - StartOffset));
                }
            }
        }
    }
}
