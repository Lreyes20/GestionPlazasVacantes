-- ============================================
-- Script para migrar base de datos de producción
-- De: GestionPlazasVacantesDB (esquema antiguo)
-- A: GestionPlazasVacantesDB (esquema actualizado)
-- ============================================

USE GestionPlazasVacantesDB;
GO

PRINT '=== Iniciando migración de base de datos de producción ===';
GO

-- Paso 1: Verificar si la columna Password existe y renombrarla a PasswordHash
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[auth].[Usuarios]') AND name = 'Password')
BEGIN
    PRINT 'Renombrando columna Password a PasswordHash...';
    EXEC sp_rename 'auth.Usuarios.Password', 'PasswordHash', 'COLUMN';
    PRINT '✓ Columna renombrada exitosamente';
END
ELSE
BEGIN
    PRINT '⚠ La columna Password no existe o ya fue renombrada';
END
GO

-- Paso 2: Verificar y ajustar el tamaño de la columna PasswordHash si es necesario
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID(N'[auth].[Usuarios]') 
           AND name = 'PasswordHash' 
           AND max_length < 500)
BEGIN
    PRINT 'Ajustando tamaño de columna PasswordHash...';
    ALTER TABLE [auth].[Usuarios] 
    ALTER COLUMN [PasswordHash] NVARCHAR(250) NOT NULL;
    PRINT '✓ Tamaño de columna ajustado';
END
ELSE
BEGIN
    PRINT '⚠ La columna PasswordHash ya tiene el tamaño correcto';
END
GO

-- Paso 3: Agregar columna UsuarioAsignadoId si no existe (para asignación de plazas)
IF NOT EXISTS (SELECT * FROM sys.columns 
               WHERE object_id = OBJECT_ID(N'[gestion].[PlazasVacantes]') 
               AND name = 'UsuarioAsignadoId')
BEGIN
    PRINT 'Agregando columna UsuarioAsignadoId a PlazasVacantes...';
    ALTER TABLE [gestion].[PlazasVacantes]
    ADD [UsuarioAsignadoId] INT NULL;
    
    -- Agregar foreign key
    ALTER TABLE [gestion].[PlazasVacantes]
    ADD CONSTRAINT [FK_PlazasVacantes_Usuarios_UsuarioAsignadoId] 
    FOREIGN KEY ([UsuarioAsignadoId]) 
    REFERENCES [auth].[Usuarios] ([Id])
    ON DELETE SET NULL;
    
    PRINT '✓ Columna UsuarioAsignadoId agregada';
END
ELSE
BEGIN
    PRINT '⚠ La columna UsuarioAsignadoId ya existe';
END
GO

-- Paso 4: Verificar estructura final
PRINT '';
PRINT '=== Verificando estructura de tablas ===';
GO

SELECT 
    'auth.Usuarios' as Tabla,
    COLUMN_NAME as Columna,
    DATA_TYPE as Tipo,
    CHARACTER_MAXIMUM_LENGTH as Tamaño
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'auth' AND TABLE_NAME = 'Usuarios'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '=== Migración completada exitosamente ===';
GO
