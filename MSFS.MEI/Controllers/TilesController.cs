using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Drawing;
using System.Net;
using System.Security.Authentication;

namespace MSFS.MEI.Controllers
{
    [ApiController]
    public class TilesController : Controller
    {
        public TilesController(IOptions<MEIConfig> mEIConfig)
        {
            _MEIConfig = mEIConfig.Value;
        }

        private readonly MEIConfig _MEIConfig;

        private static readonly HttpClient client = new(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.Brotli,
            MaxConnectionsPerServer = 32,
            SslProtocols = SslProtocols.Tls13
        })
        {
            DefaultRequestVersion = HttpVersion.Version30,
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };

        private byte[]? responseBody;

        public static ulong Count = 0, Size = 0;

        [Route("tiles/akh{quadKey}.jpeg", Order = 1)]
        public async Task<IActionResult> TilesAsync(string quadKey)
        {
            //TODO: User Agent implement
            if (_MEIConfig.HighLODEnabled)
            {
                responseBody = MergeImages(await GetImagesFromQuadKey(quadKey));
            }
            else
            {
                (int tileX, int tileY, int levelOfDetail) = QuadKeyToTileXY(quadKey);
                responseBody = await client.GetByteArrayAsync(string.Format(_MEIConfig.ImageServerURL, tileX, tileY, levelOfDetail));
            }
            Count++;
            Size += (ulong)responseBody.LongLength;
            if (responseBody != null)
            {
                return File(responseBody, "image/jpeg");
            }
            return NotFound();
        }

        private async Task<List<Image>> GetImagesFromQuadKey(string quadKey)
        {
            List<Image> images = new();
            foreach ((int tileX, int tileY, int levelOfDetail) in GetHighLODCoordinateFromQuadKey(quadKey))
            {
                images.Add((Bitmap)new ImageConverter().ConvertFrom(await client.GetByteArrayAsync(string.Format(_MEIConfig.ImageServerURL, tileX, tileY, levelOfDetail)))!);
            }
            return images;
        }

        private static byte[] MergeImages(List<Image> images)
        {
            Bitmap bitmap = new(512, 512, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            graphics.DrawImage(images[0], 0, 0);
            graphics.DrawImage(images[1], 256, 0);
            graphics.DrawImage(images[2], 0, 256);
            graphics.DrawImage(images[3], 256, 256);
            return (byte[])new ImageConverter().ConvertTo(bitmap, typeof(byte[]))!;
        }

        private static (int tileX, int tileY, int levelOfDetail) QuadKeyToTileXY(string quadKey)
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

        private static (int tileX, int tileY, int levelOfDetail)[] GetHighLODCoordinateFromQuadKey(string quadKey)
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