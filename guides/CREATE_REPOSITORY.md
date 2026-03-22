# How to Create a Repository

This guide shows you how to implement the Repository pattern for data access in the Infrastructure layer.

## What is a Repository?

A repository abstracts data access logic and provides a collection-like interface for working with domain entities.

**Benefits:**
- ✅ Separates domain from data access concerns
- ✅ Makes code testable (can mock repositories)
- ✅ Encapsulates EF Core queries
- ✅ Provides a clear contract for data operations

## Project Structure

```
src/Modules/{Module}/
├── {Module}.Domain/
│   └── Repositories/
│       └── IHotelRepository.cs         # Interface (contract)
└── {Module}.Infrastructure/
    └── Repositories/
        └── HotelRepository.cs          # Implementation
```

## Step-by-Step Guide

### 1. Create the Repository Interface (Domain Layer)

Create a new file: `IHotelRepository.cs`

```csharp
using Bookings.Domain.Entities;

namespace Bookings.Domain.Repositories;

public interface IHotelRepository
{
    // Queries
    Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Hotel?> GetByIdWithRoomsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Hotel>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Hotel>> GetByFiltersAsync(
        string? city,
        int? minRating,
        CancellationToken cancellationToken = default);
    Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Commands
    Task AddAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task UpdateAsync(Hotel hotel, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
```

**Why in Domain layer?**
- Domain layer defines **what** operations are needed
- Infrastructure layer implements **how** to do them
- Domain stays independent of EF Core

### 2. Create the Repository Implementation (Infrastructure Layer)

Create a new file: `HotelRepository.cs`

```csharp
using Bookings.Domain.Entities;
using Bookings.Domain.Repositories;
using Bookings.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Shared.Infrastructure.Repositories;

namespace Bookings.Infrastructure.Repositories;

public class HotelRepository(BookingsDbContext context)
    : Repository<Hotel>(context), IHotelRepository
{
    public async Task<Hotel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Hotels
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<Hotel?> GetByIdWithRoomsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Hotels
            .Include(h => h.Rooms)
            .FirstOrDefaultAsync(h => h.Id == id, cancellationToken);
    }

    public async Task<List<Hotel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await context.Hotels
            .OrderBy(h => h.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Hotel>> GetByFiltersAsync(
        string? city,
        int? minRating,
        CancellationToken cancellationToken = default)
    {
        var query = context.Hotels.AsQueryable();

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(h => h.Address.Contains(city));
        }

        if (minRating.HasValue)
        {
            query = query.Where(h => h.StarRating >= minRating.Value);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Hotels
            .AnyAsync(h => h.Id == id, cancellationToken);
    }

    // AddAsync, UpdateAsync, DeleteAsync inherited from base Repository<T>
}
```

### 3. Base Repository (Already Provided)

Your project includes a generic base repository:

**Location:** `src/Shared/Shared.Infrastructure/Repositories/Repository.cs`

```csharp
public class Repository<T>(DbContext context) : IRepository<T> 
    where T : Entity
{
    protected readonly DbContext Context = context;

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await Context.Set<T>().FindAsync([id], ct);
    }

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await Context.Set<T>().AddAsync(entity, ct);
    }

    public Task UpdateAsync(T entity, CancellationToken ct = default)
    {
        Context.Set<T>().Update(entity);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = Context.Set<T>().Find(id);
        if (entity is not null)
            Context.Set<T>().Remove(entity);
        
        return Task.CompletedTask;
    }
}
```

You can:
- Inherit from `Repository<T>` to get basic CRUD operations
- Add custom methods specific to your entity

### 4. Register the Repository

In your module's `DependencyInjection.cs`:

```csharp
using Bookings.Domain.Repositories;
using Bookings.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register repositories
        services.AddScoped<IHotelRepository, HotelRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();

        return services;
    }
}
```

## Repository Patterns

### Pattern 1: Simple CRUD Repository

