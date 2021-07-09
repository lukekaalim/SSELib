using System;
using System.IO;
using System.Threading.Tasks;

namespace SSE
{
	public interface Streamable
	{
		Stream OpenStream();
	}

	public class ReadableStreamSlice : Stream
	{
		int sliceOffset;
		int sliceLength;
		Stream originalStream;

		public override bool CanRead => true;
		public override bool CanSeek => true;
		public override bool CanWrite => false;
		public override long Length => sliceLength;

		public override long Position {
			get => originalStream.Position - sliceOffset;
			set => originalStream.Position = (value + sliceOffset);
		}

		public override void Flush() => originalStream.Flush();

		public override int Read(byte[] buffer, int offset, int count)
		{
			int remainingBytes = (int)(Length - Position);
			int maxCount = Math.Min(count, remainingBytes);
			return originalStream.Read(buffer, offset, maxCount);
		}

		// todo: probably incomplete
		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					{
						var maxReadableBytes = Length + sliceOffset;
						var originalOffset = offset + sliceOffset;
						return originalStream.Seek(Math.Min(originalOffset, maxReadableBytes), SeekOrigin.Begin);
					}
				case SeekOrigin.Current:
					{
						var distanceToEndOfSlice = Length - Position;
						return originalStream.Seek(Math.Min(offset, distanceToEndOfSlice), SeekOrigin.Current);
					}
				case SeekOrigin.End:
					{
						// offset is a negative number here
						var maxReadableBytes = Length + sliceOffset;
						var originalOffset = offset + (sliceOffset + sliceLength);
						return originalStream.Seek(Math.Max(-maxReadableBytes, originalOffset), SeekOrigin.End);
					}
				default:
					throw new Exception("Unknown SeekOrigin value");
			}
		}

		protected override void Dispose(bool disposing) => originalStream.Dispose();

		public override void SetLength(long value) {}

		public override void Write(byte[] buffer, int offset, int count) {}

		public ReadableStreamSlice(Stream originalStream, int offset, int length)
		{
			this.originalStream = originalStream;
			this.sliceOffset = offset;
			this.sliceLength = length;
		}
	}
}
