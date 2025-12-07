using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionPlazasVacantes.Migrations
{
    /// <inheritdoc />
    public partial class AddAsignacionPlazas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Rol",
                schema: "auth",
                table: "Usuarios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioAsignadoId",
                schema: "gestion",
                table: "PlazasVacantes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlazasVacantes_UsuarioAsignadoId",
                schema: "gestion",
                table: "PlazasVacantes",
                column: "UsuarioAsignadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlazasVacantes_Usuarios_UsuarioAsignadoId",
                schema: "gestion",
                table: "PlazasVacantes",
                column: "UsuarioAsignadoId",
                principalSchema: "auth",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlazasVacantes_Usuarios_UsuarioAsignadoId",
                schema: "gestion",
                table: "PlazasVacantes");

            migrationBuilder.DropIndex(
                name: "IX_PlazasVacantes_UsuarioAsignadoId",
                schema: "gestion",
                table: "PlazasVacantes");

            migrationBuilder.DropColumn(
                name: "Rol",
                schema: "auth",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioAsignadoId",
                schema: "gestion",
                table: "PlazasVacantes");
        }
    }
}
