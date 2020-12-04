
using Simula.Editor.Utils;
using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;

namespace Simula.Editor
{
    /// <summary>
    /// Exposes <see cref="Simula.Editor.TextEditor"/> to automation.
    /// </summary>
    public class TextEditorAutomationPeer : FrameworkElementAutomationPeer, IValueProvider
    {
        /// <summary>
        /// Creates a new TextEditorAutomationPeer instance.
        /// </summary>
        public TextEditorAutomationPeer(TextEditor owner) : base(owner)
        {
            Debug.WriteLine("TextEditorAutomationPeer was created");
        }

        private TextEditor TextEditor {
            get { return (TextEditor)base.Owner; }
        }

        void IValueProvider.SetValue(string value)
        {
            TextEditor.Text = value;
        }

        string IValueProvider.Value {
            get { return TextEditor.Text; }
        }

        bool IValueProvider.IsReadOnly {
            get { return TextEditor.IsReadOnly; }
        }

        /// <inheritdoc/>
        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Document;
        }

        /// <inheritdoc/>
        public override object GetPattern(PatternInterface patternInterface)
        {
            if (patternInterface == PatternInterface.Value)
                return this;

            if (patternInterface == PatternInterface.Scroll) {
                ScrollViewer scrollViewer = TextEditor.ScrollViewer;
                if (scrollViewer != null)
                    return UIElementAutomationPeer.FromElement(scrollViewer);
            }

            if (patternInterface == PatternInterface.Text) {
                return FromElement(TextEditor.TextArea);
            }

            return base.GetPattern(patternInterface);
        }

        internal void RaiseIsReadOnlyChanged(bool oldValue, bool newValue)
        {
            RaisePropertyChangedEvent(ValuePatternIdentifiers.IsReadOnlyProperty, Boxes.Box(oldValue), Boxes.Box(newValue));
        }
    }
}
