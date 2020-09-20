using Framework.Common.Services;
using Framework.Interfaces;
using Framework.Utils;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace Framework.Common.Impl.Services
{
    [Export]
    public class ImageService : IImageService
    {
        private const int MIN_BIG = 720;
        private const int MIN_MEDIUM = 320;
        private const int MIN_SMALL = 50;

        /// <summary>
        /// Fraction2 is (1 - percent1)
        /// </summary>
        private Color BlendPixels(Color pixel1, float fraction1, Color pixel2)
        {
            float fraction2 = 1 - fraction1;
            byte r = (byte)(pixel1.R * fraction1 + pixel2.R * fraction2);
            byte g = (byte)(pixel1.G * fraction1 + pixel2.G * fraction2);
            byte b = (byte)(pixel2.B * fraction1 + pixel2.B * fraction2);
            Color result = Color.FromArgb(255, r, g, b);
            return result;
        }

        public IStatus<int> GenerateAllSizes(string imageFilePath)
        {
            GenerateBig(imageFilePath);
            GenerateMedium(imageFilePath);
            GenerateSmall(imageFilePath);
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = 3;
            return result;
        }

        /// <summary>
        /// Generates a large image from the given image file. 
        /// (No-Op if image > 720p)
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        public IStatus<string> GenerateBig(string imageFilePath)
        {
            string genImagePath = $"{Path.GetDirectoryName(imageFilePath)}/{Path.GetFileNameWithoutExtension(imageFilePath)}" +
                $".big.jpg";
            int height = 0;
            using (var image = Image.FromFile(imageFilePath))
            {
                height = image.Height;
            }
            if (height > MIN_BIG)
            {
                //simple copy
                File.Copy(imageFilePath, genImagePath);
            }
            else
            {
                QuickSharpen(imageFilePath, genImagePath);
            }

            IStatus<string> result = Util.Container.CreateInstance<IStatus<string>>();
            result.IsSuccess = true;
            result.StatusInfo = genImagePath;
            return result;
        }


        /// <summary>
        /// Generates an icon size image 
        /// (No-Op if image < 50p)
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Original image file path</param>
        /// <returns>A status indicatng the success of the operation</returns>
        public IStatus<string> GenerateIcon(string imageFilePath)
        {
            string genImagePath = $"{Path.GetDirectoryName(imageFilePath)}/{Path.GetFileNameWithoutExtension(imageFilePath)}" +
               $".icon.jpg";
            int height = 0;
            using (var image = Image.FromFile(imageFilePath))
            {
                height = image.Height;
            }
            if (height < MIN_SMALL)
            {
                //simple copy
                File.Copy(imageFilePath, genImagePath);
            }
            else
            {
                QuickBlur(imageFilePath, genImagePath);
            }

            IStatus<string> result = Util.Container.CreateInstance<IStatus<string>>();
            result.IsSuccess = true;
            result.StatusInfo = genImagePath;
            return result;

        }

        /// <summary>
        /// Generates a medium sized image from the given image file. 
        /// (No-Op if image <= 720p && > 320p)
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        public IStatus<string> GenerateMedium(string imageFilePath)
        {
            string genImagePath = $"{Path.GetDirectoryName(imageFilePath)}/{Path.GetFileNameWithoutExtension(imageFilePath)}" +
               $".medium.jpg";
            int height = 0;
            using (var image = Image.FromFile(imageFilePath))
            {
                height = image.Height;
            }
            if (height <= MIN_BIG && height > MIN_MEDIUM)
            {
                //simple copy
                File.Copy(imageFilePath, genImagePath);
            }
            else if(height <= MIN_MEDIUM)
            {
                QuickSharpen(imageFilePath, genImagePath);
            }
            else
            {
                QuickBlur(imageFilePath, genImagePath);
            }

            IStatus<string> result = Util.Container.CreateInstance<IStatus<string>>();
            result.IsSuccess = true;
            result.StatusInfo = genImagePath;
            return result;

        }

        /// <summary>
        /// Generates a small image from the given image file. 
        /// (No-Op if image <= 320p)
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        public IStatus<string> GenerateSmall(string imageFilePath)
        {
            string genImagePath = $"{Path.GetDirectoryName(imageFilePath)}/{Path.GetFileNameWithoutExtension(imageFilePath)}" +
               $".small.jpg";
            int height = 0;
            using (var image = Image.FromFile(imageFilePath))
            {
                height = image.Height;
            }
            if (height <= MIN_MEDIUM && height > MIN_SMALL)
            {
                //simple copy
                File.Copy(imageFilePath, genImagePath);
            }
            else if (height <= MIN_SMALL)
            {
                QuickSharpen(imageFilePath, genImagePath);
            }
            else
            {
                QuickBlur(imageFilePath, genImagePath);
            }

            IStatus<string> result = Util.Container.CreateInstance<IStatus<string>>();
            result.IsSuccess = true;
            result.StatusInfo = genImagePath;
            return result;
        }

        /// <summary>
        /// Blurs an image by a scale of 0.3, by selecting central pixels only
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <param name="generatedImagePath">Sharpened image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        public IStatus<string> QuickBlur(string imageFilePath, string generatedImagePath)
        {
            using(Bitmap source = new Bitmap(imageFilePath))
            using(Bitmap generated = new Bitmap((int)Math.Ceiling(0.33 * source.Width),
                (int)Math.Ceiling(0.33 * source.Height)))
            {
                for(int y = 1,y2 = 0; y < source.Height; y+=3, y2++)
                {
                    for(int x = 1,x2=0; x < source.Width; x +=3,x2++)
                    {
                        generated.SetPixel(x2, y2, source.GetPixel(x, y));
                    }
                }
                generated.Save(generatedImagePath, ImageFormat.Jpeg);
            }
            IStatus<string> result = Util.Container.CreateInstance<IStatus<string>>();
            result.IsSuccess = true;
            result.StatusInfo = generatedImagePath;
            return result;
        }

        /// <summary>
        /// Sharpens an image by a scale of 3x, using linear interpolation
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <param name="generatedImagePath">Sharpened image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        public IStatus<string> QuickSharpen(string imageFilePath, string generatedImagePath)
        {
            using (Bitmap source = new Bitmap(imageFilePath))
            using (Bitmap generated = new Bitmap(3 * source.Width,
                3 * source.Height))
            {
                for(int y = 0; y < source.Height; y++)
                {
                    for(int x = 0; x < source.Width; x++)
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
                generated.Save(generatedImagePath, ImageFormat.Jpeg);
            }
            IStatus<string> result = Util.Container.CreateInstance<IStatus<string>>();
            result.IsSuccess = true;
            result.StatusInfo = generatedImagePath;
            return result;
        }
    }
}
