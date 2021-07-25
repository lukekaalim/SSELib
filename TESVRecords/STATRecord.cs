using System;
using System.Numerics;
using System.Linq;
using System.Collections.Generic;
using SSE.TESVPlugin;
using SSE.TESVRecord.DataTypes;

namespace SSE.TESVRecord
{
    public class STATRecord
	{
		public struct MODLField
		{
			public ZString Model { get; set; }

			public static MODLField Parse(byte[] fieldBytes)
			{
				var model = ZString.Parse(fieldBytes);

				return new MODLField()
				{
					Model = model
				};
			}
		}

		public MODLField Model { get; set; }

		public STATRecord(Record record)
		{
			var fields = record.Fields.ToDictionary(f => f.Type, f => f.DataBytes);

			Model = MODLField.Parse(fields["MODL"]);
		}
	}
}
