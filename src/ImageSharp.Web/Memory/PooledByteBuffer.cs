namespace SixLabors.ImageSharp.Web.Memory
{
    /// <summary>
    /// Represents a buffer whose backing array is from a pooled source.
    /// </summary>
    internal class PooledByteBuffer : IByteBuffer
    {
        private PooledBufferManager manager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PooledByteBuffer"/> class.
        /// </summary>
        /// <param name="manager">The buffer manager</param>
        /// <param name="array">The byte array</param>
        /// <param name="length">The expected minimum length of the buffer</param>
        public PooledByteBuffer(PooledBufferManager manager, byte[] array, int length)
        {
            this.manager = manager;
            this.Array = array;
            this.Length = length;
        }

        /// <inheritdoc />
        public byte[] Array { get; private set; }

        /// <inheritdoc />
        public int Length { get; }

        /// <inheritdoc />
        public void Dispose()
        {
            if (this.manager == null || this.Array == null)
            {
                return;
            }

            this.manager.Return(this.Array);
            this.manager = null;
            this.Array = null;
        }
    }
}