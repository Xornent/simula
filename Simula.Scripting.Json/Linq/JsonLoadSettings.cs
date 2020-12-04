using System;

namespace Simula.Scripting.Json.Linq
{
    public class JsonLoadSettings
    {
        private CommentHandling _commentHandling;
        private LineInfoHandling _lineInfoHandling;
        private DuplicatePropertyNameHandling _duplicatePropertyNameHandling;
        public JsonLoadSettings()
        {
            _lineInfoHandling = LineInfoHandling.Load;
            _commentHandling = CommentHandling.Ignore;
            _duplicatePropertyNameHandling = DuplicatePropertyNameHandling.Replace;
        }
        public CommentHandling CommentHandling {
            get => _commentHandling;
            set {
                if (value < CommentHandling.Ignore || value > CommentHandling.Load) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _commentHandling = value;
            }
        }
        public LineInfoHandling LineInfoHandling {
            get => _lineInfoHandling;
            set {
                if (value < LineInfoHandling.Ignore || value > LineInfoHandling.Load) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _lineInfoHandling = value;
            }
        }
        public DuplicatePropertyNameHandling DuplicatePropertyNameHandling {
            get => _duplicatePropertyNameHandling;
            set {
                if (value < DuplicatePropertyNameHandling.Replace || value > DuplicatePropertyNameHandling.Error) {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _duplicatePropertyNameHandling = value;
            }
        }
    }
}
