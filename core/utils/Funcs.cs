using System;
using System.IO;
using System.Net;
using Godot;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Image = Godot.Image;

namespace Casanova.core.utils
{
    public static class Funcs
    {
        public static string[] ParseIpString(string ip)
        {
            try
            {
                var addy = ip.Split(":");
                var port = Vars.Networking.defaultPort;

                if (addy.Length > 1 && int.TryParse(addy[1], out var newport))
                    port = newport;

                if (!IPAddress.TryParse(addy[0], out var address))
                {
                    var ips = Dns.GetHostAddresses(addy[0]);
                    if (ips.Length > 0) addy[0] = ips[0].ToString().Split(":")[0];
                }

                return new[] {addy[0], port.ToString()};
            }
            catch (Exception)
            {
                throw new Exception($"Failed parsing IP: {ip}");
            }
        }
        
        public static Texture BlurTexture(Texture texture, int radius)
        {
            MemoryStream ms = new MemoryStream();
            SixLabors.ImageSharp.Image img = SixLabors.ImageSharp.Image.Load(texture.GetData().SavePngToBuffer());
            
            img.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Pad,
                    Size = new Size(img.Width + radius*2, img.Height)
                }).BackgroundColor(new Rgba32(255, 255, 255, 0));
                
                x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Pad,
                    Size = new Size(img.Width, img.Height + radius*2)
                }).BackgroundColor(new Rgba32(255, 255, 255, 0));
                
                x.GaussianBlur(radius);
            });

            img.SaveAsPng(ms);
            ms.Position = 0;

            Image GDimage = new Image();
            GDimage.LoadPngFromBuffer(ms.ToArray());

            var tex = new ImageTexture();
            tex.CreateFromImage(GDimage, 1);

            return tex;
        }
        
        
    }
}