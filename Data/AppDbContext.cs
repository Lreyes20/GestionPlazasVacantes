using GestionPlazasVacantes.Models;
using Microsoft.EntityFrameworkCore;

namespace GestionPlazasVacantes.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<PlazaVacante> PlazasVacantes => Set<PlazaVacante>();
        public DbSet<Postulante> Postulantes => Set<Postulante>();
        public DbSet<SeguimientoPostulante> SeguimientosPostulantes => Set<SeguimientoPostulante>();

        protected override void OnModelCreating(ModelBuilder mb)
        {
            // === USUARIOS ===
            mb.Entity<Usuario>(e =>
            {
                e.ToTable("Usuarios", "auth");
                e.Property(x => x.Username).HasMaxLength(50).IsRequired();
                e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
                e.Property(x => x.Email).HasMaxLength(150).IsRequired();
                e.Property(x => x.Password).HasMaxLength(100).IsRequired();
                e.Property(x => x.Activo).IsRequired();
                e.Property(x => x.Rol).IsRequired().HasConversion<string>();

                // Relación con plazas asignadas
                e.HasMany(u => u.PlazasAsignadas)
                 .WithOne(p => p.UsuarioAsignado)
                 .HasForeignKey(p => p.UsuarioAsignadoId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // === PLAZAS ===
            mb.Entity<PlazaVacante>(e =>
            {
                e.ToTable("PlazasVacantes", "gestion");
                e.Property(p => p.Titulo).HasMaxLength(150).IsRequired();
                e.Property(p => p.TipoConcurso).HasMaxLength(50).IsRequired();
                e.Property(p => p.NumeroConcurso).HasMaxLength(50).IsRequired();
                e.Property(p => p.Requisitos).HasMaxLength(1000).IsRequired();
                e.Property(p => p.Observaciones).HasMaxLength(500);
                e.Property(p => p.SalarioCompuesto).HasPrecision(18, 2);
                e.Property(p => p.SalarioGlobal).HasPrecision(18, 2);
                e.Property(p => p.FechaLimite).IsRequired();
                e.Property(p => p.FechaCreacion).HasDefaultValueSql("GETDATE()");
                e.Property(p => p.Estado).HasMaxLength(20).IsRequired();
                e.Property(p => p.FechaCierre);

                e.HasMany(p => p.Postulantes)
                 .WithOne(x => x.PlazaVacante)
                 .HasForeignKey(x => x.PlazaVacanteId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // === POSTULANTES ===
            mb.Entity<Postulante>(e =>
            {
                e.ToTable("Postulantes", "gestion");

                e.HasKey(p => p.Id);
                e.Property(p => p.Id)
                    .ValueGeneratedOnAdd();

               
                e.Property(p => p.NombreCompleto).HasMaxLength(150).IsRequired();
                e.Property(p => p.Cedula).HasMaxLength(20);
                e.Property(p => p.Correo).HasMaxLength(150);
                e.Property(p => p.Telefono).HasMaxLength(30);
                e.Property(p => p.EstadoProceso).HasMaxLength(30).IsRequired();
                e.Property(p => p.FechaActualizacion).HasDefaultValueSql("GETDATE()");
            });

            // === SEGUIMIENTO POSTULANTES ===
            mb.Entity<SeguimientoPostulante>(e =>
            {
                e.ToTable("SeguimientosPostulantes", "gestion");

                e.Property(s => s.EtapaActual).HasMaxLength(80);
                e.Property(s => s.Observaciones).HasMaxLength(800);
                e.Property(s => s.NotaPruebaTecnica).HasPrecision(5, 2);
                e.Property(s => s.NotaPsicometrica).HasPrecision(5, 2);
                e.Property(s => s.FechaActualizacion).HasDefaultValueSql("GETDATE()");
                e.Property(s => s.CumpleRequisitos).HasDefaultValue(false);
                e.Property(s => s.Aprobado).HasDefaultValue(false);

                // Relación con Postulante y PlazaVacante
                e.HasOne(s => s.Postulante)
                    .WithMany()
                    .HasForeignKey(s => s.PostulanteId)
                    .OnDelete(DeleteBehavior.NoAction);

                e.HasOne(s => s.PlazaVacante)
                    .WithMany()
                    .HasForeignKey(s => s.PlazaVacanteId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            base.OnModelCreating(mb);
        }
    }
}
