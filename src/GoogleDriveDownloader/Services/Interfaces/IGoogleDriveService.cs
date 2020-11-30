using System.Collections.Generic;
using System.Threading.Tasks;

namespace GoogleDriveDownloader
{
    public interface IGoogleDriveService
    {
        Task DownloadFilesAsync(IEnumerable<GoogleSheetFolderModel> sheetFolders);
        Task<IEnumerable<GoogleSheetFolderModel>> GetFoldersAsync(IEnumerable<string> folderNames);
    }
}
