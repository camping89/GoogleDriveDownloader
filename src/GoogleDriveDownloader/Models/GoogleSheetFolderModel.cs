namespace GoogleDriveDownloader
{
    public class GoogleSheetFolderModel
    {
        public string FolderName { get; set; }
        public string FolderId { get; set; }
        public bool Exist { get; set; }
        public string FolderUrl => Exist ? $"https://drive.google.com/drive/u/0/folders/{FolderId}" : null;
    }
}
