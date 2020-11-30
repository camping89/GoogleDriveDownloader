using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace GoogleDriveDownloader
{
    public class PODEZTaskModel
    {
        [Name(CsvHeaders.VariantId)]
        public string VariantId { get; set; }
        public string TaskId { get; set; }

        [BooleanTrueValues("Yes")]
        [BooleanFalseValues("No")]
        public bool? IsDriveFolderAvailable { get; set; }
        public string DriveUrl { get; set; }
    }
}
