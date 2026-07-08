namespace Ecommerce.Domain.ValueObjects;

public class Address
{
    public string Street { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string Country { get; private set; }
    public string ZipCode { get; private set; }

    private Address() { }

    public Address(string street, string city, string state, string country, string zipCode)
    {
        if (string.IsNullOrWhiteSpace(street)) throw new ArgumentException("Street cannot be empty.", nameof(street));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("City cannot be empty.", nameof(city));
        if (string.IsNullOrWhiteSpace(state)) throw new ArgumentException("State cannot be empty.", nameof(state));
        if (string.IsNullOrWhiteSpace(country)) throw new ArgumentException("Country cannot be empty.", nameof(country));
        if (string.IsNullOrWhiteSpace(zipCode)) throw new ArgumentException("ZipCode cannot be empty.", nameof(zipCode));

        Street = street;
        City = city;
        State = state;
        Country = country;
        ZipCode = zipCode;
    }
}
