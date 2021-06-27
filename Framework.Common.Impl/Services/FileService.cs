using Framework.Common.Services;
using Framework.Interfaces;
using Framework.Utils;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Impl.Services
{
    /// <summary>
    /// A SFTP implementation of IFileService
    /// </summary>
    [Export(typeof(IFileService))]
    public class FileService : IFileService
    {
        private string host;
        private string username;
        private string password;

        [ImportingConstructor]
        public FileService([Import("JsonConfig")]IConfiguration configReader)
        {
            host = configReader.GetValue(ConfigConstants.SFTP_HOST);
            username = configReader.GetValue(ConfigConstants.SFTP_USERNAME);
            password = configReader.GetValue(ConfigConstants.SFTP_PASSWORD);
        }

        private SftpClient NewClient()
        {
            ConnectionInfo connectionInfo = new ConnectionInfo(host, username,
                new PasswordAuthenticationMethod(username, password));
            return new SftpClient(connectionInfo);
        }

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

        public async Task<bool> FileExists(string file)
        {
            return await DirectoryExists(file);
        }

        public async Task<IStatus<int>> MergeToExistingDirectoryAsync(string sourceDir, string mergeDir)
        {
            return await UploadNewDirectoryAsync(sourceDir, mergeDir);
        }

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
            result.StatusInfo = 1;
            return Task.FromResult(result);
        }

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
