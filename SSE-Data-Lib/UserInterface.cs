using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	public class UserInterface
	{
		public enum Language
		{
			English
		};
		Dictionary<int, StringTable> stringTablesByPluginIndex;

		public async Task<UserInterface> Load(Language language, Data data)
		{
			throw new NotImplementedException();
		}

		public UserInterface(Language language)
		{
			throw new NotImplementedException();
		}

		public string ResolveLocalizedString(LString localizedString)
		{
			throw new NotImplementedException();
			if (!localizedString.isLocalizd)
				return localizedString.content.content;

		}
	}
}
