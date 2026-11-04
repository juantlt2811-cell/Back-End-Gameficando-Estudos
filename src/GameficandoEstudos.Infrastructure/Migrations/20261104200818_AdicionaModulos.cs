using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameficandoEstudos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaModulos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdModulo",
                table: "SessoesPratica",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<int>>(
                name: "IdsAtividades",
                table: "SessoesPratica",
                type: "integer[]",
                nullable: false,
                defaultValueSql: "'{}'");

            migrationBuilder.AddColumn<int>(
                name: "IdModulo",
                table: "FonteChunks",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdModulo",
                table: "Atividades",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Modulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IdMateria = table.Column<int>(type: "integer", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Modulos_Materias_IdMateria",
                        column: x => x.IdMateria,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessoesPratica_IdModulo",
                table: "SessoesPratica",
                column: "IdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_FonteChunks_IdModulo",
                table: "FonteChunks",
                column: "IdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_IdModulo",
                table: "Atividades",
                column: "IdModulo");

            migrationBuilder.CreateIndex(
                name: "IX_Modulos_IdMateria_Nome",
                table: "Modulos",
                columns: new[] { "IdMateria", "Nome" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Atividades_Modulos_IdModulo",
                table: "Atividades",
                column: "IdModulo",
                principalTable: "Modulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FonteChunks_Modulos_IdModulo",
                table: "FonteChunks",
                column: "IdModulo",
                principalTable: "Modulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SessoesPratica_Modulos_IdModulo",
                table: "SessoesPratica",
                column: "IdModulo",
                principalTable: "Modulos",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atividades_Modulos_IdModulo",
                table: "Atividades");

            migrationBuilder.DropForeignKey(
                name: "FK_FonteChunks_Modulos_IdModulo",
                table: "FonteChunks");

            migrationBuilder.DropForeignKey(
                name: "FK_SessoesPratica_Modulos_IdModulo",
                table: "SessoesPratica");

            migrationBuilder.DropTable(
                name: "Modulos");

            migrationBuilder.DropIndex(
                name: "IX_SessoesPratica_IdModulo",
                table: "SessoesPratica");

            migrationBuilder.DropIndex(
                name: "IX_FonteChunks_IdModulo",
                table: "FonteChunks");

            migrationBuilder.DropIndex(
                name: "IX_Atividades_IdModulo",
                table: "Atividades");

            migrationBuilder.DropColumn(
                name: "IdModulo",
                table: "SessoesPratica");

            migrationBuilder.DropColumn(
                name: "IdsAtividades",
                table: "SessoesPratica");

            migrationBuilder.DropColumn(
                name: "IdModulo",
                table: "FonteChunks");

            migrationBuilder.DropColumn(
                name: "IdModulo",
                table: "Atividades");
        }
    }
}
