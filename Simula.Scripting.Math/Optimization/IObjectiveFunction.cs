using Simula.Maths.LinearAlgebra;

namespace Simula.Maths.Optimization
{
    /// <summary>
    /// Objective function with a frozen evaluation that must not be changed from the outside.
    /// </summary>
    public interface IObjectiveFunctionEvaluation
    {
        /// <summary>Create a new unevaluated and independent copy of this objective function</summary>
        IObjectiveFunction CreateNew();

        Vector<double> Point { get; }
        double Value { get; }

        bool IsGradientSupported { get; }
        Vector<double> Gradient { get; }

        bool IsHessianSupported { get; }
        Matrix<double> Hessian { get; }
    }

    /// <summary>
    /// Objective function with a mutable evaluation.
    /// </summary>
    public interface IObjectiveFunction : IObjectiveFunctionEvaluation
    {
        void EvaluateAt(Vector<double> point);

        /// <summary>Create a new independent copy of this objective function, evaluated at the same point.</summary>
        IObjectiveFunction Fork();
    }

    public interface IScalarObjectiveFunctionEvaluation
    {
        double Point { get; }
        double Value { get; }
        double Derivative { get; }
        double SecondDerivative { get; }
    }

    public interface IScalarObjectiveFunction
    {
        bool IsDerivativeSupported { get; }
        bool IsSecondDerivativeSupported { get; }
        IScalarObjectiveFunctionEvaluation Evaluate(double point);
    }
}
