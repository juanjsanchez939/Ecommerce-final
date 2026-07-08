using Ecommerce.Domain.Enums;

namespace Ecommerce.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public UserRole Role { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public ICollection<Order> Orders { get; private set; } = new List<Order>();
    public Cart Cart { get; private set; }

    private User() { }

    public User(string email, string passwordHash, string firstName, string lastName, UserRole role = UserRole.Customer)
    {
        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
        UpdatedAt = DateTime.UtcNow;
    }
}
