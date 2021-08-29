using System;
using System.Collections.Generic;

using BlockStructure.Logic;

namespace BlockStructure
{
    public class VersionKey : IEquatable<VersionKey>
    {
        public int NifVersion { get; set; }
        public uint? BethesdaVersion { get; set; }
        public uint? UserVersion { get; set; }

        public VersionKey() { }
        public VersionKey(int nifVersion,
                          uint? bsVersion = null,
                          uint? userVersion = null)
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

        public Interpreter.State AsState()
        {
            var bsVersion = BethesdaVersion.HasValue ? BethesdaVersion.Value : 0;
            var headerValue = new Interpreter.State()
            {
                ParameterCompoundStates = new Dictionary<string, Interpreter.State>(),
                ParameterValues = new Dictionary<string, long>()
                {
                    ["BS Version"] = bsVersion,
                }
            };
            var userVersion = UserVersion.HasValue ? UserVersion.Value : 0;
            return new Interpreter.State()
            {
                ParameterCompoundStates = new Dictionary<string, Interpreter.State>()
                {
                    ["BS Header"] = headerValue
                },
                ParameterValues = new Dictionary<string, long>()
                {
                    ["User Version"] = userVersion,
                    ["Version"] = NifVersion
                },
            };
        }
    }
}
