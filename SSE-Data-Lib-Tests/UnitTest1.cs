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
            SSEData.Plugin plugin = await SSEData.Plugin.Load("/Users/lukekaalim/Downloads/Ranka Axe-446-1-0-1/Data/Ranka.esp");
            Assert.AreEqual(plugin.pluginInformation.cnam.content, "visorium");
        }
    }
}
