using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using BlockStructure.Schemas;
using CaseExtensions;

namespace BlockStructure.Emit
{
    public class SchemaTypeBuilder
    {
        public Dictionary<string, Type> SchemaTypesByName { get; set; }
        public Dictionary<string, TypeBuilder> BuiltTypesByName { get; set; }
        public Dictionary<string, EnumBuilder> BuiltEnumsByName { get; set; }

        public BasicsDescription BasicTypeDescriptions { get; set; }

        public Dictionary<CompoundSchema, (TypeBuilder, GenericTypeParameterBuilder)> CompoundTypes { get; set; }
        public Dictionary<NiObjectSchema, TypeBuilder> NiObjectTypes { get; set; }

        public Dictionary<EnumSchema, EnumBuilder> EnumTypes { get; set; }
        public Dictionary<BitFieldSchema, TypeBuilder> BitFieldTypes { get; set; }
        public Dictionary<BitflagsSchema, EnumBuilder> BitflagsTypes { get; set; }

        public SchemaTypeBuilder(SchemaDocument document,
                                 ModuleBuilder moduleBuilder,
                                 BasicsDescription basicBuilder)
        {
            BasicTypeDescriptions = basicBuilder;

            CompoundTypes = document.Compounds
                .ToDictionary(kvp => kvp.Value, kvp => BuildCompoundType(moduleBuilder, kvp.Value));
            NiObjectTypes = document.NiObjects
                .ToDictionary(kvp => kvp.Value, kvp => BuildNiObjectType(moduleBuilder, kvp.Value));
            EnumTypes = document.Enums
                .ToDictionary(kvp => kvp.Value, kvp => BuildEnumType(moduleBuilder, kvp.Value, document));
            BitFieldTypes = document.Bitfields
                .ToDictionary(kvp => kvp.Value, kvp => BuildBitfieldType(moduleBuilder, kvp.Value));
            BitflagsTypes = document.Bitflags
                .ToDictionary(kvp => kvp.Value, kvp => BuildBitflagType(moduleBuilder, kvp.Value, document));

            BuiltTypesByName = new List<(string, TypeBuilder)>()
                .Concat(CompoundTypes.Select(kvp => (kvp.Key.Name, kvp.Value.Item1)))
                .Concat(NiObjectTypes.Select(kvp => (kvp.Key.Name, kvp.Value)))
                .Concat(BitFieldTypes.Select(kvp => (kvp.Key.Name, kvp.Value)))
                .ToDictionary(pair => pair.Item1, pair => pair.Item2);
            BuiltEnumsByName = new List<(string, EnumBuilder)>()
                .Concat(EnumTypes.Select(kvp => (kvp.Key.Name, kvp.Value)))
                .Concat(BitflagsTypes.Select(kvp => (kvp.Key.Name, kvp.Value)))
                .ToDictionary(pair => pair.Item1, pair => pair.Item2);

            SchemaTypesByName = new List<(string, Type)>()
                .Concat(BasicTypeDescriptions.BasicDescriptions.Select(kvp => (kvp.Key.Name, kvp.Value.UnderlyingType)))
                .Concat(BuiltTypesByName.Select(kvp => (kvp.Key, kvp.Value as Type)))
                .Concat(BuiltEnumsByName.Select(kvp => (kvp.Key, kvp.Value as Type)))
                .ToDictionary(pair => pair.Item1, pair => pair.Item2);
        }
        (TypeBuilder, GenericTypeParameterBuilder) BuildCompoundType(ModuleBuilder moduleBuilder, CompoundSchema schema)
        {
            var name = schema.Name.ToPascalCase();
            var attr = TypeAttributes.Public;
            var compoundBuilder = moduleBuilder.DefineType(name, attr);

            if (schema.Generic)
            {
                string[] typeParamNames = { "T" };
                var typeParams = compoundBuilder.DefineGenericParameters(typeParamNames);
                return (compoundBuilder, typeParams[0]);
            }

            return (compoundBuilder, null);
        }
        TypeBuilder BuildNiObjectType(ModuleBuilder moduleBuilder, NiObjectSchema schema)
        {
            var name = schema.Name.ToPascalCase();
            var attr = TypeAttributes.Public;
            var niObjectBuilder = moduleBuilder.DefineType(name, attr);
            return niObjectBuilder;
        }
        EnumBuilder BuildEnumType(ModuleBuilder moduleBuilder, EnumSchema schema, SchemaDocument document)
        {
            var name = schema.Name.ToPascalCase();
            var attr = TypeAttributes.Public;
            var underlyingTypeDescription = BasicTypeDescriptions
                .BasicDescriptions[document.Basics[schema.Storage]];
            var underlingType = underlyingTypeDescription.UnderlyingType;
            var enumBuilder = moduleBuilder.DefineEnum(name, attr, underlingType);
            return enumBuilder;
        }
        TypeBuilder BuildBitfieldType(ModuleBuilder moduleBuilder, BitFieldSchema schema)
        {
            var name = schema.Name.ToPascalCase();
            var attr = TypeAttributes.Public;
            var bitfieldBuilder = moduleBuilder.DefineType(name, attr);
            return bitfieldBuilder;
        }
        EnumBuilder BuildBitflagType(ModuleBuilder moduleBuilder, BitflagsSchema schema, SchemaDocument document)
        {
            var name = schema.Name.ToPascalCase();
            var attr = TypeAttributes.Public;
            var underlyingTypeDescription = BasicTypeDescriptions
                .BasicDescriptions[document.Basics[schema.Storage]];
            /*
            Type flagsAttributeType = typeof(FlagsAttribute);
            ConstructorInfo attributeConstructor = flagsAttributeType.GetConstructor(new Type[] { });
            */
            var underlingType = underlyingTypeDescription.UnderlyingType;
            var bitflagBuilder = moduleBuilder.DefineEnum(name, attr, underlingType);
            //bitflagBuilder.SetCustomAttribute(attributeConstructor, new byte[] { 01, 00, 01 });
            return bitflagBuilder;
        }
    }
}
