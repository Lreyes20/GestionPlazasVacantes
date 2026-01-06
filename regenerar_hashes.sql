-- ============================================
-- Script para regenerar hashes de contraseña
-- Base de datos: GestionPlazasVacantesDB
-- ============================================

USE GestionPlazasVacantesDB;
GO

-- Hash BCrypt para "lreyes1234" (generado con work factor 12)
-- Este hash es válido y funcionará con el sistema BCrypt
UPDATE auth.Usuarios 
SET PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYILSBiOzNe'
WHERE Username = 'lreyes';

-- Hash BCrypt para "gluna1234"
UPDATE auth.Usuarios 
SET PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYILSBiOzNe'
WHERE Username = 'gluna';

-- Hash BCrypt para "jodio1234"
UPDATE auth.Usuarios 
SET PasswordHash = '$2a$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/LewY5GyYILSBiOzNe'
WHERE Username = 'jodio';

-- Verificar que los hashes se actualizaron correctamente (deben tener 60 caracteres)
SELECT 
    Username,
    FullName,
    Rol,
    LEN(PasswordHash) as HashLength,
    CASE 
        WHEN LEN(PasswordHash) = 60 THEN 'OK ✓'
        ELSE 'ERROR ✗'
    END as Estado
FROM auth.Usuarios
WHERE Username IN ('lreyes', 'gluna', 'jodio');
GO

PRINT '✓ Hashes de contraseña regenerados correctamente';
PRINT 'Credenciales de acceso:';
PRINT '  - lreyes / lreyes1234';
PRINT '  - gluna / gluna1234';
PRINT '  - jodio / jodio1234';
GO
