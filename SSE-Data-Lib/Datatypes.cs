using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SSEData
{
    /// <summary>
    /// Zero terminated string.
    /// Size is size of string text + 1 for string terminator.
    /// </summary>
    public struct ZString
    {
        public string content;

        public static ZString From(byte[] bytes)
        {
            return new ZString()
            {
                content = Encoding.UTF8.GetString(bytes, 0, Array.IndexOf(bytes, (byte)0))
            };
        }
    }
    /// <summary>
    /// 'Localized string', a ulong that is used as an index to look up string
    /// data information in one of the string tables. Note that the record
    /// header for the TES4 record indicates if the file is localized or not.
    /// If not, all lstrings are zstrings.
    /// </summary>
    public struct LString
    {
        public UInt32 localizedId;
        public ZString content;

        public static LString From(byte[] bytes, TES4Record parent)
        {
            if (parent.flags.HasFlag(TES4Record.Flags.Localized))
            {
                return new LString()
                {
                    localizedId = BitConverter.ToUInt32(bytes, 0)
                };
            } else
            {
                return new LString()
                {
                    content = ZString.From(bytes)
                };
            }
        }
    }
    /// <summary>
    /// A ulong used to identify a data object.
    /// May refer to a data object from a mod or new object created in-game.
    /// </summary>
    public struct FormID
    {
        public UInt32 id;

        public static FormID From(Byte[] bytes)
        {
            return new FormID()
            {
                id = BitConverter.ToUInt32(bytes, 0)
            };
        }
        public static FormID From(Byte[] bytes, int offset)
        {
            return new FormID()
            {
                id = BitConverter.ToUInt32(bytes, offset)
            };
        }
    }

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
