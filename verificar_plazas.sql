-- Script para verificar plazas en la base de datos
USE GestionPlazasVacantesDB;
GO

-- Ver todas las plazas (sin filtros)
SELECT 
    Id,
    Titulo,
    TipoConcurso,
    NumeroConcurso,
    FechaCreacion,
    FechaLimite,
    Activa,
    Estado,
    EstadoFinal
FROM [gestion].[PlazasVacantes]
ORDER BY FechaCreacion DESC;

-- Contar total de plazas
SELECT COUNT(*) as TotalPlazas FROM [gestion].[PlazasVacantes];

-- Ver plazas que aparecerían en el Dashboard (con filtro actual)
SELECT 
    Id,
    Titulo,
    FechaLimite,
    Estado,
    CASE 
        WHEN FechaLimite > GETDATE() THEN 'Aparece en Dashboard'
        ELSE 'NO aparece (fecha vencida)'
    END as EstadoDashboard
FROM [gestion].[PlazasVacantes]
WHERE (Estado = 'Abierta' OR Estado IS NULL);

-- Ver plazas que aparecerían en el Listado de Plazas (con filtro actual)
SELECT 
    Id,
    Titulo,
    Activa,
    CASE 
        WHEN Activa = 1 THEN 'Aparece en Listado'
        ELSE 'NO aparece (inactiva)'
    END as EstadoListado
FROM [gestion].[PlazasVacantes];
