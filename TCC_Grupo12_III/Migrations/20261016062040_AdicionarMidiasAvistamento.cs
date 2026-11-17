using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarMidiasAvistamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AvistamentoMidias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AvistamentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Caminho = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AvistamentoMidias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AvistamentoMidias_Avistamentos_AvistamentoId",
                        column: x => x.AvistamentoId,
                        principalTable: "Avistamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AvistamentoMidias_AvistamentoId",
                table: "AvistamentoMidias",
                column: "AvistamentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AvistamentoMidias");
        }
    }
}
