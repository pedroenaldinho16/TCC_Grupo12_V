using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class ExpandirCampanhasDetalhesAprovacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AprovadoPorNome",
                table: "Campanhas",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cronograma",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Campanhas",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DataFim",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataInicio",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataResposta",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Horario",
                table: "Campanhas",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImpactoEsperado",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocalEncontro",
                table: "Campanhas",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoReprovacao",
                table: "Campanhas",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OQueLevar",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjetivoPrincipal",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObjetivosEspecificos",
                table: "Campanhas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Organizador",
                table: "Campanhas",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicoAlvo",
                table: "Campanhas",
                type: "TEXT",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusAprovacao",
                table: "Campanhas",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Campanhas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Campanhas_UsuarioId",
                table: "Campanhas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Campanhas_Usuarios_UsuarioId",
                table: "Campanhas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Campanhas_Usuarios_UsuarioId",
                table: "Campanhas");

            migrationBuilder.DropIndex(
                name: "IX_Campanhas_UsuarioId",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "AprovadoPorNome",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "Cronograma",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "DataFim",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "DataInicio",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "DataResposta",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "Horario",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "ImpactoEsperado",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "LocalEncontro",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "MotivoReprovacao",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "OQueLevar",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "ObjetivoPrincipal",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "ObjetivosEspecificos",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "Organizador",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "PublicoAlvo",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "StatusAprovacao",
                table: "Campanhas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Campanhas");
        }
    }
}
