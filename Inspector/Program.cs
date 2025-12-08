using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using GestionPlazasVacantes.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

Console.WriteLine("=== Generador de Hashes para Nuevos Usuarios ===\n");

// Definir usuarios a crear
var nuevosUsuarios = new[]
{
    new { Username = "aortega", Password = "aortega1234", FullName = "Andrea Ortega Bermúdez", Email = "andrea.ortega@prueba.com", Rol = "Jefe" },
    new { Username = "carce", Password = "carce1234", FullName = "Claudia Arce Lopez", Email = "claudia.arce@prueba.com", Rol = "Colaborador" },
    new { Username = "schinchilla", Password = "schinchilla1234", FullName = "Sergio Chinchilla Arroniz", Email = "sergio.chinchilla@prueba.com", Rol = "Colaborador" },
    new { Username = "rcordero", Password = "rcordero1234", FullName = "Raquel Cordero", Email = "raquel.cordero@prueba.com", Rol = "Colaborador" },
    new { Username = "kramirez", Password = "kramirez1234", FullName = "Karla Ramírez Pérez", Email = "karla.ramirez@prueba.com", Rol = "Colaborador" },
    new { Username = "pzuniga", Password = "pzuniga1234", FullName = "Paola Zúñiga Fernández", Email = "paola.zuniga@prueba.com", Rol = "Colaborador" }
};

Console.WriteLine("Generando hashes de contraseñas...\n");

foreach (var user in nuevosUsuarios)
{
    var hash = PasswordHasher.HashPassword(user.Password);
    Console.WriteLine($"Usuario: {user.Username}");
    Console.WriteLine($"Nombre: {user.FullName}");
    Console.WriteLine($"Email: {user.Email}");
    Console.WriteLine($"Rol: {user.Rol}");
    Console.WriteLine($"Hash: {hash}");
    Console.WriteLine();
}

Console.WriteLine("\n=== Insertando usuarios en la base de datos ===\n");

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseSqlServer("Server=localhost;Database=GestionPlazasVacantesDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

using (var context = new AppDbContext(optionsBuilder.Options))
{
    foreach (var user in nuevosUsuarios)
    {
        // Verificar si el usuario ya existe
        var existeUsuario = context.Usuarios.Any(u => u.Username == user.Username);
        
        if (existeUsuario)
        {
            Console.WriteLine($"⚠️  Usuario '{user.Username}' ya existe. Saltando...");
            continue;
        }

        var nuevoUsuario = new Usuario
        {
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PasswordHash = PasswordHasher.HashPassword(user.Password),
            Rol = user.Rol == "Jefe" ? RolUsuario.Jefe : RolUsuario.Colaborador,
            Activo = true,
            CreadoUtc = DateTime.UtcNow
        };

        context.Usuarios.Add(nuevoUsuario);
        Console.WriteLine($"✅ Usuario '{user.Username}' agregado.");
    }

    var cambios = context.SaveChanges();
    Console.WriteLine($"\n✅ {cambios} usuarios creados exitosamente en la base de datos.");
}

Console.WriteLine("\n=== Verificando usuarios creados ===\n");

using (var context = new AppDbContext(optionsBuilder.Options))
{
    var usuarios = context.Usuarios
        .Where(u => nuevosUsuarios.Select(nu => nu.Username).Contains(u.Username))
        .OrderBy(u => u.Username)
        .ToList();

    foreach (var usuario in usuarios)
    {
        Console.WriteLine($"✓ {usuario.Username} - {usuario.FullName} ({usuario.Rol}) - {usuario.Email}");
    }
}

Console.WriteLine("\n=== Proceso completado ===");
