using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace OWTournamentsHistory.Tasks
{
    [Obsolete]
    public class ExportDatabaseInvocable : BaseInvocable<ExportDatabaseInvocable>
    {
        private readonly IMongoDatabase _database;

        public ExportDatabaseInvocable(IMongoDatabase database, ILogger<ExportDatabaseInvocable> logger)
            : base(logger)
        {
            _database = database;
        }

        protected override async Task InvokeInternal()
        {
            var fileNames = new Dictionary<string, string>();
            var directory = Path.Combine(Path.GetTempPath(), DateTime.UtcNow.ToString("yyyy-MM-dd hh-mm-ss"));
            Debug.WriteLine(directory);
            Directory.CreateDirectory(directory);
            var collections = (await _database.ListCollectionNamesAsync()).ToList();
            foreach (var collectionName in collections)
            {
                var sb = new StringBuilder("[");
                var collection = _database.GetCollection<RawBsonDocument>(collectionName);

                using (var cursor = await collection.FindAsync(new BsonDocument()))
                {
                    while (await cursor.MoveNextAsync())
                    {
                        var batch = cursor.Current;
                        var mapped = batch.Select(BsonTypeMapper.MapToDotNetValue);
                        var json = JsonSerializer.Serialize(mapped);
                        sb.Append(json[1..^1]);
                        sb.Append(',');
                    }
                    sb = sb.Remove(sb.Length - 1, 1);
                    sb.Append(']');

                    var fileName = $"{Path.Combine(directory, collectionName)}.json";
                    fileNames.Add(collectionName, fileName);
                    File.WriteAllText(fileName, sb.ToString());
                }
            }

            ZipFiles(directory, fileNames);
        }

        void ZipFiles(string directory, IReadOnlyDictionary<string, string> fileNames)
        {
            var zipName = $"{Path.Combine(directory, "DatabaseBackup")}.zip";
            using (var archiveFileStream = new FileStream(zipName, FileMode.Create))
            {
                using (var archive = new ZipArchive(archiveFileStream, ZipArchiveMode.Create, false))
                {
                    foreach (var item in fileNames)
                    {
                        var file = archive.CreateEntry(item.Key + ".json");
                        using (var entryStream = file.Open())
                        using (var reader = File.Open(item.Value, FileMode.Open, FileAccess.Read))
                        {
                            reader.CopyTo(entryStream);
                        }
                    }
                }
            }
        }
    }
}
