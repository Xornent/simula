using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Parser
{
    public enum SyntaxError
    {
        Ok = 0,
        OperatorUndefined = 101,
        ElseIfPlacement,
        ElsePlacement,
        CatchPlacement,
        EndOverflow,
        EmptyExpression,
        UnpairedBrackets,
        MatrixEmptyElement,
        MatrixNotUniform,
        UnrecognizedCommandment,
        CommandmentParameterCountNotMatch,
        CatchUnnamedException,
        StandaloneCatch,
        CatchNotFollowingTry,
        StandaloneEif,
        EifNotFollowingIfEif,
        StandaloneElse,
        ElseNotFollowingIfEif,
        UndefinedConditionalTarget,
        UndefinedConfigureTarget,
        IfConditionMissing,
        EifConditionMissing,
        WhileConditionMissing,
        MatchDeclarationMissing,
        DataAssertionSyntaxError,
        DataInheritageSyntaxError,
        InvalidParameter,
        ExpectLiteralParameterName,
        ExpectLiteralModifers,
        DataDeclarationMissing,
        FunctionDeclarationMissing,
        FunctionReturnTypeSyntaxError,
        IterateAtSyntaxError,
        IterateInSyntaxError,
        IterateConstantSyntaxError,
        DeclarationSyntaxError,
        ExpectDeclaration,
        AssignToReadonlyValues,
        InvalidTypeCalc,
        InvalidType,
        MembersWithTheSameIdentifer,

        // internal parser errors from 901 - 999. this indicates that the parser design has logical
        // errors, and for any code, a parser should not throw any internal errors, this is for the
        // purpose of exception case handling.
        
        EmptyTokenForBlock = 901,
        InternalMatrixAssignment
    }
}
