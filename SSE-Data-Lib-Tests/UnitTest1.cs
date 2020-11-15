using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using SSE;

namespace SSE_Data_Lib_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestPlugin()
        {
            SSE.Plugin plugin = await SSE.Plugin.Load(new FileInfo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data\\Skyrim.esm"));
            List<SSE.WEAPRecord> weaponRecords = await plugin
                .EnumerateGroupRecords("WEAP")
                .Select(r => WEAPRecord.From(r, plugin.pluginRecord))
                .ToListAsync();
            Assert.AreEqual(weaponRecords.Count > 0, true);
            Assert.AreEqual(plugin.pluginRecord.cnam.content, "mcarofano");
        }
        [TestMethod]
        public async Task TestPlugin2()
        {
            SSE.Plugin plugin = await SSE.Plugin.Load(new FileInfo("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data\\Skyrim.esm"));
            List<SSE.WEAPRecord> weaponRecords = (await plugin.ReadGroupRecordsAsync("WEAP"))
                .Select(r => WEAPRecord.From(r, plugin.pluginRecord))
                .ToList();

            Assert.AreEqual(weaponRecords.Count > 0, true);
            Assert.AreEqual(plugin.pluginRecord.cnam.content, "mcarofano");
        }
        [TestMethod]
        public async Task TestArchive()
        {
			var archive = await SSE.Archive.Open("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data\\Skyrim - Interface.bsa");
            var dlstrings = await archive.Read("strings\\skyrim_english.dlstrings");
            var ilstrings = await archive.Read("strings\\skyrim_english.ilstrings");
            var strings = await archive.Read("strings\\skyrim_english.strings");

            var table = SSE.StringTable.ParseNullTerminated(strings);
            Assert.AreEqual("BSA\0", archive.header.fileId);
		}
    }
}
