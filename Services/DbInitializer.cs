using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;

namespace GestionPlazasVacantes.Services
{
    public static class DbInitializer
    {
        public static void Initialize(AppDbContext context)
        {
            // Asegurar que la base de datos está creada
            context.Database.EnsureCreated();

            // Verificar si ya existen datos
            if (context.Usuarios.Any())
            {
                return; // Ya hay datos
            }

            // ===== CREAR USUARIOS =====
            var usuarios = new List<Usuario>
            {
                new Usuario
                {
                    Username = "lreyes",
                    FullName = "Luis Reyes",
                    Email = "lreyes@example.com",
                    PasswordHash = Services.PasswordHasher.HashPassword("lreyes1234"),
                    Rol = RolUsuario.Jefe,
                    Activo = true,
                    CreadoUtc = DateTime.UtcNow
                },
                new Usuario
                {
                    Username = "gluna",
                    FullName = "Gerardo Luna",
                    Email = "gluna@example.com",
                    PasswordHash = Services.PasswordHasher.HashPassword("gluna1234"),
                    Rol = RolUsuario.Colaborador,
                    Activo = true,
                    CreadoUtc = DateTime.UtcNow
                },
                new Usuario
                {
                    Username = "jodio",
                    FullName = "John Odio",
                    Email = "jodio@example.com",
                    PasswordHash = Services.PasswordHasher.HashPassword("jodio1234"),
                    Rol = RolUsuario.Colaborador,
                    Activo = true,
                    CreadoUtc = DateTime.UtcNow
                }
            };
            
            context.Usuarios.AddRange(usuarios);
            context.SaveChanges();

            // ===== CREAR PLAZAS VACANTES =====
            var plazas = new List<PlazaVacante>
            {
                new PlazaVacante
                {
                    TipoConcurso = "Interno",
                    NumeroConcurso = "INT-2024-001",
                    Titulo = "Analista de Sistemas Senior",
                    Departamento = "Tecnologías de Información",
                    SalarioCompuesto = 850000,
                    SalarioGlobal = 1200000,
                    Horario = "Lunes a Viernes 8:00 AM - 5:00 PM",
                    FechaLimite = DateTime.Now.AddDays(15),
                    Requisitos = "Bachillerato en Ingeniería en Sistemas, 5 años de experiencia, conocimientos en .NET Core, SQL Server",
                    Observaciones = "Se requiere disponibilidad inmediata",
                    Activa = true,
                    SolicitarColegiatura = true,
                    ColegiaturaObligatoria = true,
                    SolicitarLicencia = false,
                    LicenciaObligatoria = false,
                    SolicitarPermisoArmas = false,
                    ArmasObligatorio = false,
                    SolicitarTitulos = true,
                    TitulosObligatorios = true,
                    FechaCreacion = DateTime.Now,
                    Estado = "Abierta",
                    EstadoFinal = "En Proceso",
                    FechaCierre = null,
                    UsuarioAsignadoId = usuarios[1].Id // Asignada a Gerardo Luna
                },
                new PlazaVacante
                {
                    TipoConcurso = "Externo",
                    NumeroConcurso = "EXT-2024-002",
                    Titulo = "Oficial de Seguridad",
                    Departamento = "Seguridad Municipal",
                    SalarioCompuesto = 450000,
                    SalarioGlobal = 650000,
                    Horario = "Turnos rotativos 24/7",
                    FechaLimite = DateTime.Now.AddDays(30),
                    Requisitos = "Bachillerato en educación media, licencia de conducir B1, permiso de portación de armas",
                    Observaciones = "Requiere examen psicométrico y prueba física",
                    Activa = true,
                    SolicitarColegiatura = false,
                    ColegiaturaObligatoria = false,
                    SolicitarLicencia = true,
                    LicenciaObligatoria = true,
                    SolicitarPermisoArmas = true,
                    ArmasObligatorio = true,
                    SolicitarTitulos = false,
                    TitulosObligatorios = false,
                    FechaCreacion = DateTime.Now.AddDays(-5),
                    Estado = "Abierta",
                    EstadoFinal = "En Proceso",
                    FechaCierre = null,
                    UsuarioAsignadoId = usuarios[2].Id // Asignada a John Odio
                },
                new PlazaVacante
                {
                    TipoConcurso = "Interno",
                    NumeroConcurso = "INT-2024-003",
                    Titulo = "Contador General",
                    Departamento = "Contabilidad",
                    SalarioCompuesto = 750000,
                    SalarioGlobal = 1050000,
                    Horario = "Lunes a Viernes 7:30 AM - 4:30 PM",
                    FechaLimite = DateTime.Now.AddDays(-2),
                    Requisitos = "Licenciatura en Contaduría Pública, colegiatura al día, 3 años de experiencia en sector público",
                    Observaciones = "Preferiblemente con conocimientos en SIGAF",
                    Activa = false,
                    SolicitarColegiatura = true,
                    ColegiaturaObligatoria = true,
                    SolicitarLicencia = false,
                    LicenciaObligatoria = false,
                    SolicitarPermisoArmas = false,
                    ArmasObligatorio = false,
                    SolicitarTitulos = true,
                    TitulosObligatorios = true,
                    FechaCreacion = DateTime.Now.AddDays(-20),
                    Estado = "Cerrada",
                    EstadoFinal = "Finalizada",
                    FechaCierre = DateTime.Now.AddDays(-1),
                    UsuarioAsignadoId = null // Plaza cerrada, sin asignación
                }
            };

            context.PlazasVacantes.AddRange(plazas);
            context.SaveChanges();

            // ===== CREAR POSTULANTES =====
            var postulantes = new List<Postulante>
            {
                // Postulantes para Analista de Sistemas
                new Postulante
                {
                    NombreCompleto = "María Fernández Solís",
                    Cedula = "1-1234-5678",
                    Correo = "maria.fernandez@email.com",
                    Telefono = "8888-1234",
                    Direccion = "San José, Curridabat",
                    PerfilProfesional = "Ingeniera en Sistemas con 6 años de experiencia en desarrollo de software",
                    ExperienciaLaboral = "Desarrolladora Senior en empresa privada (2018-2024)",
                    FormacionAcademica = "Bachillerato en Ingeniería en Sistemas - UCR",
                    Habilidades = "C#, .NET Core, SQL Server, Azure, Git",
                    Idiomas = "Español (nativo), Inglés (avanzado)",
                    FormacionComplementaria = "Certificación Microsoft Azure Developer",
                    NumeroColegiatura = "CFIA-12345",
                    EstadoProceso = "En Revisión",
                    FechaActualizacion = DateTime.Now,
                    PlazaVacanteId = plazas[0].Id
                },
                new Postulante
                {
                    NombreCompleto = "Carlos Rodríguez Mora",
                    Cedula = "2-2345-6789",
                    Correo = "carlos.rodriguez@email.com",
                    Telefono = "8888-5678",
                    Direccion = "Cartago, Tres Ríos",
                    PerfilProfesional = "Desarrollador Full Stack con experiencia en proyectos gubernamentales",
                    ExperienciaLaboral = "Analista de Sistemas en MIDEPLAN (2019-2024)",
                    FormacionAcademica = "Licenciatura en Ingeniería en Computación - TEC",
                    Habilidades = "Java, Spring Boot, Angular, PostgreSQL, Docker",
                    Idiomas = "Español (nativo), Inglés (intermedio)",
                    FormacionComplementaria = "Curso de Arquitectura de Software",
                    NumeroColegiatura = "CFIA-23456",
                    EstadoProceso = "Aprobado",
                    FechaActualizacion = DateTime.Now,
                    PlazaVacanteId = plazas[0].Id
                },
                
                // Postulantes para Oficial de Seguridad
                new Postulante
                {
                    NombreCompleto = "Jorge Méndez Castro",
                    Cedula = "3-3456-7890",
                    Correo = "jorge.mendez@email.com",
                    Telefono = "8888-9012",
                    Direccion = "Alajuela, San Rafael",
                    PerfilProfesional = "Ex-oficial de policía con 8 años de servicio",
                    ExperienciaLaboral = "Oficial de Policía Municipal (2015-2023)",
                    FormacionAcademica = "Bachillerato en Educación Media",
                    Habilidades = "Manejo de armas, primeros auxilios, defensa personal",
                    Idiomas = "Español (nativo)",
                    FormacionComplementaria = "Curso de Seguridad Privada, Certificación en Primeros Auxilios",
                    TipoLicencia = "B1, B2",
                    NumeroPermisoArmas = "PA-2024-001",
                    EstadoProceso = "En Proceso",
                    FechaActualizacion = DateTime.Now,
                    PlazaVacanteId = plazas[1].Id
                },
                new Postulante
                {
                    NombreCompleto = "Ana Vargas Jiménez",
                    Cedula = "4-4567-8901",
                    Correo = "ana.vargas@email.com",
                    Telefono = "8888-3456",
                    Direccion = "Heredia, Santo Domingo",
                    PerfilProfesional = "Técnica en seguridad con experiencia en vigilancia",
                    ExperienciaLaboral = "Oficial de Seguridad en centro comercial (2020-2024)",
                    FormacionAcademica = "Bachillerato en Educación Media",
                    Habilidades = "Vigilancia, control de acceso, manejo de cámaras",
                    Idiomas = "Español (nativo), Inglés (básico)",
                    FormacionComplementaria = "Curso de Seguridad y Vigilancia",
                    TipoLicencia = "B1",
                    NumeroPermisoArmas = "PA-2024-002",
                    EstadoProceso = "Rechazado",
                    FechaActualizacion = DateTime.Now,
                    PlazaVacanteId = plazas[1].Id
                },

                // Postulante para Contador (plaza cerrada)
                new Postulante
                {
                    NombreCompleto = "Roberto Sánchez Pérez",
                    Cedula = "5-5678-9012",
                    Correo = "roberto.sanchez@email.com",
                    Telefono = "8888-7890",
                    Direccion = "San José, Escazú",
                    PerfilProfesional = "Contador Público Autorizado con experiencia en sector público",
                    ExperienciaLaboral = "Contador en Ministerio de Hacienda (2018-2024)",
                    FormacionAcademica = "Licenciatura en Contaduría Pública - UCR",
                    Habilidades = "SIGAF, Excel avanzado, auditoría, presupuestos",
                    Idiomas = "Español (nativo)",
                    FormacionComplementaria = "Curso de NIIF, Diplomado en Auditoría",
                    NumeroColegiatura = "CCPA-34567",
                    EstadoProceso = "Contratado",
                    FechaActualizacion = DateTime.Now.AddDays(-1),
                    PlazaVacanteId = plazas[2].Id
                }
            };

            context.Postulantes.AddRange(postulantes);
            context.SaveChanges();

            // ===== CREAR SEGUIMIENTOS =====
            var seguimientos = new List<SeguimientoPostulante>
            {
                // Seguimiento María Fernández
                new SeguimientoPostulante
                {
                    PostulanteId = postulantes[0].Id,
                    PlazaVacanteId = plazas[0].Id,
                    EtapaActual = "Revisión de Documentos",
                    Observaciones = "Documentación completa. Cumple con todos los requisitos. Pendiente de entrevista técnica.",
                    NotaPruebaTecnica = null,
                    NotaPsicometrica = null,
                    CumpleRequisitos = true,
                    Aprobado = false,
                    FechaActualizacion = DateTime.Now,
                    MotivoDescarte = null,
                    Activo = true
                },
                
                // Seguimiento Carlos Rodríguez
                new SeguimientoPostulante
                {
                    PostulanteId = postulantes[1].Id,
                    PlazaVacanteId = plazas[0].Id,
                    EtapaActual = "Entrevista Final",
                    Observaciones = "Excelente desempeño en prueba técnica. Muy buena comunicación. Recomendado para contratación.",
                    NotaPruebaTecnica = 95.50m,
                    NotaPsicometrica = 88.75m,
                    CumpleRequisitos = true,
                    Aprobado = true,
                    FechaActualizacion = DateTime.Now,
                    MotivoDescarte = null,
                    Activo = true
                },

                // Seguimiento Jorge Méndez
                new SeguimientoPostulante
                {
                    PostulanteId = postulantes[2].Id,
                    PlazaVacanteId = plazas[1].Id,
                    EtapaActual = "Prueba Psicométrica",
                    Observaciones = "Aprobó prueba física. En espera de resultados psicométricos.",
                    NotaPruebaTecnica = null,
                    NotaPsicometrica = 82.00m,
                    CumpleRequisitos = true,
                    Aprobado = false,
                    FechaActualizacion = DateTime.Now,
                    MotivoDescarte = null,
                    Activo = true
                },

                // Seguimiento Ana Vargas
                new SeguimientoPostulante
                {
                    PostulanteId = postulantes[3].Id,
                    PlazaVacanteId = plazas[1].Id,
                    EtapaActual = "Descartado",
                    Observaciones = "No aprobó la prueba física requerida para el puesto.",
                    NotaPruebaTecnica = null,
                    NotaPsicometrica = 75.50m,
                    CumpleRequisitos = false,
                    Aprobado = false,
                    FechaActualizacion = DateTime.Now,
                    MotivoDescarte = "No cumple con requisitos físicos del puesto",
                    Activo = false
                },

                // Seguimiento Roberto Sánchez
                new SeguimientoPostulante
                {
                    PostulanteId = postulantes[4].Id,
                    PlazaVacanteId = plazas[2].Id,
                    EtapaActual = "Contratado",
                    Observaciones = "Proceso finalizado exitosamente. Candidato seleccionado e incorporado.",
                    NotaPruebaTecnica = 92.00m,
                    NotaPsicometrica = 90.25m,
                    CumpleRequisitos = true,
                    Aprobado = true,
                    FechaActualizacion = DateTime.Now.AddDays(-1),
                    MotivoDescarte = null,
                    Activo = true
                }
            };

            context.SeguimientosPostulantes.AddRange(seguimientos);
            context.SaveChanges();
        }
    }
}
