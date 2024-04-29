using System.Net;
using System.Net.Http.Json;

namespace TvMaze.Client;

public class TvmazeClient
{
    private readonly TimeSpan _retryDelay;
    private readonly HttpClient _httpClient;

    public TvmazeClient(TimeSpan? retryDelay = null)
    {
        _retryDelay = retryDelay ?? TimeSpan.FromMilliseconds(500);
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://api.tvmaze.com/")
        };
    }

    public async Task<ShowResponse?> GetShowAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            return await GetShowWithRetriesAsync(id, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            Console.WriteLine("Task was canceled");
            return null;
        }
    }

    private async Task<ShowResponse?> GetShowWithRetriesAsync(int id, CancellationToken cancellationToken)
    {
        while (true)
        {
            using HttpResponseMessage response = await _httpClient.GetAsync($"shows/{id}?embed=cast", cancellationToken);
            switch (response.StatusCode)
            {
                case HttpStatusCode.OK:
                    return await response.Content.ReadFromJsonAsync<ShowResponse>(cancellationToken: cancellationToken);
                case HttpStatusCode.NotFound:
                    Console.WriteLine($"Show {id} not found");
                    return null;
                case HttpStatusCode.ServiceUnavailable:
                case HttpStatusCode.TooManyRequests:
                    // we don't use polly here because we want access to the response headers, which polly doesn't allow easily
                    TimeSpan waitTime = response.Headers.RetryAfter?.Delta ?? _retryDelay;
                    await Task.Delay(waitTime, cancellationToken);
                    break;
                default:
                    throw new HttpRequestException($"Unexpected status code: {response.StatusCode}");
            }
        }
    }
}