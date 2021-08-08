﻿using System;
using System.Linq;
using System.Collections.Generic;
using BlockStructure.Schemas;

namespace BlockStructure
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
    }

    public class CompoundData : FieldedData
    {
        public CompoundSchema Schema { get; set; }
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
