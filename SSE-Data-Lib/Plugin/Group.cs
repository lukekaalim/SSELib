using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Plugin
{
	public struct IntVector2
	{
		public int X { get; set; }
		public int Y { get; set; }

		public IntVector2(int X, int Y)
		{
			this.X = X;
			this.Y = Y;
		}
	}


	public struct Group
	{
		public struct GroupHeader
		{
			public static uint HeaderSize = 24;
			public static string type = "GRUP";
			public uint DataSize => GroupSize - HeaderSize;

			public string RecordType => Encoding.UTF8.GetString(label);
			public int BlockNumber => BitConverter.ToInt32(label);
			public int SubBlockNumber => BitConverter.ToInt32(label);
			public IntVector2 Grid => new IntVector2(BitConverter.ToInt16(label), BitConverter.ToInt16(label, 2));

			public uint GroupSize { get; set; }
			public byte[] label;
			public enum GroupTypes : int
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
			public GroupTypes GroupType { get; set; } // 16
			public string GroupTypeName => GroupType.ToString();
			//public VersionControlInfo versionControlInfo;
			public uint unknown;
			public static async Task<GroupHeader> Read(Stream stream)
			{
				var headerBytes = new byte[HeaderSize];
				await stream.ReadAsync(headerBytes);
				return Parse(headerBytes);
			}

			public static GroupHeader Parse(byte[] bytes, long offset = 0)
			{
				// first 4 bytes are the type, which should be "GRUP"
				var groupSize = BitConverter.ToUInt32(bytes, (int)(offset + 4));
				var label = bytes[(Index)(offset + 8)..(Index)(offset + 12)];
				var groupType = (GroupTypes)BitConverter.ToInt32(bytes, (int)(offset + 12));
				//var versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
				var unknown = BitConverter.ToUInt32(bytes, (int)(offset + 20));

				return new GroupHeader()
				{
					GroupSize = groupSize,
					label = label,
					GroupType = groupType,
					//versionControlInfo = versionControlInfo,
					unknown = unknown
				};
			}
		}

		public GroupHeader Header { get; set; }
		public List<Record> Records { get; set; }
		public List<Group> Groups { get; set; }

		public static Group Parse(byte[] groupBytes, long offset = 0)
		{
			var header = GroupHeader.Parse(groupBytes, offset);
			var (records, groups) = Record.ParseAll(groupBytes, GroupHeader.HeaderSize + offset, (int)header.DataSize);

			return new Group() {
				Header = header,
				Records = records,
				Groups = groups
			};
		}
	}
}
