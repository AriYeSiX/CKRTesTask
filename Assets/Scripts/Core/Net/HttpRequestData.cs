using System;
using System.Collections.Generic;

namespace CKR.Core.Net
{
    public enum HttpMethodType
    {
        Get,
        Post,
        Put,
        Delete
    }

    /// <summary>
    /// Immutable description of a single HTTP call, independent from the transport that executes it.
    /// </summary>
    public readonly struct HttpRequestData
    {
        public readonly HttpMethodType Method;
        public readonly string Url;
        public readonly string Body;
        public readonly IReadOnlyList<HttpHeader> Headers;

        public HttpRequestData(HttpMethodType method, string url, string body = null,
            IReadOnlyList<HttpHeader> headers = null)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Url must not be empty.", nameof(url));

            Method = method;
            Url = url;
            Body = body;
            Headers = headers;
        }

        public static HttpRequestData Get(string url, IReadOnlyList<HttpHeader> headers = null)
        {
            return new HttpRequestData(HttpMethodType.Get, url, null, headers);
        }
    }

    public readonly struct HttpHeader
    {
        public readonly string Name;
        public readonly string Value;

        public HttpHeader(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}
