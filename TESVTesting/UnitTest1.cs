using NUnit.Framework;
using SSE.TESVArchive;
using System.IO;
using System.Threading.Tasks;

namespace TESVTesting
{
	public class Tests
	{
		[Test]
		public async Task Test1()
		{
			var archivePath = @"C:\Program Files (x86)\Steam\steamapps\common\Skyrim Special Edition\Data\Skyrim - Meshes0.bsa";
			var archiveFile = new FileInfo(archivePath);
			using var archiveStream = archiveFile.OpenRead();
			var headerBytes = new byte[BSA105Header.HeaderSize];
			await archiveStream.ReadAsync(headerBytes);
			var header = BSA105Header.Parse(headerBytes, 0);

			Assert.Pass();
		}
	}
}