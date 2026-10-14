using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameficandoEstudos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaSessaoPraticaETentativaResposta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atividades_Materias_IdMateria",
                table: "Atividades");

            migrationBuilder.CreateTable(
                name: "SessoesPratica",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdMateria = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TotalQuestoes = table.Column<int>(type: "integer", nullable: false),
                    TotalAcertos = table.Column<int>(type: "integer", nullable: false),
                    IniciadaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConcluidaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessoesPratica", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessoesPratica_Materias_IdMateria",
                        column: x => x.IdMateria,
                        principalTable: "Materias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessoesPratica_Usuarios_IdUsuario",
                        column: x => x.IdUsuario,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TentativasResposta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IdSessaoPratica = table.Column<int>(type: "integer", nullable: false),
                    IdAtividade = table.Column<int>(type: "integer", nullable: false),
                    IdUsuario = table.Column<int>(type: "integer", nullable: false),
                    IdMateria = table.Column<int>(type: "integer", nullable: false),
                    RespostaAluno = table.Column<string>(type: "text", nullable: false),
                    Acertou = table.Column<bool>(type: "boolean", nullable: false),
                    Feedback = table.Column<string>(type: "text", nullable: false),
                    RespondidoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TentativasResposta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TentativasResposta_Atividades_IdAtividade",
                        column: x => x.IdAtividade,
                        principalTable: "Atividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TentativasResposta_SessoesPratica_IdSessaoPratica",
                        column: x => x.IdSessaoPratica,
                        principalTable: "SessoesPratica",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SessoesPratica_IdMateria",
                table: "SessoesPratica",
                column: "IdMateria");

            migrationBuilder.CreateIndex(
                name: "IX_SessoesPratica_IdUsuario",
                table: "SessoesPratica",
                column: "IdUsuario");

            migrationBuilder.CreateIndex(
                name: "IX_TentativasResposta_IdAtividade",
                table: "TentativasResposta",
                column: "IdAtividade");

            migrationBuilder.CreateIndex(
                name: "IX_TentativasResposta_IdSessaoPratica",
                table: "TentativasResposta",
                column: "IdSessaoPratica");

            migrationBuilder.AddForeignKey(
                name: "FK_Atividades_Materias_IdMateria",
                table: "Atividades",
                column: "IdMateria",
                principalTable: "Materias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Atividades_Materias_IdMateria",
                table: "Atividades");

            migrationBuilder.DropTable(
                name: "TentativasResposta");

            migrationBuilder.DropTable(
                name: "SessoesPratica");

            migrationBuilder.AddForeignKey(
                name: "FK_Atividades_Materias_IdMateria",
                table: "Atividades",
                column: "IdMateria",
                principalTable: "Materias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
