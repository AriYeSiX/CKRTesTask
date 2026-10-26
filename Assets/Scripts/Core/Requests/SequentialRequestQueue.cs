using System;
using System.Collections.Generic;
using System.Threading;
using CKR.Core.Json;
using CKR.Core.Logging;
using CKR.Core.Net;
using Cysharp.Threading.Tasks;
using Zenject;

namespace CKR.Core.Requests
{
    /// <summary>
    /// Strictly sequential request pipeline.
    ///
    /// Design notes:
    /// * A <see cref="LinkedList{T}"/> (not a Channel/ConcurrentQueue) backs the lane, because a
    ///   pending request has to be removable from the middle of the queue in O(1).
    /// * The pump is a single long-running UniTask loop; it idles on a completion source instead of
    ///   polling, so an empty queue costs nothing per frame.
    /// * Every slot owns its own CancellationTokenSource, linked with the queue lifetime token during
    ///   execution. That is what makes "abort if sent / drop if queued" a single public operation.
    ///
    /// Thread affinity: main thread only (UniTask PlayerLoop), therefore no locking is needed.
    /// </summary>
    public sealed class SequentialRequestQueue : IRequestQueue, IInitializable, IDisposable
    {
        private readonly IWebClient _client;
        private readonly IJsonSerializer _json;
        private readonly ILogService _log;

        private readonly LinkedList<ISlot> _queue = new LinkedList<ISlot>();
        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();

        private UniTaskCompletionSource _workAvailable;
        private bool _isDisposed;

        public SequentialRequestQueue(IWebClient client, IJsonSerializer json, ILogService log)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _json = json ?? throw new ArgumentNullException(nameof(json));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        public void Initialize()
        {
            RunAsync().Forget();
        }

        public IRequestHandle<T> Enqueue<T>(IApiRequest<T> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (_isDisposed)
                throw new ObjectDisposedException(nameof(SequentialRequestQueue));

            var slot = new Slot<T>(request, this);
            slot.Node = _queue.AddLast(slot);

            _log.Info(LogTags.RequestQueue, $"enqueued '{request.Description}' (pending: {_queue.Count})");

            SignalWorkAvailable();
            return slot;
        }

        private void CancelAll()
        {
            // Copy first: cancelling mutates the list through RemoveQueued.
            var pending = new List<ISlot>(_queue);
            for (var i = 0; i < pending.Count; i++)
                pending[i].Cancel();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            _lifetime.Cancel();
            CancelAll();

            SignalWorkAvailable();

            _lifetime.Dispose();
        }

        private async UniTaskVoid RunAsync()
        {
            var token = _lifetime.Token;

            while (!token.IsCancellationRequested)
            {
                var slot = TakeNext();

                if (slot == null)
                {
                    try
                    {
                        await WaitForWorkAsync(token);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }

                    continue;
                }

                try
                {
                    // A slot never throws: it converts every outcome into a RequestResult.
                    await slot.ExecuteAsync(_client, _json, token);
                }
                catch (Exception exception)
                {
                    _log.Exception(LogTags.RequestQueue, exception);
                }
            }
        }

        private ISlot TakeNext()
        {
            var node = _queue.First;
            if (node == null)
                return null;

            var slot = node.Value;
            _queue.RemoveFirst();
            slot.Node = null;

            slot.MarkRunning();
            return slot;
        }

        private UniTask WaitForWorkAsync(CancellationToken token)
        {
            _workAvailable = _workAvailable ?? new UniTaskCompletionSource();
            return _workAvailable.Task.AttachExternalCancellation(token);
        }

        private void SignalWorkAvailable()
        {
            var source = _workAvailable;
            _workAvailable = null;
            source?.TrySetResult();
        }

        /// <summary>Called by a slot that is cancelled while still waiting for its turn.</summary>
        private void RemoveQueued(ISlot slot)
        {
            if (slot.Node == null)
                return;

            _queue.Remove(slot.Node);
            slot.Node = null;

            _log.Info(LogTags.RequestQueue, $"removed '{slot.Description}' from the queue (pending: {_queue.Count})");
        }

        private interface ISlot
        {
            string Description { get; }
            LinkedListNode<ISlot> Node { get; set; }
            void MarkRunning();
            void Cancel();
            UniTask ExecuteAsync(IWebClient client, IJsonSerializer json, CancellationToken queueToken);
        }

        private sealed class Slot<T> : ISlot, IRequestHandle<T>
        {
            private readonly IApiRequest<T> _request;
            private readonly SequentialRequestQueue _owner;
            private readonly UniTaskCompletionSource<RequestResult<T>> _completion =
                new UniTaskCompletionSource<RequestResult<T>>();
            private RequestStatus _status = RequestStatus.Queued;
            private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

            public Slot(IApiRequest<T> request, SequentialRequestQueue owner)
            {
                _request = request;
                _owner = owner;
            }

            public LinkedListNode<ISlot> Node { get; set; }

            public string Description => _request.Description;
            public bool IsCompleted => _status.IsTerminal();
            public UniTask<RequestResult<T>> ResultAsync => _completion.Task;

            public void MarkRunning()
            {
                if (!IsCompleted)
                    _status = RequestStatus.Running;
            }

            public void Cancel()
            {
                if (IsCompleted)
                    return;

                if (_status == RequestStatus.Queued)
                {
                    // Not started yet: drop it, the network is never touched.
                    _owner.RemoveQueued(this);
                    Complete(RequestResult<T>.Canceled());
                    _cancellation.Dispose();
                    return;
                }

                // Already in flight: abort the live connection, ExecuteAsync finishes the slot.
                _cancellation.Cancel();
            }

            public async UniTask ExecuteAsync(IWebClient client, IJsonSerializer json,
                CancellationToken queueToken)
            {
                if (IsCompleted)
                    return;

                if (_cancellation.IsCancellationRequested)
                {
                    Complete(RequestResult<T>.Canceled());
                    _cancellation.Dispose();
                    return;
                }

                using (var linked = CancellationTokenSource.CreateLinkedTokenSource(queueToken, _cancellation.Token))
                {
                    try
                    {
                        _owner._log.Info(LogTags.RequestQueue, $"executing '{Description}'");
                        var value = await _request.ExecuteAsync(client, json, linked.Token);
                        Complete(RequestResult<T>.Success(value));
                    }
                    catch (OperationCanceledException)
                    {
                        _owner._log.Info(LogTags.RequestQueue, $"'{Description}' canceled");
                        Complete(RequestResult<T>.Canceled());
                    }
                    catch (Exception exception)
                    {
                        _owner._log.Error(LogTags.RequestQueue,
                            $"'{Description}' failed: {exception.Message}");
                        Complete(RequestResult<T>.Failure(exception));
                    }
                }

                _cancellation.Dispose();
            }

            private void Complete(RequestResult<T> result)
            {
                if (IsCompleted)
                    return;

                _status = result.Status;
                _completion.TrySetResult(result);
            }
        }
    }
}
