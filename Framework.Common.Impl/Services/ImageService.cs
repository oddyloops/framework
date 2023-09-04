using Framework.Common.Services;
using Framework.Interfaces;
using Framework.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Framework.Common.Impl.Services
{
    /// <summary>
    /// A concrete implementation of the IImageService
    /// </summary>
    public class ImageService : IImageService
    {
        private const int MIN_BIG = 720;
        private const int MIN_MEDIUM = 320;
        private const int MIN_SMALL = 50;

        /// <summary>
        /// Fraction2 is (1 - percent1)
        /// </summary>
        private static Color BlendPixels(Color pixel1, float fraction1, Color pixel2)
        {
            float fraction2 = 1 - fraction1;
            byte r = (byte)(pixel1.R * fraction1 + pixel2.R * fraction2);
            byte g = (byte)(pixel1.G * fraction1 + pixel2.G * fraction2);
            byte b = (byte)(pixel2.B * fraction1 + pixel2.B * fraction2);
            Color result = Color.FromArgb(255, r, g, b);
            return result;
        }

        /// <summary>
        /// Genrates all three sizes of an image (small, big, medium) while maintaining
        /// aspect ratio.
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>A map of size and corresponding binary image data. Map keys are:
        /// icon,small,medium,big</returns>
        public IDictionary<string,byte[]> GenerateAllSizes(byte[] imageData)
        {
            IDictionary<string, byte[]> output = new Dictionary<string, byte[]>();
            output.Add("big",GenerateBig(imageData));
            output.Add("medium",GenerateMedium(imageData));
            output.Add("small",GenerateSmall(imageData));
            output.Add("icon", GenerateIcon(imageData));
            return output;
        }

        /// <summary>
        /// Generates a large image from the given image file. 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Large image binary data</returns>
        public byte[] GenerateBig(byte[] imageData)
        {
           
            int height = 0;
            using (var image = Image.FromStream(new MemoryStream(imageData)))
            {
                height = image.Height;
            }
            if (height > MIN_BIG)
            {
                //simple copy
                return imageData;
            }
            else
            {
                return QuickSharpen(imageData);
            }

        
        }


        /// <summary>
        /// Generates an icon size image 
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Icon image binary data</returns>
        public byte[] GenerateIcon(byte[] imageData)
        { 
            int height = 0;
            using (var image = Image.FromStream(new MemoryStream(imageData)))
            {
                height = image.Height;
            }
            if (height < MIN_SMALL)
            {
                //simple copy
                return imageData;
            }
            else
            {
                return QuickBlur(imageData);
            }

        }

        /// <summary>
        /// Generates a medium sized image from the given image file. 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Medium image binary data</returns>
        public byte[] GenerateMedium(byte[] imageData)
        {
        
            int height = 0;
            using (var image = Image.FromStream(new MemoryStream(imageData)))
            {
                height = image.Height;
            }
            if (height <= MIN_BIG && height > MIN_MEDIUM)
            {
                //simple copy
                return imageData;
            }
            else if (height <= MIN_MEDIUM)
            {
                return QuickSharpen(imageData);
            }
            else
            {
                return QuickBlur(imageData);
            }

        }

        /// <summary>
        /// Generates a small image from the given image file. 
        /// <param name="imageData">Image binary data</param>
        /// <returns>Small image binary data</returns>
        public byte[] GenerateSmall(byte[] imageData)
        {
            int height = 0;
            using (var image = Image.FromStream(new MemoryStream(imageData)))
            {
                height = image.Height;
            }
            if (height <= MIN_MEDIUM && height > MIN_SMALL)
            {
                //simple copy
                return imageData;
            }
            else if (height <= MIN_SMALL)
            {
                return QuickSharpen(imageData);
            }
            else
            {
                return QuickBlur(imageData);
            }
        }


        /// <summary>
        /// Blurs an image by with a naive rudimentary algorithm
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Blurred image bianry data</returns>
        public byte[] QuickBlur(byte[] imageData)
        {
            using Bitmap source = new Bitmap(new MemoryStream(imageData));
            using Bitmap generated = new Bitmap((int)Math.Ceiling(0.33 * source.Width),
                (int)Math.Ceiling(0.33 * source.Height));
            for (int y = 1, y2 = 0; y < source.Height && y2 < generated.Height; y += 3, y2++)
            {
                for (int x = 1, x2 = 0; x < source.Width && x2 < generated.Width; x += 3, x2++)
                {
                    generated.SetPixel(x2, y2, source.GetPixel(x, y));
                }
            }
            MemoryStream output = new MemoryStream();
            generated.Save(output, ImageFormat.Jpeg);
            return output.ToArray();

        }

        /// <summary>
        /// Sharpens an image with a naive rudimentary algorithm
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Sharpened image binary data</returns>
        public byte[] QuickSharpen(byte[] imageData)
        {
            using (Bitmap source = new Bitmap(new MemoryStream(imageData)))
            using (Bitmap generated = new Bitmap(3 * source.Width,
                3 * source.Height))
            {
                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++)
                    {
                        Color center = source.GetPixel(x, y);

                        int xTopLeft = x - 1;
                        int yTopLeft = y - 1;
                        Color topLeft = (xTopLeft < 0 || yTopLeft < 0) ? Color.Black : source.GetPixel(xTopLeft, yTopLeft);
                        generated.SetPixel(x * 3, y * 3, BlendPixels(topLeft, 0.25F, center));

                        int xTopCenter = x;
                        int yTopCenter = y - 1;
                        Color topCenter = (yTopCenter < 0) ? Color.Black : source.GetPixel(xTopCenter, yTopCenter);
                        generated.SetPixel(x * 3 + 1, y * 3, BlendPixels(topCenter, 0.25F, center));

                        int xTopRight = x + 1;
                        int yTopRight = y - 1;
                        Color topRight = (xTopRight >= source.Width || yTopRight < 0) ? Color.Black : source.GetPixel(xTopRight, yTopRight);
                        generated.SetPixel(x * 3 + 2, y * 3, BlendPixels(topRight, 0.25F, center));


                        int xLeftCenter = x - 1;
                        int yLeftCenter = y;
                        Color leftCenter = (xLeftCenter < 0) ? Color.Black : source.GetPixel(xLeftCenter, yLeftCenter);
                        generated.SetPixel(x * 3, y * 3 + 1, BlendPixels(leftCenter, 0.25F, center));

                        int xRightCenter = x + 1;
                        int yRightCenter = y;
                        Color rightCenter = (xRightCenter >= source.Width) ? Color.Black : source.GetPixel(xRightCenter, yRightCenter);
                        generated.SetPixel(x * 3 + 2, y * 3 + 1, BlendPixels(rightCenter, 0.25F, center));

                        int xLeftBottom = x - 1;
                        int yLeftBottom = y + 1;
                        Color leftBottom = (xLeftBottom < 0 || yLeftBottom >= source.Height) ? Color.Black : source.GetPixel(xLeftBottom, yLeftBottom);
                        generated.SetPixel(x * 3, y * 3 + 2, BlendPixels(leftBottom, 0.25F, center));

                        int xBottomCenter = x;
                        int yBottomCenter = y + 1;
                        Color bottomCenter = (yBottomCenter >= source.Height) ? Color.Black : source.GetPixel(xBottomCenter, yBottomCenter);
                        generated.SetPixel(x * 3 + 1, y * 3 + 2, BlendPixels(bottomCenter, 0.25F, center));

                        int xBottomRight = x + 1;
                        int yBottomRight = y + 1;
                        Color bottomRight = (yBottomRight >= source.Height || xBottomRight >= source.Width) ? Color.Black : source.GetPixel(xBottomRight, yBottomRight);
                        generated.SetPixel(x * 3 + 2, y * 3 + 2, BlendPixels(bottomRight, 0.25F, center));

                        generated.SetPixel(x * 3 + 1, y * 3 + 1, center);
                    }
                }
                MemoryStream output = new MemoryStream();
                generated.Save(output, ImageFormat.Jpeg);
                return output.ToArray();
            }
           
        }
    }
}
