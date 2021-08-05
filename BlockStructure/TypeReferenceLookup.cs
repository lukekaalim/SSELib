using System;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure
{
    public class TypeKey : IEquatable<TypeKey>
    {
        public string Name { get; set; }
        public string Template { get; set; }

        public TypeKey() { }
        public TypeKey(FieldSchema field, TypeKey parentRef)
        {
            if (field.Name == "#T#")
                Name = parentRef.Template;
            else
                Name = field.Name;

            if (field.Template == "#T#")
                Template = parentRef.Template;
            else
                Template = field.Template;
        }

        public override bool Equals(object obj) =>
            Equals(obj as TypeKey);

        public bool Equals(TypeKey other) =>
            Name == other.Name &&
            Template == other.Template;

        public override int GetHashCode() =>
            (Name, Template).GetHashCode();
    }

    public class TypeReferenceLookup
    {
        public Dictionary<TypeKey, HashSet<FieldSchema>> ExclusionsForType;

        public TypeReferenceLookup(
            Dictionary<string, NiObjectSchema> niObjects,
            Dictionary<string, CompoundSchema> compounds
            )
        {
            var referencesInNiObjects = niObjects
                .Values
                .SelectMany(ni =>
                    ni.Fields.Select(f =>
                        new TypeKey() { Name = f.Type, Template = f.Template }));
            var referencesInCompounds = compounds
                .Values
                .Where(co => !co.Generic)
                .SelectMany(co =>
                    co.Fields.Select(f =>
                        new TypeKey() { Name = f.Type, Template = f.Template }));

            var genericReferences = referencesInNiObjects.Concat(referencesInCompounds)
                .Where(trk => trk.Template != null);

            var referencesInGenerics = genericReferences
                .SelectMany(trk =>
                    FindAllReferencesForKey(trk, new HashSet<TypeKey>()))
                .ToList();

            List<TypeKey> FindAllReferencesForKey (TypeKey key, HashSet<TypeKey> visisted)
            {
                if (visisted.Contains(key))
                    return new List<TypeKey>();

                if (compounds.TryGetValue(key.Name, out var compoundSchema)) {
                    var nextVisited = new HashSet<TypeKey>(visisted);
                    nextVisited.Add(key);

                    return compoundSchema.Fields
                        .SelectMany(field => {
                            var fieldKey = new TypeKey(field, key);
                            var fieldKeyChildren = new List<TypeKey>(FindAllReferencesForKey(fieldKey, nextVisited));
                            fieldKeyChildren.Add(fieldKey);
                            return fieldKeyChildren;
                            })
                        .ToList();
                }
                return new List<TypeKey>();
            }

            var allReferences = new List<TypeKey>()
                .Concat(referencesInNiObjects)
                .Concat(referencesInCompounds)
                .Concat(referencesInGenerics);
        }
    }
}
