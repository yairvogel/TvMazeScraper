using MongoDB.Driver;
using TvMaze.Interfaces;

namespace TvMaze.MongoDb;

public class MongoDbDocumentClient(string connectionString) : IDocumentDbClient
{
    public const string Database = "tvmaze";

    private const string _showCollectionName = "shows";

    private readonly IMongoDatabase client = new MongoClient(connectionString).GetDatabase(Database);

    /// <summary>
    /// initializes the database with the required collections, and returns a client to the initialized database
    /// </summary>
    public static async Task<MongoDbDocumentClient> InitializeAsync(string connectionString)
    {
        MongoDbDocumentClient client = new(connectionString);
        await client.client.CreateCollectionAsync(_showCollectionName);
        return client;
    }

    public async Task<Show?> GetItem(int key)
    {
        IMongoCollection<Show> collection = client.GetCollection<Show>(_showCollectionName);
        FilterDefinition<Show> filter = Builders<Show>.Filter.Eq(show => show.Id, key);
        IAsyncCursor<Show> cursor = await collection.FindAsync(filter);
        return await cursor.FirstOrDefaultAsync();
    }

    public async Task<ICollection<Show>> GetItems(Range range)
    {
        IMongoCollection<Show> collection = client.GetCollection<Show>(_showCollectionName);
        FilterDefinition<Show> filter = Builders<Show>.Filter.And(
                Builders<Show>.Filter.Gte(s => s.Id, range.Start.Value),
                Builders<Show>.Filter.Lt(s => s.Id, range.End.Value));

        IAsyncCursor<Show> cursor = await collection.FindAsync(filter);
        return await cursor.ToListAsync();
    }

    public Task<bool> SetItem(string key, Show item)
    {
        throw new NotImplementedException();
    }
}
