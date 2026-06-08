using Microsoft.AspNetCore.SignalR;

namespace LoginFeature_TuanPM.Hubs
{
    /// <summary>
    /// SignalR hub for authentication-related real‑time communications.
    /// Currently used to manage user‑specific groups so that server-side events
    /// (e.g., forced logout) can be sent to all active connections of a given user.
    /// </summary>
    public class AuthHub : Hub
    {
        /// <summary>
        /// Adds the calling connection to a SignalR group identified by the user's ID.
        /// This allows the server to broadcast messages (like force‑logout) to every
        /// client instance belonging to the same user.
        /// </summary>
        /// <param name="userId">The GUID string of the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task JoinUserGroup(string userId)
        {
            // SignalR groups are lightweight, string‑based collections of connections.
            // Adding the current connection to the user's group enables targeted push.
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }

        /// <summary>
        /// Removes the calling connection from the specified user group.
        /// Call this when the client disconnects or the user logs out to avoid
        /// receiving further notifications.
        /// </summary>
        /// <param name="userId">The GUID string of the user.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task LeaveUserGroup(string userId)
        {
            // Cleanly detach the connection from the group to prevent stale listeners.
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, userId);
        }
    }
}
