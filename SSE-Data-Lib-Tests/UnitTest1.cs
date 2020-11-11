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
			using var stream = File.OpenRead("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data\\Skyrim - Interface.bsa");
			await using var archive = await SSE.Archive105.Open(stream);

			Assert.AreEqual("BSA\0", archive.fileId);
		}
    }
}
