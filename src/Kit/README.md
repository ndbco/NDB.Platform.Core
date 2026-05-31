# NDB.Platform.Kit

> Namespace: `NDB.Platform.Kit.*`
> [← Back to main README](../../README.md)

Production-ready utility helpers — caching, cryptography, regional formatting, guard clauses, identifier generation, object mapping, parsing, and text processing. All stateless, no domain coupling, usable anywhere.

---

## Caching — `Kit.Caching`

### IMemoryCache — `MemoryCacheExtensions`

```csharp
using NDB.Platform.Kit.Caching;

// Cache-aside: return from cache on hit, call factory + store on miss
var roles = await cache.GetOrSetAsync(
    key:     CacheKeyBuilder.For("roles", "all"),
    factory: () => db.Roles.OrderBy(r => r.Name).ToListAsync(ct),
    options: CacheEntryDefaults.DefaultMasterDataOptions());

// Overload with default master-data TTL:
var roles = await cache.GetOrSetAsync("roles:all", () => db.Roles.ToListAsync(ct));
```

### IDistributedCache — `DistributedCacheExtensions` (Redis)

```csharp
// Same pattern, works with Redis (IDistributedCache):
var settings = await distributedCache.GetOrSetAsync(
    key:     CacheKeyBuilder.For("settings", "smtp"),
    factory: () => db.Settings.Where(s => s.Category == "smtp").ToListAsync(ct),
    options: DistributedCacheExtensions.MasterDataOptions(),
    ct:      ct);
```

### TTL Presets

**IMemoryCache presets (`CacheEntryDefaults`):**

| Method | Sliding | Absolute | Best for |
|---|---|---|---|
| `DefaultMasterDataOptions()` | 60 s | 1 h | Roles, permissions, pages |
| `DefaultReferenceOptions()` | 5 min | 24 h | System config, rarely-changed data |
| `DefaultLookupOptions()` | 30 s | 5 min | Recent/dynamic lookup data |

**IDistributedCache presets (`DistributedCacheExtensions`):**

| Method | Absolute TTL | Best for |
|---|---|---|
| `MasterDataOptions()` | 15 min | Roles, settings, module lists |
| `ReferenceDataOptions()` | 30 min | Locale, branding, currencies |
| `ShortLivedOptions()` | 5 min | Per-entity cache (tags, user permissions) |

### Cache Key Builder

```csharp
// Consistent, collision-free key format — avoids scattered magic strings
CacheKeyBuilder.For("roles", "all")             // → "roles:all"
CacheKeyBuilder.For("perm", "user", userId)     // → "perm:user:{guid}"
CacheKeyBuilder.For("settings", "cat", "smtp")  // → "settings:cat:smtp"
CacheKeyBuilder.For("tags", entityType, id)     // → "tags:{type}:{guid}"
```

---

## Cryptography — `Kit.Crypto`

### Password Hashing (BCrypt)

```csharp
using NDB.Platform.Kit.Crypto;

// Hash before storing in DB:
string hash = PasswordHasher.Hash("userPassword123!");

// Verify at login:
bool isValid = PasswordHasher.Verify("userPassword123!", hash);
if (!isValid) return Result.BadRequest("Invalid password");
```

### PII Hashing (Blake3 — One-Way)

For indexing sensitive fields (national ID, email) without storing plaintext:

```csharp
// Hash the email for search indexing:
string emailHash = PiiHasher.Hash("user@example.com");

// Find user by hashed email:
var user = await db.Users.FirstOrDefaultAsync(u => u.EmailHash == emailHash, ct);
```

> Blake3 is a one-way hash — it cannot be reversed. Use it for fields you need to search, not fields you need to display.

### Base64

```csharp
string encoded    = Base64Helper.Encode(bytes);
string urlSafe    = Base64Helper.EncodeUrlSafe(bytes);  // URL-safe variant
byte[] decoded    = Base64Helper.Decode(encoded);
string roundTrip  = Base64Helper.Encode(Base64Helper.Decode(encoded));
```

---

## Regional Formatting — `Kit.Format`

### IRegionService

Interface for locale-aware formatting. Implementation is provided by the consuming project.

```csharp
using NDB.Platform.Kit.Format;

// Currency:
regionService.FormatCurrency(1_500_000m, "IDR", "id-ID");  // → "Rp 1.500.000"
regionService.FormatCurrency(99.99m,     "USD", "en-US");  // → "$99.99"
regionService.FormatCurrency(250.00m,    "EUR", "de-DE");  // → "250,00 €"

// Date:
regionService.FormatDate(DateTime.Now, "id-ID");  // → "31 Mei 2026"
regionService.FormatDate(DateTime.Now, "en-US");  // → "May 31, 2026"
regionService.FormatDate(DateTime.Now, "de-DE");  // → "31. Mai 2026"

// Phone (E.164 normalization):
regionService.FormatPhone("0812-3456-7890");     // → "+6281234567890"
regionService.FormatPhone("+62 812 3456 7890");  // → "+6281234567890"
```

