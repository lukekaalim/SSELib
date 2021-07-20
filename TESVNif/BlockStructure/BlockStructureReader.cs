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
        public class BlockContext
        {
            public List<string> Inheritance { get; set; }
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

        public Data ReadSchemaByName(string schemaName, BlockContext context = null)
        {
            if (Document.Basics.TryGetValue(schemaName, out var basicSchema))
                return ReadBasic(basicSchema);
            if (Document.Compounds.TryGetValue(schemaName, out var compoundSchema))
                return ReadCompound(compoundSchema, context);
            if (Document.Enums.TryGetValue(schemaName, out var enumSchema))
                return ReadEnum(enumSchema);
            if (Document.NiObjects.TryGetValue(schemaName, out var niObjectSchema))
                return ReadBlock(niObjectSchema);
            if (Document.Bitfields.TryGetValue(schemaName, out var bitFieldSchema))
                return ReadBitField(bitFieldSchema);
            if (Document.Bitflags.TryGetValue(schemaName, out var bitflagsSchema))
                return ReadBitflags(bitflagsSchema);

            throw new NotImplementedException();
        }

        public BasicData ReadBasic(BasicSchema schema)
        {
            return new BasicData(BasicReader.Read(Reader, schema));
        }

        public CompoundData ReadCompound(CompoundSchema schema, BlockContext context = null)
        {
            var fields = new Dictionary<string, Data>();
            var fieldSchemas = schema.Fields;
            var identifiers = fieldSchemas
                .GroupBy(f => f.Name)
                .Select(f => new KeyValuePair<string, Logic.Value>(f.Key, Logic.Value.From(0)))
                .Concat(GlobalIdentifiers)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var state = new Logic.Interpreter.State()
            {
                Identifiers = identifiers,
            };
            foreach (var field in fieldSchemas.Where(f => IsValidField(f)))
            {
                var result = ReadField(field, state, context);
                if (result == null)
                    continue;
                fields.Add(field.Name, result);
                identifiers[field.Name] = Logic.Value.From(result);
            }

            return new CompoundData()
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

            return inheritedFields.Concat(schema.Fields);
        }

        public List<NiObjectSchema> GetInheritenceChain(NiObjectSchema schema)
        {
            if (schema.Inherits == null)
                return new List<NiObjectSchema>() { schema };

            var chain = GetInheritenceChain(Document.NiObjects[schema.Inherits]);
            chain.Add(schema);
            return chain;
        }

        public BlockData ReadParentBlock(NiObjectSchema schema)
        {
            if (schema.Inherits == null)
                return null;

            var parentSchema = Document.NiObjects[schema.Inherits];
            return ReadBlock(parentSchema);
        }

        public BlockData ReadBlock(NiObjectSchema schema)
        {
            var fields = new Dictionary<string, Data>();

            var inheritance = GetInheritenceChain(schema);
            var fieldSchemas = inheritance
                .SelectMany(s => s.Fields);

            var identifiers = fieldSchemas
                .GroupBy(f => f.Name)
                .Select(f => new KeyValuePair<string, Logic.Value>(f.Key, Logic.Value.From(0)))
                .Concat(GlobalIdentifiers)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var state = new Logic.Interpreter.State()
            {
                Identifiers = identifiers,
            };
            var context = new BlockContext()
            {
                Inheritance = inheritance.Select(i => i.Name).ToList()
            };

            foreach (var field in fieldSchemas.Where(s => IsValidField(s, context)))
            {
                var result = ReadField(field, state, context);
                if (result == null)
                    continue;
                fields.Add(field.Name, result);
                identifiers[field.Name] = Logic.Value.From(result);
            }

            return new BlockData()
            {
                Name = schema.Name,
                Fields = fields,
                Inheritance = inheritance,
            };
        }

        EnumData ReadEnum(EnumSchema schema)
        {
            var storageResult = ReadSchemaByName(schema.Storage) as BasicData;
            return new EnumData(Convert.ToInt64(storageResult.Value));
        }
        Data ReadBitField(BitFieldSchema schema)
        {
            // TODO... we just skip over bitfield for now
            var storageResult = ReadSchemaByName(schema.Storage) as BasicData;
            return storageResult;
        }
        Data ReadBitflags(BitflagsSchema schema)
        {
            // TODO... we just skip over bitfield for now
            var storageResult = ReadSchemaByName(schema.Storage) as BasicData;
            return storageResult;
        }

        public bool IsValidField(FieldSchema field, BlockContext context = null)
        {
            if (field.MinVersion != null && Version < field.MinVersion)
                return false;
            if (field.MaxVersion != null && Version > field.MaxVersion)
                return false;

            if (field.VersionCondition != null)
                if (!InterpretExpression(field.VersionCondition).AsBoolean)
                    return false;

            if (field.OnlyT != null)
                if (!context.Inheritance.Contains(field.OnlyT))
                    return false;

            if (field.ExcludeT != null)
                if (context.Inheritance.Contains(field.ExcludeT))
                    return false;

            return true;

            Logic.Value InterpretExpression(string expressionSource)
            {
                var subsitutedSource = ResolveSubsitutions(expressionSource, "vercond", null);
                var tokens = Logic.Lexer.ReadSource(subsitutedSource);
                var expression = Logic.Parser.Parse(tokens);
                var state = new Logic.Interpreter.State()
                {
                    Identifiers = GlobalIdentifiers,
                };
                var value = Logic.Interpreter.Interpret(expression, state);
                return value;
            }
        }

        public Data ReadField(FieldSchema field, Logic.Interpreter.State state, BlockContext context = null)
        {
            var childContext = new BlockContext()
            {
                Inheritance = context?.Inheritance
            };
            if (field.Argument != null)
                childContext.Argument = InterpretExpression(field.Argument, "arg").AsString;

            if (field.Template != null)
                childContext.Template = InterpretExpression(field.Template, "template").AsString;

            if (field.Condition != null)
                if (!InterpretExpression(field.Condition, "cond").AsBoolean)
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
                return new ListData(results);
            }

            return ReadSchemaByName(fieldType, childContext);

            Logic.Value InterpretExpression(string expressionSource, string attribute)
            {
                var subsitutedSource = ResolveSubsitutions(expressionSource, attribute, context);
                var tokens = Logic.Lexer.ReadSource(subsitutedSource);
                var expression = Logic.Parser.Parse(tokens);
                var value = Logic.Interpreter.Interpret(expression, state);
                return value;
            }
        }

        public string ResolveSubsitutions(string content, string attribute = null, BlockContext context = null)
        {
            return string.Join("", content
                .Split('#')
                .Select((identifier, index) =>
                {
                    if (index % 2 == 0)
                        return identifier;

                    if (TokenSubsitutions.TryGetValue((attribute, $"#{identifier}#"), out var globalIdentifer))
                        return globalIdentifer;

                    if (context != null) 
                        if (identifier == "T" && context.Template != null)
                            return context.Template;
                        else if (identifier == "ARG" && context.Argument != null)
                            return context.Argument;

                    throw new Exception("Undefined subsitution");
                })
            );
        }
    }
}
