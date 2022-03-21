using System.IO;

namespace SimilarCode.Load
{
    internal class StreamWithReadOnlyPosition : Stream
    {
        private readonly Stream inner;
        private long _readOffset;

        public StreamWithReadOnlyPosition(Stream inner)
        {
            this.inner = inner;
            _readOffset = 0;
        }
        public override void Flush()
        {
            this.inner.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            this._readOffset += count;
            return this.inner.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return this.inner.Seek(offset, origin);
        }


        public override void SetLength(long value)
        {
            this.inner.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            this.inner.Write(buffer, offset, count);
        }

        public override bool CanRead => this.inner.CanRead;

        public override bool CanSeek => this.inner.CanSeek;

        public override bool CanWrite => this.inner.CanWrite;

        public override long Length => this.inner.Length;

        public override long Position
        {
            get => this.inner.Position;
            set => this.inner.Position = value;
        }

        public long ReadOffset => this._readOffset;
    }
}