namespace Simula.Scripting.Contexts
{
    public struct Execution
    {
        public Execution(DynamicRuntime runtime, dynamic result, ExecutionFlag flag = ExecutionFlag.Go)
        {
            Runtime = runtime;
            Result = result;
            Flag = flag;
        }

        public DynamicRuntime? Runtime;
        public dynamic Result;
        public ExecutionFlag Flag;
    }

    public enum ExecutionFlag
    {
        Pass,
        Return,
        Break,
        Else,
        Continue,
        Go
    }
}