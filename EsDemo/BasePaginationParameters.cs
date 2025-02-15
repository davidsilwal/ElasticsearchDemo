using System.ComponentModel;

namespace EsDemo;

public abstract class BasePaginationParameters
{
    [DefaultValue(1)] public int PageNumber { get; set; } = 1;

    [DefaultValue(10)] public int PageSize { get; set; } = 10;
}