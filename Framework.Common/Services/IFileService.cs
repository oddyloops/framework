using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Services
{
    public interface IFileService : IService
    {
        /// <summary>
        /// Uploads a directory to an existing directory on the file server
        /// </summary>
        /// <param name="sourceDir">Directory being uploaded</param>
        /// <param name="dest">Path on the file server</param>
        /// <returns>A status indicating how many files int he directory was successfully
        /// uploaded</returns>
        Task<IStatus<int>> UploadNewDirectoryAsync(string sourceDir, string dest);

        /// <summary>
        /// Merges the content of the source directory  with the one on the file server.
        /// Note that files with matching names will be overriden
        /// </summary>
        /// <param name="sourceDir">Directory being uploaded</param>
        /// <param name="mergeDir">Directory being merged into</param>
        /// <returns>A status indicating how many files int he directory was successfully
        /// merged</returns>
        Task<IStatus<int>> MergeToExistingDirectoryAsync(string sourceDir, string mergeDir);

        /// <summary>
        /// Uploads a file to an existing path 
        /// </summary>
        /// <param name="sourceFile">File to be uploaded, (if file already exists, it will be
        /// overwritten)</param>
        /// <param name="dest">Destination path on file server</param>
        /// <returns>A status indicating the number of files uploaded which should be 1 if 
        /// successful</returns>
        Task<IStatus<int>> UploadFileAsync(string sourceFile, string dest);

        /// <summary>
        /// Uploads multiple files to an existing path 
        /// </summary>
        /// <param name="sourceFiles">Files to be uploaded, (if file already exists, it will be
        /// overwritten)</param>
        /// <param name="dest">Destination path on file server</param>
        /// <returns>A status indicating the number of files uploaded </returns>
        Task<IStatus<int>> UploadFilesAsync(string[] sourceFiles, string dest);

        /// <summary>
        /// Streams a file upstream to the file server
        /// </summary>
        /// <param name="stream">File stream</param>
        /// <param name="dest">Destination file on file server</param>
        /// <returns>A status indicating the success/failure of operation</returns>
        Task<IStatus<int>> UploadStreamAsync(Stream stream, string dest);

        /// <summary>
        /// Deletes a directory from the file server
        /// </summary>
        /// <param name="directory">Directory to delete</param>
        /// <returns>A status indicating the number of files that were deleted as a result</returns>
        Task<IStatus<int>> DeleteDirectoryAsync(string directory);

        /// <summary>
        /// Deletes a file from the file server
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns>A status indicating the number of files that were deleted</returns>
        Task<IStatus<int>> DeleteFileAsync(string file);

        /// <summary>
        /// Deletes multiple files from the file server
        /// </summary>
        /// <param name="files">List of file paths</param>
        /// <returns>A status indicating the number of files that were deleted</returns>
        Task<IStatus<int>> DeleteFilesAsync(IList<string> files);

        /// <summary>
        /// Checks if a file exist on the server
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns>True if it exists</returns>
        Task<bool> FileExists(string file);

        /// <summary>
        /// Checks if a directory exists on the server
        /// </summary>
        /// <param name="dir">Directiry path</param>
        /// <returns>True if it exists</returns>
        Task<bool> DirectoryExists(string dir);

        /// <summary>
        /// Counts the number of files in a directory
        /// </summary>
        /// <param name="dir">Directory path</param>
        /// <returns>The number of files in directory (and sub-directories if recurse)</returns>
        Task<int> DirectoryFileCount(string dir,bool recurse);

        /// <summary>
        /// Downloads a file from the file server onto the specified download path
        /// </summary>
        /// <param name="downloadPath">Download path</param>
        /// <param name="serverPath">Path to file on the file server</param>
        /// <returns>The number of files that were successfully downloaded</returns>
        Task<IStatus<int>> DownloadFileAsync(string downloadPath, string serverPath);

        /// <summary>
        /// Streams a file from the file server
        /// </summary>
        /// <param name="bufferStream">File stream</param>
        /// <param name="serverPath">Path to file on file server</param>
        /// <returns>The number of bytes that were successfully streamed</returns>
        Task<IStatus<int>> StreamFileAsync(Stream bufferStream, string serverPath);

        /// <summary>
        /// Get all the file names in server directory
        /// </summary>
        /// <param name="directory">Server directory</param>
        /// <returns>List of files in directory</returns>
        Task<IList<string>> GetFileNamesAsync(string directory);
        
    }
}
