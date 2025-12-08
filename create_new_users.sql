-- Script para crear nuevos usuarios en la base de datos GestionPlazasVacantes
-- Fecha: 2025-12-08

USE GestionPlazasVacantesDB_Demo;
GO

-- Insertar Andrea Ortega Bermúdez (Jefa)
INSERT INTO auth.Usuarios (Username, FullName, Email, PasswordHash, Rol, Activo, CreadoUtc)
VALUES (
    'aortega', 
    'Andrea Ortega Bermúdez', 
    'andrea.ortega@prueba.com', 
    '$2a$11$YourHashedPasswordHere', -- Se actualizará con el hash real
    'Jefe', 
    1, 
    GETUTCDATE()
);

-- Insertar Claudia Arce Lopez (Colaboradora)
INSERT INTO auth.Usuarios (Username, FullName, Email, PasswordHash, Rol, Activo, CreadoUtc)
VALUES (
    'carce', 
    'Claudia Arce Lopez', 
    'claudia.arce@prueba.com', 
    '$2a$11$YourHashedPasswordHere', -- Se actualizará con el hash real
    'Colaborador', 
    1, 
    GETUTCDATE()
);

-- Insertar Sergio Chinchilla Arroniz (Colaborador)
INSERT INTO auth.Usuarios (Username, FullName, Email, PasswordHash, Rol, Activo, CreadoUtc)
VALUES (
    'schinchilla', 
    'Sergio Chinchilla Arroniz', 
    'sergio.chinchilla@prueba.com', 
    '$2a$11$YourHashedPasswordHere', -- Se actualizará con el hash real
    'Colaborador', 
    1, 
    GETUTCDATE()
);

-- Insertar Raquel Cordero (Colaboradora)
INSERT INTO auth.Usuarios (Username, FullName, Email, PasswordHash, Rol, Activo, CreadoUtc)
VALUES (
    'rcordero', 
    'Raquel Cordero', 
    'raquel.cordero@prueba.com', 
    '$2a$11$YourHashedPasswordHere', -- Se actualizará con el hash real
    'Colaborador', 
    1, 
    GETUTCDATE()
);

-- Insertar Karla Ramírez Pérez (Colaboradora)
INSERT INTO auth.Usuarios (Username, FullName, Email, PasswordHash, Rol, Activo, CreadoUtc)
VALUES (
    'kramirez', 
    'Karla Ramírez Pérez', 
    'karla.ramirez@prueba.com', 
    '$2a$11$YourHashedPasswordHere', -- Se actualizará con el hash real
    'Colaborador', 
    1, 
    GETUTCDATE()
);

-- Insertar Paola Zúñiga Fernández (Colaboradora)
INSERT INTO auth.Usuarios (Username, FullName, Email, PasswordHash, Rol, Activo, CreadoUtc)
VALUES (
    'pzuniga', 
    'Paola Zúñiga Fernández', 
    'paola.zuniga@prueba.com', 
    '$2a$11$YourHashedPasswordHere', -- Se actualizará con el hash real
    'Colaborador', 
    1, 
    GETUTCDATE()
);

GO

-- Verificar que se crearon correctamente
SELECT Username, FullName, Email, Rol, Activo 
FROM auth.Usuarios 
WHERE Username IN ('aortega', 'carce', 'schinchilla', 'rcordero', 'kramirez', 'pzuniga')
ORDER BY Username;
GO
