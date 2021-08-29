using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure.Emit
{
    public class SchemaConstructorBuilder
    {
        public Dictionary<CompoundSchema, ConstructorBuilder> CompoundConstructors { get; }
        public Dictionary<NiObjectSchema, ConstructorBuilder> NiObjectConstructors { get; }

        SchemaDocument Doc;
        SchemaTypeBuilder Types;
        SchemaFieldBuilder Fields;
        SchemaBasicReaderBuilder Basic;

        public SchemaConstructorBuilder(SchemaDocument doc,
                                        SchemaTypeBuilder types,
                                        SchemaFieldBuilder fields,
                                        SchemaBasicReaderBuilder basic)
        {
            Doc = doc;
            Types = types;
            Fields = fields;
            Basic = basic;

            CompoundConstructors = Types.CompoundTypes
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => WriteCompoundConstructor(kvp.Value)
                );
            //NiObjectConstructors = Types.NiObjectTypes
            //    .ToDictionary(
            //        kvp => kvp.Key,
            //        kvp => kvp.Value.DefineDefaultConstructor(MethodAttributes.Public)
            //    );
        }

        ConstructorBuilder WriteCompoundConstructor(TypeBuilder type)
        {
            var attr = MethodAttributes.Public;
            var call = CallingConventions.Standard;
            var args = new[] { Basic.IBasicReaderType, typeof(long) };

            var constructor = type.DefineConstructor(attr, call, args);
            constructor.DefineParameter(1, ParameterAttributes.None, "basicReader");
            constructor.DefineParameter(2, ParameterAttributes.None, "argument");

            return constructor;
        }
    }
}
