using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPlazasVacantes.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "gestion");

            migrationBuilder.EnsureSchema(
                name: "auth");

            migrationBuilder.CreateTable(
                name: "PlazasVacantes",
                schema: "gestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipoConcurso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumeroConcurso = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Departamento = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SalarioCompuesto = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SalarioGlobal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Horario = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaLimite = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Requisitos = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activa = table.Column<bool>(type: "bit", nullable: false),
                    SolicitarColegiatura = table.Column<bool>(type: "bit", nullable: false),
                    ColegiaturaObligatoria = table.Column<bool>(type: "bit", nullable: false),
                    SolicitarLicencia = table.Column<bool>(type: "bit", nullable: false),
                    LicenciaObligatoria = table.Column<bool>(type: "bit", nullable: false),
                    SolicitarPermisoArmas = table.Column<bool>(type: "bit", nullable: false),
                    ArmasObligatorio = table.Column<bool>(type: "bit", nullable: false),
                    SolicitarTitulos = table.Column<bool>(type: "bit", nullable: false),
                    TitulosObligatorios = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstadoFinal = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlazasVacantes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                schema: "auth",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    UltimoAccesoUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Postulantes",
                schema: "gestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Cedula = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    PerfilProfesional = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExperienciaLaboral = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormacionAcademica = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Habilidades = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Idiomas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FormacionComplementaria = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OtrosDatos = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroColegiatura = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TipoLicencia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroPermisoArmas = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CurriculumPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotoColegiaturaPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotoLicenciaPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotoPermisoArmasPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ArchivoTitulosPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FotoTituloPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoProceso = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    PlazaVacanteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postulantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Postulantes_PlazasVacantes_PlazaVacanteId",
                        column: x => x.PlazaVacanteId,
                        principalSchema: "gestion",
                        principalTable: "PlazasVacantes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SeguimientosPostulantes",
                schema: "gestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PostulanteId = table.Column<int>(type: "int", nullable: false),
                    EtapaActual = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    NotaPruebaTecnica = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    NotaPsicometrica = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CumpleRequisitos = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Aprobado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    MotivoDescarte = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    PlazaVacanteId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SeguimientosPostulantes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SeguimientosPostulantes_PlazasVacantes_PlazaVacanteId",
                        column: x => x.PlazaVacanteId,
                        principalSchema: "gestion",
                        principalTable: "PlazasVacantes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SeguimientosPostulantes_Postulantes_PostulanteId",
                        column: x => x.PostulanteId,
                        principalSchema: "gestion",
                        principalTable: "Postulantes",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Postulantes_PlazaVacanteId",
                schema: "gestion",
                table: "Postulantes",
                column: "PlazaVacanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientosPostulantes_PlazaVacanteId",
                schema: "gestion",
                table: "SeguimientosPostulantes",
                column: "PlazaVacanteId");

            migrationBuilder.CreateIndex(
                name: "IX_SeguimientosPostulantes_PostulanteId",
                schema: "gestion",
                table: "SeguimientosPostulantes",
                column: "PostulanteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SeguimientosPostulantes",
                schema: "gestion");

            migrationBuilder.DropTable(
                name: "Usuarios",
                schema: "auth");

            migrationBuilder.DropTable(
                name: "Postulantes",
                schema: "gestion");

            migrationBuilder.DropTable(
                name: "PlazasVacantes",
                schema: "gestion");
        }
    }
}
