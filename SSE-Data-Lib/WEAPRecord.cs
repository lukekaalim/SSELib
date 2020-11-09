using System;
namespace SSEData
{
    public struct WEAPRecord
    {
        /// <summary>
        /// Alternate block material. Points to a MATT.
        /// </summary>
        public FormID bamt;
        /// <summary>
        /// Block bash impact data set. Points to a IPDS.
        /// </summary>
        public FormID bids;
        /// <summary>
        /// Points to another WEAP record to use as a template
        /// </summary>
        public FormID cnam;

        public struct CRDTRecord
        {
            public UInt16 criticalDamage;
            public UInt16 unknown;
            public Single criticalPercentMult;
            [Flags]
            public enum Flags : UInt32
            {
                CriticalEffectOnDeath = 0x01
            }
            public Flags flags;
            public UInt32 unknown2;
            /// <summary>
            /// Links to a SPEL
            /// </summary>
            public FormID criticalSpellEffect;
            public UInt32 unknown3;

            public static CRDTRecord From(Byte[] bytes)
            {
                return new CRDTRecord()
                {
                    criticalDamage = BitConverter.ToUInt16(bytes, 0),
                    unknown = BitConverter.ToUInt16(bytes, 2),
                    criticalPercentMult = BitConverter.ToUInt16(bytes, 4),
                    flags = (Flags)BitConverter.ToUInt32(bytes, 8),
                    unknown2 = BitConverter.ToUInt32(bytes, 12),
                    criticalSpellEffect = FormID.From(bytes, 16),
                    unknown3 = BitConverter.ToUInt32(bytes, 20),
                };
            }
        }

        /// <summary>
        /// Critical Damage record
        /// </summary>
        public CRDTRecord crtd;

        public struct DATARecord
        {
            public UInt32 value;
            public Single weight;
            public Int16 damage;

            public static DATARecord From(Byte[] bytes)
            {
                return new DATARecord()
                {
                    value = BitConverter.ToUInt32(bytes, 0),
                    weight = BitConverter.ToSingle(bytes, 4),
                    damage = BitConverter.ToInt16(bytes, 8),
                };
            }
        }

        public DATARecord data;
        /// <summary>
        /// Description
        /// </summary>
        public LString desc;
        /// <summary>
        /// Enchantment Charge Amount
        /// </summary>
        public UInt16 eamt;
        /// <summary>
        /// editorID
        /// </summary>
        public ZString edid;
        /// <summary>
        /// Enchantment. Points to a ENCH.
        /// </summary>
        public FormID eitm;
        /// <summary>
        ///  Equip Type
        ///  Specifies which equipment slot is used by the weapon
        ///  (typically "BothHands" or "EitherHand"). Points to a EQUP.
        /// </summary>
        public FormID etyp;
        /// <summary>
        /// Name
        /// </summary>
        public LString full;

        public static WEAPRecord From(Record record, TES4Record plugin)
        {
            return new WEAPRecord()
            {
                bamt = record.GetFirstField("BAMT", FormID.From),
                bids = record.GetFirstField("BIDS", FormID.From),
                cnam = record.GetFirstField("CNAM", FormID.From),
                crtd = record.GetFirstField("CRTD", CRDTRecord.From),
                data = record.GetFirstField("DATA", DATARecord.From),
                desc = record.GetFirstField("DESC", b => LString.From(b, plugin)),
                eamt = record.GetFirstField("EAMT", b => BitConverter.ToUInt16(b, 0)),
                edid = record.GetFirstField("EDID", ZString.From),
                eitm = record.GetFirstField("EAMT", FormID.From),
                etyp = record.GetFirstField("ETYP", FormID.From),
                full = record.GetFirstField("FULL", b => LString.From(b, plugin))
            };
        }
    }
}
