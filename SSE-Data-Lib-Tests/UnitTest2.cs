using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE_Data_Lib_Tests
{
	[TestClass]
	public class UnitTest2
	{
		[TestMethod]
		public async Task TestMedthod()
		{
			var data = await SSE.Data.Open(
				new System.IO.DirectoryInfo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data"),
				new System.IO.DirectoryInfo("C:\\Users\\lukek\\AppData\\Local\\Skyrim Special Edition")
			);
			var weaps = await data.GetAllRecordsOfType("WEAP");
			var weapRecs = weaps.Values
				.Select(r => (r.Item3.pluginFile, SSE.WEAPRecord.From(r.Item2, r.Item3.pluginRecord)))
				.Select(w => w.Item2.full.GetResolvedContent(data.stringTables[Path.GetFileNameWithoutExtension(w.pluginFile.Name)]))
				.ToList();
		}
	}
}
