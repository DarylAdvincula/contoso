using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity;

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
        int pageSize = 4
    )
    {
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 4 : pageSize;

        int total = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new Page<T>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Total = total,
            TotalItems = items.Count,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }
}
