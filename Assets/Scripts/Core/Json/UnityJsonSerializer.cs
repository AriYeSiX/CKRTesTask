using System;
using CKR.Core.Net;
using UnityEngine;

namespace CKR.Core.Json
{
    /// <summary>
    /// JsonUtility based implementation: no extra dependencies, sufficient for the typed DTOs here.
    /// </summary>
    public sealed class UnityJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new JsonParseException($"Empty payload while parsing '{typeof(T).Name}'.", null);

            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception exception)
            {
                throw new JsonParseException($"Cannot parse '{typeof(T).Name}'.", exception);
            }
        }
    }
}
