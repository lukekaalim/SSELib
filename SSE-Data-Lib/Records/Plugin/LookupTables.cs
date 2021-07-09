using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace SSE
{
	public partial class SSEPlugin
	{
		/// <summary>
		/// Accelerate the lookup of GRUP records
		/// </summary>
		public class GroupLookupTable
		{
			long groupStartOffset;
			List<Group> groups;

			Dictionary<Index, int> offsetByIndex;
			Dictionary<string, Index> indexByRecordType;

			public GroupLookupTable(List<Group> groups, long groupStartOffset)
			{
				this.groupStartOffset = groupStartOffset;
				this.groups = groups;

				offsetByIndex = new Dictionary<Index, int>();
				indexByRecordType = new Dictionary<string, Index>();

				for (int i = 0; i < groups.Count; i++)
				{
					if (i == 0)
						offsetByIndex.Add(i, 0);
					else
						offsetByIndex.Add(i, (int)(offsetByIndex[i - 1] + groups[i - 1].groupSize));

					if (!indexByRecordType.ContainsKey(groups[i].RecordType))
						indexByRecordType.Add(groups[i].RecordType, i);
				}
			}

			public (int, Group?) GetLookupReference(string type)
			{
				if (!indexByRecordType.ContainsKey(type))
					return (0, null);
				var index = indexByRecordType[type];
				var offset = (int)(offsetByIndex[index] + groupStartOffset + Group.fixedSize);
				var group = groups[index];
				return (offset, group);
			}
		}

		/// <summary>
		/// Acceleration structure that assists in finding Fields of certain types
		/// </summary>
		public class FieldLookupTable
		{
			public Dictionary<string, List<int>> dataByType;
			public Record parentRecord;
			public FieldLookupTable(Record record)
			{
				parentRecord = record;
				dataByType = new Dictionary<string, List<int>>();
				for (int i = 0; i < record.data.Count; i++)
				{
					List<int> list;
					var field = record.data[i];

					if (!dataByType.TryGetValue(field.type, out list))
					{
						list = new List<int>();
						dataByType.Add(record.type, list);
					}

					list.Add(i);
				}
			}
			public T GetField<T>(string fieldType, Func<Byte[], T> cast)
			{
				var index = dataByType[fieldType][0];
				var field = parentRecord.data[index];
				return cast(field.data);
			}
			public List<T> GetFields<T>(string fieldType, Func<Byte[], T> cast)
			{
				var indicies = dataByType[fieldType];
				var fields = indicies.Select(index => parentRecord.data[index]);

				return fields
					.Select(field => cast(field.data))
					.ToList();
			}
		}
	}
}
