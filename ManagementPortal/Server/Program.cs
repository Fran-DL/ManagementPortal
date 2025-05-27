using System;
using ManagementPortal.Server.Configurations;
using ManagementPortal.Server.Context;
using ManagementPortal.Server.Hubs;
using ManagementPortal.Server.Middlewares;
using ManagementPortal.Shared.Constants;
using ManagementPortal.Shared.Resources.Server;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Context;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;

builder.Services.ConfigureDatabase(configuration, builder.Environment);
builder.Services.ConfigureIdentity();
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(
        configuration.GetValue<int>("TokenSettings:PasswordResetTokenExpirationInMinutes"));
});
builder.Services.ConfigureAuthentication(configuration);
builder.Services.ConfigureCors(configuration, "CORSPolicy");
builder.Services.ConfigureSwagger(configuration);
builder.Services.ConfigureCustomServices();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddAuthorization();
builder.Services.AddLogging();

builder.Services.ConfigureHttpFactory(builder.Configuration, builder.Environment, builder.WebHost);

builder.Host.UseSerilog();

var app = builder.Build();

if (!app.Environment.IsProduction())
{
    builder.WebHost.UseUrls(ApplicationUrl.ParseUrl(configuration.GetValue<string>("applicationUrl") ?? string.Empty) ?? ApplicationUrl.Localhost);
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (configuration.GetValue<bool>("EnableHttpsRedirection"))
{
    app.UseHttpsRedirection();
}

StaticWebAssetsLoader.UseStaticWebAssets(builder.Environment, builder.Configuration);

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("CORSPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapFallbackToFile("{*path}", "index.html");

LoggingConfiguration.ConfigureLogging(builder.Configuration);

app.UseRequestLocalization();

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseMiddleware<LogMiddleware>();
});

app.UseWhen(context => context.Request.Path.StartsWithSegments("/api"), appBuilder =>
{
    appBuilder.UseMiddleware<ExceptionHandlerMiddleware>();
});

app.MapControllers();

// Comentar !app.Environment.IsProduction() si se quiere actualizar BD
if (configuration.GetValue<bool>("SeedData") && !app.Environment.IsProduction())
{
    using (var scope = app.Services.CreateScope())
    {
        using (LogContext.PushProperty("Application", "ManagementPortal"))
        {
            var services = scope.ServiceProvider;
            try
            {
                Log.Information("Inicializando Datos.");

                // Modificar el Intialize si se carga nueva BD.
                await SeedData.Initialize(services);
                Log.Information("Fin de inicialización de datos.");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, LoggingResources.DBError);
            }
        }
    }
}
else
{
    using (LogContext.PushProperty("Application", "ManagementPortal"))
    {
        Log.Information("El SEED DATA ESTÁ EN FALSE.");
    }
}

// Inicialización de los Dummy's.
using (var scope = app.Services.CreateScope())
{
    if (configuration.GetValue<bool>("SeedDummys"))
    {
        using (LogContext.PushProperty("Application", "ManagementPortal"))
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            try
            {
                logger.LogInformation("Inicializando API's de SONDA.");
                var users = await scope.ServiceProvider.GetRequiredService<ApplicationContext>().ApplicationUsers.Include(user => user.Products).ToListAsync();
                SondaDummyFactory.Singleton(users, logger);
                logger.LogInformation("FIN de Inicializando API's de SONDA.");
            }
            catch (Exception ex)
            {
                logger.LogInformation(ex, "Error al inicializar la API's de SONDA dummy");
            }
        }
    }
}

LoggingConfiguration.ConfigureLogging(builder.Configuration);

using (LogContext.PushProperty("Application", "ManagementPortal"))
{
    Log.Warning($"Aplicación iniciada en {builder.WebHost.GetSetting("urls") ?? string.Empty}");
}

app.MapHub<ChatHub>("/chathub");

app.Run();