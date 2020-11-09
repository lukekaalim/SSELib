using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSEData
{
    public struct Plugin
    {
        public TES4Record pluginInformation;
        public Dictionary<FormID, WEAPRecord> weapons;
        public List<Group> topLevelGroups;
        public Record[] unknownRecords;

        public static async Task<Plugin> Load(string path)
        {
            // todo: maybe we dont need to load the whole thing into memory
            var bytes = await System.IO.File.ReadAllBytesAsync(path);

            // The first record of a Plugin should be the TES4Record
            // which should tell use about the rest of the plugins
            var firstRecord = Record.ParseFirst(bytes);
            var pluginInformation = TES4Record.From(firstRecord);
            var recordLength = (int)firstRecord.dataSize + 24;
            var topLevelGroups = Group.ParseAll(bytes, recordLength, bytes.Length - recordLength);

            var weapons = new Dictionary<FormID, WEAPRecord>();

            for (int i = 0; i < topLevelGroups.Count; i++)
            {
                var group = topLevelGroups[i];
                var groupLabel = Encoding.UTF8.GetString(group.label, 0, 4);
                switch (groupLabel)
                {
                    default:
                        break;
                    case "WEAP":
                        group.data
                            .ForEach(record => weapons.Add(record.id, WEAPRecord.From(record, pluginInformation)));
                        break;
                }
            }

            return new Plugin()
            {
                pluginInformation = pluginInformation,
                topLevelGroups = topLevelGroups,
                weapons = weapons
            };
        }
    }
}
