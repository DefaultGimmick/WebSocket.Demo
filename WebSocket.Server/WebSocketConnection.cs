using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using WebSocket.Abstractions;


namespace WebSocket.Server;

public class WebSocketConnection : IWebSocketConnection, IDisposable
{
    private readonly WebSocketServerOptions _webSocketServerOption;
    private readonly IServiceProvider _serviceProvider;
    private readonly WebSocketServer _webSocketServer;
    public WebSocketConnection(WebSocketServer webSocketServer, System.Net.WebSockets.WebSocket webSocket, string id, IServiceProvider serviceProvider)
    {
        _webSocketServer = webSocketServer;
        WebSocket = webSocket;
        Id = id;
        _serviceProvider = serviceProvider;
        _webSocketServerOption = serviceProvider.GetService<IOptions<WebSocketServerOptions>>().Value;
    }

    public readonly System.Net.WebSockets.WebSocket WebSocket;

    public string Id { get; set; }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await WebSocket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, cancellationToken);
        WebSocket.Dispose();
        _webSocketServer.RemoveConnection(this);
    }

    public async Task SendAsync(string msg, CancellationToken cancellationToken = default)
    {
        await WebSocket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(msg)), WebSocketMessageType.Text, true, cancellationToken);
        if (_webSocketServerOption.OnSend != null)
        {
            await _webSocketServerOption.OnSend(_serviceProvider, this, msg);
        }
    }

    public void Dispose()
    {
        ((IDisposable)WebSocket).Dispose();
    }
}
