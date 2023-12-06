using Microsoft.AspNetCore.Builder;


namespace WebSocket.Abstractions;

public class WebSocketServerOptions
{
    public WebSocketOptions WebSocketOptions { get; private set; }

    public void ConfigwebSocketOptions(Action<WebSocketOptions> action)
    {
        WebSocketOptions options = new WebSocketOptions();
        action(options);
        WebSocketOptions = options;
    }

    public Func<IServiceProvider, IWebSocketConnection, Task> OnConnected { get; set; }

    public Func<IServiceProvider, IWebSocketConnection, string, Task> OnReceive { get; set; }

    public Func<IServiceProvider, IWebSocketConnection, string, Task> OnSend { get; set; }
}
