using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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
            throw new NotImplementedException();
		}
    }

    /// <summary>
    /// A string prefixed with a byte length and terminated with a zero (\x00).
    /// </summary>
    public readonly struct BZString
	{
        public readonly string content;
        public readonly int length;
        public BZString(byte[] bytes, int offset)
        {
            length = bytes[offset];
            content = Encoding.UTF8.GetString(bytes, offset + 1, length - 1);
        }
        public static explicit operator string(BZString b) => b.content;
    }

    public readonly struct BSAHash
	{
        public static int ByteSize => sizeof(ulong);
        public readonly ulong value;
        static uint GetMaskForExtension(string extension) => extension switch
        {
            ".kf" => 0x80,
            ".nif" => 0x8000,
            ".dds" => 0x8080,
            ".wav" => 0x80000000,
            _ => 0x0,
        };

        public BSAHash(byte[] bytes, int offset) => value = BitConverter.ToUInt64(bytes, offset);
        public BSAHash(ulong value) => this.value = value;
        public static BSAHash HashPath(string name)
        {
            name = name.Replace('/', '\\');
            return HashFile(Path.ChangeExtension(name, null), Path.GetExtension(name));
        }
        public static BSAHash HashFile(string name, string extension)
        {
            name = name.ToLowerInvariant();
            extension = extension.ToLowerInvariant();
            var hashBytes = new byte[]
            {
                (byte)(name.Length == 0 ? '\0' : name[name.Length - 1]),
                (byte)(name.Length < 3 ? '\0' : name[name.Length - 2]),
                (byte)name.Length,
                (byte)name[0]
            };
            var hash1 = BitConverter.ToUInt32(hashBytes, 0) | GetMaskForExtension(extension);

            uint hash2 = 0;
            for (var i = 1; i < name.Length - 2; i++)
                hash2 = hash2 * 0x1003f + (byte)name[i];

            uint hash3 = 0;
            for (var i = 0; i < extension.Length; i++)
                hash3 = hash3 * 0x1003f + (byte)extension[i];

            return new BSAHash((((ulong)(hash2 + hash3)) << 32) + hash1);
        }

        public static implicit operator ulong(BSAHash h) => h.value;
    }
    public readonly struct LocalFormID
    {
        public readonly UInt32 formId;
        public LocalFormID(Byte[] bytes, int offset) => formId = BitConverter.ToUInt32(bytes, offset);

        public UInt32 RecordID => formId & 0x00FFFFFF;
        public UInt32 MasterIndex => formId >> (8 * 3);

		public ResolvedFormID Resolve(LoadOrder order, SSEPlugin parent)
		{
            var master = parent.GetMasterName((int)MasterIndex);
            var masterIndex = (UInt32)order.plugins.FindIndex(plugin =>
                string.Equals(master, plugin, StringComparison.OrdinalIgnoreCase));

            return new ResolvedFormID(RecordID, masterIndex);
		}
	}
    public readonly struct ResolvedFormID
    {
        public readonly UInt32 recordId;
        public readonly UInt32 masterIndex;
        public ResolvedFormID(UInt32 recordId, UInt32 masterIndex) =>
            (this.recordId, this.masterIndex) = (recordId, masterIndex);
    }

    /// <summary>
    /// A ulong used to identify a data object.
    /// May refer to a data object from a mod or new object created in-game.
    /// </summary>
    public struct FormID
    {
        public sbyte MasterIndex => (sbyte)(id >> (8 * 3));

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

        /// <summary>
        /// Use the Load Order to find the canonical FormID
        /// </summary>
        /// <param name="order"></param>
        /// <param name="plugin"></param>
        /// <returns></returns>
        public UInt32 ResolveFormID(LoadOrder order, SSEPlugin plugin)
		{
            // If the "master index" is not in the "master range", then this
            // is an "original" record
            if (MasterIndex >= plugin.pluginRecord.mast.Count)
                return id;
            var masterPluginName = plugin.pluginRecord.mast[MasterIndex].content;

            sbyte masterIndex = (sbyte)order
                .plugins
                .FindIndex(pluginName =>
                    String.Equals(pluginName, masterPluginName, StringComparison.OrdinalIgnoreCase));

            return (id & 0x00FFFFFF) | (UInt32)(masterIndex << (8 * 3));

        }
    }
}
