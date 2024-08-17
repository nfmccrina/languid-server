using System.Net.WebSockets;
using System.Text;
using Languid.Server.Models;
using Languid.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Languid.Server.Controllers
{
    public class TranslationController : ControllerBase
    {
        private readonly ISocketManager socketManager;
        private readonly ITranslationQueueService translationQueueService;

        public TranslationController(
            ISocketManager socketManager,
            ITranslationQueueService translationQueueService)
        {
            this.socketManager = socketManager;
            this.translationQueueService = translationQueueService;
        }

        [HttpGet("/translation")]
        public async Task Get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

                var socketFinishedTCS = new TaskCompletionSource<object>();
                socketManager.AddSocket(webSocket, socketFinishedTCS);

                await socketFinishedTCS.Task;
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        [Authorize]
        [HttpPost("/translation")]
        public IActionResult Post([FromBody] TranslationDto body)
        {
            this.translationQueueService.Push(body.Translation);
            return Created();
        }
    }
}