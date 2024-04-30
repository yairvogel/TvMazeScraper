using System.CommandLine;
using System.Threading.Tasks.Dataflow;
using TvMaze.Client;
using TvMaze.Interfaces;
using TvMaze.LocalFsDocumentClient;
using TvMaze.Scraper;
using static TvMaze.Scraper.ScraperOperations;

Option<bool> verbose = new("--verbose", "Enable verbose logging");
Option<bool> sample = new("--sample", "Run the scraper for only a small sample");

var rootCommand = new RootCommand
{
    verbose,
    sample
};

rootCommand.SetHandler(Main, verbose, sample);

await rootCommand.InvokeAsync(args);

static async Task Main(bool verbose, bool sample)
{
    Console.WriteLine("Initializing scraper. verbose={0}, sample={1}", verbose, sample);

    TvmazeClient tvMazeClient = new(retryDelay: TimeSpan.FromMilliseconds(500), verbose);
    IDocumentDbClient dbClient = new LocalFileSystemDocumentDbClient();
    CancellationTokenSource cts = new();
    ScraperOperations scraper = new(cts, tvMazeClient, dbClient, verbose);

    TransformManyBlock<int, ShowResponse> fetchShowBlock = new(scraper.GetShowsAsync, new() { MaxDegreeOfParallelism = 1, BoundedCapacity = 2 });
    TransformBlock<ShowResponse, ShowResponseWithCast> fetchCastBlock = new(scraper.GetCastAsync, new() { MaxDegreeOfParallelism = 5, BoundedCapacity = 300 });
    ActionBlock<ShowResponseWithCast> saveBlock = new(scraper.SaveToDbAsync, new() { MaxDegreeOfParallelism = 1 });

    using IDisposable link = fetchShowBlock.LinkTo(fetchCastBlock, new() { PropagateCompletion = true });
    using IDisposable link2 = fetchCastBlock.LinkTo(saveBlock, new() { PropagateCompletion = true });

    IEnumerable<int> pagesToFetch = sample ? [0] : CountToInfinity();
    await fetchShowBlock.SendAllAndCancelAsync(pagesToFetch, cts.Token);
    await saveBlock.Completion;
}

static IEnumerable<int> CountToInfinity()
{
    int i = 0;
    while (true)
    {
        yield return i++;
    }
}