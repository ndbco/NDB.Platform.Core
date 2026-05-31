using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NDB.Platform.Kit.Mapping;
using Xunit;

namespace NDB.Platform.Tests.Kit.Mapping;

/// <summary>
/// Tests untuk IServiceMapper auto-scan dan DI registration (FIX 3 — Sprint B).
/// Verifikasi bahwa AddNdbMapping auto-discover dan register IServiceMapper implementations.
/// </summary>
public sealed class IServiceMapperTests
{
    // --- Test domain types ---

    public sealed class ProductEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

    public sealed class ProductResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public sealed class OrderEntity
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    public sealed class OrderResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }

    // --- Mapper interfaces extending IServiceMapper ---

    public interface IProductMapper : IServiceMapper
    {
        ProductResponse ToResponse(ProductEntity entity);
    }

    public interface IOrderMapper : IServiceMapper
    {
        OrderResponse ToResponse(OrderEntity entity);
    }

    // --- Mapper implementations ---

    public sealed class ProductMapper : IProductMapper
    {
        public ProductResponse ToResponse(ProductEntity entity) => new()
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }

    public sealed class OrderMapper : IOrderMapper
    {
        public OrderResponse ToResponse(OrderEntity entity) => new()
        {
            Id = entity.Id,
            Code = entity.Code
        };
    }

    // --- Tests ---

    [Fact]
    public void AddNdbMapping_ShouldRegister_ProductMapper_AsConcrete()
    {
        var services = new ServiceCollection();
        services.AddNdbMapping(typeof(IServiceMapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var mapper = scope.ServiceProvider.GetService<ProductMapper>();
        mapper.Should().NotBeNull();
    }

    [Fact]
    public void AddNdbMapping_ShouldRegister_IProductMapper_Interface()
    {
        var services = new ServiceCollection();
        services.AddNdbMapping(typeof(IServiceMapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var mapper = scope.ServiceProvider.GetService<IProductMapper>();
        mapper.Should().NotBeNull();
        mapper.Should().BeOfType<ProductMapper>();
    }

    [Fact]
    public void AddNdbMapping_ShouldRegister_IOrderMapper_Interface()
    {
        var services = new ServiceCollection();
        services.AddNdbMapping(typeof(IServiceMapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var mapper = scope.ServiceProvider.GetService<IOrderMapper>();
        mapper.Should().NotBeNull();
        mapper.Should().BeOfType<OrderMapper>();
    }

    [Fact]
    public void AddNdbMapping_IProductMapper_ShouldMapCorrectly()
    {
        var services = new ServiceCollection();
        services.AddNdbMapping(typeof(IServiceMapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IProductMapper>();

        var entity = new ProductEntity { Id = 42, Name = "Widget", Price = 9.99m };
        var response = mapper.ToResponse(entity);

        response.Id.Should().Be(42);
        response.Name.Should().Be("Widget");
    }

    [Fact]
    public void AddNdbMapping_TwoMappers_ShouldBeScoped()
    {
        var services = new ServiceCollection();
        services.AddNdbMapping(typeof(IServiceMapperTests).Assembly);
        var provider = services.BuildServiceProvider();

        // Kedua scoped instance dari scope yang sama harus sama object
        using var scope = provider.CreateScope();
        var mapper1a = scope.ServiceProvider.GetRequiredService<IProductMapper>();
        var mapper1b = scope.ServiceProvider.GetRequiredService<IProductMapper>();
        mapper1a.Should().BeSameAs(mapper1b);
    }

    [Fact]
    public void AddNdbMapping_ThrowsArgumentException_WhenNoAssemblies()
    {
        var services = new ServiceCollection();
        var act = () => services.AddNdbMapping();
        act.Should().Throw<ArgumentException>()
            .WithMessage("*At least one assembly is required.*");
    }

    [Fact]
    public void IServiceMapper_IsMarkerInterface_WithNoMembers()
    {
        // IServiceMapper hanya marker — tidak ada method
        var methods = typeof(IServiceMapper).GetMethods();
        methods.Should().BeEmpty();
    }
}
