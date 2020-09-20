using Framework.Interfaces;


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
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Path to image file on disk</param>
        /// <returns>A status indicating the number of sizes that were successfully generated</returns>
        IStatus<int> GenerateAllSizes(string imageFilePath);

        /// <summary>
        /// Generates an icon size image 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Original image file path</param>
        /// <returns>A status indicatng the success of the operation</returns>
        IStatus<string> GenerateIcon(string imageFilePath);

        /// <summary>
        /// Generates a large image from the given image file. 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        IStatus<string> GenerateBig(string imageFilePath);

        /// <summary>
        /// Generates a medium sized image from the given image file. 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        IStatus<string> GenerateMedium(string imageFilePath);

        /// <summary>
        /// Generates a small image from the given image file. 
        /// Images are generated in the same folder with source along with their size suffixed to
        /// their names
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        IStatus<string> GenerateSmall(string imageFilePath);

        /// <summary>
        /// Sharpens an image with a naive rudimentary algorithm
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <param name="generatedImagePath">Sharpened image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        IStatus<string> QuickSharpen(string imageFilePath,string generatedImagePath);

        /// <summary>
        /// Blurs an image by with a naive rudimentary algorithm
        /// </summary>
        /// <param name="imageFilePath">Image file path</param>
        /// <param name="generatedImagePath">Sharpened image file path</param>
        /// <returns>A status indicating the file path of generated image</returns>
        IStatus<string> QuickBlur(string imageFilePath, string generatedImagePath);
    }
}
