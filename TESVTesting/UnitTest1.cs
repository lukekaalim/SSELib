using NUnit.Framework;
using SSE.TESVArchive;
using SSE.TESVNif;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TESVTesting
{
	public class Tests
	{
		[Test]
		public async Task Test1()
		{
			var archivePath = @"/Users/lukekaalim/projects/SSE-Data-Lib/TestData/Skyrim - Meshes0.bsa";
			var archiveFile = new FileInfo(archivePath);
			using var archiveStream = archiveFile.OpenRead();

			var reader = await ArchiveStreamReader.Load(archiveStream);
			var (path, record) = reader.RecordsByPath.First();
			var file = await reader.ReadFile(record);

			var output = new FileInfo(Path.Combine("/Users/lukekaalim/projects/SSE-Data-Lib/", path));

			using var stream = output.OpenWrite();
			await stream.WriteAsync(file);

			Assert.Pass();
		}

		[Test]
		public async Task Test2()
		{
			NIFReader.Read("");
		}
	}
}