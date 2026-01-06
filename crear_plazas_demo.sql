-- Script para crear plazas de demostración
USE GestionPlazasVacantesDB;
GO

-- Plaza INTERNA de demostración
INSERT INTO [gestion].[PlazasVacantes] (
    Titulo,
    TipoConcurso,
    NumeroConcurso,
    Departamento,
    Requisitos,
    SalarioCompuesto,
    SalarioGlobal,
    Horario,
    FechaCreacion,
    FechaLimite,
    Activa,
    Estado,
    EstadoFinal,
    Observaciones,
    SolicitarColegiatura,
    ColegiaturaObligatoria,
    SolicitarLicencia,
    LicenciaObligatoria,
    SolicitarPermisoArmas,
    ArmasObligatorio,
    SolicitarTitulos,
    TitulosObligatorios
)
VALUES (
    'Profesional Analista',
    'Interno',
    'INT-2025-001',
    'Recursos Humanos',
    'Bachillerato universitario en Administración de Empresas, Psicología o carrera afín. Experiencia mínima de 2 años en gestión de personal.',
    850000.00,
    1050000.00,
    'Lunes a Viernes, 8:00 a.m. - 4:00 p.m.',
    GETDATE(),
    DATEADD(DAY, 30, GETDATE()), -- 30 días desde hoy
    1,
    'Abierta',
    'Abierta',
    'Se valorará experiencia en sector público',
    1, -- Solicitar colegiatura
    1, -- Obligatoria
    0, -- No solicitar licencia
    0,
    0, -- No solicitar armas
    0,
    1, -- Solicitar títulos
    1  -- Obligatorios
);

-- Plaza EXTERNA de demostración
INSERT INTO [gestion].[PlazasVacantes] (
    Titulo,
    TipoConcurso,
    NumeroConcurso,
    Departamento,
    Requisitos,
    SalarioCompuesto,
    SalarioGlobal,
    Horario,
    FechaCreacion,
    FechaLimite,
    Activa,
    Estado,
    EstadoFinal,
    Observaciones,
    SolicitarColegiatura,
    ColegiaturaObligatoria,
    SolicitarLicencia,
    LicenciaObligatoria,
    SolicitarPermisoArmas,
    ArmasObligatorio,
    SolicitarTitulos,
    TitulosObligatorios
)
VALUES (
    'Ingeniero Civil',
    'Externo',
    'EXT-2025-001',
    'Obras Públicas',
    'Bachillerato universitario en Ingeniería Civil. Incorporado al Colegio Federado de Ingenieros y Arquitectos. Experiencia mínima de 3 años en proyectos de infraestructura.',
    1200000.00,
    1500000.00,
    'Lunes a Viernes, 7:30 a.m. - 3:30 p.m.',
    GETDATE(),
    DATEADD(DAY, 45, GETDATE()), -- 45 días desde hoy
    1,
    'Abierta',
    'Abierta',
    'Se requiere disponibilidad para visitas de campo',
    1, -- Solicitar colegiatura
    1, -- Obligatoria
    1, -- Solicitar licencia
    1, -- Obligatoria
    0, -- No solicitar armas
    0,
    1, -- Solicitar títulos
    1  -- Obligatorios
);

-- Verificar que se crearon correctamente
SELECT 
    Id,
    Titulo,
    TipoConcurso,
    NumeroConcurso,
    Departamento,
    FechaLimite,
    DATEDIFF(DAY, GETDATE(), FechaLimite) as DiasRestantes,
    Activa,
    Estado
FROM [gestion].[PlazasVacantes]
WHERE NumeroConcurso IN ('INT-2025-001', 'EXT-2025-001')
ORDER BY TipoConcurso;

PRINT '✅ Plazas de demostración creadas exitosamente';
PRINT 'Plaza Interna: Profesional Analista (INT-2025-001) - Vence en 30 días';
PRINT 'Plaza Externa: Ingeniero Civil (EXT-2025-001) - Vence en 45 días';
