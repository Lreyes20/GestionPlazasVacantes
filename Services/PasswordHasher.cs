using BCrypt.Net;

namespace GestionPlazasVacantes.Services
{
    /// <summary>
    /// Servicio para hashear y verificar contraseñas de forma segura usando BCrypt
    /// </summary>
    public static class PasswordHasher
    {
        // Work factor de 12 proporciona buen balance entre seguridad y rendimiento
        private const int WorkFactor = 12;

        /// <summary>
        /// Hashea una contraseña en texto plano
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <returns>Hash BCrypt de la contraseña</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        /// <summary>
        /// Verifica si una contraseña en texto plano coincide con un hash
        /// </summary>
        /// <param name="password">Contraseña en texto plano</param>
        /// <param name="hash">Hash BCrypt almacenado</param>
        /// <returns>True si la contraseña es correcta, False en caso contrario</returns>
        public static bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                // Si el hash es inválido, retornar false en lugar de lanzar excepción
                return false;
            }
        }

        /// <summary>
        /// Verifica si un hash necesita ser actualizado (rehashed) debido a cambio en work factor
        /// </summary>
        public static bool NeedsRehash(string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.PasswordNeedsRehash(hash, WorkFactor);
            }
            catch
            {
                return true; // Si hay error, asumir que necesita rehash
            }
        }
    }
}
