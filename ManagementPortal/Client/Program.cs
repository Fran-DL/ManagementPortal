using ManagementPortal.Client;
using ManagementPortal.Client.Configurations;
using ManagementPortal.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using MudExtensions.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configura services
builder.Services.ConfigureCustomServices();
await builder.Services.ConfigureCulture();

// Configura HttpClient
builder.Services.AddHttpClient("CustomHttpClient", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<CustomHttpClientHandler>();

builder.Services.AddMudServices();
builder.Services.AddMudExtensions();

await builder.Build().RunAsync();