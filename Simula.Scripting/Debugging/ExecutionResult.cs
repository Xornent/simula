using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Simula.Scripting.Reflection;
using Simula.Scripting.Type;

namespace Simula.Scripting.Debugging {

    public class ExecutionResult {
        public ExecutionResult() {
            
        }

        public ExecutionResult(uint pointer, Compilation.RuntimeContext ctx) : base() {
            if(ctx.Pointers.Any()) {
                if(ctx.Pointers.ContainsKey(pointer)) {
                    this.Pointer = pointer;
                    this.Result = ctx.Pointers[pointer];
                } else {
                    this.Pointer = 0;
                    if (ctx.Pointers[0] != Global.Null) 
                        ctx.Pointers[0] = Global.Null;

                    this.Result = ctx.Pointers[0];
                }
            } else {
                ctx.Pointers.Add(0, Global.Null);
            }
        }

        public ExecutionResult(uint pointer, Compilation.RuntimeContext ctx, ExecutableFlag flag) : this(pointer, ctx) {
            this.Flag = flag;
        }

        public ExecutionResult (Member result, uint pointer, Compilation.RuntimeContext ctx) : base() {
            if (ctx.Pointers.ContainsKey(pointer))
                ctx.Pointers[pointer] = result;
            else
                ctx.Pointers.Add(pointer, result);

            this.Result = result;
            this.Pointer = pointer;
        }

        public ExecutionResult(Member result, uint pointer, Compilation.RuntimeContext ctx, ExecutableFlag flag) :
            this(result, pointer, ctx) {
            this.Flag = flag;
        }

        public ExecutionResult(Member result, Compilation.RuntimeContext ctx) : base() {
            if(result is ClrMember clr) {
                bool found = false;
                foreach (var item in ctx.Pointers) {
                    if(item.Value is ClrMember compare) {
                        if(compare.GetNative() == clr.GetNative()) {
                            if (compare.GetNative() is Simula.Scripting.Type.Boolean) {
                                this.Pointer = item.Key;
                                this.Result = result;
                                found = true;
                                break;
                            }
                        }
                    }
                }

                if (found) return;
                else {
                    ctx.Pointers.Add(ctx.MaximumAllocatedPointer, result);
                    ctx.MaximumAllocatedPointer++;
                    this.Pointer = ctx.MaximumAllocatedPointer - 1;
                    this.Result = result;
                }
            }

            if(ctx.Pointers.ContainsValue(result)) {
                foreach (var item in ctx.Pointers) {
                    if(item.Value == result) {
                        this.Pointer = item.Key;
                        this.Result = result;
                    }
                }
            } else {
                ctx.Pointers.Add(ctx.MaximumAllocatedPointer, result);
                ctx.MaximumAllocatedPointer++;
                this.Pointer = ctx.MaximumAllocatedPointer - 1;
                this.Result = result;
            }
        }

        public Member Result { get; set; } = NullType.Null;
        public uint Pointer { get; set; } = 0;
        public ExecutableFlag Flag { get; set; } = ExecutableFlag.Pass;

        public bool IsNull() {
            return this.Pointer == 0;
        }
    }
}
