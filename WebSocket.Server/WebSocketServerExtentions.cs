using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.WebSockets;
using System.Text;
using WebSocket.Abstractions;

namespace WebSocket.Server;

public static class WebSocketServerExtentions
{
    private static IBusControl _busControl;

    public static IServiceCollection AddWebSocketServer(this IServiceCollection services, Action<WebSocketServerOptions> builder)
    {
        services.Configure(builder);
        services.AddSingleton<WebSocketServer>();
        services.AddSingleton<IWebSocketServer>(serviceProvider => serviceProvider.GetService<WebSocketServer>());
        return services;
    }

    public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder app)
    {
        IServiceProvider serviceProvider = app.ApplicationServices;

        ILogger logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(typeof(WebSocketServerExtentions));

        WebSocketServer webSocketServer = serviceProvider.GetService<WebSocketServer>();

        WebSocketServerOptions options = serviceProvider.GetService<IOptions<WebSocketServerOptions>>().Value;
        if (options.WebSocketOptions != null)
        {
            app.UseWebSockets(options.WebSocketOptions);
        }
        else
        {
            app.UseWebSockets();
        }

        IHostApplicationLifetime hostApplicationLifetime = serviceProvider.GetService<IHostApplicationLifetime>();

        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        hostApplicationLifetime.ApplicationStopping.Register(() => cancellationTokenSource.Cancel());

        app.Use(async (context, next) =>
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using System.Net.WebSockets.WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                WebSocketConnection connection = new WebSocketConnection(webSocketServer, webSocket,
                    Guid.NewGuid().ToString(), serviceProvider);

                if (options.OnConnected != null)
                {
                    await options.OnConnected(serviceProvider, connection);
                }

                webSocketServer.AddConnection(connection);

                logger.LogInformation("收到新的连接{ConnectionId}当前连接数:{Count}", connection.Id, webSocketServer.ConnectionCount);

                List<byte> bytes = new List<byte>();
                try
                {
                    while (true)
                    {
                        var buffer = new byte[1024];
                        WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationTokenSource.Token);
                        if (result.CloseStatus.HasValue)
                        {
                            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationTokenSource.Token);

                            break;
                        }

                        bytes.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                        if (result.EndOfMessage)
                        {
                            var body = Encoding.UTF8.GetString(bytes.ToArray());

                            await _busControl.Publish(new MessageReceiveEvnet
                            {
                                ConnectionId = connection.Id,
                                Body = body
                            });
                            bytes.Clear();
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (ex is WebSocketException webSocketException && webSocketException.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                    {

                    }
                    else
                    {
                        logger.LogError(ex, ex.Message);
                    }
                }
                finally
                {
                    webSocketServer.RemoveConnection(connection);
                    logger.LogInformation("连接关闭[{ConnectionId}]当前连接数:{Count}", connection.Id, webSocketServer.ConnectionCount);
                }
            }
            else
            {
                await next();
            }
        });

        _busControl = Bus.Factory.CreateUsingInMemory(sbc =>
        {
            sbc.ReceiveEndpoint(ep =>
            {
                ep.Handler<MessageReceiveEvnet>(async context =>
                {
                    var message = context.Message;
                    try
                    {
                        if (options.OnReceive != null)
                        {
                            await options.OnReceive(serviceProvider, webSocketServer.GetConnection(message.ConnectionId), message.Body);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "处理数据异常 ConnectionId:{ConnectionId} Body:{Body}", message.ConnectionId, message);
                    }
                });
            });
        });
        _busControl.Start();

        return app;
    }
}
