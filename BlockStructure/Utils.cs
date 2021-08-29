using System;
using System.Globalization;

namespace BlockStructure
{
    public static class Utils
    {
        public static long ParseLong(string source)
        {
            if (source.StartsWith("0x"))
                return long.Parse(
                    source.Substring(2),
                    NumberStyles.AllowHexSpecifier
                );
            else
                return long.Parse(source);
        }

        public static bool TryParseLong(string source, out long value)
        {
            if (source.StartsWith("0x"))
            {
                return long.TryParse(source.Substring(2), NumberStyles.AllowHexSpecifier, null, out value);
            } else
            {
                return long.TryParse(source, out value);
            }
        }
    }
}
