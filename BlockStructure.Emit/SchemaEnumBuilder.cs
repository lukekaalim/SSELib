using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

using BlockStructure.Schemas;
using CaseExtensions;


namespace BlockStructure.Emit
{
    public class SchemaEnumBuilder
    {
        SchemaTypeBuilder SchemaTypes { get; set; }
        public Dictionary<EnumSchema, Dictionary<string, FieldBuilder>> OptionsByEnum { get; set; }
        public Dictionary<BitflagsSchema, Dictionary<string, FieldBuilder>> OptionsByBitflags { get; set; }

        public SchemaEnumBuilder(SchemaTypeBuilder schemaTypes)
        {
            SchemaTypes = schemaTypes;

            OptionsByEnum = schemaTypes.EnumTypes
                .Select(kvp =>
                    (kvp.Key, kvp.Key.Options.Select(option =>
                        (option, BuildEnumOption(kvp.Value, option)))
                    .ToDictionary(k => k.option.Name, k => k.Item2)))
                .ToDictionary(k => k.Key, k => k.Item2);

            OptionsByBitflags = schemaTypes.BitflagsTypes
                .Select(kvp =>
                    (kvp.Key, kvp.Key.Options.Select(option =>
                        (option, BuildBitflagsOption(kvp.Value, option)))
                    .ToDictionary(k => k.option.Name, k => k.Item2)))
                .ToDictionary(k => k.Key, k => k.Item2);
        }

        FieldBuilder BuildEnumOption(EnumBuilder enumBuilder,
                                     EnumSchema.Option option)
        {
            var underlyingType = enumBuilder.UnderlyingSystemType;
            var value = Convert.ChangeType(option.Value, underlyingType);
            return enumBuilder.DefineLiteral(option.Name, value);
        }

        FieldBuilder BuildBitflagsOption(EnumBuilder enumBuilder,
                                         BitflagsSchema.Option option)
        {
            var underlyingType = enumBuilder.UnderlyingSystemType;
            var value = Convert.ChangeType(Math.Pow(2, option.Bit), underlyingType);
            return enumBuilder.DefineLiteral(option.Name, value);
        }
    }
}
