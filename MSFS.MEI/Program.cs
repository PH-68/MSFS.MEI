using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Runtime;

namespace MSFS.MEI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            //GCSettings.LatencyMode = GCLatencyMode.NoGCRegion;
            TimerCallback timeCB = new((object? _) =>
            {
                GC.Collect();
                Console.Clear();
                Console.Write(string.Format("Image count:{0}, Data(MB):{1}", Controllers.TilesController.Count, Controllers.TilesController.Size / 1000000));
            });

            Timer _ = new(timeCB, null, 0, 1000);


            var builder = WebApplication.CreateBuilder(args);

            builder.WebHost.ConfigureKestrel((context, options) =>
            {
                options.ListenAnyIP(443, listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
                    listenOptions.UseHttps();
                });
            });

            // Add services to the container.

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}