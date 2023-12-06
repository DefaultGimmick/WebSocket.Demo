using Microsoft.AspNetCore.Hosting;
using WebSocket.Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddLogging();

// ���Websocket����
builder.Services.AddWebSocketServer(options =>
{
    options.ConfigwebSocketOptions(option =>
    {
        option.KeepAliveInterval = TimeSpan.FromSeconds(5);
    });

    options.OnConnected = async (serviceProvider, connection) =>
    {
        //var logger =  serviceProvider.GetService<ILoggerFactory>().CreateLogger(typeof(Program));
        // logger.LogInformation("{Time} WebSocket �����Ѵ�: {Id}", DateTime.Now, connection.Id);
        await Task.CompletedTask;
    };

    options.OnReceive = async (serviceProvider, connection, message) =>
    {
        var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(typeof(Program));
        logger.LogInformation("{Time} �յ���Ϣ: {Id} {Message}", DateTime.Now, connection.Id, message);
        await Task.CompletedTask;
    };

    options.OnSend = async (serviceProvider, connection, message) =>
    {
        var logger = serviceProvider.GetService<ILoggerFactory>().CreateLogger(typeof(Program));
        logger.LogInformation("{Time} ������Ϣ: {Id} {Message}", DateTime.Now, connection.Id, message);
        await Task.CompletedTask;
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();
app.UseWebSocketServer();
app.Run();
