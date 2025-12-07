using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPlazasVacantes.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePasswordHashAndReportesBugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Password",
                schema: "auth",
                table: "Usuarios");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "auth",
                table: "Usuarios",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ReportesBugs",
                schema: "gestion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Severidad = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaginaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NotasResolucion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaResolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResueltoPorId = table.Column<int>(type: "int", nullable: true)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReportesBugs",
                schema: "gestion");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "auth",
                table: "Usuarios");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                schema: "auth",
                table: "Usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
