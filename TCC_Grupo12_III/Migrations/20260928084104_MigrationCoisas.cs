using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class MigrationCoisas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Xp",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EspecieQuizPerguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EspecieId = table.Column<int>(type: "INTEGER", nullable: false),
                    Enunciado = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoA = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoB = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoC = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoD = table.Column<string>(type: "TEXT", nullable: false),
                    RespostaCorreta = table.Column<int>(type: "INTEGER", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EspecieQuizPerguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EspecieQuizPerguntas_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PokedexRegistros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    EspecieId = table.Column<int>(type: "INTEGER", nullable: false),
                    VistaEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Dominada = table.Column<bool>(type: "INTEGER", nullable: false),
                    DominadaEm = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PokedexRegistros", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PokedexRegistros_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PokedexRegistros_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EspecieQuizPerguntas_EspecieId",
                table: "EspecieQuizPerguntas",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_PokedexRegistros_EspecieId",
                table: "PokedexRegistros",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_PokedexRegistros_UsuarioId_EspecieId",
                table: "PokedexRegistros",
                columns: new[] { "UsuarioId", "EspecieId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EspecieQuizPerguntas");

            migrationBuilder.DropTable(
                name: "PokedexRegistros");

            migrationBuilder.DropColumn(
                name: "Xp",
                table: "Usuarios");
        }
    }
}