### Static Formatters

```csharp
// DateTime:
DateTimeFormat.ToWib(utcNow);           // UTC → WIB display string
DateTimeFormat.ToDate(dt, "id-ID");     // → "31 Mei 2026"
DateTimeFormat.ToRelative(dt);          // → "3 days ago"

// Numbers:
NumberFormat.ToRupiah(1_500_000m);       // → "Rp 1.500.000"
NumberFormat.WithSeparator(1_500_000m);  // → "1.500.000"
NumberFormat.ToPercent(0.8567m);         // → "85,67%"

// File size:
FileSizeFormat.Format(1_048_576L);       // → "1 MB"
FileSizeFormat.Format(2_684_354_560L);   // → "2.5 GB"
FileSizeFormat.Format(512L);             // → "512 B"

// Phone:
PhoneFormat.ToE164("0812345678", "ID");  // → "+6281234567"
```

---

## Guard Clauses — `Kit.Guards`

Fail-fast argument validation at the start of a method:

```csharp
using NDB.Platform.Kit.Guards;

public async Task ProcessPayment(Guid orderId, decimal amount, string currency, string callbackUrl)
{
    Guard.NotEmpty(orderId,        nameof(orderId));
    Guard.Positive(amount,         nameof(amount));
    Guard.NotNullOrEmpty(currency, nameof(currency));
    Guard.MaxLength(currency, 3,   nameof(currency));
    Guard.NotNullOrEmpty(callbackUrl, nameof(callbackUrl));
    Guard.InRange(amount, 1_000m, 500_000_000m, nameof(amount));

    // Proceed only if all guards pass
}
```

**Available guards:**

| Guard | Throws when |
|---|---|
| `Guard.NotNull(value)` | value is null |
| `Guard.NotEmpty(guid)` | guid == Guid.Empty |
| `Guard.NotNullOrEmpty(str)` | null or `""` |
| `Guard.NotNullOrWhiteSpace(str)` | null, `""`, or whitespace |
| `Guard.Positive(number)` | ≤ 0 |
| `Guard.NonNegative(number)` | < 0 |
| `Guard.InRange(value, min, max)` | outside [min, max] |
| `Guard.MaxLength(str, max)` | length > max |

---

## Identifiers — `Kit.Identifiers`

### ID Generation

```csharp
using NDB.Platform.Kit.Identifiers;

// ULID — time-sortable, URL-safe, 26 chars (recommended for primary keys)
var id = IdGenerator.NewUlid();         // → "01HWKR6GZJVNKDPW3T0F5XBQM"

// Nanoid — short, URL-safe, random
var token = IdGenerator.NewNanoid();    // → "V1StGXR8_Z5jdHi6B-myT" (21 chars)
var short8 = IdGenerator.NewNanoid(8); // → "AbCd1234" (custom length)

// Snowflake — monotonic 64-bit integer for distributed systems
var snowflake = IdGenerator.NextSnowflake();
```

### Correlation ID

```csharp
// Used for request tracing across services:
string cid = CorrelationId.Generate(); // → "req-01HWKR6GZJVNKDPW3T0F5XBQM"
```

### SystemActorAccessor

Default `IActorAccessor` for background jobs and CLI (no HttpContext available). Returns `AuditActor.System`.

```csharp
// Automatically registered as singleton fallback by AddNdbCqrs().
// In web projects, override with HttpContextActorAccessor from NDB.Platform.API:
builder.Services.AddScoped<IActorAccessor, HttpContextActorAccessor>();
```

---

## Object Mapping — `Kit.Mapping`

