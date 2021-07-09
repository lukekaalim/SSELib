using System;
using System.Text;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace SSE
{
	public partial class SSEPlugin
	{
		public struct GroupHeader
		{
			public static uint headerSize = 20 + VersionControlInfo.Size;
			public static string type = "GRUP";
			public uint DataSize => groupSize - headerSize;
			public uint groupSize;
			public byte[] label;
			public enum GroupType : int
			{
				RecordType = 0,
				WorldChildren = 1,
				InteriorCellBlock = 2,
				InteriorCellSubBlock = 3,
				ExteriorCellBlock = 4,
				ExteriorCellSubBlock = 5,
				CellChildren = 6,
				TopicChildren = 7,
				CellPersistentChildren = 8,
				CellTemporaryChildren = 9
			}
			public GroupType groupType; // 16
			public VersionControlInfo versionControlInfo;
			public uint unknown;

			public static GroupHeader Parse(Byte[] bytes, int offset)
			{
				// first 4 bytes are the type, which should be "GRUP"
				var groupSize = BitConverter.ToUInt32(bytes, offset + 4);
				var label = bytes[(offset + 8)..(offset + 12)];
				var groupType = (GroupType)BitConverter.ToInt32(bytes, offset + 12);
				var versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
				var unknown = BitConverter.ToUInt32(bytes, offset + 20);

				return new GroupHeader()
				{
					groupSize = groupSize,
					label = label,
					groupType = groupType,
					versionControlInfo = versionControlInfo,
					unknown = unknown
				};
			}
		}

		/// <summary>
		/// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format#Groups
		/// </summary>
		public struct Group
		{
			public static uint fixedSize = 20 + VersionControlInfo.Size;
			public static string type = "GRUP";
			public uint DataSize => groupSize - fixedSize;
			public string RecordType { get { return Encoding.UTF8.GetString(label); } }

			public UInt32 groupSize;
			/// <summary>
			/// Label means different things based on group type,
			/// so we store it as a byte array until we inspect the group type
			/// </summary>
			public Byte[] label;
			public enum GroupType: Int32
			{
				RecordType = 0,
				WorldChildren = 1,
				InteriorCellBlock = 2,
				InteriorCellSubBlock = 3,
				ExteriorCellBlock = 4,
				ExteriorCellSubBlock = 5,
				CellChildren = 6,
				TopicChildren = 7,
				CellPersistentChildren = 8,
				CellTemporaryChildren = 9
			}
			public GroupType groupType; // 16
			public VersionControlInfo versionControlInfo;
			public UInt32 unknown;

			public static async Task<List<Group>> ReadAllTopLevelGroups(Stream stream, TES4Record pluginRecord)
			{
				List<Group> groups = new List<Group>();
				// read until the end of the stream
				while (stream.Position < stream.Length)
				{
					Byte[] bytes = new byte[fixedSize];
					await stream.ReadAsync(bytes, 0, (int)fixedSize);

					var group = new Group(bytes, 0);
					groups.Add(group);

					stream.Seek(group.groupSize - fixedSize, SeekOrigin.Current);
				}
				return groups;
			}

			public static async Task<Group> Read(Stream stream)
			{
				Byte[] headerBytes = new byte[fixedSize];
				await stream.ReadAsync(headerBytes);
				var group = new Group(headerBytes, 0);
				stream.Seek(group.DataSize, SeekOrigin.Current);
				return group;
			}
			public static Group Parse(Byte[] bytes, int offset)
			{
				BitConverter.GetBytes(10);
				// first 4 bytes are the type, which should be "GRUP"
				var groupSize = BitConverter.ToUInt32(bytes, offset + 4);
				var label = bytes[(offset + 8)..(offset + 12)];
				var groupType = (GroupType)BitConverter.ToInt32(bytes, offset + 12);
				var versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
				var unknown = BitConverter.ToUInt32(bytes, offset + 20);

				return new Group() {
					groupSize = groupSize,
					label = label,
					groupType = groupType,
					versionControlInfo = versionControlInfo,
					unknown = unknown
				};
			}

			public Group(Byte[] bytes, int offset)
			{
				BitConverter.GetBytes(10);
				// first 4 bytes are the type, which should be "GRUP"
				groupSize = BitConverter.ToUInt32(bytes, offset + 4);
				label = bytes[(offset + 8)..(offset + 12)];
				groupType = (GroupType)BitConverter.ToInt32(bytes, offset + 12);
				versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
				unknown = BitConverter.ToUInt32(bytes, offset + 20);
			}
		}

		public struct GroupReference
		{
			public Group group;
			/// <summary>
			/// Offset in bytes from the start of this file to the start of the group
			/// </summary>
			public uint startOffset;


			/// <summary>
			/// Size of the GRUP header declaration
			/// </summary>
			public static uint headerSize = 24;

			/// <summary>
			/// Total size of all records in this top level group
			/// </summary>
			public uint recordsSize => group.DataSize;

			/// <summary>
			/// The offset of the next byte that is not part of this group
			/// </summary>
			public uint endOffset => startOffset + group.groupSize;
		}

		public struct GroupContents
		{
			public List<Record> records;
			public List<Group> groups;
		}
	}
}