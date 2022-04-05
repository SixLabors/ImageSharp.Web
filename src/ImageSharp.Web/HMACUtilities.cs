// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace SixLabors.ImageSharp.Web
{
    /// <summary>
    /// Provides methods to compute a Hash-based Message Authentication Code (HMAC).
    /// </summary>
    public static class HMACUtilities
    {
        /// <summary>
        /// Computes a Hash-based Message Authentication Code (HMAC) by using the SHA256 hash function.
        /// </summary>
        /// <param name="value">The value to hash</param>
        /// <param name="secret">
        /// The secret key for <see cref="HMACSHA256"/>encryption.
        /// The key can be any length. However, the recommended size is 64 bytes.
        /// </param>
        /// <returns>The hashed <see cref="string"/>.</returns>
        public static unsafe string CreateSHA256HashCode(string value, byte[] secret)
        {
            static void Action(Span<char> chars, (IntPtr Ptr, int Length, string Value, byte[] Secret) args)
            {
                var bytes = new Span<byte>((byte*)args.Ptr, args.Length);
                Encoding.ASCII.GetBytes(args.Value, bytes);
                using var hashAlgorithm = new HMACSHA256(args.Secret);
                hashAlgorithm.TryComputeHash(bytes, MemoryMarshal.Cast<char, byte>(chars), out int _);
            }

            // Bits to chars - 256/8/2
            return CreateHMAC(value, secret, 16, Action);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe string CreateHMAC(string value, byte[] secret, int length, SpanAction<char, (IntPtr Ptr, int Length, string Value, byte[] Secret)> action)
        {
            int byteCount = Encoding.ASCII.GetByteCount(value);

            // Allocating a buffer from the pool is ~27% slower than stackalloc so use that for short strings
            if (byteCount < 257)
            {
                fixed (byte* bytesPtr = stackalloc byte[byteCount])
                {
                    return string.Create(length, ((IntPtr)bytesPtr, byteCount, value, secret), action);
                }
            }

            byte[] buffer = null;
            try
            {
                buffer = ArrayPool<byte>.Shared.Rent(byteCount);
                fixed (byte* bytesPtr = buffer.AsSpan(0, byteCount))
                {
                    return string.Create(length, ((IntPtr)bytesPtr, byteCount, value, secret), action);
                }
            }
            finally
            {
                if (buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }
        }
    }
}
