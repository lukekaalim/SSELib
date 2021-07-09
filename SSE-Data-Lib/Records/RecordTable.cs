using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.Records
{
	public class RecordTable
	{
		List<SSEPlugin> plugins;

		public RecordTable(List<SSEPlugin> plugins)
		{
			this.plugins = plugins;
		}
	}
}
