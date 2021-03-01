using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Simula.Scripting.Analysis
{
    // the analysis environment accomplishes the following tasks with an input of syntax trees.
    // (1) type registration  : all the runtime types will be added to a virtual array of record
    //                          in preperation for the code generation and other services.
    // (2) completeness check : the analysis enviroment check whether the available types is complete
    //                          that all objects are defined using the given set of types. it will 
    //                          announce the system for type incompleteness, typing errors and warnings
    //                          of inadequate types (such as dangling references)
    // (3) type inference     : the environment will infer the result type of executions

    public class AnalysisEnvironment
    {
        List<AnalysisScope> Scopes = new List<AnalysisScope>();

        // load method import the analysis environment declarations using a syntax tree. syntax trees
        // represents all of the uncompiled definition blocks that are available in the environment.

        public void Load(SyntaxTree syntaxTree)
        {
            RegisterSystem();
            Register(syntaxTree);
        }

        private void RegisterSystem()
        {
            var uint8 = new Interop.ClrTypeRecord(typeof(byte), "uint8");
            var uint16 = new Interop.ClrTypeRecord(typeof(ushort), "uint16");
            var uint32 = new Interop.ClrTypeRecord(typeof(uint), "uint32");
            var uint64 = new Interop.ClrTypeRecord(typeof(ulong), "uint64");
            var int8 = new Interop.ClrTypeRecord(typeof(sbyte), "int8");
            var int16 = new Interop.ClrTypeRecord(typeof(short), "int16");
            var int32 = new Interop.ClrTypeRecord(typeof(int), "int32");
            var int64 = new Interop.ClrTypeRecord(typeof(long), "int64");
            var logical = new Interop.ClrTypeRecord(typeof(bool), "logical");
            var float32 = new Interop.ClrTypeRecord(typeof(float), "float32");
            var float64 = new Interop.ClrTypeRecord(typeof(double), "float64");
        }

        private void RegisterClrType(System.Type clrType, string name = "")
        {
            var type = new Interop.ClrTypeRecord(clrType, string.IsNullOrEmpty(name) ? clrType.Name : name);
            
        }

        private void RegisterMemberFunction(Record.TypeRecord type, Record.FunctionRecord function)
        {
            type.Functions.Add(function);
        }

        private T Lookup<T>(string symbol) where T : Record.IRecord
        {

        }

        private void Register(SyntaxTree syntaxTree)
        {
            
        }
    }
}
