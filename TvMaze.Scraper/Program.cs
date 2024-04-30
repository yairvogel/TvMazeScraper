using System.Threading.Tasks.Dataflow;
using TvMaze.Client;
using TvMaze.Interfaces;
using TvMaze.MongoDb;
using TvMaze.Scraper;
using static TvMaze.Scraper.ScraperOperations;

TvmazeClient tvMazeClient = new(retryDelay: TimeSpan.FromMilliseconds(500));
IDocumentDbClient dbClient = new LocalFileSystemDocumentDbClient();
CancellationTokenSource cts = new();
ScraperOperations scraper = new(cts, tvMazeClient, dbClient);

TransformManyBlock<int, ShowResponse> fetchShowBlock = new(scraper.GetShowsAsync, new() { MaxDegreeOfParallelism = 1, BoundedCapacity = 2 });
TransformBlock<ShowResponse, ShowResponseWithCast> fetchCastBlock = new(scraper.GetCastAsync, new() { MaxDegreeOfParallelism = 5, BoundedCapacity = 300 });
ActionBlock<ShowResponseWithCast> saveBlock = new(scraper.SaveToDbAsync, new() { MaxDegreeOfParallelism = 1 });

using IDisposable link = fetchShowBlock.LinkTo(fetchCastBlock, new() { PropagateCompletion = true });
using IDisposable link2 = fetchCastBlock.LinkTo(saveBlock, new() { PropagateCompletion = true });

IEnumerable<int> pagesToFetch = Enumerable.Range(30, 35);
await fetchShowBlock.SendAllAndCancelAsync(pagesToFetch, cts.Token);
await saveBlock.Completion;