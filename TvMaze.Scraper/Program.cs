using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using TvMaze.Client;
using TvMaze.Scraper;

TvmazeClient client = new();
CancellationTokenSource cts = new();

TransformManyBlock<int, ShowResponse> fetchShowBlock = new(async i =>
{
    Console.WriteLine($"Fetching show {i}");
    List<ShowResponse> shows = await client.GetShowsAsync(i, cts.Token);
    if (shows.Count == 0)
    {
        Console.WriteLine("No more shows to fetch");
        // no more shows to fetch, cancel the token
        cts.Cancel();
    }
    return shows;
}, new() { MaxDegreeOfParallelism = 1, BoundedCapacity = 2 });

TransformBlock<ShowResponse, ShowResponseWithCast> fetchCastBlock = new(async show =>
{
    Console.WriteLine($"Fetching cast for show {show.Id}");
    List<Cast> cast = await client.GetCastAsync(show.Id, cts.Token) ;
    return new ShowResponseWithCast(show, cast);
}, new() { MaxDegreeOfParallelism = 10, BoundedCapacity = 300 });

using IDisposable link = fetchShowBlock.LinkTo(fetchCastBlock, new() { PropagateCompletion = true });

cts.CancelAfter(TimeSpan.FromSeconds(10));

// we run to infinity until there are no more shows to fetch (we get a null response)
IEnumerable<int> idsToFetch = CountToInfinity();
Task sendTask = fetchShowBlock.SendAllAsync(idsToFetch, cts.Token);
Task<List<ShowResponseWithCast>> toListTask = fetchCastBlock.ToListAsync();
await Task.WhenAll(sendTask, toListTask);
List<ShowResponseWithCast> list = toListTask.Result;

Console.WriteLine(list.Count);
Console.WriteLine(JsonSerializer.Serialize(list[..3], new() { WriteIndented = true }));

static IEnumerable<int> CountToInfinity()
{
    int i = 0;
    while (true)
    {
        yield return i++;
    }
}

public record ShowResponseWithCast(ShowResponse Show, List<Cast> Cast);