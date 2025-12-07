-- Script para crear usuario lreyes en la base de datos GestionPlazasVacantes
USE GestionPlazasVacantesDB;
GO

INSERT INTO auth.Usuarios (Username, FullName, Email, Password, Activo, CreadoUtc)
VALUES ('lreyes', 'Luis Reyes', 'lreyes@example.com', 'lreyes1234', 1, GETUTCDATE());
GO

-- Verificar que se creó correctamente
SELECT * FROM auth.Usuarios WHERE Username = 'lreyes';
GO
