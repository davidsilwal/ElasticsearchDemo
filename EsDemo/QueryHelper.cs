using System.ComponentModel;
using Elastic.Clients.Elasticsearch;

namespace EsDemo;

public abstract class BasePaginationParameters
{
    [DefaultValue(1)] public int PageNumber { get; set; } = 1;

    [DefaultValue(10)] public int PageSize { get; set; } = 10;
}

public class BaseQueryParameterDto : BasePaginationParameters
{
    public string? SearchString { get; set; }

    //Name desc
    //Name asc
    public string? SortBy { get; set; }
}


public class Product
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public interface IProductRepository : IBaseRepository<Product>
{
}

public class ProductRepository(ElasticsearchClient client)
    : BaseRepository<Product>(client, "products"), IProductRepository
{
}