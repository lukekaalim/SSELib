using System;
using System.IO;
using NUnit.Framework;

using SSE.TESVNif;

namespace TESVTesting
{
    public class NifTests
    {
        [Test]
        public void TestHeader()
		{
			var axePath = "/Users/lukekaalim/projects/SSE-Data-Lib/TestData/axe01.nif";
			var rankaPath = "/Users/lukekaalim/projects/SSE-Data-Lib/TestData/Ranka Axe-446-1-0-1-1/Data/meshes/weapons/ranka/ranka.nif";
			var macePath = "/Users/lukekaalim/projects/SSE-Data-Lib/TestData/W_art_Ice_mace.NIF";


			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();
			for (int i = 0; i < 1000; i++)
			{
				using var axeReader = new NIFStreamReader(new FileInfo(axePath).OpenRead());
				var axe = axeReader.ReadFile();
			}
			watch.Stop();
			Console.WriteLine(watch.Elapsed);
		}
    }
}
