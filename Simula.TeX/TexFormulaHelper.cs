using Simula.TeX.Atoms;
using System.Diagnostics;

namespace Simula.TeX
{
    internal class TexFormulaHelper
    {
        private readonly TexFormulaParser _formulaParser;
        private readonly SourceSpan _source;

        public TexFormulaHelper(TexFormula formula, SourceSpan source)
        {
            _formulaParser = new TexFormulaParser();
            Formula = formula;
            _source = source;
        }

        public TexFormula Formula { get; }

        private TexFormula ParseFormula(string source) =>
            _formulaParser.Parse(new SourceSpan("Predefined formula fragment", source, 0, source.Length));

        public void SetFixedTypes(TexAtomType leftType, TexAtomType rightType)
        {
            Formula.RootAtom = new TypedAtom(_source, Formula.RootAtom, leftType, rightType);
        }

        public void CenterOnAxis()
        {
            Formula.RootAtom = new VerticalCenteredAtom(_source, Formula.RootAtom);
        }

        public void AddAccent(string formula, string accentName)
        {
            AddAccent(ParseFormula(formula), accentName);
        }

        public void AddAccent(TexFormula baseAtom, string accentName)
        {
            Add(new AccentedAtom(_source, baseAtom?.RootAtom, accentName));
        }

        public void AddAccent(TexFormula baseAtom, TexFormula accent)
        {
            Add(new AccentedAtom(null, baseAtom?.RootAtom, accent));
        }

        public void AddEmbraced(string formula, char leftChar, char rightChar)
        {
            AddEmbraced(ParseFormula(formula), leftChar, rightChar);
        }

        public void AddEmbraced(TexFormula formula, char leftChar, char rightChar)
        {
            AddEmbraced(formula, TexFormulaParser.GetDelimeterMapping(leftChar),
                TexFormulaParser.GetDelimeterMapping(rightChar));
        }

        public void AddEmbraced(string formula, string leftSymbol, string rightSymbol)
        {
            AddEmbraced(ParseFormula(formula), leftSymbol, rightSymbol);
        }

        public void AddEmbraced(TexFormula formula, string leftSymbol, string rightSymbol)
        {
            Add(
                new FencedAtom(
                    _source,
                    formula?.RootAtom,
                    TexFormulaParser.GetDelimiterSymbol(leftSymbol, null),
                    TexFormulaParser.GetDelimiterSymbol(rightSymbol, null)));
        }

        public void AddFraction(string numerator, string denominator, bool drawLine)
        {
            AddFraction(ParseFormula(numerator), ParseFormula(denominator), drawLine);
        }

        public void AddFraction(string numerator, TexFormula denominator, bool drawLine)
        {
            AddFraction(ParseFormula(numerator), denominator, drawLine);
        }

        public void AddFraction(string numerator, string denominator, bool drawLine, TexAlignment numeratorAlignment,
            TexAlignment denominatorAlignment)
        {
            AddFraction(ParseFormula(numerator), ParseFormula(denominator), drawLine, numeratorAlignment,
                denominatorAlignment);
        }

        public void AddFraction(TexFormula numerator, string denominator, bool drawLine)
        {
            AddFraction(numerator, ParseFormula(denominator), drawLine);
        }

        public void AddFraction(TexFormula numerator, TexFormula denominator, bool drawLine)
        {
            Add(new FractionAtom(null, numerator?.RootAtom, denominator?.RootAtom, drawLine));
        }

        public void AddFraction(TexFormula numerator, TexFormula denominator, bool drawLine,
            TexAlignment numeratorAlignment, TexAlignment denominatorAlignment)
        {
            Add(
                new FractionAtom(
                    null,
                    numerator?.RootAtom,
                    denominator?.RootAtom,
                    drawLine,
                    numeratorAlignment,
                    denominatorAlignment));
        }

        public void AddRadical(string baseFormula, string nthRoot)
        {
            AddRadical(ParseFormula(baseFormula), ParseFormula(nthRoot));
        }

        public void AddRadical(string baseFormula, TexFormula nthRoot)
        {
            AddRadical(ParseFormula(baseFormula), nthRoot);
        }

        public void AddRadical(string baseFormula)
        {
            AddRadical(ParseFormula(baseFormula));
        }

