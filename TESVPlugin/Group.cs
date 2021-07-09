using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.TESVPlugin
{
	public struct GroupHeader
	{
		public static uint HeaderSize = 24;
		public static string type = "GRUP";
		public uint DataSize => GroupSize - HeaderSize;
		public uint GroupSize { get; set; }
		public int Label { get; set; }
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
		// public VersionControlInfo versionControlInfo;
		// public uint unknown;

		public static GroupHeader Parse(byte[] bytes, long offset = 0)
		{
			var groupSize = BitConverter.ToUInt32(bytes, (int)(offset + 4));
			var label = BitConverter.ToInt32(bytes, (int)offset + 8);
			var groupType = (GroupTypes)BitConverter.ToInt32(bytes, (int)(offset + 12));
			// var versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
			// var unknown = BitConverter.ToUInt32(bytes, (int)(offset + 20));

			return new GroupHeader()
			{
				GroupSize = groupSize,
				Label = label,
				GroupType = groupType,
				// versionControlInfo = versionControlInfo,
				// unknown = unknown
			};
		}
	}



	public struct Group
	{
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
