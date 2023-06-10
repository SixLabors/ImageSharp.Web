// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Globalization;

namespace SixLabors.ImageSharp.Web.Tests.TestUtilities;

/// <summary>
/// <see href="https://github.com/aspnet/AspNetIdentity/blob/b7826741279450c58b230ece98bd04b4815beabf/src/Microsoft.AspNet.Identity.Core/AsyncHelper.cs"/>
/// </summary>
internal static class AsyncHelper
{
    private static readonly TaskFactory TaskFactory
        = new(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

    /// <summary>
    /// Executes an async <see cref="Task"/> method synchronously.
    /// </summary>
    /// <param name="task">The task to execute.</param>
    public static void RunSync(Func<Task> task)
    {
        CultureInfo cultureUi = CultureInfo.CurrentUICulture;
        CultureInfo culture = CultureInfo.CurrentCulture;
        TaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return task();
        }).Unwrap().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes an async <see cref="Task{TResult}"/> method which has
    /// a <paramref name="task"/> return type synchronously.
    /// </summary>
    /// <typeparam name="TResult">The type of result to return.</typeparam>
    /// <param name="task">The task to execute.</param>
    /// <returns>The <typeparamref name="TResult"/>.</returns>
    public static TResult RunSync<TResult>(Func<Task<TResult>> task)
    {
        CultureInfo cultureUi = CultureInfo.CurrentUICulture;
        CultureInfo culture = CultureInfo.CurrentCulture;
        return TaskFactory.StartNew(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return task();
        }).Unwrap().GetAwaiter().GetResult();
    }
}
