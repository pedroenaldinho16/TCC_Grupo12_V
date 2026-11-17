using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TCC_Grupo12_III.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarModeradorSuspensaoEBanimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BanidoPorNome",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataBanimento",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataSuspensaoForum",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBanido",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsModerador",
                table: "Usuarios",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MotivoBanimento",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoSuspensaoForum",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspensoForumAte",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensoPorNome",
                table: "Usuarios",
                type: "TEXT",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BanidoPorNome",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataBanimento",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "DataSuspensaoForum",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IsBanido",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "IsModerador",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoBanimento",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoSuspensaoForum",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SuspensoForumAte",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "SuspensoPorNome",
                table: "Usuarios");
        }
    }
}
