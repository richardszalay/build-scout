using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Media.Imaging;

namespace RichardSzalay.PocketCiTray.Services
{
    public class TileImageCreator
    {
        private readonly IApplicationResourceFacade applicationResourceFacade;

        private static readonly Uri TemplateUri = new Uri("/Images/Tiles/Template.png", UriKind.Relative);

        private const int TileWidth = 173;

        public void Create(Color color, Stream outputStream)
        {
            int colorValue = (color.R << 24) |
                color.G << 16 |
                color.B << 8 |
                color.A;


            WriteableBitmap template = new WriteableBitmap(GetTemplateImage());

            WriteableBitmap bitmap = new WriteableBitmap(TileWidth, TileWidth);

            int[] sourcePixels = template.Pixels;
            int[] destPixels = bitmap.Pixels;

            for (int i = 0; i < destPixels.Length; i++)
            {
                destPixels[i] = AlphaBlend(sourcePixels[i], colorValue);
            }

            bitmap.SaveJpeg(outputStream, TileWidth, TileWidth, 0, 8);
        }

        private BitmapSource GetTemplateImage()
        {
            var stream = applicationResourceFacade.GetResourceStream(TemplateUri);

            BitmapImage image = new BitmapImage();
            image.SetSource(stream);

            return image;
        }

        // TODO: This is totally wrong due to the byte ordering
        private int AlphaBlend(int src, int dest)
        {
            int a = src >> 24;   
            /* alpha */    
            /* If source pixel is transparent, just return the background */   
            if (0 == a)
                return dest;
            
            /* alpha blending the source and background colors */
            int rb = (((src & 0x00ff00ff) * a) +
                ((dest & 0x00ff00ff) * (0xff - a))) & unchecked((int)0xff00ff00);
            
            int    g  = (((src & 0x0000ff00) * a) +
                ((dest & 0x0000ff00) * (0xff - a))) & 0x00ff0000;
            
            return (src & unchecked((int)0xff000000)) | ((rb | g) >> 8);
        }

        private WriteableBitmap CreateEmptyBitmap(int width, int height, Color color)
        {
            int colorValue = (color.R << 24) |
                color.G << 16 |
                color.B << 8 |
                color.A;


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
