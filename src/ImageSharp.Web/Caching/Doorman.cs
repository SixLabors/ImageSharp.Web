// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SixLabors.ImageSharp.Web.Caching
{
    internal sealed class Doorman
    {
        private Queue<TaskCompletionSource<Releaser>> waitingWriters;
        private TaskCompletionSource<Releaser> waitingReader;
        private int readersWaiting;
        private readonly Task<Releaser> readerReleaser;
        private readonly Task<Releaser> writerReleaser;
        private readonly string key;
        private int status;
        private readonly Action reset;

        public Doorman(string key, Action reset)
        {
            this.waitingWriters = new Queue<TaskCompletionSource<Releaser>>();
            this.waitingReader = new TaskCompletionSource<Releaser>();
            this.status = 0;

            this.key = key;
            this.readerReleaser = Task.FromResult(new Releaser(this, false));
            this.writerReleaser = Task.FromResult(new Releaser(this, true));
            this.reset = reset;
        }

        public Task<Releaser> ReaderLockAsync()
        {
            lock (this.waitingWriters)
            {
                if (this.status >= 0 && this.waitingWriters.Count == 0)
                {
                    ++this.status;
                    return this.readerReleaser;
                }
                else
                {
                    ++this.readersWaiting;
                    return this.waitingReader.Task.ContinueWith(t => t.Result);
                }
            }
        }

        public Task<Releaser> WriterLockAsync()
        {
            lock (this.waitingWriters)
            {
                if (this.status == 0)
                {
                    this.status = -1;
                    return this.writerReleaser;
                }
                else
                {
                    var waiter = new TaskCompletionSource<Releaser>();
                    this.waitingWriters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        private void ReaderRelease()
        {
            TaskCompletionSource<Releaser> toWake = null;

            lock (this.waitingWriters)
            {
                --this.status;

                if (this.status == 0)
                {
                    if (this.waitingWriters.Count > 0)
                    {
                        this.status = -1;
                        toWake = this.waitingWriters.Dequeue();
                    }
                    else
                    {
                        this.reset();
                    }
                }
            }

            if (toWake != null)
            {
                toWake.SetResult(new Releaser(this, true));
            }
        }

        private void WriterRelease()
        {
            TaskCompletionSource<Releaser> toWake = null;
            bool toWakeIsWriter = false;

            lock (this.waitingWriters)
            {
                if (this.waitingWriters.Count > 0)
                {
                    toWake = this.waitingWriters.Dequeue();
                    toWakeIsWriter = true;
                }
                else if (this.readersWaiting > 0)
                {
                    toWake = this.waitingReader;
                    this.status = this.readersWaiting;
                    this.readersWaiting = 0;
                    this.waitingReader = new TaskCompletionSource<Releaser>();
                }
                else
                {
                    this.reset();
                }
            }

            if (toWake != null)
            {
                toWake.SetResult(new Releaser(this, toWakeIsWriter));
            }
        }

        public struct Releaser : IDisposable
        {
            private readonly Doorman toRelease;
            private readonly bool writer;

            internal Releaser(Doorman toRelease, bool writer)
            {
                this.toRelease = toRelease;
                this.writer = writer;
            }

            public void Dispose()
            {
                if (this.toRelease != null)
                {
                    if (this.writer)
                    {
                        this.toRelease.WriterRelease();
                    }
                    else
                    {
                        this.toRelease.ReaderRelease();
                    }
                }
            }
        }
    }
}
