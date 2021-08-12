using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using SSE.TESVPlugin;
using SSE.TESVRecord.DataTypes;

namespace SSE.TESVRecord
{
	public class REFRRecord
	{
		public class DATAField
		{
			public Vector3 Position { get; set; }
			public Vector3 Rotation { get; set; }

			public static DATAField Parse(byte[] fieldBytes)
			{
				var position = new Vector3(
					BitConverter.ToSingle(fieldBytes, 0),
					BitConverter.ToSingle(fieldBytes, 4),
					BitConverter.ToSingle(fieldBytes, 8)
				);
				var rotation = new Vector3(
					BitConverter.ToSingle(fieldBytes, 12),
					BitConverter.ToSingle(fieldBytes, 16),
					BitConverter.ToSingle(fieldBytes, 20)
				);

				return new DATAField()
				{
					Position = position,
					Rotation = rotation
				};
			}
		}

		public class XMBOField
		{
			public Vector3 Extents { get; set; }

			public static XMBOField Parse(byte[] fieldBytes)
			{
				var extents = new Vector3(
					BitConverter.ToSingle(fieldBytes, 0),
					BitConverter.ToSingle(fieldBytes, 4),
					BitConverter.ToSingle(fieldBytes, 8)
				);

				return new XMBOField()
				{
					Extents = extents
				};
			}
		}

		public ZString? EditorID { get; set; }
		public LocalFormID Name { get; set; }
		public DATAField Data { get; set; }
		public XMBOField Bounds { get; set; }

		public Record baseRecord;

		public REFRRecord(Record record)
		{
			baseRecord = record;

			var fields = record.Fields
				.GroupBy(f => f.Type)
				.ToDictionary(f => f.Key, f => f.First().DataBytes);
			EditorID = fields.ContainsKey("EDID") ? (ZString?)ZString.Parse(fields["EDID"]) : null;
			Name = new LocalFormID(fields["NAME"], 0);
			Data = fields.ContainsKey("DATA") ? DATAField.Parse(fields["DATA"]) : null;
			Bounds = fields.ContainsKey("XMBO") ? XMBOField.Parse(fields["XMBO"]) : null;
		}
	}
}
