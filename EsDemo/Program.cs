using EsDemo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddSingleton<IProductRepository, ProductRepository>();
builder.Services.AddSingleton(typeof(IBaseRepository<>), typeof(BaseRepository<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapGet("/products",
    async (IProductRepository productRepository, [AsParameters] BaseQueryParameterDto queryParameterDto) =>
    {
        var products = await productRepository.AllPagedListAsync(
            queryParameterDto.PageNumber,
            queryParameterDto.PageSize,
            queryParameterDto.SortBy,
            queryParameterDto.SearchString);

        return Results.Ok(products);
    });

app.MapGet("/products/{id}", async (IProductRepository productRepository, string id) =>
{
    var product = await productRepository.GetAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPost("/products", async (IProductRepository productRepository, Product product) =>
{
    var result = await productRepository.AddAsync(product);
    return result ? Results.Created("/products", product) : Results.Problem("Unable to add product.");
});

app.MapPut("/products/{id}", async (IProductRepository productRepository, string id, Product product) =>
{
    var result = await productRepository.UpdateAsync(id, product);
    return result ? Results.NoContent() : Results.Problem("Unable to update product.");
});

app.MapDelete("/products/{id}", async (IProductRepository productRepository, string id) =>
{
    var result = await productRepository.RemoveAsync(id);
    return result ? Results.NoContent() : Results.Problem("Unable to delete product.");
});

app.Run();