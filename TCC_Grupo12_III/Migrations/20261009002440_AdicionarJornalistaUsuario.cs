using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarJornalistaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsJornalista",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegistroJornalista",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsJornalista",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "RegistroJornalista",
                table: "Usuarios");
        }
    }
}
