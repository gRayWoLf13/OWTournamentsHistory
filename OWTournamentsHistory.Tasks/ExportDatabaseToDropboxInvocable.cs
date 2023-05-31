using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.Text;
using Dropbox.Api;
using MongoDB.Bson.IO;
using Microsoft.Extensions.Options;
using OWTournamentsHistory.Common.Settings;

namespace OWTournamentsHistory.Tasks
{
    public class ExportDatabaseToDropboxInvocable : BaseInvocable<ExportDatabaseToDropboxInvocable>
    {
        private const string _separator = "/";

        private readonly JsonWriterSettings _jsonWriterSettings = new()
        {
            OutputMode = JsonOutputMode.CanonicalExtendedJson,
        };

        private readonly IMongoDatabase _database;
        private readonly DropboxApiSettings _apiSettings;

        public ExportDatabaseToDropboxInvocable(IMongoDatabase database, ILogger<ExportDatabaseToDropboxInvocable> logger, IOptions<DropboxApiSettings> apiSettings)
            : base(logger)
        {
            _database = database;
            _apiSettings = apiSettings.Value;
        }

        protected override async Task InvokeInternal()
        {
            var dropboxClient = CreateDropboxClient();

            var directoryName = DateTime.UtcNow.ToString("yyyy-MM-dd hh-mm-ss");
            Debug.WriteLine(directoryName);

            try
            {
                var uploadedFolder = await dropboxClient.Files.CreateFolderV2Async(new Dropbox.Api.Files.CreateFolderArg(_separator + directoryName));
                Debug.WriteLine($"Dropbox folder created: {uploadedFolder.Metadata.AsFolder?.PreviewUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Dropbox directory creation failed");
                Debug.WriteLine(ex);
                throw;
            }

            await foreach (var (fileName, fileContent) in GetDatabaseCollections())
            {
                try
                {
                    var uploadedFile = await dropboxClient.Files.UploadAsync(new Dropbox.Api.Files.UploadArg(_separator + string.Join(_separator, directoryName, fileName)), fileContent);
                    Debug.WriteLine($"File {fileName} uploaded to Dropbox: {uploadedFile.Id}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Dropbox file upload failed");
                    Debug.WriteLine(ex);
                    throw;
                }
            }
        }

        private DropboxClient CreateDropboxClient() =>
            new DropboxClient(_apiSettings.AccessToken);

        private async IAsyncEnumerable<(string FileName, Stream FileContent)> GetDatabaseCollections()
        {
            var collections = (await _database.ListCollectionNamesAsync()).ToList();

            foreach (var collectionName in collections)
            {
                var collection = _database.GetCollection<RawBsonDocument>(collectionName);

                using (var memoryStream = new MemoryStream())
                using (var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true))
                using (var jsonWriter = new JsonWriter(streamWriter, _jsonWriterSettings))
                using (var cursor = await collection.FindAsync(new BsonDocument()))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        foreach (var item in cursor.Current)
                        {
                            jsonWriter.WriteRawBsonDocument(item.Slice);
                        }
                    }

                    jsonWriter.Flush();
                    memoryStream.Position = 0;

                    var fileName = $"{collectionName}.json";

                    yield return (fileName, memoryStream);
                }
            }
        }
    }
}
