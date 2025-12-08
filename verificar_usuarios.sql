-- ============================================
-- Script para verificar usuarios en la base de datos CORRECTA
-- Base de datos: GestionPlazasVacantesDB_Demo
-- ============================================

-- IMPORTANTE: Asegúrate de estar conectado a la base de datos correcta
USE GestionPlazasVacantesDB_Demo;
GO

-- Verificar todos los usuarios en el sistema
SELECT 
    Id,
    Username,
    FullName,
    Email,
    Rol,
    Activo,
    CreadoUtc,
    UltimoAccesoUtc
FROM auth.Usuarios
ORDER BY 
    CASE 
        WHEN Rol = 'Jefe' THEN 1 
        ELSE 2 
    END,
    FullName;
GO

-- Contar usuarios por rol
SELECT 
    Rol,
    COUNT(*) as Cantidad
FROM auth.Usuarios
WHERE Activo = 1
GROUP BY Rol;
GO

-- Verificar los 6 nuevos usuarios específicamente
SELECT 
    Username,
    FullName,
    Email,
    Rol
FROM auth.Usuarios
WHERE Username IN ('aortega', 'carce', 'schinchilla', 'rcordero', 'kramirez', 'pzuniga')
ORDER BY 
    CASE 
        WHEN Rol = 'Jefe' THEN 1 
        ELSE 2 
    END,
    Username;
GO

-- Verificar TODAS las bases de datos disponibles
SELECT 
    name as [Nombre Base de Datos],
    database_id,
    create_date as [Fecha Creación]
FROM sys.databases
WHERE name LIKE '%GestionPlazas%'
ORDER BY name;
GO
