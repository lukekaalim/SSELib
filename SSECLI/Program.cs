using System;
using System.Threading.Tasks;
using SSE;
using System.Linq;

namespace SSECLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("Please include path to ESM file");
                    return;
                }
                var pathToPlugin = args[0];
                if (!System.IO.File.Exists(pathToPlugin))
                {
                    Console.WriteLine("This file doesnt exist");
                    return;
                }
                var plugin = await Plugin.Load(new System.IO.FileInfo(pathToPlugin));
                var archive = await Archive.Open("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Skyrim Special Edition\\Data\\Skyrim - Interface.bsa");
                var table = StringLookupTable.ParseNullTerminated(await archive.Read("strings\\skyrim_english.strings"));

                Console.WriteLine("Plugin");
                Console.WriteLine("\tName: " + plugin.pluginRecord.cnam.content);
                Console.WriteLine("\tDescription: " + plugin.pluginRecord.snam.content);

                Console.WriteLine("\tMasters:");
                foreach (ZString master in plugin.pluginRecord.mast)
                {
                    Console.WriteLine("\t\t" + master.content);
                }

                Console.WriteLine("Weapons:");
                await foreach (WEAPRecord weapon in plugin.EnumerateGroupRecords("WEAP").Select(record => WEAPRecord.From(record, plugin.pluginRecord)))
                {
                    Console.WriteLine("\tName: " + weapon.full.GetResolvedContent(table));
                    Console.WriteLine("\t\tDamage: " + weapon.data.damage);
                    Console.WriteLine("\t\tWeight: " + weapon.data.weight);
                    Console.WriteLine("\t\tValue: " + weapon.data.value);
                }
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
