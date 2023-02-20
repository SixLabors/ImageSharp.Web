// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using System.Collections.Concurrent;
using SixLabors.ImageSharp.Web.Synchronization;

namespace SixLabors.ImageSharp.Web.Tests.Synchronization;

public class RefCountedConcurrentDictionaryTests
{
    private readonly ConcurrentBag<string> released;
    private readonly RefCountedConcurrentDictionary<string, string> dictionary;

    public RefCountedConcurrentDictionaryTests()
    {
        this.released = new ConcurrentBag<string>();
        this.dictionary = new RefCountedConcurrentDictionary<string, string>(
            valueFactory: this.ValueForKey,
            valueReleaser: value => this.released.Add(value));
    }

    [Fact]
    public void AddThenRelease()
    {
        this.dictionary.Get("test");
        this.ValidateDictionary(("test", 1));
        this.ValidateReleased();

        this.dictionary.Release("test");
        this.ValidateDictionary();
        this.ValidateReleased("test");
    }

    [Fact]
    public void AddTwiceThenReleaseTwice()
    {
        this.dictionary.Get("test");
        this.ValidateDictionary(("test", 1));
        this.ValidateReleased();

        this.dictionary.Get("test");
        this.ValidateDictionary(("test", 2));
        this.ValidateReleased();

        this.dictionary.Release("test");
        this.ValidateDictionary(("test", 1));
        this.ValidateReleased();

        this.dictionary.Release("test");
        this.ValidateDictionary();
        this.ValidateReleased("test");
    }

    [Fact]
    public void AddAndReleaseABunchOfKeys()
    {
        this.dictionary.Get("a");
        this.dictionary.Get("b");
        this.dictionary.Get("c");
        this.dictionary.Get("a");
        this.dictionary.Release("b");
        this.dictionary.Get("b");
        this.ValidateDictionary(("a", 2), ("b", 1), ("c", 1));
        this.ValidateReleased("b");

        this.dictionary.Release("a");
        this.dictionary.Release("b");
        this.dictionary.Release("c");
        this.ValidateDictionary(("a", 1));
        this.ValidateReleased("b", "b", "c");

        this.dictionary.Release("a");
        this.ValidateDictionary();
        this.ValidateReleased("a", "b", "b", "c");
    }

    [Fact]
    public async Task StressTest()
    {
        string[] keys = new string[] { "a", "b", "c", "d", "e", "f", "g", "h" };

        async Task Worker(int workerIndex)
        {
            var random = new Random(workerIndex);
            for (int i = 0; i < 1000; i++)
            {
                string key = keys[random.Next(0, keys.Length)];
                this.dictionary.Get(key);
                await Task.Delay(random.Next(0, 2));
                this.dictionary.Release(key);
            }
        }

        await Task.WhenAll(Enumerable.Range(0, 1000).Select(Worker));

        Assert.Empty(this.dictionary.DebugGetContents());
    }

    [Fact]
    public void ReleaseNonExistentThrows()
        => Assert.Throws<InvalidOperationException>(() => this.dictionary.Release("test"));

    [Fact]
    public void DoubleReleaseThrows()
    {
        this.dictionary.Get("test");
        this.dictionary.Release("test");
        Assert.Throws<InvalidOperationException>(() => this.dictionary.Release("test"));
    }

    [Fact]
    public void ValueFactoryIsRequired()
        => Assert.Throws<ArgumentNullException>("valueFactory", () => new RefCountedConcurrentDictionary<string, string>(null!, null));

    private string ValueForKey(string key) => $"{key}.value";

    /// <summary>
    /// Validate that the dictionary contains precisely the specified key/value/refcount tuples.
    /// </summary>
    private void ValidateDictionary(params (string Key, int RefCount)[] expectedValues)
    {
        Action<(string, string, int)> CreateElementInspector((string Key, int RefCount) expected)
            => ((string Key, string Value, int RefCount) actual) =>
            {
                Assert.Equal(expected.Key, actual.Key);
                Assert.Equal(this.ValueForKey(expected.Key), actual.Value);
                Assert.Equal(expected.RefCount, actual.RefCount);
            };

        Assert.Collection(
            collection: this.dictionary.DebugGetContents().OrderBy(v => v.Key),
            elementInspectors: expectedValues.OrderBy(v => v.Key).Select(CreateElementInspector).ToArray());
    }

    /// <summary>
    /// Validate that the specific values were released.
    /// </summary>
    private void ValidateReleased(params string[] expectedValues)
    {
        Action<string> CreateElementInspector(string expected) => (string actual) => Assert.Equal(this.ValueForKey(expected), actual);

        Assert.Collection(
            collection: this.released.OrderBy(v => v),
            elementInspectors: expectedValues.OrderBy(v => v).Select(CreateElementInspector).ToArray());
    }
}
