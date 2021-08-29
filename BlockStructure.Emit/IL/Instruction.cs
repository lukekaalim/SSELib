using System;
using System.Reflection;
using System.Reflection.Emit;

namespace BlockStructure.Emit.IL
{
    public abstract class Instruction
    {
        public abstract void Write(ILGenerator il);

        public class NoOperation : Instruction
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Nop);
        }

        public class Set : Instruction
        {
            public Instruction[] Instructions { get; set; }
            public Set(params Instruction[] instructions) =>
                Instructions = instructions;

            public override void Write(ILGenerator il) {
                for (int i = 0; i < Instructions.Length; i++)
                    Instructions[i].Write(il);
            }
        }

        public class LoadField : Instruction
        {
            public FieldInfo Field { get; set; }
            public LoadField(FieldInfo field) =>
                Field = field;

            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Ldfld, Field);
        }

        public class LoadArgument : Instruction
        {
            public int ArgumentIndex { get; set; }
            public LoadArgument(int index) =>
                ArgumentIndex = index;

            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Ldarg_S, ArgumentIndex);
        }

        public class LoadThis : Instruction
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Ldarg_0);
        }

        public class Add : Instruction
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Add);
        }

        public class IfBranch : Instruction
        {
            public Instruction LoadCondition { get; set; }
            public Instruction LoadSuccess { get; set; }

            public IfBranch(Instruction loadCondition, Instruction loadSuccess)
            {
                LoadCondition = loadCondition;
                LoadSuccess = loadSuccess;
            }

            public override void Write(ILGenerator il) {
                var endIfLabel = il.DefineLabel();
                LoadCondition.Write(il);
                il.Emit(OpCodes.Brfalse, endIfLabel);
                LoadSuccess.Write(il);
                il.MarkLabel(endIfLabel);
            }
        }

        public class LoadInt32 : Instruction
        {
            public int Value { get; }

            public LoadInt32(int value) =>
                Value = value;

            public override void Write(ILGenerator il)
            {
                il.Emit(OpCodes.Ldc_I4, Value);
            }
        }
        public class LoadInt64 : Instruction
        {
            public long Value { get; }

            public LoadInt64(long value) =>
                Value = value;

            public override void Write(ILGenerator il)
            {
                il.Emit(OpCodes.Ldc_I8, Value);
            }
        }

        public class VirtualCall : Instruction
        {
            public MethodInfo Method { get; set; }
            public VirtualCall(MethodInfo method) =>
                Method = method;

            public override void Write(ILGenerator il) =>
                il.EmitCall(OpCodes.Callvirt, Method, null);
        }

        public class Call : Instruction
        {
            public MethodInfo Method { get; set; }
            public ConstructorInfo Constructor { get; set; }

            public Call(MethodInfo method) =>
                Method = method;
            public Call(ConstructorInfo constructor) =>
                Constructor = constructor;

            public override void Write(ILGenerator il)
            {
                if (Method != null)
                    il.Emit(OpCodes.Call, Method);
                else if (Constructor != null)
                    il.Emit(OpCodes.Call, Constructor);

                throw new Exception();
            }
        }

        public class NewObject : Instruction
        {
            public ConstructorInfo Constructor { get; set; }
            public NewObject(ConstructorInfo constructor) =>
                Constructor = constructor;

            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Newobj, Constructor);
        }

        public class SetField : Instruction
        {
            public FieldInfo Field { get; set; }
            public SetField(FieldInfo field) =>
                Field = field;

            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Stfld, Field);
        }

        public class Return : Instruction
        {
            public override void Write(ILGenerator il) =>
                il.Emit(OpCodes.Ret);
        }

        public class InstructionWriter : Instruction
        {
            public Action<ILGenerator> WriteInstruction { get; }
            public InstructionWriter(Action<ILGenerator> writeInstruction) =>
                WriteInstruction = writeInstruction;

            public override void Write(ILGenerator il) =>
                WriteInstruction(il);
        }
    }
}