        public void AddRadical(TexFormula baseFormula, string degreeFormula)
        {
            AddRadical(baseFormula, ParseFormula(degreeFormula));
        }

        public void AddRadical(TexFormula baseFormula)
        {
            AddRadical(baseFormula, (TexFormula?)null);
        }

        public void AddRadical(TexFormula baseFormula, TexFormula? degreeFormula)
        {
            Debug.Assert(baseFormula.RootAtom != null);
            Add(new Radical(null, baseFormula.RootAtom, degreeFormula?.RootAtom));
        }

        public void AddOperator(string operatorFormula, string lowerLimitFormula, string upperLimitFormula)
        {
            AddOperator(ParseFormula(operatorFormula), ParseFormula(lowerLimitFormula),
                ParseFormula(upperLimitFormula));
        }

        public void AddOperator(string operatorFormula, string lowerLimitFormula, string upperLimitFormula,
            bool useVerticalLimits)
        {
            AddOperator(ParseFormula(operatorFormula), ParseFormula(lowerLimitFormula),
                ParseFormula(upperLimitFormula), useVerticalLimits);
        }

        public void AddOperator(string operatorFormula, bool useVerticalLimits)
        {
            AddOperator(ParseFormula(operatorFormula), null, null, useVerticalLimits);
        }

        public void AddOperator(TexFormula operatorFormula, TexFormula lowerLimitFormula, TexFormula upperLimitFormula)
        {
            Add(
                new BigOperatorAtom(
                    operatorFormula?.RootAtom?.Source,
                    operatorFormula?.RootAtom,
                    lowerLimitFormula?.RootAtom,
                    upperLimitFormula?.RootAtom));
        }

        public void AddOperator(
            TexFormula operatorFormula,
            TexFormula? lowerLimitFormula,
            TexFormula? upperLimitFormula,
            bool useVerticalLimits)
        {
            Add(
                new BigOperatorAtom(
                    operatorFormula?.RootAtom?.Source,
                    operatorFormula?.RootAtom,
                    lowerLimitFormula?.RootAtom,
                    upperLimitFormula?.RootAtom,
                    useVerticalLimits));
        }

        public void AddPhantom(string formula)
        {
            AddPhantom(ParseFormula(formula));
        }

        public void AddPhantom(string formula, bool useWidth, bool useHeight, bool useDepth)
        {
            AddPhantom(ParseFormula(formula), useWidth, useHeight, useDepth);
        }

        public void AddPhantom(TexFormula formula)
        {
            Add(new PhantomAtom(null, formula?.RootAtom));
        }

        public void AddPhantom(TexFormula phantom, bool useWidth, bool useHeight, bool useDepth)
        {
            Add(new PhantomAtom(null, phantom?.RootAtom, useWidth, useHeight, useDepth));
        }

        public void AddStrut(TexUnit unit, double width, double height, double depth)
        {
            Add(new SpaceAtom(null, unit, width, height, depth));
        }

        public void AddStrut(
            TexUnit widthUnit,
            double width,
            TexUnit heightUnit,
            double height,
            TexUnit depthUnit,
            double depth)
        {
            Add(new SpaceAtom(null, widthUnit, width, heightUnit, height, depthUnit, depth));
        }

        public void AddSymbol(string name)
        {
            Add(SymbolAtom.GetAtom(name, null));
        }

        public void AddSymbol(string name, TexAtomType type)
        {
            Add(new SymbolAtom(null, SymbolAtom.GetAtom(name, null), type));
        }

        public void Add(string formula)
        {
            Add(ParseFormula(formula));
        }

        public void Add(TexFormula formula)
        {
            Formula.Add(formula, _source);
        }

        public void Add(Atom atom)
        {
            Formula.Add(atom, _source);
        }

        public void PutAccentOver(string accentName)
        {
            Formula.RootAtom = new AccentedAtom(_source, Formula.RootAtom, accentName);
        }

        public void PutDelimiterOver(TexDelimiter delimiter)
        {
            var name = TexFormulaParser.DelimiterNames[(int)delimiter][(int)TexDelimeterType.Over];
            Formula.RootAtom = new OverUnderDelimiter(
                _source,
                Formula.RootAtom,
                null,
                SymbolAtom.GetAtom(name, null),
                TexUnit.Ex,
                0.0,
                true);
        }

