using System;
using System.Reflection.Emit;

namespace BlockStructure.Emit.IL
{
    public class OperationWriter
    {
        public void Write(Operation op, ILGenerator il)
        {
            switch (op)
            {
                case Operation.Call call:
                    il.Emit(OpCodes.Call, call.Method); return;
                case Operation.CallVirt call:
                    il.Emit(OpCodes.Callvirt, call.Method); return;
            }
        }
    }
}
