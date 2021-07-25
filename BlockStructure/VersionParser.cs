using System;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;

namespace BlockStructure
{
    public static class VersionParser
    {
        public static int Parse(string versionString)
        {
            var versionParts = versionString
                .Split('.')
                .Select(f => byte.Parse(f, NumberStyles.Integer))
                .Reverse()
                .ToArray();

            var versionBytes = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                versionBytes[i] = i < versionParts.Length ? versionParts[i] : (byte)0x00;
            }
            return BitConverter.ToInt32(versionBytes, 0);
        }
        public static int? Parse(XAttribute attribute)
        {
            if (attribute == null)
                return null;
            return Parse(attribute.Value);
        }
    }
}
