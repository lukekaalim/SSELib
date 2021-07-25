using System.Text;

namespace SSE.TESVArchive
{
	/// <summary>
	/// A string prefixed with a byte length and terminated with a zero (\x00).
	/// </summary>
	public readonly struct BZString
	{
		public readonly string content;
		public readonly int length;
		public int ByteLength => length + 1;
		public BZString(byte[] bytes, int offset)
		{
			length = bytes[offset];
			content = Encoding.UTF8.GetString(bytes, offset + 1, length - 1);
		}
		public static explicit operator string(BZString b) => b.content;
	}

	public readonly struct BString
	{
		public readonly string content;
		public readonly int length;
		public int ByteLength => length + 1;

		public BString(byte[] bytes, int offset)
		{
			length = bytes[offset];
			content = Encoding.UTF8.GetString(bytes, offset + 1, length);
		}
	}
}