using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Logic;
using BlockStructure.Schemas;
using CaseExtensions;

namespace BlockStructure.Emit
{
    public class SchemaFieldBuilder
    {
        public IReadOnlyDictionary<string, List<FieldSchema>> IncludedFieldsByTypeName { get; }
        public IReadOnlyDictionary<FieldSchema, FieldBuilder> FieldBuilders { get; }
        public IReadOnlyDictionary<FieldSchema, Type> FieldTypes { get; }

        SchemaDocumentBuilder Doc;

        public SchemaFieldBuilder(SchemaDocumentBuilder documentBuilder,
                                  VersionKey version)
        {
            Doc = documentBuilder;

            var state = version.AsState();
            var interpreter = new Interpreter(state);

            var compoundFields = documentBuilder.Document
                .Compounds.Values
                .Select(c => (c.Name, c.Fields));
            var niObjectFields = documentBuilder.Document
                .NiObjects.Values
                .Select(ni => (ni.Name, ni.Fields));

            IncludedFieldsByTypeName = compoundFields.Concat(niObjectFields)
                .ToDictionary(
                    p => p.Name,
                    p => p.Fields
                        .Where(f => IsFieldIncluded(f, interpreter, version))
                        .ToList()
                );


            FieldTypes = IncludedFieldsByTypeName
                .SelectMany(kvp =>
                    kvp.Value.Select(field =>
                        (field, documentBuilder.SchemaTypes.GetFieldType(field))))
                .ToDictionary(ft => ft.field, ft => ft.Item2);
            FieldTypes = IncludedFieldsByTypeName
                .SelectMany(kvp =>
                    kvp.Value.Select(field =>
                        (field, documentBuilder.SchemaTypes.GetFieldType(field))))
                .ToDictionary(ft => ft.field, ft => ft.Item2);

            FieldBuilders = IncludedFieldsByTypeName
                .SelectMany(kvp => kvp.Value.Select(field => (field, BuildField(kvp.Key, field))))
                .ToDictionary(p => p.field, p => p.Item2);
        }

        public IEnumerable<(FieldSchema, FieldBuilder)> GetBuildersForType(string typeName)
        {
            return IncludedFieldsByTypeName[typeName]
                .Select(f => (f, FieldBuilders[f]));
        }

        bool IsFieldIncluded(FieldSchema field, Interpreter versionInterpreter, VersionKey version)
        {
            if (!Doc.Document.ExpressionLookup.CheckVersionCondition(field, versionInterpreter))
                return false;
            if (!field.InVersionRange(version))
                return false;
            return true;
        }

        FieldBuilder BuildField(string typeName,
                                FieldSchema fieldSchema)
        {
            var typeBuilder = Doc.SchemaTypes.BuiltTypesByName[typeName];
            var name = fieldSchema.Name.ToPascalCase();
            var fieldType = FieldTypes[fieldSchema];
            var attr = FieldAttributes.Public;
            var fieldBuilder = typeBuilder.DefineField(name, fieldType, attr);

            return fieldBuilder;
        }
    }
}
