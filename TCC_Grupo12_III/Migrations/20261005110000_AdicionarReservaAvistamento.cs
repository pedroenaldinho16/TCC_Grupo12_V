using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarReservaAvistamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReservasAvistamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    EspecieId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuantidadeDisponivel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasAvistamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservasAvistamento_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReservasAvistamento_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservasAvistamento_EspecieId",
                table: "ReservasAvistamento",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasAvistamento_UsuarioId_EspecieId",
                table: "ReservasAvistamento",
                columns: new[] { "UsuarioId", "EspecieId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservasAvistamento");
        }
    }
}
