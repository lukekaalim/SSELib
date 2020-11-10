using System;
namespace SSEData
{
    /// <summary>
    /// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format/WEAP
    /// </summary>
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

        public struct DNAMRecord
        {
            public enum AnimType: Byte
            {
                Other = 0,
                OneHandSword = 1,
                OneHandDagger = 2,
                OneHandAxe = 3,
                OneHandMace = 4,
                TwoHandSword = 5,
                TwoHandAxe = 6,
                Bow = 7,
                Staff = 8,
                Crossbow = 9
            }
            public AnimType animType;
            public Byte unknown1;
            public Int16 unknown2;
            /// <summary>
            /// Speed of weapon.
            /// </summary>
            public Single speed;
            /// <summary>
            /// For melee weapons, this is a multiplier used in the reach
            /// formula: fCombatDistance * NPCScale * WeaponReach
            /// (where fCombatDistance is a gamesetting).
            /// </summary>
            public Single reach;

            [Flags]
            public enum Flags1: UInt16
            {
                /// <summary>
                /// (probably an obsolete holdover from Oblivion)
                /// </summary>
                IgnoreNormalWeaponResistance = 0x01,
                Automatic = 0x02,
                HasScope = 0x04,
                CantDrop = 0x08,
                HideBackpack = 0x10,
                EmbeddedWeapon = 0x20,
                /// <summary>
                /// always paired with 0x100 in third set of flags
                /// </summary>
                DontUse1stOr3rdISAnimations = 0x40,
                Unplayable = 0x80,
            }
            public Flags1 flags1;
            /// <summary>
            /// Possibly more flags,
            /// but has a constant value of 145 (0x91) in all records
            /// </summary>
            public UInt16 flags2;
            /// <summary>
            /// (deprecated?) The angle of view when using Iron Sights. Done by zooming in.
            /// </summary>
            public Single sightFOV;
            /// <summary>
            /// 0 in all records
            /// </summary>
            public UInt32 unknown3;
            /// <summary>
            /// Base VATS To-Hit Chance
            /// (possibly obsolete). Only values are 0 and 5
            /// </summary>
            public Byte vatsToHit;
            /// <summary>
            /// -1 in all records
            /// </summary>
            public Byte unknown4;
            /// <summary>
            /// Number of projectiles per single ammo object
            /// (possibly obsolete). 1 in all records
            /// </summary>
            public Byte projectiles;
            /// <summary>
            /// Actor Value, adding 46 to the stored value gives the actor
            /// value index, CK allows
            /// (0 PerceptionCondition through 6 BrainCondition),
            /// only valid if Embedded Weapon flag is set
            /// </summary>
            public ActorValue embeddedWeapon;
            /// <summary>
            /// Tells combat AI they don't need to be closer than this value.
            /// </summary>
            public Single minRange;
            /// <summary>
            /// Tells combat AI they don't want to be any further away than this value.
            /// </summary>
            public Single maxRange;
            /// <summary>
            /// 0 in all records
            /// </summary>
            public UInt32 unknown5;

            [Flags]
            public enum Flags3: UInt32
            {
                PlayerOnly = 0x01,
                NPCsUseAmmo = 0x02,
                NoJamAfterReload = 0x04,
                MinorCrime = 0x10,
                FixedRange = 0x20,
                NotUsedInNormalCombat = 0x40,
                /// <summary>
                /// Always paired with 0x40 in first set of flags
                /// </summary>
                DontUse1stOr3rdISAnimations = 0x100,
                BurstShot = 0x200,
                AlternateRumble = 0x400,
                LongBurst = 0x800,
                NonHostile = 0x1000,
                BoundWeapon = 0x2000,
            }
            public Flags3 flags3;
            public Single unknown6;
            public Single unknown7;
            public Single rumbleLeft;
            public Single rumbleRight;
            public Single rumbleDuration;
            public UInt32 unknown8;
            public UInt32 unknown9;
            public UInt32 unknown10;
            public ActorValue? skill;
            public UInt32 unknown11;
            public UInt32 unknown12;
            /// <summary>
            /// CK allows 39 DamageResist through 44
            /// MagicResist or -1 for None (always -1 in original files)
            /// </summary>
            public ActorValue? resist;
            public UInt32 unknown13;
            public Single stagger;

            public static DNAMRecord From(Byte[] bytes)
            {
                var skill = BitConverter.ToInt32(bytes, 76);
                var resist = BitConverter.ToInt32(bytes, 88);
                return new DNAMRecord()
                {
                    animType = (AnimType)bytes[0],
                    unknown1 = bytes[1],
                    unknown2 = BitConverter.ToInt16(bytes, 2),
                    speed = BitConverter.ToSingle(bytes, 4),
                    reach = BitConverter.ToSingle(bytes, 8),
                    flags1 = (Flags1)BitConverter.ToUInt16(bytes, 12),
                    flags2 = BitConverter.ToUInt16(bytes, 14),
                    sightFOV = BitConverter.ToSingle(bytes, 16),
                    unknown3 = BitConverter.ToUInt32(bytes, 20),
                    vatsToHit = bytes[24],
                    unknown4 = bytes[25],
                    projectiles = bytes[26],
                    embeddedWeapon = (ActorValue)bytes[27] + 46,
                    minRange = BitConverter.ToSingle(bytes, 28),
                    maxRange = BitConverter.ToSingle(bytes, 32),
                    unknown5 = BitConverter.ToUInt32(bytes, 36),
                    flags3 = (Flags3)BitConverter.ToUInt32(bytes, 40),
                    unknown6 = BitConverter.ToSingle(bytes, 44),
                    unknown7 = BitConverter.ToSingle(bytes, 48),
                    rumbleLeft = BitConverter.ToSingle(bytes, 52),
                    rumbleRight = BitConverter.ToSingle(bytes, 56),
                    rumbleDuration = BitConverter.ToSingle(bytes, 60),
                    unknown8 = BitConverter.ToUInt32(bytes, 64),
                    unknown9 = BitConverter.ToUInt32(bytes, 68),
                    unknown10 = BitConverter.ToUInt32(bytes, 72),
                    skill = skill == -1 ? null : (ActorValue?)skill,
                    unknown11 = BitConverter.ToUInt32(bytes, 80),
                    unknown12 = BitConverter.ToUInt32(bytes, 84),
                    resist = resist == -1 ? null : (ActorValue?)resist,
                    unknown13 = BitConverter.ToUInt32(bytes, 92),
                    stagger = BitConverter.ToSingle(bytes, 96),
                };
            }
        }
        public DNAMRecord dnam;

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
                eitm = record.GetFirstField("EITM", FormID.From),
                etyp = record.GetFirstField("ETYP", FormID.From),
                full = record.GetFirstField("FULL", b => LString.From(b, plugin)),
                dnam = record.GetFirstField("DNAM", DNAMRecord.From)
            };
        }
    }
}
