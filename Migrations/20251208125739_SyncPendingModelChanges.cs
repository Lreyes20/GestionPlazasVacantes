using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPlazasVacantes.Migrations
{
    /// <inheritdoc />
    public partial class SyncPendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportesBugs",
                schema: "gestion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportesBugs",
                schema: "gestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ResueltoPorId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NotasResolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PaginaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Severidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesBugs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReportesBugs_Usuarios_ResueltoPorId",
                        column: x => x.ResueltoPorId,
                        principalSchema: "auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReportesBugs_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "auth",
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReportesBugs_ResueltoPorId",
                schema: "gestion",
                table: "ReportesBugs",
                column: "ResueltoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportesBugs_UsuarioId",
                schema: "gestion",
                table: "ReportesBugs",
                column: "UsuarioId");
        }
    }
}
