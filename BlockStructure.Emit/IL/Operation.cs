using System;
using System.Reflection;
using System.Reflection.Emit;

namespace BlockStructure.Emit.IL
{

    /// <summary>
    /// Base type for any IL-performable operation.
    /// Each operation maps to zero or more IL-Emittable tasks
    /// </summary>
    public abstract record Operation()
    {
        public record Label();

        // Control flow
        public record BranchTrue(Label Destination) : Operation;
        public record BranchFalse(Label Destination) : Operation;
        public record MarkLabel(Label Marked) : Operation;

        // Function meta
        public record LoadArg(int ArgumentIndex) : Operation;
        public record Return() : Operation;

        // Fields
        public record SetField(FieldInfo Field) : Operation;
        public record LoadField(FieldInfo Field) : Operation;

        // New Objects
        public record NewArray(Type ArrayType) : Operation;
        public record NewObject(ConstructorInfo Constructor) : Operation;
        public record LoadConstant64(long Constant) : Operation;
        public record LoadConstant32(int Constant) : Operation;

        // Function calling
        public record CallVirt(MethodInfo Method) : Operation;
        public record Call(MethodInfo Method) : Operation;

        // Binary (two argument) operations
        public abstract record Binary : Operation
        {
            public abstract record Numerical : Binary
            {
                public record Add : Numerical;
                public record Subtract : Numerical;
                public record Multiply : Numerical;
                public record Divide : Numerical;
            }
            public abstract record Comparable : Binary
            {
                public record Equal : Comparable;
                public record GreaterThan : Comparable;
                public record LessThan : Comparable;
            }
            public abstract record Bitwise : Binary
            {
                public record And : Bitwise;
                public record Or : Bitwise;
                public record ShiftLeft : Bitwise;
                public record ShiftRight : Bitwise;
            }
        }
    }
}