        public void PutDelimiterOver(TexDelimiter delimiter, string superscriptFormula, TexUnit kernUnit, double kern)
        {
            PutDelimiterOver(delimiter, ParseFormula(superscriptFormula), kernUnit, kern);
        }

        public void PutDelimiterOver(
            TexDelimiter delimiter,
            TexFormula superscriptFormula,
            TexUnit kernUnit,
            double kern)
        {
            var name = TexFormulaParser.DelimiterNames[(int)delimiter][(int)TexDelimeterType.Over];
            Formula.RootAtom = new OverUnderDelimiter(
                _source,
                Formula.RootAtom,
                superscriptFormula?.RootAtom,
                SymbolAtom.GetAtom(name, null),
                kernUnit,
                kern,
                true);
        }

        public void PutDelimiterUnder(TexDelimiter delimiter)
        {
            var name = TexFormulaParser.DelimiterNames[(int)delimiter][(int)TexDelimeterType.Under];
            Formula.RootAtom = new OverUnderDelimiter(
                _source,
                Formula.RootAtom,
                null,
                SymbolAtom.GetAtom(name, null),
                TexUnit.Ex,
                0.0,
                false);
        }

        public void PutDelimiterUnder(TexDelimiter delimiter, string subscriptFormula, TexUnit kernUnit, double kern)
        {
            PutDelimiterUnder(delimiter, ParseFormula(subscriptFormula), kernUnit, kern);
        }

        public void PutDelimiterUnder(TexDelimiter delimiter, TexFormula subscriptName, TexUnit kernUnit, double kern)
        {
            var name = TexFormulaParser.DelimiterNames[(int)delimiter][(int)TexDelimeterType.Under];
            Formula.RootAtom = new OverUnderDelimiter(
                _source,
                Formula.RootAtom,
                subscriptName?.RootAtom,
                SymbolAtom.GetAtom(name, null),
                kernUnit,
                kern,
                false);
        }

        public void PutOver(TexFormula? overFormula, TexUnit overUnit, double overSpace, bool overScriptSize)
        {
            Formula.RootAtom = new UnderOverAtom(
                _source,
                Formula.RootAtom,
                overFormula?.RootAtom,
                overUnit,
                overSpace,
                overScriptSize,
                true);
        }

        public void PutOver(string? overFormula, TexUnit overUnit, double overSpace, bool overScriptSize)
        {
            PutOver(overFormula == null ? null : ParseFormula(overFormula), overUnit, overSpace, overScriptSize);
        }

        public void PutUnder(string? underFormula, TexUnit underUnit, double underSpace, bool underScriptSize)
        {
            PutUnder(underFormula == null ? null : ParseFormula(underFormula), underUnit, underSpace,
                underScriptSize);
        }

        public void PutUnder(TexFormula? underFormula, TexUnit underUnit, double underSpace, bool underScriptSize)
        {
            Formula.RootAtom = new UnderOverAtom(
                _source,
                Formula.RootAtom,
                underFormula?.RootAtom,
                underUnit,
                underSpace,
                underScriptSize,
                false);
        }

        public void PutUnderAndOver(string? underFormula, TexUnit underUnit, double underSpace, bool underScriptSize,
            string? over, TexUnit overUnit, double overSpace, bool overScriptSize)
        {
            PutUnderAndOver(underFormula == null ? null : ParseFormula(underFormula), underUnit, underSpace,
                underScriptSize, over == null ? null : ParseFormula(over), overUnit, overSpace, overScriptSize);
        }

        public void PutUnderAndOver(TexFormula? underFormula, TexUnit underUnit, double underSpace, bool underScriptSize,
            TexFormula? over, TexUnit overUnit, double overSpace, bool overScriptSize)
        {
            Formula.RootAtom = new UnderOverAtom(
                _source,
                Formula.RootAtom,
                underFormula?.RootAtom,
                underUnit,
                underSpace,
                underScriptSize,
                over?.RootAtom,
                overUnit,
                overSpace,
                overScriptSize);
        }
    }
}
