using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarDenunciaResolvidaECategoriaForum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "ForumTopicos",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ObservacaoAdmin",
                table: "Denuncias",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Resolvida",
                table: "Denuncias",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "ForumTopicos");

            migrationBuilder.DropColumn(
                name: "ObservacaoAdmin",
                table: "Denuncias");

            migrationBuilder.DropColumn(
                name: "Resolvida",
                table: "Denuncias");
        }
    }
}
