using System;
using System.Text;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace SSE
{
	public partial class Plugin
	{
		/// <summary>
		/// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format#Groups
		/// </summary>
		public readonly struct Group
		{
			public static uint fixedSize = 20 + VersionControlInfo.Size;
			public static string type = "GRUP";
			public uint DataSize => groupSize - fixedSize;
			public string RecordType { get { return Encoding.UTF8.GetString(label); } }

			public readonly UInt32 groupSize;
			/// <summary>
			/// Label means different things based on group type,
			/// so we store it as a byte array until we inspect the group type
			/// </summary>
			public readonly Byte[] label;
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
			public readonly GroupType groupType; // 16
			public readonly VersionControlInfo versionControlInfo;
			public readonly UInt32 unknown;

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

			public Group(Byte[] bytes, int offset)
			{
				// first 4 bytes are the type, which should be "GRUP"
				groupSize = BitConverter.ToUInt32(bytes, offset + 4);
				label = bytes[(offset + 8)..(offset + 12)];
				groupType = (GroupType)BitConverter.ToInt32(bytes, offset + 12);
				versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
				unknown = BitConverter.ToUInt32(bytes, offset + 20);
			}
		}
	}
}