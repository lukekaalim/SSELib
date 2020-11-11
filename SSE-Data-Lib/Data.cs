using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	public struct Data: IAsyncDisposable
	{
		public async Task<Data> Open(string directory)
		{
			return new Data();
		}

		public ValueTask DisposeAsync()
		{
			throw new NotImplementedException();
		}
	}
}
