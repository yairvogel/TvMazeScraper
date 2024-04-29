using System.Threading.Tasks.Dataflow;

namespace TvMaze.Scraper;

internal static class IDataflowExtensions
{
    internal static async Task<List<T>> ToListAsync<T>(this IReceivableSourceBlock<T> sourceBlock)
    {
        List<T> list = [];
        try
        {
            await foreach (T item in sourceBlock.ReceiveAllAsync())
            {
                list.Add(item);
            }
            return list;
        }
        catch (TaskCanceledException)
        {
            return list;
        }
        finally
        {
            await sourceBlock.Completion;
        }
    }

    internal static async Task SendAllAsync<T>(this ITargetBlock<T> target, IEnumerable<T> items, CancellationToken cancellationToken)
    {
        try
        {
            foreach (T i in items)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                await target.SendAsync(i, cancellationToken);
            }
        }
        catch (TaskCanceledException)
        {
        }
        finally
        {
            target.Complete();
            await target.Completion;
        }

    }
}