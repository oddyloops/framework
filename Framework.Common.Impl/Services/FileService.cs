using Framework.Common.Configs;
using Framework.Common.Services;
using Framework.Interfaces;
using Framework.Utils;
using Renci.SshNet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Impl.Services
{
    /// <summary>
    /// A SFTP implementation of IFileService
    /// </summary>
 
    public class FileService : IFileService
    {
        
        private string host;
        private string username;
        private string password;
        //config
        private IFTPConfig _ftpConfig;

        /// <summary>
        /// Constructs a new instance of FileService with required dependencies
        /// </summary>
        /// <param name="ftpConfig">Object encapsulating configuration parameters for SFTP operations</param>
        public FileService(IFTPConfig ftpConfig)
        {
            _ftpConfig = ftpConfig;
            host = _ftpConfig.FTPServerUrl;
            username = _ftpConfig.FTPUsername;
            password = _ftpConfig.FTPPassword;
        }

        /// <summary>
        /// Establishes a new connection with a SFTP Server
        /// </summary>
        /// <returns>A client object used to interact with SFTP Server</returns>
        private SftpClient NewClient()
        {
            ConnectionInfo connectionInfo = new ConnectionInfo(host, username,
                new PasswordAuthenticationMethod(username, password));
            return new SftpClient(connectionInfo);
        }

        /// <summary>
        /// Gets the number of files in a directory and its sub-directories
        /// </summary>
        /// <param name="directory">Path to directory</param>
        /// <param name="client">SFTP Client object</param>
        /// <returns>Number of files (This includes files in sub-directories but not the sub-directories themselves)</returns>
        private int FileCountRecurse(string directory, SftpClient client)
        {
            var children = client.ListDirectory(directory);
            int fileCount = (from c in children where !c.IsDirectory select c).Count();
            var directories = from c in children where c.IsDirectory select c;
            if(directories != null && directories.Count() > 0)
            {
                foreach(var dir in directories)
                {
                    fileCount += FileCountRecurse(dir.FullName, client);
                }
            }
            return fileCount;
        }

        /// <summary>
        /// Uploads a directory to a destination path
        /// </summary>
        /// <param name="sourceDir">Directory to upload</param>
        /// <param name="destDir">Destination directory</param>
        /// <param name="client">sftp client object</param>
        /// <returns>The total number of files uploaded in the directory</returns>
        private async  Task<int> FileUploadRecurse(string sourceDir,string destDir,SftpClient client)
        {
            int count = 0;
            var files = Directory.GetFiles(sourceDir);
            var directories = Directory.GetDirectories(sourceDir);
            if(!await DirectoryExists(destDir))
            {
                client.CreateDirectory(destDir);
            }
            if(files.Length > 0)
            {
                foreach(string file in files)
                {
                    using (FileStream fs = File.OpenRead(file))
                        client.UploadFile(fs, $"{destDir}/{Path.GetFileName(file)}");
                }
                count += files.Length;
            }
            if(directories.Length > 0)
            {
                foreach(string directory in directories)
                {
                    count += await FileUploadRecurse(directory, $"{destDir}/{Path.GetFileName(directory)}", client);
                }
            }
            return count;
        }
        /// <summary>
        /// Deletes a directory from the file server
        /// </summary>
        /// <param name="directory">Directory to delete</param>
        /// <returns>A status indicating the number of files that were deleted as a result</returns>
        public Task<IStatus<int>> DeleteDirectoryAsync(string directory)
        {
            using(var client = NewClient())
            {
                client.Connect();
                client.DeleteDirectory(directory);
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Deletes a file from the file server
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns>A status indicating the number of files that were deleted</returns>
        public Task<IStatus<int>> DeleteFileAsync(string file)
        {
            using (var client = NewClient())
            {
                client.Connect();
                client.DeleteFile(file);
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = 1;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Deletes multiple files from the file server
        /// </summary>
        /// <param name="files">List of file paths</param>
        /// <returns>A status indicating the number of files that were deleted</returns>
        public Task<IStatus<int>> DeleteFilesAsync(IList<string> files)
        {
            using (var client = NewClient())
            {
                client.Connect();
                foreach(string file in files)
                {
                    client.DeleteFile(file);
                }
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = files.Count;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Checks if a directory exists on the server
        /// </summary>
        /// <param name="dir">Directiry path</param>
        /// <returns>True if it exists</returns>
        public Task<bool> DirectoryExists(string dir)
        {
            bool exists = false;
            using (var client = NewClient())
            {
                client.Connect();
                exists = client.Exists(dir);
                client.Disconnect();
            }
            return Task.FromResult(exists);
        }

        /// <summary>
        /// Counts the number of files in a directory
        /// </summary>
        /// <param name="dir">Directory path</param>
        /// <returns>The number of files in directory (and sub-directories if recurse)</returns>
        public Task<int> DirectoryFileCount(string dir, bool recurse)
        {
            int total = 0;
            using (var client = NewClient())
            {
                client.Connect();
                if(!recurse)
                {
                    total = client.ListDirectory(dir).Count();
                }
                else
                {
                    total = FileCountRecurse(dir, client);
                }
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            return Task.FromResult(total);
        }

        /// <summary>
        /// Downloads a file from the file server onto the specified download path
        /// </summary>
        /// <param name="downloadPath">Download path</param>
        /// <param name="serverPath">Path to file on the file server</param>
        /// <returns>The number of files that were successfully downloaded</returns>
        public Task<IStatus<int>> DownloadFileAsync(string downloadPath, string serverPath)
        {
            using (var client = NewClient())
            {
                client.Connect();
                using (FileStream fs = File.Create(downloadPath))
                    client.DownloadFile(serverPath, fs);
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = 1;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Checks if a file exist on the server
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns>True if it exists</returns>
        public async Task<bool> FileExists(string file)
        {
            return await DirectoryExists(file);
        }


        /// <summary>
        /// Merges the content of the source directory  with the one on the file server.
        /// Note that files with matching names will be overriden
        /// </summary>
        /// <param name="sourceDir">Directory being uploaded</param>
        /// <param name="mergeDir">Directory being merged into</param>
        /// <returns>A status indicating how many files int he directory was successfully
        /// merged</returns>
        public async Task<IStatus<int>> MergeToExistingDirectoryAsync(string sourceDir, string mergeDir)
        {
            return await UploadNewDirectoryAsync(sourceDir, mergeDir);
        }

        /// <summary>
        /// Streams a file from the file server
        /// </summary>
        /// <param name="bufferStream">File stream</param>
        /// <param name="serverPath">Path to file on file server</param>
        /// <returns>The number of bytes that were successfully streamed</returns>
        public Task<IStatus<int>> StreamFileAsync(Stream bufferStream, string serverPath)
        {
            using (var client = NewClient())
            {
                client.Connect();
                client.DownloadFile(serverPath, bufferStream);
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = 1;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Uploads a file to an existing path 
        /// </summary>
        /// <param name="sourceFiles">Files to be uploaded, (if file already exists, it will be
        /// overwritten)</param>
        /// <param name="dest">Destination path on file server</param>
        /// <returns>A status indicating the number of files uploaded which should be 1 if 
        /// successful</returns>
        public Task<IStatus<int>> UploadFileAsync(string sourceFile, string dest)
        {
            using (var client = NewClient())
            {
                client.Connect();
                using (FileStream fs = File.OpenRead(sourceFile))
                    client.UploadFile(fs, dest);
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = 1;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Uploads multiple files to an existing path 
        /// </summary>
        /// <param name="sourceFiles">Files to be uploaded, (if file already exists, it will be
        /// overwritten)</param>
        /// <param name="dest">Destination path on file server</param>
        /// <returns>A status indicating the number of files
        /// </returns>

        public Task<IStatus<int>> UploadFilesAsync(string[] sourceFiles, string dest)
        {
            using (var client = NewClient())
            {
                client.Connect();
                foreach (string sourceFile in sourceFiles)
                {
                    using (FileStream fs = File.OpenRead(sourceFile))
                        client.UploadFile(fs, dest);
                }
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = sourceFiles.Length;
            return Task.FromResult(result);
        }

        /// <summary>
        /// Uploads a directory to an existing directory on the file server
        /// </summary>
        /// <param name="sourceDir">Directory being uploaded</param>
        /// <param name="dest">Path on the file server</param>
        /// <returns>A status indicating how many files int he directory was successfully
        /// uploaded</returns>

        public async Task<IStatus<int>> UploadNewDirectoryAsync(string sourceDir, string dest)
        {
            int count = 0;
            using (var client = NewClient())
            {
                client.Connect();
                count = await FileUploadRecurse(sourceDir, dest, client);
                client.Disconnect();
            }
            IStatus<int> result = Util.Container.CreateInstance<IStatus<int>>();
            result.IsSuccess = true;
            result.StatusInfo = count;
            return result;
        }

        /// <summary>
        /// Uploads a directory to an existing directory on the file server
        /// </summary>
        /// <param name="sourceDir">Directory being uploaded</param>
        /// <param name="dest">Path on the file server</param>
        /// <returns>A status indicating how many files int he directory was successfully
        /// uploaded</returns>

        public Task<IList<string>> GetFileNamesAsync(string directory)
        {
            IList<string> results = new List<string>();
            using (var client = NewClient())
            {
                client.Connect();
                var files = client.ListDirectory(directory);
                foreach(var file in files)
                {
                    results.Add(file.Name);
                }
                client.Disconnect();
            }
            return Task.FromResult(results);
        }
    }
}
