using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure
{
    public class TypeLookup
    {
        public abstract class Result
        {
            public class Compound : Result
            {
                public CompoundSchema Schema { get; set; }
            }
            public class NiObject : Result
            {
                public NiObjectSchema Schema { get; set; }
            }
            public class Basic : Result
            {
                public BasicSchema Schema { get; set; }
            }
        }

        IReadOnlyDictionary<string, CompoundSchema> Compounds { get; }
        IReadOnlyDictionary<string, NiObjectSchema> NiObjects { get; }
        IReadOnlyDictionary<string, BasicSchema> Basics { get; }

        public IReadOnlyDictionary<FieldSchema, string> ParentNameBySchema { get;  }

        public TypeLookup(IEnumerable<CompoundSchema> compounds, IEnumerable<NiObjectSchema> niObjects)
        {
            var compoundFieldParents = compounds
                .SelectMany(c => c.Fields.Select(f => (Field: f, Name: c.Name)));
            var niObjectFieldParents = niObjects
                .SelectMany(ni => ni.Fields.Select(f => (Field: f, Name: ni.Name)));

            ParentNameBySchema = compoundFieldParents
                .Concat(niObjectFieldParents)
                .ToDictionary(p => p.Field, p => p.Name);
        }

        public Result FindTypeByName(string typeName)
        {
            if (Compounds.TryGetValue(typeName, out var compoundSchema))
                return new Result.Compound() { Schema = compoundSchema };
            if (Basics.TryGetValue(typeName, out var basicSchema))
                return new Result.Basic() { Schema = basicSchema };
            if (NiObjects.TryGetValue(typeName, out var niObjectSchema))
                return new Result.NiObject() { Schema = niObjectSchema };

            throw new Exception();
        }
    }
}
