using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YourCompany.EFCore.PostgreSQL.CustomConfigurationMigrations
{
    /// <inheritdoc />
    public partial class ItsAlive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "eodehsotric_Entity",
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
                    table.PrimaryKey("eodehsotric_PK_Entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eodehsotric_ReusableEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WithCrossCuttingFeatureData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("eodehsotric_PK_ReusableEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eodehsotric_EntityMixin",
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
                    table.PrimaryKey("eodehsotric_PK_EntityMixin", x => x.EntityId);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityMixin_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "eodehsotric_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eodehsotric_EntityOwnedCollection",
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
                    table.PrimaryKey("eodehsotric_PK_EntityOwnedCollection", x => x.Id);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityOwnedCollection_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "eodehsotric_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "eodehsotric_EntityLinkToReusableEntity",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    ReusableEntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("eodehsotric_PK_EntityLinkToReusableEntity", x => x.EntityId);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityLinkToReusableEntity_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "eodehsotric_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityLinkToReusableEntity_ReusableEntiCB4546B",
                        column: x => x.ReusableEntityId,
                        principalTable: "eodehsotric_ReusableEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "eodehsotric_IX_EntityLinkToReusableEntity_ReusableEntityId",
                table: "eodehsotric_EntityLinkToReusableEntity",
                column: "ReusableEntityId");

            migrationBuilder.CreateIndex(
                name: "eodehsotric_IX_EntityOwnedCollection_EntityId",
                table: "eodehsotric_EntityOwnedCollection",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "eodehsotric_EntityLinkToReusableEntity");

            migrationBuilder.DropTable(
                name: "eodehsotric_EntityMixin");

            migrationBuilder.DropTable(
                name: "eodehsotric_EntityOwnedCollection");

            migrationBuilder.DropTable(
                name: "eodehsotric_ReusableEntity");

            migrationBuilder.DropTable(
                name: "eodehsotric_Entity");
        }
    }
}
