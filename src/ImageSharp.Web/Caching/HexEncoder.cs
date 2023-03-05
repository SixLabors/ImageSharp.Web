// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Web.Caching;

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
    /// Converts a <see cref="ReadOnlySpan{Byte}"/> to a hexidecimal formatted <see cref="string"/> padded to 2 digits.
    /// </summary>
    /// <param name="bytes">The bytes.</param>
    /// <returns>The <see cref="string"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string Encode(ReadOnlySpan<byte> bytes)
    {
        fixed (byte* bytesPtr = bytes)
        {
            return string.Create(bytes.Length * 2, (Ptr: (IntPtr)bytesPtr, bytes.Length), (chars, args) =>
            {
                var ros = new ReadOnlySpan<byte>((byte*)args.Ptr, args.Length);
                EncodeToUtf16(ros, chars);
            });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EncodeToUtf16(ReadOnlySpan<byte> bytes, Span<char> chars)
    {
        ref byte bytesRef = ref MemoryMarshal.GetReference(bytes);
        ref char charRef = ref MemoryMarshal.GetReference(chars);
        ref char hiRef = ref MemoryMarshal.GetReference<char>(HexLutHi);
        ref char lowRef = ref MemoryMarshal.GetReference<char>(HexLutLo);

        int index = 0;
        for (int i = 0; i < bytes.Length; i++)
        {
            byte byteIndex = Unsafe.Add(ref bytesRef, i);
            Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref hiRef, byteIndex);
            Unsafe.Add(ref charRef, index++) = Unsafe.Add(ref lowRef, byteIndex);
        }
    }
}
