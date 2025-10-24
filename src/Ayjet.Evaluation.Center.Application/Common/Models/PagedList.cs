namespace Ayjet.Evaluation.Center.Application.Common.Models;

public class PagedList<T>
{
    public List<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; } // <-- ADD THIS PROPERTY
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageNumber = pageNumber;
        PageSize = pageSize; // <-- ASSIGN THE VALUE
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }
}