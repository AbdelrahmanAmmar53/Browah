namespace Berwah.BL.Service.IService
{
    /// <summary>
    /// Pushes a stored notification to the user's open app, the moment it is made.
    /// Implemented by the web project (SignalR); the business layer only knows this.
    /// </summary>
    public interface INotificationPusher
    {
        Task PushAsync(int userId, object payload);
    }
}
