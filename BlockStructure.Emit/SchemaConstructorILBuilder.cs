using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Schemas;
using BlockStructure.Emit.IL;

namespace BlockStructure.Emit
{
    public class SchemaConstructorILBuilder
    {
        SchemaDocumentBuilder DocBuilder;

        public SchemaConstructorILBuilder(SchemaDocumentBuilder docBuilder)
        {
            DocBuilder = docBuilder;

            foreach (var (schema, constructor) in docBuilder.SchemaConstructors.CompoundConstructors)
                ConstructCompound(schema).Write(constructor.GetILGenerator());
        }

        public Instruction ConstructCompound(CompoundSchema schema)
        {
            var baseType = DocBuilder.SchemaTypes.SchemaTypesByName[schema.Name];
            var templateType = DocBuilder.SchemaTypes.TemplateTypeByName[schema.Name];

            var fields = DocBuilder.SchemaFields.GetBuildersForType(schema.Name);
            var state = new ExpressionWriter.State(fields, new string[] { "Argument" });
            var instructions = fields
                .Select(sb => ConstructField(sb.Item1, sb.Item2, state))
                .ToArray();
            return new Instruction.Set(
                new Instruction.Set(instructions),
                new Instruction.Return()
            );
        }

        public Instruction ConstructField(FieldSchema schema, FieldBuilder builder, ExpressionWriter.State state)
        {
            if (schema.Dimensions.Count > 0)
                return new Instruction.NoOperation();

            var setFieldInstruction = new Instruction.Set(
                new Instruction.LoadThis(),
                LoadFieldValue(schema),
                new Instruction.SetField(builder)
            );

            return WithCondition(setFieldInstruction, schema, state);
        }

        Instruction WithArray(Instruction loadValue, FieldSchema fieldSchema, FieldBuilder builder)
        {
            if (!fieldSchema.IsMultiDimensional)
            {
                return new Instruction.Set(
                    new Instruction.LoadThis(),
                    loadValue,
                    new Instruction.SetField(builder)
                );
            }

            return new Instruction.Set(
                new Instruction.LoadThis(),
                //ConstructArray(),
                new Instruction.SetField(builder)
            );
        }

        public Instruction ConstructArray(Instruction loadLength, FieldSchema fieldSchema)
        {
            var fieldType = DocBuilder.SchemaTypes.GetArrayType(fieldSchema);
            return new Instruction.InstructionWriter(il =>
            {
                il.Emit(OpCodes.Ldarg_0);
                loadLength.Write(il);
                il.Emit(OpCodes.Conv_Ovf_I);
                il.Emit(OpCodes.Newobj, fieldType);
            });
        }

        public Instruction LoadFieldValue(FieldSchema field)
        {
            if (DocBuilder.Document.Compounds.TryGetValue(field.Type, out var compoundSchema))
                return ReadCompoundValue(field, compoundSchema);
            if (DocBuilder.Document.Basics.TryGetValue(field.Type, out var basicSchema))
                return ReadBasicValue(basicSchema);

            return new Instruction.LoadInt32(0);
        }

        public Instruction ReadBasicValue(BasicSchema schema)
        {
            if (!DocBuilder.SchemaBasicReader.IBasicReaderMethods.TryGetValue(schema, out var method))
                throw new Exception();

            return new Instruction.Set(
                new Instruction.LoadArgument(1),
                new Instruction.VirtualCall(method)
            );
        }

        public Instruction ReadCompoundValue(FieldSchema field, CompoundSchema compound)
        {
            var fieldType = DocBuilder.SchemaFields.FieldTypes[field];
            var constructor = DocBuilder.SchemaConstructors.CompoundConstructors[compound] as ConstructorInfo;

            if (field.Template != null)
                constructor = TypeBuilder.GetConstructor(fieldType, constructor);

            if (constructor == null)
                throw new Exception();

            return new Instruction.Set(
                new Instruction.LoadArgument(1),
                new Instruction.LoadInt64(0),
                new Instruction.NewObject(constructor)
            );
        }

        Instruction LoadDimensions(FieldSchema field)
        {
            return null;
        }

        public Instruction LoadCondition(FieldSchema schema, ExpressionWriter.State state)
        {
            if (!DocBuilder.Document.ExpressionLookup.Conditions.TryGetValue(schema, out var expression))
                return new Instruction.LoadInt32(1);

            return new Instruction.InstructionWriter(il => new ExpressionWriter(il, state).WriteExpression(expression)) ;
        }

        Instruction WithCondition(Instruction onSuccess, FieldSchema schema, ExpressionWriter.State state)
        {
            if (schema.Condition == null)
                return onSuccess;

            var conditionalInstruction = new Instruction.IfBranch(
                LoadCondition(schema, state),
                onSuccess
            );
            return conditionalInstruction;
        }
    }
}
