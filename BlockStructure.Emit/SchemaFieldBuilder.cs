using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Schemas;
using CaseExtensions;

namespace BlockStructure.Emit
{
    public class SchemaFieldBuilder
    {

        public Dictionary<CompoundSchema, Dictionary<FieldSchema, FieldBuilder>> CompoundFields { get; set; }
        public Dictionary<NiObjectSchema, Dictionary<FieldSchema, FieldBuilder>> NiObjectFields { get; set; }

        SchemaTypeBuilder SchemaTypes { get; set; }

        public SchemaFieldBuilder(SchemaDocument document,
                                  SchemaTypeBuilder schemaTypes,
                                  VersionKey version)
        {
            SchemaTypes = schemaTypes;

            CompoundFields = schemaTypes
                .CompoundTypes
                .Select(kvp =>
                    (kvp.Key, kvp.Key.Fields
                        .Where(field => document.ExpressionLookup.CheckVersionCondition(field, version))
                        .Where(field => field.InVersionRange(version))
                        .ToDictionary(field => field, field =>
                            BuildField(kvp.Value.Item1, kvp.Value.Item2, field))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Item2);

            NiObjectFields = schemaTypes
                .NiObjectTypes
                .Select(kvp =>
                    (kvp.Key, kvp.Key.Fields
                        .Where(field => document.ExpressionLookup.CheckVersionCondition(field, version))
                        .Where(field => field.InVersionRange(version))
                        .ToDictionary(field => field, field =>
                            BuildField(kvp.Value, null, field))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Item2);
        }

        FieldBuilder BuildField(TypeBuilder typeBuilder,
                                GenericTypeParameterBuilder compoundTemplateType,
                                FieldSchema field)
        {
            var name = field.Name.ToPascalCase();
            var fieldType = GetFieldType(field, compoundTemplateType);
            var attr = FieldAttributes.Public;
            var fieldBuilder = typeBuilder.DefineField(name, fieldType, attr);

            return fieldBuilder;
        }

        Type GetBaseType(string typeName,
                         GenericTypeParameterBuilder templateType = null)
        {
            var typeIsTemplate = typeName == "#T#";
            return typeIsTemplate ? templateType : SchemaTypes.SchemaTypesByName[typeName];
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

        Type GetFieldType(FieldSchema field, GenericTypeParameterBuilder templateType)
        {
            return GetDimensionalType(field.Type, field.Template, field.Dimensions.Count, templateType);
        }
    }
}
