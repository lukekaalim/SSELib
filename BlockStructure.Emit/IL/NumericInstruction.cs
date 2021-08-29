using System;
using System.Reflection.Emit;

namespace BlockStructure.Emit.IL
{
    public abstract class Numeric : Instruction
    {
        public class Add : Numeric
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Add);
        }
        public class Multiply : Numeric
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Mul);
        }
        public class Subtract : Numeric
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Sub);
        }
        public class Divide : Numeric
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Div);
        }
    }
}
