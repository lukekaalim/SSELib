﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace SSE
{
	public partial class Plugin
    {

        public FileInfo pluginFile;

        public TES4Record pluginRecord;
        public GroupLookupTable groupTable;

        public static async Task<Plugin> Load(FileInfo pluginFile)
        {
            using var stream = pluginFile.OpenRead();

            // The first record of a Plugin should be the TES4Record
            // which should tell use about the rest of the records
            var firstRecord = await Record.Read(stream);
            var pluginRecord = TES4Record.From(firstRecord);
            var groupStartOffset = stream.Position;

            var groups = await Group.ReadAllTopLevelGroups(stream, pluginRecord);
            var groupTable = new GroupLookupTable(groups, groupStartOffset);

            return new Plugin()
            {
                pluginFile = pluginFile,
                pluginRecord = pluginRecord,
                groupTable = groupTable,
            };
        }

        public async Task<List<Record>> ReadGroupRecordsAsync(string recordType)
		{
            using var stream = pluginFile.OpenRead();
            (int offset, Group group) = groupTable.GetLookupReference(recordType);

            stream.Seek(offset, SeekOrigin.Begin);

            Byte[] bytes = new Byte[group.groupSize];
            await stream.ReadAsync(bytes);

            int parsedBytes = 0;
            var records = new List<Record>();
            while (parsedBytes < group.DataSize)
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
            (int offset, Group group) = groupTable.GetLookupReference(recordType);

            stream.Seek(offset, SeekOrigin.Begin);

            while (stream.Position - offset < group.DataSize)
                yield return await Record.Read(stream);
        }

        public IAsyncEnumerable<WEAPRecord> GetWEAPRecords()
        {
            return EnumerateGroupRecords("WEAP").Select(record => WEAPRecord.From(record, pluginRecord));
        }
    }
}
