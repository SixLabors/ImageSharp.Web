// Copyright (c) Six Labors.
// Licensed under the Six Labors Split License.

using SixLabors.ImageSharp.Web.Commands;

namespace SixLabors.ImageSharp.Web.Tests.Commands;

public class CommandCollectionTests
{
    [Fact]
    public void CanAddCommands()
    {
        CommandCollection collection = new();
        collection.Add(new("a", "b"));
        Assert.Single(collection);
    }

    [Fact]
    public void CannotAddDuplicateCommands()
    {
        CommandCollection collection = new();
        collection.Add(new("a", "b"));
        Assert.Throws<ArgumentException>(() => collection.Add(new("a", "b")));
        Assert.Single(collection);
    }

    [Fact]
    public void CanReadCommands()
    {
        const string key = "a";
        const string value = "b";
        CommandCollection collection = new();
        collection.Add(new(key, value));

        Assert.Equal(value, collection[key]);
    }

    [Fact]
    public void CanInsertCommands()
    {
        KeyValuePair<string, string> kv1 = new("a", "b");
        KeyValuePair<string, string> kv2 = new("c", "d");

        CommandCollection collection = new();
        collection.Add(kv1);
        Assert.Single(collection);

        collection.Insert(0, kv2);
        Assert.Equal(2, collection.Count);

        Assert.Equal(kv1.Value, collection[kv1.Key]);
        Assert.Equal(kv2.Value, collection[kv2.Key]);

        Assert.Equal(1, collection.IndexOf(kv1));
        Assert.Equal(0, collection.IndexOf(kv2));
    }

    [Fact]
    public void CannotInsertDuplicateCommands()
    {
        CommandCollection collection = new();
        collection.Add(new("a", "b"));
        Assert.Throws<ArgumentException>(() => collection.Insert(0, new("a", "b")));
        Assert.Single(collection);
    }

    [Fact]
    public void CanInsertCommandsViaKey()
    {
        KeyValuePair<string, string> kv1 = new("a", "b");
        KeyValuePair<string, string> kv2 = new("c", "d");

        CommandCollection collection = new();
        collection.Add(kv1);
        Assert.Single(collection);

        collection.Insert(0, kv2.Key, kv2.Value);
        Assert.Equal(2, collection.Count);

        Assert.Equal(kv1.Value, collection[kv1.Key]);
        Assert.Equal(kv2.Value, collection[kv2.Key]);

        Assert.Equal(1, collection.IndexOf(kv1));
        Assert.Equal(0, collection.IndexOf(kv2));
    }

    [Fact]
    public void CannotInsertDuplicateCommandsViaKey()
    {
        CommandCollection collection = new();
        collection.Add(new("a", "b"));
        Assert.Throws<ArgumentException>(() => collection.Insert(0, "a", "b"));
        Assert.Single(collection);
    }

    [Fact]
    public void CanSetCommandsViaIndex()
    {
        KeyValuePair<string, string> kv1 = new("a", "b");
        KeyValuePair<string, string> kv2 = new("c", "d");
        CommandCollection collection = new();

        collection.Add(kv1);
        Assert.Single(collection);
        Assert.Equal(0, collection.IndexOf(kv1));
        Assert.Equal(kv1.Value, collection[kv1.Key]);

        collection[0] = kv2;
        Assert.Single(collection);
        Assert.Equal(0, collection.IndexOf(kv2));
        Assert.Equal(kv2.Value, collection[kv2.Key]);
    }

    [Fact]
    public void CanSetCommandsViaKey()
    {
        KeyValuePair<string, string> kv1 = new("a", "b");
        KeyValuePair<string, string> kv2 = new("a", "d");
        CommandCollection collection = new();

        collection.Add(kv1);
        Assert.Single(collection);
        Assert.Equal(0, collection.IndexOf(kv1));
        Assert.Equal(kv1.Value, collection[kv1.Key]);

        collection[kv1.Key] = kv2.Value;
        Assert.Single(collection);
        Assert.Equal(0, collection.IndexOf(kv2));
        Assert.Equal(kv2.Value, collection[kv2.Key]);
    }

    [Fact]
    public void CanRemoveCommands()
    {
        KeyValuePair<string, string> kv1 = new("a", "b");
        KeyValuePair<string, string> kv2 = new("c", "d");
        CommandCollection collection = new();

        collection.Add(kv1);
        collection.Add(kv2);

        Assert.Equal(2, collection.Count);

        collection.Remove(kv1);

        Assert.Single(collection);
        Assert.Equal(kv2.Value, collection[kv2.Key]);
    }

    [Fact]
    public void CanRemoveCommandsViaKey()
    {
        KeyValuePair<string, string> kv1 = new("a", "b");
        KeyValuePair<string, string> kv2 = new("c", "d");
        CommandCollection collection = new();

        collection.Add(kv1);
        collection.Add(kv2);

        Assert.Equal(2, collection.Count);

        collection.Remove(kv1.Key);

        Assert.Single(collection);
        Assert.Equal(kv2.Value, collection[kv2.Key]);
    }

    [Fact]
    public void CanClearCommands()
    {
        KeyValuePair<string, string> kv1 = new("a", "b");
        KeyValuePair<string, string> kv2 = new("c", "d");
        CommandCollection collection = new();

        collection.Add(kv1);
        collection.Add(kv2);

        Assert.Equal(2, collection.Count);

        collection.Clear();

        Assert.Empty(collection);
    }

    [Fact]
    public void KeysAreOrdered()
    {
        string[] keys = new[] { "a", "b", "c", "d" };

        CommandCollection collection = new();
        foreach (string key in keys)
        {
            collection.Insert(0, new(key, null));
        }

        int index = 0;
        int offset = keys.Length - 1;
        foreach (string key in collection.Keys)
        {
            Assert.Equal(index++, collection.IndexOf(key));
            Assert.Equal(keys[offset--], key);
        }
    }

    [Fact]
    public void GetByInvalidKeyThrowsCorrectly()
        => Assert.Throws<KeyNotFoundException>(() =>
        {
            CommandCollection collection = new();
            return collection["a"];
        });
}
