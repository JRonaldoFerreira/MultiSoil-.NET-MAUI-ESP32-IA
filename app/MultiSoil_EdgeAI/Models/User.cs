using SQLite;

namespace MultiSoil_EdgeAI.Models;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Unique, NotNull, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    // Hash + Salt para senha
    [NotNull, MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    [NotNull, MaxLength(64)]
    public string Salt { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginUtc { get; set; }
}
