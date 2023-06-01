using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

namespace google_drive_api
{
    internal class Program
    {
        static string[] Scopes = { DriveService.Scope.Drive };
        static string ApplicationName = "Your Application Name";

        static async Task Main(string[] args)
        {
            await UploadToGoogleDrive("google drive folder id");
        }

        const string CREDENTIAL_JSON = "credentials.json";
        const string CREDENTIAL_PATH = "token-response";
        const string PATH_TO_IMAGE_TO_UPLOAD = "path/to/image.jpg";

        /// <summary>
        /// upload and overwrite the existing image file to google drive folder
        /// </summary>
        /// <param name="fileId">file ID of the image you want to overwrite</param>
        private static async Task UploadAndOverwriteExistingFile(string fileId)
        {
            UserCredential credential;
            using (var stream = new FileStream(CREDENTIAL_JSON, FileMode.Open, FileAccess.Read))
            {
                var secret = GoogleClientSecrets.FromStream(stream);
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    secret.Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CREDENTIAL_PATH, true));
                Console.WriteLine("Credential file saved to: " + CREDENTIAL_PATH);
            }

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Read the image file
            byte[] imageBytes = File.ReadAllBytes(PATH_TO_IMAGE_TO_UPLOAD);

            // Create the update request
            var updateRequest = service.Files.Update(new GoogleDriveFile(), fileId, new MemoryStream(imageBytes), "image/jpeg");
            updateRequest.Upload();

            Console.WriteLine("Image uploaded and overwritten successfully.");
        }

        /// <summary>
        /// upload image file to google drive folder
        /// </summary>
        /// <param name="googleDriveFolderId">google drive folder id</param>
        private static async Task UploadToGoogleDrive(string googleDriveFolderId)
        {
            UserCredential credential;

            using (var stream = new FileStream(CREDENTIAL_JSON, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CREDENTIAL_PATH, true));
                Console.WriteLine("Credential file saved to: " + CREDENTIAL_PATH);
            }

            // Create Drive API service.
            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define the file parameters
            var fileMetadata = new GoogleDriveFile()
            {
                Name = "example.jpg",   // name to save in google drive folder
                Parents = new List<string> { googleDriveFolderId } // Set the ID of the folder where you want to upload the image
            };

            FilesResource.CreateMediaUpload request;
            using (var stream = new FileStream(PATH_TO_IMAGE_TO_UPLOAD, FileMode.Open))
            {
                request = service.Files.Create(fileMetadata, stream, "image/jpeg");
                request.Upload();
            }

            var file = request.ResponseBody;
            Console.WriteLine("Image uploaded and overwritten successfully. File Id " + file.Id);
        }
    }
}