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