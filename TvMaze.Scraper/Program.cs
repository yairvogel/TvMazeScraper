using System.Threading.Tasks.Dataflow;
using TvMaze.Client;
using TvMaze.Scraper;

TvmazeClient client = new();
CancellationTokenSource cts = new();

ExecutionDataflowBlockOptions options = new() { MaxDegreeOfParallelism = 10, BoundedCapacity = 50 };
TransformManyBlock<int, ShowResponse> fetchShowBlock = new(async i =>
{
    Console.WriteLine($"Fetching show {i}");
    ShowResponse? show = await client.GetShowAsync(i, cts.Token);
    if (show is null)
    {
        // no more shows to fetch, cancel the token
        cts.Cancel();
        return [];
    }
    Console.WriteLine($"Fetched show {show.Name}");
    return [show];
}, options);

cts.CancelAfter(TimeSpan.FromSeconds(10));

// we run to infinity until there are no more shows to fetch (we get a null response)
IEnumerable<int> idsToFetch = CountToInfinity();
Task sendTask = fetchShowBlock.SendAllAsync(idsToFetch, cts.Token);
Task<List<ShowResponse>> toListTask = fetchShowBlock.ToListAsync();
await Task.WhenAll(sendTask, toListTask);
List<ShowResponse> list = toListTask.Result;

Console.WriteLine(list.Count);

static IEnumerable<int> CountToInfinity()
{
    int i = 1;
    while (true)
    {
        yield return i++;
    }
}