```csharp
public interface IProductRepository
{
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
    Task UpdateAsync(Product product, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

### Pattern 2: Query-Rich Repository

```csharp
public interface IBookingRepository
{
    // By ID
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    // By foreign keys
    Task<List<Booking>> GetByGuestIdAsync(Guid guestId, CancellationToken ct = default);
    Task<List<Booking>> GetByHotelIdAsync(Guid hotelId, CancellationToken ct = default);
    
    // By status
    Task<List<Booking>> GetPendingBookingsAsync(CancellationToken ct = default);
    Task<List<Booking>> GetCompletedBookingsAsync(CancellationToken ct = default);
    
    // Complex queries
    Task<List<Booking>> GetOverlappingBookingsAsync(
        Guid roomId,
        DateTime checkIn,
        DateTime checkOut,
        CancellationToken ct = default);
    
    // Statistics
    Task<int> GetTotalBookingsCountAsync(CancellationToken ct = default);
    Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    
    // Commands
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task UpdateAsync(Booking booking, CancellationToken ct = default);
}
```

### Pattern 3: Specification Pattern

For complex, reusable queries:

```csharp
// Domain layer
public interface IBookingRepository
{
    Task<List<Booking>> GetBySpecificationAsync(
        ISpecification<Booking> specification,
        CancellationToken ct = default);
}

// Implementation
public async Task<List<Booking>> GetBySpecificationAsync(
    ISpecification<Booking> specification,
    CancellationToken ct = default)
{
    return await context.Bookings
        .Where(specification.ToExpression())
        .ToListAsync(ct);
}

// Usage
var spec = new ActiveBookingsSpecification(hotelId);
var bookings = await _bookingRepository.GetBySpecificationAsync(spec);
```

### Pattern 4: Pagination Support

```csharp
public interface IHotelRepository
{
    Task<PagedResult<Hotel>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        CancellationToken ct = default);
}

public class HotelRepository : Repository<Hotel>, IHotelRepository
{
    public async Task<PagedResult<Hotel>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm = null,
        CancellationToken ct = default)
    {
        var query = context.Hotels.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(h => 
                h.Name.Contains(searchTerm) || 
                h.Address.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(h => h.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Hotel>(items, totalCount, pageNumber, pageSize);
    }
}
```

## Advanced Repository Techniques

### Including Related Entities

```csharp
public async Task<Hotel?> GetHotelWithDetailsAsync(Guid id, CancellationToken ct = default)
{
    return await context.Hotels
        .Include(h => h.Rooms)
        .Include(h => h.Reviews)
            .ThenInclude(r => r.User)
        .FirstOrDefaultAsync(h => h.Id == id, ct);
}
```

### Filtering with Expressions

```csharp
public async Task<List<Hotel>> FindAsync(
    Expression<Func<Hotel, bool>> predicate,
    CancellationToken ct = default)
{
    return await context.Hotels
        .Where(predicate)
        .ToListAsync(ct);
}

// Usage
var hotels = await _hotelRepository.FindAsync(h => h.StarRating >= 4 && h.City == "Paris");
```

### Projections (DTOs)

```csharp
public async Task<List<HotelSummaryDto>> GetHotelSummariesAsync(CancellationToken ct = default)
{
    return await context.Hotels
        .Select(h => new HotelSummaryDto(
            h.Id,
            h.Name,
            h.StarRating,
            h.Rooms.Count))
        .ToListAsync(ct);
}
```

### Soft Delete Support

```csharp
public interface IBookingRepository
{
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
    Task<List<Booking>> GetActiveBookingsAsync(CancellationToken ct = default);
}

public class BookingRepository : Repository<Booking>, IBookingRepository
{
    public async Task SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await GetByIdAsync(id, ct);
        if (booking is not null)
        {
            booking.IsDeleted = true;
            booking.DeletedAt = DateTime.UtcNow;
            await UpdateAsync(booking, ct);
        }
    }

    public async Task<List<Booking>> GetActiveBookingsAsync(CancellationToken ct = default)
    {
        return await context.Bookings
            .Where(b => !b.IsDeleted)
            .ToListAsync(ct);
    }
}
```

## Unit of Work Pattern

**Already provided:** `IUnitOfWork`

**Location:** `src/Shared/Shared.Domain/Repositories/IUnitOfWork.cs`

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

**Usage in Command Handler:**

```csharp
public class CreateHotelCommandHandler(
    IHotelRepository hotelRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateHotelCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateHotelCommand request, CancellationToken ct)
    {
        var hotel = Hotel.Create(request.Name, request.Address, request.StarRating);
        
        await hotelRepository.AddAsync(hotel, ct);
        await unitOfWork.SaveChangesAsync(ct); // Single transaction
        
        return Result<Guid>.Success(hotel.Id);
    }
}
```

## Testing Repositories

### Integration Test (Real Database)

```csharp
public class HotelRepositoryTests : IAsyncLifetime
{
    private BookingsDbContext _context;
    private HotelRepository _repository;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new BookingsDbContext(options);
        _repository = new HotelRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingHotel_ReturnsHotel()
    {
        // Arrange
        var hotel = Hotel.Create("Grand Hotel", "123 Main St", "Luxury", 5);
        _context.Hotels.Add(hotel);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(hotel.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Grand Hotel");
    }

    [Fact]
    public async Task GetByFiltersAsync_WithCityFilter_ReturnsMatchingHotels()
    {
        // Arrange
        _context.Hotels.Add(Hotel.Create("Paris Hotel", "Paris, France", "Desc", 4));
        _context.Hotels.Add(Hotel.Create("London Hotel", "London, UK", "Desc", 3));
        await _context.SaveChangesAsync();

        // Act
        var results = await _repository.GetByFiltersAsync(city: "Paris", minRating: null);

        // Assert
        results.Should().ContainSingle();
        results.First().Name.Should().Be("Paris Hotel");
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
    }
}
```

### Unit Test (Mocked Repository)

```csharp
public class CreateHotelCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidCommand_AddsHotelToRepository()
    {
        // Arrange
        var repository = Substitute.For<IHotelRepository>();
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var handler = new CreateHotelCommandHandler(repository, unitOfWork);
        
