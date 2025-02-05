using LChat.GUI;
using LChat.GUI.ChatService;
using LChat.GUI.Data;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOptions<ConnectionOptions>()
                .Bind(builder.Configuration.GetSection("Connection"));
builder.Services.AddScoped(sp => new HttpClient 
                                 {
                                    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) 
                                 });
builder.Services.AddSingleton<IChatService, ChatService>();

await builder.Build().RunAsync();
