using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

namespace EsDemo;

public interface IBaseRepository<T>
{
    Task<bool> AddBulkAsync(IReadOnlyCollection<T> documents);
    Task<bool> AddAsync(T document);
    Task<T> GetAsync(string id);
    Task<IReadOnlyList<T>> AllAsync();
    Task<IReadOnlyList<T>?> QueryAsync(QueryDescriptor<T> predicate);
    Task<bool> RemoveAsync(string id);
    Task<bool> RemoveAllAsync();

    Task<PagedList<T>> AllPagedListAsync(
        int pageNumber = 0, int pageSize = 10,
        string? sortBy = null, string? searchText = null);

    Task<PagedList<T>?> QueryPagedListAsync(
        QueryDescriptor<T> predicate, int pageNumber = 0, int pageSize = 10,
        string? sortBy = null, string? searchText = null);

    Task<long> TotalCountAsync();
    Task<bool> ExistAsync(string id);
    Task<bool> UpdateAsync(string id, T document);
}

public class BaseRepository<T>(ElasticsearchClient client, string indexName) : IBaseRepository<T>
{
    public async Task<bool> AddBulkAsync(IReadOnlyCollection<T> documents)
    {
        var indexResponse = await client.BulkAsync(b => b
            .Index(indexName)
            .IndexMany(documents)
        );
        return indexResponse.IsValidResponse;
    }

    public async Task<bool> AddAsync(T document)
    {
        var indexResponse = await client.IndexAsync(document);
        return indexResponse.IsValidResponse;
    }

    public async Task<T> GetAsync(string id)
    {
        var response = await client.GetAsync<T>(indexName, id);
        return response.Source;
    }

    public async Task<IReadOnlyList<T>> AllAsync()
    {
        var searchResponse = await client.SearchAsync<T>(s => s.Index(indexName)
            .Query(new QueryDescriptor<T>()
                .MatchAll(new MatchAllQuery())));
        return searchResponse.Documents.ToList().AsReadOnly();
    }

    public async Task<IReadOnlyList<T>?> QueryAsync(QueryDescriptor<T> predicate)
    {
        var searchResponse = await client.SearchAsync<T>(s => s.Index(indexName)
            .Query(predicate));
        return searchResponse.Documents.ToList().AsReadOnly();
    }

    public async Task<bool> RemoveAsync(string id)
    {
        var response = await client.DeleteAsync(index: indexName, id);
        return response.IsValidResponse;
    }

    public async Task<bool> RemoveAllAsync()
    {
        var response =
            await client.DeleteByQueryAsync<T>(indexName, q =>
                q.Query(new QueryDescriptor<T>()
                    .MatchAll(new MatchAllQuery())));
        return response.IsValidResponse;
    }


    public async Task<PagedList<T>> AllPagedListAsync(int pageNumber = 0, int pageSize = 10,
        string? sortBy = null, string? searchText = null)
    {
        var from = ((pageNumber - 1) * pageSize);

        var sortFields = sortBy.Split(',').Select(x =>
        {
            var parts = x.Split(' ');
            var fieldName = parts[0];
            var order = parts.Length > 1 ? parts[1] : "asc";

            var sortOrder = order == "asc" ? SortOrder.Asc : SortOrder.Desc;

            var field = fieldName.Contains("At") ? fieldName : string.Concat(fieldName, ".keyword");

            Action<SortOptionsDescriptor<T>> sortOptions
                = x => x.Field(field, f => f.Order(sortOrder));

            return sortOptions;
        }).ToArray();

        var response = await client
            .SearchAsync<T>(s => s
                .From(from)
                .Size(pageSize)
                .Index(indexName)
                .Sort(sortFields)
                .Query(q =>
                {
                    if (searchText is { Length: > 0 })
                    {
                        q.QueryString(m => m
                            .Fields(new[] { "*" })
                            .Query("*" + searchText + "*")
                        );
                    }
                    else
                    {
                        q.MatchAll(new MatchAllQuery());
                    }
                })
            );

        return new PagedList<T>(response.Documents, response.Total, from, pageSize);
    }

    public async Task<PagedList<T>?> QueryPagedListAsync(
        QueryDescriptor<T> predicate, int pageNumber = 0, int pageSize = 10,
        string? sortBy = null, string? searchText = null)
    {
        var from = ((pageNumber - 1) * pageSize);

        var response = await client
            .SearchAsync<T>(s => s
                .From(from)
                .Size(pageSize)
                .Index(indexName)
                .Query(predicate)
            );

        return new PagedList<T>(response.Documents, response.Total, pageNumber, pageSize);
    }

    public async Task<long> TotalCountAsync()
    {
        var response = await client
            .CountAsync(new CountRequestDescriptor(indexName));
        return response.Count;
    }

    public async Task<bool> ExistAsync(string id)
    {
        var response = await client.ExistsAsync(index: indexName, id);
        return response.IsValidResponse;
    }

    public async Task<bool> UpdateAsync(string id, T document)
    {
        var response = await client.UpdateAsync<T, T>(indexName, id,
            doc => doc.Doc(document));
        return response.IsValidResponse;
    }
}

public record PagedList<T>(
    IReadOnlyCollection<T> Data,
    long TotalCount,
    int PageNumber,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}