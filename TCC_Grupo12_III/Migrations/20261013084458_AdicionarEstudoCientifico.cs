using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarEstudoCientifico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstudosCientificos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Subtitulo = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Resumo = table.Column<string>(type: "TEXT", nullable: false),
                    ConteudoHtml = table.Column<string>(type: "TEXT", nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    EspecieNome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    EspecieId = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusConservacao = table.Column<string>(type: "TEXT", maxLength: 60, nullable: true),
                    Autor = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Instituicao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ImagemUrl = table.Column<string>(type: "TEXT", nullable: true),
                    TempoLeituraMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    DestaquesTexto = table.Column<string>(type: "TEXT", nullable: true),
                    AmeacasTexto = table.Column<string>(type: "TEXT", nullable: true),
                    AcoesTexto = table.Column<string>(type: "TEXT", nullable: true),
                    ReferenciasTexto = table.Column<string>(type: "TEXT", nullable: true),
                    StatusAprovacao = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    DataSubmissao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataResposta = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoReprovacao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ObservacoesAdmin = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstudosCientificos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EstudosCientificos_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_EstudosCientificos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EstudosCientificos_EspecieId",
                table: "EstudosCientificos",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_EstudosCientificos_UsuarioId",
                table: "EstudosCientificos",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EstudosCientificos");
        }
    }
}
