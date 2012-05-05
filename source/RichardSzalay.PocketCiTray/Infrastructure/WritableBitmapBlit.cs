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
using System.Windows.Media.Imaging;

namespace RichardSzalay.PocketCiTray.Infrastructure
{
    public static class WritableBitmapBlit
    {
        public static void AlphaBlit(this WriteableBitmap bmp, Rect destRect, WriteableBitmap source, Rect sourceRect)
        {
            Color color = Colors.White;

            if (color.A != 0)
            {
                int width = (int)destRect.Width;
                int height = (int)destRect.Height;
                int pixelWidth = bmp.PixelWidth;
                int pixelHeight = bmp.PixelHeight;
                Rect rect = new Rect(0.0, 0.0, (double)pixelWidth, (double)pixelHeight);
                rect.Intersect(destRect);
                if (!rect.IsEmpty)
                {
                    int num5 = source.PixelWidth;
                    int[] pixels = source.Pixels;
                    int[] dst = bmp.Pixels;
                    int length = pixels.Length;
                    int index = -1;
                    int x = (int)destRect.X;
                    int y = (int)destRect.Y;
                    int num15 = 0;
                    int num16 = 0;
                    int num17 = 0;
                    int num22 = 0;
                    int a = color.A;
                    int r = color.R;
                    int g = color.G;
                    int b = color.B;
                    bool flag = color != Colors.White;
                    int num28 = (int)sourceRect.Width;
                    double num29 = sourceRect.Width / destRect.Width;
                    double num30 = sourceRect.Height / destRect.Height;
                    int num31 = (int)sourceRect.X;
                    int num32 = (int)sourceRect.Y;
                    int num33 = -1;
                    int num34 = -1;
                    double num14 = num32;
                    int num11 = y;
                    for (int i = 0; i < height; i++)
                    {
                        if ((num11 >= 0) && (num11 < pixelHeight))
                        {
                            double num13 = num31;
                            int num12 = x + (num11 * pixelWidth);
                            int num10 = x;
                            int num21 = pixels[0];

                            for (int j = 0; j < width; j++)
                            {
                                if ((num10 >= 0) && (num10 < pixelWidth))
                                {
                                    if ((((int)num13) != num33) || (((int)num14) != num34))
                                    {
                                        index = ((int)num13) + (((int)num14) * num5);
                                        if ((index >= 0) && (index < length))
                                        {
                                            num21 = pixels[index];
                                            num22 = (num21 >> 0x18) & 0xff;
                                            num15 = (num21 >> 0x10) & 0xff;
                                            num16 = (num21 >> 8) & 0xff;
                                            num17 = num21 & 0xff;
                                            if (flag && (num22 != 0))
                                            {
                                                num22 = ((num22 * a) * 0x8081) >> 0x17;
                                                num15 = (((((num15 * r) * 0x8081) >> 0x17) * a) * 0x8081) >> 0x17;
                                                num16 = (((((num16 * g) * 0x8081) >> 0x17) * a) * 0x8081) >> 0x17;
                                                num17 = (((((num17 * b) * 0x8081) >> 0x17) * a) * 0x8081) >> 0x17;
                                                num21 = (((num22 << 0x18) | (num15 << 0x10)) | (num16 << 8)) | num17;
                                            }
                                        }
                                        else
                                        {
                                            num22 = 0;
                                        }
                                    }

                                    int num18;
                                    int num19;
                                    int num20;
                                    int num23;

                                    int num42 = dst[num12];
                                    num23 = (num42 >> 0x18) & 0xff;

                                    num18 = (num42 >> 0x10) & 0xff;
                                    num19 = (num42 >> 8) & 0xff;
                                    num20 = num42 & 0xff;
                                    num42 = ((((num22 + (((num23 * (0xff - num22)) * 0x8081) >> 0x17)) << 0x18) | ((num15 + (((num18 * (0xff - num22)) * 0x8081) >> 0x17)) << 0x10)) | ((num16 + (((num19 * (0xff - num22)) * 0x8081) >> 0x17)) << 8)) | (num17 + (((num20 * (0xff - num22)) * 0x8081) >> 0x17));
                                    dst[num12] = num42;
                                }
                                num10++;
                                num12++;
                                num13 += num29;
                            }
                        }
                        num14 += num30;
                        num11++;
                    }
                }
            }
        }

    }
}
