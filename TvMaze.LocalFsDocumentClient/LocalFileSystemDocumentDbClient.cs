using System.Text.Json;
using TvMaze.Interfaces;

namespace TvMaze.LocalFsDocumentClient;
public class LocalFileSystemDocumentDbClient : IDocumentDbClient
{
    private const string _showDirectory = "shows";
    private readonly string _workDir;

    public LocalFileSystemDocumentDbClient()
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _workDir = Path.Combine(appData, "TvMaze");
        Console.WriteLine($"Using work directory: {_workDir}");
        Directory.CreateDirectory(_workDir);
    }

    public async Task<bool> SetItem(string key, Show item)
    {
        Directory.CreateDirectory(Path.Combine(_workDir, _showDirectory));
        await using FileStream fileStream = File.Create(Path.Combine(_workDir, _showDirectory, key));
        await fileStream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(item));
        return true;
    }

    public Task<Show?> GetItem(int key)
    {
        string filePath = Path.Combine(_workDir, _showDirectory, key.ToString());
        return DeserializeFile<Show>(filePath);
    }

    public async Task<ICollection<Show>> GetItems(Range range)
    {
        string dirPath = Path.Combine(_workDir, _showDirectory);

        int start = range.Start.Value;
        int end = range.End.Value;
        IEnumerable<Task<Show?>> tasks = Enumerable.Range(start, end - start)
            .Select(i => DeserializeFile<Show>(Path.Combine(dirPath, i.ToString())));
            
        var results = await Task.WhenAll(tasks);
        return results.Where(s => s is not null).Cast<Show>().ToList();
    }

    private async Task<T?> DeserializeFile<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }

        await using FileStream fileStream = File.OpenRead(filePath);
        return await JsonSerializer.DeserializeAsync<T>(fileStream);
    }
}
