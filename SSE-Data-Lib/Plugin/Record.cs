using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace SSE.Plugin
{

    public struct Record
    {
        public struct RecordHeader
        {
            public static int HeaderSize = 24;
            public long RecordSize => HeaderSize + DataSize;

            public string Type { get; set; }
            public uint DataSize { get; set; }

            /// <summary>
            /// Flags means different things depending on the record type
            /// </summary>
            [Flags]
            public enum RecordFlags : uint
            {
                Master = 0x00000001,
                DeletedGroup = 0x00000010,
                DeletedRecord = 0x00000020,
                Constant = 0x00000040,
                Localized = 0x00000080,
                MustUpdateAnim = 0x00000100,
                Light = 0x00000200,
                Quest = 0x00000400,
                InitiallyDisabled = 0x00000800,
                Ignored = 0x00001000,
                VisibleWhenDistant = 0x00008000,
                RandomAnimationStart = 0x00010000,
                Dangrous = 0x00020000,
                Compressed = 0x00040000,
                CantWait = 0x00080000,
                IgnoreObjectInteraction = 0x00100000,
                IsMarker = 0x00800000,
                Obstacle = 0x02000000,
                Filter = 0x04000000,
                BoundingBox = 0x08000000,
                MustExitToTalk = 0x10000000,
                ChildCanUse = 0x20000000,
                Ground = 0x40000000,
                MultiBound = 0x80000000,
            }
            public RecordFlags flags;
            public LocalFormID Id { get; set; }
            //public VersionControlInfo versionControlInfo;
            public UInt16 version;
            //public UInt16 unknown;

            public static RecordHeader Parse(byte[] bytes, int offset = 0)
			{
                var type = Encoding.UTF8.GetString(bytes, offset + 0, 4);
                var dataSize = BitConverter.ToUInt32(bytes, offset + 4);
                var flags = (RecordFlags)BitConverter.ToUInt32(bytes, offset + 8);
                var id = new LocalFormID(bytes, offset + 12);
                //versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
                var version = BitConverter.ToUInt16(bytes, offset + 20);
               // var unknown = BitConverter.ToUInt16(bytes, offset + 22);

                return new RecordHeader()
                {
                    Type = type,
                    DataSize = dataSize,
                    flags = flags,
                    Id = id,
                    version = version,
                };
            }

            public static async Task<RecordHeader> Read(Stream stream)
			{
                byte[] bytes = new byte[HeaderSize];
                await stream.ReadAsync(bytes);
                return Parse(bytes);
			}
        }

        public RecordHeader Header { get; set; }
        public List<Field> Fields { get; set; }

        public static Record Parse(byte[] bytes, int offset = 0)
		{
            var header = RecordHeader.Parse(bytes, offset);
            var fields = (header.flags & RecordHeader.RecordFlags.Compressed) == RecordHeader.RecordFlags.Compressed ?
                new List<Field>() :
                Field.ParseAll(bytes, offset + RecordHeader.HeaderSize, (int)header.DataSize);
            return new Record() { Fields = fields, Header = header };
		}

        public static (List<Record>, List<Group>) ParseAll(byte[] bytes, long offset, int bytesToParse)
        {
            var records = new List<Record>();
            var groups = new List<Group>();
            var index = offset;
            while (index < offset + bytesToParse)
            {
                string type = Encoding.UTF8.GetString(bytes, (int)(index), 4);
                if (type == "GRUP")
                {
                    groups.Add(Group.Parse(bytes, index));
                    index += groups.Last().Header.GroupSize;
                }
                else
                {
                    records.Add(Parse(bytes, (int)index));
                    index += records.Last().Header.RecordSize;
                }
            }
            return (records, groups);
        }
    }
}
