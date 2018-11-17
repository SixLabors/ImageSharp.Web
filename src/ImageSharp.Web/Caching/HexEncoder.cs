// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Web.Caching
{
    /// <summary>
    /// Provides methods for encoding byte arrays into hexidecimal strings.
    /// </summary>
    internal static class HexEncoder
    {
        // LUT's that provide the hexidecimal representation of each possible byte value.
        private static readonly char[] HexLutBase = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };

        // The base LUT arranged in 16x each item order. 0 * 16, 1 * 16, .... F * 16
        private static readonly char[] HexLutHi = Enumerable.Range(0, 256).Select(x => HexLutBase[x / 0x10]).ToArray();

        // The base LUT repeated 16x.
        private static readonly char[] HexLutLo = Enumerable.Range(0, 256).Select(x => HexLutBase[x % 0x10]).ToArray();

        /// <summary>
        /// Converts a <see cref="T:Span{byte}"/> to a hexidecimal formatted <see cref="string"/> padded to 2 digits.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <returns>The <see cref="string"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Encode(Span<byte> bytes)
        {
            int length = bytes.Length;
            char[] chars = new char[length * 2];
            ref char charRef = ref MemoryMarshal.GetReference<char>(chars);
            ref byte bytesRef = ref MemoryMarshal.GetReference(bytes);
            ref char hiRef = ref MemoryMarshal.GetReference<char>(HexLutHi);
            ref char lowRef = ref MemoryMarshal.GetReference<char>(HexLutLo);

            int index = 0;
            for (int i = 0; i < length; i++)
            {
                byte byteIndex = Unsafe.Add(ref bytesRef, i);
                Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref hiRef, byteIndex);
                Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref lowRef, byteIndex);
            }

            return new string(chars, 0, chars.Length);
        }
    }
}