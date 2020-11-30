using System.Collections.Generic;

namespace GoogleDriveDownloader
{
    public interface ICsvService
    {
        List<T> ReadFromFile<T>(string filePath) where T : class;
        void UpdateToFile<T>(string filePath, List<T> objects) where T : class;
    }
}
