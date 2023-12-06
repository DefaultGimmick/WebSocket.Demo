using Microsoft.AspNetCore.Mvc;
using WebSocket.Abstractions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebSocket.Web.Host.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class WebSocketSendMessgeController : ControllerBase
{
    private readonly IWebSocketServer _webSocketServer;
    private readonly ILogger<WebSocketSendMessgeController> _logger;
    public WebSocketSendMessgeController(IWebSocketServer webSocketServer, ILogger<WebSocketSendMessgeController> logger)
    {
        _webSocketServer = webSocketServer;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> PushMessage(string Id, string message)
    {
        var connection = _webSocketServer.GetConnection(Id);
        await connection.SendAsync(message);
        return Ok();
    }
}
