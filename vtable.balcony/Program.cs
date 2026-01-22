using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using OpenCvSharp;
using System;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Vtable.Balcony.CameraServices;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Extensions;
using Vtable.Balcony.Factory;
using Vtable.Balcony.Helpers;
using Vtable.Balcony.ImageServices.Flux24bklein;
using Vtable.Balcony.Models;
using Vtable.Balcony.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Vtable.Balcony
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Find the configuration definitions
            builder.Services.Configure<AppServices>(builder.Configuration.GetSection($"{Constants.ApplicationName}:AppServices"));
            builder.Services.Configure<AppConfig>(builder.Configuration.GetSection($"{Constants.ApplicationName}:AppConfig"));

            // Add our factories
            builder.Services.AddSingleton<CameraServiceFactory>();
            builder.Services.AddSingleton<ImageServiceFactory>();
            builder.Services.AddSingleton<FluxPayloadFactory>();

            // Add all implemΑntations dynamically
            builder.Services.AddImplementationsOf<IImageService>();
            builder.Services.AddImplementationsOf<ICameraService>();
            builder.Services.AddImplementationsOf<IPayloadFactory>();
            builder.Services.Scan(s => s.FromAssembliesOf(typeof(ICredentialsFactory<>))
                .AddClasses(c => c.AssignableTo(typeof(ICredentialsFactory<>)))
                .AsImplementedInterfaces()
                .WithSingletonLifetime()
            );

            // Add generic implementations dynamically
            builder.Services.ScanGenericImplementations(typeof(ICredentialsFactory<>));
            builder.Services.ScanGenericImplementations(typeof(IConfigurationFactory<>));

            // add the background service and make it referenceable
            builder.Services.AddSingleton<CameraBackgroundService>();
            builder.Services.AddHostedService(provider => provider.GetRequiredService<CameraBackgroundService>());

            // Add known services
            builder.Services.AddSingleton<FileService>();
            builder.Services.AddSingleton<RequestsService>();

            // Add the refresh cancellation token source
            builder.Services.AddSingleton<RefreshTokenSource>();

            // Add the UI
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

            // Add logging
            builder.Logging.AddConsole();

            var app = builder.Build();
            app.MapRazorPages();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, "Static")),
                RequestPath = "/Static"
            });

            // Map HTTP calls
            MapPaths.CreateMappings(app);

            await app.RunAsync();
        }
    }
}