using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Vtable.Balcony.Configuration;
using Vtable.Balcony.Services;

namespace Vtable.Balcony.Helpers
{
    public static class MapPaths
    {
        public static void CreateMappings(WebApplication app)
        {
            // Apply a nonce to all requests
            app.Use(async (context, next) =>
            {
                var nonce = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
                context.Items["CSPNonce"] = nonce;
                context.Response.Headers["Content-Security-Policy"] =
                    $"script-src 'self' 'nonce-{nonce}'; style-src 'self' 'nonce-{nonce}';";
                await next();
            });

            // Enfoce CSP
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "DENY";
                context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["X-XSS-Protection"] = "0";
                await next();
            });

            // Force generation of a new image
            app.MapGet("/gennew", async (HttpContext context, IOptions<AppConfig> appConfig, RefreshTokenSource tokenSource) =>
            {
                if (context.Request.Query.ContainsKey("key") && context.Request.Query["key"] != appConfig.Value.PrivilegedKey)
                    return Results.Empty;

                tokenSource.Trigger();

                return Results.Ok("Done!");
            });

            // Force generation of a new image
            app.MapGet("/delete/{image}", async (HttpContext context, IOptions<AppConfig> appConfig, RequestsService requests, string image) =>
            {
                if (context.Request.Query.ContainsKey("key") && context.Request.Query["key"] != appConfig.Value.PrivilegedKey)
                    return Results.Empty;

                requests.DeleteImage(image);

                return Results.Ok("Done!");
            });

            // Get the last generated image
            app.MapGet("/gen", GenerateAsync);

            // Get the last snapshot
            app.MapGet("/snapshot", SnapshotAsync);

            // Signal the front-end if at least one image has been generated
            app.MapGet("/ready", CheckIfReady);

            // Get a previously generated image
            app.MapGet("/generated/{image}", async (HttpContext context, RequestsService request, string image) =>
            {
                return Results.File(await request.GetGeneratedImage(image, false), "image/jpeg");
            });

            // Get a previously generated image thumbnail
            app.MapGet("/generated_thumb/{image}", async (HttpContext context, RequestsService request, string image) =>
            {
                return Results.File(await request.GetGeneratedImage(image, true), "image/jpeg");
            });

            // Get a previous snapshot
            app.MapGet("/snapshots/{image}", async (HttpContext context, RequestsService request, string image) =>
            {
                return Results.File(await request.GetCameraSnapshot(image, false), "image/jpeg");
            });

            // Get a previous snapshot thumbnail
            app.MapGet("/snapshots_thumb/{image}", async (HttpContext context, RequestsService request, string image) =>
            {
                return Results.File(await request.GetCameraSnapshot(image, true), "image/jpeg");
            });
        }

        private static async Task<IResult> GenerateAsync(HttpContext context, RequestsService request)
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var image = request.GetNewestGeneratedImage();

            if (image is null)
                return Results.Problem(new ProblemDetails
                {
                    Detail = "The image has not been generated yet. Try again in a few seconds",
                    Status = 503,
                    Title = "Image currently unavailable"
                });

            return Results.File(image, "image/jpeg");
        }

        private static async Task<IResult> SnapshotAsync(HttpContext context, RequestsService request)
        {
            var image = request.GetNewestCameraSnapshot();

            if (image is null)
                return Results.Problem(new ProblemDetails
                {
                    Detail = "The image has not been captured yet. Try again in a few seconds",
                    Status = 503,
                    Title = "Image currently unavailable"
                });

            return Results.File(image, "image/jpeg");
        }


        private static async Task CheckIfReady(HttpContext context, RequestsService request, IOptions<AppConfig> config)
        {
            context.Response.Headers.ContentType = "text/event-stream";


            while (true)
            {
                var ready = request.GetNewestGeneratedImage() is not null && request.GetNewestCameraSnapshot() is not null;

                if (ready)
                {
                    string dataFormat = @"data: {{""theme"": ""{0}"", ""nextUpdate"": ""{1}""}}";
                    var span = TimeSpan.FromMinutes(config.Value.RefreshPeriodMinutes).Subtract(DateTimeOffset.Now.Subtract(request.LastGeneratedImageTtimestamp!.Value));
                    var format = string.Format(dataFormat, request.LastGeneratedImageTheme ?? "(Please wait)", span.ToString("hh\\:mm"));

                    await context.Response.WriteAsync(format);
                    await context.Response.WriteAsync("\n\n");
                    await context.Response.Body.FlushAsync();
                    return; // close the SSE connection
                }

                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }
}
