using System.Text;
using Languid.Server;
using Languid.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables("LANGUID_");
builder.Services.AddSingleton<ITranslationQueueService, TranslationQueueService>();
builder.Services.AddSingleton<ISocketManager, SocketManager>();
builder.Services.AddControllers();
builder.Services.AddAuthentication().AddBasicAuthentication();
builder.Services.AddHostedService<BackgroundSocketProcessor>();

var app = builder.Build();

app.UseWebSockets(new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(5)
});

app.MapControllers();
app.UseDefaultFiles();
app.UseStaticFiles();

app.Run();