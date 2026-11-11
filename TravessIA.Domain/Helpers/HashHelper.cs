namespace TravessIA.Domain.Helpers;

public static class HashHelper
{
    public static string Hash(string rawPassword)
    {
        return BCrypt.Net.BCrypt.HashPassword(rawPassword);
    }

    public static bool Verify(string rawPassword, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(rawPassword, hashedPassword);
    }

    public static string Hash(string rawPassword, string salt)
    {
        return BCrypt.Net.BCrypt.HashPassword(rawPassword + salt);
    }

    public static bool Verify(string rawPassword, string salt, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(rawPassword + salt, hashedPassword);
    }
}
