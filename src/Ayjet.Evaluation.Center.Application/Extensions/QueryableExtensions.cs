using Ayjet.Evaluation.Center.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Ayjet.Evaluation.Center.Application.Extensions;

public static class QueryableExtensions
{
    public static async Task<PagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var count = await source.CountAsync(ct);
        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}