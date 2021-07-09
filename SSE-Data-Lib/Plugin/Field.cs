using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Plugin
{
	public struct Field
    {
        public static uint FixedSize = 4 + 2;
        public uint Size => FixedSize + dataSize;

        public string Type { get; set; } // 4 bytes
        public UInt16 dataSize;
        public Byte[] data;

        public static Field Parse(Byte[] bytes, int offset)
        {
            var type = Encoding.UTF8.GetString(bytes, offset, 4);
            var dataSize = BitConverter.ToUInt16(bytes, offset + 4);
            var data = bytes[(offset + (int)FixedSize)..(offset + (int)FixedSize + dataSize)];

            return new Field()
            {
                Type = type,
                dataSize = dataSize,
                data = data,
            };
        }

        public static List<Field> ParseAll(Byte[] bytes, int offset, int length)
        {
            var fields = new List<Field>();
            int index = offset;
            while (index < offset + length)
            {
                var field = Parse(bytes, index);
                index += (int)field.Size;
                fields.Add(field);
            }
            return fields;
        }
    }
}
