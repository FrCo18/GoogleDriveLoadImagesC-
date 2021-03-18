using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using File = Google.Apis.Drive.v3.Data.File;
using System;
using System.Collections.Generic;
using System.IO;
using Google.Apis.Download;
using Google.Apis.Util.Store;
using System.Threading;
using Google.Apis.Services;

namespace MARAFON
{
    public class GoogleDriveApi
    {
        static string[] Scopes = { DriveService.Scope.Drive,
                                         DriveService.Scope.DriveAppdata,
                                         DriveService.Scope.DriveFile,
                                         DriveService.Scope.DriveMetadataReadonly,
                                         DriveService.Scope.DriveReadonly,
                                         DriveService.Scope.DriveScripts};
        private static string ApplicationName = "CollegePracticeImages";
        private static string FolderId = "1kEhKSgJIZ3Aaer9Ma4ZG0vkdlswN5yVP";
        //private static string fileName = "fqaw.jpg";
        //private static string filePath = @"fqaw.jpg";
        private static string contentType = "image/jpeg/png";
        //static string downloadPath = @"C:\Users\FrCo18PC\Desktop\test.jpg";
        private static string downloadPath = System.IO.Path.GetTempPath() + "tempImage.jpg";
        private static DriveService service;
        private static UserCredential credential;

        public GoogleDriveApi()
        {
            credential = GetUserCredential();
            service = GetDriveService(credential);
        }

        public void DeleteImage(string fileId)
        {
            service.Files.Delete(fileId).Execute();
        }

        public string UploadFileToDrive(string filePath)
        {
            var arr = filePath.Split('\\');
            var fileName = arr[arr.Length - 1];
            File file = new File();
            file.Name = fileName;
            file.MimeType = contentType;
            file.Parents = new List<string> { FolderId };

            byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
            MemoryStream stream = new MemoryStream(byteArray);
            FilesResource.CreateMediaUpload request = service.Files.Create(file, stream, contentType);
            request.Upload();
            var fileEnd = request.ResponseBody;
            Console.WriteLine("FILe ID :" + fileEnd.Id);
            return fileEnd.Id;
        }

        public string DownloadFileFromGoogleDrive(string fileId)
        {
            var request = service.Files.Get(fileId);
            using (var memoryStream = new MemoryStream())
            {
                request.MediaDownloader.ProgressChanged += (IDownloadProgress progress) =>
                {
                    switch (progress.Status)
                    {
                        case DownloadStatus.Downloading:
                            Console.WriteLine(progress.BytesDownloaded);
                            break;
                        case DownloadStatus.Completed:
                            Console.WriteLine("Download complete!");
                            break;
                        case DownloadStatus.Failed:
                            Console.WriteLine("Download failed");
                            break;
                    }
                };

                request.Download(memoryStream);

                using (var fileStream = new FileStream(downloadPath, FileMode.Create, FileAccess.Write))
                {
                    fileStream.Write(memoryStream.GetBuffer(), 0, memoryStream.GetBuffer().Length);
                }
            }

            return downloadPath;
        }



        private static UserCredential GetUserCredential()
        {
            using (var stream =
               new FileStream("../../resources/GoogleDrive/client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, "college practice images", "college-practice-images");
                return GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }
        }

        private static DriveService GetDriveService(UserCredential credential)
        {
            return new DriveService(
                new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName
                }
                );
        }
    }
}