Powered by [Mapster](https://github.com/MapsterMapper/Mapster).

### Auto-Mapping

```csharp
using NDB.Platform.Kit.Mapping;

// Register all profiles from the assembly:
builder.Services.AddNdbMapping(typeof(Program).Assembly);

// Simple auto-mapping (same-name fields):
public class ProductDto : IMapFrom<Product>
{
    public Guid   Id    { get; set; }
    public string Name  { get; set; } = default!;
    public decimal Price { get; set; }
}

// Use:
var dto  = product.Adapt<ProductDto>();
var list = products.Adapt<List<ProductDto>>();

// Efficient EF projection (avoids SELECT *):
var page = await db.Products.ProjectToType<ProductDto>().ToListAsync(ct);
```

### Custom Mapping

```csharp
// Override specific fields with IMapObject:
public class InvoiceDto : IMapObject
{
    public string CustomerName { get; set; } = default!;
    public string FormattedTotal { get; set; } = default!;

    public void ConfigureMapping(TypeAdapterConfig config)
    {
        config.NewConfig<Invoice, InvoiceDto>()
            .Map(dest => dest.CustomerName,   src => src.Customer.FullName)
            .Map(dest => dest.FormattedTotal, src => $"Rp {src.Total:N0}");
    }
}
```

### Service Mapper (DI-aware)

For mappers that need injected services:

```csharp
// Automatically registered as Scoped via AddNdbMapping():
public class OrderMapper(IRegionService region) : IServiceMapper
{
    public OrderDto Map(Order order) => new()
    {
        Id    = order.Id,
        Total = region.FormatCurrency(order.Amount, order.Currency)
    };
}
```

---

## Parsing — `Kit.Parse`

Safe parsers that return null instead of throwing on bad input:

```csharp
using NDB.Platform.Kit.Parse;

Guid?     id   = Parse.ToGuid("not-a-guid");     // → null
DateTime? dt   = Parse.ToDateTime("2026-05-31"); // → DateTime?
decimal?  num  = Parse.ToDecimal("1.500,00");    // → decimal?
int?      page = Parse.ToInt32("abc");            // → null

// Enum helpers:
var status  = EnumHelper.Parse<OrderStatus>("PENDING");         // → OrderStatus.Pending or throws
var status2 = EnumHelper.ParseOrDefault("UNKNOWN", OrderStatus.Draft); // → OrderStatus.Draft
bool valid  = EnumHelper.IsDefined<OrderStatus>("SHIPPED");     // → true
var values  = EnumHelper.GetValues<OrderStatus>();               // → all enum values
```

---

## Text Processing — `Kit.Text`

```csharp
using NDB.Platform.Kit.Text;

// String normalization:
StringNormalize.ToSlug("Hello World 2026!");    // → "hello-world-2026"
StringNormalize.ToUpperSnake("helloWorld");     // → "HELLO_WORLD"
StringNormalize.ToCamelCase("hello_world");     // → "helloWorld"
StringNormalize.StripHtml("<p>Hello <b>World</b></p>"); // → "Hello World"
StringNormalize.Truncate("Long description...", 20, "…"); // → "Long description…"
StringNormalize.NormalizeWhitespace("a  b\t\nc"); // → "a b c"

// Compiled regex patterns (source-generated, zero-allocation):
bool isEmail  = RegexPatterns.Email.IsMatch("user@example.com");
bool isPhone  = RegexPatterns.PhoneID.IsMatch("08123456789");
bool isUrl    = RegexPatterns.Url.IsMatch("https://ndb.co.id");
bool isNik    = RegexPatterns.Nik.IsMatch("3271234567890001");    // Indonesian National ID
bool isNpwp   = RegexPatterns.Npwp.IsMatch("12.345.678.9-012.345"); // Indonesian Tax ID
```

---

## Source Files

```
src/Kit/
├── Caching/
│   ├── CacheKeyBuilder.cs
│   ├── CacheEntryDefaults.cs
│   ├── MemoryCacheExtensions.cs       ← IMemoryCache.GetOrSetAsync
│   └── DistributedCacheExtensions.cs  ← IDistributedCache.GetOrSetAsync
├── Crypto/
│   ├── PasswordHasher.cs              ← BCrypt
│   ├── PiiHasher.cs                   ← Blake3 one-way
│   └── Base64Helper.cs
├── Format/
│   ├── IRegionService.cs              ← Locale-aware formatting interface
│   ├── DateTimeFormat.cs
│   ├── NumberFormat.cs
│   ├── PhoneFormat.cs
│   └── FileSizeFormat.cs
├── Guards/
│   └── Guard.cs
├── Identifiers/
│   ├── IdGenerator.cs                 ← ULID, Nanoid, Snowflake
│   ├── CorrelationId.cs
│   └── SystemActorAccessor.cs         ← IActorAccessor fallback for jobs/CLI
├── Mapping/
│   ├── IMapFrom.cs
│   ├── IMapTo.cs
│   ├── IMapObject.cs
│   ├── IServiceMapper.cs
│   ├── AutoMappingProfile.cs
│   └── DependencyInjection.cs         ← AddNdbMapping()
├── Parse/
│   ├── Parse.cs
│   └── EnumHelper.cs
└── Text/
    ├── StringNormalize.cs
    └── RegexPatterns.cs
```

---

> Part of [NDB.Platform.Core](https://github.com/ndbco/NDB.Platform.Core) — free and open source under [GPL v3](../../LICENSE).
> Built by [PT. Navigate Digital Boundaries](https://ndb.co.id/)
