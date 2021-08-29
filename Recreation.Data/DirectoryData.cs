using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Recreation.Data
{
    using SSE.TESVRecord;
    using SSE.TESVNif;
    using SSE.TESVArchive;

    /// <summary>
    /// Provides metadata and objects detailing
    /// available resources.
    /// </summary>
    public class DirectoryData
    {
        public Dictionary<ArchiveInfo, FileInfo> FileByArchive;
        public Dictionary<string, (ArchiveInfo, FileRecord)> FileByPath;

        public static async Task<DirectoryData> Load(string dataDirectoryPath,
                                                     string pluginName)
        {
            var dataDirectoryInfo = new DirectoryInfo(dataDirectoryPath);

            var archiveFiles = dataDirectoryInfo
                .EnumerateFiles()
                .Where(file => file.Extension == "bsa")
                .Where(file => file.Name.StartsWith(pluginName));

            var fileByArchive = await archiveFiles
                .ToAsyncEnumerable()
                .SelectAwait(async file => (await ArchiveInfo.ReadInfo(file), file))
                .ToDictionaryAsync(f => f.Item1, f => f.file);

            var fileByPath = fileByArchive.Keys
                .SelectMany(archiveInfo =>
                    archiveInfo.RecordsByPath.Select(kvp =>
                        (kvp.Key, (archiveInfo, kvp.Value))))
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(g => g.Key, g => g.Last().Item2);

            return new DirectoryData()
            {
                FileByArchive = fileByArchive,
                FileByPath = fileByPath
            };
        }

        public async Task<byte[]> Load(string path)
        {
            var (info, record) = FileByPath[path];
            var fileInfo = FileByArchive[info];

            using (var stream = fileInfo.OpenRead())
            {
                var reader = new ArchiveStreamReader(stream, info);
                return await reader.ReadFile(record);
            }
        }

        //public async Task<NIFFile> LoadStatic(STATRecord stat)
        //{
        //    return null;
        //}
    }
}
