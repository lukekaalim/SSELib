using NUnit.Framework;
using System;
using SSE.TESVArchive;
using SSE.TESVNif;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TESVTesting
{
	public class Tests
	{
		/*
		[Test]
		public async Task Test1()
		{
			var archivePath = @"/Users/lukekaalim/projects/SSE-Data-Lib/TestData/Skyrim - Meshes0.bsa";
			var archiveFile = new FileInfo(archivePath);
			using var archiveStream = archiveFile.OpenRead();

			var reader = await ArchiveStreamReader.Load(archiveStream);

			foreach (var (path, record) in reader.RecordsByPath.Take(0))
			{
				var file = await reader.ReadFile(record);
				var localFile = path.Replace('\\', Path.DirectorySeparatorChar);
				var output = new FileInfo(Path.Combine("/Users/lukekaalim/projects/SSE-Data-Lib/unzippedStuff", localFile));
				Directory.CreateDirectory(output.DirectoryName);
				using var stream = output.OpenWrite();
				await stream.WriteAsync(file);
			}
		}
		*/
		/*
        public class BlockConverter : JsonConverter<Data>
        {
            public override Data Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
            {
                throw new System.NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, Data value, JsonSerializerOptions options)
            {
				switch (value)
                {
					case ListData list:
						writer.WriteStartArray();
						foreach (var element in list.Contents)
							Write(writer, element, options);
						writer.WriteEndArray();
						break;
					case BasicData basic:
						writer.WriteStringValue(basic.Value.ToString());
						break;
					case CompoundData compound:
						switch (compound.Name)
                        {
							case "ExportString":
							case "SizedString":
								writer.WriteStringValue(SSE.TESVNif.Structures.CharList.ReadString(compound));
								return;
                        }
						writer.WriteStartObject();
						foreach (var field in compound.Fields)
                        {
							writer.WritePropertyName(field.Key);
							Write(writer, field.Value, options);
                        }
						writer.WriteEndObject();
						break;
					case EnumData @enum:
						writer.WriteStringValue(@enum.Value.ToString());
						break;
					case BlockData niObject:
						writer.WriteStartObject();
						foreach (var field in niObject.Fields)
						{
							writer.WritePropertyName(field.Key);
							Write(writer, field.Value, options);
						}
						writer.WriteEndObject();
						break;
				}
            }
        }
		*/
		/*
        [Test]
		public void Test2()
		{
			var axePath = "/Users/lukekaalim/projects/SSE-Data-Lib/TestData/axe01.nif";
			var rankaPath = "/Users/lukekaalim/projects/SSE-Data-Lib/TestData/Ranka Axe-446-1-0-1-1/Data/meshes/weapons/ranka/ranka.nif";
			var macePath = "/Users/lukekaalim/projects/SSE-Data-Lib/TestData/W_art_Ice_mace.NIF";
			using var axeStream = new FileInfo(axePath).OpenRead();
			using var rankaStream = new FileInfo(rankaPath).OpenRead();
			using var maceStream = new FileInfo(macePath).OpenRead();

			var reader = new NIFStreamReader()
			{
				Schema = NifSchema.LoadEmbedded(),
				Stream = axeStream,
			};

			var axe = reader.ReadFile();
			reader.Stream = rankaStream;
			var ranka = reader.ReadFile();
			reader.Stream = maceStream;
			var mace = reader.ReadFile();
		}
		*/
	}
}