using System;
using System.Linq;
using System.Collections.Generic;
using BlockStructure.Schemas;

namespace BlockStructure.Reading
{
    public abstract class Data { }

    public class SchemaData : Data
    {
        public Dictionary<FieldSchema, Data> Fields { get; set; }
    }

    public class FieldedData : Data
    {
        public T GetBasic<T>(string fieldName)
        {
            var data = (BasicData)Fields[fieldName];
            return (T)data.Value;
        }
        public T TryGetBasic<T> (string fieldName, T fallback)
        {
            if (Fields.TryGetValue(fieldName, out var data))
            {
                var basicData = (BasicData)data;
                return (T)basicData.Value;
            }
            return fallback;
        }

        public CompoundData GetCompound(string fieldName)
        {
            return (CompoundData)Fields[fieldName];
        }
        public CompoundData TryGetCompound(string fieldName)
        {
            if (Fields.TryGetValue(fieldName, out var data))
            {
                var compoundData = (CompoundData)data;
                return compoundData;
            }
            return null;
        }

        public List<T> GetBasicList<T>(string fieldName)
        {
            var data = (ListData)Fields[fieldName];
            return data.Contents
                .Select(c => (BasicData)c)
                .Select(b => (T)b.Value)
                .ToList();
        }
        public List<T> TryGetBasicList<T>(string fieldName, List<T> fallback)
        {
            if (Fields.TryGetValue(fieldName, out var data))
            {
                return ((ListData)data).Contents
                    .Select(c => (BasicData)c)
                    .Select(b => (T)b.Value)
                    .ToList();
            }
            return fallback;
        } 
        public List<CompoundData> GetCompoundList(string fieldName)
        {
            var data = (ListData)Fields[fieldName];
            return data.Contents
                .Select(c => (CompoundData)c)
                .ToList();
        }

        public Dictionary<string, Data> Fields { get; set; }

        public Logic.Interpreter.State AsState()
        {
            var values = Fields
                .Select(kvp => (kvp.Key, kvp.Value as BasicData))
                .Where(kvp => kvp.Item2 != null)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Item2.AsIntergral()
                );
            return new Logic.Interpreter.State()
            {
                ParameterCompoundStates = new Dictionary<string, Logic.Interpreter.State>(),
                ParameterValues = values
            };
        }
    }

    public class NiObjectData : FieldedData
    {
        public string Name { get; set; }
        public NiObjectSchema Schema { get; set; }
    }

    public class BasicData : Data
    {
        public object Value { get; set; }
        public BasicData(object value) => Value = value;

        public long AsIntergral()
        {
            try
            {
                return Convert.ToInt64(Value);
            } catch
            {
                return 0;
            }
        }
    }

    public class CompoundData : FieldedData
    {
        public CompoundSchema Schema { get; set; }
    }

    public class EnumData : BasicData
    {
        public EnumData(long value) : base(value)
        {

        }
    }

    public class ListData : Data
    {
        public List<Data> Contents { get; set; }
        public ListData(List<Data> contents) => Contents = contents;
    }
}
