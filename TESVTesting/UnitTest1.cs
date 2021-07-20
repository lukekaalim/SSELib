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
using SSE.TESVNif.BlockStructure.Logic;
using SSE.TESVNif.BlockStructure;

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

        public class BlockConverter : JsonConverter<Block>
        {
            public override Block Read(ref Utf8JsonReader reader, System.Type typeToConvert, JsonSerializerOptions options)
            {
                throw new System.NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, Block value, JsonSerializerOptions options)
            {
				switch (value)
                {
					case ListBlock list:
						writer.WriteStartArray();
						foreach (var element in list.Contents)
							Write(writer, element, options);
						writer.WriteEndArray();
						break;
					case BasicBlock basic:
						writer.WriteStringValue(basic.Value.ToString());
						break;
					case CompoundBlock compound:
						switch (compound.Name)
                        {
							case "ExportString":
							case "SizedString":
								writer.WriteStringValue(NIFReader.ReadSizedString(compound));
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
					case EnumBlock @enum:
						writer.WriteStringValue(@enum.Value.ToString());
						break;
					case NiObjectBlock niObject:
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

        [Test]
		public async Task Test2()
		{
			Console.WriteLine(DateTime.Now);
			for (int i = 0; i < 100; i++)
				await NIFReader.Read("/Users/lukekaalim/projects/SSE-Data-Lib/axe01.nif");
			Console.WriteLine(DateTime.Now);
			/*
			var output = new FileInfo("/Users/lukekaalim/projects/SSE-Data-Lib/axe01.json");
			using var outputStream = output.Open(FileMode.Create, FileAccess.Write);
			var options = new JsonSerializerOptions();
			options.Converters.Add(new BlockConverter());
			options.WriteIndented = true;
			await JsonSerializer.SerializeAsync(outputStream, file, options);
			*/
		}

		[Test]
		public void Test3()
		{
			var source = "(Version == 10.0.1.2) || (Version == 20.2.0.7)";
			var state = new Interpreter.State()
			{
				Identifiers = new Dictionary<string, Value>()
				{
					{ "Version", Value.From(SSE.TESVNif.BlockStructure.VersionParser.Parse("10.0.1.2")) },
				}
			};
			var tokens = Lexer.ReadSource(source);
			var expression = Parser.Parse(tokens);
			var result = Interpreter.Interpret(expression, state);

			Assert.IsTrue(result.AsBoolean);
		}
	}
}