using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarComunidadesForumECampanhas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ComunidadeId",
                table: "ForumTopicos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ComunidadeId",
                table: "Campanhas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ForumComunidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Regras = table.Column<string>(type: "TEXT", maxLength: 600, nullable: true),
                    IconePath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    BannerPath = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    CriadorId = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusAprovacao = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false),
                    AprovadoEm = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AprovadoPorId = table.Column<int>(type: "INTEGER", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsGeral = table.Column<bool>(type: "INTEGER", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ForumComunidades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ForumComunidades_Usuarios_CriadorId",
                        column: x => x.CriadorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ForumTopicos_ComunidadeId",
                table: "ForumTopicos",
                column: "ComunidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Campanhas_ComunidadeId",
                table: "Campanhas",
                column: "ComunidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumComunidades_CriadorId",
                table: "ForumComunidades",
                column: "CriadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ForumComunidades_Slug",
                table: "ForumComunidades",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Campanhas_ForumComunidades_ComunidadeId",
                table: "Campanhas",
                column: "ComunidadeId",
                principalTable: "ForumComunidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ForumTopicos_ForumComunidades_ComunidadeId",
                table: "ForumTopicos",
                column: "ComunidadeId",
                principalTable: "ForumComunidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campanhas_ForumComunidades_ComunidadeId",
                table: "Campanhas");

            migrationBuilder.DropForeignKey(
                name: "FK_ForumTopicos_ForumComunidades_ComunidadeId",
                table: "ForumTopicos");

            migrationBuilder.DropTable(
                name: "ForumComunidades");

            migrationBuilder.DropIndex(
                name: "IX_ForumTopicos_ComunidadeId",
                table: "ForumTopicos");

            migrationBuilder.DropIndex(
                name: "IX_Campanhas_ComunidadeId",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "ComunidadeId",
                table: "ForumTopicos");

            migrationBuilder.DropColumn(
                name: "ComunidadeId",
                table: "Campanhas");
        }
    }
}
