using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GoogleDriveDownloader
{
    public class CsvService : ICsvService
    {
        public List<T> ReadFromFile<T>(string filePath) where T : class
        {
            try
            {
                using var reader = new StreamReader(filePath, Encoding.Default);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var records = csv.GetRecords<T>().ToList();
                return records;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void UpdateToFile<T>(string filePath, List<T> objects) where T : class
        {
            try
            {
                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteRecords(objects);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
