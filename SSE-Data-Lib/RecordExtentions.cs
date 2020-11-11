using System;
using System.Collections.Generic;

namespace SSE
{
    /// <summary>
    /// Helper methods to get find fields on records and cast them to appropriate types
    /// </summary>
    public static class RecordExtensions
    {

        public static T GetFirstField<T>(this Record record, string type, Func<byte[], T> castToValue)
        {
            var field = record.data
                .Find(field => string.Equals(field.type, type, StringComparison.OrdinalIgnoreCase));
            if (field.data == null)
                return default(T);
            return castToValue(field.data);
        }

        public static List<T> GetAllFields<T>(this Record record, string type, Func<byte[], T> castToValue)
        {
            var values = new List<T>();
            for (int i = 0; i < record.data.Count; i++)
            {
                if (!string.Equals(record.data[i].type, type, StringComparison.OrdinalIgnoreCase))
                    continue;
                var field = record.data[i];
                var value = castToValue(field.data);
                values.Add(value);
            }
            return values;
        }
    }
}