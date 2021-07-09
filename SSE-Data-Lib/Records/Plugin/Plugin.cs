using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace SSE
{
	public partial class SSEPlugin
    {
        public string Name => Path.GetFileNameWithoutExtension(pluginFile.Name);
        public string Filename => pluginFile.Name;
        public List<string> Masters => pluginRecord.mast
            .Select(masterString => masterString.content)
            .ToList();

        public string GetMasterName(int index) => pluginRecord.mast[index].content;

        public FileInfo pluginFile;

        public TES4Record pluginRecord;
        public GroupLookupTable groupTable;

        public static async Task<SSEPlugin> Load(FileInfo pluginFile)
        {
            using var stream = pluginFile.OpenRead();

            // The first record of a Plugin should be the TES4Record
            // which should tell use about the rest of the records
            var firstRecord = await Record.Read(stream);
            var pluginRecord = TES4Record.From(firstRecord);
            var groupStartOffset = stream.Position;

            var groups = await Group.ReadAllTopLevelGroups(stream, pluginRecord);
            var groupTable = new GroupLookupTable(groups, groupStartOffset);

            return new SSEPlugin()
            {
                pluginFile = pluginFile,
                pluginRecord = pluginRecord,
                groupTable = groupTable,
            };
        }

        public async ValueTask<List<Record>> ReadGroupRecordsAsync(string recordType)
        {
            (int offset, Group? group) = groupTable.GetLookupReference(recordType);
            var records = new List<Record>();

            if (!group.HasValue)
                return records;

            using var stream = pluginFile.OpenRead();
            stream.Seek(offset, SeekOrigin.Begin);

            Byte[] bytes = new Byte[group.Value.groupSize];
            await stream.ReadAsync(bytes);

            int parsedBytes = 0;
            while (parsedBytes < group.Value.DataSize)
            {
                var record = new Record(bytes, parsedBytes);
                records.Add(record);
                parsedBytes += (int)record.Size;
            }
            return records;
        }

        public async IAsyncEnumerable<Record> EnumerateGroupRecords(string recordType)
        {
            using var stream = pluginFile.OpenRead();
            (int offset, Group? group) = groupTable.GetLookupReference(recordType);

            if (!group.HasValue)
                yield break;

            stream.Seek(offset, SeekOrigin.Begin);

            while (stream.Position - offset < group.Value.DataSize)
                yield return await Record.Read(stream);
            yield break;
        }
    }
}
