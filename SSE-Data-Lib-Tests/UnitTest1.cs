using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SSE_Data_Lib_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestPlugin()
        {
            SSE.Plugin plugin = await SSE.Plugin.Load("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data\\Skyrim.esm");
            Assert.AreEqual(plugin.pluginInformation.cnam.content, "mcarofano");
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
