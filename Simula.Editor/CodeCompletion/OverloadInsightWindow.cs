
using Simula.Editor.Editing;
using System.Windows;
using System.Windows.Input;

namespace Simula.Editor.CodeCompletion
{
    /// <summary>
    /// Insight window that shows an OverloadViewer.
    /// </summary>
    public class OverloadInsightWindow : InsightWindow
    {
        private readonly OverloadViewer overloadViewer = new OverloadViewer();

        /// <summary>
        /// Creates a new OverloadInsightWindow.
        /// </summary>
        public OverloadInsightWindow(TextArea textArea) : base(textArea)
        {
            overloadViewer.Margin = new Thickness(2, 0, 0, 0);
            Content = overloadViewer;
        }

        /// <summary>
        /// Gets/Sets the item provider.
        /// </summary>
        public IOverloadProvider Provider {
            get { return overloadViewer.Provider; }
            set { overloadViewer.Provider = value; }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled && Provider != null && Provider.Count > 1) {
                switch (e.Key) {
                    case Key.Up:
                        e.Handled = true;
                        overloadViewer.ChangeIndex(-1);
                        break;
                    case Key.Down:
                        e.Handled = true;
                        overloadViewer.ChangeIndex(+1);
                        break;
                }
                if (e.Handled) {
                    UpdateLayout();
                    UpdatePosition();
                }
            }
        }
    }
}
