using System;

namespace SixLabors.ImageSharp.Web.Memory
{
    /// <summary>
    /// Represents a buffer backed by a managed byte array.
    /// </summary>
    public interface IByteBuffer : IDisposable
    {
        /// <summary>
        /// Gets the managed array backing this buffer instance.
        /// </summary>
        byte[] Array { get; }

        /// <summary>
        /// Gets the expected length of the buffer
        /// </summary>
        int Length { get; }
    }
}