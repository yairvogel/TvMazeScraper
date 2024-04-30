using TvMaze.Client;
using TvMaze.Interfaces;

namespace TvMaze.Scraper;

public class ScraperOperations(CancellationTokenSource cts, TvmazeClient client, IDocumentDbClient dbClient)
{
    public async Task<IEnumerable<ShowResponse>> GetShowsAsync(int page)
    {
        List<ShowResponse> shows = await client.GetShowsAsync(page, cts.Token);
        if (shows.Count == 0)
        {
            // no more shows to fetch, cancel the token
            cts.Cancel();
        }
        return shows;
    }

    public async Task<ShowResponseWithCast> GetCastAsync(ShowResponse show)
    {
        List<CastResponse> cast = await client.GetCastAsync(show.Id, cts.Token);
        return new ShowResponseWithCast(show, cast);
    }

    public Task SaveToDbAsync(ShowResponseWithCast result)
    {
        Show show = new(
            result.Show.Id,
            result.Show.Name,
            result.Cast.Select(c => new Cast(c.Person.Id, c.Person.Name, c.Person.Birthday)).ToList()
        );

        return dbClient.SetItem(show.Id.ToString(), show);
    }

    public record ShowResponseWithCast(ShowResponse Show, List<CastResponse> Cast);
}