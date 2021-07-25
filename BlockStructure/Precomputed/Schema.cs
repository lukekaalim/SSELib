using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure.Precomputed
{
    public class TypeContext
    {
        public string TName { get; set; }
        public TypeReference WithTemplate { get; set; }
        public TypeReference ArgumentType { get; set; }
        public PDocumentSchema Document { get; set; }
    }

    public struct TypeReferenceKey
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public string TName { get; set; }
    }

    public class TypeReference
    {
        public string Name { get; set; }

        public static TypeReference CreateNew(TypeReferenceKey key, string type, TypeContext context)
        {
            if (context.Document.BaseDocument.Compounds.TryGetValue(type, out var comp))
                return new BlockTypeReference(key, comp, context);
            if (context.Document.BaseDocument.NiObjects.TryGetValue(type, out var ni))
                return new BlockTypeReference(key, ni, context);
            if (context.Document.BaseDocument.Basics.TryGetValue(type, out var basic))
                return new BasicTypeReference(basic);
            if (context.Document.BaseDocument.Bitfields.TryGetValue(type, out var fields))
                return new BasicTypeReference(fields);
            if (context.Document.BaseDocument.Enums.TryGetValue(type, out var @enum))
                return new BasicTypeReference(@enum);
            if (context.Document.BaseDocument.Bitflags.TryGetValue(type, out var flags))
                return new BasicTypeReference(flags);

            throw new Exception();
        }

        public static TypeReference From(string type, TypeContext context)
        {
            var key = new TypeReferenceKey()
            {
                Name = type,
                Template = context.WithTemplate?.Name,
                TName = context.TName
            };
            if (context.Document.References.TryGetValue(key, out var exisingReference))
                return exisingReference;
            return CreateNew(key, type, context);
        }
    }
    public class BlockTypeReference : TypeReference
    {
        public PBlockSchema Block { get; set; }
        public BlockTypeReference(TypeReferenceKey key, CompoundSchema comp, TypeContext context)
        {
            context.Document.References.Add(key, this);

            Name = comp.Name;
            Block = new PBlockSchema(comp, context);
        }
        public BlockTypeReference(TypeReferenceKey key, NiObjectSchema ni, TypeContext context)
        {
            context.Document.References.Add(key, this);

            Name = ni.Name;
            Block = new PBlockSchema(ni, context);
        }
    }
    public class BasicTypeReference : TypeReference
    {
        public string Basic { get; set; }
        public BasicTypeReference(BasicSchema basic)
        {
            Name = basic.Name;
            Basic = basic.Name;
        }
        public BasicTypeReference(EnumSchema eum)
        {
            Name = eum.Name;
            Basic = eum.Storage;
        }
        public BasicTypeReference(BitFieldSchema field)
        {
            Name = field.Name;
            Basic = field.Storage;
        }
        public BasicTypeReference(BitflagsSchema flags)
        {
            Name = flags.Name;
            Basic = flags.Storage;
        }
    }

    public class PFieldSchema
    {
        public string Name { get; set; }
        public TypeReference Type { get; set; }

        public Logic.Expression Condition { get; set; }
        public List<Logic.Expression> Dimensions { get; set; }
        public Logic.Expression Argument { get; set; }
        public bool ValidForVersion { get; set; }

        public static PFieldSchema CreateField(FieldSchema field, List<FieldSchema> fields, TypeContext context)
        {
            var doc = context.Document;

            var typeSubsitutes = new Dictionary<string, string>();
            if (context.WithTemplate != null)
            {
                typeSubsitutes.Add("T", context.WithTemplate.Name);
            }
            var childContext = new TypeContext()
            {
                Document = context.Document,
            };
            if (context.TName != null)
            {
                childContext.TName = context.TName;
            }
            if (field.Template != null)
            {
                var subtitutedTemplate = TokenSubsitution
                    .ResolveSubsitutions(field.Template, typeSubsitutes);
                var template = TypeReference.From(subtitutedTemplate, context);
                childContext.WithTemplate = template;
            }
            var dimensions = field.Dimensions
                .Select((dimension, index) =>
                {
                    var subsituedDimension = TokenSubsitution
                        .ResolveSubsitutions(dimension, doc.Tokens[$"arr{index + 1}"]);
                    var tokens = Logic.Lexer
                        .ReadSource(subsituedDimension);
                    return Logic.Parser.Parse(tokens);
                })
                .ToList();

            var type = TokenSubsitution.ResolveSubsitutions(field.Type, typeSubsitutes);
            var fieldSchema = new PFieldSchema()
            {
                Name = field.Name,
                Type = TypeReference.From(type, childContext),
                Dimensions = dimensions,
                ValidForVersion = true,
            };

            if (field.Argument != null)
            {
                var argSubs = doc.Tokens["arg"];
                var subsitutedArgument = TokenSubsitution.ResolveSubsitutions(field.Argument, argSubs);
                var tokens = Logic.Lexer
                    .ReadSource(subsitutedArgument);
                var expression = Logic.Parser.Parse(tokens);
                fieldSchema.Argument = expression;
            }
            if (field.MinVersion != null && doc.NifVersion < field.MinVersion)
                fieldSchema.ValidForVersion = false;
            if (field.MaxVersion != null && doc.NifVersion > field.MaxVersion)
                fieldSchema.ValidForVersion = false;
            if (field.VersionCondition != null)
            {
                var conditionSubsitutes = doc.Tokens["vercond"];
                var subsitutedCondition = TokenSubsitution
                    .ResolveSubsitutions(field.VersionCondition, conditionSubsitutes);
                var tokens = Logic.Lexer
                    .ReadSource(subsitutedCondition);
                var expression = Logic.Parser.Parse(tokens);
                var state = new Logic.Interpreter.State()
                {
                    Identifiers = doc.StaticGlobals
                };
                var value = Logic.Interpreter.Interpret(expression, state);
                if (!value.AsBoolean)
                    fieldSchema.ValidForVersion = false;
            }
            if (field.Condition != null)
            {
                var subsitutes = doc.Tokens["cond"];
                var tokens = Logic.Lexer.ReadSource(TokenSubsitution.ResolveSubsitutions(field.Condition, subsitutes));
                var expression = Logic.Parser.Parse(tokens);

                var localIdentifiers = fields
                    .Where(f => !doc.StaticGlobals.ContainsKey(f.Name))
                    .Select(f => f.Name)
                    .Concat(new List<string> { "Argument" });
                var textExpressions = Logic.TextExpression.FindAll(expression);
                var state = new Logic.Interpreter.State()
                {
                    Identifiers = doc.StaticGlobals
                };

                if (!textExpressions.Any(exp => localIdentifiers.Contains(exp.Text)))
                    if (!Logic.Interpreter.Interpret(expression, state).AsBoolean)
                        fieldSchema.ValidForVersion = false;
                    else
                        fieldSchema.Condition = null;
                else
                    // Consider creating an expression tree!
                    fieldSchema.Condition = expression;
            }
            if (field.OnlyT != null && context.TName != null)
            {
                if (!doc
                    .NiObjectInheritance[context.TName]
                    .Select(n => n.Name)
                    .Contains(field.OnlyT))
                    fieldSchema.ValidForVersion = false;
            }
            if (field.ExcludeT != null && context.TName != null)
            {
                if (doc
                    .NiObjectInheritance[context.TName]
                    .Select(n => n.Name)
                    .Contains(field.ExcludeT))
                    fieldSchema.ValidForVersion = false;
            }

            return fieldSchema;
        }
    }

    public class PBlockSchema
    {
        public List<PFieldSchema> Fields { get; set; }
        public string Name { get; set; }

        public PBlockSchema(CompoundSchema comp, TypeContext context)
        {
            Name = comp.Name;
            Fields = comp.Fields
                .Select(f => PFieldSchema.CreateField(f, comp.Fields, context))
                .Where(f => f != null)
                .ToList();
        }

        public PBlockSchema(NiObjectSchema ni, TypeContext context)
        {
            Name = ni.Name;

            var inheritance = context.Document.NiObjectInheritance[ni.Name];
            var fields = inheritance
                .SelectMany(i => i.Fields)
                .ToList();

            Fields = fields
                .Select(f => PFieldSchema.CreateField(f, fields, context))
                .ToList();
        }
    }
}
