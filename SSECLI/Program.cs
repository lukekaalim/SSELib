using System;
using System.Threading.Tasks;
using SSEData;

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
                var plugin = await Plugin.Load(pathToPlugin);

                Console.WriteLine("Plugin");
                Console.WriteLine("\tName: " + plugin.pluginInformation.cnam.content);
                Console.WriteLine("\tDescription: " + plugin.pluginInformation.snam.content);

                Console.WriteLine("\tMasters:");
                foreach (ZString master in plugin.pluginInformation.mast)
                {
                    Console.WriteLine("\t\t" + master.content);
                }

                Console.WriteLine("Weapons:");
                foreach (WEAPRecord weapon in plugin.weapons.Values)
                {
                    Console.WriteLine("\tName: " + weapon.full.content.content);
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
