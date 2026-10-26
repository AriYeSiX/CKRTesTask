using System.Text;

namespace CKR.Core.Net
{
    /// <summary>
    /// Raw transport level answer. Never throws by itself: whether a response counts as an error
    /// is decided by the request that issued it.
    ///
    /// The payload is kept as bytes — text endpoints decode it on demand through <see cref="Body"/>,
    /// binary ones read <see cref="RawData"/> directly.
    /// </summary>
    public readonly struct HttpResponseData
    {
        public readonly long StatusCode;
        public readonly byte[] RawData;

        private readonly string _transportError;
        private readonly string _bodyText;

        private HttpResponseData(long statusCode, byte[] rawData, string bodyText, string transportError)
        {
            StatusCode = statusCode;
            RawData = rawData;
            _transportError = transportError;
            _bodyText = bodyText;
        }

        public static HttpResponseData Completed(long statusCode, byte[] rawData)
        {
            return new HttpResponseData(statusCode, rawData, null, null);
        }

        public static HttpResponseData Failed(long statusCode, string bodyText, string transportError)
        {
            return new HttpResponseData(statusCode, null, bodyText, transportError);
        }

        public string Body
        {
            get
            {
                if (_bodyText != null)
                    return _bodyText;

                return RawData != null && RawData.Length > 0
                    ? Encoding.UTF8.GetString(RawData)
                    : null;
            }
        }

        public bool IsSuccess => string.IsNullOrEmpty(_transportError) && StatusCode >= 200 && StatusCode < 300;

        /// <summary>
        /// Transport failures, throttling and server side errors are worth another attempt;
        /// client errors (4xx except 408/429) are not.
        /// </summary>
        public bool IsRetryable
        {
            get
            {
                if (IsSuccess)
                    return false;

                if (!string.IsNullOrEmpty(_transportError))
                    return true;

                return StatusCode == 0 || StatusCode == 408 || StatusCode == 429 || StatusCode >= 500;
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(_transportError)
                ? "HTTP " + StatusCode
                : "HTTP " + StatusCode + " (" + _transportError + ")";
        }
    }
}
