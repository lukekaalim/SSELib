using System;
using System.Collections.Generic;
using System.Linq;

namespace BlockStructure.Logic
{
    /*
    public abstract class Oldvalue
    {
        public virtual long AsInterger =>
            throw new Exception("Cannot cast Type");

        public virtual double AsFloat =>
            throw new Exception("Cannot cast Type");

        public virtual string AsString =>
            throw new Exception("Cannot cast Type");

        public virtual bool AsBoolean =>
            throw new Exception("Cannot cast Type");

        public virtual Dictionary<string, Oldvalue> AsStructure =>
            throw new Exception("Cannot cast Type");


        public static Oldvalue From(bool value)
            => new BooleanValue(value);
        public static Oldvalue From(string value)
            => new StringValue(value);
        public static Oldvalue From(long value)
            => new IntergerValue(value);
        public static Oldvalue From(float value)
            => new FloatValue(value);

        public static Oldvalue From(Data result)
        {
            switch (result)
            {
                case BasicData basic:
                    switch (basic.Value)
                    {
                        case byte b:
                            return From(b);
                        case short s:
                            return From(s);
                        case ushort us:
                            return From(us);
                        case float fl:
                            return From(fl);
                        case int i:
                            return From(i);
                        case uint ui:
                            return From(ui);
                        case string st:
                            return From(st);
                        case ulong ul:
                            return From((long)ul);
                        default:
                            return null;
                    }
                case SchemaData schemaData:
                    return new StructureValue(schemaData.Fields
                        .Where(kv => kv.Value is BasicData || kv.Value is CompoundData)
                        .Where(kv => kv.Value != null)
                        .ToDictionary(kv => kv.Key.Name, kv => From(kv.Value)));
                case CompoundData compound:
                    return new StructureValue(compound.Fields
                        .Where(kv => kv.Value is BasicData || kv.Value is CompoundData)
                        .ToDictionary(kv => kv.Key, kv => From(kv.Value)));
                case EnumData enumBlock:
                    return From(enumBlock.Value);
                case ListData listBlock:
                    return null;
                case null:
                    return null;
                default:
                    throw new Exception();
            }
        }
    }

    public class IntergerValue : Oldvalue
    {
        public long Content { get; set; }
        public IntergerValue(long content) => Content = content;
        public override string AsString => Content.ToString();
        public override long AsInterger => Content;
        public override bool AsBoolean => Content != 0;
    }

    public class FloatValue : Oldvalue
    {
        public double Content { get; set; }
        public FloatValue(double content) => Content = content;
        public override string AsString => Content.ToString();
        public override double AsFloat => Content;
    }

    public class StringValue : Oldvalue
    {
        public string Content { get; set; }
        public StringValue(string content) => Content = content; 
        public override string AsString => Content;
    }

    public class BooleanValue : Oldvalue
    {
        public bool Content { get; set; }
        public BooleanValue(bool content) => Content = content;
        public override string AsString => Content.ToString();
        public override bool AsBoolean => Content;
    }

    public class StructureValue : Oldvalue
    {
        public Dictionary<string, Oldvalue> Content { get; set; }
        public StructureValue(Dictionary<string, Oldvalue> content) => Content = content;
        public override Dictionary<string, Oldvalue> AsStructure => Content;
    }
    */
}
