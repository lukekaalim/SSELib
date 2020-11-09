using System;
using System.Text;
using System.Collections.Generic;

namespace SSEData
{
    /// <summary>
    /// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format/TES4
    /// </summary>
    public struct TES4Record
    {
        [Flags]
        public enum Flags: UInt32
        {
            Master = 0x00000001,
            Localized = 0x00000080,
            LightMaster = 0x00000200
        }

        public struct HEDRRecord
        {
            public Single version;
            public Int32 numRecords;
            public UInt32 nextObjectId;

            public static HEDRRecord From(Byte[] bytes)
            {
                return new HEDRRecord()
                {
                    version = BitConverter.ToSingle(bytes, 0),
                    numRecords = BitConverter.ToInt32(bytes, 4),
                    nextObjectId = BitConverter.ToUInt32(bytes, 8)
                };
            }
        }
        /// <summary>
        /// Plugin Header
        /// </summary>
        public HEDRRecord hedr;
        /// <summary>
        /// Author Name
        /// </summary>
        public ZString cnam;
        /// <summary>
        /// Plugin Description
        /// </summary>
        public ZString snam;
        public Flags flags;
        /// <summary>
        /// Master Files
        /// </summary>
        public List<ZString> mast;
        /// <summary>
        /// Overridden forms.
        /// This record only appears in ESM flagged files which
        /// override their masters' cell children.
        /// An ONAM subrecord will list, exclusively, FormIDs of
        /// overridden cell children(ACHR, LAND, NAVM, PGRE, PHZD, REFR).
        /// Observed in Update.esm as of Patch 1.5.24.
        /// Number of records is based solely on field size.
        /// </summary>
        public List<FormID> onam;

        public static TES4Record From(Record record)
        {
            return new TES4Record()
            {
                flags = (Flags)record.flags,
                cnam = record.GetFirstField("CNAM", ZString.From),
                snam = record.GetFirstField("SNAM", ZString.From),
                hedr = record.GetFirstField("HEDR", HEDRRecord.From),
                mast = record.GetAllFields("MAST", ZString.From),
                onam = record.GetFirstField("ONAM", bytes =>
                {
                    var formIds = new List<FormID>();
                    for (int i = 0; i < bytes.Length; i += 4)
                        formIds.Add(FormID.From(bytes, i));
                    return formIds;
                })
            };
        }
    }
}
