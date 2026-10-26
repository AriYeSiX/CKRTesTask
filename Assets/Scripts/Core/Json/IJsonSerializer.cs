namespace CKR.Core.Json
{
    /// <summary>
    /// Seam over the JSON backend: requests stay testable and the backend can be replaced
    /// without touching the features that use it.
    /// </summary>
    public interface IJsonSerializer
    {
        T Deserialize<T>(string json);
    }
}
