using BCrypt.Net;

namespace DonorTrackingSystem.Services
{
    // Service for hashing and verifying passwords using BCrypt
    public class PasswordHasher
    {
        // Hashes a password (expects a 6-digit number as string)
        public static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Verifies a password against a stored hash
        public static bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        // Validates that the password is a 6-digit number
        public static bool IsValidPasswordFormat(string password)
        {
            return int.TryParse(password, out int passwordNum) && 
                   passwordNum >= 100000 && 
                   passwordNum <= 999999;
        }
    }
}
