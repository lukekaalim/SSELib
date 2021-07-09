using System;
using System.Collections.Generic;
using System.Text;

namespace SSE.TESVRecord.DataTypes
{
    public readonly struct LocalFormID
    {
        public readonly uint formId;
        public LocalFormID(byte[] bytes, int offset) => formId = BitConverter.ToUInt32(bytes, offset);

        public uint RecordID => formId & 0x00FFFFFF;
        public uint MasterIndex => formId >> (8 * 3);

        /*
        public ResolvedFormID Resolve(LoadOrder order, SSEPlugin parent)
        {
            var master = parent.GetMasterName((int)MasterIndex);
            var masterIndex = (UInt32)order.plugins.FindIndex(plugin =>
                string.Equals(master, plugin, StringComparison.OrdinalIgnoreCase));

            return new ResolvedFormID(RecordID, masterIndex);
        }
        */
    }
    public readonly struct ResolvedFormID
    {
        public readonly uint recordId;
        public readonly uint masterIndex;
        public ResolvedFormID(uint recordId, uint masterIndex) =>
            (this.recordId, this.masterIndex) = (recordId, masterIndex);
    }
}
