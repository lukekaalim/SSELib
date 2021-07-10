using System;
using System.IO;

namespace SSE.TESVArchive
{

	public struct BSAHash
	{
		public static int HashSize => sizeof(ulong);
		public ulong Value { get; set; }

		static uint GetMaskForExtension(string extension)
		{
			switch (extension)
			{
				case ".kf":
					return 0x80u;
				case ".nif":
					return 0x8000u;
				case ".dds":
					return 0x8080u;
				case ".wav":
					return 0x80000000u;
				default:
					return 0x0;
			}
		}

		public BSAHash(byte[] bytes, int offset) => Value = BitConverter.ToUInt64(bytes, offset);
		public BSAHash(ulong value) => this.Value = value;

		public static BSAHash Parse(byte[] bytes, int offset)
		{
			var value = BitConverter.ToUInt64(bytes, offset);
			return new BSAHash() { Value = value, };
		}

		public static BSAHash HashPath(string name)
		{
			name = name.Replace('/', '\\');
			return HashFile(Path.ChangeExtension(name, null), Path.GetExtension(name));
		}
		public static BSAHash HashFile(string name, string extension)
		{
			name = name.ToLowerInvariant();
			extension = extension.ToLowerInvariant();
			var hashBytes = new byte[]
			{
				(byte)(name.Length == 0 ? '\0' : name[name.Length - 1]),
				(byte)(name.Length < 3 ? '\0' : name[name.Length - 2]),
				(byte)name.Length,
				(byte)name[0]
			};
			var hash1 = BitConverter.ToUInt32(hashBytes, 0) | GetMaskForExtension(extension);

			uint hash2 = 0;
			for (var i = 1; i < name.Length - 2; i++)
				hash2 = hash2 * 0x1003f + (byte)name[i];

			uint hash3 = 0;
			for (var i = 0; i < extension.Length; i++)
				hash3 = hash3 * 0x1003f + (byte)extension[i];

			return new BSAHash((((ulong)(hash2 + hash3)) << 32) + hash1);
		}

		public static implicit operator ulong(BSAHash h) => h.Value;
	}
}