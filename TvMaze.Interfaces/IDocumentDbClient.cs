namespace TvMaze.Interfaces;

public interface IDocumentDbClient
{
    Task<bool> SetItem<T>(string key, T item);
    Task<T?> GetItem<T>(string key);
} 