using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SerEU.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServicosDigitais : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Icone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServicosDigitais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Pais = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Licenca = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    LogotipoUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataSubmissao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Aprovado = table.Column<bool>(type: "INTEGER", nullable: false),
                    CategoriaId = table.Column<int>(type: "INTEGER", nullable: false),
                    UtilizadorId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicosDigitais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServicosDigitais_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServicosDigitais_Categorias_CategoriaId",
                        column: x => x.CategoriaId,
                        principalTable: "Categorias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Avaliacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ServicoDigitalId = table.Column<int>(type: "INTEGER", nullable: false),
                    UtilizadorId = table.Column<string>(type: "TEXT", nullable: true),
                    Nota = table.Column<int>(type: "INTEGER", nullable: false),
                    Comentario = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Data = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Avaliacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Avaliacoes_AspNetUsers_UtilizadorId",
                        column: x => x.UtilizadorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Avaliacoes_ServicosDigitais_ServicoDigitalId",
                        column: x => x.ServicoDigitalId,
                        principalTable: "ServicosDigitais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServicoTag",
                columns: table => new
                {
                    ServicosId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagsId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicoTag", x => new { x.ServicosId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_ServicoTag_ServicosDigitais_ServicosId",
                        column: x => x.ServicosId,
                        principalTable: "ServicosDigitais",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicoTag_Tags_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_ServicoDigitalId",
                table: "Avaliacoes",
                column: "ServicoDigitalId");

            migrationBuilder.CreateIndex(
                name: "IX_Avaliacoes_UtilizadorId",
                table: "Avaliacoes",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosDigitais_CategoriaId",
                table: "ServicosDigitais",
                column: "CategoriaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosDigitais_UtilizadorId",
                table: "ServicosDigitais",
                column: "UtilizadorId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicoTag_TagsId",
                table: "ServicoTag",
                column: "TagsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Avaliacoes");

            migrationBuilder.DropTable(
                name: "ServicoTag");

            migrationBuilder.DropTable(
                name: "ServicosDigitais");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Categorias");
        }
    }
}
