using Elastic.Clients.Elasticsearch;

namespace EsDemo;

public class ProductRepository(ElasticsearchClient client)
    : BaseRepository<Product>(client, "products"), IProductRepository
{
}