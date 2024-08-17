using System.Net.WebSockets;
using System.Text;
using Languid.Server.Models;

namespace Languid.Server
{
    public interface ISocketManager
    {
        void AddSocket(WebSocket socket, TaskCompletionSource<object> socketFinishedTCS);
        Task PushTranslationAsync(string translation);
        void Stop();
        CancellationToken StoppingToken { set; }
    }

    public class SocketManager : ISocketManager
    {
        public SocketManager(ILogger<SocketManager> logger)
        {
            sockets = new Dictionary<Guid, SocketInfo>();
            stoppingToken = CancellationToken.None;
            this.logger = logger;
        }

        public void AddSocket(WebSocket socket, TaskCompletionSource<object> socketFinishedTCS)
        {
            var newId = Guid.NewGuid();

            var heartbeatTask = Task.Run(async () => {
                var buffer = new byte[1024];
                while (!stoppingToken.IsCancellationRequested) {
                    var heartbeatResponse = await socket.ReceiveAsync(buffer, stoppingToken);

                    if (heartbeatResponse.CloseStatus.HasValue)
                    {
                        CloseSocket(newId);
                        break;
                    }

                    if (sockets.ContainsKey(newId))
                    {
                        sockets[newId].LastPing = DateTimeOffset.Now;
                    }
                }
            });

            sockets.Add(newId, new SocketInfo(socket, socketFinishedTCS, heartbeatTask));
        }

        public CancellationToken StoppingToken
        {
            set
            {
                stoppingToken = value;
            }
        }

        public async Task PushTranslationAsync(string? translation)
        {
            if (string.IsNullOrEmpty(translation))
            {
                return;
            }

            foreach (var key in sockets.Keys)
            {
                SocketInfo? currentSocket;
                var socketExists = sockets.TryGetValue(key, out currentSocket);

                if (!socketExists || currentSocket == null)
                {
                    continue;
                }

                if (currentSocket.LastPing < DateTimeOffset.Now.AddMinutes(-2) || currentSocket.HeartbeatTask.IsCompleted || currentSocket.HeartbeatTask.IsCompletedSuccessfully)
                {
                    CloseSocket(key);
                    continue;
                }

                try
                {
                    await currentSocket.Socket.SendAsync(Encoding.UTF8.GetBytes(translation), WebSocketMessageType.Text, true, stoppingToken);
                }
                catch (WebSocketException ex)
                {
                    logger.LogError($"Error sending to socket (it has probably been closed normally): {ex.Message}");
                }
            }
        }

        public void Stop()
        {
            foreach (var socket in sockets.Values)
            {
                socket.SocketFinishedTCS.TrySetResult(new());
            }
        }

        private void CloseSocket(Guid key)
        {
            if (sockets.ContainsKey(key))
            {
                var socket = sockets[key];
                socket.SocketFinishedTCS.TrySetResult(new());
                sockets.Remove(key);
            }
        }

        private IDictionary<Guid, SocketInfo> sockets;
        private CancellationToken stoppingToken;
        private readonly ILogger<SocketManager> logger;
    }
}