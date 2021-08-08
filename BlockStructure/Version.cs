using System;

namespace BlockStructure
{
    public class VersionKey : IEquatable<VersionKey>
    {
        public int NifVersion { get; set; }
        public uint? BethesdaVersion { get; set; }
        public uint? UserVersion { get; set; }

        public Lazy<Logic.State> State { get; set; }

        public VersionKey() { }
        public VersionKey(int nifVersion,
                          uint? bsVersion = null,
                          uint? userVersion = null)
        {
            NifVersion = nifVersion;
            BethesdaVersion = bsVersion;
            UserVersion = userVersion;
            State = new Lazy<Logic.State>(() => new Logic.State(this));
        }

        public bool Equals(VersionKey other)
        {
            return (
                NifVersion == other.NifVersion &&
                BethesdaVersion == other.BethesdaVersion &&
                UserVersion == other.UserVersion
            );
        }

        public bool MatchesVersionConstraint(int? minVersion, int? maxVersion)
        {
            if (minVersion != null && NifVersion < minVersion)
                return false;
            if (maxVersion != null && NifVersion > maxVersion)
                return false;
            return true;
        }

        public override bool Equals(object obj) =>
            Equals(obj as VersionKey);
        public override int GetHashCode() =>
            (NifVersion, BethesdaVersion, UserVersion).GetHashCode();
        public override string ToString() =>
            $"{NifVersion} ({BethesdaVersion}, {UserVersion})";
    }
}
