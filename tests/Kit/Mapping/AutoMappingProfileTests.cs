using FluentAssertions;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using NDB.Platform.Kit.Mapping;
using Xunit;

namespace NDB.Platform.Tests.Kit.Mapping;

public sealed class AutoMappingProfileTests
{
    private sealed class SourceEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TargetDto : NDB.Platform.Kit.Mapping.IMapFrom<SourceEntity>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class OutgoingDto : IMapTo<SourceEntity>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    private sealed class TwoWayA { public string Value { get; set; } = string.Empty; }
    private sealed class TwoWayB : IMapObject<TwoWayA, TwoWayB>
    {
        public string Value { get; set; } = string.Empty;
    }

    [Fact]
    public void Apply_IMapFrom_ShouldRegisterMapping()
    {
        var config = new TypeAdapterConfig();
        AutoMappingProfile.Apply(config, typeof(TargetDto).Assembly);

        var source = new SourceEntity { Id = 1, Name = "test" };
        var mapped = source.Adapt<TargetDto>(config);
        mapped.Id.Should().Be(1);
        mapped.Name.Should().Be("test");
    }

    [Fact]
    public void Apply_IMapTo_ShouldRegisterReverseMapping()
    {
        var config = new TypeAdapterConfig();
        AutoMappingProfile.Apply(config, typeof(OutgoingDto).Assembly);

        var dto = new OutgoingDto { Id = 2, Name = "outgoing" };
        var entity = dto.Adapt<SourceEntity>(config);
        entity.Id.Should().Be(2);
        entity.Name.Should().Be("outgoing");
    }

    [Fact]
    public void DependencyInjection_AddNdbMapping_ShouldRegisterIMapper()
    {
        var services = new ServiceCollection();
        services.AddNdbMapping(typeof(TargetDto).Assembly);
        var provider = services.BuildServiceProvider();

        var mapper = provider.GetService<IMapper>();
        mapper.Should().NotBeNull();
    }

    [Fact]
    public void IMapper_Map_ShouldWorkAfterDiSetup()
    {
        var services = new ServiceCollection();
        services.AddNdbMapping(typeof(TargetDto).Assembly);
        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var source = new SourceEntity { Id = 3, Name = "mapped" };
        var dto = mapper.Map<TargetDto>(source);
        dto.Id.Should().Be(3);
        dto.Name.Should().Be("mapped");
    }

    [Fact]
    public void Apply_IMapObject_ShouldRegisterTwoWayMapping()
    {
        var config = new TypeAdapterConfig();
        AutoMappingProfile.Apply(config, typeof(TwoWayB).Assembly);

        var a = new TwoWayA { Value = "hello" };
        var b = a.Adapt<TwoWayB>(config);
        b.Value.Should().Be("hello");

        var aBack = b.Adapt<TwoWayA>(config);
        aBack.Value.Should().Be("hello");
    }
}
