// Copyright (c) Six Labors.
// Licensed under the Apache License, Version 2.0.

using System.Collections.Generic;
using SixLabors.ImageSharp.Web.Commands;
using Xunit;

namespace SixLabors.ImageSharp.Web.Tests.Commands
{
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
    }
}
