using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using NUnit.Framework;

using BlockStructure.Emit.IL;
using BlockStructure.Logic;

namespace TESVTesting
{
    public class ILGeneration
    {
        [Test]
        public void TestExpression()
        {
            var name = new AssemblyName("TestAssembly");
            var assembly = AssemblyBuilder.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);

            var module = assembly.DefineDynamicModule("TestAssembly.dll");

            var myClass = module.DefineType("MyClass");
            var myField = myClass.DefineField("MyField", typeof(long), FieldAttributes.Public);
            var myMethod = myClass.DefineMethod("MyMethod", MethodAttributes.Public, CallingConventions.HasThis, typeof(long), new Type[0]);

            var expression = new Expression.Operation.Binary()
            {
                LeftOperand = new Expression.Parameter("Input"),
                RightOperand = new Expression.Literal(10000),
                Operator = new Syntax.Operator.Numerical.Addition()
            };

            var state = new ExpressionWriter.State()
            {
                FieldByName = new Dictionary<string, FieldInfo>() { ["Input"] = myField },
                ArgumentsByName = new Dictionary<string, int>()
            };


            var generator = myMethod.GetILGenerator();
            var writer = new ExpressionWriter(generator, state);
            writer.WriteExpression(expression);
            generator.Emit(OpCodes.Ret);

            var myClassInfo = myClass.CreateType();
            var myFieldInfo = myClass.GetField("MyField");
            var myMethodInfo = myClassInfo.GetMethods()[0];

            var myClassInstance = Activator.CreateInstance(myClass);
            myFieldInfo.SetValue(myClassInstance, 3);
            var output = myMethodInfo.Invoke(myClassInstance, new object[0]);

            Assert.AreEqual(output, 10003);
        }
    }
}
