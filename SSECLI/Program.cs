using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SSE.TESVPlugin;
using SSE.TESVPlugin.Reader;
using SSE.TESVRecord;

namespace SSECLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var path = @"C:\Program Files (x86)\Steam\steamapps\common\Skyrim Special Edition\Data\Skyrim.esm";
                var file = new System.IO.FileInfo(path);
                using var stream = file.OpenRead();
                var parser = await PluginStreamReader.LoadStream(stream);

                var (cell, cellGroup) = await parser.ReadCell(0x00050F1Eu);
                var persisted = cellGroup.Groups.Find(g => g.Header.GroupType == GroupHeader.GroupTypes.CellPersistentChildren);
                var temporary = cellGroup.Groups.Find(g => g.Header.GroupType == GroupHeader.GroupTypes.CellTemporaryChildren);

                var childrenReferences = persisted.Records
                    .Concat(temporary.Records)
                    .Where(r => r.Header.Type == "REFR")
                    .Select(r => new REFRRecord(r))
                    .ToList();
                var children = new List<Record>();
                foreach (var refRecord in childrenReferences)
				{
                    children.Add(await parser.ReadRecordFromFormID(refRecord.Name.formId));
                }
                foreach (var child in children) {
                    var edidIndex = child.Fields.FindIndex(f => f.Type == "EDID");
                    if (edidIndex != -1)
                    {
                        Console.WriteLine($"{child.Header.Id}: {child.Header.Type} ({Encoding.UTF8.GetString(child.Fields[edidIndex].DataBytes)})");
                    } else
					{
                        Console.WriteLine($"{child.Header.Id}: {child.Header.Type}");
                    }

                }
                //var result = await parser.ReadRecordFromFormID(793987);

                /*
                var options = new System.Text.Json.JsonSerializerOptions();
                options.WriteIndented = true;
                options.IgnoreReadOnlyProperties = false;
                var json = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(result, options);
                using var outFile = new System.IO.FileInfo(@"C:\Users\lukek\Desktop\skyrim.json").Open(
                    System.IO.FileMode.Create,
                    System.IO.FileAccess.Write
                );
                await outFile.WriteAsync(json, 0, json.Length);
                */
                Console.WriteLine("DONE");
            } catch (Exception error)
            {
                Console.WriteLine("There was an unexpected error");
                Console.WriteLine(error.Message);
                Console.WriteLine(error.StackTrace);
                return;
            }
        }
    }
}
