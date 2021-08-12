using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using CaseExtensions;

using BlockStructure.Schemas;

namespace BlockStructure.Emit
{
    public class SchemaDocumentBuilder
    {
        public SchemaDocument Document { get; set; }
        public Dictionary<string, Type> TypeOverride { get; set; }

        public SchemaTypeBuilder SchemaTypes { get; set; }
        public SchemaFieldBuilder SchemaFields { get; set; }
        public SchemaInheritanceBuilder SchemaInheritance { get; set; }
        public SchemaEnumBuilder SchemaEnum { get; set; }

        public SchemaDocumentBuilder(ModuleBuilder moduleBuilder,
                                     SchemaDocument document,
                                     BasicsDescription basics,
                                     VersionKey version)
        {
            SchemaTypes = new SchemaTypeBuilder(document, moduleBuilder, basics);
            SchemaFields = new SchemaFieldBuilder(document, SchemaTypes, version);
            SchemaInheritance = new SchemaInheritanceBuilder(SchemaTypes);
            SchemaEnum = new SchemaEnumBuilder(SchemaTypes);
        }

        public void CreateTypes()
        {
            foreach (var enumBuilder in SchemaTypes.BuiltEnumsByName.Values)
                enumBuilder.CreateTypeInfo();
            foreach (var typeBuilder in SchemaTypes.BuiltTypesByName.Values)
                typeBuilder.CreateTypeInfo();
        }

        public static AssemblyBuilder CreateNewAssembly(SchemaDocument document,
                                                            BasicsDescription basics,
                                                            VersionKey version)
        {
            var assemblyName = new AssemblyName("DynamicAssemblyExample");
            var asemblyBuilder =
                AssemblyBuilder.DefineDynamicAssembly(
                    assemblyName,
                    AssemblyBuilderAccess.Run);

            var moduleBuilder =
                asemblyBuilder.DefineDynamicModule(assemblyName.Name + ".dll");

            var schemaDocument = new SchemaDocumentBuilder(moduleBuilder,
                                             document,
                                             basics,
                                             version);
            schemaDocument.CreateTypes();
            return asemblyBuilder;
        }
    }
}
