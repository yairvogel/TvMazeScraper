using System.Net;
using System.Net.Http.Json;

namespace TvMaze.Client;

public class TvmazeClient
{
    private readonly bool _verbose;
    private readonly TimeSpan _retryDelay;
    private readonly HttpClient _httpClient;

    public TvmazeClient(TimeSpan retryDelay, bool verbose)
    {
        _verbose = verbose;
        _retryDelay = retryDelay;
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("http://api.tvmaze.com/")
        };
    }

    public async Task<List<ShowResponse>> GetShowsAsync(int page, CancellationToken cancellationToken)
    {
        return await GetWithRetriesAsync<List<ShowResponse>>($"shows?page={page}", cancellationToken) ?? [];
    }

    public async Task<List<CastResponse>> GetCastAsync(int showId, CancellationToken cancellationToken)
    {
        return await GetWithRetriesAsync<List<CastResponse>>($"shows/{showId}/cast", cancellationToken) ?? [];
    }
    
    private async Task<T?> GetWithRetriesAsync<T>(string url, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return default;
                }

                using HttpResponseMessage response = await _httpClient.GetAsync(url, cancellationToken);
                if (_verbose)
                {
                    Console.WriteLine($"Request to {url} returned {response.StatusCode}");
                }

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
                    case HttpStatusCode.NotFound:
                        return default;
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
        catch (TaskCanceledException)
        {
            return default;
        }
    }
}