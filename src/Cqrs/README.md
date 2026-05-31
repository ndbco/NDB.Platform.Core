# NDB.Platform.Cqrs

> Namespace: `NDB.Platform.Cqrs`
> [← Back to main README](../../README.md)

Compile-time CQRS using [Mediator.SourceGenerator](https://github.com/martinothamar/Mediator) — no runtime reflection, no service locator, maximum performance. Includes logging and validation pipeline behaviors registered automatically.

---

## Setup

```csharp
// Program.cs
using NDB.Platform.Cqrs;

// 1. Register pipeline behaviors + FluentValidation discovery
builder.Services.AddNdbCqrs(typeof(Program).Assembly);

// 2. Register the Mediator source-generated dispatcher
//    Must be called AFTER AddNdbCqrs
builder.Services.AddMediator(opt => opt.ServiceLifetime = ServiceLifetime.Scoped);
```

> **Why two calls?** `AddNdbCqrs` registers behaviors and validators. `AddMediator` (from `Mediator.SourceGenerator`) generates and registers the compile-time dispatch table. The order matters.

---

## Query — Read Operations

Queries do not change state. They return data.

```csharp
// 1. Define the query (use a record for immutability):
public sealed record GetProductQuery(Guid Id) : IQuery<Result<ProductDto>>;

// 2. Implement the handler:
public sealed class GetProductHandler(AppDbContext db)
    : IQueryHandler<GetProductQuery, Result<ProductDto>>
{
    public async ValueTask<Result<ProductDto>> Handle(
        GetProductQuery query, CancellationToken ct)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == query.Id && !p.IsDeleted)
            .ProjectToType<ProductDto>()
            .FirstOrDefaultAsync(ct);

        return product is null
            ? Result.NotFound($"Product {query.Id} not found")
            : Result.Success(product);
    }
}

// 3. Dispatch from a controller or minimal API:
[HttpGet("{id:guid}")]
public async Task<IActionResult> Get(Guid id, IMediator mediator, CancellationToken ct)
{
    var result = await mediator.Send(new GetProductQuery(id), ct);
    return result.ToActionResult(); // from NDB.Platform.API
}
```

### Paginated Query

```csharp
public sealed class GetProductsQuery : IQuery<Result<PagedResult<ProductDto>>>
{
    public int Page    { get; init; } = 1;
    public int Size    { get; init; } = 20;
    public string? Search    { get; init; }
    public string? Category  { get; init; }
    public string? SortBy    { get; init; }
    public string? SortDir   { get; init; } = "asc";
}

public sealed class GetProductsHandler(AppDbContext db)
    : IQueryHandler<GetProductsQuery, Result<PagedResult<ProductDto>>>
{
    public async ValueTask<Result<PagedResult<ProductDto>>> Handle(
        GetProductsQuery q, CancellationToken ct)
    {
        var page = await db.Products
            .AsNoTracking()
            .WhereIf(q.Category is not null, p => p.Category == q.Category)
            .WhereContainsIgnoreCase(p => p.Name, q.Search, db)
            .ApplySort(q)                        // from NDB.Platform.Ef
            .ProjectToType<ProductDto>()
            .ToPagedAsync(q.Page, q.Size, ct);   // from NDB.Platform.Ef

        return Result.Success(page);
    }
}
```

---

## Command — Write Operations

Commands mutate state. They should return `Result` (void) or `Result<T>` (returning an ID or created entity).

```csharp
// Command with return value:
public sealed record CreateProductCommand : ICommand<Result<Guid>>
{
    public string  Name         { get; init; } = default!;
    public decimal Price        { get; init; }
    public string  CategoryCode { get; init; } = default!;
}

public sealed class CreateProductHandler(AppDbContext db)
    : ICommandHandler<CreateProductCommand, Result<Guid>>
{
    public async ValueTask<Result<Guid>> Handle(
        CreateProductCommand cmd, CancellationToken ct)
    {
        if (await db.Products.AnyAsync(p => p.Name == cmd.Name, ct))
            return Result.Conflict($"A product named '{cmd.Name}' already exists");

        var product = new Product
        {
            Id           = Guid.NewGuid(),
            Name         = cmd.Name,
            Price        = cmd.Price,
            CategoryCode = cmd.CategoryCode,
            CreatedAt    = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return Result.Success(product.Id);
    }
}

// Command without return value:
public sealed record DeleteProductCommand(Guid Id) : ICommand<Result>;

public sealed class DeleteProductHandler(AppDbContext db)
    : ICommandHandler<DeleteProductCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteProductCommand cmd, CancellationToken ct)
    {
        var deleted = await db.Products
            .Where(p => p.Id == cmd.Id)
            .ExecuteDeleteAsync(ct);

        return deleted == 0
            ? Result.NotFound("Product not found")
            : Result.Success();
    }
}
```

---

## Validation Pipeline

`ValidationBehavior` runs all `IValidator<TRequest>` instances **before the handler is invoked**. If any validator fails, the handler is skipped and `Result.ValidationError(errors)` is returned automatically.

```csharp
using FluentValidation;

public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Price)
            .GreaterThan(0);

        RuleFor(x => x.CategoryCode)
            .NotEmpty()
            .Length(2, 20);
    }
}
```

No manual validation calls in the handler — the pipeline handles it. The errors are available at `result.ValidationErrors` as `IDictionary<string, string[]>`.

---

## Logging Pipeline

`LoggingBehavior` logs around every request automatically:

```
[INF] Handling CreateProductCommand { Name = "Widget", Price = 9.99, CategoryCode = "ELEC" }
[INF] CreateProductCommand completed in 38ms — Success
[WRN] GetProductsQuery completed in 1847ms — Success (slow: >1000ms)
[ERR] DeleteProductCommand completed in 12ms — Error: Product not found
```

---

## IHandler Aliases

Shorter aliases for declaring handler contracts:

```csharp
// Instead of ICommandHandler / IQueryHandler, you can use:
public class MyHandler : IHandler<MyCommand, Result> { ... }
```

---

## Source Files

```
src/Cqrs/
├── ICommand.cs              ← Marker interface for commands
├── IQuery.cs                ← Marker interface for queries
├── IHandler.cs              ← Shorthand handler alias
├── DependencyInjection.cs   ← AddNdbCqrs() extension method
└── Behaviors/
    ├── LoggingBehavior.cs   ← Logs request + execution time
    └── ValidationBehavior.cs ← Runs FluentValidation before handler
```

---

> Part of [NDB.Platform.Core](https://github.com/ndbco/NDB.Platform.Core) — free and open source under [GPL v3](../../LICENSE).
> Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/)
