using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SSE
{
	public partial struct Archive
	{
		public struct FolderRecord
		{
			/// <summary>
			/// Complex Hash Algorithm
			/// </summary>
			public Hash nameHash;
			public UInt32 count;
			public UInt32 padding;
			public UInt64 offset;

			public static FolderRecord From(Byte[] bytes, int offset)
			{
				return new FolderRecord()
				{
					nameHash = Hash.From(bytes, offset + 0),
					count = BitConverter.ToUInt32(bytes, offset + 8),
					padding = BitConverter.ToUInt32(bytes, offset + 12),
					offset = BitConverter.ToUInt64(bytes, offset + 16),
				};
			}
		}
		public struct FileRecord
		{
			public Hash nameHash;
			public UInt32 size;
			/// <summary>
			/// If the 30th bit (0x40000000) is set in the size:
			/// If files are default compressed, this file is not compressed.
			/// If files are default not compressed, this file is compressed.
			/// If the file is compressed the file data will have the specification
			/// of Compressed File block. In addition, the size of compressed data
			/// is considered to be the ulong "original size" plus the compressed
			/// data size (4 + compressed size).
			/// </summary>
			public bool compressionBit;
			public UInt32 offset;
			public static FileRecord From(Byte[] bytes, int offset)
			{
				return new FileRecord()
				{
					nameHash = Hash.From(bytes, offset + 0),
					// Mask out the 30th bit
					size = BitConverter.ToUInt32(bytes, offset + 8) & 0xFFFFFFFE,
					// And mask out every bit BUT the 30th
					compressionBit = (BitConverter.ToUInt32(bytes, offset + 8) & 0x00000001) == 1,
					offset = BitConverter.ToUInt32(bytes, offset + 12),
				};
			}
		}
		public struct FileRecordBlock
		{
			public BZString name;
			public FileRecord[] fileRecords;
			public static FileRecordBlock From(Byte[] bytes, int offset, int fileCount)
			{
				var name = BZString.From(bytes, offset);
				var fileRecords = new FileRecord[fileCount];

				for (int i = 0; i < fileCount; i++)
					fileRecords[i] = FileRecord.From(bytes, offset + (name.length + 1) + (i * 16));

				return new FileRecordBlock()
				{
					name = name,
					fileRecords = fileRecords,
				};
			}
		}

		public struct RecordDatabase
		{
			public static int folderRecordSize = 8 + 4 + 4 + 8;
			public static int fileRecordSize = 8 + 4 + 4;

			public FolderRecord[] folderRecords;
			public FileRecordBlock[] fileRecordBlocks;
			public string[] filenames;

			public Dictionary<string, FileRecord> recordsByFilepath;

			public static int CalculateFolderRecordSize(Header header)
			{
				return (int)(folderRecordSize * header.folderCount);
			}

			public static int CalculateFileRecordSize(Header header)
			{
				return (int)(header.fileCount * fileRecordSize);
			}

			public static int CalculateFileRecordBlockSize(Header header)
			{
				return (int)(
					header.totalFolderNameLength +
					// totalFolderNameLength does not include the "length" byte of all the strings
					// so we include it again
					(1 * header.folderCount) +
					CalculateFileRecordSize(header)
				);
			}

			public static int CalculateSize(Header header)
			{
				return CalculateFolderRecordSize(header) + CalculateFileRecordBlockSize(header) + (int)header.totalFileNameLength;
			}

			public RecordDatabase(Byte[] bytes, int offset, Header header)
			{
				folderRecords = new FolderRecord[header.folderCount];
				fileRecordBlocks = new FileRecordBlock[header.folderCount];
				recordsByFilepath = new Dictionary<string, FileRecord>();

				var filenamesOffset = offset + CalculateFolderRecordSize(header) + CalculateFileRecordBlockSize(header);
				filenames = Encoding.UTF8.GetString(bytes, filenamesOffset, (int)header.totalFileNameLength).Split('\0', (int)header.fileCount);

				int fileIndex = 0;
				for (int i = 0; i < header.folderCount; i++)
				{
					var folderRecord = FolderRecord.From(bytes, offset + i * folderRecordSize);
					folderRecords[i] = folderRecord;
					int folderOffset = (int)((int)(folderRecord.offset) - header.totalFileNameLength - Header.size);

					var fileRecordBlock = FileRecordBlock.From(bytes, folderOffset, (int)folderRecord.count);
					fileRecordBlocks[i] = fileRecordBlock;
					for (int y = 0; y < folderRecord.count; y++)
					{
						var filename = filenames[fileIndex + y];
						var directory = fileRecordBlock.name.content;
						var filepath = Path.Combine(directory, filename);
						recordsByFilepath.Add(filepath, fileRecordBlock.fileRecords[y]);
					}

					fileIndex += (int)folderRecord.count;
				}
			}
		}
	}
}
