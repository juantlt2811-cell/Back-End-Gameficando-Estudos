using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace GameficandoEstudos.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AjusteVetorGemini : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embeddings",
                table: "FonteChunks",
                type: "vector(768)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(1024)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Vector>(
                name: "Embeddings",
                table: "FonteChunks",
                type: "vector(1024)",
                nullable: false,
                oldClrType: typeof(Vector),
                oldType: "vector(768)");
        }
    }
}
