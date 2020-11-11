using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSE
{
    public struct VersionControlInfo
    {
        public Byte date;
        public Byte month;
        public Byte lastUserId;
        public Byte currentUserId;

        public static VersionControlInfo Parse(Byte[] bytes, int offset)
        {
            return new VersionControlInfo()
            {
                date = bytes[offset],
                month = bytes[offset + 1],
                lastUserId = bytes[offset + 2],
                currentUserId = bytes[offset + 3],
            };
        }
    }

    /// <summary>
    /// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format#Groups
    /// </summary>
    public struct Group
    {
        public string type;
        public UInt32 groupSize;
        /// <summary>
        /// Label means different things based on group type,
        /// so we store it as a byte array until we inspect the group type
        /// </summary>
        public Byte[] label;
        public Int32 groupType;
        public VersionControlInfo versionControlInfo;
        public UInt32 unknown;
        public List<Record> records;
        public List<Group> groups;

        public static Group ParseFirst(Byte[] bytes, int offset = 0)
        {
            var groupSize = BitConverter.ToUInt32(bytes, offset + 4);
            var label = new Byte[4];
            Buffer.BlockCopy(bytes, offset + 8, label, 0, 4);
            (List<Record> records, List<Group> groups) = ParseAllRecordsAndGroups(bytes, offset + 24, (int)groupSize - 24);

            return new Group()
            {
                type = Encoding.UTF8.GetString(bytes, offset, 4),
                groupSize = groupSize,
                label = label,
                groupType = BitConverter.ToInt32(bytes, offset + 12),
                versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16),
                unknown = BitConverter.ToUInt32(bytes, offset + 20),
                records = records,
                groups = groups,
            };
        }

        public static (List<Record>, List<Group>) ParseAllRecordsAndGroups(Byte[] bytes, int offset, int length) {
            var records = new List<Record>();
            var groups = new List<Group>();
            int index = offset;

            while (index < offset + length)
            {
                var type = Encoding.UTF8.GetString(bytes, index, 4);
                switch (type) {
                    case "GRUP": {
                        var group = Group.ParseFirst(bytes, index);
                        index += (int)group.groupSize;
                        groups.Add(group);
                        break;
                    }
                    default: {
                        var record = Record.ParseFirst(bytes, index);
                        index += (int)record.dataSize + 24;
                        records.Add(record);
                        break;
                    }
                }
            }

            return (records, groups);
        }

        public static List<Group> ParseAll(Byte[] bytes, int offset, int length)
        {
            var groups = new List<Group>();
            int index = offset;
            while (index < offset + length)
            {
                var group = Group.ParseFirst(bytes, index);
                index += (int)group.groupSize;
                groups.Add(group);
            }
            return groups;
        }
    }

    /// <summary>
    /// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format#Records
    /// </summary>
    public struct Record
    {
        // We use a lot of "specific" data types here to match
        // The "record" format, which is a binary format.
        public string type; // 4 bytes
        public UInt32 dataSize;
        /// <summary>
        /// Flags means different things depending on the record type
        /// </summary>
        public UInt32 flags;
        public FormID id;
        public VersionControlInfo versionControlInfo;
        public UInt32 unknown;
        public List<Field> data;

        public static Record ParseFirst(Byte[] bytes, int offset = 0)
        {
            var dataSize = BitConverter.ToUInt32(bytes, offset + 4);
            var type = Encoding.UTF8.GetString(bytes, offset, 4);
            return new Record() {
                type = type,
                dataSize = dataSize,
                flags = BitConverter.ToUInt32(bytes, offset + 8),
                id =  FormID.From(bytes, offset + 12),
                versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16),
                unknown = BitConverter.ToUInt32(bytes, offset + 20),
                data = Field.ParseAll(bytes, offset + 24, Convert.ToInt32(dataSize))
            };
        }

        public static List<Record> ParseAll(Byte[] bytes, int offset, int length)
        {
            var records = new List<Record>();
            int index = offset;
            while (index < offset + length)
            {
                var record = Record.ParseFirst(bytes, index);
                index += (int)record.dataSize + 24;
                records.Add(record);
            }
            return records;
        }
    }

    /// <summary>
    /// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format#Fields
    /// </summary>
    public struct Field
    {
        public string type; // 4 bytes
        public UInt16 dataSize;
        public Byte[] data;

        public static Field ParseFirst(Byte[] bytes, int offset)
        {
            var type = Encoding.UTF8.GetString(bytes, offset, 4);
            var dataSize = BitConverter.ToUInt16(bytes, offset + 4);
            var data = new byte[dataSize];
            Buffer.BlockCopy(bytes, offset + 6, data, 0, Convert.ToInt32(dataSize));

            return new Field()
            {
                type = type,
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
                var field = Field.ParseFirst(bytes, index);
                index += field.dataSize + (4 + 2);
                fields.Add(field);
            }
            return fields;
        }
    }
}
