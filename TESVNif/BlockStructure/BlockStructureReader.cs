using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using SSE.TESVNif.BlockStructure.Schemas;

namespace SSE.TESVNif.BlockStructure
{
    public class BlockStructureReader
    {
        public class CompoundContext
        {
            public string Owner { get; set; }
            public string Argument { get; set; }
            public string Template { get; set; }
        }

        public int? Version { get; set; }
        public BinaryReader Reader { get; set; }
        public SchemaDocument Document { get; set; }

        public Dictionary<string, Logic.Value> GlobalIdentifiers { get; set; }
        public Dictionary<(string, string), string> TokenSubsitutions { get; set; }

        public BlockStructureReader(XDocument xmlDoc, Stream reader)
        {
            Document = new SchemaDocument(xmlDoc.Root);
            Reader = new BinaryReader(reader);
            Version = null;
            GlobalIdentifiers = new Dictionary<string, Logic.Value>();
            TokenSubsitutions = new Dictionary<(string, string), string>();

            foreach (var token in Document.Tokens.Reverse<TokenSchema>())
                foreach (var attribute in token.Attributes)
                    foreach (var entry in token.Entries)
                        TokenSubsitutions.Add(
                            (attribute, entry.Identifier),
                            ResolveSubsitutions(entry.Content, attribute)
                        );
        }

        public Block ReadSchemaByName(string schemaName, CompoundContext context = null)
        {
            if (Document.Basics.TryGetValue(schemaName, out var basicSchema))
                return ReadBasic(basicSchema);
            if (Document.Compounds.TryGetValue(schemaName, out var compoundSchema))
                return ReadCompound(compoundSchema, context);
            if (Document.Enums.TryGetValue(schemaName, out var enumSchema))
                return ReadEnum(enumSchema);
            if (Document.NiObjects.TryGetValue(schemaName, out var niObjectSchema))
                return ReadNiObject(niObjectSchema);
            if (Document.Bitfields.TryGetValue(schemaName, out var bitFieldSchema))
                return ReadBitField(bitFieldSchema);
            if (Document.Bitflags.TryGetValue(schemaName, out var bitflagsSchema))
                return ReadBitflags(bitflagsSchema);

            throw new NotImplementedException();
        }

        public BasicBlock ReadBasic(BasicSchema schema)
        {
            return new BasicBlock(BasicReader.Read(Reader, schema));
        }

        public CompoundBlock ReadCompound(CompoundSchema schema, CompoundContext context = null)
        {
            var fields = new Dictionary<string, Block>();
            foreach (var field in schema.Fields)
            {
                var result = ReadField(field, fields, context);
                if (result != null)
                    fields.Add(field.Name, result);
            }

            return new CompoundBlock()
            {
                Name = schema.Name,
                Fields = fields,
            };
        }

        public IEnumerable<FieldSchema> GetNiObjectFields(NiObjectSchema schema)
        {
            var inheritedFields = schema.Inherits == null ?
                new List<FieldSchema>() :
                GetNiObjectFields(Document.NiObjects[schema.Inherits]);

            return inheritedFields.Concat(schema.OwnFields);
        }

        public NiObjectBlock ReadNiObject(NiObjectSchema schema)
        {
            var fields = new Dictionary<string, Block>();
            var fieldSchemas = GetNiObjectFields(schema);
            var context = new CompoundContext()
            {
                Owner = schema.Name
            }; ;
            foreach (var field in fieldSchemas)
            {
                var result = ReadField(field, fields, context);
                if (result != null)
                    fields.Add(field.Name, result);
            }
            return new NiObjectBlock()
            {
                Name = schema.Name,
                Fields = fields,
            };
        }

        EnumBlock ReadEnum(EnumSchema schema)
        {
            var storageResult = ReadSchemaByName(schema.Storage) as BasicBlock;
            return new EnumBlock(Convert.ToInt64(storageResult.Value));
        }
        BasicBlock ReadBitField(BitFieldSchema schema)
        {
            // TODO... we just skip over bitfield for now
            var storageResult = ReadSchemaByName(schema.Storage) as BasicBlock;
            return storageResult;
        }
        BasicBlock ReadBitflags(BitflagsSchema schema)
        {
            // TODO... we just skip over bitfield for now
            var storageResult = ReadSchemaByName(schema.Storage) as BasicBlock;
            return storageResult;
        }

