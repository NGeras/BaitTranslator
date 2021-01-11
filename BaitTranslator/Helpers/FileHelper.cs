using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace BaitTranslator.Helpers
{
    public static class FileHelper
    {
        public static async Task<StorageFile> TryGetFile(string path)
        {
            try
            {
                return await StorageFile.GetFileFromPathAsync(path);
            }
            catch (UnauthorizedAccessException)
            {
                await Notifications.CatchUnauthorizedAccessException("file system");
            }

            return null;
        }
        public static async Task<StorageFolder> TryGetFolder(string path)
        {
            try
            {
                return await StorageFolder.GetFolderFromPathAsync(path);
            }
            catch (UnauthorizedAccessException)
            {
                await Notifications.CatchUnauthorizedAccessException("file system");
            }

            return null;
        }
        public static async Task<StorageFile> CreateFile(string folderPath, string fileName)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
            return await folder.CreateFileAsync(fileName);
        }
        public static async Task<bool> IsFileExists(string path)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                return true;
            }
            catch (Exception e)
            {
                if (e is UnauthorizedAccessException)
                {
                    await Notifications.CatchUnauthorizedAccessException("file system");
                }

                if (e is FileNotFoundException)
                {
                    return false;
                }

                return false;
            }
        }
    }
}
