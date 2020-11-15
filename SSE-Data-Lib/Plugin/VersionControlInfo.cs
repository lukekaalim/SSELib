using System;

namespace SSE
{
	public partial class Plugin
	{
        public struct VersionControlInfo
        {
            public static uint Size = 4;
            public Byte date;
            public Byte month;
            public Byte lastUserId;
            public Byte currentUserId;

            public static VersionControlInfo Parse(Byte[] bytes, int offset)
            {
                return new VersionControlInfo()
                {
                    date = bytes[offset],
                    month = bytes[offset + 1],
                    lastUserId = bytes[offset + 2],
                    currentUserId = bytes[offset + 3],
                };
            }
        }
    }
}