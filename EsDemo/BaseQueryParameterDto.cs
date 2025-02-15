namespace EsDemo;

public class BaseQueryParameterDto : BasePaginationParameters
{
    public string? SearchString { get; set; }

    //Name desc
    //Name asc
    public string? SortBy { get; set; }
}