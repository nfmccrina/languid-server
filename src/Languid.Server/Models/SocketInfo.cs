using System.Net.WebSockets;

namespace Languid.Server.Models
{
    public class SocketInfo
    {
        public SocketInfo(
            WebSocket socket,
            TaskCompletionSource<object> socketFinishedTCS,
            Task heartbeatTask
        )
        {
            Socket = socket;
            SocketFinishedTCS = socketFinishedTCS;
            HeartbeatTask = heartbeatTask;
            LastPing = DateTimeOffset.Now;
        }
        public WebSocket Socket { get; }
        public TaskCompletionSource<object> SocketFinishedTCS { get; }
        public Task HeartbeatTask { get; }
        public DateTimeOffset LastPing { get; set; }
    }
}