using Microsoft.AspNetCore.Mvc;
using WebSocket.Abstractions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebSocket.Web.Host.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly IWebSocketServer _webSocketServer;
    private readonly ILogger<WebSocketController> _logger;
    public WebSocketController(IWebSocketServer webSocketServer, ILogger<WebSocketController> logger)
    {
        _webSocketServer = webSocketServer;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> SendMessage(string id, string message)
    {
        var connection = _webSocketServer.GetConnection(id);
        await connection.SendAsync(message);
        return Ok();
    }
}
