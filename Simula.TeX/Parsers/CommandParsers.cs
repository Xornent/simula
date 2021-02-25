using Simula.TeX.Atoms;
using Simula.TeX.Exceptions;

namespace Simula.TeX.Parsers
{
    /// <summary>A context that will be passed to the command parser.</summary>
    internal class CommandContext
    {
        /// <summary>TeX formula parser that calls the command.</summary>
        public TexFormulaParser Parser { get; }

        /// <summary>Current formula that is being constructed.</summary>
        public TexFormula Formula { get; }

        /// <summary>An environment in which the command should be parsed.</summary>
        public ICommandEnvironment Environment { get; }

        /// <summary>Source of the current command: includes both the command name and the arguments.</summary>
        public SourceSpan CommandSource { get; }

        /// <summary>
        /// A position inside of source where the command name start. Useful to provide the source information, not for
        /// the parsing itself.
        /// </summary>
        public int CommandNameStartPosition { get; }

        /// <summary>
        /// A position inside of source where the command arguments start. Should be a parser start position.
        /// </summary>
        public int ArgumentsStartPosition { get; }

        public CommandContext(
            TexFormulaParser parser,
            TexFormula formula,
            ICommandEnvironment environment,
            SourceSpan commandSource,
            int commandNameStartPosition,
            int argumentsStartPosition)
        {
            Parser = parser;
            Formula = formula;
            Environment = environment;
            CommandSource = commandSource;
            CommandNameStartPosition = commandNameStartPosition;
            ArgumentsStartPosition = argumentsStartPosition;
        }
    }

    internal class CommandProcessingResult
    {
        /// <summary>A parsed atom. May be <c>null</c>.</summary>
        public Atom Atom { get; }

        /// <summary>
        /// A position pointing to the part of the <see cref="CommandContext.CommandSource"/> where the parsing should
        /// proceed.
        /// </summary>
        public int NextPosition { get; }

        public CommandProcessingResult(Atom atom, int nextPosition)
        {
            Atom = atom;
            NextPosition = nextPosition;
        }
    }

    /// <summary>
    /// A parser for a particular command that should read the command name and its arguments and produce an
    /// <see cref="Atom"/>, and also move the parser position towards the end.
    /// </summary>
    internal interface ICommandParser
    {
        /// <summary>Parsing of the command arguments.</summary>
        /// <param name="context">The context of the command.</param>
        /// <returns>The parsing result, never <c>null</c>.</returns>
        /// <exception cref="TexParseException">Should be thrown on any error.</exception>
        CommandProcessingResult ProcessCommand(CommandContext context);
    }
}
