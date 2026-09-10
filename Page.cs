using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity;

public static class PageOptions
{
    public static readonly int pageSizeMin = 3;
    public static readonly int pageSizeMax = 100;
}

public class Page<T>
{
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int Total { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public IEnumerable<T> Items { get; init; } = [];

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public static async Task<Page<T>> CreateAsync(
        IQueryable<T> query,
        int pageNumber = 1,
        int? pageSize = null
    )
    {
        // prevent negative page number values
        pageNumber = pageNumber < 1 
            ? 1 
            : pageNumber;

        // clamp page size to prevent too many row retrievals
        // as well as negative page size values
        var pageSizeFinal = Math.Min(
            Math.Max(
                pageSize ?? PageOptions.pageSizeMin, 
                PageOptions.pageSizeMin
            ), 
            PageOptions.pageSizeMax
        );

        int total = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSizeFinal)
            .Take(pageSizeFinal)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(total / (double)pageSizeFinal);

        return new Page<T>
        {
            PageNumber = totalPages > 0 
                ? pageNumber 
                : 0,
            PageSize = pageSizeFinal,
            Total = total,
            TotalItems = items.Count,
            TotalPages = totalPages,
            Items = items
        };
    }
}
