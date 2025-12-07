using GestionPlazasVacantes.Data;
using GestionPlazasVacantes.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

Console.WriteLine("Inspeccionando Plazas...");

var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
// Usando la misma connection string que en appsettings (asumiendo localdb por defecto en dev)
optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GestionPlazasVacantes;Trusted_Connection=True;MultipleActiveResultSets=true");

using (var context = new AppDbContext(optionsBuilder.Options))
{
    var plazas = context.PlazasVacantes
        .Select(p => new { p.Id, p.Titulo, p.FechaLimite, p.Activa, p.EstadoFinal })
        .ToList();

    foreach (var p in plazas)
    {
        Console.WriteLine($"ID: {p.Id} | Titulo: {p.Titulo} | FechaLimite: {p.FechaLimite} | Activa: {p.Activa} | EstadoFinal: '{p.EstadoFinal}'");
        
        bool visibleEnIndex = p.Activa && p.FechaLimite >= DateTime.Now && (p.EstadoFinal == "Abierta" || p.EstadoFinal == null);
        Console.WriteLine($" -> Visible en Index? {visibleEnIndex} (Now: {DateTime.Now})");
    }
}
