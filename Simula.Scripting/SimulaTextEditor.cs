using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Controls;
using Simula.Editor;
using Simula.Editor.CodeCompletion;
using Simula.Editor.Document;
using Simula.Editor.Editing;
using System.Windows.Media.Effects;

namespace Simula.Scripting {

    public class SimulaTextEditor : TextEditor {
        CompletionWindow completionWindow;

        public SimulaTextEditor() : base() {
            this.SyntaxHighlighting = Simula.Editor.Highlighting.HighlightingManager.Instance.GetDefinition("Simula");
            this.FontFamily = new System.Windows.Media.FontFamily("Consolas, Simsun");
            this.FontSize = 13;

            this.TextArea.TextEntering += TextArea_TextEntering;
            this.TextArea.TextEntered += TextArea_TextEntered;
        }

        private void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            if (e.Text == ".") {
                // Open code completion after the user has pressed dot:
                completionWindow = new CompletionWindow(this.TextArea);
                IList<ICompletionData> data = completionWindow.CompletionList.CompletionData;
                foreach (var item in Completion.Data.KeywordData.Registry) {
                    data.Add(item);
                }
                completionWindow.Show();
                completionWindow.Closed += delegate {
                    completionWindow = null;
                };
            }
        }

        private void TextArea_TextEntering(object sender, System.Windows.Input.TextCompositionEventArgs e) {
            if (e.Text.Length > 0 && completionWindow != null) {
                if (!char.IsLetterOrDigit(e.Text[0])) {
                    completionWindow.CompletionList.RequestInsertion(e);
                }
            }
        }
    }
}
