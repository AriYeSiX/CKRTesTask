using System;

namespace CKR.Core.Net
{
    /// <summary>
    /// Raised when a transport level call succeeds but does not yield usable data.
    /// </summary>
    public class ApiException : Exception
    {
        public long StatusCode { get; }

        public ApiException(string message, long statusCode = 0) : base(message)
        {
            StatusCode = statusCode;
        }

        public ApiException(string message, Exception innerException, long statusCode = 0)
            : base(message, innerException)
        {
            StatusCode = statusCode;
        }

        public static ApiException FromResponse(string endpoint, HttpResponseData response)
        {
            return new ApiException($"Request to '{endpoint}' failed: {response}.", response.StatusCode);
        }
    }

    public sealed class JsonParseException : ApiException
    {
        public JsonParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
