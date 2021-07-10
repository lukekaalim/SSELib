using System;

namespace SSE.TESVArchive
{

	[Flags]
	public enum BSA105ArchiveFlags : uint
	{
		IncludeDirectoryNames = 0x1,
		IncludeFileNames = 0x2,
		/// <summary>
		/// This does not mean all files are compressed.
		/// It means they are compressed by default.
		/// </summary>
		CompressedArchive = 0x4,
		RetainDirectorynames = 0x8,
		RetainFileNames = 0x10,
		RetainFileNameOffsets = 0x20,
		Xbox360Archive = 0x40,
		RetainStringsDuringStartup = 0x80,
		/// <summary>
		///  Indicates the file data blocks begin with a
		///  bstring containing the full path of the file.
		///  For example, in "Skyrim - Textures.bsa" the
		///  first data block is
		///  $2B textures\effects\fxfluidstreamdripatlus.dds
		///  ($2B indicating the name is 43 bytes).
		///  The data block begins immediately after the bstring.
		/// </summary>
		EmbedFileNames = 0x100,
		/// <summary>
		/// This can only be used with Bit 3
		/// (Compress Archive). This is an Xbox 360
		/// only compression algorithm.
		/// </summary>
		XMemCodec = 0x200
	}

	[Flags]
	public enum BSA105FileFlags : uint
	{
		Meshes = 1,
		Textures = 2,
		Menus = 4,
		Sounds = 8,
		Voices = 16,
		Shaders = 32,
		Trees = 64,
		Fonts = 128,
		Miscellaneous = 256,
	}
}