using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GestionPlazasVacantes.Controllers
{
    [AllowAnonymous]
    public class SeedController : Controller
    {
        private readonly AppDbContext _context;

        public SeedController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return Content("Usa /Seed/Generar para datos y /Seed/SimularCierre para cerrar plazas.");
        }

        public async Task<IActionResult> Generar()
        {
            // 1. Asegurar Usuarios (Crear o Actualizar)
            var jefe = await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == "lreyes");
            if (jefe == null)
            {
                jefe = new Usuario
                {
                    Username = "lreyes",
                    FullName = "Lucía Reyes",
                    Email = "lreyes@empresa.com",
                    Rol = RolUsuario.Jefe,
                    Activo = true,
                    CreadoUtc = DateTime.UtcNow
                };
                _context.Usuarios.Add(jefe);
            }
            // Siempre actualizar la contraseña para asegurar que sea la correcta
            jefe.SetPassword("lreyes1234");

            if (!await _context.Usuarios.AnyAsync(u => u.Username == "jperez"))
            {
                var colab1 = new Usuario
                {
                    Username = "jperez",
                    FullName = "Juan Pérez",
                    Email = "jperez@empresa.com",
                    Rol = RolUsuario.Colaborador,
                    Activo = true,
                    CreadoUtc = DateTime.UtcNow
                };
                colab1.SetPassword("User123!");
                _context.Usuarios.Add(colab1);
            }

            if (!await _context.Usuarios.AnyAsync(u => u.Username == "mmontero"))
            {
                var colab2 = new Usuario
                {
                    Username = "mmontero",
                    FullName = "María Montero",
                    Email = "mmontero@empresa.com",
                    Rol = RolUsuario.Colaborador,
                    Activo = true,
                    CreadoUtc = DateTime.UtcNow
                };
                colab2.SetPassword("User123!");
                _context.Usuarios.Add(colab2);
            }

            await _context.SaveChangesAsync();

            var usuariosColaboradores = await _context.Usuarios
                .Where(u => u.Rol == RolUsuario.Colaborador && u.Activo)
                .ToListAsync();

            // 2. Crear Plazas
            if (await _context.PlazasVacantes.CountAsync() < 5)
            {
                var plazas = new List<PlazaVacante>();
                var departamentos = new[] { "Recursos Humanos", "TI", "Ventas", "Finanzas", "Operaciones" };
                var puestos = new[] { "Analista", "Gerente", "Asistente", "Desarrollador", "Especialista" };
                var rnd = new Random();

                var fechaLimiteHoy = DateTime.Today.AddHours(17);

                for (int i = 1; i <= 15; i++)
                {
                    var depto = departamentos[rnd.Next(departamentos.Length)];
                    var puesto = puestos[rnd.Next(puestos.Length)];
                    var usuarioAsignado = usuariosColaboradores.Any() 
                        ? usuariosColaboradores[rnd.Next(usuariosColaboradores.Count)] 
                        : null;

                    var plaza = new PlazaVacante
                    {
                        Titulo = $"{puesto} de {depto} {i}",
                        TipoConcurso = i % 2 == 0 ? "Interno" : "Externo",
                        NumeroConcurso = $"2025-{i:000}",
                        Departamento = depto,
                        SalarioCompuesto = rnd.Next(500000, 1500000),
                        SalarioGlobal = rnd.Next(600000, 1800000),
                        Horario = "Lunes a Viernes 8:00am - 5:00pm",
                        FechaLimite = fechaLimiteHoy,
                        FechaCreacion = DateTime.Now.AddDays(-rnd.Next(1, 10)),
                        Estado = "Abierta",
                        EstadoFinal = "En proceso",
                        Requisitos = "Título universitario en área afín.\nExperiencia mínima de 2 años.\nInglés intermedio.",
                        Observaciones = "Disponibilidad inmediata.",
                        UsuarioAsignadoId = usuarioAsignado?.Id,
                        Activa = true
                    };
                    plazas.Add(plaza);
                }
                _context.PlazasVacantes.AddRange(plazas);
                await _context.SaveChangesAsync();
            }

            // 3. Crear Postulantes
            var plazasExistentes = await _context.PlazasVacantes
                .Include(p => p.Postulantes)
                .ToListAsync();

            var nombres = new[] { "Ana", "Carlos", "Elena", "Luis", "Sofía", "Miguel", "Laura", "David", "Carmen", "Jose" };
            var apellidos = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            var rnd2 = new Random();

            int nuevosPostulantes = 0;

            foreach (var plaza in plazasExistentes)
            {
                if (plaza.Postulantes.Count < 5)
                {
                    int cantidadACrear = rnd2.Next(5, 12);
                    for (int k = 0; k < cantidadACrear; k++)
                    {
                        var nombre = nombres[rnd2.Next(nombres.Length)] + " " + apellidos[rnd2.Next(apellidos.Length)];
                        var cedula = rnd2.Next(100000000, 999999999).ToString();

                        var postulante = new Postulante
                        {
                            NombreCompleto = nombre,
                            Cedula = cedula,
                            Correo = $"{nombre.Replace(" ", "").ToLower()}@email.com",
                            Telefono = $"8{rnd2.Next(1000000, 9999999)}",
                            Direccion = "San José, Costa Rica",
                            EstadoProceso = "En revisión",
                            FechaActualizacion = DateTime.Now,
                            PlazaVacanteId = plaza.Id,
                            PerfilProfesional = "Profesional con experiencia.",
                            ExperienciaLaboral = "3 años en puestos similares.",
                            FormacionAcademica = "Licenciatura."
                        };

                        _context.Postulantes.Add(postulante);
                        await _context.SaveChangesAsync();

                        var seguimiento = new SeguimientoPostulante
                        {
                            PostulanteId = postulante.Id,
                            PlazaVacanteId = plaza.Id,
                            EtapaActual = "Revisión de Atestados",
                            Observaciones = "Pendiente de revisar documentos.",
                            CumpleRequisitos = true,
                            Aprobado = false,
                            Activo = true,
                            FechaActualizacion = DateTime.Now
                        };
                        _context.SeguimientosPostulantes.Add(seguimiento);
                        nuevosPostulantes++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Content($"Datos generados exitosamente.\nNuevos postulantes creados: {nuevosPostulantes}");
        }

        public async Task<IActionResult> SimularCierre()
        {
            // Forzamos la fecha límite de todas las plazas activas a \"ayer\" para simular que vencieron y cerramos
            var plazasActivas = await _context.PlazasVacantes
                .Where(p => p.Activa && p.Estado == "Abierta")
                .ToListAsync();

            int plazasCerradas = 0;
            int candidatosDescartados = 0;
            var rnd = new Random();

            foreach (var plaza in plazasActivas)
            {
                plaza.Estado = "Cerrada";
                plaza.EstadoFinal = "Evaluación";
                plaza.FechaCierre = DateTime.Now;

                var seguimientos = await _context.SeguimientosPostulantes
                    .Include(s => s.Postulante)
                    .Where(s => s.PlazaVacanteId == plaza.Id && s.Activo)
                    .ToListAsync();

                foreach (var s in seguimientos)
                {
                    // Simulación de descarte (30% probabilidad)
                    int factorSuerte = rnd.Next(100);
                    
                    if (factorSuerte < 10) // 10%: No entregó documentos
                    {
                        s.CumpleRequisitos = false;
                        s.Activo = false;
                        s.EtapaActual = "Descartado";
                        s.Observaciones = "No cumple requisitos mínimos (filtrado automático).";
                        s.MotivoDescarte = "No entregó documentación completa";
                        s.Postulante.EstadoProceso = "Descartado";
                        candidatosDescartados++;
                    }
                    else if (factorSuerte < 20) // 10%: Reprobó técnicas (simulado)
                    {
                        s.CumpleRequisitos = true;
                        s.Activo = false;
                        s.EtapaActual = "Descartado";
                        s.NotaPruebaTecnica = rnd.Next(40, 69); // Nota baja
                        s.Observaciones = "Reprobó pruebas técnicas.";
                        s.MotivoDescarte = "Reprobó pruebas técnicas (< 70)";
                        s.Postulante.EstadoProceso = "Descartado";
                        candidatosDescartados++;
                    }
                    else if (factorSuerte < 30) // 10%: Reprobó psicométricas
                    {
                        s.CumpleRequisitos = true;
                        s.Activo = false;
                        s.EtapaActual = "Descartado";
                        s.NotaPruebaTecnica = rnd.Next(70, 100);
                        s.NotaPsicometrica = rnd.Next(40, 69); // Nota baja
                        s.Observaciones = "Reprobó pruebas psicométricas.";
                        s.MotivoDescarte = "Reprobó pruebas psicométricas (< 70)";
                        s.Postulante.EstadoProceso = "Descartado";
                        candidatosDescartados++;
                    }
                    else
                    {
                        // Avanza
                        s.CumpleRequisitos = true;
                        s.EtapaActual = "Pruebas Técnicas";
                        s.NotaPruebaTecnica = rnd.Next(70, 100);
                        s.NotaPsicometrica = rnd.Next(70, 100);
                        s.Observaciones = "Aprobado filtro inicial. Pendiente programación de pruebas.";
                        s.Postulante.EstadoProceso = "En pruebas";
                    }
                    s.FechaActualizacion = DateTime.Now;
                }

                plazasCerradas++;
            }

            await _context.SaveChangesAsync();
            return Content($"Simulación completada.\nPlazas cerradas: {plazasCerradas}\nCandidatos descartados automáticamente: {candidatosDescartados}");
        }
    }
}
