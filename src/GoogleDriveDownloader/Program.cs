using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleDriveDownloader
{
    class Program
    {
        public static IConfigurationRoot Configuration;
        static async Task Main(string[] args)
        {
            using var scope = ConfigureServiceScope();
            var services = scope.ServiceProvider;
            var csvService = services.GetRequiredService<ICsvService>();
            var googleDriveService = services.GetRequiredService<IGoogleDriveService>();
            var filePath = Configuration["DataFilePath"];
            var podezTasks = csvService.ReadFromFile<PODEZTaskModel>(filePath);

            var folders = await googleDriveService.GetFoldersAsync(podezTasks.Select(x => x.TaskId));

            foreach (var item in podezTasks)
            {
                var foundFolder = folders.FirstOrDefault(x => x.FolderName == item.TaskId);
                item.IsDriveFolderAvailable = foundFolder?.Exist == true;
                item.DriveUrl = foundFolder?.FolderUrl;
            }

            csvService.UpdateToFile(filePath, podezTasks);

            var foldersToDownload = folders.Where(x => x.Exist && x.FolderId != null).DistinctBy(x => x.FolderId);
            if (foldersToDownload != null)
            {
                await googleDriveService.DownloadFilesAsync(foldersToDownload);
            }
        }

        private static IServiceScope ConfigureServiceScope()
        {
            var host = new HostBuilder()
                         .ConfigureAppConfiguration((context, builder) =>
                         {
                             builder.AddJsonFile("appsettings.json", optional: true);
                             Configuration = builder.Build();
                         })
                         .ConfigureServices((context, services) =>
                         {
                             services.AddSingleton(context.Configuration.GetSection(nameof(GoogleConfiguration)).Get<GoogleConfiguration>())
                                     .AddScoped<IGoogleDriveService, GoogleDriveService>()
                                     .AddScoped<ICsvService, CsvService>();
                         });

            return host.Build().Services.CreateScope();
        }
    }
}
