-- Script para crear una plaza de prueba y verificar guardado
USE GestionPlazasVacantesDB;
GO

-- Insertar plaza de prueba
INSERT INTO [gestion].[PlazasVacantes] 
(TipoConcurso, NumeroConcurso, Titulo, Departamento, SalarioCompuesto, SalarioGlobal, 
 Horario, FechaLimite, Requisitos, Observaciones, Activa, 
 SolicitarColegiatura, ColegiaturaObligatoria, SolicitarLicencia, LicenciaObligatoria, 
 SolicitarPermisoArmas, ArmasObligatorio, SolicitarTitulos, TitulosObligatorios, 
 FechaCreacion, Estado, EstadoFinal)
VALUES 
('Interno', 'SQL-TEST-001', 'Plaza Creada por SQL', 'Testing', 500000, 700000, 
 'L-V 8-5', '2025-12-31', 'Requisitos de prueba SQL', 'Prueba de guardado en BD', 1, 
 0, 0, 0, 0, 0, 0, 0, 0, 
 GETDATE(), 'Abierta', 'En Proceso');
GO

-- Verificar que se creó
SELECT TOP 5 Id, NumeroConcurso, Titulo, Departamento, FechaCreacion 
FROM [gestion].[PlazasVacantes] 
ORDER BY FechaCreacion DESC;
GO

-- Contar total de plazas
SELECT COUNT(*) as TotalPlazas FROM [gestion].[PlazasVacantes];
GO
