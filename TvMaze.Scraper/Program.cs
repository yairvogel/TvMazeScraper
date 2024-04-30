using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using TvMaze.Client;
using TvMaze.Interfaces;
using TvMaze.MongoDb;
using TvMaze.Scraper;

TvmazeClient client = new();
IDocumentDbClient dbClient = new LocalFileSystemDocumentDbClient();
CancellationTokenSource cts = new();

var fetchShowBlock = new TransformManyBlock<int, ShowResponse>(GetShowsAsync, new() { MaxDegreeOfParallelism = 1, BoundedCapacity = 2 });
var fetchCastBlock = new TransformBlock<ShowResponse, ShowResponseWithCast>(GetCastAsync, new() { MaxDegreeOfParallelism = 5, BoundedCapacity = 300 });
var saveBlock = new ActionBlock<ShowResponseWithCast>(SaveToDbAsync, new() { MaxDegreeOfParallelism = 1 });

using IDisposable link = fetchShowBlock.LinkTo(fetchCastBlock, new() { PropagateCompletion = true });
using IDisposable link2 = fetchCastBlock.LinkTo(saveBlock, new() { PropagateCompletion = true });

// we run to infinity until there are no more shows to fetch (we get a null response). We cancel the token when there are no more results.

Stopwatch sw = Stopwatch.StartNew();
IEnumerable<int> pagesToFetch = Enumerable.Range(30, 35);
await fetchShowBlock.SendAllAsync(pagesToFetch, cts.Token);
await saveBlock.Completion;

async Task<IEnumerable<ShowResponse>> GetShowsAsync(int page)
{
    List<ShowResponse> shows = await client.GetShowsAsync(page, cts.Token);
    if (shows.Count == 0)
    {
        // no more shows to fetch, cancel the token
        cts.Cancel();
    }
    return shows;
}

async Task<ShowResponseWithCast> GetCastAsync(ShowResponse show)
{
    List<Cast> cast = await client.GetCastAsync(show.Id, cts.Token);
    return new ShowResponseWithCast(show, cast);
}

Task SaveToDbAsync(ShowResponseWithCast showWithCast)
{
    return dbClient.SetItem(showWithCast.Show.Id.ToString(), showWithCast);
}

static IEnumerable<int> Count()
{
    for (int i = 0; i < int.MaxValue; i++)
    {
        yield return i;
    }
}

public record ShowResponseWithCast(ShowResponse Show, List<Cast> Cast);