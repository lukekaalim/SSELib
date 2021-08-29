using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Schemas;
using CaseExtensions;

namespace BlockStructure.Emit
{
    /// <summary>
    /// Description of basics in a document, and how they map to underlying
    /// primitive types
    /// </summary>
    public class SchemaBasicReaderBuilder
    {
        public class BasicTypeDescription
        {
            public Type UnderlyingType { get; set; }
            public BasicSchema Schema { get; set; }

            public BasicTypeDescription(Type underlyingType, BasicSchema schema)
            {
                UnderlyingType = underlyingType;
                Schema = schema;
            }
        }

        public Dictionary<BasicSchema, BasicTypeDescription> BasicDescriptions { get; set; }
        public Type IBasicReaderType { get; set; }
        public Dictionary<BasicSchema, MethodBuilder> IBasicReaderMethods { get; set; }

        public SchemaBasicReaderBuilder(ModuleBuilder module, SchemaDocument document, Dictionary<string, Type> basicTypeMap)
        {
            BasicDescriptions = basicTypeMap
                .Select(kvp => new BasicTypeDescription(kvp.Value, document.Basics[kvp.Key]))
                .ToDictionary(desc => desc.Schema);

            var builder =  CreateBasicReaderInterface(module, BasicDescriptions);
            
            IBasicReaderMethods = BasicDescriptions
                .ToDictionary(desc => desc.Key,
                              desc => WriteBasicReaderMethod(builder, desc.Value));
            
            IBasicReaderType = builder.CreateType();
        }

        public TypeBuilder CreateBasicReaderInterface(ModuleBuilder module,
                                                      Dictionary<BasicSchema, BasicTypeDescription> descriptions)
        {
            var attr = TypeAttributes.Public
                | TypeAttributes.Abstract
                | TypeAttributes.Interface;
            var type = module.DefineType("IBasicReader", attr);
            return type;
        }

        public MethodBuilder WriteBasicReaderMethod(TypeBuilder readerType, BasicTypeDescription description)
        {
            var methodName = $"Read{description.Schema.Name.ToPascalCase()}";
            var returnType = description.UnderlyingType;
            var args = new Type[0];
            var convention = CallingConventions.HasThis;
            var attr = MethodAttributes.Public
                | MethodAttributes.Virtual
                | MethodAttributes.Abstract;
            var meth = readerType.DefineMethod(methodName,
                                               attr,
                                               convention,
                                               returnType,
                                               args);
            return meth;
        }
    }
}
