using Framework.Interfaces;
using System.Collections.Generic;

namespace Framework.Common.Services
{
    /// <summary>
    /// An interface for image processing
    /// (Not using async since its CPU intensive)
    /// </summary>
    public interface IImageService
    {
        /// <summary>
        /// Genrates all three sizes of an image (small, big, medium) while maintaining
        /// aspect ratio.
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>A map of size and corresponding binary image data. Map keys are:
        /// icon,small,medium,big</returns>
        IDictionary<string,byte[]> GenerateAllSizes(byte[] imageData);

        /// <summary>
        /// Generates an icon size image 
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Icon image binary data</returns>
        byte[] GenerateIcon(byte[] imageData);

        /// <summary>
        /// Generates a large image from the given image file. 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Large image binary data</returns>
        byte[] GenerateBig(byte[] imageData);

        /// <summary>
        /// Generates a medium sized image from the given image file. 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Medium image binary data</returns>
        byte[] GenerateMedium(byte[] imageData);

        /// <summary>
        /// Generates a small image from the given image file. 
        /// <param name="imageData">Image binary data</param>
        /// <returns>Small image binary data</returns>
        byte[] GenerateSmall(byte[] imageData);

        /// <summary>
        /// Sharpens an image with a naive rudimentary algorithm
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Sharpened image binary data</returns>
        byte[] QuickSharpen(byte[] imageData);

        /// <summary>
        /// Blurs an image by with a naive rudimentary algorithm
        /// </summary>
        /// <param name="imageData">Image binary data</param>
        /// <returns>Blurred image bianry data</returns>
        byte[] QuickBlur(byte[] imageData);
    }
}
