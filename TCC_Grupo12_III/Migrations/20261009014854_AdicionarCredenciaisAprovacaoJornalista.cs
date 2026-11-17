using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCredenciaisAprovacaoJornalista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataRespostaJornalista",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataSolicitacaoJornalista",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JustificativaJornalista",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkPortfolioJornalista",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoReprovacaoJornalista",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusJornalista",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VeiculoJornalista",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataRespostaJornalista",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataSolicitacaoJornalista",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "JustificativaJornalista",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "LinkPortfolioJornalista",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoReprovacaoJornalista",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "StatusJornalista",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "VeiculoJornalista",
                table: "Usuarios");
        }
    }
}
