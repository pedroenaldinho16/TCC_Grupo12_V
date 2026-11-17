using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Campanhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Periodo = table.Column<string>(type: "TEXT", nullable: false),
                    Participantes = table.Column<int>(type: "INTEGER", nullable: false),
                    Meta = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    ImagemUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campanhas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Denuncias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", nullable: false),
                    DataOcorrencia = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    FotoPath = table.Column<string>(type: "TEXT", nullable: true),
                    NomeContato = table.Column<string>(type: "TEXT", nullable: true),
                    EmailContato = table.Column<string>(type: "TEXT", nullable: true),
                    Anonima = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Denuncias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Especies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NomeCientifico = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Categoria = table.Column<string>(type: "TEXT", nullable: false),
                    StatusConservacao = table.Column<string>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: false),
                    Habitat = table.Column<string>(type: "TEXT", nullable: false),
                    Alimentacao = table.Column<string>(type: "TEXT", nullable: false),
                    Ameacas = table.Column<string>(type: "TEXT", nullable: false),
                    Caracteristicas = table.Column<string>(type: "TEXT", nullable: false),
                    Comportamento = table.Column<string>(type: "TEXT", nullable: false),
                    StatusDetalhe = table.Column<string>(type: "TEXT", nullable: false),
                    ImagemUrl = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Especies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quizzes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", nullable: false),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DuracaoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quizzes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Senha = table.Column<string>(type: "TEXT", nullable: false),
                    IsAdmin = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuizPerguntas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuizId = table.Column<int>(type: "INTEGER", nullable: false),
                    Enunciado = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoA = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoB = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoC = table.Column<string>(type: "TEXT", nullable: false),
                    OpcaoD = table.Column<string>(type: "TEXT", nullable: false),
                    RespostaCorreta = table.Column<int>(type: "INTEGER", nullable: false),
                    EspecieId = table.Column<int>(type: "INTEGER", nullable: true),
                    Ordem = table.Column<int>(type: "INTEGER", nullable: false)
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
                name: "Avistamentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EspecieId = table.Column<int>(type: "INTEGER", nullable: true),
                    NomeAnimal = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FotoPath = table.Column<string>(type: "TEXT", nullable: true),
                    Latitude = table.Column<double>(type: "REAL", nullable: false),
                    Longitude = table.Column<double>(type: "REAL", nullable: false),
                    DataAvistamento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Quantidade = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    NomeObservador = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avistamentos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avistamentos_Especies_EspecieId",
                        column: x => x.EspecieId,
                        principalTable: "Especies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Avistamentos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ForumTopicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    Conteudo = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Hashtags = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumTopicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumTopicos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
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
                    IniciadoEm = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FinalizadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Finalizada = table.Column<bool>(type: "INTEGER", nullable: false),
                    TempoEsgotado = table.Column<bool>(type: "INTEGER", nullable: false),
                    Pontuacao = table.Column<int>(type: "INTEGER", nullable: false),
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
                name: "ForumCurtidasTopico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ForumTopicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumCurtidasTopico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumCurtidasTopico_ForumTopicos_ForumTopicoId",
                        column: x => x.ForumTopicoId,
                        principalTable: "ForumTopicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForumCurtidasTopico_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ForumRespostas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ForumTopicoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Conteudo = table.Column<string>(type: "TEXT", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumRespostas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumRespostas_ForumTopicos_ForumTopicoId",
                        column: x => x.ForumTopicoId,
                        principalTable: "ForumTopicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForumRespostas_Usuarios_UsuarioId",
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
                    QuizTentativaId = table.Column<int>(type: "INTEGER", nullable: false),
                    QuizPerguntaId = table.Column<int>(type: "INTEGER", nullable: false),
                    OpcaoSelecionada = table.Column<int>(type: "INTEGER", nullable: true),
                    Correta = table.Column<bool>(type: "INTEGER", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "ForumCurtidasResposta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ForumRespostaId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumCurtidasResposta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumCurtidasResposta_ForumRespostas_ForumRespostaId",
                        column: x => x.ForumRespostaId,
                        principalTable: "ForumRespostas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ForumCurtidasResposta_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avistamentos_EspecieId",
                table: "Avistamentos",
                column: "EspecieId");

            migrationBuilder.CreateIndex(
                name: "IX_Avistamentos_UsuarioId",
                table: "Avistamentos",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumCurtidasResposta_ForumRespostaId_UsuarioId",
                table: "ForumCurtidasResposta",
                columns: new[] { "ForumRespostaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumCurtidasResposta_UsuarioId",
                table: "ForumCurtidasResposta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumCurtidasTopico_ForumTopicoId_UsuarioId",
                table: "ForumCurtidasTopico",
                columns: new[] { "ForumTopicoId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ForumCurtidasTopico_UsuarioId",
                table: "ForumCurtidasTopico",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumRespostas_ForumTopicoId",
                table: "ForumRespostas",
                column: "ForumTopicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumRespostas_UsuarioId",
                table: "ForumRespostas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumTopicos_UsuarioId",
                table: "ForumTopicos",
                column: "UsuarioId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avistamentos");

            migrationBuilder.DropTable(
                name: "Campanhas");

            migrationBuilder.DropTable(
                name: "Denuncias");

            migrationBuilder.DropTable(
                name: "ForumCurtidasResposta");

            migrationBuilder.DropTable(
                name: "ForumCurtidasTopico");

            migrationBuilder.DropTable(
                name: "QuizRespostas");

            migrationBuilder.DropTable(
                name: "ForumRespostas");

            migrationBuilder.DropTable(
                name: "QuizPerguntas");

            migrationBuilder.DropTable(
                name: "QuizTentativas");

            migrationBuilder.DropTable(
                name: "ForumTopicos");

            migrationBuilder.DropTable(
                name: "Especies");

            migrationBuilder.DropTable(
                name: "Quizzes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
