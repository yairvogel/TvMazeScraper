using System.Text.Json;
using TvMaze.Interfaces;

namespace TvMaze.MongoDb;
public class LocalFileSystemDocumentDbClient : IDocumentDbClient
{
    private readonly string _workDir;

    public LocalFileSystemDocumentDbClient()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _workDir = Path.Combine(appData, "TvMaze");
        Console.WriteLine($"Using work directory: {_workDir}");
        Directory.CreateDirectory(_workDir);
    }

    public async Task<bool> SetItem<T>(string key, T item)
    {
        string collectionName = typeof(T).Name;
        Directory.CreateDirectory(Path.Combine(_workDir, collectionName));
        await using FileStream fileStream = File.Create(Path.Combine(_workDir, collectionName, key));
        await fileStream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(item));
        return true;
    }

    public async Task<T?> GetItem<T>(string key)
    {
        string collectionName = typeof(T).Name;
        string filePath = Path.Combine(_workDir, collectionName, key);
        if (!File.Exists(filePath))
        {
            return default;
        }

        await using FileStream fileStream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<T>(fileStream);
    }
}