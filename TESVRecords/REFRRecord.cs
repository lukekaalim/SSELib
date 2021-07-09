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
		public struct DATAField
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

				return new DATAField()
				{
					Position = position
				};
			}
		}

		public ZString? EditorID { get; set; }
		public LocalFormID Name { get; set; }
		public DATAField Data { get; set; }

		public REFRRecord(Record record)
		{
			var fields = record.Fields.ToDictionary(f => f.Type, f => f.DataBytes);
			EditorID = fields.ContainsKey("EDID") ? (ZString?)ZString.Parse(fields["EDID"]) : null;
			Name = new LocalFormID(fields["NAME"], 0);
			Data = DATAField.Parse(fields["DATA"]);
		}
	}
}
