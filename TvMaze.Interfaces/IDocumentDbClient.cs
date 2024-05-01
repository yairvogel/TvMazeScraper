namespace TvMaze.Interfaces;

public interface IDocumentDbClient
{
    Task<bool> SetItem(string key, Show item);

    Task<Show?> GetItem(int key);

    Task<ICollection<Show>> GetItems(Range range);
} 
