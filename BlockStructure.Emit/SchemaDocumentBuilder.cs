using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using CaseExtensions;

using BlockStructure.Schemas;
using BlockStructure.Emit.IL;

namespace BlockStructure.Emit
{
    public class SchemaDocumentBuilder
    {
        public SchemaDocument Document { get; set; }
        public Dictionary<string, Type> TypeOverride { get; set; }

        public SchemaBasicReaderBuilder SchemaBasicReader { get; set; }
        public SchemaTypeBuilder SchemaTypes { get; set; }
        public SchemaFieldBuilder SchemaFields { get; set; }
        public SchemaInheritanceBuilder SchemaInheritance { get; set; }
        public SchemaEnumBuilder SchemaEnum { get; set; }
        public SchemaConstructorBuilder SchemaConstructors { get; set; }
        public SchemaConstructorILBuilder SchemaILBuilder { get; }

        public IEnumerable<CompoundSchema> CompoundSchemas => Document.Compounds.Values;
        public IEnumerable<NiObjectSchema> NiObjectSchemas => Document.NiObjects.Values;

        /// <summary>
        /// Writes types for each niObject, compound, enum, bitflag and bitfield
        /// into a reflection module, as well as building constructors for each
        /// type.
        /// </summary>
        /// <param name="moduleBuilder">The module to write the types to</param>
        /// <param name="document">The base schema document that described the types in NIF.xml/BlockStructure format</param>
        /// <param name="basics">A map of basic type names to their primitive values</param>
        /// <param name="version">The specific version of the document to use</param>
        public SchemaDocumentBuilder(ModuleBuilder moduleBuilder,
                                     SchemaDocument document,
                                     Dictionary<string, Type> basics,
                                     VersionKey version)
        {
            Document = document;
            SchemaBasicReader = new SchemaBasicReaderBuilder(moduleBuilder, document, basics);
            
            SchemaTypes = new SchemaTypeBuilder(document, moduleBuilder, SchemaBasicReader);
            SchemaFields = new SchemaFieldBuilder(this, version);
            SchemaInheritance = new SchemaInheritanceBuilder(SchemaTypes);
            SchemaEnum = new SchemaEnumBuilder(SchemaTypes);
            SchemaConstructors = new SchemaConstructorBuilder(document, SchemaTypes, SchemaFields, SchemaBasicReader);
            SchemaILBuilder = new SchemaConstructorILBuilder(this);

            foreach (var enumBuilder in SchemaTypes.BuiltEnumsByName.Values)
                enumBuilder.CreateTypeInfo();
            foreach (var typeBuilder in SchemaTypes.BuiltTypesByName.Values)
                typeBuilder.CreateTypeInfo();
        }
    }
}