        public Block ReadField(FieldSchema field, Dictionary<string, Block> fields, CompoundContext context = null)
        {
            var childContext = new CompoundContext()
            {
                Owner = context?.Owner
            };
            if (field.Argument != null)
                childContext.Argument = InterpretExpression(field.Argument, "arg").AsString;

            if (field.Template != null)
                childContext.Template = InterpretExpression(field.Template, "template").AsString;

            if (field.MinVersion != null && Version < field.MinVersion)
                return null;
            if (field.MaxVersion != null && Version > field.MaxVersion)
                return null;

            if (field.VersionCondition != null)
                if (!InterpretExpression(field.VersionCondition, "vercond").AsBoolean)
                    return null;

            if (field.Condition != null)
                if (!InterpretExpression(field.Condition, "cond").AsBoolean)
                    return null;

            if (field.OnlyT != null)
                if (context != null && context.Owner != field.OnlyT)
                    return null;

            var fieldType = InterpretExpression(field.Type, "type").AsString;

            if (field.Dimensions != null && field.Dimensions.Count > 0)
            {
                var count = field.Dimensions
                    .Select((dimensionSource, i) => InterpretExpression(dimensionSource, $"arr{i + 1}").AsInterger)
                    .Aggregate((prev, curr) => prev * curr);
                var results = Enumerable.Range(0, (int)count)
                    .Select(_ => ReadSchemaByName(fieldType, childContext))
                    .ToList();
                return new ListBlock(results);
            }

            return ReadSchemaByName(fieldType, childContext);

            Logic.Value InterpretExpression(string expressionSource, string attribute)
            {
                var subsitutedSource = ResolveSubsitutions(expressionSource, attribute, context);
                var identifiers = fields
                    .Select(kv => (kv.Key, Logic.Value.From(kv.Value)))
                    .Concat(GlobalIdentifiers.Select(kv => (kv.Key, kv.Value)))
                    .ToDictionary(kv => kv.Key, kv => kv.Item2);
                var state = new Logic.Interpreter.State() {
                    Identifiers = identifiers
                };
                var tokens = Logic.Lexer.ReadSource(subsitutedSource);
                var expression = Logic.Parser.Parse(tokens);
                var value = Logic.Interpreter.Interpret(expression, state);
                return value;
            }
        }

        public string ResolveSubsitutions(string content, string attribute = null, CompoundContext context = null)
        {
            return string.Join("", content
                .Split('#')
                .Select((identifier, index) =>
                {
                    if (index % 2 == 0)
                        return identifier;

                    if (TokenSubsitutions.TryGetValue((attribute, $"#{identifier}#"), out var globalIdentifer))
                        return globalIdentifer;

                    if (identifier == "T" && context.Template != null)
                        return context.Template;
                    if (identifier == "ARG" && context.Argument != null)
                        return context.Argument;

                    throw new Exception("Undefined subsitution");
                })
            );
        }
    }

