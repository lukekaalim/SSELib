using System;
namespace BlockStructure.Schemas
{
    public static class Utils
    {
        public static long ParseLong(string source)
        {
            if (source.StartsWith("0x"))
                return long.Parse(
                    source.Substring(2),
                    System.Globalization.NumberStyles.AllowHexSpecifier
                );
            else
                return long.Parse(source);
        }
    }
}
