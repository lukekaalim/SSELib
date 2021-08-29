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
        SchemaDocument Document;

        public IReadOnlyDictionary<string, Type> SchemaTypesByName { get; }
        public IReadOnlyDictionary<string, GenericTypeParameterBuilder> TemplateTypeByName { get; }

        public Dictionary<string, TypeBuilder> BuiltTypesByName { get; set; }
        public Dictionary<string, EnumBuilder> BuiltEnumsByName { get; set; }

        public SchemaBasicReaderBuilder BasicTypeDescriptions { get; set; }

        public Dictionary<CompoundSchema, TypeBuilder> CompoundTypes { get; set; }
        public Dictionary<NiObjectSchema, TypeBuilder> NiObjectTypes { get; set; }

        public Dictionary<EnumSchema, EnumBuilder> EnumTypes { get; set; }
        public Dictionary<BitFieldSchema, TypeBuilder> BitFieldTypes { get; set; }
        public Dictionary<BitflagsSchema, EnumBuilder> BitflagsTypes { get; set; }

        public SchemaTypeBuilder(SchemaDocument document,
                                 ModuleBuilder moduleBuilder,
                                 SchemaBasicReaderBuilder basicBuilder)
        {
            Document = document;
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
                .Concat(CompoundTypes.Select(kvp => (kvp.Key.Name, kvp.Value)))
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
            TemplateTypeByName = CompoundTypes
                .ToDictionary(c => c.Key.Name, c => BuildTemplateType(c.Value, c.Key));
        }

        /// <summary>
        /// Get the type associated with a field, with the 
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parentName">The name of the compound or niObject that this object belongs to</param>
        /// <returns></returns>
        public Type GetFieldType(FieldSchema field)
        {
            var parentName = Document.TypeLookup.ParentNameBySchema[field];
            var templateType = TemplateTypeByName.GetValueOrDefault(parentName);
            return GetDimensionalType(field.Type, field.Template, field.Dimensions.Count, templateType);
        }
        /// <summary>
        /// Get the type associated with the template argument of a field
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Type GetFieldTemplateType(FieldSchema field)
        {
            if (field.Template == null)
                return null;
            var parentName = Document.TypeLookup.ParentNameBySchema[field];
            var templateType = TemplateTypeByName.GetValueOrDefault(parentName);
            return GetBaseType(field.Template, templateType);
        }
        /// <summary>
        /// Gets the type that the field is an array of (assuming the field is for an array)
        /// </summary>
        /// <param name="field"></param>
        /// <param name="parentName"></param>
        /// <returns></returns>
        public Type GetArrayType(FieldSchema field)
        {
            var parentName = Document.TypeLookup.ParentNameBySchema[field];
            var templateType = TemplateTypeByName.GetValueOrDefault(parentName);
            return GetTemplatedType(field.Type, field.Template, templateType);
        }

        Type GetBaseType(string typeName,
                         GenericTypeParameterBuilder templateType = null)
        {
            var typeIsTemplate = typeName == "#T#";
            return typeIsTemplate ? templateType : SchemaTypesByName[typeName];
        }
        Type GetTemplatedType(string typeName,
                              string templateName = null,
                              GenericTypeParameterBuilder templateType = null)
        {
            var baseType = GetBaseType(typeName, templateType);

            if (!baseType.IsGenericTypeDefinition)
                return baseType;

            var genericArgument = GetBaseType(templateName, templateType);
            return baseType.MakeGenericType(genericArgument);
        }
        Type GetDimensionalType(string typeName,
                                string templateName = null,
                                int dimensions = 0,
                                GenericTypeParameterBuilder templateType = null)
        {
            var baseType = GetTemplatedType(typeName, templateName, templateType);

            if (dimensions == 0)
                return baseType;

            return baseType.MakeArrayType(dimensions);
        }

        TypeBuilder BuildCompoundType(ModuleBuilder moduleBuilder, CompoundSchema schema)
        {
            var name = schema.Name.ToPascalCase();
            var attr = TypeAttributes.Public;
            var compoundBuilder = moduleBuilder.DefineType(name, attr);
            return compoundBuilder;
        }
        GenericTypeParameterBuilder BuildTemplateType(TypeBuilder compoundBuilder, CompoundSchema schema)
        {
            if (!schema.Generic)
                return null;

            string[] typeParamNames = { "T" };
            var typeParams = compoundBuilder.DefineGenericParameters(typeParamNames);
            return typeParams[0];
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
            
            Type flagsAttributeType = typeof(FlagsAttribute);
            ConstructorInfo attributeConstructor = flagsAttributeType.GetConstructor(new Type[] { });
            
            var underlingType = underlyingTypeDescription.UnderlyingType;
            var bitflagBuilder = moduleBuilder.DefineEnum(name, attr, underlingType);

            //bitflagBuilder.SetCustomAttribute(attributeConstructor, new byte[] { 01, 00, 01 });
            return bitflagBuilder;
        }
    }
}