    // A . B > C
    /*

    /// <summary>
    /// "Block Structure" is a document that describes a binary type.
    /// I think it used to just be _the_ "nif.xml", but is now
    /// its own independant system.
    /// https://github.com/niftools/nifxml/wiki
    /// </summary>
    public class BlockStructureSchema
    {

        public RootNode Root { get; set; }
        public Dictionary<string, Dictionary<string, string>> TokenSubsitutions { get; set; }

        public BlockStructureSchema(XElement rootElement)
        {
            Root = new RootNode(rootElement);
            TokenSubsitutions = new Dictionary<string, Dictionary<string, string>>();

            foreach (var token in Root.Tokens.Reverse<Schemas.TokenSchema>())
                foreach (var entry in token.Entries)
                    foreach (var attribute in token.Attributes)
                    {
                        var subsitutions = TokenSubsitutions.ReadOrSet(
                            attribute,
                            new Dictionary<string, string>()
                        );
                        subsitutions.Add(
                            entry.Identifier.Substring(1, entry.Identifier.Length - 2),
                            ResolveSubsitutions(entry.Content, subsitutions)
                        );
                    }
        }

        public static string ResolveSubsitutions(string content, Dictionary<string, string> subsitutions)
        {
            return string.Join("", content
                .Split('#')
                .Select((s, i) => i % 2 == 0 ? s : subsitutions[s])
            );
        }

        public BlockStructureReader.CompoundResult ReadHeader(Stream stream)
        {
            var binaryStream = new BinaryReader(stream);
            var reader = new BlockStructureReader() {
                Reader = binaryStream,
                Schema = this
            };
            return reader.ReadCompoundByName("Header");
        }

        public BlockStructureReader.NiObjectResult ReadNiObject(Stream stream, NiObjectSchema schema)
        {
            var binaryStream = new BinaryReader(stream);
            var reader = new BlockStructureReader()
            {
                Reader = binaryStream,
                Schema = this
            };
            return reader.ReadNiObject(schema);
        }
    }

    public class BlockStructureReader
    {
        public BlockStructureSchema Schema { get; set; }
        public BinaryReader Reader { get; set; }

        int version = VersionParser.Parse("20.2.0.7");

        public CompoundResult ReadCompoundByName(string name)
        {
            var headerCompound = Schema.Root.Compounds[name];
            return ReadCompound(headerCompound);
        }
        public NiObjectResult ReadByNiObjectName(string name)
        {
            var schema = Schema.Root.NiObjects[name];
            return ReadNiObject(schema);
        }
        long ReadEnum(EnumNode @enum)
        {
            switch (@enum.Storage)
            {
                case BasicTypeReference basicRef:
                    if (basicRef.ReferenceFor.Size != -1)
                        Reader.ReadBytes(basicRef.ReferenceFor.Size);
                    return 0;
                default:
                    throw new Exception("Complicated enum!");
            }
        }

        public CompoundResult ReadCompound(CompoundSchema compound)
        {
            var fields = new Dictionary<string, Result>();
            foreach (var field in compound.Fields)
                fields.Add(field.Name, ReadField(field, fields));
            return new CompoundResult()
            {
                Name = compound.Name,
                Fields = fields,
            };
        }

        public NiObjectResult ReadNiObject(NiObjectSchema niObject)
        {
            var fields = new Dictionary<string, Result>();
            foreach (var field in niObject.Fields)
                fields.Add(field.Name, ReadField(field, fields));
            return new NiObjectResult()
            {
                Name = niObject.Name,
                FieldResults = fields,
            };
        }

        public Result ReadField(FieldSchema field, Dictionary<string, Result> previousResults)
        {
            if (field.MinVersion != null && version < field.MinVersion)
                return null;
            if (field.MaxVersion != null && version > field.MaxVersion)
                return null;

            if (field.Condition != null)
                if (!IsConditionSatified(field.Condition, previousResults))
                    return null;

            if (field.VersionCondition != null)
                if (!IsVercondSatisified(field.VersionCondition, previousResults))
                    return null;

            if (field.Dimensions != null && field.Dimensions.Count > 0)
                return new ListResult(
                    Enumerable.Range(0, (int)GetArrayCount(field.Dimensions, previousResults))
                        .Select(_ => ReadTypeReference(field.Type))
                        .ToList()
                );

            return ReadTypeReference(field.Type);
        }

        public Result ReadTypeReference(TypeReference reference)
        {
            switch (reference)
            {
                case BasicTypeReference basicRef:
                    return new BasicResult(BasicReader.Read(Reader, basicRef.ReferenceFor));
                case EnumTypeReference enumRef:
                    return new EnumResult(ReadEnum(enumRef.ReferenceFor));
                case CompoundTypeReference compRef:
                    return ReadCompound(compRef.ReferenceFor);
                default:
                    throw new NotImplementedException();
            }
        }

        public Logic.Interpreter.State BuildState(Dictionary<string, Result> previousResults)
        {
            return new Logic.Interpreter.State()
            {
                Identifiers = previousResults
                    .Select(kv => kv.Value is BasicResult basic ?
                        (kv.Key, Logic.Interpreter.Result.Convert(basic.Value)) :
                        (kv.Key, null))
                    .Where(kv => kv.Item2 != null)
                    .Concat(
                        new List<(string, Logic.Interpreter.Result)>() {
                            ("Version", Logic.Interpreter.Result.Convert(version)),
                            ("User Version", Logic.Interpreter.Result.Convert(12)),
                            ("BS Header BS Version", Logic.Interpreter.Result.Convert(100)),
                        }
                        .Where(kv => !previousResults.ContainsKey(kv.Item1))
                    )
                    .ToDictionary(kv => kv.Item1, kv => kv.Item2)
            };
        }

        public bool IsConditionSatified(string source, Dictionary<string, Result> previousResults)
        {
            var subsitutedSource = BlockStructureSchema.ResolveSubsitutions(
                source,
                Schema.TokenSubsitutions[$"cond"]
            );
            var state = BuildState(previousResults);
            var result = Logic.Logic.Run(subsitutedSource, state);

            if (result is Logic.Interpreter.BooleanResult booleanResult)
                return booleanResult.Value;

            throw new Exception();
        }

        public bool IsVercondSatisified(string source, Dictionary<string, Result> previousResults)
        {
            var subsitutedSource = BlockStructureSchema.ResolveSubsitutions(
                source,
                Schema.TokenSubsitutions[$"vercond"]
            );
            var state = BuildState(previousResults);
            var result = Logic.Logic.Run(subsitutedSource, state);

            if (result is Logic.Interpreter.BooleanResult booleanResult)
                return booleanResult.Value;

            throw new Exception();
        }

        public long GetArrayCount(List<string> dimensions, Dictionary<string, Result> previousResults)
        {
            var state = BuildState(previousResults);
            var count = dimensions.Select((dimension, index) =>
            {
                var subsitutedSource = BlockStructureSchema.ResolveSubsitutions(
                    dimension,
                    Schema.TokenSubsitutions[$"arr{index + 1}"]
                );
                var result = Logic.Logic.Run(subsitutedSource, state);
                if (result is Logic.Interpreter.NumberResult numberResult)
                    return numberResult.Value;
                throw new Exception();
            }).Aggregate((prev, next) => prev * next);

            return count;
        }
    }
    */
}
