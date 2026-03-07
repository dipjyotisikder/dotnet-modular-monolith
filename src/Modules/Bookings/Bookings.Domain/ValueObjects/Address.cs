using Shared.Domain;

namespace Bookings.Domain.ValueObjects;

public class Address
{
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string Country { get; private set; } = string.Empty;
    public string ZipCode { get; private set; } = string.Empty;

    private Address() { }

    public static Result<Address> Create(string street, string city, string country, string zipCode = "")
    {
        if (string.IsNullOrWhiteSpace(street))
            return Result.Failure<Address>("Street cannot be empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(city))
            return Result.Failure<Address>("City cannot be empty", ErrorCodes.VALIDATION_ERROR);

        if (string.IsNullOrWhiteSpace(country))
            return Result.Failure<Address>("Country cannot be empty", ErrorCodes.VALIDATION_ERROR);

        return Result.Success(new Address
        {
            Street = street.Trim(),
            City = city.Trim(),
            Country = country.Trim(),
            ZipCode = zipCode.Trim()
        });
    }

    public override string ToString() => $"{Street}, {City}, {Country} {ZipCode}".TrimEnd();
}
