
namespace TmsApi.Application.DTOs;

public class PagedRequest
{
    private int _pageSize = 10;

    public int Page { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > 50 ? 50 : value; // HARD CAP
    }

    public string? Search { get; set; }

    public string? SortBy { get; set; } = "Code";

    public bool Desc { get; set; } = false;
}