using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YourCompany.EFCore.PostgreSQL.YourCompanyDbContextConfigurationMigrations
{
    /// <inheritdoc />
    public partial class ItsAlive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ex_configu_e_Entity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<string>(type: "text", nullable: true),
                    Column4 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_configu_e_PK_Entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ex_configu_e_ReusableEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WithCrossCuttingFeatureData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_configu_e_PK_ReusableEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ex_configu_e_EntityMixin",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Column1 = table.Column<string>(type: "text", nullable: true),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<string>(type: "text", nullable: true),
                    Column4 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_configu_e_PK_EntityMixin", x => x.EntityId);
                    table.ForeignKey(
                        name: "ex_configu_e_FK_EntityMixin_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "ex_configu_e_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ex_configu_e_EntityOwnedCollection",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Column1 = table.Column<string>(type: "text", nullable: true),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<string>(type: "text", nullable: true),
                    Column4 = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_configu_e_PK_EntityOwnedCollection", x => x.Id);
                    table.ForeignKey(
                        name: "ex_configu_e_FK_EntityOwnedCollection_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "ex_configu_e_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ex_configu_e_EntityLinkToReusableEntity",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    ReusableEntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("ex_configu_e_PK_EntityLinkToReusableEntity", x => x.EntityId);
                    table.ForeignKey(
                        name: "ex_configu_e_FK_EntityLinkToReusableEntity_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "ex_configu_e_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "ex_configu_e_FK_EntityLinkToReusableEntity_ReusableEntiCB4546B",
                        column: x => x.ReusableEntityId,
                        principalTable: "ex_configu_e_ReusableEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ex_configu_e_IX_EntityLinkToReusableEntity_ReusableEntityId",
                table: "ex_configu_e_EntityLinkToReusableEntity",
                column: "ReusableEntityId");

            migrationBuilder.CreateIndex(
                name: "ex_configu_e_IX_EntityOwnedCollection_EntityId",
                table: "ex_configu_e_EntityOwnedCollection",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ex_configu_e_EntityLinkToReusableEntity");

            migrationBuilder.DropTable(
                name: "ex_configu_e_EntityMixin");

            migrationBuilder.DropTable(
                name: "ex_configu_e_EntityOwnedCollection");

            migrationBuilder.DropTable(
                name: "ex_configu_e_ReusableEntity");

            migrationBuilder.DropTable(
                name: "ex_configu_e_Entity");
        }
    }
}
