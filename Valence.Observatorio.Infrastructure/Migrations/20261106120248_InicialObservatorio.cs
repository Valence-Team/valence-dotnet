using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Valence.Observatorio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InicialObservatorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TB_OBS_BOLETIM",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Municipio = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    Titulo = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    Texto = table.Column<string>(type: "NCLOB", maxLength: 4000, nullable: false),
                    GestorId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    IdempotencyKey = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    CriadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_OBS_BOLETIM", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_OBS_EVENTO_PROCESSADO",
                columns: table => new
                {
                    EventoId = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    Tipo = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    ProcessadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_OBS_EVENTO_PROCESSADO", x => x.EventoId);
                });

            migrationBuilder.CreateTable(
                name: "TB_OBS_FOCO_ANONIMIZADO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    OcorrenciaId = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    Cultura = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    PragaProvavel = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: true),
                    Severidade = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Municipio = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Uf = table.Column<string>(type: "NVARCHAR2(2)", maxLength: 2, nullable: false),
                    Status = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false),
                    DataOcorrencia = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    AtualizadoEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_OBS_FOCO_ANONIMIZADO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TB_OBS_REQUISICAO_IDEMPOTENTE",
                columns: table => new
                {
                    Chave = table.Column<Guid>(type: "RAW(16)", nullable: false),
                    HashCorpo = table.Column<string>(type: "NVARCHAR2(64)", maxLength: 64, nullable: false),
                    StatusCode = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CorpoResposta = table.Column<string>(type: "NVARCHAR2(2000)", nullable: false),
                    CriadaEm = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_OBS_REQUISICAO_IDEMPOTENTE", x => x.Chave);
                });

            migrationBuilder.CreateTable(
                name: "TB_OBS_USUARIO_GESTOR",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Email = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    SenhaHash = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    Papel = table.Column<string>(type: "NVARCHAR2(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TB_OBS_USUARIO_GESTOR", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TB_OBS_USUARIO_GESTOR",
                columns: new[] { "Id", "Email", "Papel", "SenhaHash" },
                values: new object[,]
                {
                    { 1, "gestor1@valence.local", "GESTOR", "$2a$11$K.r20tSB2PUqshozizTX3uCvcteJhr1Lut3CbliyDZYGhtoE842ba" },
                    { 2, "gestor2@valence.local", "GESTOR", "$2a$11$qxwNpdY08uO/ha4OLD8J8.42cg.4V/.9u7WHP98qlnk27Ww0IsAb." },
                    { 3, "agronomo@valence.local", "AGRONOMO", "$2a$11$eXJuSbsmKu9hyku7F5CVp.1ZUK7dnb4DQMBqxEk/y19YPdt9plCTS" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_TB_OBS_BOLETIM_IdempotencyKey",
                table: "TB_OBS_BOLETIM",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_OBS_BOLETIM_Uf_Municipio",
                table: "TB_OBS_BOLETIM",
                columns: new[] { "Uf", "Municipio" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_OBS_FOCO_ANONIMIZADO_OcorrenciaId",
                table: "TB_OBS_FOCO_ANONIMIZADO",
                column: "OcorrenciaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TB_OBS_FOCO_ANONIMIZADO_Uf_Municipio",
                table: "TB_OBS_FOCO_ANONIMIZADO",
                columns: new[] { "Uf", "Municipio" });

            migrationBuilder.CreateIndex(
                name: "IX_TB_OBS_USUARIO_GESTOR_Email",
                table: "TB_OBS_USUARIO_GESTOR",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TB_OBS_BOLETIM");

            migrationBuilder.DropTable(
                name: "TB_OBS_EVENTO_PROCESSADO");

            migrationBuilder.DropTable(
                name: "TB_OBS_FOCO_ANONIMIZADO");

            migrationBuilder.DropTable(
                name: "TB_OBS_REQUISICAO_IDEMPOTENTE");

            migrationBuilder.DropTable(
                name: "TB_OBS_USUARIO_GESTOR");
        }
    }
}
