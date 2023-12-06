namespace WebSocket.Abstractions;

/// <summary>
/// WebSocket连接
/// </summary>
public interface IWebSocketConnection
{
    /// <summary>
    /// 唯一Id
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// 发送信息
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendAsync(string msg, CancellationToken cancellationToken = default);

    /// <summary>
    /// 关闭连接
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CloseAsync(CancellationToken cancellationToken = default);
}
