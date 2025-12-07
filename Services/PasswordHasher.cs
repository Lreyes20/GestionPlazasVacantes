using System.Security.Cryptography;
using System.Text;

namespace GestionPlazasVacantes.Services
{
    public static class PasswordHasher
    {
        public static byte[] ComputePBKDF2(byte[] salt, int iterations, string password, int keySize = 32)
        {
            return Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(password),
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                keySize);
        }
    }
}
