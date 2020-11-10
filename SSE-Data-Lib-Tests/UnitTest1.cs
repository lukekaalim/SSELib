using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SSE_Data_Lib_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod()
        {
            SSEData.Plugin plugin = await SSEData.Plugin.Load("/Users/lukekaalim/Downloads/data/Immersive Weapons.esp");
            Assert.AreEqual(plugin.pluginInformation.cnam.content, "visorium");
        }
    }
}
