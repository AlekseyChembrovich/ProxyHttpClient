using WebApp.Proxy.Provider;
using OkParser.Infrastructure.Client.Common.Proxy.DI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IProxyProvider, ProxyProvider>();
builder.Services.AddProxyHttpClient("proxy-client")
    .ConfigureProxyProvider(provider => provider.GetRequiredService<IProxyProvider>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
