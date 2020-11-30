using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Requests;
using Google.Apis.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using File = System.IO.File;

namespace GoogleDriveDownloader
{
    public class GoogleDriveService : IGoogleDriveService
    {
        private readonly GoogleConfiguration _configuration;

        public GoogleDriveService(GoogleConfiguration configuration, ICsvService csvService)
        {
            _configuration = configuration;
        }

        public async Task DownloadFilesAsync(IEnumerable<GoogleSheetFolderModel> folders)
        {
            if (folders.IsNotNullOrEmpty())
            {
                var driveService = AuthenticateDriveServiceAccount();
                var batch = new BatchRequest(driveService);

                BatchRequest.OnResponse<FileList> callback = delegate (FileList list, RequestError error, int index, HttpResponseMessage message)
                {
                    if (error == null)
                    {
                        foreach (var file in list.Files)
                        {
                            var directoryPath = Path.Combine(_configuration.DownloadFolder, folders.FirstOrDefault(x => x.FolderId == file.Parents.FirstOrDefault())?.FolderName ?? "Undefined");
                            var filePath = Path.Combine(directoryPath, file.Name);
                            if (!Directory.Exists(directoryPath))
                            {
                                Directory.CreateDirectory(directoryPath);
                            }
                            var downloadRequest = driveService.Files.Get(file.Id);
                            using (var stream = new MemoryStream())
                            {
                                downloadRequest.Download(stream);
                                SaveStream(stream, filePath);
                            }
                        }
                    }
                };

                foreach (var folder in folders)
                {
                    var listRequest = BuildDriveListRequest(driveService, folder.FolderId);

                    batch.Queue(listRequest, callback);
                }

                await batch.ExecuteAsync();
            }
        }

        private void SaveStream(MemoryStream stream, string filePath)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                stream.WriteTo(file);
            }
        }

        public async Task<IEnumerable<GoogleSheetFolderModel>> GetFoldersAsync(IEnumerable<string> folderNames)
        {
            var driveService = AuthenticateDriveServiceAccount();


            var results = new List<GoogleSheetFolderModel>();

            var batch = new BatchRequest(driveService);
            var errors = new List<string>();
            BatchRequest.OnResponse<FileList> callback = delegate (FileList list, RequestError error, int index, HttpResponseMessage message)
            {
                if (error == null)
                {
                    if (list.Files.Any())
                    {
                        var file = list.Files.FirstOrDefault();
                        var result = new GoogleSheetFolderModel
                        {
                            FolderName = file.Name,
                            Exist = true,
                            FolderId = file.Id,
                        };

                        results.Add(result);
                    }
                }
                else
                {
                    errors.Add(error.Message);
                }
            };

            foreach (var name in folderNames)
            {
                var listRequest = driveService.Files.List();
                listRequest.PageSize = 1000;
                listRequest.SupportsAllDrives = true;
                listRequest.SupportsAllDrives = true;
                listRequest.IncludeItemsFromAllDrives = true;
                listRequest.DriveId = _configuration.DriveId;
                listRequest.Corpora = "drive";
                listRequest.Fields = "files(id, name, parents, trashed, mimeType)";
                listRequest.Q = listRequest.Q = $"name = '{name}' and mimeType = 'application/vnd.google-apps.folder' and trashed=false";
                batch.Queue(listRequest, callback);
            }

            await batch.ExecuteAsync();

            return results;
        }

        private ServiceAccountCredential GetServiceAccount()
        {
            var path = Path.Combine(Environment.CurrentDirectory, _configuration.ServiceAccountKeyFilePath);
            if (!File.Exists(path))
            {
                Console.WriteLine("An Error occurred - Key file does not exist");
                return null;
            }

            var scopes = new string[] { DriveService.Scope.Drive };

            ServiceAccountCredential credential;
            using (Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                credential = (ServiceAccountCredential)
                    GoogleCredential.FromStream(stream).UnderlyingCredential;

                var initializer = new ServiceAccountCredential.Initializer(credential.Id)
                {
                    Key = credential.Key,
                    Scopes = scopes
                };
                credential = new ServiceAccountCredential(initializer);
            }

            return credential;
        }

        public DriveService AuthenticateDriveServiceAccount()
        {
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = GetServiceAccount(),
                ApplicationName = _configuration.ApplicationName,
            });
            return service;
        }

        private FilesResource.ListRequest BuildDriveListRequest(DriveService driveService, string folderId)
        {
            var listRequest = driveService.Files.List();
            listRequest.PageSize = 100;
            listRequest.SupportsAllDrives = true;
            listRequest.IncludeItemsFromAllDrives = true;
            listRequest.Fields = "files(id, name, parents, trashed)";
            listRequest.Q = $"'{folderId}' in parents and trashed=false and (mimeType contains 'image/')";

            return listRequest;
        }
    }
}
