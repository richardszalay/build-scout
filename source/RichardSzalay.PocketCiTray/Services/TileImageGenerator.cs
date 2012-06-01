using System;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using RichardSzalay.PocketCiTray.Infrastructure;
using System.Windows;

namespace RichardSzalay.PocketCiTray.Services
{
    public interface ITileImageGenerator
    {
        void Create(Color color, Stream outputStream);
    }

    public class TileImageGenerator : ITileImageGenerator
    {
        public TileImageGenerator(IApplicationResourceFacade applicationResourceFacade)
        {
            this.applicationResourceFacade = applicationResourceFacade;
        }

        private readonly IApplicationResourceFacade applicationResourceFacade;

        private static readonly Uri TemplateUri = new Uri("Images/Tiles/Template.png", UriKind.Relative);

        private const int TileWidth = 173;

        public void Create(Color color, Stream outputStream)
        {
            /*int colorValue = color.R << 24 |
                color.G << 16 |
                color.B << 8 |
                color.A;*/

            int colorValue = color.R << 16 |
                color.G << 8 |
                color.B |
                color.A << 24;

            var template = new WriteableBitmap(GetTemplateImage());
            var bitmap = CreateEmptyBitmap(TileWidth, TileWidth, color);

            var tileRect = new Rect(0D, 0D, TileWidth, TileWidth);

            bitmap.Blit(tileRect, template, tileRect, WriteableBitmapExtensions.BlendMode.Alpha);
            
            bitmap.SaveJpeg(outputStream, TileWidth, TileWidth, 0, 90);
        }

        private BitmapSource GetTemplateImage()
        {
            var stream = applicationResourceFacade.GetResourceStream(TemplateUri);

            BitmapImage image = new BitmapImage();
            image.SetSource(stream);

            return image;
        }

        private int AlphaBlend(int src, int dest)
        {
            int srcA = ((src >> 24) & 0xff);
            int srcR = ((src >> 16) & 0xff);
            int srcG = ((src >> 8) & 0xff);
            int srcB = ((src) & 0xff);

            int destA = ((dest >> 24) & 0xff);
            int destR = ((dest >> 16) & 0xff);
            int destG = ((dest >> 8) & 0xff);
            int destB = ((dest) & 0xff);

            int blendedA = srcA + Blend(destA, (255 - srcA));
            int blendedR = Blend(srcR, srcA) + Blend(destR, (255 - srcA));
            int blendedG = Blend(srcG, srcA) + Blend(destG, (255 - srcA));
            int blendedB = Blend(srcB, srcA) + Blend(destB, (255 - srcA));

            return (blendedA << 24) |
                (blendedR << 16) |
                (blendedG << 8) |
                (blendedB);
        }

        static int Blend(int a, int b)
        {
            return (((a * b) * 0x8081) >> 23);
        }

        private WriteableBitmap CreateEmptyBitmap(int width, int height, Color color)
        {
            int colorValue = (color.R << 16) |
                color.G << 8 |
                color.B |
                color.A << 24;


            WriteableBitmap bitmap = new WriteableBitmap(width, height);

            int[] pixels = bitmap.Pixels;

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = colorValue;
            }

            return bitmap;
        }
    }
}