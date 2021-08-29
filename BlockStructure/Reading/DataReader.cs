using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Logic;
using BlockStructure.Schemas;

namespace BlockStructure.Reading
{
    public class DataReader
    {
        public delegate BasicData ReadBasicData(BasicSchema schema,
                                                ReadingContext context);
        public delegate bool TryReadCompoundData(CompoundSchema schema,
                                                 ReadingContext context,
                                                 out Data data);

        public readonly struct ReadingContext
        {
            public readonly string Template;
            public readonly long Argument;
            public readonly NiObjectSchema Parent;
            public readonly VersionKey Version;
            public readonly Interpreter VersionInterpreter;

            public ReadingContext(string template, long argument, NiObjectSchema parent, VersionKey version, Interpreter versionInterpreter)
            {
                Template = template;
                Argument = argument;
                Parent = parent;
                Version = version;
                VersionInterpreter = versionInterpreter;
            }

            public ReadingContext(VersionKey version)
            {
                Template = null;
                Argument = 0;
                Parent = null;
                Version = version;
                VersionInterpreter = new Interpreter(version.AsState());
            }

            public string ReadType(string typeString)
            {
                if (typeString == "#T#")
                    return Template;
                return typeString;
            }

            public ReadingContext Extend(FieldSchema field, long argument = 0)
            {
                return new ReadingContext(ReadType(field.Template), argument, Parent, Version, VersionInterpreter);
            }
            public ReadingContext Extend(NiObjectSchema parent)
            {
                return new ReadingContext(Template, Argument, parent, Version, VersionInterpreter);
            }
        }

        SchemaDocument Document;
        ReadBasicData ReadBasicFunc;
        TryReadCompoundData ReadExtraCompoundFunc;

        InheritanceLookup Inheritance;
        ExpressionLookup Expressions;

        public DataReader(SchemaDocument document,
                          ReadBasicData readBasicFunc,
                          TryReadCompoundData readCompoundFunc = null)
        {
            Document = document;
            ReadBasicFunc = readBasicFunc;
            ReadExtraCompoundFunc = readCompoundFunc;

            Inheritance = document.InheritanceLookup;
            Expressions = document.ExpressionLookup;
        }

        Data ReadCompound(CompoundSchema compound, ReadingContext parentContext)
        {
            if (ReadExtraCompoundFunc != null
                && ReadExtraCompoundFunc(compound, parentContext, out var data))
                return data;

            var state = Document.ParameterLookup.BuildCompoundState(compound, parentContext.Argument);
            var interpreter = new Interpreter(state);
            var compoundData = new Dictionary<string, Data>();
            foreach (var fieldSchema in compound.Fields)
            {
                var fieldData = ReadField(fieldSchema, interpreter, parentContext);
                if (fieldData == null)
                    continue;
                //state.Set(fieldSchema, Oldvalue.From(fieldData));
                compoundData[fieldSchema.Name] = fieldData;
            }

            return new CompoundData()
            {
                Fields = compoundData,
                Schema = compound,
            };
        }

        NiObjectData ReadNiObject(NiObjectSchema niObject,
                                ReadingContext parentContext)
        {
            var fields = Inheritance.GetFields(niObject);
            var state = Document.ParameterLookup.BuildNiObjectState(niObject);
            var interpreter = new Interpreter(state);
            var childContext = parentContext.Extend(niObject);
            var niObjectData = new Dictionary<string, Data>();
            foreach (var fieldSchema in fields)
            {
                var fieldData = ReadField(fieldSchema, interpreter, childContext);
                if (fieldData == null)
                    continue;
                //state.Set(fieldSchema, Oldvalue.From(fieldData));
                niObjectData[fieldSchema.Name] = fieldData;
            }

            return new NiObjectData()
            {
                Name = niObject.Name,
                Fields = niObjectData,
                Schema = niObject,
            };
        }

        BasicData ReadEnum(EnumSchema enumSchema, ReadingContext context)
        {
            if (Document.Basics.TryGetValue(enumSchema.Storage, out var basicSchema))
                return ReadBasicFunc(basicSchema, context);
            throw new Exception();
        }
        BasicData ReadBitField(BitFieldSchema bitFieldSchema, ReadingContext context)
        {
            if (Document.Basics.TryGetValue(bitFieldSchema.Storage, out var basicSchema))
                return ReadBasicFunc(basicSchema, context);
            throw new Exception();
        }
        BasicData ReadBitFlags(BitflagsSchema bitflagsSchema, ReadingContext context)
        {
            if (Document.Basics.TryGetValue(bitflagsSchema.Storage, out var basicSchema))
                return ReadBasicFunc(basicSchema, context);
            throw new Exception();
        }

        Data ReadField(FieldSchema fieldSchema, Interpreter interpreter, ReadingContext parentContext)
        {
            var type = parentContext.ReadType(fieldSchema.Type);
            var argument = Expressions.GetArgument(fieldSchema, interpreter);
            var childContext = parentContext.Extend(fieldSchema, argument);

            if (!parentContext.Version.MatchesVersionConstraint(fieldSchema.MinVersion, fieldSchema.MaxVersion))
                return null;
            if (!Expressions.CheckCondition(fieldSchema, interpreter))
                return null;
            if (!Expressions.CheckVersionCondition(fieldSchema, parentContext.VersionInterpreter))
                return null;
            if (!Inheritance.CheckIncludedInType(fieldSchema, parentContext.Parent))
                return null;

            if (fieldSchema.IsMultiDimensional)
            {
                var count = Expressions.GetCount(fieldSchema, interpreter);
                var elements = new List<Data>(count);
                for (int i = 0; i < count; i++)
                    elements.Add(ReadData(type, childContext));
                return new ListData(elements);
            }
            return ReadData(type, childContext);
        }

        public Data ReadData(string type)
        {
            return ReadData(type, new ReadingContext());
        }

        public Data ReadData(string type, VersionKey version)
        {
            return ReadData(type, new ReadingContext(version));
        }

        public Data ReadData(string type, ReadingContext context)
        {
            if (Document.Basics.TryGetValue(type, out var basicSchema))
                return ReadBasicFunc(basicSchema, context);

            if (Document.Enums.TryGetValue(type, out var enumSchema))
                return ReadEnum(enumSchema, context);
            if (Document.Bitfields.TryGetValue(type, out var bitFieldsSchema))
                return ReadBitField(bitFieldsSchema, context);
            if (Document.Bitflags.TryGetValue(type, out var bitflagsSchema))
                return ReadBitFlags(bitflagsSchema, context);

            if (Document.Compounds.TryGetValue(type, out var compoundSchema))
                return ReadCompound(compoundSchema, context);
            if (Document.NiObjects.TryGetValue(type, out var niObjectSchema))
                return ReadNiObject(niObjectSchema, context);

            throw new Exception();
        }
    }
}
