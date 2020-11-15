using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace SSE
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
        public bool isLocalizd;
        public UInt32 localizedId;
        public ZString content;

        public static LString From(byte[] bytes, TES4Record parent)
        {
            if (parent.flags.HasFlag(TES4Record.Flags.Localized))
            {
                return new LString()
                {
                    isLocalizd = true,
                    localizedId = BitConverter.ToUInt32(bytes, 0)
                };
            } else
            {
                return new LString()
                {
                    isLocalizd = false,
                    content = ZString.From(bytes)
                };
            }
        }

        public string GetResolvedContent(StringTable table)
		{
            if (!isLocalizd)
                return content.content;
            return table.strings[localizedId];
		}
    }

    /// <summary>
    /// A string prefixed with a byte length and terminated with a zero (\x00).
    /// </summary>
    public struct BZString
	{
        public string content;
        public int length;
        public static BZString From(Byte[] bytes, int offset)
		{
            var length = bytes[offset];
            return new BZString()
            {
                content = Encoding.UTF8.GetString(bytes, offset + 1, length - 1),
                length = length
            };
		}
	}
    public struct Hash
	{
        public UInt64 value;
        public static Hash From(Byte[] bytes, int offset)
		{
            return new Hash()
            {
                value = BitConverter.ToUInt64(bytes, offset),
            };
		}
	}
    /// <summary>
    /// A ulong used to identify a data object.
    /// May refer to a data object from a mod or new object created in-game.
    /// </summary>
    public struct FormID
    {
        public static uint Size = 4;
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
}
