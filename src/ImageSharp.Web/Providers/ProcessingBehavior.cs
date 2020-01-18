// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

namespace SixLabors.ImageSharp.Web.Providers
{
    /// <summary>
    /// Enumerates the possible processing behaviors.
    /// </summary>
    public enum ProcessingBehavior
    {
        /// <summary>
        /// The image will be processed only when commands are supplied.
        /// </summary>
        CommandOnly,

        /// <summary>
        /// The image will always be processed.
        /// </summary>
        All
    }
}
