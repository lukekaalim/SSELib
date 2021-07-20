using System;
using System.Linq;
using System.Collections.Generic;

using SSE.TESVNif.BlockStructure.Schemas;

namespace SSE.TESVNif.BlockStructure
{
    public abstract class Data { }

    public class FieldedData : Data
    {
        public T GetBasic<T>(string fieldName)
        {
            var data = (BasicData)Fields[fieldName];
            return (T)data.Value;
        }

        public CompoundData GetCompound(string fieldName)
        {
            return (CompoundData)Fields[fieldName];
        }

        public List<T> GetBasicList<T>(string fieldName)
        {
            var data = (ListData)Fields[fieldName];
            return data.Contents
                .Select(c => (BasicData)c)
                .Select(b => (T)b.Value)
                .ToList();
        }
        public List<CompoundData> GetCompoundList(string fieldName)
        {
            var data = (ListData)Fields[fieldName];
            return data.Contents
                .Select(c => (CompoundData)c)
                .ToList();
        }

        public Dictionary<string, Data> Fields { get; set; }
    }

    public class BlockData : FieldedData
    {
        public string Name { get; set; }
        public List<NiObjectSchema> Inheritance { get; set; }
    }

    public class BasicData : Data
    {
        public object Value { get; set; }
        public BasicData(object value) => Value = value;
    }

    public class CompoundData : FieldedData
    {
        public string Name { get; set; }
    }

    public class EnumData : Data
    {
        public long Value { get; set; }
        public EnumData(long value) => Value = value;
    }

    public class ListData : Data
    {
        public List<Data> Contents { get; set; }
        public ListData(List<Data> contents) => Contents = contents;
    }
}
