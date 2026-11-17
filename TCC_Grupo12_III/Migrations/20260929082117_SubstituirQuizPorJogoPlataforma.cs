using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class SubstituirQuizPorJogoPlataforma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizRespostas");

            migrationBuilder.DropTable(
                name: "QuizPerguntas");

            migrationBuilder.DropTable(
                name: "QuizTentativas");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.CreateTable(
                name: "NivelJogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    DadosJson = table.Column<string>(type: "TEXT", nullable: false),
                    TotalAnimais = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NivelJogos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JogoTentativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NivelJogoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IniciadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ConcluidoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Concluida = table.Column<bool>(type: "INTEGER", nullable: false),
                    AnimaisResgatados = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalAnimais = table.Column<int>(type: "INTEGER", nullable: false),
                    TempoSegundos = table.Column<int>(type: "INTEGER", nullable: false),
                    VidasRestantes = table.Column<int>(type: "INTEGER", nullable: false),
                    AnimaisResgatadosIds = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JogoTentativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JogoTentativas_NivelJogos_NivelJogoId",
                        column: x => x.NivelJogoId,
                        principalTable: "NivelJogos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JogoTentativas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JogoTentativas_NivelJogoId",
                table: "JogoTentativas",
                column: "NivelJogoId");

            migrationBuilder.CreateIndex(
                name: "IX_JogoTentativas_UsuarioId_NivelJogoId_Dia",
                table: "JogoTentativas",
                columns: new[] { "UsuarioId", "NivelJogoId", "Dia" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JogoTentativas");

            migrationBuilder.DropTable(
                name: "NivelJogos");

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizPerguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EspecieId = table.Column<int>(type: "INTEGER", nullable: true),
                    QuizId = table.Column<int>(type: "INTEGER", nullable: false),
                    Enunciado = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoA = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoB = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoC = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoD = table.Column<string>(type: "TEXT", nullable: false),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false),
                    RespostaCorreta = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizPerguntas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizPerguntas_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_QuizPerguntas_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizTentativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuizId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Dia = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Finalizada = table.Column<bool>(type: "INTEGER", nullable: false),
                    FinalizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IniciadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Pontuacao = table.Column<int>(type: "INTEGER", nullable: false),
                    TempoEsgotado = table.Column<bool>(type: "INTEGER", nullable: false),
                    TotalPerguntas = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizTentativas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizTentativas_Quizzes_QuizId",
                        column: x => x.QuizId,
                        principalTable: "Quizzes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizTentativas_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuizRespostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuizPerguntaId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuizTentativaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Correta = table.Column<bool>(type: "INTEGER", nullable: false),
                    OpcaoSelecionada = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizRespostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizRespostas_QuizPerguntas_QuizPerguntaId",
                        column: x => x.QuizPerguntaId,
                        principalTable: "QuizPerguntas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizRespostas_QuizTentativas_QuizTentativaId",
                        column: x => x.QuizTentativaId,
                        principalTable: "QuizTentativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizPerguntas_EspecieId",
                table: "QuizPerguntas",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizPerguntas_QuizId",
                table: "QuizPerguntas",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizRespostas_QuizPerguntaId",
                table: "QuizRespostas",
                column: "QuizPerguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizRespostas_QuizTentativaId",
                table: "QuizRespostas",
                column: "QuizTentativaId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizTentativas_QuizId",
                table: "QuizTentativas",
                column: "QuizId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizTentativas_UsuarioId_QuizId_Dia",
                table: "QuizTentativas",
                columns: new[] { "UsuarioId", "QuizId", "Dia" },
                unique: true);
        }
    }
}
