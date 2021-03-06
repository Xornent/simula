using System;
using System.Collections.Generic;
using System.Text;
using Simula.Scripting.Parser.Ast;

namespace Simula.Scripting.Parser
{
    public class ParserResult
    {
        public string File { get; set; } = "";
        public List<ParserError> Diagnostics = new List<ParserError>();

        public bool Successful 
        {
            get {
                int fatalCount = 0;
                foreach (var diag in this.Diagnostics){
                    if (diag.Severity == Severity.Fatal) fatalCount++;
                }

                return fatalCount == 0;
            }
        }

        public int Fatals {
            get {
                int fatalCount = 0;
                foreach (var diag in this.Diagnostics) {
                    if (diag.Severity == Severity.Fatal) fatalCount++;
                }

                return fatalCount;
            }
        }

        public int Warnings {
            get {
                int warningCount = 0;
                foreach (var diag in this.Diagnostics) {
                    if (diag.Severity == Severity.Warning) warningCount++;
                }

                return warningCount;
            }
        }

        public void AddFatal(SyntaxError error, Token token)
        {
            Diagnostics.Add(new ParserError(error, token, this.File, Severity.Fatal));
        }

        public void AddFatal(SyntaxError error, Span span)
        {
            Diagnostics.Add(new ParserError(error, span, this.File, Severity.Fatal));
        }

        public void AddWarning(SyntaxError error, Token token)
        { 
            Diagnostics.Add(new ParserError(error, token, this.File, Severity.Warning));
        }

        public void AddWarning(SyntaxError error, Span span)
        {
            Diagnostics.Add(new ParserError(error, span, this.File, Severity.Warning));
        }

        public void AddInformation(SyntaxError error, Token token)
        {
            Diagnostics.Add(new ParserError(error, token, this.File, Severity.Information));
        }

        public void AddInformation(SyntaxError error, Span span)
        {
            Diagnostics.Add(new ParserError(error, span, this.File, Severity.Information));
        }
    }

    public class ParserError
    {
        public ParserError(SyntaxError error, Span location, string file, Severity severity = Severity.Fatal)
        {
            this.Error = error;
            this.Location = location;
            this.Severity = severity;
        }

        public ParserError(SyntaxError error, Token token, string file, Severity severity = Severity.Fatal)
        {
            this.Error = error;
            this.Location = token.Location;
            this.Severity = severity;
        }

        public Span Location { get; set; }
        public string File { get; set; }
        public SyntaxError Error { get; set; }
        public Severity Severity { get; set; }
    }

    public enum Severity
    {
        Information,
        Warning,
        Fatal
    }
}
