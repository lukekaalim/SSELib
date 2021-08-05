using System;

namespace BlockStructure
{
    public class VersionKey : IEquatable<VersionKey>
    {
        public int NifVersion { get; set; }
        public uint? BethesdaVersion { get; set; }
        public uint? UserVersion { get; set; }

        public VersionKey() { }
        public VersionKey(int nifVersion, uint? bsVersion, uint? userVersion)
        {
            NifVersion = nifVersion;
            BethesdaVersion = bsVersion;
            UserVersion = userVersion;
        }

        public bool Equals(VersionKey other)
        {
            return (
                NifVersion == other.NifVersion &&
                BethesdaVersion == other.BethesdaVersion &&
                UserVersion == other.UserVersion
            );
        }

        public override bool Equals(object obj) =>
            Equals(obj as VersionKey);
        public override int GetHashCode() =>
            (NifVersion, BethesdaVersion, UserVersion).GetHashCode();
        public override string ToString() =>
            $"{NifVersion} ({BethesdaVersion}, {UserVersion})";
    }
}
