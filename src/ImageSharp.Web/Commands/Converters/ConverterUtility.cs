// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace SixLabors.ImageSharp.Web.Commands.Converters;
internal static class ConverterUtility
{
    /// <summary>
    /// Gets the <see cref="char"/> that is used by the given culture to separate items in a list.
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <returns>The <see cref="char"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static char GetListSeparator(CultureInfo culture)
        => MemoryMarshal.GetReference<char>(culture.TextInfo.ListSeparator);
}
