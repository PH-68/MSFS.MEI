using Microsoft.AspNetCore.Server.Kestrel.Core;
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

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();


            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            //GCSettings.LatencyMode = GCLatencyMode.NoGCRegion;

            timer.Change(0, 1000);

            app.Run();
        }
        public static void TimerCallback(object? state)
        {
            GC.Collect();
            Console.Clear();
            Console.Write(string.Format("[{0}]Image count:{1}, Data(MB):{2}", DateTime.Now.ToString(), Controllers.TilesController.Count, Controllers.TilesController.Size / 1000000.0));
        }
    }
}