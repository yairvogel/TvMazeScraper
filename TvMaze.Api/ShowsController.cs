using Microsoft.AspNetCore.Mvc;
using TvMaze.Interfaces;

namespace TvMaze.Api;

[ApiController]
[Route("[controller]")]
public class ShowsController(IDocumentDbClient documentDbClient) : ControllerBase
{
    /// <summary>
    /// Get Paginated Shows
    /// </summary>
    /// <param name="page">page number</param>
    /// <param name="pageSize">page size</param>
    /// <param name="order">order to sort the cast members by (using their birthday). accepted values are "asc" and "desc"</param>
    [HttpGet]
    [ProducesResponseType(200, Type = typeof(Show[]))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetShows(int page = 0, int pageSize = 10, string? order = null)
    {
        int start = page * pageSize;
        int end = (page + 1) * pageSize;

        ICollection<Show> shows = await documentDbClient.GetItems(start..end);

        shows = shows.Select(s => WithSortedCast(s, order)).ToArray();
        return shows.Count > 0 ? Ok(shows) : NotFound();
    }

    /// <summary>
    /// Get a show by its id
    /// </summary>
    /// <param name="id">The show id</param>
    /// <param name="order">order to sort the cast members by (using their birthday). accepted values are "asc" and "desc"</param>
    /// </param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(200, Type = typeof(Show))]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetShowById(int id, string? order = null)
    {
        Show? show = await documentDbClient.GetItem(id);

        if (show is null)
        {
            return NotFound();
        }

        show = WithSortedCast(show, order);
        return Ok(show);
    }

    private static Show WithSortedCast(Show show, string? orderStr)
    {
        Enum? order = ParseOrder(orderStr);
        if (order is null || show.Cast.Count <= 1)
        {
            return show;
        }

        return order switch
        {
            Order.Asc => show with { Cast = [.. show.Cast.OrderBy(c => c.Birthday)] },
            Order.Desc => show with { Cast = [.. show.Cast.OrderByDescending(c => c.Birthday)] },
            _ => throw new NotImplementedException("Unreachable"),
        };
    }

    private static Order? ParseOrder(string? orderStr)
    {
        return Enum.TryParse(orderStr, true, out Order order) ? order : null;
    }

    public enum Order
    {
        Asc,
        Desc
    }
}