        var command = new CreateHotelCommand("Hotel", "Address", "Desc", 4);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        await repository.Received(1).AddAsync(Arg.Any<Hotel>(), Arg.Any<CancellationToken>());
        await unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
```

## Best Practices

✅ **DO:**
- Define interfaces in Domain layer
- Implement in Infrastructure layer
- Return domain entities, not DTOs
- Use async methods
- Accept CancellationToken
- Keep repositories focused on one entity

❌ **DON'T:**
- Expose IQueryable (breaks encapsulation)
- Return DbSet directly
- Include business logic
- Use repositories for simple queries (use DbContext directly in query handlers)
- Create generic "RepositoryOfEverything"

## When NOT to Use Repositories

For **read-only queries**, you can bypass repositories and query `DbContext` directly in query handlers:

```csharp
public class GetHotelsQueryHandler(BookingsDbContext context)
    : IRequestHandler<GetHotelsQuery, Result<List<HotelDto>>>
{
    public async Task<Result<List<HotelDto>>> Handle(GetHotelsQuery request, CancellationToken ct)
    {
        // Direct DbContext access for queries is fine
        var hotels = await context.Hotels
            .Select(h => new HotelDto(h.Id, h.Name, h.StarRating))
            .ToListAsync(ct);

        return Result<List<HotelDto>>.Success(hotels);
    }
}
```

**Use repositories for:**
- ✅ Commands (write operations)
- ✅ Complex domain queries
- ✅ Queries used in multiple places

**Skip repositories for:**
- ✅ Simple read queries
- ✅ Reporting queries
- ✅ One-off queries

## Related Guides

- [CREATE_ENTITY.md](CREATE_ENTITY.md) - Domain entities
- [ADD_COMMAND.md](ADD_COMMAND.md) - Using repositories in commands
- [ADD_QUERY.md](ADD_QUERY.md) - Querying data
- [CONFIGURE_ENTITY_FRAMEWORK.md](CONFIGURE_ENTITY_FRAMEWORK.md) - EF Core setup
