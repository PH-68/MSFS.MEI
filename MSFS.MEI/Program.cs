using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime;

namespace MSFS.MEI
{
    public class Program
    {

        public static Timer timer = new(TimerCallback);

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel((context, options) =>
            {
                options.ListenAnyIP(443, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                    listenOptions.UseHttps("./msfs2020-server-cert.pfx", "password");
                });
            });

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddOptions();

            builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

            builder.Services.Configure<MEIConfig>(builder.Configuration.GetSection("MEIConfig"));

            var app = builder.Build();

            app.MapReverseProxy();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;

            app.Use(async (context, next) =>
            {
                string[] blackList = { "tsom_cc_activation_masks", "texture_synthesis_online_map_high_res", "color_corrected_images" };

                if (context.Request.Path.HasValue && (blackList.Any(context.Request.Path.Value!.Contains) || (context.Request.Path.Value.Contains("/coverage_maps/") && !context.Request.Path.Value.Contains("tin_extensions"))))
                {
                    context.Response.StatusCode = 404;
                }
                else
                {
                    await next.Invoke();
                }


            });

#if !RELEASE
            app.Use(async (context, next) =>
            {
                if (!context.Request.Path.Value!.Contains("akh", StringComparison.CurrentCulture))
                {
                    Trace.WriteLine(context.Request.Host + context.Request.Path);
                }
                await next.Invoke();
            });
#endif

            timer.Change(0, 1000);

            app.Run();
        }
        public static void TimerCallback(object? state)
        {
            GC.Collect();
            Console.Clear();
            Console.Write($"[{DateTime.Now}]Image count:{Controllers.TilesController.Count}, Data usage(MB):{Controllers.TilesController.Size / 1000000.0}");
        }
    }
}