using System;

namespace CKR.Core.Requests
{
    public enum RequestStatus
    {
        /// <summary>Waiting in the queue, can still be removed without ever hitting the network.</summary>
        Queued,

        /// <summary>Currently executing, cancellation aborts the live connection.</summary>
        Running,

        Succeeded,
        Failed,
        Canceled
    }

    public static class RequestStatusExtensions
    {
        public static bool IsTerminal(this RequestStatus status)
        {
            return status == RequestStatus.Succeeded
                   || status == RequestStatus.Failed
                   || status == RequestStatus.Canceled;
        }
    }

    /// <summary>
    /// Result of a queued request. Cancellation is a normal outcome rather than an exception,
    /// which keeps call sites free of try/catch noise.
    /// </summary>
    public readonly struct RequestResult<T>
    {
        public readonly RequestStatus Status;
        public readonly T Value;
        public readonly Exception Error;

        private RequestResult(RequestStatus status, T value, Exception error)
        {
            Status = status;
            Value = value;
            Error = error;
        }

        public bool IsSuccess => Status == RequestStatus.Succeeded;
        public bool IsCanceled => Status == RequestStatus.Canceled;
        public bool IsFailed => Status == RequestStatus.Failed;

        public static RequestResult<T> Success(T value)
        {
            return new RequestResult<T>(RequestStatus.Succeeded, value, null);
        }

        public static RequestResult<T> Failure(Exception error)
        {
            return new RequestResult<T>(RequestStatus.Failed, default, error);
        }

        public static RequestResult<T> Canceled()
        {
            return new RequestResult<T>(RequestStatus.Canceled, default, null);
        }
    }
}
