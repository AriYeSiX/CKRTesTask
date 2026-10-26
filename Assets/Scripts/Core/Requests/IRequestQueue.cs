namespace CKR.Core.Requests
{
    /// <summary>
    /// Single lane request pipeline: requests run strictly one after another, the next one starts
    /// only when the previous one has finished (succeeded, failed or canceled).
    /// </summary>
    public interface IRequestQueue
    {
        /// <summary>
        /// Appends the request to the lane and returns a handle carrying both its future result
        /// and the ability to cancel it.
        /// </summary>
        IRequestHandle<T> Enqueue<T>(IApiRequest<T> request);
    }
}
