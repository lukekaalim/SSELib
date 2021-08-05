using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using BlockStructure.Logic;

namespace BlockStructure.Precomputed
{
    public class PReader
    {
        public BinaryReader Reader { get; set; }
        public BasicReader BasicReader { get; set; }
        public PDocumentSchema Document { get; set; }

        public Data ReadBlock(string blockName, Value argument = null)
        {
            return ReadBlock(Document.Blocks[blockName], argument);
        }

        public Data ReadBasic(string type)
        {
            return new BasicData(BasicReader.Read(Reader, type));
        }

        public Data ReadBlock(PBlockSchema blockSchema, Value argument = null)
        {
            var block = new BlockData()
            {
                Fields = new Dictionary<string, Data>(),
                Name = blockSchema.Name,
            };
            var state = new Interpreter.State()
            {
                Identifiers = blockSchema.AllIdentifiers
                    .ToDictionary(id => id, _ => Value.From(0))
            };
            state.Identifiers.Add("Argument", argument);
            foreach (var fieldSchema in blockSchema.Fields)
            {
                var field = ReadField(fieldSchema, state);
                if (field == null)
                    continue;
                state.Identifiers[fieldSchema.Name] = Value.From(field);
                block.Fields.Add(fieldSchema.Name, field);
            }
            return block;
        }

        public Data ReadField(PFieldSchema fieldSchema, Interpreter.State state)
        {
            if (fieldSchema.Condition != null)
            {
                var condValue = Interpreter.Interpret(fieldSchema.Condition, state);
                if (!condValue.AsBoolean)
                    return null;
            }
            Value argument = argument = Interpreter.Interpret(fieldSchema.Argument, state);
            if (fieldSchema.Dimensions.Count > 0)
            {
                var count = fieldSchema.Dimensions
                    .Select(d => Interpreter.Interpret(d, state).AsInterger)
                    .Aggregate((curr, next) => curr * next);
                var data = new Data[count];
                for (int i = 0; i < count; i++)
                {
                    data[i] = ReadReference(fieldSchema.Type, argument);
                }
                return new ListData(data.ToList());
            }
            return ReadReference(fieldSchema.Type, argument);
        }

        public Data ReadReference(TypeReference reference, Value argument = null)
        {
            switch (reference)
            {
                case BasicTypeReference basic:
                    return ReadBasic(basic.Basic);
                case BlockTypeReference block:
                    return ReadBlock(block.Block, argument);
                default:
                    throw new Exception();
            }
        }
    }
}
