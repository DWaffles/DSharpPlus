﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// This is somewhat inspired by Discord.NET 
// asynchronous event code

namespace DSharpPlus
{
    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <returns>Event handling task.</returns>
    public delegate Task AsyncEventHandler();

    /// <summary>
    /// Represents an asynchronous event handler.
    /// </summary>
    /// <typeparam name="T">Type of EventArgs for the event.</typeparam>
    /// <returns>Event handling task.</returns>
    public delegate Task AsyncEventHandler<T>(T e) where T : EventArgs;

    /// <summary>
    /// Represents an asynchronously-handled event.
    /// </summary>
    public sealed class AsyncEvent
    {
        private readonly object _lock = new object();
        private List<AsyncEventHandler> Handlers { get; }
        private Action<string, Exception> ErrorHandler { get; }
        private string EventName { get; }

        public AsyncEvent(Action<string, Exception> errhandler, string event_name)
        {
            this.Handlers = new List<AsyncEventHandler>();
            this.ErrorHandler = errhandler;
            this.EventName = event_name;
        }

        public void Register(AsyncEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Add(handler);
        }

        public void Unregister(AsyncEventHandler handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Remove(handler);
        }

        public async Task InvokeAsync()
        {
            var handlers = (AsyncEventHandler[])null;
            lock (this._lock)
                handlers = this.Handlers.ToArray();

            if (!handlers.Any())
                return;

            var exs = new List<Exception>(handlers.Length);
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i]().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                }
            }

            if (exs.Any())
                this.ErrorHandler(this.EventName, new AggregateException("Exceptions occured within one or more event handlers. Check InnerExceptions for details.", exs));
        }
    }

    /// <summary>
    /// Represents an asynchronously-handled event.
    /// </summary>
    /// <typeparam name="T">Type of EventArgs for this event.</typeparam>
    public sealed class AsyncEvent<T> where T : EventArgs
    {
        private readonly object _lock = new object();
        private List<AsyncEventHandler<T>> Handlers { get; }
        private Action<string, Exception> ErrorHandler { get; }
        private string EventName { get; }

        public AsyncEvent(Action<string, Exception> errhandler, string event_name)
        {
            this.Handlers = new List<AsyncEventHandler<T>>();
            this.ErrorHandler = errhandler;
            this.EventName = event_name;
        }

        public void Register(AsyncEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Add(handler);
        }

        public void Unregister(AsyncEventHandler<T> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler), "Handler cannot be null");

            lock (this._lock)
                this.Handlers.Remove(handler);
        }

        public async Task InvokeAsync(T e)
        {
            var handlers = (AsyncEventHandler<T>[])null;
            lock (this._lock)
                handlers = this.Handlers.ToArray();

            if (!handlers.Any())
                return;

            var exs = new List<Exception>(handlers.Length);
            for (var i = 0; i < handlers.Length; i++)
            {
                try
                {
                    await handlers[i](e).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    exs.Add(ex);
                }
            }

            if (exs.Any())
                this.ErrorHandler(this.EventName, new AggregateException("Exceptions occured within one or more event handlers. Check InnerExceptions for details.", exs));
        }
    }
}
