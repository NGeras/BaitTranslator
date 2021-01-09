using System;
using System.Collections.Generic;
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
    }
}
