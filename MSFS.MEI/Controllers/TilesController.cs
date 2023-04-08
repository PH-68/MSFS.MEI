using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Runtime;
using System.Text.Json;

namespace MSFS.MEI.Controllers
{
    [ApiController]
    public class TilesController : Controller
    {
        static readonly HttpClient client = new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip,
            MaxConnectionsPerServer = 10,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls13
        })
        {
            DefaultRequestVersion = HttpVersion.Version30,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
        };

        private byte[]? responseBody;

        public static ulong Count, Size = 0;

        [Route("tiles/akh{path}.jpeg")]
        public async Task<IActionResult> TilesAsync(string path)
        {
            var (tileX, tileY, levelOfDetail) = QuadKeyToTileXY(path);
            using HttpResponseMessage response = await client.GetAsync(string.Format("https://khms.google.com/kh/v=939?x={0}&y={1}&z={2}", tileX, tileY, levelOfDetail));
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsByteArrayAsync();
            response.Dispose();
            Count++;
            Size += (ulong)responseBody.LongLength;
            return File(responseBody, "image/jpeg");
        }


        public static (int tileX, int tileY, int levelOfDetail) QuadKeyToTileXY(string quadKey)
        {
            int tileX = 0;
            int tileY = 0;
            int levelOfDetail = quadKey.Length;
            for (int i = levelOfDetail; i > 0; i--)
            {
                int mask = 1 << i - 1;
                switch (quadKey[levelOfDetail - i])
                {
                    case '1':
                        tileX |= mask;
                        break;
                    case '2':
                        tileY |= mask;
                        break;
                    case '3':
                        tileX |= mask;
                        tileY |= mask;
                        break;
                    default:
                        throw new ArgumentException("Invalid QuadKey digit sequence.");
                    case '0':
                        break;
                }
            }
            return (tileX, tileY, levelOfDetail);
        }

        public static (int tileX, int tileY, int levelOfDetail)[] GetNextLevel(string quadKey)
        {
            var (x, y, z) = QuadKeyToTileXY(quadKey);
            return new (int tileX, int tileY, int levelOfDetail)[4]
            {
            (x * 2, y * 2, z + 1),
            (x * 2 + 1, y * 2, z + 1),
            (x * 2, y * 2 + 1, z + 1),
            (x * 2 + 1, y * 2 + 1, z + 1)
            };
        }
    }
}
