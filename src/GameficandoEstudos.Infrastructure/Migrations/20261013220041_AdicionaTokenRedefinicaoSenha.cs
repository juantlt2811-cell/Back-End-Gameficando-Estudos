using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameficandoEstudos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaTokenRedefinicaoSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TokenRedefinicaoSenhaExpiraEm",
                table: "Usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenRedefinicaoSenhaHash",
                table: "Usuarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenRedefinicaoSenhaExpiraEm",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "TokenRedefinicaoSenhaHash",
                table: "Usuarios");
        }
    }
}
