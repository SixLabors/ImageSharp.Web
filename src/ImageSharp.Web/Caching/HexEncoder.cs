// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Provides methods for encoding byte arrrays into hex strings.
    /// </summary>
    internal static class HexEncoder
    {
        /// <summary>
        /// Converts a <see cref="Span{Byte}"/> to a hexidecimal formatted <see cref="string"/> padded to 2 digits.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Encode(Span<byte> bytes)
        {
            int length = bytes.Length;
            char[] c = new char[length * 2];
            ref char charRef = ref c[0];
            ref byte bytesRef = ref MemoryMarshal.GetReference(bytes);
            const int padHi = 0x37 + 0x20;
            const int padLo = 0x30;

            byte b;
            for (int bx = 0, cx = 0; bx < length; ++bx, ++cx)
            {
                byte bref = Unsafe.Add(ref bytesRef, bx);
                b = (byte)(bref >> 4);
                Unsafe.Add(ref charRef, cx) = (char)(b > 9 ? b + padHi : b + padLo);

                b = (byte)(bref & 0x0F);
                Unsafe.Add(ref charRef, ++cx) = (char)(b > 9 ? b + padHi : b + padLo);
            }

            return new string(c);
        }
    }
}
